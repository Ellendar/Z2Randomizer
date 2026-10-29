using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Random = Z2Randomizer.RandomizerCore.Random;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class TerrainLayoutTests
{
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(-1, 0)]
    [DataRow(0, 1)]
    [DataRow(0, -1)]
    public void CaveEntranceClosesThreeSidesAndPreservesTheApproach(int dx, int dy)
    {
        var map = new OverworldMap(7, 7, Terrain.LAVA);
        var position = new IntVector2(3, 3);
        var intoMountain = new IntVector2(dx, dy);
        var side = dx == 0 ? IntVector2.EAST : IntVector2.SOUTH;

        TerrainLayout.DrawCaveEntrance(map, position, intoMountain);

        map[position].Should().Be(Terrain.CAVE);
        map[position - intoMountain].Should().Be(Terrain.LAVA);
        map[position + intoMountain].Should().Be(Terrain.MOUNTAIN);
        map[position + side].Should().Be(Terrain.MOUNTAIN);
        map[position - side].Should().Be(Terrain.MOUNTAIN);
        for (int y = 0; y < 7; y++)
            for (int x = 0; x < 7; x++)
                if ((new IntVector2(x, y) - position).ManhattanLength > 1)
                    map[y, x].Should().Be(Terrain.LAVA);
    }

    [TestMethod]
    public void Surroundings_OverwriteOnlyTheEightNeighborsAndRetainTheCenter()
    {
        var map = new OverworldMap(5, 5, Terrain.LAVA);
        map[2, 2] = Terrain.PALACE;

        TerrainLayout.PaintSurroundings(map, new(2, 2), Terrain.FOREST);

        map[2, 2].Should().Be(Terrain.PALACE);
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
                if (x == 2 && y == 2) continue;
                else map[y, x].Should().Be(x >= 1 && x <= 3 && y >= 1 && y <= 3 ? Terrain.FOREST : Terrain.LAVA);
    }

    [TestMethod]
    [DataRow(false, Terrain.WATER)]
    [DataRow(true, Terrain.WATER)]
    [DataRow(false, Terrain.PREPLACED_WATER_WALKABLE)]
    [DataRow(true, Terrain.PREPLACED_WATER_WALKABLE)]
    [DataRow(false, Terrain.DESERT)]
    [DataRow(true, Terrain.DESERT)]
    public void Canyon_SeedsABoundedBandAndAllowedBanksInEitherOrientation(bool horizontal, Terrain river)
    {
        var world = PaddedWorld(65, 70, Terrain.NONE);

        world.SeedCanyon(river, BankClimate(), new[] { Terrain.GRASS }, horizontal);

        int length = horizontal ? 70 : 65;
        int width = horizontal ? 65 : 70;
        for (int forward = 0; forward < length; forward++)
        {
            var band = Enumerable.Range(0, width).Where(side => world.map[Position(forward, side, horizontal)] == river).ToArray();
            band.Length.Should().BeInRange(2, 4);
            band.Should().OnlyContain(side => side >= 15 && side <= width - 15);
            (band[^1] - band[0] + 1).Should().Be(band.Length, "each cross-section is contiguous");
        }
        var logical = LogicalCells(world);
        logical.Should().Contain(Terrain.GRASS).And.Contain(Terrain.MOUNTAIN);
        logical.Should().OnlyContain(t => t == Terrain.NONE || t == river || t == Terrain.GRASS || t == Terrain.MOUNTAIN);
        AssertPadding(world);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Canyon_PaintsOverExistingTerrainRatherThanOnlySeedingEmptyCells(bool horizontal)
    {
        var world = PaddedWorld(65, 70, Terrain.PALACE);

        world.SeedCanyon(Terrain.WATER, BankClimate(), new[] { Terrain.GRASS }, horizontal);

        var cells = LogicalCells(world);
        cells.Should().Contain(Terrain.WATER).And.Contain(Terrain.MOUNTAIN).And.Contain(Terrain.GRASS);
        cells.Should().Contain(Terrain.PALACE, "terrain outside the painted geometry is retained");
        AssertPadding(world);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Canyon_RepeatedInputsReproduceGeometryWithoutChangingClimate(bool horizontal)
    {
        var first = new TestWorld(65, 70, Terrain.NONE);
        var second = new TestWorld(65, 70, Terrain.NONE);
        var climate = BankClimate();
        var coefficients = climate.DistanceCoefficients.ToArray();

        first.SeedCanyon(Terrain.WATER, climate, new[] { Terrain.GRASS }, horizontal);
        second.SeedCanyon(Terrain.WATER, climate, new[] { Terrain.GRASS }, horizontal);

        LogicalCells(first).Should().Equal(LogicalCells(second));
        climate.DistanceCoefficients.Should().Equal(coefficients);
        climate.SeedTerrainCount.Should().Be(10);
    }

    [TestMethod]
    [DataRow(false, 64, 75)]
    [DataRow(true, 75, 64)]
    [DataRow(false, 20, 35)]
    [DataRow(true, 35, 20)]
    public void CentralMountain_PreservesItsProfileAndUntouchedTerrain(bool horizontal, int rows, int columns)
    {
        var world = PaddedWorld(rows, columns, Terrain.GRASS);
        // Central mountains use the opposite longitudinal orientation to canyons.
        bool alongX = !horizontal;
        int length = horizontal ? rows : columns;
        int center = (horizontal ? columns : rows) / 2;
        int start = (length - 35) / 2;

        world.SeedCentralMountain(horizontal);

        world.map[Position(start, center - 3, alongX)].Should().Be(Terrain.MOUNTAIN);
        world.map[Position(start, center + 2, alongX)].Should().Be(Terrain.MOUNTAIN);
        world.map[Position(start, center + 3, alongX)].Should().Be(Terrain.GRASS);
        world.map[Position(start + 8, center - 10, alongX)].Should().Be(Terrain.MOUNTAIN);
        world.map[Position(start + 26, center + 9, alongX)].Should().Be(Terrain.MOUNTAIN);
        world.map[Position(start + 34, center + 1, alongX)].Should().Be(Terrain.MOUNTAIN);
        world.map[Position(start + 34, center + 2, alongX)].Should().Be(Terrain.GRASS);
        if (start > 0)
        {
            world.map[Position(0, 0, alongX)].Should().Be(Terrain.MOUNTAIN);
            int end = length - start;
            world.map[Position(end, center, alongX)].Should().Be(Terrain.GRASS,
                "the existing far end of the caldera zone is inclusive");
            world.map[Position(end + 1, center, alongX)].Should().Be(Terrain.MOUNTAIN);
        }
        AssertPadding(world);
        world.NextRandom().Should().Be(new Random(42).Next(), "central geometry is deterministic without RNG draws");
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void CentralMountain_UnsupportedDimensionsRetainTheExistingFailure(bool horizontal)
    {
        var world = new TestWorld(10, 10, Terrain.NONE);

        Action draw = () => world.SeedCentralMountain(horizontal);

        draw.Should().Throw<IndexOutOfRangeException>();
    }

    [TestMethod]
    public void Stripes_IgnoreBackingPaddingAndPreserveReservedFeatures()
    {
        var world = PaddedWorld(31, 37, Terrain.NONE);
        for (int x = 0; x < 37; x++) world.map[15, x] = Terrain.PALACE;

        world.SeedStripes(Terrain.MOUNTAIN, 10, 11);

        LogicalCells(world).Should().Contain(Terrain.MOUNTAIN);
        for (int x = 0; x < 37; x++) world.map[15, x].Should().Be(Terrain.PALACE);
        AssertPadding(world);
    }

    [TestMethod]
    public void ServiceBorderAndStripes_ComposeWithoutWorldAndPreserveFeaturesAndPadding()
    {
        var map = new OverworldMap(33, 40, Terrain.LAVA);
        for (int y = 0; y < 31; y++)
            for (int x = 0; x < 37; x++) map[y, x] = Terrain.NONE;
        map[15, 18] = Terrain.PALACE;

        TerrainLayout.PaintBorder(map, 31, 37, Terrain.WATER);
        TerrainLayout.DrawStripes(map, 31, 37, Terrain.MOUNTAIN, 10, 11, new Random(42));

        map[15, 18].Should().Be(Terrain.PALACE);
        bool hasStripe = false;
        for (int y = 0; y < 33; y++)
            for (int x = 0; x < 40; x++)
                if (y >= 31 || x >= 37) map[y, x].Should().Be(Terrain.LAVA);
                else if (y == 0 || y == 30 || x == 0 || x == 36) map[y, x].Should().Be(Terrain.WATER);
                else if (map[y, x] == Terrain.MOUNTAIN) hasStripe = true;
        hasStripe.Should().BeTrue();
    }

    [TestMethod]
    public void ServiceCentralMountain_ExplicitOrientationTransposesItsGeometry()
    {
        var original = new OverworldMap(64, 75, Terrain.GRASS);
        var transposed = new OverworldMap(75, 64, Terrain.GRASS);

        TerrainLayout.DrawCentralMountain(original, 64, 75, horizontal: false);
        TerrainLayout.DrawCentralMountain(transposed, 75, 64, horizontal: true);

        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 75; x++) original[y, x].Should().Be(transposed[x, y]);
    }

    private static IntVector2 Position(int forward, int side, bool alongX)
        => alongX ? new(forward, side) : new(side, forward);

    private static Climate BankClimate()
        => new("Canyon fixture", new() { [Terrain.GRASS] = 1, [Terrain.FOREST] = 1 },
            new TableWeightedRandom<Terrain>(new[] { (Terrain.GRASS, 1), (Terrain.FOREST, 3) }), 10);

    private static TestWorld PaddedWorld(int rows, int columns, Terrain fill)
    {
        var world = new TestWorld(rows, columns);
        world.map = new OverworldMap(rows + 2, columns + 3, Terrain.LAVA);
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++) world.map[y, x] = fill;
        return world;
    }

    private static Terrain[] LogicalCells(TestWorld world)
        => (from y in Enumerable.Range(0, world.MapRows)
            from x in Enumerable.Range(0, world.MapColumns)
            select world.map[y, x]).ToArray();

    private static void AssertPadding(TestWorld world)
    {
        for (int y = 0; y < world.map.GetLength(0); y++)
            for (int x = 0; x < world.map.GetLength(1); x++)
                if (y >= world.MapRows || x >= world.MapColumns) world.map[y, x].Should().Be(Terrain.LAVA);
    }
}
