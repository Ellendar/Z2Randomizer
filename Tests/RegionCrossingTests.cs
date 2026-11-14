using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class RegionCrossingTests
{
    [TestMethod]
    [DataRow((int)CrossingFeatures.None, false, false)]
    [DataRow((int)CrossingFeatures.Saria, false, false)]
    [DataRow((int)CrossingFeatures.LongBridge, false, false)]
    [DataRow((int)CrossingFeatures.DaruniaEncounters, false, false)]
    [DataRow((int)(CrossingFeatures.Saria | CrossingFeatures.LongBridge | CrossingFeatures.DaruniaEncounters), false, false)]
    [DataRow((int)CrossingFeatures.None, true, true)]
    public void NamedOptionsCanBeReused(int featureMask, bool riverDevil, bool rock)
    {
        var features = (CrossingFeatures)featureMask;
        var options = new RegionCrossingOptions
        {
            MaximumCrossings = 1,
            CrossingTerrain = Terrain.WATER,
            Biome = Biome.VANILLALIKE,
            CanWalkOnWaterWithBoots = false,
            Features = features,
            RiverDevilBlocker = riverDevil,
            RockBlocker = rock,
            DeadZone = new(20, 30, 20, 30)
        };
        var named = RiverWithFeatures(features);
        var reused = RiverWithFeatures(features);

        var result = named.PlaceCrossings(options);
        var reusedResult = reused.PlaceCrossings(options);

        reusedResult.Should().Be(result, "progress from a prior placement must not consume a reusable request");
        Snapshot(named.map).Should().Equal(Snapshot(reused.map));
        var locations = named.AllLocations.Select(l => (l.ID, l.Pos, l.CanShuffle)).ToArray();
        reused.AllLocations.Select(l => (l.ID, l.Pos, l.CanShuffle)).Should().Equal(locations);
        var nextRandom = named.NextRandom();
        reused.NextRandom().Should().Be(nextRandom);
    }

    [TestMethod]
    public void EncounterResetUsesTheLocationIdEvenWhenTheRomAddressDiffers()
    {
        var world = River();
        var encounter = Add(world, LocationID.WEST_BRIDGE_AFTER_DM_WEST);
        var unrelated = Add(world, LocationID.WEST_GRASS);
        unrelated.MemAddress = encounter.MemAddress;
        encounter.MemAddress++;
        encounter.Pos = new(2, 2);
        unrelated.Pos = new(3, 2);

        world.PlaceCrossings(0, longBridge: true);

        encounter.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        unrelated.Pos.Should().Be(new IntVector2(3, 2));
    }

    [TestMethod]
    public void PartialDeadZone_ThrowsBeforeChangingLocationsOrMap()
    {
        var world = River();
        var location = Add(world, LocationID.WEST_BRIDGE_AFTER_DM_WEST);
        location.Pos = new(2, 2); // Detect an erroneous feature reset before input validation.
        var before = Snapshot(world.map);
        var position = location.Pos;

        Action cross = () => world.PlaceCrossings(1, longBridge: true, deadZone: new(3, -1, 999, -1));

        cross.Should().Throw<ArgumentException>();
        Snapshot(world.map).Should().Equal(before);
        location.Pos.Should().Be(position);
    }

    [TestMethod]
    [DataRow(3, 9, 20, 30, false)]
    [DataRow(20, 30, 1, 7, false)]
    [DataRow(20, 30, 20, 30, true)]
    public void DeadZoneExcludesStartingShoresInEitherCoordinateBand(int minX, int maxX, int minY, int maxY, bool crosses)
    {
        var world = River();

        var result = world.PlaceCrossings(1, deadZone: new(minX, maxX, minY, maxY));

        result.PlacedCrossings.Should().Be(crosses ? 1 : 0);

        CanCross(world).Should().Be(crosses,
            "either inclusive coordinate band excludes starting shores, even when the other band is outside the map");
        Count(world.map, Terrain.BRIDGE).Should().Be(crosses ? 5 : 0);
    }

    [TestMethod]
    [DataRow(true, false, Terrain.RIVER_DEVIL)]
    [DataRow(false, true, Terrain.ROCK)]
    [DataRow(true, true, Terrain.RIVER_DEVIL)]
    public void BlockerRequestsPaintTheCrossingWithRiverDevilPriority(bool riverDevil, bool rock, Terrain blocker)
    {
        var world = River();

        world.PlaceCrossings(1, riverDevil: riverDevil, rock: rock);

        Count(world.map, blocker).Should().Be(1);
        Count(world.map, blocker == Terrain.ROCK ? Terrain.RIVER_DEVIL : Terrain.ROCK).Should().Be(0);
        Count(world.map, Terrain.BRIDGE).Should().Be(4);
    }

    [TestMethod]
    public void SimpleCrossing_ConnectsTheBanksWithoutRequiringTheMaximumCount()
    {
        var world = River();

        world.PlaceCrossings(10);

        CanCross(world).Should().BeTrue();
        Count(world.map, Terrain.BRIDGE).Should().Be(5, "one five-tile crossing connects these two banks");
    }

    [TestMethod]
    public void SariaCrossing_PlacesBothEntrancesOnOppositeBanks()
    {
        var world = River();
        var north = Add(world, LocationID.WEST_TOWN_SARIA_NORTH);
        var south = Add(world, LocationID.WEST_TOWN_SARIA_SOUTH);
        var town = new Town([], [], []) { Type = TownType.SARIA };
        north.Town = south.Town = town;

        world.PlaceCrossings(1, saria: true).PlacedFeatures.Should().Be(CrossingFeatures.Saria);

        north.Pos.Should().NotBe(south.Pos);
        new[] { north.Xpos, south.Xpos }.Should().BeEquivalentTo(new[] { 3, 9 });
        world.map[north.Pos].Should().Be(Terrain.TOWN);
        world.map[south.Pos].Should().Be(Terrain.TOWN);
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void EncounterPairsKeepTheirOrderedRolesOnBothAxes(bool darunia, bool vertical)
    {
        var world = River(vertical: vertical);
        var (first, second) = AddPair(world, darunia);

        var result = world.PlaceCrossings(1, longBridge: !darunia, darunia: darunia);

        result.PlacedCrossings.Should().Be(1);
        var near = darunia ? second : first;
        var far = darunia ? first : second;
        (vertical ? near.Y : near.Xpos).Should().BeLessThan(vertical ? far.Y : far.Xpos);
        (vertical ? near.Xpos : near.Y).Should().Be(vertical ? far.Xpos : far.Y);
        CanCross(world, vertical).Should().BeTrue();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void OnlyDaruniaSkipsAnUnavailableEncounterPair(bool darunia)
    {
        var world = River();
        var (first, _) = AddPair(world, darunia);
        first.CanShuffle = false;

        var result = world.PlaceCrossings(1, longBridge: !darunia, darunia: darunia);

        result.PlacedCrossings.Should().Be(darunia ? 0 : 1);
        result.UnavailableFeatures.Should().Be(darunia ? CrossingFeatures.DaruniaEncounters : CrossingFeatures.None);
        CanCross(world).Should().Be(!darunia);
    }

    [TestMethod]
    public void CandidateSearchDoesNotBridgeABayWhoseBanksAlreadyConnect()
    {
        var world = River();
        for (int x = 1; x < 12; x++) world.map[1, x] = Terrain.GRASS;
        var before = Snapshot(world.map);

        var result = world.PlaceCrossings(10);

        result.PlacedCrossings.Should().Be(0);
        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void NamedCrossing_PlacesThePairAndPreservesRequirements(bool darunia)
    {
        var world = River();
        var (first, second) = AddPair(world, darunia);
        first.AccessRequirements = new Requirements([RequirementType.JUMP]);

        world.PlaceCrossings(1, longBridge: !darunia, darunia: darunia).PlacedCrossings.Should().Be(1);

        first.Pos.Should().NotBe(second.Pos);
        first.CanShuffle.Should().BeFalse();
        second.CanShuffle.Should().BeFalse();
        var terrain = darunia ? Terrain.DESERT : Terrain.BRIDGE;
        world.map[first.Pos].Should().Be(terrain);
        world.map[second.Pos].Should().Be(terrain);
        first.AccessRequirements.HasHardRequirement(RequirementType.JUMP).Should().BeTrue();
        CanCross(world).Should().BeTrue();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Result_ReportsAnEmptyPoolAndPendingSaria(bool saria)
    {
        var world = new TestWorld(5, 7, Terrain.GRASS);
        var before = Snapshot(world.map);

        var result = world.PlaceCrossings(4, saria: saria);

        result.Termination.Should().Be(RegionCrossingTermination.CandidatePoolExhausted);
        result.PlacedCrossings.Should().Be(0);
        result.PendingFeatures.Should().Be(saria ? CrossingFeatures.Saria : CrossingFeatures.None);
        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Result_ReportsBudgetExhaustionWithCandidatesStillAvailable(bool saria)
    {
        var world = BlockedShorelines();
        var before = Snapshot(world.map);

        var result = world.PlaceCrossings(1, saria: saria);

        result.Termination.Should().Be(RegionCrossingTermination.AttemptBudgetExhausted);
        result.PlacedCrossings.Should().Be(0);
        result.PendingFeatures.Should().Be(saria ? CrossingFeatures.Saria : CrossingFeatures.None);
        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Result_ZeroLimitDoesNotClaimPendingFeaturesWerePlaced(bool saria)
    {
        var world = River();
        var before = Snapshot(world.map);

        var result = world.PlaceCrossings(0, saria: saria, longBridge: true, darunia: true);

        result.Termination.Should().Be(RegionCrossingTermination.CrossingLimitReached);
        result.PlacedCrossings.Should().Be(0);
        result.PlacedFeatures.Should().Be(CrossingFeatures.None);
        result.UnavailableFeatures.Should().Be(CrossingFeatures.None);
        result.PendingFeatures.Should().Be(result.RequestedFeatures);
        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    public void Result_CountsTheActualCrossingEvenWhenTheLimitIsLarger(int limit)
    {
        var world = River();

        var result = world.PlaceCrossings(limit);

        result.Termination.Should().Be(limit == 1
            ? RegionCrossingTermination.CrossingLimitReached : RegionCrossingTermination.CandidatePoolExhausted);
        result.PlacedCrossings.Should().Be(1);
        result.PlacedFeatures.Should().Be(CrossingFeatures.None);
        CanCross(world).Should().BeTrue();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Result_ReportsCompletedNamedPlacement(bool darunia)
    {
        var world = River();
        var (first, second) = AddPair(world, darunia);
        var requested = darunia ? CrossingFeatures.DaruniaEncounters : CrossingFeatures.LongBridge;

        var result = world.PlaceCrossings(1, longBridge: !darunia, darunia: darunia);

        result.RequestedFeatures.Should().Be(requested);
        result.PlacedFeatures.Should().Be(requested);
        result.PendingFeatures.Should().Be(CrossingFeatures.None);
        result.PlacedCrossings.Should().Be(1);
        first.Pos.Should().NotBe(second.Pos);
        CanCross(world).Should().BeTrue();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void BlockedEncounterEntrance_LeavesTerrainUnchangedAndReportsThePendingFeature(bool darunia)
    {
        var world = River(cliffFlanks: true);
        var (first, second) = AddPair(world, darunia);
        var before = Snapshot(world.map);

        var result = world.PlaceCrossings(1, longBridge: !darunia, darunia: darunia);

        result.Termination.Should().Be(darunia
            ? RegionCrossingTermination.DaruniaEntranceBlocked : RegionCrossingTermination.LongBridgeEntranceBlocked);
        result.PlacedCrossings.Should().Be(0);
        result.PlacedFeatures.Should().Be(CrossingFeatures.None);
        result.PendingFeatures.Should().Be(darunia ? CrossingFeatures.DaruniaEncounters : CrossingFeatures.LongBridge);
        Snapshot(world.map).Should().Equal(before);
        first.IsEnabled().Should().BeFalse();
        second.IsEnabled().Should().BeFalse();
        first.CanShuffle.Should().BeTrue();
        second.CanShuffle.Should().BeTrue();
        CanCross(world).Should().BeFalse();
    }

    [TestMethod]
    public void Result_UnavailablePairConsumesALimitSlotWithoutCountingAsPlaced()
    {
        var world = River();
        var (first, second) = AddPair(world, darunia: true);
        first.CanShuffle = false;
        var before = Snapshot(world.map);

        var result = world.PlaceCrossings(1, darunia: true);

        result.Termination.Should().Be(RegionCrossingTermination.CrossingLimitReached);
        result.PlacedCrossings.Should().Be(0);
        result.UnavailableFeatures.Should().Be(CrossingFeatures.DaruniaEncounters);
        result.PendingFeatures.Should().Be(CrossingFeatures.None);
        result.PlacedFeatures.Should().Be(CrossingFeatures.None);
        first.IsEnabled().Should().BeFalse();
        second.IsEnabled().Should().BeFalse();
        Snapshot(world.map).Should().Equal(before);
        CanCross(world).Should().BeFalse();
    }

    [TestMethod]
    public void Result_SariaHasPriorityAndReportsOtherRequestedFeaturesStillPending()
    {
        var world = River();
        var north = Add(world, LocationID.WEST_TOWN_SARIA_NORTH);
        var south = Add(world, LocationID.WEST_TOWN_SARIA_SOUTH);
        north.Town = south.Town = new Town([], [], []) { Type = TownType.SARIA };
        AddPair(world, darunia: false);
        AddPair(world, darunia: true);

        var result = world.PlaceCrossings(1, saria: true, longBridge: true, darunia: true);

        result.PlacedFeatures.Should().Be(CrossingFeatures.Saria);
        result.PlacedCrossings.Should().Be(1);
        result.PendingFeatures.Should().Be(CrossingFeatures.LongBridge | CrossingFeatures.DaruniaEncounters);
        world.map[north.Pos].Should().Be(Terrain.TOWN);
        world.map[south.Pos].Should().Be(Terrain.TOWN);
    }

    [TestMethod]
    public void Result_NoShoreDoesNotClaimNamedFeaturesWereUnavailableOrPlaced()
    {
        var world = new TestWorld(5, 7, Terrain.GRASS);
        AddPair(world, darunia: false);
        AddPair(world, darunia: true);

        var result = world.PlaceCrossings(4, longBridge: true, darunia: true);

        result.PendingFeatures.Should().Be(CrossingFeatures.LongBridge | CrossingFeatures.DaruniaEncounters);
        result.PlacedFeatures.Should().Be(CrossingFeatures.None);
        result.UnavailableFeatures.Should().Be(CrossingFeatures.None);
        result.PlacedCrossings.Should().Be(0);
    }

    private static TestWorld BlockedShorelines()
    {
        // Thousands of isolated one-tile islands provide candidates, but no usable
        // shore. The search must remain bounded without consuming the whole pool.
        var world = new TestWorld(101, 101, Terrain.WATER);
        for (int y = 2; y < 100; y += 2)
            for (int x = 2; x < 100; x += 2)
                world.map[y, x] = Terrain.GRASS;
        return world;
    }

    private static TestWorld River(bool cliffFlanks = false, bool vertical = false)
    {
        var world = new TestWorld(vertical ? 13 : 9, vertical ? 9 : 13);
        for (int y = 1; y < 8; y++)
            for (int x = 1; x < 12; x++)
            {
                var position = vertical ? new IntVector2(y, x) : new IntVector2(x, y);
                if (x <= 3 || x >= 9) world.map[position] = Terrain.GRASS;
                else if (!cliffFlanks || y == 4) world.map[position] = Terrain.WATER;
            }
        return world;
    }

    private static TestWorld RiverWithFeatures(CrossingFeatures features)
    {
        var world = River();
        if ((features & CrossingFeatures.Saria) != 0)
        {
            var north = Add(world, LocationID.WEST_TOWN_SARIA_NORTH);
            var south = Add(world, LocationID.WEST_TOWN_SARIA_SOUTH);
            north.Town = south.Town = new Town([], [], []) { Type = TownType.SARIA };
        }
        if ((features & CrossingFeatures.LongBridge) != 0) AddPair(world, darunia: false);
        if ((features & CrossingFeatures.DaruniaEncounters) != 0) AddPair(world, darunia: true);
        return world;
    }

    private static (Location, Location) AddPair(TestWorld world, bool darunia)
        => darunia
            ? (Add(world, LocationID.EAST_TRAP_DESERT1), Add(world, LocationID.EAST_TRAP_DESERT2))
            : (Add(world, LocationID.WEST_BRIDGE_AFTER_DM_WEST), Add(world, LocationID.WEST_BRIDGE_AFTER_DM_EAST));

    private static Location Add(TestWorld world, LocationID id)
    {
        var location = new Location(id, 30, 0, 0, id.GetContinent()) { CanShuffle = true };
        world.AddLocation(location);
        return location;
    }

    private static bool CanCross(TestWorld world, bool vertical = false)
    {
        var start = new Location(LocationID.WEST_GRASS, vertical ? 32 : 34, vertical ? 4 : 2, 0, Continent.WEST);
        var target = new Location(LocationID.WEST_GRASS, vertical ? 40 : 34, vertical ? 4 : 10, 0, Continent.WEST);
        return BasicWorldRouting.CanReachRequiredLocations(world.map, world.MapRows, world.MapColumns,
            new[] { start }, new[] { target }, _ => Array.Empty<Location>());
    }

    private static int Count(OverworldMap map, Terrain terrain) => Snapshot(map).Count(t => t == terrain);
    private static Terrain[] Snapshot(OverworldMap map)
        => (from y in Enumerable.Range(0, map.GetLength(0))
            from x in Enumerable.Range(0, map.GetLength(1))
            select map[y, x]).ToArray();
}
