using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class OceanLayoutTests
{
    [TestMethod]
    [DataRow(Direction.WEST, Terrain.PREPLACED_WATER)]
    [DataRow(Direction.EAST, Terrain.PREPLACED_WATER_WALKABLE)]
    [DataRow(Direction.NORTH, Terrain.PREPLACED_WATER)]
    [DataRow(Direction.SOUTH, Terrain.PREPLACED_WATER_WALKABLE)]
    public void Ocean_PaintsOnlyAContiguousPartOfTheSelectedLogicalEdge(Direction edge, Terrain water)
    {
        var world = Fixture(42);

        world.SeedOcean(edge, water);

        var painted = Positions(world).Where(p => world.map[p] == water).ToArray();
        painted.Should().NotBeEmpty();
        foreach (var pos in painted)
            OnEdge(pos, edge, world.MapRows, world.MapColumns).Should().BeTrue();
        var coordinates = painted.Select(p => edge is Direction.EAST or Direction.WEST ? p.Y : p.X).Order().ToArray();
        coordinates.Should().Equal(Enumerable.Range(coordinates[0], coordinates.Length));
        Positions(world).Select(p => world.map[p]).Should().OnlyContain(t => t == Terrain.NONE || t == water);
        AssertPadding(world);
    }

    [TestMethod]
    [DataRow(Direction.WEST)]
    [DataRow(Direction.EAST)]
    [DataRow(Direction.NORTH)]
    [DataRow(Direction.SOUTH)]
    public void Ocean_ObstructionRejectsWithoutOverwritingOccupiedShore(Direction edge)
    {
        var world = Fixture(42, Terrain.PALACE);

        world.SeedOcean(edge, Terrain.PREPLACED_WATER).Should().BeFalse();

        Positions(world).Select(p => world.map[p]).Should().OnlyContain(t => t == Terrain.PALACE);
        AssertPadding(world);
    }

    [TestMethod]
    [DataRow(Biome.MOUNTAINOUS, Direction.WEST)]
    [DataRow(Biome.MOUNTAINOUS, Direction.EAST)]
    [DataRow(Biome.MOUNTAINOUS, Direction.NORTH)]
    [DataRow(Biome.MOUNTAINOUS, Direction.SOUTH)]
    [DataRow(Biome.CALDERA, Direction.WEST)]
    [DataRow(Biome.CALDERA, Direction.EAST)]
    [DataRow(Biome.CALDERA, Direction.NORTH)]
    [DataRow(Biome.CALDERA, Direction.SOUTH)]
    public void Ocean_MountainousAndCalderaMayOpenOccupiedShore(Biome biome, Direction edge)
    {
        var world = Fixture(42, Terrain.MOUNTAIN, biome);

        world.SeedOcean(edge, Terrain.PREPLACED_WATER).Should().BeTrue();

        var painted = Positions(world).Where(p => world.map[p] == Terrain.PREPLACED_WATER).ToArray();
        painted.Should().NotBeEmpty();
        foreach (var pos in painted) OnEdge(pos, edge, world.MapRows, world.MapColumns).Should().BeTrue();
        Positions(world).Select(p => world.map[p]).Should().Contain(Terrain.MOUNTAIN);
        AssertPadding(world);
    }

    [TestMethod]
    public void Ocean_RepeatInputsReproduceThePaintingAndRandomState()
    {
        var first = Fixture(42);
        var second = Fixture(42);

        first.SeedOcean(Direction.WEST, Terrain.PREPLACED_WATER)
            .Should().Be(second.SeedOcean(Direction.WEST, Terrain.PREPLACED_WATER));

        Positions(first).Select(p => first.map[p]).Should().Equal(Positions(second).Select(p => second.map[p]));
        first.NextRandom().Should().Be(second.NextRandom());
    }

    [TestMethod]
    public void Ocean_UnrecognizedDirectionRetainsSouthFallback()
    {
        var invalid = Fixture(42);
        var south = Fixture(42);

        invalid.SeedOcean(Direction.NONE, Terrain.PREPLACED_WATER)
            .Should().Be(south.SeedOcean(Direction.SOUTH, Terrain.PREPLACED_WATER));

        Positions(invalid).Select(p => invalid.map[p]).Should().Equal(Positions(south).Select(p => south.map[p]));
        invalid.NextRandom().Should().Be(south.NextRandom());
    }

    [TestMethod]
    [DataRow(Direction.WEST)]
    [DataRow(Direction.EAST)]
    public void Ocean_BlockedShoreRetainsEarlierPaintingAndAdvancesRandomState(Direction edge)
    {
        var reference = Fixture(42);
        reference.SeedOcean(edge, Terrain.PREPLACED_WATER).Should().BeTrue();
        var footprint = Positions(reference).Where(p => reference.map[p] == Terrain.PREPLACED_WATER)
            .OrderBy(p => p.Y).ToArray();
        footprint.Length.Should().BeGreaterThan(2);
        var obstacle = footprint[footprint.Length / 2];
        var world = Fixture(42);
        world.map[obstacle] = Terrain.PALACE;

        world.SeedOcean(edge, Terrain.PREPLACED_WATER).Should().BeFalse();

        world.map[obstacle].Should().Be(Terrain.PALACE);
        int painted = Positions(world).Count(p => world.map[p] == Terrain.PREPLACED_WATER);
        painted.Should().BeInRange(1, footprint.Length - 1);
        world.NextRandom().Should().Be(reference.NextRandom(), "painting itself consumes no further random draws");
        AssertPadding(world);
    }

    [TestMethod]
    [DataRow(Direction.NORTH)]
    [DataRow(Direction.SOUTH)]
    public void Ocean_EmptyHorizontalShoreCompletesItsWestwardScan(Direction edge)
    {
        var world = Fixture(0);

        world.SeedOcean(edge, Terrain.PREPLACED_WATER).Should().BeTrue();

        var painted = Positions(world).Where(p => world.map[p] == Terrain.PREPLACED_WATER).ToArray();
        painted.Length.Should().BeGreaterThan(2, "a westward scan must not stop on water it just painted");
        foreach (var pos in painted) OnEdge(pos, edge, world.MapRows, world.MapColumns).Should().BeTrue();
        var columns = painted.Select(p => p.X).Order().ToArray();
        columns.Should().Equal(Enumerable.Range(columns[0], columns.Length));
        AssertPadding(world);
    }

    [TestMethod]
    [DataRow(Direction.NORTH, false)]
    [DataRow(Direction.NORTH, true)]
    [DataRow(Direction.SOUTH, false)]
    [DataRow(Direction.SOUTH, true)]
    public void Ocean_WestwardHorizontalScanStopsAtTheFirstOccupiedTile(Direction edge, bool atStart)
    {
        var reference = Fixture(0);
        reference.SeedOcean(edge, Terrain.PREPLACED_WATER);
        var start = Positions(reference).Where(p => reference.map[p] == Terrain.PREPLACED_WATER).MaxBy(p => p.X);
        var obstacle = atStart ? start : start + IntVector2.WEST;
        var world = Fixture(0);
        world.map[obstacle] = Terrain.PALACE;

        world.SeedOcean(edge, Terrain.PREPLACED_WATER).Should().BeFalse();

        world.map[obstacle].Should().Be(Terrain.PALACE, "overwrite permission must apply to the tile being painted");
        var painted = Positions(world).Where(p => world.map[p] == Terrain.PREPLACED_WATER).ToArray();
        if (atStart) painted.Should().BeEmpty();
        else painted.Should().Equal([start], "earlier painting remains when a later tile is blocked");
        world.NextRandom().Should().Be(reference.NextRandom(), "painting consumes no additional random draws");
        AssertPadding(world);
    }

    [TestMethod]
    public void Ocean_ZeroLengthSucceedsWithoutInspectingOccupiedShore()
    {
        var world = Fixture(16, Terrain.PALACE);

        world.SeedOcean(Direction.WEST, Terrain.PREPLACED_WATER).Should().BeTrue();

        Positions(world).Select(p => world.map[p]).Should().OnlyContain(t => t == Terrain.PALACE);
        AssertPadding(world);
    }

    [TestMethod]
    [DataRow(Direction.WEST)]
    [DataRow(Direction.EAST)]
    public void Ocean_DecreasingVerticalScanRetainsSuccessForUndersizedBackingMap(Direction edge)
    {
        var world = Fixture(0);
        world.map = new OverworldMap(4, 30, Terrain.NONE);

        world.SeedOcean(edge, Terrain.PREPLACED_WATER).Should().BeTrue(
            "the legacy decreasing vertical scan catches its backing-map bounds failure");

        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 30; x++) world.map[y, x].Should().Be(Terrain.NONE);
    }

    [TestMethod]
    [DataRow(Direction.WEST)]
    [DataRow(Direction.EAST)]
    public void Ocean_IncreasingVerticalScanPropagatesBackingMapFailureAfterEarlierPainting(Direction edge)
    {
        var world = Fixture(42);
        world.map = new OverworldMap(4, 30, Terrain.NONE);

        Action paint = () => world.SeedOcean(edge, Terrain.PREPLACED_WATER);

        paint.Should().Throw<IndexOutOfRangeException>();
        int column = edge == Direction.WEST ? 0 : 29;
        world.map[0, column].Should().Be(Terrain.NONE);
        for (int y = 1; y < 4; y++) world.map[y, column].Should().Be(Terrain.PREPLACED_WATER);
    }

    [TestMethod]
    [DataRow(Terrain.NONE, false, nameof(OceanPaintResult.Completed))]
    [DataRow(Terrain.PALACE, false, nameof(OceanPaintResult.Blocked))]
    [DataRow(Terrain.PALACE, true, nameof(OceanPaintResult.Completed))]
    public void Service_ReportsPaintingOutcomeWithExplicitOverwritePolicy(Terrain fill, bool overwrite, string expectedResult)
    {
        var expected = Enum.Parse<OceanPaintResult>(expectedResult);
        var map = new OverworldMap(23, 33, Terrain.LAVA);
        for (int y = 0; y < 21; y++)
            for (int x = 0; x < 30; x++) map[y, x] = fill;

        var result = TerrainLayout.DrawOcean(map, 21, 30, Direction.EAST,
            Terrain.PREPLACED_WATER_WALKABLE, overwrite, new Z2Randomizer.RandomizerCore.Random(42));

        result.Should().Be(expected);
        var edge = Enumerable.Range(0, 21).Select(y => map[y, 29]).ToArray();
        if (expected == OceanPaintResult.Blocked) edge.Should().OnlyContain(t => t == fill);
        else edge.Should().Contain(Terrain.PREPLACED_WATER_WALKABLE);
        for (int y = 0; y < 23; y++)
            for (int x = 0; x < 33; x++)
                if (y >= 21 || x >= 30) map[y, x].Should().Be(Terrain.LAVA);
                else if (x != 29) map[y, x].Should().Be(fill);
    }

    [TestMethod]
    public void Service_DistinguishesCaughtBackingMapFailureFromCompletion()
    {
        var map = new OverworldMap(4, 30, Terrain.NONE);

        TerrainLayout.DrawOcean(map, 21, 30, Direction.WEST, Terrain.PREPLACED_WATER,
            overwrite: false, new Z2Randomizer.RandomizerCore.Random(0))
            .Should().Be(OceanPaintResult.BackingMapBoundsFailure);

        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 30; x++) map[y, x].Should().Be(Terrain.NONE);
    }

    private static TestWorld Fixture(int seed, Terrain fill = Terrain.NONE, Biome biome = Biome.VANILLALIKE)
    {
        var world = new TestWorld(21, 30, fill) { biome = biome };
        world.map = new OverworldMap(23, 33, Terrain.LAVA);
        foreach (var pos in Positions(world)) world.map[pos] = fill;
        world.SeedRandom(seed);
        return world;
    }

    private static IEnumerable<IntVector2> Positions(TestWorld world)
        => from y in Enumerable.Range(0, world.MapRows)
           from x in Enumerable.Range(0, world.MapColumns)
           select new IntVector2(x, y);

    private static bool OnEdge(IntVector2 pos, Direction edge, int rows, int columns) => edge switch
    {
        Direction.WEST => pos.X == 0,
        Direction.EAST => pos.X == columns - 1,
        Direction.NORTH => pos.Y == 0,
        Direction.SOUTH => pos.Y == rows - 1,
        _ => false
    };

    private static void AssertPadding(TestWorld world)
    {
        for (int y = 0; y < world.map.GetLength(0); y++)
            for (int x = 0; x < world.map.GetLength(1); x++)
                if (y >= world.MapRows || x >= world.MapColumns) world.map[y, x].Should().Be(Terrain.LAVA);
    }
}
