using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class RiverLayoutTests
{
    [TestMethod]
    [DataRow(0, false, 2)]
    [DataRow(1, true, 2)]
    [DataRow(42, false, 1)]
    [DataRow(12345, true, 2)]
    public void River_PaintsAConnectedWaterwayWithoutTouchingBackingPadding(int seed, bool boots, int edgeCount)
    {
        var world = Fixture(seed);

        world.SeedRiver(boots);

        var water = boots ? Terrain.WALKABLEWATER : Terrain.WATER;
        var cells = Positions(world).Where(p => world.map[p] == water).ToHashSet();
        cells.Should().NotBeEmpty();
        new[]
        {
            cells.Any(p => p.X == 0), cells.Any(p => p.X == world.MapColumns - 1),
            cells.Any(p => p.Y == 0), cells.Any(p => p.Y == world.MapRows - 1)
        }.Count(edge => edge).Should().Be(edgeCount, "a river may turn back toward its starting edge");
        var connected = new HashSet<IntVector2> { cells.First() };
        var frontier = new Queue<IntVector2>(connected);
        while (frontier.TryDequeue(out var current))
            foreach (var direction in IntVector2.CARDINALS)
                if (cells.Contains(current + direction) && connected.Add(current + direction))
                    frontier.Enqueue(current + direction);
        connected.Should().BeEquivalentTo(cells, "the empty-map river is one cardinally connected waterway");
        Positions(world).Select(p => world.map[p]).Should().OnlyContain(t => t == Terrain.NONE || t == water);
        AssertPadding(world);
    }

    [TestMethod]
    [DataRow(Terrain.TOWN)]
    [DataRow(Terrain.CAVE)]
    [DataRow(Terrain.PALACE)]
    [DataRow(Terrain.BRIDGE)]
    [DataRow(Terrain.ROCK)]
    public void River_RetainsReservedFeaturesWhenMeandering(Terrain feature)
    {
        var world = Fixture(42);
        var occupied = Positions(world).Where(p => (p.X + p.Y) % 2 == 0).ToArray();
        foreach (var pos in occupied) world.map[pos] = feature;

        world.SeedRiver(false);

        occupied.Select(p => world.map[p]).Should().OnlyContain(t => t == feature);
        Positions(world).Select(p => world.map[p]).Should().Contain(Terrain.WATER);
        AssertPadding(world);
    }

    [TestMethod]
    public void River_CanExpandIntoOccupiedUnreservedTerrain()
    {
        var world = Fixture(42);
        var occupied = Positions(world).Where(p => (p.X + p.Y) % 2 == 0).ToArray();
        foreach (var pos in occupied) world.map[pos] = Terrain.GRASS;

        world.SeedRiver(false);

        occupied.Select(p => world.map[p]).Should().Contain(Terrain.WATER)
            .And.Contain(Terrain.GRASS, "only the river footprint is changed");
    }

    [TestMethod]
    public void River_ReservationPolicyAppliesToSidewaysExpansionRatherThanEmptyForwardCells()
    {
        var world = Fixture(42);
        world.RiverReservation = (_, _) => true;

        world.SeedRiver(false);

        Positions(world).Select(p => world.map[p]).Should().Contain(Terrain.WATER,
            "empty forward cells are painted before the sideways reservation check");
        AssertPadding(world);
    }

    [TestMethod]
    public void River_FullyOccupiedMapRemainsUnchanged()
    {
        var world = Fixture(42);
        foreach (var pos in Positions(world)) world.map[pos] = Terrain.GRASS;

        world.SeedRiver(false);

        Positions(world).Select(p => world.map[p]).Should().OnlyContain(t => t == Terrain.GRASS);
        AssertPadding(world);
    }

    [TestMethod]
    public void River_BootsChangeOnlyTheWaterTypeAndRepeatedInputsReproduceThePath()
    {
        var first = Fixture(42);
        var repeated = Fixture(42);
        var boots = Fixture(42);

        first.SeedRiver(false);
        repeated.SeedRiver(false);
        boots.SeedRiver(true);

        var positions = Positions(first).ToArray();
        positions.Select(p => first.map[p]).Should().Equal(positions.Select(p => repeated.map[p]));
        foreach (var pos in positions)
            boots.map[pos].Should().Be(first.map[pos] == Terrain.WATER ? Terrain.WALKABLEWATER : first.map[pos]);
        first.NextRandom().Should().Be(repeated.NextRandom()).And.Be(boots.NextRandom());
    }

    [TestMethod]
    [DataRow(Terrain.WATER)]
    [DataRow(Terrain.WALKABLEWATER)]
    public void Service_UsesExplicitBoundsWaterAndReservationPolicyWithoutWorld(Terrain water)
    {
        const int rows = 21, columns = 30;
        var map = new OverworldMap(rows + 2, columns + 3, Terrain.LAVA);
        var occupied = new List<IntVector2>();
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                var pos = new IntVector2(x, y);
                map[pos] = (x + y) % 2 == 0 ? Terrain.GRASS : Terrain.NONE;
                if (map[pos] == Terrain.GRASS) occupied.Add(pos);
            }
        int reservationsChecked = 0;

        TerrainLayout.DrawRiver(map, rows, columns, water, (pos, terrain) =>
        {
            pos.X.Should().BeInRange(0, columns - 1);
            pos.Y.Should().BeInRange(0, rows - 1);
            terrain.Should().Be(map[pos], "the policy receives current terrain");
            reservationsChecked++;
            return terrain == Terrain.GRASS;
        }, new Z2Randomizer.RandomizerCore.Random(42));

        reservationsChecked.Should().BeGreaterThan(0);
        occupied.Select(p => map[p]).Should().OnlyContain(t => t == Terrain.GRASS);
        (from y in Enumerable.Range(0, rows)
         from x in Enumerable.Range(0, columns)
         select map[y, x]).Should().Contain(water);
        for (int y = 0; y < map.GetLength(0); y++)
            for (int x = 0; x < map.GetLength(1); x++)
                if (y >= rows || x >= columns) map[y, x].Should().Be(Terrain.LAVA);
    }

    private static TestWorld Fixture(int seed)
    {
        var world = new TestWorld(21, 30, Terrain.NONE);
        world.map = new OverworldMap(23, 33, Terrain.LAVA);
        foreach (var pos in Positions(world)) world.map[pos] = Terrain.NONE;
        world.SeedRandom(seed);
        return world;
    }

    private static IEnumerable<IntVector2> Positions(TestWorld world)
        => from y in Enumerable.Range(0, world.MapRows)
           from x in Enumerable.Range(0, world.MapColumns)
           select new IntVector2(x, y);

    private static void AssertPadding(TestWorld world)
    {
        for (int y = 0; y < world.map.GetLength(0); y++)
            for (int x = 0; x < world.map.GetLength(1); x++)
                if (y >= world.MapRows || x >= world.MapColumns) world.map[y, x].Should().Be(Terrain.LAVA);
    }
}
