using System;

namespace RandomizerCore.Sidescroll;

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
