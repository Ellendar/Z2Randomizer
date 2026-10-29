using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class CalderaCavePlacementTests
{
    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(false, 1)]
    [DataRow(true, 0)]
    [DataRow(true, 1)]
    public void PassagesKeepOrderedEntranceRolesInAllFourDirections(bool horizontal, int side)
    {
        var (world, center, left, right, outward) = Fixture(horizontal, side, depth: 7);
        var requirements = left.AccessRequirements;

        world.PlaceCalderaPassage(horizontal, side, center, left, right).Should().BeTrue();

        var inner = side == 0 ? right : left;
        var outer = side == 0 ? left : right;
        IntVector2.Dot(inner.Pos - center, outward).Should().Be(4);
        IntVector2.Dot(outer.Pos - center, outward).Should().Be(10);
        world.map[inner.Pos].Should().Be(Terrain.CAVE);
        world.map[outer.Pos].Should().Be(Terrain.CAVE);
        left.CanShuffle.Should().BeTrue("caldera callers own the scheduling flags");
        right.CanShuffle.Should().BeTrue();
        left.AccessRequirements.Should().BeSameAs(requirements);
    }

    [TestMethod]
    [DataRow(false, 0, 0)]
    [DataRow(false, 1, 2)]
    [DataRow(true, 0, 2)]
    [DataRow(true, 1, 0)]
    public void MissingOrThinMountainRunsFailWithoutPaintingOrMovingEntrances(bool horizontal, int side, int depth)
    {
        var (world, center, left, right, _) = Fixture(horizontal, side, depth);
        var before = Tiles(world);
        var positions = new[] { left.Pos, right.Pos };

        world.PlaceCalderaPassage(horizontal, side, center, left, right).Should().BeFalse();

        Tiles(world).Should().Equal(before);
        new[] { left.Pos, right.Pos }.Should().Equal(positions);
        left.CanShuffle.Should().BeTrue();
        right.CanShuffle.Should().BeTrue();
    }

    private static (TestWorld, IntVector2, Location, Location, IntVector2) Fixture(bool horizontal, int side, int depth)
    {
        var world = new TestWorld(37, 43, Terrain.GRASS);
        var center = new IntVector2(21, 18);
        var axis = horizontal ? IntVector2.EAST : IntVector2.SOUTH;
        var outward = side == 0 ? -axis : axis;
        var across = axis.Perpendicular();
        for (int distance = 4; distance < 4 + depth; distance++)
            for (int lateral = -5; lateral <= 5; lateral++) world.map[center + distance * outward + lateral * across] = Terrain.MOUNTAIN;
        var left = new Location(LocationID.WEST_DM_ENTRANCE, 31, 1, 0, Continent.WEST)
        {
            AccessRequirements = new Requirements([RequirementType.FAIRY])
        };
        var right = new Location(LocationID.WEST_DM_EXIT, 31, 2, 0, Continent.WEST);
        return (world, center, left, right, outward);
    }

    private static Terrain[] Tiles(TestWorld world) =>
        (from y in Enumerable.Range(0, world.MapRows)
         from x in Enumerable.Range(0, world.MapColumns) select world.map[y, x]).ToArray();
}
