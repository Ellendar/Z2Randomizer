using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Z2Randomizer.Tests;

[TestClass]
public class CalderaTerrainTests
{
    private static readonly IntVector2 Center = new(23, 21);

    [TestMethod]
    [DataRow(Direction.WEST, 3, 4, 3, 3, 3, 5)]
    [DataRow(Direction.EAST, 5, 4, 5, 3, 5, 5)]
    [DataRow(Direction.NORTH, 4, 3, 3, 3, 5, 3)]
    [DataRow(Direction.SOUTH, 4, 5, 3, 5, 5, 5)]
    public void CaveApproachPaintsOnlyTheThreeTilesOutsideTheEntrance(Direction outward,
        int x1, int y1, int x2, int y2, int x3, int y3)
    {
        var map = new OverworldMap(9, 9, Terrain.MOUNTAIN);
        var cave = new IntVector2(4, 4);
        map[cave] = Terrain.CAVE;

        CalderaTerrain.PaintCaveApproach(map, cave, outward.ToIntVector2(), Terrain.DESERT);

        var painted = from y in Enumerable.Range(0, 9)
                      from x in Enumerable.Range(0, 9)
                      where map[y, x] == Terrain.DESERT select new IntVector2(x, y);
        painted.Should().BeEquivalentTo(new[] { new IntVector2(x1, y1), new(x2, y2), new(x3, y3) });
        map[cave].Should().Be(Terrain.CAVE);
        (from y in Enumerable.Range(0, 9) from x in Enumerable.Range(0, 9)
         where map[y, x] == Terrain.MOUNTAIN select 1).Should().HaveCount(77);
    }

    [TestMethod]
    public void CenterSearchFindsTheOnlyAreaWithEnoughMountainClearance()
    {
        for (int seed = 0; seed < 16; seed++)
        {
            var map = MountainArea();
            CalderaTerrain.TryChooseCenter(map, new Random(seed), Center - new IntVector2(1, 1),
                Center + new IntVector2(2, 2), out var selected).Should().BeTrue();
            selected.Should().Be(Center);
            map.AllTerrainInSquareEquals(Center, Terrain.MOUNTAIN, 7).Should().BeTrue();
            map[0, 0].Should().Be(Terrain.GRASS);
        }
    }

    [TestMethod]
    public void CenterSearchKeepsBothCoordinatesInsideTheSuppliedExclusiveBounds()
    {
        var map = new OverworldMap(43, 47, Terrain.MOUNTAIN);
        for (int seed = 0; seed < 32; seed++)
        {
            CalderaTerrain.TryChooseCenter(map, new Random(seed), new(10, 12), new(14, 15), out var selected)
                .Should().BeTrue();
            selected.X.Should().BeInRange(10, 13);
            selected.Y.Should().BeInRange(12, 14);
        }
    }

    [TestMethod]
    [DataRow(999, true)]
    [DataRow(1000, true)]
    [DataRow(1001, false)]
    public void CenterSearchAcceptsTheFinalAllowedSampleButStopsAfterTheBudget(int firstFittingSample, bool accepted)
    {
        var rng = new LateFittingCenter(firstFittingSample);
        var map = MountainArea();
        CalderaTerrain.TryChooseCenter(map, rng, Center - new IntVector2(1, 0), Center + new IntVector2(1, 1), out var selected)
            .Should().Be(accepted);
        rng.Draws.Should().Be(2 * (accepted ? firstFittingSample : 1000));
        selected.Should().Be(firstFittingSample <= 1000 ? Center : Center - IntVector2.EAST);
        map.AllTerrainInSquareEquals(Center, Terrain.MOUNTAIN, 7).Should().BeTrue();
    }

    // Force a late fit to characterize the retry boundary without relying on a
    // seed accidentally hitting its only valid center on one particular sample.
    private sealed class LateFittingCenter(int firstFittingSample) : IRandom
    {
        internal int Draws { get; private set; }
        public int Next(int minimum, int maximum)
        {
            int sample = Draws / 2 + 1;
            bool x = Draws++ % 2 == 0;
            int value = x ? Center.X - (sample < firstFittingSample ? 1 : 0) : Center.Y;
            value.Should().BeInRange(minimum, maximum - 1);
            return value;
        }
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(-7, 0)]
    [DataRow(7, -7)]
    public void CalderaRequiresMountainThroughoutItsOpeningWithoutChangingObstructions(int dx, int dy)
    {
        var map = MountainArea();
        map.AllTerrainInSquareEquals(Center, Terrain.MOUNTAIN, radius: 7).Should().BeTrue();
        var obstruction = Center + new IntVector2(dx, dy);
        map[obstruction] = Terrain.CAVE;
        map.AllTerrainInSquareEquals(Center, Terrain.MOUNTAIN, radius: 7).Should().BeFalse();
        map[obstruction].Should().Be(Terrain.CAVE);
        map[0, 0].Should().Be(Terrain.GRASS);
    }

    [TestMethod]
    [DataRow(false, Terrain.WATER)]
    [DataRow(true, Terrain.WATER)]
    [DataRow(false, Terrain.WALKABLEWATER)]
    [DataRow(true, Terrain.WALKABLEWATER)]
    public void LakeAndForestBanksStayInsideTheChosenMountainArea(bool horizontal, Terrain water)
    {
        for (int seed = 0; seed < 16; seed++)
        {
            var map = MountainArea();
            CalderaTerrain.DrawLake(map, Center, horizontal, water, new Random(seed));
            int waterTiles = 0, forestTiles = 0;
            for (int y = 0; y < map.GetLength(0); y++)
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if (Math.Abs(x - Center.X) > 7 || Math.Abs(y - Center.Y) > 7)
                        map[y, x].Should().Be(Terrain.GRASS, "painting must preserve terrain outside the selected opening");
                    else
                        map[y, x].Should().BeOneOf(Terrain.MOUNTAIN, Terrain.FOREST, water);
                    if (map[y, x] == water) waterTiles++;
                    if (map[y, x] == Terrain.FOREST) forestTiles++;
                }
            waterTiles.Should().BePositive();
            forestTiles.Should().BePositive();
        }
    }

    [TestMethod]
    [DataRow(Terrain.WATER)]
    [DataRow(Terrain.WALKABLEWATER)]
    [DataRow(Terrain.PREPLACED_WATER)]
    [DataRow(Terrain.PREPLACED_WATER_WALKABLE)]
    public void SwappingAxesPreservesTheFinishedLakeAndRandomState(Terrain water)
    {
        for (int seed = 0; seed < 128; seed++)
        {
            var horizontal = MountainArea();
            var vertical = Transpose(horizontal);
            var horizontalRng = new Random(seed);
            var verticalRng = new Random(seed);

            CalderaTerrain.DrawLake(horizontal, Center, true, water, horizontalRng);
            CalderaTerrain.DrawLake(vertical, new(Center.Y, Center.X), false, water, verticalRng);

            var differences = new List<IntVector2>();
            for (int y = 0; y < horizontal.GetLength(0); y++)
                for (int x = 0; x < horizontal.GetLength(1); x++)
                    if (horizontal[y, x] != vertical[x, y]) differences.Add(new(x, y));
            differences.Should().BeEmpty($"axis exchange should preserve the finished lake for seed {seed}");
            verticalRng.GetState().Should().Be(horizontalRng.GetState());
        }
    }

    [TestMethod]
    [DataRow(3, 7)]
    [DataRow(3, 8)]
    [DataRow(3, 9)]
    [DataRow(3, 10)]
    [DataRow(4, 7)]
    [DataRow(4, 8)]
    [DataRow(4, 9)]
    [DataRow(4, 10)]
    [DataRow(5, 7)]
    [DataRow(5, 8)]
    [DataRow(5, 9)]
    [DataRow(5, 10)]
    public void LakeEndJoinsItsWiderNeighborEquallyOnEitherAxis(int endWidth, int interiorWidth)
    {
        var horizontal = MountainArea();
        var vertical = Transpose(horizontal);

        CalderaTerrain.DrawLake(horizontal, Center, true, Terrain.WATER, new LakeWidths(endWidth, interiorWidth));
        CalderaTerrain.DrawLake(vertical, new(Center.Y, Center.X), false, Terrain.WATER, new LakeWidths(endWidth, interiorWidth));

        for (int y = 0; y < horizontal.GetLength(0); y++)
            for (int x = 0; x < horizontal.GetLength(1); x++)
                horizontal[y, x].Should().Be(vertical[x, y], $"the lake and banks must match at ({x}, {y}) after swapping axes");
    }

    // Controls the shape choices, without coupling the test to how many times
    // each choice is sampled or to a particular PRNG seed hitting every width.
    private sealed class LakeWidths(int endWidth, int interiorWidth) : IRandom
    {
        public int Next(int minimum, int maximum) => (minimum, maximum) switch
        {
            (3, 6) => endWidth,
            (7, 11) => interiorWidth,
            _ => throw new InvalidOperationException($"Unexpected lake width range [{minimum}, {maximum}).")
        };
    }

    private static OverworldMap Transpose(OverworldMap source)
    {
        var result = new OverworldMap(source.GetLength(1), source.GetLength(0));
        for (int y = 0; y < source.GetLength(0); y++)
            for (int x = 0; x < source.GetLength(1); x++) result[x, y] = source[y, x];
        return result;
    }

    private static OverworldMap MountainArea()
    {
        var map = new OverworldMap(43, 47, Terrain.GRASS);
        for (int y = Center.Y - 7; y <= Center.Y + 7; y++)
            for (int x = Center.X - 7; x <= Center.X + 7; x++)
                map[y, x] = Terrain.MOUNTAIN;
        return map;
    }
}
