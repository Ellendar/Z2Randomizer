using System;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

/// <summary>
/// IDs for overworld forest sideviews. Overworld sideviews with ObjectSet bit set to 0 in the header, to be exact.
/// </summary>
public enum ForestObject
{
    HEADSTONE = 0x00,
    CROSS = 0x01,
    ANGLED_CROSS = 0x02,
    TREE_STUMP = 0x03,
    STONEHENGE = 0x04,
    LOCKED_DOOR = 0x05,
    SLEEPING_ZELDA_06 = 0x06,
    SLEEPING_ZELDA_07 = 0x07,
    PIT_VERTICAL = 0x08,
    LARGE_CLOUD = 0x09,
    SMALL_CLOUD_0A = 0x0A,
    SMALL_CLOUD_0B = 0x0B,
    SMALL_CLOUD_0C = 0x0C,
    SMALL_CLOUD_0D = 0x0D,
    SMALL_CLOUD_0E = 0x0E,
    COLLECTABLE = 0x0F,
    FOREST_CEILING_20 = 0x20,
    FOREST_CEILING_30 = 0x30,
    CURTAINS_2_ROWS = 0x40,
    CURTAINS_1_ROW = 0x50,
    BREAKABLE_BLOCK = 0x60,
    PIT = 0x70,
    SINGLE_WEED = 0x80,
    DOUBLE_WEED = 0x90,
    NORTH_CASTLE_STEPS = 0xA0,
    NORTH_CASTLE_BRICK = 0xB0,
    VOLCANO_BACKGROUND = 0xC0,
    BREAKABLE_BLOCK_VERTICAL = 0xD0,
    TREE_TRUNK = 0xE0,
    PILLAR = 0xF0
}

/// <summary>
/// IDs for overworld cave sideviews. Overworld sideviews with ObjectSet bit set to 1 in the header, to be exact.
/// </summary>
public enum CaveObject
{
    HEADSTONE = 0x00,
    CROSS = 0x01,
    ANGLED_CROSS = 0x02,
    TREE_STUMP = 0x03,
    STONEHENGE = 0x04,
    LOCKED_DOOR = 0x05,
    SLEEPING_ZELDA_06 = 0x06,
    SLEEPING_ZELDA_07 = 0x07,
    PIT_VERTICAL = 0x08,
    LARGE_CLOUD = 0x09,
    SMALL_CLOUD_0A = 0x0A,
    SMALL_CLOUD_0B = 0x0B,
    SMALL_CLOUD_0C = 0x0C,
    SMALL_CLOUD_0D = 0x0D,
    SMALL_CLOUD_0E = 0x0E,
    COLLECTABLE = 0x0F,
    ROCK_FLOOR = 0x20,
    ROCK_CEILING = 0x30,
    BRIDGE = 0x40,
    ROCK_PLATFORM = 0x50,
    BREAKABLE_BLOCK = 0x60,
    CRUMBLE_BRIDGE = 0x70,
    SINGLE_WEED = 0x80,
    DOUBLE_WEED = 0x90,
    PIT = 0xA0,
    NORTH_CASTLE_BRICK = 0xB0,
    VOLCANO_BACKGROUND = 0xC0,
    BREAKABLE_BLOCK_VERTICAL = 0xD0,
    ROCK_FLOOR_VERTICAL = 0xE0,
    STONE_SPIRE = 0xF0
}

/// <summary>
/// IDs that are shared in all palaces.
/// 
/// It's a class of constants and not an enum, so that
/// PalaceObjectShared.Window == PalaceObject.Window is true.
/// </summary>
public static class PalaceObjectShared
{
    public const int WINDOW = 0x00;
    public const int DRAGON_HEAD = 0x01;
    public const int WOLF_HEAD = 0x02;
    public const int CRYSTAL_STATUE_03 = 0x03;
    public const int CRYSTAL_STATUE_04 = 0x04;
    public const int LOCKED_DOOR = 0x05;
    public const int LARGE_CLOUD = 0x07;
    public const int SMALL_CLOUD_08 = 0x08;
    public const int SMALL_CLOUD_0B = 0x0B;
    public const int SMALL_CLOUD_0C = 0x0C;
    public const int SMALL_CLOUD_0D = 0x0D;
    public const int SMALL_CLOUD_0E = 0x0E;
    public const int COLLECTABLE = 0x0F;
    public const int BRICK_1_ROW = 0x20;
    public const int BREAKABLE_BLOCK_1_ROW = 0x30;
    public const int STEEL_BRICK = 0x40;
}

/// <summary>
/// IDs for palace 1-6
/// </summary>
public enum PalaceObject
{
    WINDOW = 0x00,
    DRAGON_HEAD = 0x01,
    WOLF_HEAD = 0x02,
    CRYSTAL_STATUE_03 = 0x03,
    CRYSTAL_STATUE_04 = 0x04,
    LOCKED_DOOR = 0x05,
    LARGE_CLOUD = 0x07,
    SMALL_CLOUD_08 = 0x08,
    IRON_KNUCKLE_STATUE = 0x09,
    SMALL_CLOUD_0A = 0x0A,
    SMALL_CLOUD_0B = 0x0B,
    SMALL_CLOUD_0C = 0x0C,
    SMALL_CLOUD_0D = 0x0D,
    SMALL_CLOUD_0E = 0x0E,
    COLLECTABLE = 0x0F,
    PIT_OR_LAVA = 0x10,
    BRICK_1_ROW = 0x20,
    BREAKABLE_BLOCK_1_ROW = 0x30,
    STEEL_BRICK = 0x40,
    CRUMBLE_BRIDGE_OR_ELEVATOR = 0x50,
    BRIDGE = 0x60,
    BRICK_2_ROWS = 0x70,
    CURTAINS = 0x80,
    BREAKABLE_BLOCK_2_ROWS = 0x90,
    WALKTHROUGH_WALL = 0xA0,
    BREAKABLE_BLOCK_VERTICAL = 0xB0,
    PILLAR = 0xC0,
    PIT_VERTICAL_D0 = 0xD0,
    PIT_VERTICAL_E0 = 0xE0,
    PIT = 0xF0,
}

/// <summary>
/// IDs for Great Palace
/// </summary>
public enum GreatPalaceObject
{
    WINDOW = 0x00,
    DRAGON_HEAD = 0x01,
    WOLF_HEAD = 0x02,
    CRYSTAL_STATUE_03 = 0x03,
    CRYSTAL_STATUE_04 = 0x04,
    LOCKED_DOOR = 0x05,
    PIT_VERTICAL = 0x06,
    LARGE_CLOUD = 0x07,
    SMALL_CLOUD_08 = 0x08,
    SLEEPING_ZELDA = 0x09,
    FOKKA_STATUE = 0x0A,
    SMALL_CLOUD_0B = 0x0B,
    SMALL_CLOUD_0C = 0x0C,
    SMALL_CLOUD_0D = 0x0D,
    SMALL_CLOUD_0E = 0x0E,
    COLLECTABLE = 0x0F,
    FINAL_BOSS_CANOPY_OR_LAVA = 0x10,
    BRICK_1_ROW = 0x20,
    BREAKABLE_BLOCK_1_ROW = 0x30,
    STEEL_BRICK = 0x40,
    NORTH_CASTLE_BRICK_OR_ELEVATOR = 0x50,
    NORTH_CASTLE_STEPS = 0x60,
    CRUMBLE_BRIDGE = 0x70,
    BRIDGE = 0x80,
    BRICK_2_ROWS = 0x90,
    CURTAINS = 0xA0,
    WALKTHROUGH_WALL = 0xB0,
    BREAKABLE_BLOCK_2_ROWS = 0xC0,
    BREAKABLE_BLOCK_VERTICAL = 0xD0,
    ELECTRIC_BARRIER = 0xE0,
    PILLAR = 0xF0
}

public static class ForestObjectExtensions
{
    public static int Width(SideviewMapCommand<ForestObject> command)
    {
        switch (command.Id)
        {
            case ForestObject.FOREST_CEILING_20:
            case ForestObject.FOREST_CEILING_30:
            case ForestObject.CURTAINS_2_ROWS:
            case ForestObject.CURTAINS_1_ROW:
            case ForestObject.BREAKABLE_BLOCK:
            case ForestObject.PIT:
            case ForestObject.SINGLE_WEED:
            case ForestObject.DOUBLE_WEED:
            case ForestObject.NORTH_CASTLE_STEPS:
            case ForestObject.NORTH_CASTLE_BRICK:
            case ForestObject.VOLCANO_BACKGROUND:
                return 1 + command.Param;
            case ForestObject.SLEEPING_ZELDA_06:
            case ForestObject.SLEEPING_ZELDA_07:
            case ForestObject.LARGE_CLOUD:
            case ForestObject.SMALL_CLOUD_0A:
            case ForestObject.SMALL_CLOUD_0B:
            case ForestObject.SMALL_CLOUD_0C:
            case ForestObject.SMALL_CLOUD_0D:
            case ForestObject.SMALL_CLOUD_0E:
                return 2;
            case ForestObject.STONEHENGE:
                return 4;
            default:
                return 1;
        }
    }

    public static int Height(SideviewMapCommand<ForestObject> command)
    {
        switch (command.Id)
        {
            case ForestObject.BREAKABLE_BLOCK_VERTICAL:
            case ForestObject.TREE_TRUNK:
            case ForestObject.PILLAR:
                return 1 + command.Param;
            case ForestObject.PIT_VERTICAL:
                return 13 - command.Y;
            case ForestObject.TREE_STUMP:
            case ForestObject.FOREST_CEILING_20:
            case ForestObject.FOREST_CEILING_30:
            case ForestObject.CURTAINS_2_ROWS:
                return 2;
            case ForestObject.STONEHENGE:
            case ForestObject.LOCKED_DOOR:
                return 3;
            default:
                return 1;
        }
    }

    public static bool IsSolid(SideviewMapCommand<ForestObject> command)
    {
        switch (command.Id)
        {
            case ForestObject.TREE_STUMP:
            case ForestObject.STONEHENGE:
            case ForestObject.FOREST_CEILING_20:
            case ForestObject.FOREST_CEILING_30:
            case ForestObject.BREAKABLE_BLOCK:
            case ForestObject.BREAKABLE_BLOCK_VERTICAL:
                return true;
            default:
                return false;
        }
    }

    public static bool IsSolidAt(SideviewMapCommand<ForestObject> command, int x, int y)
    {
        return IsSolid(command);
    }

    public static bool IsBreakable(SideviewMapCommand<ForestObject> command)
    {
        switch (command.Id)
        {
            case ForestObject.BREAKABLE_BLOCK:
            case ForestObject.BREAKABLE_BLOCK_VERTICAL:
                return true;
            default:
                return false;
        }
    }

    public static bool IsPit(SideviewMapCommand<ForestObject> command)
    {
        switch (command.Id)
        {
            case ForestObject.PIT:
            case ForestObject.PIT_VERTICAL:
                return true;
            default:
                return false;
        }
    }
}

public static class CaveObjectExtensions
{
    public static int Width(SideviewMapCommand<CaveObject> command)
    {
        switch (command.Id)
        {
            case CaveObject.ROCK_FLOOR:
            case CaveObject.ROCK_CEILING:
            case CaveObject.BRIDGE:
            case CaveObject.ROCK_PLATFORM:
            case CaveObject.BREAKABLE_BLOCK:
            case CaveObject.CRUMBLE_BRIDGE:
            case CaveObject.SINGLE_WEED:
            case CaveObject.DOUBLE_WEED:
            case CaveObject.PIT:
            case CaveObject.NORTH_CASTLE_BRICK:
            case CaveObject.VOLCANO_BACKGROUND:
                return 1 + command.Param;
            case CaveObject.SLEEPING_ZELDA_06:
            case CaveObject.SLEEPING_ZELDA_07:
            case CaveObject.LARGE_CLOUD:
            case CaveObject.SMALL_CLOUD_0A:
            case CaveObject.SMALL_CLOUD_0B:
            case CaveObject.SMALL_CLOUD_0C:
            case CaveObject.SMALL_CLOUD_0D:
            case CaveObject.SMALL_CLOUD_0E:
                return 2;
            case CaveObject.STONEHENGE:
                return 4;
            default:
                return 1;
        }
    }

    public static int Height(SideviewMapCommand<CaveObject> command)
    {
        switch (command.Id)
        {
            case CaveObject.BREAKABLE_BLOCK_VERTICAL:
            case CaveObject.ROCK_FLOOR_VERTICAL:
            case CaveObject.STONE_SPIRE:
                return 1 + command.Param;
            case CaveObject.PIT_VERTICAL:
                return 13 - command.Y;
            case CaveObject.TREE_STUMP:
            case CaveObject.ROCK_FLOOR:
            case CaveObject.ROCK_CEILING:
            case CaveObject.BRIDGE:
                return 2;
            case CaveObject.STONEHENGE:
            case CaveObject.LOCKED_DOOR:
                return 3;
            default:
                return 1;
        }
    }

    public static bool IsSolid(SideviewMapCommand<CaveObject> command)
    {
        switch (command.Id)
        {
            case CaveObject.TREE_STUMP:
            case CaveObject.STONEHENGE:
            case CaveObject.ROCK_FLOOR:
            case CaveObject.ROCK_CEILING:
            case CaveObject.BRIDGE:
            case CaveObject.ROCK_PLATFORM:
            case CaveObject.CRUMBLE_BRIDGE:
            case CaveObject.BREAKABLE_BLOCK:
            case CaveObject.BREAKABLE_BLOCK_VERTICAL:
            case CaveObject.ROCK_FLOOR_VERTICAL:
            case CaveObject.STONE_SPIRE:
                return true;
            default:
                return false;
        }
    }

    public static bool IsSolidAt(SideviewMapCommand<CaveObject> command, int x, int y)
    {
        switch (command.Id)
        {
            case CaveObject.BRIDGE:
                return y == command.Y + 1;
            default:
                return IsSolid(command);
        }
    }

    public static bool IsBreakable(SideviewMapCommand<CaveObject> command)
    {
        switch (command.Id)
        {
            case CaveObject.BREAKABLE_BLOCK:
            case CaveObject.BREAKABLE_BLOCK_VERTICAL:
                return true;
            default:
                return false;
        }
    }

    public static bool IsPit(SideviewMapCommand<CaveObject> command)
    {
        switch (command.Id)
        {
            case CaveObject.PIT:
            case CaveObject.PIT_VERTICAL:
                return true;
            default:
                return false;
        }
    }
}

public static class PalaceObjectExtensions
{
    public static int Width(SideviewMapCommand<PalaceObject> command)
    {
        switch (command.Id)
        {
            case PalaceObject.PIT_OR_LAVA:
            case PalaceObject.BRICK_1_ROW:
            case PalaceObject.BREAKABLE_BLOCK_1_ROW:
            case PalaceObject.STEEL_BRICK:
            case PalaceObject.CRUMBLE_BRIDGE_OR_ELEVATOR:
            case PalaceObject.BRIDGE:
            case PalaceObject.BRICK_2_ROWS:
            case PalaceObject.CURTAINS:
            case PalaceObject.BREAKABLE_BLOCK_2_ROWS:
            case PalaceObject.WALKTHROUGH_WALL:
            case PalaceObject.PIT:
                return 1 + command.Param;
            case PalaceObject.LARGE_CLOUD:
            case PalaceObject.SMALL_CLOUD_08:
            case PalaceObject.SMALL_CLOUD_0A:
            case PalaceObject.SMALL_CLOUD_0B:
            case PalaceObject.SMALL_CLOUD_0C:
            case PalaceObject.SMALL_CLOUD_0D:
            case PalaceObject.SMALL_CLOUD_0E:
                return 2;
            case PalaceObject.CRYSTAL_STATUE_03:
            case PalaceObject.CRYSTAL_STATUE_04:
                return 4;
            default:
                return 1;
        }
    }

    public static int Height(SideviewMapCommand<PalaceObject> command)
    {
        switch (command.Id)
        {
            case PalaceObject.BREAKABLE_BLOCK_VERTICAL:
            case PalaceObject.PILLAR:
            case PalaceObject.PIT_VERTICAL_D0:
            case PalaceObject.PIT_VERTICAL_E0:
                return 1 + command.Param;
            case PalaceObject.PIT:
                return 13 - command.Y;
            case PalaceObject.WINDOW:
            case PalaceObject.IRON_KNUCKLE_STATUE:
            case PalaceObject.BRIDGE:
            case PalaceObject.BRICK_2_ROWS:
            case PalaceObject.CURTAINS:
            case PalaceObject.BREAKABLE_BLOCK_2_ROWS:
            case PalaceObject.WALKTHROUGH_WALL:
                return 2;
            case PalaceObject.LOCKED_DOOR:
                return 3;
            case PalaceObject.CRYSTAL_STATUE_03:
            case PalaceObject.CRYSTAL_STATUE_04:
                return 5;
            default:
                return 1;
        }
    }

    public static bool IsSolid(SideviewMapCommand<PalaceObject> command)
    {
        switch (command.Id)
        {
            case PalaceObject.WINDOW:
            case PalaceObject.DRAGON_HEAD:
            case PalaceObject.WOLF_HEAD:
            case PalaceObject.CRYSTAL_STATUE_03:
            case PalaceObject.CRYSTAL_STATUE_04:
            case PalaceObject.LOCKED_DOOR:
            case PalaceObject.LARGE_CLOUD:
            case PalaceObject.SMALL_CLOUD_08:
            case PalaceObject.IRON_KNUCKLE_STATUE:
            case PalaceObject.SMALL_CLOUD_0A:
            case PalaceObject.SMALL_CLOUD_0B:
            case PalaceObject.SMALL_CLOUD_0C:
            case PalaceObject.SMALL_CLOUD_0D:
            case PalaceObject.SMALL_CLOUD_0E:
            case PalaceObject.COLLECTABLE:
            case PalaceObject.PIT_OR_LAVA:
            case PalaceObject.CURTAINS:
            case PalaceObject.WALKTHROUGH_WALL:
            case PalaceObject.PILLAR:
            case PalaceObject.PIT_VERTICAL_D0:
            case PalaceObject.PIT_VERTICAL_E0:
            case PalaceObject.PIT:
                return false;
            case PalaceObject.BRICK_1_ROW:
            case PalaceObject.BREAKABLE_BLOCK_1_ROW:
            case PalaceObject.STEEL_BRICK:
            case PalaceObject.CRUMBLE_BRIDGE_OR_ELEVATOR:
            case PalaceObject.BRIDGE:
            case PalaceObject.BRICK_2_ROWS:
            case PalaceObject.BREAKABLE_BLOCK_2_ROWS:
            case PalaceObject.BREAKABLE_BLOCK_VERTICAL:
                return true;
            default:
                throw new NotImplementedException();
        }
    }

    public static bool IsSolidAt(SideviewMapCommand<PalaceObject> command, int x, int y)
    {
        switch (command.Id)
        {
            case PalaceObject.BRIDGE:
                return y == command.Y + 1;
            default:
                return IsSolid(command);
        }
    }

    public static bool IsBreakable(SideviewMapCommand<PalaceObject> command)
    {
        switch (command.Id)
        {
            case PalaceObject.BREAKABLE_BLOCK_1_ROW:
            case PalaceObject.BREAKABLE_BLOCK_2_ROWS:
            case PalaceObject.BREAKABLE_BLOCK_VERTICAL:
                return true;
            default:
                return false;
        }
    }

    public static bool IsPit(SideviewMapCommand<PalaceObject> command)
    {
        switch (command.Id)
        {
            case PalaceObject.PIT_OR_LAVA:
            case PalaceObject.WALKTHROUGH_WALL:
            case PalaceObject.PIT_VERTICAL_D0:
            case PalaceObject.PIT_VERTICAL_E0:
            case PalaceObject.PIT:
                return true;
            default:
                return false;
        }
    }
}

public static class GreatPalaceObjectExtensions
{
    public static int Width(SideviewMapCommand<GreatPalaceObject> command)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.FINAL_BOSS_CANOPY_OR_LAVA:
            case GreatPalaceObject.BRICK_1_ROW:
            case GreatPalaceObject.BREAKABLE_BLOCK_1_ROW:
            case GreatPalaceObject.STEEL_BRICK:
            case GreatPalaceObject.NORTH_CASTLE_BRICK_OR_ELEVATOR:
            case GreatPalaceObject.NORTH_CASTLE_STEPS:
            case GreatPalaceObject.CRUMBLE_BRIDGE:
            case GreatPalaceObject.BRIDGE:
            case GreatPalaceObject.BRICK_2_ROWS:
            case GreatPalaceObject.CURTAINS:
            case GreatPalaceObject.WALKTHROUGH_WALL:
            case GreatPalaceObject.BREAKABLE_BLOCK_2_ROWS:
                return 1 + command.Param;
            case GreatPalaceObject.LARGE_CLOUD:
            case GreatPalaceObject.SMALL_CLOUD_08:
            case GreatPalaceObject.SMALL_CLOUD_0B:
            case GreatPalaceObject.SMALL_CLOUD_0C:
            case GreatPalaceObject.SMALL_CLOUD_0D:
            case GreatPalaceObject.SMALL_CLOUD_0E:
            case GreatPalaceObject.SLEEPING_ZELDA:
                return 2;
            case GreatPalaceObject.CRYSTAL_STATUE_03:
            case GreatPalaceObject.CRYSTAL_STATUE_04:
                return 4;
            default:
                return 1;
        }
    }

    public static int Height(SideviewMapCommand<GreatPalaceObject> command)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.BREAKABLE_BLOCK_VERTICAL:
            case GreatPalaceObject.ELECTRIC_BARRIER:
            case GreatPalaceObject.PILLAR:
                return 1 + command.Param;
            case GreatPalaceObject.PIT_VERTICAL:
                return 13 - command.Y;
            case GreatPalaceObject.WINDOW:
            case GreatPalaceObject.FOKKA_STATUE:
            case GreatPalaceObject.BRIDGE:
            case GreatPalaceObject.BRICK_2_ROWS:
            case GreatPalaceObject.CURTAINS:
            case GreatPalaceObject.WALKTHROUGH_WALL:
            case GreatPalaceObject.BREAKABLE_BLOCK_2_ROWS:
                return 2;
            case GreatPalaceObject.LOCKED_DOOR:
                return 3;
            case GreatPalaceObject.CRYSTAL_STATUE_03:
            case GreatPalaceObject.CRYSTAL_STATUE_04:
                return 5;
            default:
                return 1;
        }
    }

    public static bool IsSolid(SideviewMapCommand<GreatPalaceObject> command)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.WINDOW:
            case GreatPalaceObject.DRAGON_HEAD:
            case GreatPalaceObject.WOLF_HEAD:
            case GreatPalaceObject.CRYSTAL_STATUE_03:
            case GreatPalaceObject.CRYSTAL_STATUE_04:
            case GreatPalaceObject.LOCKED_DOOR:
            case GreatPalaceObject.PIT_VERTICAL:
            case GreatPalaceObject.LARGE_CLOUD:
            case GreatPalaceObject.SMALL_CLOUD_08:
            case GreatPalaceObject.SLEEPING_ZELDA:
            case GreatPalaceObject.FOKKA_STATUE:
            case GreatPalaceObject.SMALL_CLOUD_0B:
            case GreatPalaceObject.SMALL_CLOUD_0C:
            case GreatPalaceObject.SMALL_CLOUD_0D:
            case GreatPalaceObject.SMALL_CLOUD_0E:
            case GreatPalaceObject.COLLECTABLE:
            case GreatPalaceObject.FINAL_BOSS_CANOPY_OR_LAVA:
            case GreatPalaceObject.CURTAINS:
            case GreatPalaceObject.WALKTHROUGH_WALL:
            case GreatPalaceObject.ELECTRIC_BARRIER:
            case GreatPalaceObject.PILLAR:
                return false;
            case GreatPalaceObject.BRICK_1_ROW:
            case GreatPalaceObject.BREAKABLE_BLOCK_1_ROW:
            case GreatPalaceObject.STEEL_BRICK:
            case GreatPalaceObject.NORTH_CASTLE_BRICK_OR_ELEVATOR:
            case GreatPalaceObject.NORTH_CASTLE_STEPS:
            case GreatPalaceObject.CRUMBLE_BRIDGE:
            case GreatPalaceObject.BRIDGE:
            case GreatPalaceObject.BRICK_2_ROWS:
            case GreatPalaceObject.BREAKABLE_BLOCK_2_ROWS:
            case GreatPalaceObject.BREAKABLE_BLOCK_VERTICAL:
                return true;
            default:
                throw new NotImplementedException();
        }
    }

    public static bool IsSolidAt(SideviewMapCommand<GreatPalaceObject> command, int x, int y)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.BRIDGE:
                return y == command.Y + 1;
            default:
                return IsSolid(command);
        }
    }

    public static bool IsBreakable(SideviewMapCommand<GreatPalaceObject> command)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.BREAKABLE_BLOCK_1_ROW:
            case GreatPalaceObject.BREAKABLE_BLOCK_2_ROWS:
            case GreatPalaceObject.BREAKABLE_BLOCK_VERTICAL:
                return true;
            default:
                return false;
        }
    }

    public static bool IsPit(SideviewMapCommand<GreatPalaceObject> command)
    {
        switch (command.Id)
        {
            case GreatPalaceObject.PIT_VERTICAL:
            case GreatPalaceObject.WALKTHROUGH_WALL:
                return true;
            default:
                return false;
        }
    }
}
