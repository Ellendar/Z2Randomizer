using System;
using System.Collections.Generic;
using System.Text;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public enum SegmentConnectionType
{
    ELEVATOR_DOWN,
    ELEVATOR_UP,
    BOTH_ELEVATORS,
    DROP_INTO,
    DROP_OUT,
    DROP_COLUMN,
    DROP_TO_ELEVATOR
}

public static class SegmentConnectionTypeExtensions
{
    public static bool RequiresUpRoom(this SegmentConnectionType connectionType)
    {
        return connectionType switch
        {
            SegmentConnectionType.ELEVATOR_UP => true,
            SegmentConnectionType.BOTH_ELEVATORS => true,
            SegmentConnectionType.DROP_COLUMN => true,
            SegmentConnectionType.DROP_TO_ELEVATOR => true,
            SegmentConnectionType.DROP_INTO => true,
            _ => false
        };
    }

    public static bool RequiresDownRoom(this SegmentConnectionType connectionType)
    {
        return connectionType switch
        {
            SegmentConnectionType.ELEVATOR_DOWN => true,
            SegmentConnectionType.BOTH_ELEVATORS => true,
            SegmentConnectionType.DROP_COLUMN => true,
            SegmentConnectionType.DROP_TO_ELEVATOR => true,
            SegmentConnectionType.DROP_OUT => true,
            _ => false
        };
    }
}