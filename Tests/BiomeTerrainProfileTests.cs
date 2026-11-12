using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Z2Randomizer.Tests;

[TestClass]
public class BiomeTerrainProfileTests
{
    [TestMethod]
    public void ClimateSampling_TreatsTerrainFiltersAsMembershipRatherThanWeightsOrOrder()
    {
        var climate = Climates.Create(Continent.WEST, ClimateEnum.CLASSIC);
        var first = new Random(42);
        var second = new Random(42);
        Terrain[] filter = [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST];
        Terrain[] reordered = [Terrain.FOREST, Terrain.GRASS, Terrain.DESERT, Terrain.FOREST];

        var expected = Enumerable.Range(0, 100).Select(_ => climate.GetRandomTerrain(first, filter)).ToArray();
        var actual = Enumerable.Range(0, 100).Select(_ => climate.GetRandomTerrain(second, reordered)).ToArray();

        actual.Should().Equal(expected);
        second.GetState().Should().Be(first.GetState());
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void IslandProfiles_DistinguishPreplacedAndExpandableWater(bool goodBoots)
    {
        var west = Profile(Continent.WEST, Biome.ISLANDS, goodBoots);
        var east = Profile(Continent.EAST, Biome.ISLANDS, goodBoots);
        var dm = Profile(Continent.DM, Biome.ISLANDS, goodBoots);
        var water = goodBoots ? Terrain.WALKABLEWATER : Terrain.WATER;
        var preplaced = goodBoots ? Terrain.PREPLACED_WATER_WALKABLE : Terrain.PREPLACED_WATER;

        west.SeedTerrains.Should().Contain(preplaced).And.NotContain(water);
        east.SeedTerrains.Should().Contain(water).And.NotContain(preplaced);
        dm.SeedTerrains.Should().Contain(water).And.NotContain(preplaced);
        foreach (var profile in new[] { west, east, dm })
        {
            profile.Water.Should().Be(water);
            profile.PreplacedWater.Should().Be(preplaced);
            profile.MapBorder.Should().Be(preplaced);
            profile.StripeTerrain.Should().Be(preplaced);
        }
        east.InitialPlacementTerrains.Should().Contain(Terrain.LAVA);
        west.InitialPlacementTerrains.Should().NotContain(Terrain.LAVA);
    }

    [TestMethod]
    [DataRow(Continent.WEST)]
    [DataRow(Continent.EAST)]
    public void CanyonProfiles_AllowMountainBanksButKeepPlacementOffMountains(Continent continent)
    {
        var wet = Profile(continent, Biome.CANYON);
        var dry = Profile(continent, Biome.DRY_CANYON);

        wet.CanyonBankTerrains.Should().Contain(Terrain.MOUNTAIN);
        wet.InitialPlacementTerrains.Should().NotContain(Terrain.MOUNTAIN).And.NotContain(Terrain.SWAMP);
        wet.SeedTerrains.Should().Contain(Terrain.MOUNTAIN);
        wet.RiverTerrain.Should().Be(wet.PreplacedWater);
        dry.RiverTerrain.Should().Be(Terrain.DESERT);
    }

    [TestMethod]
    [DataRow(Biome.VANILLALIKE)]
    [DataRow(Biome.VOLCANO)]
    public void EastProfiles_UseMountainCrossingsForDefaultAndVolcano(Biome biome)
    {
        Profile(Continent.EAST, biome).RiverTerrain.Should().Be(Terrain.MOUNTAIN);
    }

    [TestMethod]
    [DataRow(Continent.WEST)]
    [DataRow(Continent.EAST)]
    [DataRow(Continent.DM)]
    public void CoastalProfiles_UseCanyonLandAndPreplacedIslandWater(Continent continent)
    {
        var profile = Profile(continent, Biome.COASTAL_ISLANDS);

        profile.RiverTerrain.Should().Be(profile.PreplacedWater);
        profile.MapBorder.Should().Be(profile.PreplacedWater);
        profile.StripeTerrain.Should().Be(profile.PreplacedWater);
        profile.CanyonBankTerrains.Should().Contain(Terrain.MOUNTAIN);
        profile.InitialPlacementTerrains.Should().NotContain(Terrain.MOUNTAIN).And.NotContain(Terrain.SWAMP);
        profile.EasternSeaStartColumn.Should().BeNull();
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void CoastalProfiles_SeedWaterLikeIslands(bool goodBoots)
    {
        var west = Profile(Continent.WEST, Biome.COASTAL_ISLANDS, goodBoots);
        var east = Profile(Continent.EAST, Biome.COASTAL_ISLANDS, goodBoots);
        var water = goodBoots ? Terrain.WALKABLEWATER : Terrain.WATER;
        var preplaced = goodBoots ? Terrain.PREPLACED_WATER_WALKABLE : Terrain.PREPLACED_WATER;

        west.SeedTerrains.Should().Contain(preplaced).And.NotContain(water);
        east.SeedTerrains.Should().Contain(water).And.NotContain(preplaced);
    }

    [TestMethod]
    [DataRow(Continent.WEST, Biome.CALDERA)]
    [DataRow(Continent.DM, Biome.CALDERA)]
    [DataRow(Continent.EAST, Biome.VOLCANO)]
    public void Resolve_AcceptsContinentSpecificBiomes(Continent continent, Biome biome)
    {
        Profile(continent, biome).RiverTerrain.Should().Be(Terrain.MOUNTAIN);
    }

    [TestMethod]
    [DataRow(Continent.WEST, Biome.VOLCANO)]
    [DataRow(Continent.DM, Biome.VOLCANO)]
    [DataRow(Continent.EAST, Biome.CALDERA)]
    [DataRow(Continent.EAST, Biome.RANDOM_UNIQUE)]
    public void Resolve_RejectsBiomesUnavailableToTheSelectedContinent(Continent continent, Biome biome)
    {
        Action resolve = () => Profile(continent, biome);

        resolve.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("biome");
    }

    [TestMethod]
    public void Resolve_RejectsUnsupportedContinentsAndUnresolvedBiomes()
    {
        Action maze = () => Profile(Continent.MAZE, Biome.VANILLALIKE);
        Action random = () => Profile(Continent.WEST, Biome.RANDOM);
        maze.Should().Throw<ArgumentOutOfRangeException>();
        random.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static BiomeTerrainProfile Profile(Continent continent, Biome biome, bool goodBoots = false)
        => BiomeTerrainProfile.Resolve(continent, biome, goodBoots, Climates.Create(continent, ClimateEnum.CLASSIC), isHorizontal: false);
}
