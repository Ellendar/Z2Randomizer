using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.Tests;

[TestClass]
public class BasicWorldRoutingTests
{
    [TestMethod]
    public void DisconnectedTargetsRequireAllOfTheirRoots()
    {
        var map = BlockedMap(3, 5);
        var left = LocationAt(0, 1);
        var right = LocationAt(4, 1);
        var targets = new[] { left, right };

        BasicWorldRouting.CanReachRequiredLocations(map, 3, 5, [left], targets, _ => []).Should().BeFalse();
        BasicWorldRouting.CanReachRequiredLocations(map, 3, 5, [left, right], targets, _ => []).Should().BeTrue();
        targets.Should().Equal(left, right);
        left.Reachable.Should().BeFalse();
        right.Reachable.Should().BeFalse();
    }

    [TestMethod]
    public void DiagonalTerrainDoesNotConnectATarget()
    {
        var map = BlockedMap(2, 2);
        var root = LocationAt(0, 0);
        var target = LocationAt(1, 1);
        map[target.Pos] = Terrain.ROAD;

        BasicWorldRouting.CanReachRequiredLocations(map, 2, 2, [root], [target], _ => []).Should().BeFalse();
        map[0, 1] = Terrain.ROAD;
        BasicWorldRouting.CanReachRequiredLocations(map, 2, 2, [root], [target], _ => []).Should().BeTrue();
    }

    [TestMethod]
    public void SideviewExitsAreFollowedOnlyWhenTheirEntranceIsARequiredTarget()
    {
        var map = BlockedMap(1, 3);
        var entrance = LocationAt(0, 0);
        var target = LocationAt(2, 0);
        IEnumerable<Location> Exits(Location location) => location == entrance ? [target] : [];

        BasicWorldRouting.CanReachRequiredLocations(map, 1, 3, [entrance], [target], Exits).Should().BeFalse();
        BasicWorldRouting.CanReachRequiredLocations(map, 1, 3, [entrance], [entrance, target], Exits).Should().BeTrue();
    }

    [TestMethod]
    public void TraversalDoesNotUseWalkableTilesOutsideLogicalDimensions()
    {
        var map = BlockedMap(3, 4);
        var root = LocationAt(2, 0);
        var target = LocationAt(2, 2);
        map[target.Pos] = Terrain.ROAD;
        for (var y = 0; y < 3; y++) map[y, 3] = Terrain.ROAD;

        BasicWorldRouting.CanReachRequiredLocations(map, 3, 3, [root], [target], _ => []).Should().BeFalse();
        BasicWorldRouting.CanReachRequiredLocations(map, 3, 4, [root], [target], _ => []).Should().BeTrue();
    }

    [TestMethod]
    public void NoRequiredTargetsStillNeedsAValidRoot()
    {
        var map = BlockedMap(1, 1);
        BasicWorldRouting.CanReachRequiredLocations(map, 1, 1, [LocationAt(0, 0)], [], _ => []).Should().BeTrue();
        Action withoutRoots = () => BasicWorldRouting.CanReachRequiredLocations(map, 1, 1, [], [], _ => []);
        withoutRoots.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void WalkableWaterOnlyConnectsTargetsWhenBootsMayBeAssumed()
    {
        var map = BlockedMap(1, 4);
        var root = LocationAt(0, 0);
        var target = LocationAt(3, 0);
        map[target.Pos] = Terrain.ROAD;
        map[0, 1] = Terrain.WALKABLEWATER;
        map[0, 2] = Terrain.PREPLACED_WATER_WALKABLE;

        BasicWorldRouting.CanReachRequiredLocations(map, 1, 4, [root], [target], _ => [])
            .Should().BeTrue("the default traversal assumes Boots");

        BasicWorldRouting.CanReachRequiredLocations(map, 1, 4, [root], [target], _ => [], assumeBoots: false)
            .Should().BeFalse("walkable water needs Boots");

        map[0, 1] = Terrain.ROAD;
        map[0, 2] = Terrain.ROAD;
        BasicWorldRouting.CanReachRequiredLocations(map, 1, 4, [root], [target], _ => [], assumeBoots: false)
            .Should().BeTrue("a land crossing does not need Boots");
    }

    private static Location LocationAt(int x, int y)
        => new(LocationID.WEST_NORTH_PALACE, y + 30, x, 0, Continent.WEST);

    private static OverworldMap BlockedMap(int rows, int columns)
    {
        var map = new OverworldMap(rows, columns);
        for (var y = 0; y < rows; y++)
            for (var x = 0; x < columns; x++) map[y, x] = Terrain.MOUNTAIN;
        return map;
    }
}
