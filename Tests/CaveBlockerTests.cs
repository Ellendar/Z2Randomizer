using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class CaveBlockerTests
{
    [TestMethod]
    [DataRow(Direction.NORTH, Terrain.ROCK)]
    [DataRow(Direction.SOUTH, Terrain.ROCK)]
    [DataRow(Direction.EAST, Terrain.ROCK)]
    [DataRow(Direction.WEST, Terrain.ROCK)]
    [DataRow(Direction.NORTH, Terrain.RIVER_DEVIL)]
    [DataRow(Direction.SOUTH, Terrain.RIVER_DEVIL)]
    [DataRow(Direction.EAST, Terrain.RIVER_DEVIL)]
    [DataRow(Direction.WEST, Terrain.RIVER_DEVIL)]
    public void BlockerMovesTheCaveIntoTheMountainAndPaintsOnlyItsFourTileFootprint(Direction direction, Terrain blocker)
    {
        var (world, cave, inward) = Fixture(direction);
        var origin = cave.Pos;
        world.map[origin + 2 * inward] = Terrain.DESERT;
        var before = Tiles(world);
        var metadata = (cave.Map, cave.MapPageRaw, cave.EntranceNumber, cave.AccessRequirements, cave.CanShuffle, cave.Reachable);

        world.ValidBlockingCavePosition(origin).Should().Be(inward);
        world.PlaceCaveBlocker(cave, inward, blocker);

        cave.Pos.Should().Be(origin + inward);
        world.map[origin - inward].Should().Be(blocker);
        world.map[origin].Should().Be(Terrain.ROAD);
        world.map[cave.Pos].Should().Be(Terrain.CAVE);
        world.map[origin + 2 * inward].Should().Be(Terrain.MOUNTAIN);
        Cells(world).Where(p => world.map[p] != before[p.Y * world.MapColumns + p.X]).Should()
            .BeEquivalentTo(new[] { origin - inward, origin, origin + inward, origin + 2 * inward });
        (cave.Map, cave.MapPageRaw, cave.EntranceNumber, cave.AccessRequirements, cave.CanShuffle, cave.Reachable).Should().Be(metadata);
    }

    [TestMethod]
    [DataRow(-2, 0)]
    [DataRow(2, 0)]
    [DataRow(3, -1)]
    [DataRow(3, 0)]
    [DataRow(3, 1)]
    [DataRow(1, 0)]
    [DataRow(-1, 0)]
    public void OccupiedFootprintOrNearbyFeatureRejectsWithoutPainting(int dx, int dy)
    {
        var (world, cave, _) = Fixture(Direction.EAST);
        var origin = cave.Pos;
        world.AddLocation(LocationAt(LocationID.WEST_GRASS, origin + new IntVector2(dx, dy)));
        var before = Tiles(world);

        world.ValidBlockingCavePosition(origin).Should().BeNull();

        Tiles(world).Should().Equal(before);
        cave.Pos.Should().Be(origin);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    public void CaveNeedsExactlyOneOpenSide(int openSides)
    {
        var (world, cave, _) = Fixture(Direction.EAST);
        world.map[cave.Pos - IntVector2.EAST] = Terrain.MOUNTAIN;
        foreach (var direction in IntVector2.CARDINALS.Take(openSides)) world.map[cave.Pos + direction] = Terrain.GRASS;

        world.ValidBlockingCavePosition(cave.Pos).Should().BeNull();
    }

    [TestMethod]
    public void MapBorderIsRejectedWithoutReadingOutsideTheMap()
    {
        var (world, cave, _) = Fixture(Direction.EAST);
        cave.Pos = new(0, 5);
        var before = Tiles(world);

        world.ValidBlockingCavePosition(cave.Pos).Should().BeNull();

        Tiles(world).Should().Equal(before);
    }

    private static (TestWorld, Location, IntVector2) Fixture(Direction direction)
    {
        var world = new TestWorld(25, 27, Terrain.MOUNTAIN);
        var cave = LocationAt(LocationID.WEST_DM_ENTRANCE, new(12, 12));
        cave.TerrainType = Terrain.CAVE;
        cave.AccessRequirements = new Requirements([RequirementType.FAIRY]);
        cave.CanShuffle = true;
        cave.Reachable = true;
        world.AddLocation(cave);
        var inward = direction.ToIntVector2();
        world.map[cave.Pos] = Terrain.CAVE;
        world.map[cave.Pos - inward] = world.map[cave.Pos - 2 * inward] = Terrain.GRASS;
        return (world, cave, inward);
    }

    private static Location LocationAt(LocationID id, IntVector2 position) =>
        new(id, 42, 12, 0, Continent.WEST) { Pos = position };

    private static IEnumerable<IntVector2> Cells(World world) =>
        from y in Enumerable.Range(0, world.MapRows)
        from x in Enumerable.Range(0, world.MapColumns) select new IntVector2(x, y);

    private static Terrain[] Tiles(World world) => Cells(world).Select(p => world.map[p]).ToArray();
}
