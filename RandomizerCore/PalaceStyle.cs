namespace Z2Randomizer.Core;

public enum PalaceStyle
{
    VANILLA, SHUFFLED, RECONSTRUCTED, CARTESIAN, CHAOS, RANDOM_ALL, RANDOM_PER_PALACE
}

public static class PalaceStyleExtensions
{
    public static bool UsesVanillaRoomPool(this PalaceStyle style)
    {
        return style switch
        {
            PalaceStyle.VANILLA => true,
            PalaceStyle.SHUFFLED => true,
            _ => false
        };
    }
}