using System;
using System.Linq;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore.Enemy;

public class PalaceEnemyShuffler
{
    public static byte[] Shuffle(Room room, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG)
    {
        //Because a 125 room could be shuffled into 346 or vice versa, we have to check if the enemy is that type in either
        //palace group, and if so, shuffle that enemy into a new enemy specifically appropriate to that palace
        switch (room.PalaceGroup)
        {
            case PalaceGrouping.Palace125:
                var g125 = Enemies.GroupedPalace125Enemies;
                var ee125 = new EnemiesEditable<EnemiesPalace125>(room.Enemies);
                return RandomizeEnemiesInner(room, g125, ee125, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
            case PalaceGrouping.Palace346:
                var g346 = Enemies.GroupedPalace346Enemies;
                var ee346 = new EnemiesEditable<EnemiesPalace346>(room.Enemies);
                return RandomizeEnemiesInner(room, g346, ee346, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
            case PalaceGrouping.PalaceGp:
                var gGp = Enemies.GroupedGreatPalaceEnemies;
                var eeGp = new EnemiesEditable<EnemiesGreatPalace>(room.Enemies);
                return RandomizeEnemiesInner(room, gGp, eeGp, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG);
            default:
                throw new ImpossibleException("Invalid Palace Group");
        }
    }

    private static byte[] RandomizeEnemiesInner<T>(Room room, GroupedEnemies<T> groupedEnemies, EnemiesEditable<T> ee, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG) where T : Enum
    {
        bool[,]? solidGridLazy = null; // lazily instanced if needed
        bool[,] GetSolidGrid<P>() where P : Enum
        {
            if (solidGridLazy == null)
            {
                var sv = new SideviewEditable<P>(room.SideView);
                solidGridLazy = sv.CreateSolidGrid();
            }
            return solidGridLazy;
        }
        bool AreaIsOpen<P>(ref bool? cachedResult, int x, int y, int w, int h) where P : Enum
        {
            if (cachedResult == null)
            {
                var solidGrid = GetSolidGrid<P>();
                cachedResult = SolidGridHelper.AreaIsOpen(solidGrid, x, y, w, h);
            }
            return cachedResult.Value;
        }
        bool PositionKingBot(ref bool? cachedResult, Enemy<T> enemy)
        {
            // Vanilla King Bot positions for reference:
            // GP Map 37 at y == 1 where floor is at y == 8
            // GP Map 48 at y == 3 where floor is at y == 11

            // Do our best to position King Bots high up, so they are unlikely to
            // spawn inside the player's position.  y==3 is the preferred spot unless
            // the floor is high, as there is also the risk of having the King Bot
            // spawn in your position when you are Fairying over a room.
            // (Enemies in Zelda II cannot be positioned at y == 0 or y == 2.)

            // We also check that we don't elevator or drop straight into a King Bot.

            // Unless we find a 3x6 empty area here we will not spawn a King Bot.
            if (cachedResult == null)
            {
                if (room.ElevatorScreen != -1)
                {
                    var elevatorXPos = (room.ElevatorScreen * 16) + 7;
                    if (elevatorXPos - 3 <= enemy.X && elevatorXPos + 2 >= enemy.X)
                    {
                        cachedResult = false;
                        return false;
                    }
                }
                if (room.IsDropZone)
                {
                    if (14 <= enemy.X && 48 >= enemy.X)
                    {
                        cachedResult = false;
                        return false;
                    }
                }
                var solidGrid = GetSolidGrid<GreatPalaceObject>();

                int newY = 0;
                foreach (int j in new int[] { 3, 4, 1, 5 })
                {
                    if (SolidGridHelper.AreaIsOpen(solidGrid, enemy.X, j, 3, 6))
                    {
                        newY = j;
                        break;
                    }
                }
                if (newY != 0)
                {
                    enemy.Y = newY;
                    cachedResult = true;
                }
                else
                {
                    cachedResult = false;
                }
            }
            return cachedResult.Value;
        }

        T RerollLargeEnemyIfNeeded(Enemy<T> enemy, T swapToId)
        {
            if (room.PalaceGroup != PalaceGrouping.PalaceGp)
            {
                bool? roomForStalfos = null;
                while (true)
                {
                    bool reroll = false;
                    switch (swapToId)
                    {
                        // Re-roll Magos and Wizards unless their y pos is 9.
                        case EnemiesPalace125.MAGO:
                        case EnemiesPalace346.WIZARD:
                            reroll = enemy.Y != 0x09;
                            break;

                        // Re-roll Stalfos if they don't have room to dive from the ceiling to their position
                        case EnemiesPalace125.RED_STALFOS:
                        case EnemiesPalace125.BLUE_STALFOS:
                        case EnemiesPalace346.RED_STALFOS:
                        case EnemiesPalace346.BLUE_STALFOS:
                            // Stalfos will get fully stuck if the ceiling is solid until y=4. If it's solid only until y=3 or y=2, it will slowly slide down.
                            reroll = !AreaIsOpen<PalaceObject>(ref roomForStalfos, enemy.X, 4, 1, Math.Max(enemy.Y - 4, 2));
                            break;
                    }

                    if (reroll)
                    {
                        swapToId = groupedEnemies.LargeEnemies[RNG.Next(0, groupedEnemies.LargeEnemies.Length)];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return swapToId;
        }

        T RerollFlyingEnemyIfNeeded(Enemy<T> enemy, T swapToId)
        {
            if (room.PalaceGroup != PalaceGrouping.PalaceGp)
            {
                // The rando does not re-roll Moas in regular palaces (I just simplified the legacy behavior)
                if (enemy.IdByte == EnemiesRegularPalaceShared.ORANGE_MOA)
                {
                    swapToId = enemy.Id; // swap it back to Moa
                }
            }
            else // GP
            {
                bool? roomForBubble = null;
                bool? roomForBigBubble = null;
                bool? roomForKingBot = null;
                while (true)
                {
                    // Re-roll enemies that do not fit (get stuck in walls)
                    bool reroll = false;
                    switch (swapToId)
                    {
                        case EnemiesGreatPalace.SLOW_BUBBLE:
                        case EnemiesGreatPalace.FAST_BUBBLE:
                            reroll = !AreaIsOpen<GreatPalaceObject>(ref roomForBubble, enemy.X, enemy.Y, 1, 1);
                            break;

                        case EnemiesGreatPalace.BIG_BUBBLE:
                            reroll = !AreaIsOpen<GreatPalaceObject>(ref roomForBigBubble, enemy.X, enemy.Y, 2, 2);
                            break;

                        case EnemiesGreatPalace.KING_BOT:
                            reroll = !PositionKingBot(ref roomForKingBot, enemy); // updates enemy position on success
                            break;
                    }

                    if (reroll)
                    {
                        swapToId = groupedEnemies.FlyingEnemies[RNG.Next(0, groupedEnemies.FlyingEnemies.Length)];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return swapToId;
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
                    if (enemy.IsShufflableSmall() && groupedEnemies.LargeEnemies.Contains(swapToId))
                    {
                        enemy.Y--; // subtract Y by 1 when switching a small enemy to a large enemy
                        swapToId = RerollLargeEnemyIfNeeded(enemy, swapToId);
                    }
                    else
                    {
                        swapToId = RerollLargeEnemyIfNeeded(enemy, swapToId);
                    }
                    enemy.Id = swapToId;
                    continue;
                }
            }
            else
            {
                if (enemy.IsShufflableLarge())
                {
                    T swapToId = groupedEnemies.LargeEnemies[RNG.Next(0, groupedEnemies.LargeEnemies.Length)];
                    swapToId = RerollLargeEnemyIfNeeded(enemy, swapToId);
                    enemy.Id = swapToId;
                    continue;
                }
                else if (enemy.IsShufflableSmall())
                {
                    T swapEnemy = groupedEnemies.SmallEnemies[RNG.Next(0, groupedEnemies.SmallEnemies.Length)];
                    enemy.Id = swapEnemy;
                    continue;
                }
            }

            if (enemy.IsShufflableFlying())
            {
                T swapToId = groupedEnemies.FlyingEnemies[RNG.Next(0, groupedEnemies.FlyingEnemies.Length)];
                swapToId = RerollFlyingEnemyIfNeeded(enemy, swapToId);
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
        }
        return ee.Finalize();
    }
}
