using System;

namespace Z2Randomizer.RandomizerCore;

/// Subset of Collectables that can be used as enemy drops. The enum
/// values match their Collectable counterparts with 7th bit set.
public enum SmallItem : byte
{
    KEY = 0x88,
    SMALL_BAG = 0x8a,   // 50 P
    MEDIUM_BAG = 0x8b,  // 100 P
    LARGE_BAG = 0x8c,   // 200 P
    XL_BAG = 0x8d,      // 500 P
    BLUE_JAR = 0x90,
    RED_JAR = 0x91,
    ONEUP = 0x92,
}

public static class SmallItemExtensions
{
    public static Collectable ToCollectable(this SmallItem drop)
    {
        return drop switch
        {
            SmallItem.KEY => Collectable.KEY,
            SmallItem.SMALL_BAG => Collectable.SMALL_BAG,
            SmallItem.MEDIUM_BAG => Collectable.MEDIUM_BAG,
            SmallItem.LARGE_BAG => Collectable.LARGE_BAG,
            SmallItem.XL_BAG => Collectable.XL_BAG,
            SmallItem.BLUE_JAR => Collectable.BLUE_JAR,
            SmallItem.RED_JAR => Collectable.RED_JAR,
            SmallItem.ONEUP => Collectable.ONEUP,
            _ => throw new ArgumentOutOfRangeException(nameof(drop), drop, null),
        };
    }
}
