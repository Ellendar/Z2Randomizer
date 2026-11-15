using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Z2Randomizer.Tests;

[TestClass]
public class IslandStripeLayoutTests
{
    [TestMethod]
    [DataRow(Terrain.PREPLACED_WATER)]
    [DataRow(Terrain.PREPLACED_WATER_WALKABLE)]
    public void IslandStripes_PreserveExistingFeaturesBorderAndPaddingAndReproduceTheirOutput(Terrain water)
    {
        var first = Map(water);
        var second = Map(water);
        var reserved = new List<IntVector2>();
        for (int x = 1; x < 36; x++) reserved.Add(new(x, 15));
        for (int y = 1; y < 30; y++) reserved.Add(new(18, y));
        foreach (var pos in reserved) first[pos] = second[pos] = Terrain.PALACE;
        var firstRandom = new Random(42);
        var secondRandom = new Random(42);

        TerrainLayout.DrawIslandStripes(first, 31, 37, water, firstRandom);
        TerrainLayout.DrawIslandStripes(second, 31, 37, water, secondRandom);

        foreach (var pos in reserved) first[pos].Should().Be(Terrain.PALACE);
        LogicalCells(first).Should().Contain(water).And.Contain(Terrain.NONE)
            .And.OnlyContain(t => t == water || t == Terrain.NONE || t == Terrain.PALACE);
        LogicalCells(first).Should().Equal(LogicalCells(second));
        firstRandom.GetState().Should().Be(secondRandom.GetState());
        AssertBorderAndPadding(first, water);
    }

    [TestMethod]
    [DataRow(Terrain.PREPLACED_WATER)]
    [DataRow(Terrain.PREPLACED_WATER_WALKABLE)]
    public void IslandStripes_RowsWanderBeyondTheTwoOrThreeStraightStartingRows(Terrain water)
    {
        var map = Map(water);

        TerrainLayout.DrawIslandStripes(map, 31, 37, water, new Random(42));

        var nonStripeColumns = Enumerable.Range(1, 35)
            .Where(x => Enumerable.Range(1, 29).Any(y => map[y, x] == Terrain.NONE)).ToArray();
        nonStripeColumns.Should().NotBeEmpty();
        var rowFootprint = Enumerable.Range(1, 29)
            .Where(y => nonStripeColumns.Any(x => map[y, x] == water)).ToArray();
        rowFootprint.Length.Should().BeGreaterThan(3, "water crossings shift rows rather than leaving them straight");
        AssertBorderAndPadding(map, water);
    }

    [TestMethod]
    public void IslandStripes_FullyOccupiedMapRemainsUnchanged()
    {
        var map = new OverworldMap(31, 37, Terrain.PALACE);
        var rng = new Random(42);
        var before = rng.GetState();

        TerrainLayout.DrawIslandStripes(map, 31, 37, Terrain.PREPLACED_WATER, rng);

        LogicalCells(map).Should().OnlyContain(t => t == Terrain.PALACE);
        rng.GetState().Should().NotBe(before, "counts and distinct positions are still sampled");
    }

    private static OverworldMap Map(Terrain water)
    {
        var map = new OverworldMap(33, 40, Terrain.LAVA);
        for (int y = 0; y < 31; y++)
            for (int x = 0; x < 37; x++) map[y, x] = Terrain.NONE;
        TerrainLayout.PaintBorder(map, 31, 37, water);
        return map;
    }

    private static IEnumerable<Terrain> LogicalCells(OverworldMap map)
        => from y in Enumerable.Range(0, 31)
           from x in Enumerable.Range(0, 37)
           select map[y, x];

    private static void AssertBorderAndPadding(OverworldMap map, Terrain water)
    {
        for (int y = 0; y < 33; y++)
            for (int x = 0; x < 40; x++)
                if (y >= 31 || x >= 37) map[y, x].Should().Be(Terrain.LAVA);
                else if (y == 0 || y == 30 || x == 0 || x == 36) map[y, x].Should().Be(water);
    }
}
