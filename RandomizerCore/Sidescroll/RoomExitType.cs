namespace RandomizerCore.Sidescroll;

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

    //Left, down, drop down, up, right
    //Drop down
    DROP_STUB = 0b00100,
    DROP_SE_L = 0b00101,
    DROP_COLUMN = 0b00110,
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
}