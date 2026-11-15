using System;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public static class RoomPoolConfigs
{
    public static RoomPoolSpec Vanilla = new()
    {
        Description = "Only Vanilla rooms",
        GroupsInclude = ["VANILLA"],
    };

    public static RoomPoolSpec Upstarts = new()
    {
        Description = "All vanilla and community rooms excluding Expert-tagged rooms",
        GroupsInclude = ["VANILLA", "V4_0", "V5_0"],
        TagsExclude = ["Expert"],
    };

    public static RoomPoolSpec V4_0 = new()
    {
        Description = "Vanilla and v4.0 community rooms",
        GroupsInclude = ["VANILLA", "V4_0"],
    };

    public static RoomPoolSpec V5_0 = new()
    {
        Description = "Vanilla, v4.0 and v5.0 community rooms",
        GroupsInclude = ["VANILLA", "V4_0", "V5_0"],
    };

    public static RoomPoolSpec SGL2026 = new()
    {
        Name = "SGL 2026 Room Pool",
        Description = "Room pool for SGL 2026",
        GroupsInclude = ["VANILLA", "V4_0"],
        TagsInclude = ["SGL"],
        TagsExclude = ["NotSGL"],
        Rooms =
        [
            // Remove doors too close to the entrance.
            new RoomOverride { Name = "triforceOfCourage M5", ItemPresenceBits = 0b1110 },
            new RoomOverride { Name = "triforceOfCourage M18", ItemPresenceBits = 0b1110 },
            // Remove one of the flying enemies.
            new RoomOverride { Name = "Up elevator block columns right turn", Enemies = "073F843E44028E" },
        ],
    };

    internal static RoomPoolSpec Get(RoomPoolOption roomPoolOption)
    {
        return roomPoolOption switch
        {
            RoomPoolOption.VANILLA => Vanilla,
            RoomPoolOption.V5_0_NO_EXPERT => Upstarts,
            RoomPoolOption.V5_0 => V5_0,
            RoomPoolOption.V4_0 => V4_0,
            RoomPoolOption.SGL2026 => SGL2026,
            _ => throw new ArgumentOutOfRangeException(nameof(roomPoolOption), roomPoolOption, "Expected a built-in room pool."),
        };
    }

    internal static int PotentialRoomPools(RoomPoolOption roomPoolOption)
    {
        switch (roomPoolOption)
        {
            case RoomPoolOption.VANILLA:
                return 1;
            case RoomPoolOption.V5_0_NO_EXPERT:
            case RoomPoolOption.V5_0:
                return 3;
            case RoomPoolOption.V4_0:
                return 2;
            case RoomPoolOption.SGL2026:
                return 2;
            case RoomPoolOption.CUSTOM:
                // just leave it up to the user
                return 3;
            default:
                throw new NotImplementedException();
        }
    }

    /*
    public static RoomPoolSpec All = new()
    {
        Description = "All vanilla and community rooms are included.",
        GroupsInclude = ["VANILLA", "V4_0", "V5_0"],
        TagsInclude = ["SGL"],
    };
    */
}
