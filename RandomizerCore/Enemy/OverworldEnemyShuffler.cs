using js65;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore.Enemy;

public class OverworldEnemyShuffler
{
    public static void Shuffle(List<World> worlds, Assembler asm, ROM romData, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG)
    {
        int tableRamBaseAddr = 0x7000;
        int tablePrgBaseAddr = 0x88a0;
        byte[] emptyTable = [0x01];

        for (int bank = 1; bank < 3; bank++)
        {
            var worldsInBank = worlds.Where(w => w.sideviewBank == bank);

            // In bank PRG1 and PRG2, the enemy tables are stored at $88A0
            // This is then copied to SRAM at $7000 when the world is loaded.
            // So we need to update the data in the PRG ROM at one address and
            // update the pointer table to point to another SRAM address.
            var a = asm.Module();
            a.Segment($"PRG{bank}");

            List<byte> newTable = new(400);
            // default empty enemy table at offset 0 for maps we don't use
            newTable.AddRange(emptyTable);
            // we need a 2nd empty table at start to reset large encounter on game over
            newTable.AddRange(emptyTable);

            foreach (World world in worldsInBank)
            {
                int[] newPointers = new int[64];
                int tableRomAddrBase = NesPointer.ConvertNesPtrToPrgRomAddr(bank, tablePrgBaseAddr);

                // encounters have a second enemy list (small & large encounter)
                foreach (int map in world.overworldEncounterMaps)
                {
                    int i = world.enemyPtr + map * 2;
                    int enemiesNesPtr = romData.GetShort(i + 1, i);
                    Debug.Assert((enemiesNesPtr & 0xF000) == 0x7000, "All valid rooms have their enemy tables in SRAM");
                    // The encounter enemy tables point (mostly) to SRAM.
                    // As SRAM is not part of the ROM directly, we instead
                    // point to the source PRG data that the game will copy.
                    int addr1 = tableRomAddrBase + enemiesNesPtr - tableRamBaseAddr;
                    byte enemiesLength1 = romData.GetByte(addr1);

                    // Encounters have a big and small variant. The address
                    // they point to contain two lists.
                    //
                    // Go past the 1st enemy list to get 2nd enemy list.
                    int addr2 = addr1 + enemiesLength1;

                    int j = world.sideviewPtrTable + map * 2;
                    int sideviewNesPtr = romData.GetShort(j + 1, j);
                    int sideviewAddr = NesPointer.ConvertNesPtrToPrgRomAddr(world.sideviewBank, sideviewNesPtr);
                    byte[] sideviewBytes = romData.GetBytes(sideviewAddr, romData.GetByte(sideviewAddr));

                    byte[] enemiesBytes1 = romData.GetBytes(addr1, enemiesLength1);
                    byte[] newEnemyBytes1 = RandomizeEnemiesInner(world, sideviewBytes, enemiesBytes1, true, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
                    Debug.Assert(newEnemyBytes1.Length == enemiesLength1);
                    byte enemiesLength2 = romData.GetByte(addr2);
                    byte[] enemiesBytes2 = romData.GetBytes(addr2, enemiesLength2);
                    byte[] newEnemyBytes2 = RandomizeEnemiesInner(world, sideviewBytes, enemiesBytes2, true, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
                    Debug.Assert(newEnemyBytes2.Length == enemiesLength2);

                    newPointers[map] = newTable.Count;
                    newTable.AddRange(newEnemyBytes1);
                    newTable.AddRange(newEnemyBytes2);
                }
                foreach (int map in world.overworldEncounterMapDuplicate)
                {
                    // this is needed so we don't run out of space, and
                    // it's only used when there are already duplicates in vanilla
                    newPointers[map] = newPointers[map - 1];
                }
                foreach (int map in world.nonEncounterMaps)
                {
                    int i = world.enemyPtr + map * 2;
                    int enemiesNesPtr = romData.GetShort(i + 1, i);
                    Debug.Assert((enemiesNesPtr & 0xF000) == 0x7000, "All valid rooms have their enemy tables in SRAM");
                    int addr = tableRomAddrBase + enemiesNesPtr - tableRamBaseAddr;

                    int j = world.sideviewPtrTable + map * 2;
                    int sideviewNesPtr = romData.GetShort(j + 1, j);
                    int sideviewAddr = NesPointer.ConvertNesPtrToPrgRomAddr(world.sideviewBank, sideviewNesPtr);
                    byte[] sideviewBytes = romData.GetBytes(sideviewAddr, romData.GetByte(sideviewAddr));

                    int enemiesLength = romData.GetByte(addr);
                    if (enemiesLength < 3)
                    {
                        continue; // no enemies to randomize here
                    }
                    byte[] enemiesBytes = romData.GetBytes(addr, enemiesLength);
                    byte[] newEnemyBytes = RandomizeEnemiesInner(world, sideviewBytes, enemiesBytes, false, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
                    Debug.Assert(newEnemyBytes.Length == enemiesLength);

                    newPointers[map] = newTable.Count;
                    newTable.AddRange(newEnemyBytes);
                }
                a.RomOrg(world.enemyPtr);
                a.Word(newPointers.Select(p => (ushort)(tableRamBaseAddr + p)).ToArray());
            }
            // 0x3ff bytes of data are copied to RAM
            Debug.Assert(newTable.Count < 0x400);
            // in PRG1, 0x39c bytes are used for the table already,
            // then we made space for another 0x18 bytes
            Debug.Assert(bank != 1 || newTable.Count < 0x39c + 0x18);
            Debug.Assert(bank != 2 || newTable.Count < 0x3c2);
            a.Org((ushort)tablePrgBaseAddr);
            a.Byt(newTable.ToArray());
        }
    }

    /// Recreate Enemy generic type and proceed
    ///
    /// This is just a step that is needed so we can create the
    /// Generic Enemy type. We are basically unpacking T from a property
    /// as best as is possible in C#.
    protected static byte[] RandomizeEnemiesInner(World world, byte[] sideviewBytes, byte[] enemyBytes, bool encounter, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG)
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
    protected static byte[] RandomizeEnemiesInner<T>(byte[] sideviewBytes, GroupedEnemies<T> groupedEnemies, EnemiesEditable<T> ee, bool encounter, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG) where T : Enum
    {
        bool[,]? solidGrid = null;
        byte objectSet = (byte)((sideviewBytes[1] & 0b10000000) >> 7);
        bool[,] svGrid = (objectSet == 0) ?
            new SideviewEditable<ForestObject>(sideviewBytes).CreateSolidGrid() :
            new SideviewEditable<CaveObject>(sideviewBytes).CreateSolidGrid();

        if (solidGrid == null)
        {
            solidGrid = svGrid;
        }
        else
        {
            solidGrid = SolidGridHelper.GridUnion(solidGrid, svGrid);
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
                var newY = SolidGridHelper.FindFloor(solidGrid, enemy.X, enemy.Y, 1, 5 - j);
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
                    var leeverFloor = SolidGridHelper.FindFloor(solidGrid, enemy.X, enemy.Y, 1, 1);
                    leeverFloor--; // because Leevers have strange y positioning.
                    if (leeverFloor < 7)
                    {
                        return false; // Don't roll Leevers above ground level
                    }
                    enemy.Y = Math.Min(9, leeverFloor);
                    return true;
                default:
                    var defaultFloor = SolidGridHelper.FindFloor(solidGrid, enemy.X, enemy.Y, 1, 1);
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
                    var defaultFloor = SolidGridHelper.FindFloor(solidGrid, enemy.X, enemy.Y, 1, 2);
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
