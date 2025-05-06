using System;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public enum RoomExitType
{
    //Left, down, drop down, up, right

    //No down
    NO_ESCAPE = 0b00000,
    DEADEND_EXIT_RIGHT = 0b00001,
    DEADEND_EXIT_UP = 0b00010,
    NE_L = 0b00011,
    DEADEND_EXIT_LEFT = 0b10000,
    HORIZONTAL_PASSTHROUGH = 0b10001,
    NW_L = 0b10010,
    INVERSE_T = 0b10011,

    //Elevator Down
    DEADEND_EXIT_DOWN = 0b01000,
    SE_L = 0b01001,
    VERTICAL_PASSTHROUGH = 0b01010,
    RIGHT_T = 0b01011,
    SW_L = 0b11000,
    T = 0b11001,
    LEFT_T = 0b11010,
    FOUR_WAY = 0b11011,

    //Drop down
    DROP_STUB = 0b00100,
    DROP_SE_L = 0b00101,
    DROP_ELEVATOR_UP = 0b00110,
    DROP_RIGHT_T = 0b00111,
    DROP_SW_L = 0b10100,
    DROP_T = 0b10101,
    DROP_LEFT_T = 0b10110,
    DROP_FOUR_WAY = 0b10111,
}

public static class RoomExitTypeExtensions
{
    public const int LEFT = 0b00010000;
    public const int DOWN = 0b00001000;
    public const int DROP = 0b00000100;
    public const int UP = 0b00000010;
    public const int RIGHT = 0b00000001;
    public static bool ContainsRight(this RoomExitType exitType)
    {
        return ((int)exitType & RIGHT) == RIGHT;
    }
    public static bool ContainsUp(this RoomExitType exitType)
    {
        return ((int)exitType & UP) == UP;
    }
    public static bool ContainsDrop(this RoomExitType exitType)
    {
        return ((int)exitType & DROP) == DROP;
    }
    public static bool ContainsDown(this RoomExitType exitType)
    {
        return ((int)exitType & DOWN) == DOWN;
    }
    public static bool ContainsLeft(this RoomExitType exitType)
    {
        return ((int)exitType & LEFT) == LEFT;
    }

    public static RoomExitType AddUp(this RoomExitType exitType)
    {
        return (RoomExitType)((int)exitType | UP);
    }

    public static RoomExitType AddDown(this RoomExitType exitType)
    {
        if(exitType.ContainsDrop())
        {
            throw new Exception("Can't add down to a room that drops");
        }
        return (RoomExitType)((int)exitType | DOWN);
    }
    public static RoomExitType AddDrop(this RoomExitType exitType)
    {
        if (exitType.ContainsDrop())
        {
            throw new Exception("Can't add drop to a room that downs");
        }
        return (RoomExitType)((int)exitType | DROP);
    }

    public static RoomExitType AddLeft(this RoomExitType exitType)
    {
        return (RoomExitType)((int)exitType | LEFT);
    }

    public static RoomExitType AddRight(this RoomExitType exitType)
    {
        return (RoomExitType)((int)exitType | RIGHT);
    }

    public static RoomExitType ConvertToDrop(this RoomExitType exitType)
    {
        if(!exitType.ContainsDown())
        {
            throw new Exception("Cannot convert non-down room to drop");
        }
        return (RoomExitType)((int)exitType & 0b10111 | DROP);
    }

    public static RoomExitType ConvertFromDropToDown(this RoomExitType exitType)
    {
        if (!exitType.ContainsDrop())
        {
            throw new Exception("Cannot convert non-drop room to down");
        }
        return (RoomExitType)((int)exitType & 0b11011 | DOWN);
    }

    public static RoomExitType RemoveUp(this RoomExitType exitType)
    {
        return (RoomExitType)((int)exitType & 0b11101);
    }

    public static RoomExitType Merge(this RoomExitType exitType, RoomExitType toMerge)
    {
        return exitType | toMerge;
    }
}