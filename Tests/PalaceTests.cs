using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.Tests;

[TestClass]
public class PalaceTests
{
    private static readonly (PalaceLengthOption Option, bool VanillaPool, int[] Min, int[] Max)[] EXPECTED_LENGTHS = [
        (PalaceLengthOption.SHORT, true,   [11, 16, 10, 19, 23, 20, 31], [12, 16, 13, 19, 23, 20, 37]),
        (PalaceLengthOption.MEDIUM, true,  [11, 16, 10, 19, 23, 20, 33], [14, 18, 14, 19, 23, 21, 45]),
        (PalaceLengthOption.FULL, true,    [14, 21, 15, 21, 28, 27, 55], [14, 21, 15, 21, 28, 27, 55]),
        (PalaceLengthOption.RANDOM, true,  [11, 16, 10, 19, 23, 20, 31], [14, 21, 15, 21, 28, 27, 55]),
        (PalaceLengthOption.SHORT, false,  [9, 10, 9, 10, 12, 12, 28],   [12, 15, 13, 15, 17, 17, 37]),
        (PalaceLengthOption.MEDIUM, false, [10, 13, 10, 13, 16, 15, 33], [14, 18, 14, 18, 22, 21, 45]),
        (PalaceLengthOption.FULL, false,   [12, 18, 13, 18, 24, 23, 47], [17, 25, 18, 25, 21, 20, 61]),
        (PalaceLengthOption.RANDOM, false, [7, 10, 8, 10, 14, 14, 28],   [17, 25, 18, 25, 21, 20, 61]),
    ];

    [TestMethod]
    public void RollPalaceLengths_AllLowSeed_MatchesExpected()
    {
        foreach ((PalaceLengthOption option, bool vanillaPool, int[] min, _) in EXPECTED_LENGTHS)
        {
            int[] sizes = Palaces.RollPalaceLengths(
                BuildConfig(option), BuildProps(vanillaPool), new MockRandomMinRoll());

            sizes.Should().Equal(min, $"{option} pool={vanillaPool}");
        }
    }

    [TestMethod]
    public void RollPalaceLengths_AllHighSeed_MatchesExpected()
    {
        foreach ((PalaceLengthOption option, bool vanillaPool, _, int[] max) in EXPECTED_LENGTHS)
        {
            int[] sizes = Palaces.RollPalaceLengths(
                BuildConfig(option), BuildProps(vanillaPool), new MockRandomMaxRoll());

            sizes.Should().Equal(max, $"{option} pool={vanillaPool}");
        }
    }

    private static RandomizerConfiguration BuildConfig(PalaceLengthOption option) => new()
    {
        NormalPalaceLength = option,
        GpLength = option
    };

    private static RandomizerProperties BuildProps(bool vanillaPool)
    {
        PalaceStyle style = vanillaPool ? PalaceStyle.VANILLA : PalaceStyle.SEQUENTIAL;
        return new RandomizerProperties
        {
            PalaceStyles = Enumerable.Range(0, 7).Select(_ => style).ToArray()
        };
    }
}
