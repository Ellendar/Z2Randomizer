using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class CaveExitSafetyTests
{
    [TestMethod]
    [DataRow(Terrain.GRASS)]
    [DataRow(Terrain.ROAD)]
    [DataRow(Terrain.CAVE)]
    [DataRow(Terrain.LAVA)]
    [DataRow(Terrain.ROCK)]
    [DataRow(Terrain.RIVER_DEVIL)]
    [DataRow(Terrain.PREPLACED_WATER_WALKABLE)]
    public void LegacyValidation_AcceptsOtherWalkableTerrainWithoutRepair(Terrain exit)
    {
        var world = new TestWorld(3, 4);
        world.map[1, 1] = Terrain.CAVE;
        world.map[1, 2] = exit;
        var before = Snapshot(world.map);

        world.RepairAndValidateCaves().Should().BeTrue();

        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    [DataRow(Terrain.MOUNTAIN)]
    [DataRow(Terrain.WATER)]
    [DataRow(Terrain.PREPLACED_WATER)]
    [DataRow(Terrain.NONE)]
    public void LegacyValidation_RejectsImpassableExitsWithoutRepair(Terrain exit)
    {
        var world = new TestWorld(3, 4);
        world.map[1, 1] = Terrain.CAVE;
        world.map[1, 2] = exit;
        var before = Snapshot(world.map);

        world.RepairAndValidateCaves().Should().BeFalse();

        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    public void LegacyValidation_TwoWaterExitsRemainUnchanged()
    {
        // Existing rule repairs only a sole exit; this does not claim two water
        // exits are safe without Boots. Changing that policy needs its own fix.
        var world = new TestWorld(3, 3);
        world.map[1, 1] = Terrain.CAVE;
        world.map[1, 0] = world.map[1, 2] = Terrain.WALKABLEWATER;
        var before = Snapshot(world.map);

        world.RepairAndValidateCaves().Should().BeTrue();

        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    public void LegacyValidation_StopsRepairingAtTheFirstEnclosedCave()
    {
        var world = new TestWorld(3, 9);
        world.map[1, 1] = world.map[1, 4] = world.map[1, 7] = Terrain.CAVE;
        world.map[1, 2] = world.map[1, 8] = Terrain.WALKABLEWATER;

        world.RepairAndValidateCaves().Should().BeFalse();

        world.map[1, 2].Should().Be(Terrain.ROAD, "repairs before the failed cave are retained");
        world.map[1, 8].Should().Be(Terrain.WALKABLEWATER, "later caves have not been processed");
    }

    [TestMethod]
    [DataRow(3, 1)]
    [DataRow(1, 3)]
    public void LegacyValidation_PaddingCannotSupplyAnExit(int paddingX, int paddingY)
    {
        var world = new TestWorld(3, 3);
        world.map = new OverworldMap(4, 4, Terrain.MOUNTAIN);
        world.map[paddingY == 3 ? 2 : 1, paddingX == 3 ? 2 : 1] = Terrain.CAVE;
        world.map[paddingY, paddingX] = Terrain.GRASS;
        var before = Snapshot(world.map);

        world.RepairAndValidateCaves().Should().BeFalse();

        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    public void LegacyValidation_IgnoresCavesInPadding()
    {
        var world = new TestWorld(3, 3);
        world.map = new OverworldMap(4, 4, Terrain.MOUNTAIN);
        world.map[3, 3] = Terrain.CAVE;
        var before = Snapshot(world.map);

        world.RepairAndValidateCaves().Should().BeTrue();

        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    public void LegacyRepair_SharedWaterExitStaysReachableAndRepeatedValidationIsIdempotent()
    {
        var world = new TestWorld(3, 5);
        world.map[1, 1] = world.map[1, 3] = Terrain.CAVE;
        world.map[1, 2] = Terrain.WALKABLEWATER;
        var start = new Location(LocationID.WEST_CAVE_PARAPA_NORTH, 31, 1, 0, Continent.WEST);
        var target = new Location(LocationID.WEST_CAVE_PARAPA_SOUTH, 31, 3, 0, Continent.WEST);
        var required = new[] { start, target };
        bool CanRoute() => BasicWorldRouting.CanReachRequiredLocations(world.map, 3, 5,
            new[] { start }, required, _ => Array.Empty<Location>());

        CanRoute().Should().BeTrue();
        world.RepairAndValidateCaves().Should().BeTrue();
        world.map[1, 2].Should().Be(Terrain.ROAD);
        CanRoute().Should().BeTrue("repair must retain the route to the other cave");
        var repaired = Snapshot(world.map);

        world.RepairAndValidateCaves().Should().BeTrue();
        Snapshot(world.map).Should().Equal(repaired);
    }

    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(2, 1)]
    [DataRow(1, 2)]
    [DataRow(0, 1)]
    public void ReadOnlyValidation_ReportsSoleWaterExitWithoutRepairingIt(int exitX, int exitY)
    {
        var map = new OverworldMap(3, 3, Terrain.MOUNTAIN);
        map[1, 1] = Terrain.CAVE;
        map[exitY, exitX] = Terrain.WALKABLEWATER;
        var before = Snapshot(map);

        var failure = CaveExitSafety.FindFailure(map, 3, 3);

        failure.Should().Be(new CaveExitFailure(new(1, 1), CaveExitFailureReason.SoleWalkableWaterExit));
        CaveExitSafety.FindFailure(map, 3, 3).Should().Be(failure);
        Snapshot(map).Should().Equal(before);
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(2, 0)]
    [DataRow(0, 2)]
    [DataRow(2, 2)]
    public void ReadOnlyValidation_ReportsEnclosedEdgeCaveWithoutUsingDiagonalAccess(int x, int y)
    {
        var map = new OverworldMap(3, 3, Terrain.MOUNTAIN);
        map[y, x] = Terrain.CAVE;
        map[1, 1] = Terrain.GRASS;
        var before = Snapshot(map);

        CaveExitSafety.FindFailure(map, 3, 3).Should().Be(
            new CaveExitFailure(new(x, y), CaveExitFailureReason.Enclosed));

        Snapshot(map).Should().Equal(before);
    }

    [TestMethod]
    public void RepairPhase_ChangesOnlyTheSoleWaterExitAndLeavesTheFinalCheckReadOnly()
    {
        var map = new OverworldMap(3, 7, Terrain.MOUNTAIN);
        map[1, 1] = map[1, 5] = Terrain.CAVE;
        map[1, 2] = map[1, 4] = Terrain.WALKABLEWATER;
        map[1, 6] = Terrain.GRASS;
        var expected = map.Clone();
        expected[1, 2] = Terrain.ROAD;

        CaveExitSafety.RepairSoleWaterExits(map, 3, 7);

        Snapshot(map).Should().Equal(Snapshot(expected));
        CaveExitSafety.FindFailure(map, 3, 7).Should().BeNull();
        Snapshot(map).Should().Equal(Snapshot(expected));
        CaveExitSafety.RepairSoleWaterExits(map, 3, 7);
        Snapshot(map).Should().Equal(Snapshot(expected));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ReadOnlyValidation_RetainsTheNarrowLegacyWaterPolicy(bool twoWaterExits)
    {
        var map = new OverworldMap(3, 3, Terrain.MOUNTAIN);
        map[1, 1] = Terrain.CAVE;
        map[1, 0] = twoWaterExits ? Terrain.WALKABLEWATER : Terrain.PREPLACED_WATER_WALKABLE;
        if (twoWaterExits) map[1, 2] = Terrain.WALKABLEWATER;
        var before = Snapshot(map);

        CaveExitSafety.FindFailure(map, 3, 3).Should().BeNull();

        Snapshot(map).Should().Equal(before);
    }

    [TestMethod]
    [DataRow(3, 1)]
    [DataRow(1, 3)]
    public void ReadOnlyValidation_IgnoresBackingPadding(int paddingX, int paddingY)
    {
        var map = new OverworldMap(4, 4, Terrain.MOUNTAIN);
        var cave = new IntVector2(paddingX == 3 ? 2 : 1, paddingY == 3 ? 2 : 1);
        map[cave] = Terrain.CAVE;
        map[paddingY, paddingX] = Terrain.WALKABLEWATER;
        var before = Snapshot(map);

        CaveExitSafety.RepairSoleWaterExits(map, 3, 3);
        CaveExitSafety.FindFailure(map, 3, 3).Should().Be(new CaveExitFailure(cave, CaveExitFailureReason.Enclosed));

        Snapshot(map).Should().Equal(before);
    }

    private static Terrain[] Snapshot(OverworldMap map)
        => (from y in Enumerable.Range(0, map.GetLength(0))
            from x in Enumerable.Range(0, map.GetLength(1))
            select map[y, x]).ToArray();
}
