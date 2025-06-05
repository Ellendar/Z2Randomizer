using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.RandomizerCore.Enemy;

public class OverworldEnemyShuffler
{
    public static void Shuffle(World world, ROM romData, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG)
    {
        // Dictionary of enemy address pointers of areas to randomize,
        // mapping to a set of address pointers to the linked sideview map
        // command data.  Since the same pointers are used for multiple
        // rooms sometimes, it seemed best to keep track of all of them
        // to make the enemies fit into every one of the linked sideviews.
        SortedDictionary<int, SortedSet<int>> dict = new();
        HashSet<int> encounterAddrs = new();
        for (int map = 0; map < 63; map++)
        {
            int i = world.enemyPtr + map * 2;
            int enemiesPtrOffset = romData.GetShort(i + 1, i) & 0x0FFF;
            int addr = world.enemyAddr + enemiesPtrOffset;

            int j = world.sideviewPtrTable + map * 2;
            int sideviewNesPtr = romData.GetShort(j + 1, j);
            int sideviewAddr = ROM.ConvertNesPtrToRomAddr(world.sideviewBank, sideviewNesPtr);
            if (!dict.TryAdd(addr, new([sideviewAddr])))
            {
                dict[addr].Add(sideviewAddr); // add to existing set
            }
        }
        // encounters have a second enemy list (small & large encounter)
        foreach (int map in world.overworldEncounterMaps)
        {
            int i = world.enemyPtr + map * 2;
            int enemiesPtrOffset = romData.GetShort(i + 1, i) & 0x0FFF;
            int addr = world.enemyAddr + enemiesPtrOffset;
            encounterAddrs.Add(addr);
            byte enemiesLength = romData.GetByte(addr);
            addr += enemiesLength; // go past first enemy list

            int j = world.sideviewPtrTable + map * 2;
            int sideviewNesPtr = romData.GetShort(j + 1, j);
            int sideviewAddr = ROM.ConvertNesPtrToRomAddr(world.sideviewBank, sideviewNesPtr);
            if (!dict.TryAdd(addr, new([sideviewAddr])))
            {
                dict[addr].Add(sideviewAddr); // add to existing set
            }
            encounterAddrs.Add(addr);
        }

        foreach (var (eAddr, svAddrs) in dict)
        {
            ShuffleEnemiesAtAddress(world, romData, svAddrs, eAddr, encounterAddrs.Contains(eAddr), mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
        }
    }

    public static void ShuffleEnemiesAtAddress(World world, ROM romData, SortedSet<int> sideviewAddr, int enemiesAddr, bool encounter, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG)
    {
        if (enemiesAddr == 0x4A88)
        {
            return; // skip incomplete death_mountain 0 (no enemies anyway)
        }
        if (enemiesAddr == 0x8AB0)
        {
            return; // skip incomplete maze_island 0 (no enemies anyway)
        }
        if (enemiesAddr == 0x95A4)
        {
            return; // skip east_hyrule 21 (does not look like real enemy data)
        }

        List<byte[]> sideviewBytes = [.. sideviewAddr.Select(a => romData.GetBytes(a, romData.GetByte(a)))];
        int enemiesLength = romData.GetByte(enemiesAddr);
        byte[] enemiesBytes = romData.GetBytes(enemiesAddr, enemiesLength);
        byte[] newEnemyBytes = RandomizeEnemiesInner(world, sideviewBytes, enemiesBytes, encounter, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
        Debug.Assert(newEnemyBytes.Length == enemiesLength);
        romData.Put(enemiesAddr, newEnemyBytes);
    }

    /// Recreate Enemy generic type and proceed
    ///
    /// This is just a step that is needed so we can create the
    /// Generic Enemy type. We are basically unpacking T from a property
    /// as best as is possible in C#.
    protected static byte[] RandomizeEnemiesInner(World world, List<byte[]> sideviewBytes, byte[] enemyBytes, bool encounter, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG)
    {
        switch (world.groupedEnemies)
        {
            case GroupedEnemies<EnemiesWest> gWest:
                var eeWest = new EnemiesEditable<EnemiesWest>(enemyBytes);
                return RandomizeEnemiesInner(sideviewBytes, gWest, eeWest, encounter, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
            case GroupedEnemies<EnemiesEast> gEast:
                var eeEast = new EnemiesEditable<EnemiesEast>(enemyBytes);
                return RandomizeEnemiesInner(sideviewBytes, gEast, eeEast, encounter, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
            default:
                throw new NotImplementedException();
        }
    }

    /// Determine the Sideview generic type and then use it to generate
    /// the logical solid grid for the room.
    /// 
    /// Then proceed to the main function.
    protected static byte[] RandomizeEnemiesInner<T>(List<byte[]> sideviewBytes, GroupedEnemies<T> groupedEnemies, EnemiesEditable<T> ee, bool encounter, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG) where T : Enum
    {
        Debug.Assert(sideviewBytes.Count > 0);
        bool[,]? solidGrid = null;
        foreach (var bytes in sideviewBytes)
        {
            byte objectSet = (byte)((bytes[1] & 0b10000000) >> 7);
            bool[,] svGrid = (objectSet == 0) ?
                new Sidescroll.SideviewEditable<Sidescroll.ForestObject>(bytes).CreateSolidGrid() :
                new Sidescroll.SideviewEditable<Sidescroll.CaveObject>(bytes).CreateSolidGrid();

            if (solidGrid == null)
            {
                solidGrid = svGrid;
            }
            else
            {
                solidGrid = Sidescroll.SolidGridHelper.GridUnion(solidGrid, svGrid);
            }
        }
        return RandomizeEnemiesInner(solidGrid!, groupedEnemies, ee, encounter, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
    }

    protected static byte[] RandomizeEnemiesInner<T>(bool[,] solidGrid, GroupedEnemies<T> groupedEnemies, EnemiesEditable<T> ee, bool encounter, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG) where T : Enum
    {
        void PositionGeldarm(Enemy<T> enemy)
        {
            // do our best to fit the Geldarm. if there is no space, prioritize aligning with the floor
            for (int j = 0; j < 5; j++)
            {
                var newY = Sidescroll.SolidGridHelper.FindFloor(solidGrid, enemy.X, enemy.Y, 1, 5 - j);
                if (newY != 0)
                {
                    enemy.Y = Math.Min(9, newY - j);
                    break;
                }
            }
        }
        bool PositionSmallEnemy(Enemy<T> enemy, T swapToId)
        {
            switch (swapToId)
            {
                case EnemiesWest.MEGMET:
                    if (enemy.Id is not EnemiesWest.MEGMET && enemy.Y > 0 && solidGrid[enemy.X, enemy.Y - 1] == false)
                    {
                        enemy.Y -= 1;
                    }
                    return true;
                case EnemiesEast.LEEVER:
                    // Vanilla Leever positions for reference:
                    // East Map 29 Small & Large encounter at y == 8
                    // East Map 30 Large encounter at y == 8
                    // East Map 33 at y == 9
                    var leeverFloor = Sidescroll.SolidGridHelper.FindFloor(solidGrid, enemy.X, enemy.Y, 1, 1);
                    leeverFloor--; // because Leevers have strange y positioning.
                    if (leeverFloor < 7)
                    {
                        return false; // Don't roll Leevers above ground level
                    }
                    enemy.Y = Math.Min(9, leeverFloor);
                    return true;
                default:
                    var defaultFloor = Sidescroll.SolidGridHelper.FindFloor(solidGrid, enemy.X, enemy.Y, 1, 1);
                    enemy.Y = Math.Min(9, defaultFloor);
                    return true;
            }
        }
        void PositionLargeEnemy(Enemy<T> enemy, T swapToId)
        {
            switch (swapToId)
            {
                case EnemiesWest.GELDARM:
                    PositionGeldarm(enemy);
                    break;
                default:
                    var defaultFloor = Sidescroll.SolidGridHelper.FindFloor(solidGrid, enemy.X, enemy.Y, 1, 2);
                    enemy.Y = Math.Min(9, defaultFloor);
                    break;
            }
        }
        T RollSmallEnemy(Enemy<T> enemy, T swapToId)
        {
            while (true)
            {
                if (PositionSmallEnemy(enemy, swapToId))
                {
                    break;
                }
                swapToId = groupedEnemies.SmallEnemies[RNG.Next(0, groupedEnemies.SmallEnemies.Length)];
            }
            return swapToId;
        }
        void MoveAwayFromLinkSpawnInEncounter(Enemy<T> enemy)
        {
            if (encounter)
            {
                // Move ground enemies in encounters away from Link's spawning position at x == 24
                // Geldarms are the only ones that appear very close in vanilla, so those are allowed.
                int minSpace = enemy.Id is EnemiesWest.GELDARM ? 1 : 4;
                var x = enemy.X;
                var maxLeft = 24 - minSpace - 1;
                var minRight = 24 + minSpace + 1;
                if (maxLeft < x && x <= 24)
                {
                    enemy.X = maxLeft;
                }
                else if (24 < x && x < minRight)
                {
                    enemy.X = minRight;
                }
            }
        }

        int? firstGenerator = null;
        for (int i = 0; i < ee.Enemies.Count; i++)
        {
            Enemy<T> enemy = ee.Enemies[i];

            if (mixLargeAndSmallEnemies)
            {
                if (enemy.IsShufflableSmallOrLarge())
                {
                    T swapToId = groupedEnemies.GroundEnemies[RNG.Next(0, groupedEnemies.GroundEnemies.Length)];
                    if (groupedEnemies.LargeEnemies.Contains(swapToId))
                    {
                        if (enemy.IsShufflableSmall())
                        {
                            enemy.Y -= 1;
                        }
                        PositionLargeEnemy(enemy, swapToId);
                    }
                    else
                    {
                        swapToId = RollSmallEnemy(enemy, swapToId);
                    }
                    enemy.Id = swapToId;
                    MoveAwayFromLinkSpawnInEncounter(enemy);
                    continue;
                }
            }
            else
            {
                if (enemy.IsShufflableLarge())
                {
                    T swapToId = groupedEnemies.LargeEnemies[RNG.Next(0, groupedEnemies.LargeEnemies.Length)];
                    PositionLargeEnemy(enemy, swapToId);
                    enemy.Id = swapToId;
                    MoveAwayFromLinkSpawnInEncounter(enemy);
                    continue;
                }
                else if (enemy.IsShufflableSmall())
                {
                    T swapToId = groupedEnemies.SmallEnemies[RNG.Next(0, groupedEnemies.SmallEnemies.Length)];
                    swapToId = RollSmallEnemy(enemy, swapToId);
                    enemy.Id = swapToId;
                    MoveAwayFromLinkSpawnInEncounter(enemy);
                    continue;
                }
            }

            if (enemy.IsShufflableFlying())
            {
                T swapToId = groupedEnemies.FlyingEnemies[RNG.Next(0, groupedEnemies.FlyingEnemies.Length)];
                switch (swapToId)
                {
                    case EnemiesWest.ACHE:
                    case EnemiesWest.ACHEMAN:
                    case EnemiesWest.BLUE_DEELER:
                    case EnemiesWest.RED_DEELER:
                    case EnemiesEast.ACHE:
                    case EnemiesEast.ACHEMAN:
                    case EnemiesEast.BLUE_DEELER:
                    case EnemiesEast.RED_DEELER:
                        enemy.Y = 0;
                        break;
                }
                enemy.Id = swapToId;
                continue;
            }

            if (enemy.IsShufflableGenerator())
            {
                T swapToId = groupedEnemies.Generators[RNG.Next(0, groupedEnemies.Generators.Length)];
                firstGenerator ??= (int)(object)swapToId;
                if (generatorsAlwaysMatch)
                {
                    enemy.Id = (T)(object)firstGenerator;
                }
                else
                {
                    enemy.Id = swapToId;
                }
                continue;
            }

            //Moblin generators can become things, but things can't become moblin generators.
            //Why? I assume it causes some kind of issue, but I've never investigated.
            if (enemy.Id is EnemiesWest.DUMB_MOBLIN_GENERATOR)
            {
                int swapIndex = RNG.Next(0, groupedEnemies.Generators.Length + 1);
                T swapToId = swapIndex == groupedEnemies.Generators.Length ? (T)(object)EnemiesWest.DUMB_MOBLIN_GENERATOR : groupedEnemies.Generators[RNG.Next(0, groupedEnemies.Generators.Length)];
                firstGenerator ??= (int)(object)swapToId;
                if (generatorsAlwaysMatch)
                {
                    enemy.Id = (T)(object)firstGenerator;
                }
                else
                {
                    enemy.Id = swapToId;
                }
                continue;
            }
        }

        return ee.Finalize();
    }
}
