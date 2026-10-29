using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Z2Randomizer.Tests;

// Characterizes shared World behavior. This fixture deliberately does not stand in
// for any continent constructor, Terraform implementation, or GetPathingStarts policy.
[TestClass]
public class WorldCharacterizationTests
{
    [TestMethod]
    public void InitializeGeneratedMap_ClearsPreviousTerrainWithoutResettingLocations()
    {
        var world = new TestWorld(3, 5);
        var location = LocationAt(LocationID.WEST_GRASS, 2, 1, Terrain.GRASS);
        world.AddLocation(location);
        location.CanShuffle = false;

        world.StartGeneratedMap();

        world.map.GetLength(0).Should().Be(3);
        world.map.GetLength(1).Should().Be(5);
        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 5; x++)
                world.map[y, x].Should().Be(Terrain.NONE);
        location.Pos.Should().Be(new IntVector2(2, 1));
        location.CanShuffle.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(Terrain.MOUNTAIN)]
    [DataRow(Terrain.PREPLACED_WATER)]
    public void FillMapBorder_PaintsLogicalPerimeterAndPreservesInteriorAndPadding(Terrain terrain)
    {
        var world = new TestWorld(3, 5);
        world.map = new OverworldMap(3, 64);
        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 64; x++)
                world.map[y, x] = Terrain.GRASS;

        world.PaintBorder(terrain);

        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 64; x++)
                world.map[y, x].Should().Be(x < 5 && (x == 0 || x == 4 || y == 0 || y == 2)
                    ? terrain : Terrain.GRASS);
    }

    [TestMethod]
    [DataRow(Terrain.MOUNTAIN, 10, 11)]
    [DataRow(Terrain.PREPLACED_WATER, 10, 11)]
    [DataRow(Terrain.PREPLACED_WATER, 1, 1)]
    public void DrawTerrainStripes_SeedsDistinctBoundedStripesWithoutOverwritingReservedTerrain(
        Terrain terrain, int nearEdgeMargin, int farEdgeMargin)
    {
        var world = new TestWorld(31, 37, Terrain.NONE);
        // A reserved feature crosses every possible stripe on both axes.
        for (int x = 0; x < 37; x++) world.map[15, x] = Terrain.PALACE;
        for (int y = 0; y < 31; y++) world.map[y, 18] = Terrain.LAVA;

        world.SeedStripes(terrain, nearEdgeMargin, farEdgeMargin);

        var columns = Enumerable.Range(0, 37).Where(x => world.map[0, x] == terrain).ToArray();
        var rows = Enumerable.Range(0, 31).Where(y => world.map[y, 0] == terrain).ToArray();
        columns.Length.Should().BeInRange(2, 3);
        rows.Length.Should().BeInRange(2, 3);
        columns.Should().OnlyContain(x => x >= nearEdgeMargin && x < 37 - farEdgeMargin);
        rows.Should().OnlyContain(y => y >= nearEdgeMargin && y < 31 - farEdgeMargin);
        for (int x = 0; x < 37; x++) world.map[15, x].Should().Be(x == 18 ? Terrain.LAVA : Terrain.PALACE);
        for (int y = 0; y < 31; y++) world.map[y, 18].Should().Be(Terrain.LAVA);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void PrepareTerraformLocations_RestoresPlacementWithoutResettingLogic(bool vanilla)
    {
        var world = new TestWorld(3, 5) { biome = vanilla ? Biome.VANILLA : Biome.VANILLALIKE, AllReached = true };
        var location = new Location(LocationID.WEST_GRASS, 31, 2, 0, Continent.WEST, isPassthrough: true)
        {
            Pos = new IntVector2(3, 2), IsPassthrough = false, CanShuffle = false, Reachable = true,
            TerrainType = Terrain.GRASS, AccessRequirements = new Requirements([RequirementType.HAMMER])
        };
        location.SetCollectables([Collectable.GLOVE]);
        world.AddLocation(location);

        world.ResetPlacement();

        location.Pos.Should().Be(vanilla ? new IntVector2(2, 1) : IntVector2.NOT_ON_MAP);
        location.IsPassthrough.Should().BeTrue();
        location.CanShuffle.Should().BeTrue();
        location.Reachable.Should().BeTrue();
        location.AccessRequirements.HasHardRequirement(RequirementType.HAMMER).Should().BeTrue();
        location.GetAllCollectables().Should().Equal(Collectable.GLOVE);
        world.AllReached.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void RepaintLocationTiles_UsesLocationOrderAndKeepsPassthroughPolicyExplicit(bool markPassthrough)
    {
        var world = new TestWorld(3, 5);
        var first = LocationAt(LocationID.WEST_GRASS, 2, 1, Terrain.GRASS);
        var last = LocationAt(LocationID.WEST_CAVE_TROPHY, 2, 1, Terrain.FOREST);
        first.IsPassthrough = true;
        world.AddLocation(first);
        world.AddLocation(last);

        world.PaintLocations(markPassthrough);

        world.map[1, 2].Should().Be(Terrain.FOREST);
        first.IsPassthrough.Should().BeTrue();
        last.IsPassthrough.Should().Be(markPassthrough);
        first.Pos.Should().Be(new IntVector2(2, 1));
        world.map[0, 0].Should().Be(Terrain.MOUNTAIN);
    }

    [TestMethod]
    public void InitializeVisitation_UsesLogicalDimensionsAndRetainsLocationAndMapState()
    {
        var world = new TestWorld(3, 5) { AllReached = true };
        world.map = new OverworldMap(3, 64);
        var start = LocationAt(LocationID.WEST_NORTH_PALACE, 2, 1);
        world.AddLocation(start);
        world.map[start.Pos] = Terrain.ROAD;
        world.SetStart(start);
        world.TileVisited(start.Pos).Should().BeFalse();
        start.Reachable.Should().BeFalse();

        world.UpdateVisit(RequirementTypeExtensions.ALL);
        world.TileVisited(start.Pos).Should().BeTrue();
        start.CanShuffle = false;

        world.InitializeGrid();

        world.TileVisited(start.Pos).Should().BeFalse();
        world.GridDimensions.Should().Be((3, 5));
        start.Reachable.Should().BeTrue();
        start.CanShuffle.Should().BeFalse();
        world.AllReached.Should().BeTrue();
        world.map[start.Pos].Should().Be(Terrain.ROAD);
    }

    [TestMethod]
    [DataRow(Terrain.WALKABLEWATER, RequirementType.BOOTS)]
    [DataRow(Terrain.PREPLACED_WATER_WALKABLE, RequirementType.BOOTS)]
    [DataRow(Terrain.ROCK, RequirementType.HAMMER)]
    [DataRow(Terrain.RIVER_DEVIL, RequirementType.FLUTE)]
    public void UpdateVisit_TerrainGateBlocksUntilItsItemIsAvailable(Terrain gate, RequirementType item)
    {
        var world = Corridor(gate, out var start, out var target);
        world.SetStart(start);

        world.UpdateVisit(new HashSet<RequirementType>());
        target.Reachable.Should().BeFalse();

        world.UpdateVisit(new HashSet<RequirementType> { item });
        target.Reachable.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(Terrain.WATER)]
    [DataRow(Terrain.PREPLACED_WATER)]
    [DataRow(Terrain.MOUNTAIN)]
    public void UpdateVisit_ImpassableTerrainStillBlocksWithAllTerrainItems(Terrain gate)
    {
        var world = Corridor(gate, out var start, out var target);
        world.SetStart(start);

        world.UpdateVisit(new HashSet<RequirementType>
        {
            RequirementType.BOOTS, RequirementType.HAMMER, RequirementType.FLUTE
        });

        target.Reachable.Should().BeFalse();
    }

    [TestMethod]
    public void UpdateVisit_LocationRequirementsAlsoBlockThePathBeyondIt()
    {
        var world = Corridor(Terrain.GRASS, out var start, out var target);
        var gate = LocationAt(LocationID.WEST_CAVE_JUMP_NORTH, 2, 1, Terrain.GRASS);
        gate.AccessRequirements = new Requirements([RequirementType.JUMP, RequirementType.FAIRY]);
        world.AddLocation(gate);
        world.SetStart(start);

        world.UpdateVisit(new HashSet<RequirementType>());
        gate.Reachable.Should().BeFalse();
        target.Reachable.Should().BeFalse();

        world.UpdateVisit(new HashSet<RequirementType> { RequirementType.FAIRY });
        gate.Reachable.Should().BeFalse("Fairy also requires sufficient magic containers");
        target.Reachable.Should().BeFalse();

        world.UpdateVisit(new HashSet<RequirementType> { RequirementType.FAIRY, RequirementType.FOUR_CONTAINERS });
        gate.Reachable.Should().BeTrue();
        target.Reachable.Should().BeTrue();
    }

    [TestMethod]
    public void UpdateVisit_CaveExitBecomesAPathingRootOnTheNextCall()
    {
        var world = new TestWorld(3, 7);
        var start = LocationAt(LocationID.WEST_NORTH_PALACE, 0, 1);
        var entrance = LocationAt(LocationID.WEST_CAVE_PARAPA_NORTH, 1, 1, Terrain.CAVE);
        var exit = LocationAt(LocationID.WEST_CAVE_PARAPA_SOUTH, 4, 1, Terrain.CAVE);
        var target = LocationAt(LocationID.WEST_GRASS, 5, 1);
        foreach (var location in new[] { start, entrance, exit, target })
        {
            world.AddLocation(location);
            world.map[location.Pos] = location.TerrainType;
        }
        world.connections[entrance] = exit;
        world.connections[exit] = entrance;
        world.SetStart(start);

        world.UpdateVisit(new HashSet<RequirementType>());
        exit.Reachable.Should().BeTrue();
        target.Reachable.Should().BeFalse();

        world.UpdateVisit(new HashSet<RequirementType>());
        target.Reachable.Should().BeTrue();
    }

    [TestMethod]
    public void UpdateVisit_TopRowLocationIsTraversedButNotMarkedReachable()
    {
        // Existing Y > 0 guard: compatibility evidence, not a desired new rule.
        var world = new TestWorld(2, 3);
        var start = LocationAt(LocationID.WEST_NORTH_PALACE, 0, 1);
        var topRow = LocationAt(LocationID.WEST_GRASS, 1, 0);
        var target = LocationAt(LocationID.WEST_CAVE_TROPHY, 2, 1);
        foreach (var pos in new[] { start.Pos, new IntVector2(0, 0), topRow.Pos, new IntVector2(2, 0), target.Pos })
        {
            world.map[pos] = Terrain.GRASS;
        }
        world.AddLocation(start);
        world.AddLocation(topRow);
        world.AddLocation(target);
        world.SetStart(start);

        world.UpdateVisit(new HashSet<RequirementType>());

        topRow.Reachable.Should().BeFalse();
        target.Reachable.Should().BeTrue();
    }

    [TestMethod]
    public void ResetVisitabilityState_ClearsReachabilityAndUnlocksShuffleButKeepsTheLayoutAndAllReachedLatch()
    {
        var world = Corridor(Terrain.ROCK, out var start, out var target);
        target.SetCollectables([Collectable.HAMMER]);
        world.SetStart(start);
        world.UpdateVisit(new HashSet<RequirementType> { RequirementType.HAMMER });
        target.CanShuffle = false;
        world.UpdateAllReached();
        world.AllReached.Should().BeTrue();
        var previousPosition = target.Pos;

        world.ResetVisitabilityState();

        world.AllLocations.Should().OnlyContain(location => !location.Reachable && location.CanShuffle);
        target.Pos.Should().Be(previousPosition);
        world.map[1, 2].Should().Be(Terrain.ROCK);
        world.AllReached.Should().BeTrue();

        world.SetStart(start);
        world.UpdateVisit(new HashSet<RequirementType>());
        target.Reachable.Should().BeFalse("the previous terrain visitation must also have been cleared");
    }

    [TestMethod]
    public void UpdateAllReached_ConsidersItemLocationsAndTownsButIgnoresAnEmptyConnector()
    {
        var world = new TestWorld(3, 5);
        var item = LocationAt(LocationID.WEST_GRASS, 1, 1);
        item.SetCollectables([Collectable.HAMMER]);
        var town = LocationAt(LocationID.WEST_TOWN_RAURU, 2, 1, Terrain.TOWN);
        var connector = LocationAt(LocationID.WEST_RAFT_TO_EAST, 3, 1, Terrain.BRIDGE);
        world.AddLocation(item);
        world.AddLocation(town);
        world.AddLocation(connector);

        world.UpdateAllReached();
        world.AllReached.Should().BeFalse();
        item.Reachable = true;
        world.UpdateAllReached();
        world.AllReached.Should().BeFalse();
        town.Reachable = true;
        world.UpdateAllReached();
        world.AllReached.Should().BeTrue();
        connector.Reachable.Should().BeFalse();
    }

    [TestMethod]
    public void AddLocation_WalkableWaterUsesTheSwampPlacementBucketWithoutChangingItsTerrain()
    {
        var world = new TestWorld(3, 3);
        var location = LocationAt(LocationID.EAST_WATER, 1, 1, Terrain.WALKABLEWATER);

        world.AddLocation(location);

        world.AllLocations.Should().ContainSingle().Which.Should().BeSameAs(location);
        world.Locations[Terrain.SWAMP].Should().ContainSingle().Which.Should().BeSameAs(location);
        world.Locations[Terrain.WALKABLEWATER].Should().BeEmpty();
        location.TerrainType.Should().Be(Terrain.WALKABLEWATER);
    }

    [TestMethod]
    public void GetLocation_MissingIdentityThrows()
    {
        var world = new TestWorld(3, 3);

        Action lookup = () => world.GetLocation(LocationID.WEST_GRASS);

        lookup.Should().Throw<Exception>();
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(0, 0)]
    public void ValidateCaves_RejectsASealedExitIncludingAtTheMapEdge(int x, int y)
    {
        var world = new TestWorld(3, 3);
        world.map[y, x] = Terrain.CAVE;
        world.map[2, 2] = Terrain.GRASS; // Diagonal access is insufficient.

        world.RepairAndValidateCaves().Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(1, 2)]
    [DataRow(0, 1)]
    [DataRow(2, 1)]
    public void ValidateCaves_RepairsASoleWalkableWaterExit(int exitX, int exitY)
    {
        var world = new TestWorld(3, 3);
        world.map[1, 1] = Terrain.CAVE;
        world.map[exitY, exitX] = Terrain.WALKABLEWATER;

        world.RepairAndValidateCaves().Should().BeTrue();

        world.map[exitY, exitX].Should().Be(Terrain.ROAD);
        world.map[1, 1].Should().Be(Terrain.CAVE);
    }

    [TestMethod]
    public void ValidateCaves_LeavesWaterUnchangedWhenThereIsAlsoADryExit()
    {
        var world = new TestWorld(3, 3);
        world.map[1, 1] = Terrain.CAVE;
        world.map[1, 0] = Terrain.WALKABLEWATER;
        world.map[1, 2] = Terrain.GRASS;

        world.RepairAndValidateCaves().Should().BeTrue();

        world.map[1, 0].Should().Be(Terrain.WALKABLEWATER);
        world.map[1, 2].Should().Be(Terrain.GRASS);
    }

    [TestMethod]
    [DataRow(Direction.NORTH, 2, 0)]
    [DataRow(Direction.EAST, 4, 2)]
    [DataRow(Direction.SOUTH, 2, 4)]
    [DataRow(Direction.WEST, 0, 2)]
    public void PlaceBridge_DrawsFromTheLandingToTheSelectedEdge(Direction direction, int edgeX, int edgeY)
    {
        var world = new TestWorld(5, 5, Terrain.WATER);
        var bridge = LocationAt(LocationID.MI_CONNECTOR_BRIDGE, 1, 1, Terrain.BRIDGE);
        bridge.IsPassthrough = true;

        World.PlaceBridge(world.map, bridge, 2, 2, direction);

        bridge.Pos.Should().Be(new IntVector2(2, 2));
        bridge.CanShuffle.Should().BeFalse();
        bridge.IsPassthrough.Should().BeFalse();
        world.map[edgeY, edgeX].Should().Be(Terrain.BRIDGE);
        world.map[(edgeY + 2) / 2, (edgeX + 2) / 2].Should().Be(Terrain.BRIDGE);
        var bridgeTiles = 0;
        for (var y = 0; y < 5; y++)
        {
            for (var x = 0; x < 5; x++)
            {
                if (world.map[y, x] == Terrain.BRIDGE) bridgeTiles++;
                else world.map[y, x].Should().Be(Terrain.WATER);
            }
        }
        bridgeTiles.Should().Be(3);
    }

    [TestMethod]
    public void PlaceRaft_DrawsOnlyTheLandingAndPreservesPassthrough()
    {
        var world = new TestWorld(3, 3, Terrain.WATER);
        var raft = LocationAt(LocationID.WEST_RAFT_TO_EAST, 0, 0, Terrain.BRIDGE);
        raft.IsPassthrough = true;

        World.PlaceRaft(world.map, raft, 1, 1);

        raft.Pos.Should().Be(new IntVector2(1, 1));
        raft.CanShuffle.Should().BeFalse();
        raft.IsPassthrough.Should().BeTrue();
        world.map[1, 1].Should().Be(Terrain.BRIDGE);
        world.map[1, 0].Should().Be(Terrain.WATER);
        world.map[1, 2].Should().Be(Terrain.WATER);
        world.map[0, 1].Should().Be(Terrain.WATER);
        world.map[2, 1].Should().Be(Terrain.WATER);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void DrawConnector_NoWaterAtTheEdgeFailsWithoutMovingTheLocation(bool isRaft)
    {
        var world = new TestWorld(5, 5, Terrain.GRASS);
        var location = LocationAt(LocationID.WEST_RAFT_TO_EAST, 2, 2, Terrain.BRIDGE);
        var rng = new Random(42);

        var success = isRaft
            ? World.DrawRaft(rng, world.map, location, [Terrain.GRASS], Direction.EAST)
            : World.DrawBridge(rng, world.map, location, [Terrain.GRASS], Direction.EAST);

        success.Should().BeFalse();
        location.Pos.Should().Be(new IntVector2(2, 2));
        location.CanShuffle.Should().BeTrue();
        world.map[2, 2].Should().Be(Terrain.GRASS);
    }

    [TestMethod]
    public void TryPlaceConnectors_AbsentConnectorsSucceedWithoutConsumingRng()
    {
        var world = new TestWorld(5, 9, Terrain.GRASS);

        world.PlaceConnectors(Direction.WEST, Direction.EAST).Should().Be((true, (string?)null));

        world.NextRandom().Should().Be(new Random(42).Next());
    }

    [TestMethod]
    public void TryPlaceConnectors_RaftFailureLeavesAnOtherwisePlaceableBridgeUntouched()
    {
        var world = ConnectorShoreline(westWater: false, eastWater: true);
        world.raft = LocationAt(LocationID.WEST_RAFT_TO_EAST, 4, 2, Terrain.BRIDGE);
        world.bridge = LocationAt(LocationID.WEST_BRIDGE_NORTH_OF_SARIA, 4, 3, Terrain.BRIDGE);

        world.PlaceConnectors(Direction.WEST, Direction.EAST).Should().Be((false, "Raft"));

        world.raft.CanShuffle.Should().BeTrue();
        world.bridge.CanShuffle.Should().BeTrue();
        world.bridge.Pos.Should().Be(new IntVector2(4, 3));
        world.map[3, 8].Should().Be(Terrain.WATER);
    }

    [TestMethod]
    public void TryPlaceConnectors_BridgeFailureRetainsSuccessfulRaftPlacement()
    {
        var world = ConnectorShoreline(westWater: true, eastWater: false);
        world.raft = LocationAt(LocationID.WEST_RAFT_TO_EAST, 4, 2, Terrain.BRIDGE);
        world.bridge = LocationAt(LocationID.WEST_BRIDGE_NORTH_OF_SARIA, 4, 3, Terrain.BRIDGE);

        world.PlaceConnectors(Direction.WEST, Direction.EAST).Should().Be((false, "Bridge"));

        world.raft.CanShuffle.Should().BeFalse();
        world.raft.Xpos.Should().Be(1);
        world.map[world.raft.Pos].Should().Be(Terrain.BRIDGE);
        world.bridge.CanShuffle.Should().BeTrue();
        world.bridge.Pos.Should().Be(new IntVector2(4, 3));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void TryPlaceConnectors_PlacesLoadedConnectorsOnTheirAssignedShores(bool includeRaft)
    {
        var world = ConnectorShoreline(westWater: true, eastWater: true);
        if (includeRaft) world.raft = LocationAt(LocationID.WEST_RAFT_TO_EAST, 4, 2, Terrain.BRIDGE);
        world.bridge = LocationAt(LocationID.WEST_BRIDGE_NORTH_OF_SARIA, 4, 3, Terrain.BRIDGE);

        world.PlaceConnectors(Direction.WEST, Direction.EAST).Should().Be((true, (string?)null));

        world.bridge.CanShuffle.Should().BeFalse();
        world.bridge.Xpos.Should().Be(7);
        world.map[world.bridge.Pos].Should().Be(Terrain.BRIDGE);
        if (includeRaft)
        {
            world.raft!.CanShuffle.Should().BeFalse();
            world.raft.Xpos.Should().Be(1);
            world.map[world.raft.Pos].Should().Be(Terrain.BRIDGE);
        }
    }

    private static TestWorld ConnectorShoreline(bool westWater, bool eastWater)
    {
        var world = new TestWorld(5, 9, Terrain.GRASS);
        for (int y = 0; y < 5; y++)
        {
            if (westWater) world.map[y, 0] = world.map[y, 1] = Terrain.WATER;
            if (eastWater) world.map[y, 7] = world.map[y, 8] = Terrain.WATER;
        }
        return world;
    }

    private static TestWorld Corridor(Terrain gate, out Location start, out Location target)
    {
        var world = new TestWorld(3, 5);
        start = LocationAt(LocationID.WEST_NORTH_PALACE, 1, 1);
        target = LocationAt(LocationID.WEST_GRASS, 3, 1);
        world.AddLocation(start);
        world.AddLocation(target);
        world.map[start.Pos] = Terrain.GRASS;
        world.map[1, 2] = gate;
        world.map[target.Pos] = Terrain.GRASS;
        return world;
    }

    private static Location LocationAt(LocationID id, int x, int y, Terrain terrain = Terrain.GRASS)
        => new(id, y + 30, x, 0, id.GetContinent()) { TerrainType = terrain, AppearsOnMap = true };

    // A hand-authored map with explicit roots: all reachable fixture locations.
    // All traversal, validation, placement, and encoding algorithms remain production code.
    internal sealed class TestWorld : World
    {
        internal Func<IntVector2, Terrain, bool>? RiverReservation { get; set; }
        internal void SeedRiver(bool boots) => DrawRiver(boots);
        internal bool SeedOcean(Direction edge, Terrain water) => DrawOcean(edge, water);
        protected override bool IsReserved(IntVector2 pos, Terrain terrain)
            => RiverReservation?.Invoke(pos, terrain) ?? base.IsReserved(pos, terrain);

        internal void UsePlacementClimate(Climate placementClimate, IEnumerable<Terrain> allowed)
        {
            climate = placementClimate;
            walkableTerrains = allowed.ToList();
        }
        internal void KeepOnlyPlacementTerrain(Terrain terrain)
        {
            walkableTerrains.Clear();
            walkableTerrains.Add(terrain);
        }
        internal bool PlaceWithHiddenLocations(Location kasuto, Location palace)
            => PlaceLocations(Terrain.WATER, false, kasuto, palace);
        internal void SeedCanyon(Terrain river, Climate bankClimate, IEnumerable<Terrain> banks, bool horizontal)
            => DrawCanyon(river, bankClimate, banks, horizontal);
        internal void SeedCentralMountain(bool horizontal) => DrawCenterMountain(horizontal);
        internal bool GrowWithClimate(Climate growthClimate) => GrowTerrain(growthClimate);
        internal void SeedTerrain(Climate seedClimate, IEnumerable<Terrain> allowed, int maximum = 500)
            => PlaceRandomTerrain(seedClimate, allowed, maximum);
        internal bool RepairAndValidateCaves() => TryRepairAndValidateCaveExits();

        internal RegionCrossingResult PlaceCrossings(int limit, bool saria = false,
            bool longBridge = false, bool darunia = false, bool riverDevil = false, bool rock = false,
            RegionCrossingDeadZone? deadZone = null)
        {
            return PlaceCrossings(new RegionCrossingOptions
            {
                MaximumCrossings = limit,
                CrossingTerrain = Terrain.WATER,
                Biome = Biome.VANILLALIKE,
                CanWalkOnWaterWithBoots = false,
                Features = (saria ? CrossingFeatures.Saria : CrossingFeatures.None)
                    | (longBridge ? CrossingFeatures.LongBridge : CrossingFeatures.None)
                    | (darunia ? CrossingFeatures.DaruniaEncounters : CrossingFeatures.None),
                RiverDevilBlocker = riverDevil,
                RockBlocker = rock,
                DeadZone = deadZone
            });
        }
        internal RegionCrossingResult PlaceCrossings(RegionCrossingOptions options)
        {
            UseClassicClimate();
            return PlaceRegionCrossings(options);
        }
        internal void SeedRandom(int seed) => RNG = new Random(seed);
        internal void AddCavePair(Location first, Location second)
        {
            AddLocation(first);
            AddLocation(second);
            connections[first] = second;
            connections[second] = first;
        }
        internal bool PlaceCavePair(Location first, Terrain crossing) => PlaceSaneCave(first, crossing);
        internal bool PlaceCalderaPassage(bool horizontal, int side, IntVector2 center, Location left, Location right)
            => horizontal ? HorizontalCave(side, center.X, center.Y, left, right)
                : VerticalCave(side, center.X, center.Y, left, right);
        internal bool ReserveContinentConnectors(RandomizerProperties props, BiomeTerrainProfile profile)
        {
            var (raftDirection, bridgeDirection) = PrepareConnectorShores(props, profile);
            return TryReserveContinentConnectors(profile, raftDirection, bridgeDirection);
        }
        internal bool FinalizeContinentConnectors() => TryFinalizeContinentConnectors();
        internal bool Reserved(IntVector2 position) => IsContinentConnectorReserved(position);
        internal bool GrowClassicTerrain() => GrowTerrain(Climates.Create(Continent.WEST, ClimateEnum.CLASSIC));
        internal void AddMinorLocation(Location location)
        {
            AddLocation(location);
            unimportantLocs.Add(location);
        }
        internal void RemoveWorldLocations(params Location[] locations) => RemoveLocations(locations);
        internal bool StageTerrainMap() => TryStageTerrainMap();
        public (bool success, string? failedConnector) PlaceConnectors(Direction raftDirection, Direction bridgeDirection)
        {
            bool success = TryPlaceConnectors(raftDirection, bridgeDirection, out var failure);
            return (success, failure?.ToString());
        }
        public bool PlaceOrdinaryLocations() => PlaceLocations(Terrain.WATER, false);
        public void UseClassicClimate()
        {
            climate = Climates.Create(Continent.WEST, ClimateEnum.CLASSIC);
            walkableTerrains = [Terrain.GRASS, Terrain.FOREST, Terrain.DESERT];
        }
        public int NextRandom() => RNG.Next();
        public void StartGeneratedMap() => InitializeGeneratedMap();
        public void PaintBorder(Terrain terrain) => FillMapBorder(terrain);
        public void SeedStripes(Terrain terrain, int nearEdgeMargin, int farEdgeMargin)
            => DrawTerrainStripes(terrain, nearEdgeMargin, farEdgeMargin);
        public void ResetPlacement() => PrepareTerraformLocations();
        public void PaintLocations(bool markPassthrough) => RepaintLocationTiles(markPassthrough);
        public void InitializeGrid() => InitializeVisitation();
        public bool TileVisited(IntVector2 pos) => visitation[pos.Y, pos.X];
        public void MarkTileVisited(IntVector2 pos) => visitation[pos.Y, pos.X] = true;
        public (int, int) GridDimensions => (visitation.GetLength(0), visitation.GetLength(1));

        public TestWorld(int rows, int columns, Terrain fill = Terrain.MOUNTAIN,
            int mapRomOffset = 0, int mapRomCapacity = RomMap.OVERWORLD_MAP_CAPACITY)
            : base(new Random(42), mapRomOffset, mapRomCapacity)
        {
            MapRows = rows;
            MapColumns = columns;
            map = new OverworldMap(rows, columns);
            visitation = new bool[rows, columns];
            walkableTerrains = [Terrain.GRASS];
            for (var y = 0; y < rows; y++)
                for (var x = 0; x < columns; x++)
                    map[y, x] = fill;
        }

        internal EncodedWorldMap EncodeMap(MapEncodingOptions options = default) => EncodeTerrainMap(options);

        internal bool StageEncodedMap(EncodedWorldMap encodedMap) => TryStageEncodedTerrainMap(encodedMap);

        protected override List<Location> GetPathingStarts() => AllLocations.Where(location => location.Reachable).ToList();
        public override bool Terraform(RandomizerProperties props, ROM rom) => throw new NotSupportedException();
        public override void DisableDisallowedPassthroughs() => throw new NotSupportedException();
        public override void ResetCollectables(RandomizerProperties props) => throw new NotSupportedException();
        public override string GetName() => "Characterization fixture";
        public override string GenerateSpoiler() => throw new NotSupportedException();
        public override IEnumerable<Location> RequiredLocations(bool hiddenPalace, bool hiddenKasuto) => AllLocations;
    }
}
