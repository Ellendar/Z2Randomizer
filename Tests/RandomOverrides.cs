using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.Tests;

internal class MockRandomMinRoll : IRandom
{
    public int Next(int minValue, int maxValue)
    {
        return minValue;
    }
}

internal class MockRandomMaxRoll : IRandom
{
    public int Next(int minValue, int maxValue)
    {
        return maxValue;
    }
}
