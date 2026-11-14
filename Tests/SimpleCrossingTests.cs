using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class SimpleCrossingTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WaterCrossingsCycleThroughRoadBridgeAndOptionalWalkableWater(bool boots)
    {
        var world = Channels(Terrain.WATER);

        var result = world.PlaceCrossings(Options(Biome.ISLANDS, Terrain.WATER, boots));

        result.PlacedCrossings.Should().Be(4);
        Count(world, Terrain.ROAD).Should().Be(boots ? 10 : 12);
        Count(world, Terrain.BRIDGE).Should().Be(boots ? 10 : 5);
        Count(world, Terrain.WALKABLEWATER).Should().Be(boots ? 0 : 5);
    }

    [TestMethod]
    [DataRow(Terrain.WATER, Terrain.BRIDGE)]
    [DataRow(Terrain.WALKABLEWATER, Terrain.BRIDGE)]
    [DataRow(Terrain.PREPLACED_WATER, Terrain.ROAD)]
    [DataRow(Terrain.PREPLACED_WATER_WALKABLE, Terrain.ROAD)]
    [DataRow(Terrain.MOUNTAIN, Terrain.ROAD)]
    [DataRow(Terrain.DESERT, Terrain.ROAD)]
    public void VanillaLikeChoosesCrossingTerrainWithoutNormalizingWaterVariants(Terrain crossing, Terrain painted)
    {
        var world = Channels(crossing, gaps: 1);

        var result = world.PlaceCrossings(Options(Biome.VANILLALIKE, crossing, boots: true, maximum: 1));

        result.PlacedCrossings.Should().Be(1);
        Count(world, painted).Should().Be(5);
        Count(world, painted == Terrain.ROAD ? Terrain.BRIDGE : Terrain.ROAD).Should().Be(0);
    }

    [TestMethod]
    public void MountainousUsesTheBankTerrainAndItsEncounterPool()
    {
        var world = Channels(Terrain.MOUNTAIN, Terrain.FOREST);
        var first = AddEncounter(world, Terrain.FOREST);
        var second = AddEncounter(world, Terrain.FOREST);
        int before = Count(world, Terrain.FOREST);

        var result = world.PlaceCrossings(Options(Biome.MOUNTAINOUS, Terrain.MOUNTAIN, boots: true, maximum: 2));

        result.PlacedCrossings.Should().Be(2);
        Count(world, Terrain.FOREST).Should().BeInRange(before + 18, before + 20);
        new[] { first, second }.Should().OnlyContain(l => !l.CanShuffle && world.map[l.Pos] == Terrain.FOREST);
        first.Pos.Should().NotBe(second.Pos);
    }

    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(true, 1)]
    public void BlockersAreUsedOnceBeforeEligibleEncountersAndWalkableWaterUsesNeither(bool boots, int placedBridges)
    {
        var world = Channels(Terrain.WATER);
        var unavailable = AddEncounter(world, Terrain.ROAD, available: false);
        var road = AddEncounter(world, Terrain.ROAD);
        var laterRoad = AddEncounter(world, Terrain.ROAD);
        var bridge = AddEncounter(world, Terrain.BRIDGE);
        var originalPosition = unavailable.Pos;
        var requirements = new Requirements([RequirementType.GLOVE]);
        road.AccessRequirements = requirements;

        var result = world.PlaceCrossings(Options(Biome.ISLANDS, Terrain.WATER, boots) with
        {
            RiverDevilBlocker = true,
            RockBlocker = true
        });

        result.PlacedCrossings.Should().Be(4);
        Count(world, Terrain.RIVER_DEVIL).Should().Be(1);
        Count(world, Terrain.ROCK).Should().Be(1);
        unavailable.Pos.Should().Be(originalPosition);
        road.CanShuffle.Should().BeFalse();
        world.map[road.Pos].Should().Be(Terrain.ROAD);
        road.AccessRequirements.Should().BeSameAs(requirements);
        laterRoad.CanShuffle.Should().BeTrue();
        bridge.CanShuffle.Should().Be(placedBridges == 0);
        if (placedBridges != 0) world.map[bridge.Pos].Should().Be(Terrain.BRIDGE);
    }

    [TestMethod]
    public void NewRequestRestartsItsTerrainCycleAndBlockerRequests()
    {
        var world = Channels(Terrain.WATER);
        var options = Options(Biome.ISLANDS, Terrain.WATER, boots: false, maximum: 1) with { RiverDevilBlocker = true };

        world.PlaceCrossings(options).PlacedCrossings.Should().Be(1);
        world.PlaceCrossings(options).PlacedCrossings.Should().Be(1);

        Count(world, Terrain.RIVER_DEVIL).Should().Be(2);
        Count(world, Terrain.ROAD).Should().Be(8);
        Count(world, Terrain.BRIDGE).Should().Be(0);
        Count(world, Terrain.WALKABLEWATER).Should().Be(0);
    }

    [TestMethod]
    public void SariaDoesNotAdvanceTheOrdinaryCycleOrConsumeItsBlocker()
    {
        var world = Channels(Terrain.WATER);
        var town = new Town([], [], []) { Type = TownType.SARIA };
        foreach (var id in new[] { LocationID.WEST_TOWN_SARIA_NORTH, LocationID.WEST_TOWN_SARIA_SOUTH })
            world.AddLocation(new Location(id, 30, 0, 0, Continent.WEST)
            {
                Pos = IntVector2.NOT_ON_MAP,
                TerrainType = Terrain.TOWN,
                Town = town
            });
        world.SeedRandom(0);

        var result = world.PlaceCrossings(Options(Biome.ISLANDS, Terrain.WATER, boots: false) with
        {
            Features = CrossingFeatures.Saria,
            RiverDevilBlocker = true
        });

        result.PlacedCrossings.Should().Be(4);
        result.PlacedFeatures.Should().Be(CrossingFeatures.Saria);
        Count(world, Terrain.TOWN).Should().Be(2);
        Count(world, Terrain.RIVER_DEVIL).Should().Be(1);
        Count(world, Terrain.ROAD).Should().Be(6);
        Count(world, Terrain.BRIDGE).Should().Be(5);
        Count(world, Terrain.WALKABLEWATER).Should().Be(5);
    }

    [TestMethod]
    public void DesertBridgeCycleKeepsItsExistingGapAndDoesNotAddWalkableWater()
    {
        var world = Channels(Terrain.DESERT);

        var result = world.PlaceCrossings(Options(Biome.DRY_CANYON, Terrain.DESERT, boots: false));

        result.PlacedCrossings.Should().Be(4);
        Count(world, Terrain.ROAD).Should().Be(10);
        Count(world, Terrain.BRIDGE).Should().Be(8);
        Count(world, Terrain.WALKABLEWATER).Should().Be(0);
    }

    internal static TestWorld Channels(Terrain crossing, Terrain bank = Terrain.GRASS, int gaps = 4)
    {
        var world = new TestWorld(9, 9 * gaps + 5);
        for (int y = 1; y < 8; y++)
            for (int x = 1; x < world.MapColumns - 1; x++)
                world.map[y, x] = x % 9 is >= 4 and <= 8 ? crossing : bank;
        return world;
    }

    internal static RegionCrossingOptions Options(Biome biome, Terrain crossing, bool boots, int maximum = 4) => new()
    {
        MaximumCrossings = maximum,
        CrossingTerrain = crossing,
        Biome = biome,
        CanWalkOnWaterWithBoots = boots
    };

    internal static Location AddEncounter(TestWorld world, Terrain terrain, bool available = true)
    {
        var id = Enumerable.Range(0, 64).Select(i => LocationIDUtils.FromIndex(Continent.WEST, i))
            .First(id => world.AllLocations.All(l => l.ID != id));
        var location = new Location(id, 30, 0, 0, Continent.WEST)
        {
            Pos = IntVector2.NOT_ON_MAP,
            TerrainType = terrain,
            CanShuffle = available
        };
        world.AddLocation(location);
        return location;
    }

    private static int Count(TestWorld world, Terrain terrain) =>
        (from y in Enumerable.Range(0, world.MapRows)
         from x in Enumerable.Range(0, world.MapColumns)
         where world.map[y, x] == terrain select x).Count();
}
