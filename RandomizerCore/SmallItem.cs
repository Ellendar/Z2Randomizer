using System;

namespace Z2Randomizer.Core
{
    //private readonly int[] drops = { 0x8a, 0x8b, 0x8c, 0x8d, 0x90, 0x91, 0x92, 0x88 }
    public enum SmallItem
    {
        BLUE_JAR = 0x90, 
        RED_JAR = 0x91, 
        SMALL_BAG = 0x8a, 
        MEDIUM_BAG = 0x8b, 
        LARGE_BAG = 0x8c, 
        XL_BAG = 0x8d, 
        ONE_UP = 0x92, 
        KEY = 0x88
    }

    static class SmallItemExtensions
    {
        public static SmallItem Random(this SmallItem s, Random random)
        {
            return random.Next(8) switch
            {
                0 => SmallItem.BLUE_JAR,
                1 => SmallItem.RED_JAR,
                2 => SmallItem.SMALL_BAG,
                3 => SmallItem.MEDIUM_BAG,
                4 => SmallItem.LARGE_BAG,
                5 => SmallItem.XL_BAG,
                6 => SmallItem.ONE_UP,
                7 => SmallItem.KEY,
                _ => throw new ArgumentException("Invalid smallItem")
            };
        }
    }
}