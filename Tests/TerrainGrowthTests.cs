using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Random = Z2Randomizer.RandomizerCore.Random;
using TestWorld = Z2Randomizer.Tests.WorldCharacterizationTests.TestWorld;

namespace Z2Randomizer.Tests;

[TestClass]
public class TerrainGrowthTests
{
    [TestMethod]
    [DataRow(Terrain.GRASS)]
    [DataRow(Terrain.WATER)]
    [DataRow(Terrain.PREPLACED_WATER)]
    [DataRow(Terrain.CAVE)]
    public void Growth_FillsFromTheOnlyAnchorWithoutChangingTheInputMap(Terrain terrain)
    {
        var world = new TestWorld(3, 5, Terrain.NONE);
        world.map[1, 2] = terrain;
        var input = world.map;
        var before = Snapshot(input);

        world.GrowWithClimate(Climate()).Should().BeTrue();

        Snapshot(world.map).Should().OnlyContain(t => t == terrain);
        world.map.Should().NotBeSameAs(input);
        Snapshot(input).Should().Equal(before);
    }

    [TestMethod]
    public void Growth_UsesOriginalAnchorsRatherThanAllowingNewCellsToTakeOver()
    {
        var world = BankAnchors();

        world.GrowWithClimate(Climate()).Should().BeTrue();

        world.map[0, 1].Should().Be(Terrain.GRASS);
        world.map[0, 2].Should().Be(Terrain.GRASS);
        world.map[0, 4].Should().Be(Terrain.FOREST);
        world.map[0, 5].Should().Be(Terrain.FOREST);
        world.map[0, 3].Should().BeOneOf(Terrain.GRASS, Terrain.FOREST);
    }

    [TestMethod]
    public void Growth_ClimateDistanceCoefficientsCanFavorTheFartherAnchor()
    {
        var world = BankAnchors();
        var climate = Climate(grassDistance: 3);
        var coefficients = climate.DistanceCoefficients.ToArray();

        world.GrowWithClimate(climate).Should().BeTrue();

        world.map[0, 2].Should().Be(Terrain.FOREST, "Forest's smaller coefficient outweighs its longer distance");
        world.map[0, 0].Should().Be(Terrain.GRASS, "fixed anchors cannot be overwritten");
        climate.DistanceCoefficients.Should().Equal(coefficients);
    }

    [TestMethod]
    public void Growth_TiesUseSeededRandomnessAndRemainReproducible()
    {
        var outcomes = new HashSet<Terrain>();
        for (int seed = 0; seed < 16; seed++)
        {
            var first = BankAnchors();
            var second = BankAnchors();
            first.SeedRandom(seed);
            second.SeedRandom(seed);

            first.GrowWithClimate(Climate()).Should().BeTrue();
            second.GrowWithClimate(Climate()).Should().BeTrue();

            Snapshot(first.map).Should().Equal(Snapshot(second.map));
            outcomes.Add(first.map[0, 3]);
        }
        outcomes.Should().BeEquivalentTo(new[] { Terrain.GRASS, Terrain.FOREST });
    }

    [TestMethod]
    public void Growth_UsesLogicalDimensionsAndIgnoresBackingPaddingAnchors()
    {
        var world = new TestWorld(3, 5);
        world.map = new OverworldMap(4, 7, Terrain.FOREST);
        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 5; x++) world.map[y, x] = Terrain.NONE;
        world.map[1, 2] = Terrain.GRASS;
        var input = world.map;
        var before = Snapshot(input);

        world.GrowWithClimate(Climate()).Should().BeTrue();

        world.map.GetLength(0).Should().Be(3);
        world.map.GetLength(1).Should().Be(5);
        Snapshot(world.map).Should().OnlyContain(t => t == Terrain.GRASS);
        Snapshot(input).Should().Equal(before);
    }

    [TestMethod]
    public void Growth_PreservesEveryExistingFeatureAndConsumesNoRandomnessOnAFilledMap()
    {
        var world = new TestWorld(1, 6);
        Terrain[] features = [Terrain.CAVE, Terrain.PALACE, Terrain.BRIDGE, Terrain.ROAD, Terrain.WALKABLEWATER, Terrain.PREPLACED_WATER_WALKABLE];
        for (int x = 0; x < features.Length; x++) world.map[0, x] = features[x];

        world.GrowWithClimate(Climate()).Should().BeTrue();

        Snapshot(world.map).Should().Equal(features);
        world.NextRandom().Should().Be(new Random(42).Next());
    }

    [TestMethod]
    [DataRow(8, 3)]
    [DataRow(3, 8)]
    public void Seeding_RespectsBothClimateCountAndCallerLimit(int requested, int maximum)
    {
        var world = new TestWorld(8, 9, Terrain.NONE);
        world.map[4, 4] = Terrain.PALACE;

        world.SeedTerrain(Climate(seedCount: requested), new[] { Terrain.GRASS }, maximum);

        Snapshot(world.map).Count(t => t == Terrain.GRASS).Should().Be(3);
        world.map[4, 4].Should().Be(Terrain.PALACE);
        Snapshot(world.map).Should().OnlyContain(t => t == Terrain.NONE || t == Terrain.GRASS || t == Terrain.PALACE);
    }

    [TestMethod]
    [DataRow(0, 8)]
    [DataRow(8, 0)]
    public void Seeding_ZeroBudgetLeavesMapAndRandomStateAlone(int requested, int maximum)
    {
        var world = new TestWorld(3, 5, Terrain.NONE);
        var before = Snapshot(world.map);

        world.SeedTerrain(Climate(seedCount: requested), new[] { Terrain.GRASS }, maximum);

        Snapshot(world.map).Should().Equal(before);
        world.NextRandom().Should().Be(new Random(42).Next());
    }

    [TestMethod]
    public void Seeding_FullMapStopsWithoutOverwritingFeatures()
    {
        var world = new TestWorld(3, 5, Terrain.PALACE);
        var before = Snapshot(world.map);

        world.SeedTerrain(Climate(seedCount: 10), new[] { Terrain.GRASS });

        Snapshot(world.map).Should().Equal(before);
    }

    [TestMethod]
    public void Seeding_FilterOrderAndDuplicatesDoNotSupplySamplingWeights()
    {
        var first = new TestWorld(15, 15, Terrain.NONE);
        var second = new TestWorld(15, 15, Terrain.NONE);
        var climate = Climate(seedCount: 80, forestWeight: 9);

        first.SeedTerrain(climate, new[] { Terrain.GRASS, Terrain.FOREST });
        second.SeedTerrain(climate, new[] { Terrain.FOREST, Terrain.GRASS, Terrain.GRASS });

        Snapshot(first.map).Should().Equal(Snapshot(second.map));
        var cells = Snapshot(first.map);
        cells.Count(t => t == Terrain.FOREST).Should().BeGreaterThan(50);
        cells.Count(t => t == Terrain.GRASS).Should().BeGreaterThan(0);
        cells.Count(t => t != Terrain.NONE).Should().Be(80);
    }

    [TestMethod]
    public void Seeding_OnlyTouchesEmptyLogicalCellsAndRetainsTheClimate()
    {
        var world = new TestWorld(5, 7);
        world.map = new OverworldMap(6, 9, Terrain.PALACE);
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 7; x++) world.map[y, x] = Terrain.NONE;
        world.map[2, 3] = Terrain.ROAD;
        var input = world.map;
        var climate = Climate(seedCount: 12);
        var coefficients = climate.DistanceCoefficients.ToArray();

        world.SeedTerrain(climate, new[] { Terrain.GRASS });

        world.map.Should().BeSameAs(input);
        Snapshot(input).Count(t => t == Terrain.GRASS).Should().Be(12);
        world.map[2, 3].Should().Be(Terrain.ROAD);
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 9; x++)
                if (y >= 5 || x >= 7) world.map[y, x].Should().Be(Terrain.PALACE);
        climate.SeedTerrainCount.Should().Be(12);
        climate.DistanceCoefficients.Should().Equal(coefficients);
    }

    [TestMethod]
    [DataRow(0, false, 0, false)]
    [DataRow(1, false, 1, false)]
    [DataRow(3, false, 1, true)]
    [DataRow(3, true, 0, true)]
    public void ServiceSeeding_ReportsActualProgressAndOccupiedCellExhaustion(
        int requested, bool occupied, int expectedPlaced, bool expectedExhausted)
    {
        var map = new OverworldMap(1, 1, occupied ? Terrain.PALACE : Terrain.NONE);

        var result = TerrainGrowth.Seed(map, 1, 1, Climate(seedCount: requested),
            new[] { Terrain.GRASS }, new Random(42));

        result.PlacedSeeds.Should().Be(expectedPlaced);
        result.Exhausted.Should().Be(expectedExhausted);
        map[0, 0].Should().Be(occupied ? Terrain.PALACE : requested == 0 ? Terrain.NONE : Terrain.GRASS);
    }

    [TestMethod]
    public void ServiceGrowth_ReturnsANewMapWithoutNeedingWorldOrMutatingItsInputs()
    {
        var map = new OverworldMap(1, 7, Terrain.NONE);
        map[0, 0] = Terrain.GRASS;
        map[0, 6] = Terrain.FOREST;
        var climate = Climate(grassDistance: 3);
        var before = Snapshot(map);
        var coefficients = climate.DistanceCoefficients.ToArray();

        var result = TerrainGrowth.Grow(map, 1, 7, climate.DistanceCoefficients, new Random(42));

        result.Should().NotBeSameAs(map);
        result[0, 2].Should().Be(Terrain.FOREST);
        Snapshot(result).Should().NotContain(Terrain.NONE);
        Snapshot(map).Should().Equal(before);
        climate.DistanceCoefficients.Should().Equal(coefficients);
    }

    private static TestWorld BankAnchors()
    {
        var world = new TestWorld(1, 7, Terrain.NONE);
        world.map[0, 0] = Terrain.GRASS;
        world.map[0, 6] = Terrain.FOREST;
        return world;
    }

    private static Climate Climate(int seedCount = 5, float grassDistance = 1, int forestWeight = 1)
        => new("Growth fixture", new() { [Terrain.GRASS] = grassDistance, [Terrain.FOREST] = 1 },
            new TableWeightedRandom<Terrain>(new[] { (Terrain.GRASS, 1), (Terrain.FOREST, forestWeight) }), seedCount);

    private static Terrain[] Snapshot(OverworldMap map)
        => (from y in Enumerable.Range(0, map.GetLength(0))
            from x in Enumerable.Range(0, map.GetLength(1))
            select map[y, x]).ToArray();
}
