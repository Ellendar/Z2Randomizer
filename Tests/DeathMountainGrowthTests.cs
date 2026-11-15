using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Z2Randomizer.Tests;

[TestClass]
public class DeathMountainGrowthTests
{
    [TestMethod]
    [DataRow(Biome.VANILLALIKE)]
    [DataRow(Biome.ISLANDS)]
    [DataRow(Biome.CANYON)]
    [DataRow(Biome.DRY_CANYON)]
    [DataRow(Biome.MOUNTAINOUS)]
    [DataRow(Biome.CALDERA)]
    public void SeedProfiles_KeepRoadAndSwampOutOfRandomSeeds(Biome biome)
    {
        var profile = BiomeTerrainProfile.Resolve(Continent.DM, biome, false,
            Climates.Create(Continent.DM, ClimateEnum.CLASSIC), false);

        profile.SeedTerrains.Should().NotContain(Terrain.ROAD).And.NotContain(Terrain.SWAMP);
    }

    [TestMethod]
    [DataRow(100f, 5f)]
    [DataRow(1f, 1f)]
    public void Safety_RoadUsesLandAverageWithoutWeakeningAnAlreadySaferRoad(float road, float expected)
    {
        var climate = new Climate("Road fixture", new()
        {
            [Terrain.GRASS] = 4, [Terrain.FOREST] = 6,
            [Terrain.ROAD] = road, [Terrain.MOUNTAIN] = 1
        }, new TableWeightedRandom<Terrain>(new[] { (Terrain.GRASS, 1), (Terrain.FOREST, 1), (Terrain.ROAD, 1) }), 0);

        // Zero isolates road averaging from the separate mountain-dominance adjustment.
        climate.ApplyDeathMountainSafety(new[] { Terrain.GRASS, Terrain.FOREST, Terrain.ROAD }, constraintLimitFactor: 0);

        climate.DistanceCoefficients[(int)Terrain.ROAD].Should().Be(expected);
        climate.DistanceCoefficients[(int)Terrain.GRASS].Should().Be(4);
        climate.DistanceCoefficients[(int)Terrain.FOREST].Should().Be(6);
    }

    [TestMethod]
    public void Safety_IncreasesLandGrowthWhenMountainsWouldDominate()
    {
        var climate = new Climate("Dominant mountains", new()
        {
            [Terrain.DESERT] = 10, [Terrain.FOREST] = 10,
            [Terrain.ROAD] = 10, [Terrain.MOUNTAIN] = .5f
        }, new TableWeightedRandom<Terrain>(new[] { (Terrain.DESERT, 1), (Terrain.FOREST, 1), (Terrain.ROAD, 1) }), 0);

        climate.ApplyDeathMountainSafety(new[] { Terrain.DESERT, Terrain.FOREST, Terrain.ROAD });

        foreach (var terrain in new[] { Terrain.DESERT, Terrain.FOREST, Terrain.ROAD })
            climate.DistanceCoefficients[(int)terrain].Should().BeInRange(.01f, 9.99f,
                "smaller distance coefficients let land grow farther from its anchors");
        climate.DistanceCoefficients[(int)Terrain.MOUNTAIN].Should().Be(.5f);
    }

    [TestMethod]
    [DataRow(Biome.VANILLALIKE, false)]
    [DataRow(Biome.DRY_CANYON, false)]
    [DataRow(Biome.MOUNTAINOUS, false)]
    [DataRow(Biome.CANYON, true)]
    [DataRow(Biome.CALDERA, true)]
    [DataRow(Biome.ISLANDS, true)]
    public void GrowthClimate_RetainsInputsAndUsesOnlyTheBiomesSeededRandomness(Biome biome, bool randomOpenness)
    {
        var source = GrowthSource();
        var profile = BiomeTerrainProfile.Resolve(Continent.DM, biome, false, source, false);
        var seedsBefore = profile.SeedTerrains.ToArray();
        var coefficientsBefore = source.DistanceCoefficients.ToArray();
        var rng = new Random(42);
        var originalState = rng.GetState();
        var repeatRng = new Random(42);

        var result = TerrainGrowth.CreateDeathMountainClimate(source, profile.SeedTerrains.Append(Terrain.ROAD), biome, rng);
        var repeat = TerrainGrowth.CreateDeathMountainClimate(source, profile.SeedTerrains.Append(Terrain.ROAD), biome, repeatRng);

        result.Should().NotBeSameAs(source);
        result.Name.Should().Be(source.Name);
        result.Type.Should().Be(source.Type);
        result.SeedTerrainCount.Should().Be(source.SeedTerrainCount);
        result.DistanceCoefficients.Should().Equal(repeat.DistanceCoefficients);
        result.DistanceCoefficients[(int)Terrain.ROAD].Should().BeLessThan(coefficientsBefore[(int)Terrain.ROAD]);
        source.DistanceCoefficients.Should().Equal(coefficientsBefore);
        profile.SeedTerrains.Should().Equal(seedsBefore);
        rng.GetState().Should().Be(repeatRng.GetState());
        (rng.GetState() != originalState).Should().Be(randomOpenness);
    }

    [TestMethod]
    public void GrowthClimate_RetainsTheDistinctBiomeOpennessRanges()
    {
        var source = GrowthSource();
        var terrains = BiomeTerrainProfile.Resolve(Continent.DM, Biome.CANYON, false, source, false)
            .SeedTerrains.Append(Terrain.ROAD).ToArray();
        Climate Create(Biome biome) => TerrainGrowth.CreateDeathMountainClimate(source, terrains, biome, new Random(42));

        var baseline = Create(Biome.MOUNTAINOUS);
        var canyon = Create(Biome.CANYON);
        var caldera = Create(Biome.CALDERA);
        var islands = Create(Biome.ISLANDS);

        caldera.DistanceCoefficients.Should().Equal(canyon.DistanceCoefficients);
        float normal = baseline.DistanceCoefficients[(int)Terrain.DESERT];
        canyon.DistanceCoefficients[(int)Terrain.DESERT].Should().BeInRange(normal / 1.75f, normal);
        islands.DistanceCoefficients[(int)Terrain.DESERT].Should().BeInRange(normal / 1.5f, normal);
        canyon.DistanceCoefficients[(int)Terrain.DESERT].Should().BeLessThan(islands.DistanceCoefficients[(int)Terrain.DESERT]);
    }

    [TestMethod]
    public void GrowthClimate_ScrublandDoublesSafetyWithoutChangingTheRandomDraws()
    {
        var normal = GrowthSource();
        var scrubland = GrowthSource(ClimateEnum.SCRUBLAND);
        var terrains = BiomeTerrainProfile.Resolve(Continent.DM, Biome.ISLANDS, false, normal, false)
            .SeedTerrains.Append(Terrain.ROAD).ToArray();
        var normalRng = new Random(42);
        var scrublandRng = new Random(42);

        var normalGrowth = TerrainGrowth.CreateDeathMountainClimate(normal, terrains, Biome.ISLANDS, normalRng);
        var scrublandGrowth = TerrainGrowth.CreateDeathMountainClimate(scrubland, terrains, Biome.ISLANDS, scrublandRng);

        scrublandGrowth.DistanceCoefficients[(int)Terrain.DESERT]
            .Should().BeApproximately(normalGrowth.DistanceCoefficients[(int)Terrain.DESERT] / 2, .000001f);
        normalRng.GetState().Should().Be(scrublandRng.GetState());
        scrubland.DistanceCoefficients.Should().Equal(normal.DistanceCoefficients);
    }

    private static Climate GrowthSource(ClimateEnum type = ClimateEnum.CLASSIC)
        => new("DM fixture", new()
        {
            [Terrain.DESERT] = 10, [Terrain.FOREST] = 10, [Terrain.GRAVE] = 10,
            [Terrain.WATER] = 10, [Terrain.ROAD] = 100, [Terrain.MOUNTAIN] = .1f
        }, new TableWeightedRandom<Terrain>(new[]
        {
            (Terrain.DESERT, 1), (Terrain.FOREST, 1), (Terrain.GRAVE, 1),
            (Terrain.WATER, 1), (Terrain.ROAD, 1), (Terrain.MOUNTAIN, 0)
        }), 33, type);
}
