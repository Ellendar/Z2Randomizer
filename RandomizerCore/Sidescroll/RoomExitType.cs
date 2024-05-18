namespace RandomizerCore.Sidescroll;

public enum RoomExitType
{
    DEADEND_UP, DEADEND_DOWN, DEADEND_LEFT, DEADEND_RIGHT,
    HORIZONTAL_PASSTHROUGH, VERTICAL_PASSTHROUGH, 
    NE_L, SE_L, SW_L, NW_L,
    T, INVERSE_T, LEFT_T, RIGHT_T,
    FOUR_WAY
}

public static class RoomExitTypeExtensions
{
    public static bool ContainsDown(this RoomExitType exitType)
    {
        return exitType switch
        {
            RoomExitType.DEADEND_DOWN => true,
            RoomExitType.VERTICAL_PASSTHROUGH => true,
            RoomExitType.SW_L => true,
            RoomExitType.SE_L => true,
            RoomExitType.LEFT_T => true,
            RoomExitType.T => true,
            RoomExitType.RIGHT_T => true,
            RoomExitType.FOUR_WAY => true,
            _ => false
        };
    }
}