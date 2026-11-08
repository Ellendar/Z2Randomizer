using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using static Z2Randomizer.Tests.WorldCharacterizationTests;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Z2Randomizer.Tests;

[TestClass]
public class ContinentConnectorTests
{
    [TestMethod]
    [DataRow(Direction.NORTH, false)]
    [DataRow(Direction.EAST, false)]
    [DataRow(Direction.SOUTH, false)]
    [DataRow(Direction.WEST, false)]
    [DataRow(Direction.NORTH, true)]
    [DataRow(Direction.EAST, true)]
    [DataRow(Direction.SOUTH, true)]
    [DataRow(Direction.WEST, true)]
    public void LegacyPlacementUsesTheLastWaterTileAndPreservesTheLandApproach(Direction edge, bool raft)
    {
        var world = new TestWorld(9, 9, Terrain.GRASS);
        var inward = -edge.ToIntVector2();
        var start = EdgeCenter(edge, 9);
        for (int depth = 0; depth < 2; depth++) world.map[start + depth * inward] = Terrain.WATER;
        var connector = Connector(raft);
        bool success = raft
            ? World.DrawRaft(new Random(42), world.map, connector, [Terrain.GRASS], edge)
            : World.DrawBridge(new Random(42), world.map, connector, [Terrain.GRASS], edge);
        success.Should().BeTrue();
        connector.Pos.Should().Be(start + inward);
        world.map[start + 2 * inward].Should().Be(Terrain.GRASS);
        OverworldSemanticAssertions.ContinentConnector(world, connector, raft);
    }

    [TestMethod]
    [DataRow(Direction.NORTH, false)]
    [DataRow(Direction.EAST, false)]
    [DataRow(Direction.SOUTH, false)]
    [DataRow(Direction.WEST, false)]
    [DataRow(Direction.NORTH, true)]
    [DataRow(Direction.EAST, true)]
    [DataRow(Direction.SOUTH, true)]
    [DataRow(Direction.WEST, true)]
    public void ReservedShoreSurvivesOrdinaryPlacementAndGrowth(Direction edge, bool raft)
    {
        var world = new TestWorld(15, 15, Terrain.NONE);
        world.UseClassicClimate();
        var connector = Connector(raft);
        var reservation = ContinentConnectorReservation.Plan(new Random(42), world.map, world.biome,
            raft ? World.ConnectorKind.Raft : World.ConnectorKind.Bridge, edge, [Terrain.GRASS], [], [])!;
        reservation.Should().NotBeNull();
        connector.Pos.Should().Be(IntVector2.NOT_ON_MAP, "planning does not publish coordinates");
        reservation.Seed(world.map, Terrain.PREPLACED_WATER, Terrain.GRASS);
        var ordinary = new Location(LocationID.WEST_GRASS, 0, 0, 0, Continent.WEST)
            { TerrainType = Terrain.GRASS, AppearsOnMap = true };
        world.AddLocation(ordinary);
        world.PlaceOrdinaryLocations().Should().BeTrue();
        reservation.Contains(ordinary.Pos).Should().BeFalse();
        world.GrowClassicTerrain().Should().BeTrue();
        reservation.IsIntact(world.map, false).Should().BeTrue();
        reservation.Finalize(world.map, connector);
        connector.Pos.Should().Be(reservation.Landing);
        reservation.IsIntact(world.map, true).Should().BeTrue();
        OverworldSemanticAssertions.ContinentConnector(world, connector, raft);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public void OptionalConnectorsAreFinalizedOnDistinctAllowedEdges(int loaded)
    {
        var world = new TestWorld(15, 15, Terrain.NONE) { continentId = Continent.WEST, biome = Biome.CANYON };
        world.UseClassicClimate();
        if ((loaded & 1) != 0) world.raft = Connector(true);
        if ((loaded & 2) != 0) world.bridge = Connector(false);
        var profile = Profile(horizontal: true);
        world.ReserveContinentConnectors(new RandomizerProperties(), profile).Should().BeTrue();
        if (loaded == 0) world.NextRandom().Should().Be(new Random(42).Next());
        world.map[7, 7] = Terrain.GRASS;
        world.GrowClassicTerrain().Should().BeTrue();
        world.FinalizeContinentConnectors().Should().BeTrue();
        OverworldSemanticAssertions.ContinentConnector(world, world.raft, true);
        OverworldSemanticAssertions.ContinentConnector(world, world.bridge, false);
        if (world.raft != null) world.raft.Xpos.Should().BeOneOf(1, 13);
        if (world.bridge != null)
            (world.bridge.Xpos is 1 or 13 || world.bridge.Y is 1 or 13).Should().BeTrue();
        if (loaded == 3) world.raft!.Xpos.Should().NotBe(world.bridge!.Xpos);
    }

    [TestMethod]
    public void ABridgeWithoutARaftCanUseTheOnlyUsableShore()
    {
        for (int seed = 0; seed < 16; seed++)
        {
            var world = new TestWorld(15, 15, Terrain.MOUNTAIN) { continentId = Continent.WEST, biome = Biome.CANYON };
            world.UseClassicClimate();
            world.SeedRandom(seed);
            for (int y = 4; y <= 10; y++)
            {
                world.map[y, 0] = world.map[y, 1] = Terrain.WATER;
                world.map[y, 2] = world.map[y, 3] = Terrain.GRASS;
            }
            world.bridge = Connector(false);

            world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeTrue();
            world.FinalizeContinentConnectors().Should().BeTrue();
            world.bridge.Xpos.Should().Be(1);
            OverworldSemanticAssertions.ContinentConnector(world, world.bridge, false);
        }
    }

    [TestMethod]
    public void AMapWithOnlyOneUsableShoreRejectsTwoConnectorsWithoutSharingIt()
    {
        for (int seed = 0; seed < 16; seed++)
        {
            var world = new TestWorld(15, 15, Terrain.MOUNTAIN) { continentId = Continent.WEST, biome = Biome.CANYON };
            world.UseClassicClimate();
            world.SeedRandom(seed);
            for (int y = 4; y <= 10; y++)
            {
                world.map[y, 0] = world.map[y, 1] = Terrain.WATER;
                world.map[y, 2] = world.map[y, 3] = Terrain.GRASS;
            }
            var before = world.map.Clone();
            world.raft = Connector(true);
            world.bridge = Connector(false);

            world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeFalse();

            for (int y = 0; y < 15; y++)
            for (int x = 0; x < 15; x++) world.map[y, x].Should().Be(before[y, x]);
            world.raft.Pos.Should().Be(IntVector2.NOT_ON_MAP);
            world.bridge.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        }
    }

    [TestMethod]
    public void BlockedPerpendicularShoresDoNotPreventUsingAnOpenOppositeShore()
    {
        for (int seed = 0; seed < 32; seed++)
        {
            var world = new TestWorld(15, 15, Terrain.NONE) { continentId = Continent.WEST, biome = Biome.CANYON };
            world.UseClassicClimate();
            world.SeedRandom(seed);
            for (int x = 0; x < 15; x++)
            for (int depth = 0; depth < 3; depth++)
                world.map[depth, x] = world.map[14 - depth, x] = Terrain.MOUNTAIN;
            world.raft = Connector(true);
            world.bridge = Connector(false);

            world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeTrue();
            world.FinalizeContinentConnectors().Should().BeTrue();

            world.raft.Xpos.Should().BeOneOf(1, 13);
            world.bridge.Xpos.Should().BeOneOf(1, 13);
            world.bridge.Xpos.Should().NotBe(world.raft.Xpos);
            for (int x = 0; x < 15; x++)
            for (int depth = 0; depth < 3; depth++)
            {
                world.map[depth, x].Should().Be(Terrain.MOUNTAIN);
                world.map[14 - depth, x].Should().Be(Terrain.MOUNTAIN);
            }
        }
    }

    [TestMethod]
    public void OpenShoresCanGenerateBothOppositeAndPerpendicularConnectorPairs()
    {
        int opposite = 0, perpendicular = 0;
        for (int seed = 0; seed < 32; seed++)
        {
            var world = new TestWorld(15, 15, Terrain.NONE) { continentId = Continent.WEST, biome = Biome.CANYON };
            world.UseClassicClimate();
            world.SeedRandom(seed);
            world.raft = Connector(true);
            world.bridge = Connector(false);
            world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeTrue();
            world.map[7, 7] = Terrain.GRASS;
            world.GrowClassicTerrain().Should().BeTrue();
            world.FinalizeContinentConnectors().Should().BeTrue();
            OverworldSemanticAssertions.ContinentConnector(world, world.raft, true);
            OverworldSemanticAssertions.ContinentConnector(world, world.bridge, false);

            // On this empty map the seeded routes are two tiles deep, so their
            // landings identify shores unambiguously without inspecting the policy.
            world.raft.Xpos.Should().BeOneOf(1, 13);
            world.bridge.Xpos.Should().NotBe(world.raft.Xpos);
            if (world.bridge.Xpos is 1 or 13) opposite++;
            else
            {
                world.bridge.Y.Should().BeOneOf(1, 13);
                perpendicular++;
            }
        }
        opposite.Should().BeGreaterThan(0);
        perpendicular.Should().BeGreaterThan(0);
    }

    [TestMethod]
    [DataRow(4, 4, false)]
    [DataRow(5, 5, true)]
    [DataRow(5, 9, true)]
    public void NarrowMapsHaveABoundedReservationResult(int rows, int columns, bool expected)
    {
        var map = new OverworldMap(rows, columns, Terrain.NONE);
        var reservation = ContinentConnectorReservation.Plan(new Random(42), map, Biome.VANILLALIKE, World.ConnectorKind.Bridge,
            Direction.WEST, [Terrain.GRASS], [], []);
        (reservation is not null).Should().Be(expected);
    }

    [TestMethod]
    public void AnObstructedShoreFailsWithoutRepaintingItsFeatures()
    {
        var world = new TestWorld(9, 9, Terrain.MOUNTAIN) { continentId = Continent.WEST, biome = Biome.CANYON };
        var connector = Connector(true);
        world.raft = connector;
        world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeFalse();
        connector.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        Enumerable.Range(0, 9).SelectMany(y => Enumerable.Range(0, 9).Select(x => world.map[y, x]))
            .Should().OnlyContain(t => t == Terrain.MOUNTAIN);
    }

    [TestMethod]
    public void OrdinaryBiomesCannotOpenAMountainCoastDuringPlanning()
    {
        var map = new OverworldMap(5, 9, Terrain.NONE);
        for (int y = 0; y < 5; y++) map[y, 0] = Terrain.MOUNTAIN;

        ContinentConnectorReservation.CanPlan(map, Biome.VANILLALIKE, World.ConnectorKind.Bridge,
            Direction.WEST, [Terrain.GRASS], [], []).Should().BeFalse();
        ContinentConnectorReservation.Plan(new Random(42), map, Biome.VANILLALIKE, World.ConnectorKind.Bridge,
            Direction.WEST, [Terrain.GRASS], [], []).Should().BeNull();
        for (int y = 0; y < 5; y++) map[y, 0].Should().Be(Terrain.MOUNTAIN);
    }

    [TestMethod]
    [DataRow(Biome.MOUNTAINOUS)]
    [DataRow(Biome.CALDERA)]
    public void MountainCoastsMayOpenTheirPerimeterButNeverTunnelThroughAnInteriorFeature(Biome biome)
    {
        var map = new OverworldMap(5, 9, Terrain.NONE);
        for (int y = 0; y < 5; y++) map[y, 0] = Terrain.MOUNTAIN;
        ContinentConnectorReservation.CanPlan(map, biome, World.ConnectorKind.Bridge,
            Direction.WEST, [Terrain.GRASS], [], []).Should().BeTrue();
        var route = ContinentConnectorReservation.Plan(new Random(42), map, biome, World.ConnectorKind.Bridge, Direction.WEST,
            [Terrain.GRASS], [], []);
        route.Should().NotBeNull();
        route!.Seed(map, Terrain.PREPLACED_WATER, Terrain.GRASS);
        map[2, 0].Should().Be(Terrain.PREPLACED_WATER);
        map[1, 0].Should().Be(Terrain.MOUNTAIN);
        var blocked = new OverworldMap(5, 9, Terrain.NONE);
        for (int y = 0; y < 5; y++) blocked[y, 0] = blocked[y, 1] = Terrain.MOUNTAIN;
        ContinentConnectorReservation.Plan(new Random(42), blocked, biome, World.ConnectorKind.Bridge, Direction.WEST,
            [Terrain.GRASS], [], []).Should().BeNull();
    }

    [TestMethod]
    public void ReservationsCannotOverlapExistingLocationsOrEachOther()
    {
        var map = new OverworldMap(5, 8, Terrain.NONE);
        // This narrow shore offers one apron. Occupying it must reject, even if
        // the location's terrain has not been painted yet.
        ContinentConnectorReservation.Plan(new Random(42), map, Biome.VANILLALIKE, World.ConnectorKind.Raft, Direction.WEST,
            [Terrain.GRASS], [new IntVector2(2, 2)], []).Should().BeNull();
        var first = ContinentConnectorReservation.Plan(new Random(42), map, Biome.VANILLALIKE, World.ConnectorKind.Raft,
            Direction.WEST, [Terrain.GRASS], [], [])!;
        ContinentConnectorReservation.Plan(new Random(42), map, Biome.VANILLALIKE, World.ConnectorKind.Bridge, Direction.WEST,
            [Terrain.GRASS], [], [first]).Should().BeNull();
    }

    [TestMethod]
    public void LateFeatureConflictsRejectInsteadOfMovingOrRepaintingTheLanding()
    {
        var world = new TestWorld(9, 9, Terrain.NONE) { continentId = Continent.WEST, biome = Biome.CANYON };
        world.UseClassicClimate();
        world.raft = Connector(true);
        world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeTrue();
        var reserved = Enumerable.Range(0, 9).SelectMany(y => Enumerable.Range(0, 9).Select(x => new IntVector2(x, y)))
            .First(p => world.Reserved(p) && !world.map[p].IsWater());
        world.map[reserved] = Terrain.ROCK;
        world.FinalizeContinentConnectors().Should().BeFalse();
        world.raft.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        world.map[reserved].Should().Be(Terrain.ROCK);
    }

    [TestMethod]
    public void HiddenMinorLocationsCannotOccupyAReservedApproach()
    {
        var world = new TestWorld(9, 9, Terrain.NONE) { continentId = Continent.WEST, biome = Biome.CANYON };
        world.UseClassicClimate();
        world.raft = Connector(true);
        world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeTrue();
        var land = Enumerable.Range(0, 9).SelectMany(y => Enumerable.Range(0, 9).Select(x => new IntVector2(x, y)))
            .First(p => world.Reserved(p) && !world.map[p].IsWater());
        var minor = new Location(LocationID.WEST_GRASS, 0, 0, 0, Continent.WEST)
            { TerrainType = world.map[land], AppearsOnMap = true };
        world.AddMinorLocation(minor);
        // The only tiles of this terrain are reserved. Hiding must leave the
        // minor location off-map, rather than consuming the connector approach.
        world.PlaceHiddenLocations(LessImportantLocationsOption.HIDE);
        minor.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        world.FinalizeContinentConnectors().Should().BeTrue();
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void EastShoreConnectorKeepsWaterBeyondTheLogicalMap(bool raft)
    {
        var world = new TestWorld(15, 15, Terrain.MOUNTAIN) { continentId = Continent.WEST, biome = Biome.VANILLALIKE };
        world.UseClassicClimate();
        var connector = Connector(raft);
        if (raft) world.raft = connector;
        else world.bridge = connector;
        world.AddLocation(connector);

        // Leave the top-right corner mountainous and open a single inlet on the east.
        world.map[7, 13] = world.map[7, 14] = Terrain.WALKABLEWATER;
        for (int y = 6; y <= 8; y++)
            world.map[y, 11] = world.map[y, 12] = Terrain.GRASS;
        var profile = BiomeTerrainProfile.Resolve(Continent.WEST, world.biome, true,
            Climates.Create(Continent.WEST, ClimateEnum.CLASSIC), true);
        var rom = new ROM(new byte[1400]);

        using (var candidate = world.BeginTerrainCandidate(rom))
        {
            var props = new RandomizerProperties { ContinentConnections = ContinentConnectionType.NORMAL };
            world.ReserveContinentConnectors(props, profile).Should().BeTrue();
            world.FinalizeContinentConnectors().Should().BeTrue();
            connector.Pos.Should().Be(new IntVector2(13, 7));
            world.StageTerrainMap().Should().BeTrue();
            candidate.Commit();
        }

        var (tiles, _) = OverworldSemanticAssertions.Decode(rom.rawdata, world.MapRows);
        tiles[0, 14].Should().Be((byte)Terrain.MOUNTAIN, "the logical corner stays unchanged");
        tiles[7, 14].Should().Be((byte)(raft ? Terrain.WALKABLEWATER : Terrain.BRIDGE));
        for (int x = world.MapColumns; x < 64; x++)
            tiles[7, x].Should().Be((byte)Terrain.WATER, "the connector needs water beyond the logical east edge");
    }


    [TestMethod]
    public void FinalStagingRejectsALateConflictWithoutWritingTheRom()
    {
        var world = new TestWorld(9, 9, Terrain.NONE) { continentId = Continent.WEST, biome = Biome.CANYON };
        world.UseClassicClimate();
        world.raft = Connector(true);
        world.AddLocation(world.raft);
        var rom = new ROM(Enumerable.Repeat((byte)0xCC, 2048).ToArray());
        using (world.BeginTerrainCandidate(rom))
        {
            world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeTrue();
            world.StageTerrainMap().Should().BeFalse("locations must be finalized first");
            world.GrowClassicTerrain();
            world.FinalizeContinentConnectors().Should().BeTrue();
            world.StageTerrainMap().Should().BeTrue();
            world.map[world.raft.Pos] = Terrain.ROCK;
            world.StageTerrainMap().Should().BeFalse();
        }
        rom.rawdata.Should().OnlyContain(b => b == 0xCC);
        world.raft.Pos.Should().Be(IntVector2.NOT_ON_MAP);
    }

    [TestMethod]
    public void BridgesKeepTheirLengthLimitWhileRaftsCanCrossAWideSea()
    {
        var map = new OverworldMap(9, 20, Terrain.NONE);
        for (int y = 0; y < 9; y++) for (int x = 0; x < 11; x++) map[y, x] = Terrain.WATER;
        ContinentConnectorReservation.Plan(new Random(42), map, Biome.VANILLALIKE, World.ConnectorKind.Bridge,
            Direction.WEST, [Terrain.GRASS], [], []).Should().BeNull();
        ContinentConnectorReservation.Plan(new Random(42), map, Biome.VANILLALIKE, World.ConnectorKind.Raft,
            Direction.WEST, [Terrain.GRASS], [], []).Should().NotBeNull();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WaterRequirementsAndConnectorMetadataSurviveFinalization(bool boots)
    {
        var world = new TestWorld(9, 9, Terrain.NONE);
        var connector = Connector(true);
        connector.IsExternalWorld = true;
        connector.EntranceNumber = 2;
        connector.AccessRequirements = new Requirements([RequirementType.HAMMER]);
        var reservation = ContinentConnectorReservation.Plan(new Random(42), world.map, world.biome, World.ConnectorKind.Raft,
            Direction.WEST, [Terrain.GRASS], [], [])!;
        reservation.Seed(world.map, boots ? Terrain.PREPLACED_WATER_WALKABLE : Terrain.PREPLACED_WATER, Terrain.GRASS);
        reservation.Finalize(world.map, connector);
        world.map[connector.Pos + IntVector2.WEST].Should().Be(boots ? Terrain.PREPLACED_WATER_WALKABLE : Terrain.PREPLACED_WATER);
        connector.IsExternalWorld.Should().BeTrue();
        connector.EntranceNumber.Should().Be(2);
        connector.AccessRequirements.HasHardRequirement(RequirementType.HAMMER).Should().BeTrue();
        connector.CanShuffle.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(Continent.WEST, Direction.EAST)]
    [DataRow(Continent.EAST, Direction.WEST)]
    public void NormalTravelKeepsTheOriginalRaftShore(Continent continent, Direction expected)
    {
        var policy = ContinentConnectorOrientationPolicy.Resolve(continent, Biome.VANILLALIKE, false, ContinentConnectionType.NORMAL);
        policy.RaftEdges.Should().Equal(expected);
        policy.BridgeEdges.Should().BeEquivalentTo(DirectionExtensions.CARDINAL_DIRECTIONS);
    }

    [TestMethod]
    [DataRow(ContinentConnectionType.TRANSPORTATION_SHUFFLE)]
    [DataRow(ContinentConnectionType.ANYTHING_GOES)]
    public void ShuffledTravelAllowsAllRaftShoresOutsideRestrictedBiomes(ContinentConnectionType connections)
    {
        foreach (var continent in new[] { Continent.WEST, Continent.EAST, Continent.DM })
            ContinentConnectorOrientationPolicy.Resolve(continent, Biome.VANILLALIKE, true, connections).RaftEdges
                .Should().BeEquivalentTo(DirectionExtensions.CARDINAL_DIRECTIONS);
    }

    [TestMethod]
    [DataRow(Continent.WEST)]
    [DataRow(Continent.EAST)]
    public void CoastalNormalTravelUsesTheSeaFacingEdge(Continent continent)
    {
        ContinentConnectorOrientationPolicy.Resolve(continent, Biome.COASTAL_ISLANDS, true, ContinentConnectionType.NORMAL)
            .RaftEdges.Should().Equal(Direction.SOUTH);
        ContinentConnectorOrientationPolicy.Resolve(continent, Biome.COASTAL_ISLANDS, false, ContinentConnectionType.NORMAL)
            .RaftEdges.Should().Equal(Direction.EAST);

        foreach (var connections in new[] { ContinentConnectionType.TRANSPORTATION_SHUFFLE, ContinentConnectionType.ANYTHING_GOES })
            ContinentConnectorOrientationPolicy.Resolve(continent, Biome.COASTAL_ISLANDS, true, connections)
                .RaftEdges.Should().BeEquivalentTo(DirectionExtensions.CARDINAL_DIRECTIONS);

        ContinentConnectorOrientationPolicy.Resolve(Continent.DM, Biome.COASTAL_ISLANDS, false, ContinentConnectionType.NORMAL)
            .RaftEdges.Should().BeEquivalentTo(DirectionExtensions.CARDINAL_DIRECTIONS);
    }

    [TestMethod]
    [DataRow(Biome.CANYON, false)]
    [DataRow(Biome.CANYON, true)]
    [DataRow(Biome.DRY_CANYON, false)]
    [DataRow(Biome.DRY_CANYON, true)]
    public void CanyonRaftsUseTheBiomeAxisWhileBridgesRetainPerpendicularChoices(Biome biome, bool horizontal)
    {
        Direction[] axis = horizontal ? [Direction.EAST, Direction.WEST] : [Direction.NORTH, Direction.SOUTH];
        foreach (var continent in new[] { Continent.WEST, Continent.EAST, Continent.DM })
        {
            var policy = ContinentConnectorOrientationPolicy.Resolve(continent, biome, horizontal, ContinentConnectionType.ANYTHING_GOES);
            policy.RaftEdges.Should().BeEquivalentTo(axis);
            policy.BridgeEdges.Should().BeEquivalentTo(DirectionExtensions.CARDINAL_DIRECTIONS);
        }
    }

    [TestMethod]
    [DataRow(Continent.WEST, Biome.CALDERA)]
    [DataRow(Continent.EAST, Biome.VOLCANO)]
    [DataRow(Continent.DM, Biome.CALDERA)]
    public void CenteredBiomesKeepRaftAxesWithoutForcingTheBridgeOpposite(Continent continent, Biome biome)
    {
        foreach (bool horizontal in new[] { false, true })
        foreach (var connections in Enum.GetValues<ContinentConnectionType>())
        {
            Direction[] axis = horizontal ? [Direction.WEST, Direction.EAST] : [Direction.NORTH, Direction.SOUTH];
            var policy = ContinentConnectorOrientationPolicy.Resolve(continent, biome, horizontal, connections);
            policy.RaftEdges.Should().Equal(axis);
            policy.BridgeEdges.Should().BeEquivalentTo(DirectionExtensions.CARDINAL_DIRECTIONS);
        }
    }

    [TestMethod]
    public void UnrestrictedConnectorChoicesShareAClockwiseSamplingOrder()
    {
        Direction[] clockwise = [Direction.NORTH, Direction.EAST, Direction.SOUTH, Direction.WEST];
        foreach (var continent in new[] { Continent.WEST, Continent.EAST, Continent.DM })
        foreach (var connections in Enum.GetValues<ContinentConnectionType>())
        {
            var policy = ContinentConnectorOrientationPolicy.Resolve(continent, Biome.VANILLALIKE, false, connections);
            policy.BridgeEdges.Should().Equal(clockwise);
            if (continent == Continent.DM || connections != ContinentConnectionType.NORMAL)
                policy.RaftEdges.Should().Equal(clockwise);
        }
    }

    [TestMethod]
    public void EveryRaftEdgeLeavesADistinctBridgeChoice()
    {
        foreach (var continent in new[] { Continent.WEST, Continent.EAST, Continent.DM })
        foreach (var biome in new[] { Biome.VANILLALIKE, Biome.ISLANDS, Biome.MOUNTAINOUS, Biome.CANYON, Biome.DRY_CANYON,
            continent == Continent.EAST ? Biome.VOLCANO : Biome.CALDERA })
        foreach (bool horizontal in new[] { false, true })
        foreach (var connections in Enum.GetValues<ContinentConnectionType>())
        {
            var policy = ContinentConnectorOrientationPolicy.Resolve(continent, biome, horizontal, connections);
            policy.RaftEdges.Should().NotBeEmpty();
            foreach (var raftEdge in policy.RaftEdges)
                policy.BridgeEdges.Should().Contain(edge => edge != raftEdge, "shore selection must be able to finish");
        }
    }

    [TestMethod]
    [DataRow(Biome.CANYON)]
    [DataRow(Biome.DRY_CANYON)]
    [DataRow(Biome.CALDERA)]
    [DataRow(Biome.VOLCANO)]
    public void ConstrainedRaftsAllowBothOppositeAndPerpendicularBridgeAlternatives(Biome biome)
    {
        var continent = biome == Biome.VOLCANO ? Continent.EAST : Continent.WEST;
        foreach (bool horizontal in new[] { false, true })
        {
            var policy = ContinentConnectorOrientationPolicy.Resolve(continent, biome, horizontal, ContinentConnectionType.NORMAL);
            foreach (var raft in policy.RaftEdges)
            {
                var distinct = policy.BridgeEdges.Where(edge => edge != raft).ToArray();
                distinct.Should().Contain(edge => edge.ToIntVector2() == -raft.ToIntVector2());
                distinct.Should().Contain(edge => edge.IsHorizontal() != raft.IsHorizontal());
            }
        }
    }

    [TestMethod]
    public void RetryingAnAttemptDropsOldReservationsWithoutRewindingRandom()
    {
        var world = new TestWorld(15, 15, Terrain.NONE) { continentId = Continent.WEST, biome = Biome.CANYON };
        world.raft = Connector(true);
        world.AddLocation(world.raft);
        using (world.BeginGeneratedTerrainAttempt(Profile(true)))
        {
            world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeTrue();
            world.GrowClassicTerrain();
            world.FinalizeContinentConnectors().Should().BeTrue();
        }
        var oldLanding = world.raft.Pos;
        using (world.BeginGeneratedTerrainAttempt(Profile(true)))
        {
            world.StartGeneratedMap();
            world.Reserved(oldLanding).Should().BeFalse();
            world.raft.Pos.Should().Be(IntVector2.NOT_ON_MAP);
            world.ReserveContinentConnectors(new RandomizerProperties(), Profile(true)).Should().BeTrue();
            world.GrowClassicTerrain();
            world.FinalizeContinentConnectors().Should().BeTrue();
            OverworldSemanticAssertions.ContinentConnector(world, world.raft, true);
        }
    }

    private static BiomeTerrainProfile Profile(bool horizontal) => BiomeTerrainProfile.Resolve(Continent.WEST,
        Biome.CANYON, false, Climates.Create(Continent.WEST, ClimateEnum.CLASSIC), horizontal);
    private static Location Connector(bool raft) => new(raft ? LocationID.WEST_RAFT_TO_EAST : LocationID.WEST_BRIDGE_NORTH_OF_SARIA,
        0, 0, 0, Continent.WEST) { TerrainType = Terrain.BRIDGE, AppearsOnMap = true };
    private static IntVector2 EdgeCenter(Direction edge, int size) => edge switch
    {
        Direction.NORTH => new(size / 2, 0), Direction.SOUTH => new(size / 2, size - 1),
        Direction.WEST => new(0, size / 2), Direction.EAST => new(size - 1, size / 2),
        _ => throw new ArgumentOutOfRangeException(nameof(edge))
    };
}
