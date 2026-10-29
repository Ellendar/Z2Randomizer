using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Random = Z2Randomizer.RandomizerCore.Random;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class LocationPlacementSearchTests
{
    [TestMethod]
    [DataRow(Terrain.GRASS)]
    [DataRow(Terrain.PALACE)]
    public void OrdinaryPlacement_UsesTheOnlyEmptyAreaAndRetainsRequirements(Terrain terrain)
    {
        var world = OrdinaryWorld();
        var location = Add(world, terrain);
        location.AccessRequirements = new Requirements([RequirementType.GLOVE]);

        world.PlaceOrdinaryLocations().Should().BeTrue();

        location.Pos.Should().Be(new IntVector2(3, 3));
        location.CanShuffle.Should().BeFalse();
        world.map[location.Pos].Should().Be(terrain);
        foreach (var direction in IntVector2.DIRECTIONS)
            world.map[location.Pos + direction].Should().NotBe(Terrain.NONE);
        var surroundings = IntVector2.DIRECTIONS.Select(d => world.map[location.Pos + d]).ToArray();
        surroundings.Should().OnlyContain(t => t == surroundings[0]);
        location.AccessRequirements.HasHardRequirement(RequirementType.GLOVE).Should().BeTrue();
    }

    [TestMethod]
    public void OrdinaryPlacement_FailureRetainsEarlierPlacementButNotTheNextLocation()
    {
        var world = OrdinaryWorld();
        var first = Add(world, Terrain.GRASS);
        var second = Add(world, Terrain.PALACE, LocationID.WEST_PALACE1);
        var secondBefore = second.Pos;

        world.PlaceOrdinaryLocations().Should().BeFalse();

        first.Pos.Should().Be(new IntVector2(3, 3));
        first.CanShuffle.Should().BeFalse();
        second.Pos.Should().Be(secondBefore);
        second.CanShuffle.Should().BeTrue();
        world.map[first.Pos].Should().Be(Terrain.GRASS);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void OrdinaryPlacement_HammerRequirementRetainsItsEligibilityOverride(bool hammer)
    {
        var world = OrdinaryWorld();
        var location = Add(world, Terrain.GRASS);
        location.CanShuffle = false;
        if (hammer) location.AccessRequirements = new Requirements([RequirementType.HAMMER]);

        world.PlaceOrdinaryLocations().Should().BeTrue();

        location.Pos.Should().Be(hammer ? new IntVector2(3, 3) : IntVector2.NOT_ON_MAP);
        location.AccessRequirements.HasHardRequirement(RequirementType.HAMMER).Should().Be(hammer);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void HiddenKasuto_CannotUseTheHiddenPalaceColumn(bool alternativeArea)
    {
        var world = new TestWorld(9, 12);
        world.UseClassicClimate();
        EmptyArea(world, new(3, 4));
        if (alternativeArea) EmptyArea(world, new(8, 4));
        var palace = Add(world, Terrain.PALACE, LocationID.EAST_PALACE6);
        palace.Pos = new(3, 1);
        palace.CanShuffle = false;
        var kasuto = Add(world, Terrain.TOWN, LocationID.EAST_TOWN_NEW_KASUTO);

        world.PlaceWithHiddenLocations(kasuto, palace).Should().Be(alternativeArea);

        kasuto.Pos.Should().Be(alternativeArea ? new IntVector2(8, 4) : IntVector2.NOT_ON_MAP);
        palace.Pos.Should().Be(new IntVector2(3, 1));
    }

    [TestMethod]
    public void ShuffleableHiddenPalace_ThrowsBeforePlacementOrRandomness()
    {
        var world = OrdinaryWorld();
        var palace = Add(world, Terrain.PALACE);
        var kasuto = Add(world, Terrain.TOWN, LocationID.EAST_TOWN_NEW_KASUTO);
        var before = Snapshot(world.map);

        Action place = () => world.PlaceWithHiddenLocations(kasuto, palace);

        place.Should().Throw<ImpossibleException>();
        Snapshot(world.map).Should().Equal(before);
        world.NextRandom().Should().Be(new Random(42).Next());
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(2, 2)]
    public void HiddenMinor_UsesMatchingTerrainIncludingMapEdgesWithoutPainting(int x, int y)
    {
        var world = new TestWorld(5, 5);
        var minor = AddMinor(world);
        minor.AppearsOnMap = false;
        minor.AccessRequirements = new Requirements([RequirementType.GLOVE]);
        minor.SetCollectables([Collectable.HAMMER]);
        world.map[y, x] = Terrain.GRASS;
        var before = Snapshot(world.map);

        world.PlaceHiddenLocations(LessImportantLocationsOption.HIDE);

        minor.Pos.Should().Be(new IntVector2(x, y));
        minor.CanShuffle.Should().BeFalse();
        minor.AccessRequirements.HasHardRequirement(RequirementType.GLOVE).Should().BeTrue();
        minor.GetAllCollectables().Should().Equal(Collectable.HAMMER);
        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    [DataRow(Terrain.BRIDGE, 0, -1)]
    [DataRow(Terrain.CAVE, 1, 0)]
    [DataRow(Terrain.ROCK, 0, 1)]
    [DataRow(Terrain.RIVER_DEVIL, -1, 0)]
    public void HiddenMinor_RejectsCardinalNeighborsOfBlockingFeatures(Terrain terrain, int dx, int dy)
    {
        var world = new TestWorld(5, 5);
        var minor = AddMinor(world);
        world.map[2, 2] = Terrain.GRASS;
        var blocker = Add(world, terrain, LocationID.WEST_CAVE_TROPHY);
        blocker.Pos = new(2 + dx, 2 + dy);

        world.PlaceHiddenLocations(LessImportantLocationsOption.HIDE);

        minor.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        minor.CanShuffle.Should().BeFalse();
        blocker.Pos.Should().Be(new IntVector2(2 + dx, 2 + dy));
    }

    [TestMethod]
    [DataRow(Terrain.GRASS)]
    [DataRow(Terrain.TOWN)]
    [DataRow(Terrain.PALACE)]
    public void HiddenMinor_AllowsCardinalNeighborsOfOtherFeatures(Terrain terrain)
    {
        var world = new TestWorld(5, 5);
        var minor = AddMinor(world);
        world.map[2, 2] = Terrain.GRASS;
        var neighbor = Add(world, terrain, LocationID.WEST_CAVE_TROPHY);
        neighbor.Pos = new(3, 2);

        world.PlaceHiddenLocations(LessImportantLocationsOption.HIDE);

        minor.Pos.Should().Be(new IntVector2(2, 2));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void HiddenMinor_RejectsOverlapButAllowsDiagonalCaves(bool diagonal)
    {
        var world = new TestWorld(5, 5);
        var minor = AddMinor(world);
        world.map[2, 2] = Terrain.GRASS;
        var other = Add(world, Terrain.CAVE, LocationID.WEST_CAVE_TROPHY);
        other.Pos = diagonal ? new(3, 3) : new(2, 2);

        world.PlaceHiddenLocations(LessImportantLocationsOption.HIDE);

        minor.Pos.Should().Be(diagonal ? new IntVector2(2, 2) : IntVector2.NOT_ON_MAP);
    }

    [TestMethod]
    public void HiddenMinor_CanRetainItsOwnPositionAndDoesNotConsumeRandomnessAgain()
    {
        var world = new TestWorld(5, 5);
        var minor = AddMinor(world);
        minor.Pos = new(2, 2);
        world.map[2, 2] = Terrain.GRASS;
        world.PlaceHiddenLocations(LessImportantLocationsOption.HIDE);
        minor.Pos.Should().Be(new IntVector2(2, 2));
        world.SeedRandom(42);

        world.PlaceHiddenLocations(LessImportantLocationsOption.HIDE);

        world.NextRandom().Should().Be(new Random(42).Next());
    }

    [TestMethod]
    public void HiddenMinors_ObserveEarlierPlacementsRatherThanSharingTheOnlyTile()
    {
        var world = new TestWorld(5, 5);
        var first = AddMinor(world);
        var second = AddMinor(world, LocationID.WEST_CAVE_PBAG);
        world.map[2, 2] = Terrain.GRASS;

        world.PlaceHiddenLocations(LessImportantLocationsOption.HIDE);

        first.Pos.Should().Be(new IntVector2(2, 2));
        second.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        first.CanShuffle.Should().BeFalse();
        second.CanShuffle.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(LessImportantLocationsOption.HIDE)]
    [DataRow(LessImportantLocationsOption.REMOVE)]
    public void UnplacedMinor_HideFailureAndRemovalBothRetainEntranceMetadata(LessImportantLocationsOption mode)
    {
        var world = new TestWorld(5, 5);
        var minor = AddMinor(world);
        minor.IsExternalWorld = true;
        minor.EntranceNumber = 2;

        world.PlaceHiddenLocations(mode);

        minor.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        minor.CanShuffle.Should().BeFalse();
        minor.IsExternalWorld.Should().BeTrue();
        minor.EntranceNumber.Should().Be(2);
    }

    [TestMethod]
    public void EmptyAreaSearch_ReportsExhaustionWithoutPainting()
    {
        var map = new OverworldMap(5, 5, Terrain.MOUNTAIN);
        var before = Snapshot(map);

        var result = LocationPlacementSearch.FindEmptyArea(map, 5, 5, new Random(42), 3);

        result.Position.Should().BeNull();
        result.Samples.Should().Be(3);
        Snapshot(map).Should().Equal(before);
    }

    [TestMethod]
    public void EmptyAreaSearch_ZeroBudgetDoesNotConsumeRandomness()
    {
        var rng = new Random(42);

        var result = LocationPlacementSearch.FindEmptyArea(new(3, 3, Terrain.NONE), 3, 3, rng, 0);

        result.Position.Should().BeNull();
        result.Samples.Should().Be(0);
        rng.Next().Should().Be(new Random(42).Next());
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void EmptyAreaSearch_ExcludedColumnCanMakeAnOtherwiseValidAreaUnavailable(bool excluded)
    {
        var map = new OverworldMap(3, 3, Terrain.NONE);

        var result = LocationPlacementSearch.FindEmptyArea(map, 3, 3, new Random(42), 4, excluded ? 1 : null);

        result.Position.Should().Be(excluded ? null : new IntVector2(1, 1));
        result.Samples.Should().Be(excluded ? 4 : 1);
        Snapshot(map).Should().OnlyContain(t => t == Terrain.NONE);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void HiddenSearch_ReturnsAPositionOrExhaustionWithoutMutatingLocations(bool available)
    {
        var map = new OverworldMap(3, 3, Terrain.MOUNTAIN);
        if (available) map[0, 0] = Terrain.GRASS;
        var moving = NewLocation(Terrain.GRASS, LocationID.WEST_GRASS);
        var before = Snapshot(map);

        var result = LocationPlacementSearch.FindHiddenPosition(map, 3, 3, moving,
            new[] { moving }, Array.Empty<ContinentConnectorReservation>(), new Random(42), 100);

        result.Position.Should().Be(available ? new IntVector2(0, 0) : null);
        result.Samples.Should().BeInRange(1, 100);
        if (!available) result.Samples.Should().Be(100);
        moving.Pos.Should().Be(IntVector2.NOT_ON_MAP);
        moving.CanShuffle.Should().BeTrue();
        Snapshot(map).Should().Equal(before);
    }

    [TestMethod]
    public void HiddenSearch_ReadsExplicitReservationsWithoutChangingTheChosenConnector()
    {
        var map = new OverworldMap(9, 9, Terrain.NONE);
        var reservation = ContinentConnectorReservation.Plan(new Random(42), map, Biome.VANILLALIKE, World.ConnectorKind.Raft, Direction.WEST,
            new[] { Terrain.GRASS }, Array.Empty<IntVector2>(), Array.Empty<ContinentConnectorReservation>())!;
        reservation.Should().NotBeNull();
        reservation.Seed(map, Terrain.WATER, Terrain.GRASS);
        var moving = NewLocation(Terrain.GRASS, LocationID.WEST_GRASS);
        var before = Snapshot(map);

        var result = LocationPlacementSearch.FindHiddenPosition(map, 9, 9, moving,
            new[] { moving }, new[] { reservation }, new Random(42), 100);

        result.Position.Should().BeNull("all matching land cells belong to the reserved approach");
        result.Samples.Should().Be(100);
        Snapshot(map).Should().Equal(before);
        reservation.IsIntact(map, finalized: false).Should().BeTrue();
        moving.CanShuffle.Should().BeTrue();
    }

    [TestMethod]
    public void Searches_CannotUseBackingPaddingOutsideLogicalDimensions()
    {
        var map = new OverworldMap(5, 5, Terrain.NONE);
        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++) map[y, x] = Terrain.MOUNTAIN;
        map[4, 4] = Terrain.GRASS;
        var before = Snapshot(map);
        var moving = NewLocation(Terrain.GRASS, LocationID.WEST_GRASS);

        LocationPlacementSearch.FindEmptyArea(map, 3, 3, new Random(42), 5).Position.Should().BeNull();
        LocationPlacementSearch.FindHiddenPosition(map, 3, 3, moving, new[] { moving },
            Array.Empty<ContinentConnectorReservation>(), new Random(42), 5).Position.Should().BeNull();

        Snapshot(map).Should().Equal(before);
    }

    [TestMethod]
    [DataRow(Terrain.GRASS, Terrain.FOREST)]
    [DataRow(Terrain.FOREST, Terrain.GRASS)]
    [DataRow(Terrain.TOWN, Terrain.FOREST)]
    public void OrdinarySurroundings_RejectTheLocationTerrainAndPaintOneTerrainThroughout(Terrain terrain, Terrain alternative)
    {
        var world = new TestWorld(3, 3, Terrain.NONE);
        var climate = new Climate("Placement fixture", new(),
            new TableWeightedRandom<Terrain>(new[] { (terrain, 9), (alternative, 1) }), 0);
        world.UsePlacementClimate(climate, new[] { terrain, alternative });
        var location = Add(world, terrain);

        world.PlaceOrdinaryLocations().Should().BeTrue();

        location.Pos.Should().Be(new IntVector2(1, 1));
        world.map[location.Pos].Should().Be(terrain);
        foreach (var direction in IntVector2.DIRECTIONS)
            world.map[location.Pos + direction].Should().Be(alternative);
        location.CanShuffle.Should().BeFalse();
    }

    [TestMethod]
    public void PalaceSurroundings_AllowThePalaceTerrainWhenTheClimateOffersIt()
    {
        var outcomes = new HashSet<Terrain>();
        for (int seed = 0; seed < 16; seed++)
        {
            var world = new TestWorld(3, 3, Terrain.NONE);
            world.SeedRandom(seed);
            var climate = new Climate("Palace fixture", new(),
                new TableWeightedRandom<Terrain>(new[] { (Terrain.PALACE, 1), (Terrain.GRASS, 1) }), 0);
            world.UsePlacementClimate(climate, new[] { Terrain.PALACE, Terrain.GRASS });
            var palace = Add(world, Terrain.PALACE);

            world.PlaceOrdinaryLocations().Should().BeTrue();

            var surrounding = world.map[0, 0];
            outcomes.Add(surrounding);
            foreach (var direction in IntVector2.DIRECTIONS)
                world.map[palace.Pos + direction].Should().Be(surrounding);
            world.map[palace.Pos].Should().Be(Terrain.PALACE);
            palace.CanShuffle.Should().BeFalse();
        }
        outcomes.Should().BeEquivalentTo(new[] { Terrain.PALACE, Terrain.GRASS });
    }

    private static TestWorld OrdinaryWorld()
    {
        var world = new TestWorld(7, 7);
        world.UseClassicClimate();
        EmptyArea(world, new(3, 3));
        return world;
    }

    private static void EmptyArea(TestWorld world, IntVector2 center)
    {
        for (int y = center.Y - 1; y <= center.Y + 1; y++)
            for (int x = center.X - 1; x <= center.X + 1; x++) world.map[y, x] = Terrain.NONE;
    }

    private static Location Add(TestWorld world, Terrain terrain, LocationID id = LocationID.WEST_GRASS)
    {
        var location = NewLocation(terrain, id);
        world.AddLocation(location);
        return location;
    }

    private static Location AddMinor(TestWorld world, LocationID id = LocationID.WEST_GRASS)
    {
        var location = NewLocation(Terrain.GRASS, id);
        world.AddMinorLocation(location);
        return location;
    }

    private static Location NewLocation(Terrain terrain, LocationID id)
        => new(id, 0, 0, 0, id.GetContinent()) { TerrainType = terrain, AppearsOnMap = true };

    private static Terrain[] Snapshot(OverworldMap map)
        => (from y in Enumerable.Range(0, map.GetLength(0))
            from x in Enumerable.Range(0, map.GetLength(1))
            select map[y, x]).ToArray();
}
