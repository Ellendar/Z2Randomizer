namespace Z2Randomizer.Core;

public enum PalaceStyle
{
    VANILLA, SHUFFLED, RECONSTRUCTED, RANDOM, NORMALIZED, RECONSTRUCTED_SHORTENED, RECONSTRUCTED_MEDIUM, RECONSTRUCTED_RANDOM_LENGTH
}

public static class PalaceStyleExtensions
{
    public static bool IsReconstructed(this PalaceStyle style)
    {
        return style == PalaceStyle.RECONSTRUCTED 
            || style == PalaceStyle.RECONSTRUCTED_SHORTENED 
            || style == PalaceStyle.RECONSTRUCTED_RANDOM_LENGTH
            || style == PalaceStyle.RECONSTRUCTED_MEDIUM;
    }
}