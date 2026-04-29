using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Tests;

[TestClass]
public class ShuffleBagTests
{
    [TestMethod]
    public void Draw_DoesNotRepeatBeforePoolExhausted()
    {
        ShuffleBag<string> bag = new(["rauru", "ruto", "mido"], new Random(12345));

        List<string> firstRound = Enumerable.Range(0, 3).Select(_ => bag.Draw()).ToList();
        List<string> secondRound = Enumerable.Range(0, 3).Select(_ => bag.Draw()).ToList();

        firstRound.Should().OnlyHaveUniqueItems();
        firstRound.Should().BeEquivalentTo(["rauru", "ruto", "mido"]);
        secondRound.Should().OnlyHaveUniqueItems();
        secondRound.Should().BeEquivalentTo(["rauru", "ruto", "mido"]);
    }

    [TestMethod]
    public void Draw_IsDeterministicForSeed()
    {
        ShuffleBag<int> firstBag = new([1, 2, 3, 4], new Random(11235813));
        ShuffleBag<int> secondBag = new([1, 2, 3, 4], new Random(11235813));

        List<int> firstDraws = Enumerable.Range(0, 10).Select(_ => firstBag.Draw()).ToList();
        List<int> secondDraws = Enumerable.Range(0, 10).Select(_ => secondBag.Draw()).ToList();

        firstDraws.Should().Equal(secondDraws);
    }
}
