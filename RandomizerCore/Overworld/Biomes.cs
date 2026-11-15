using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>
/// Source of truth for configuration parameters for each biome.
/// </summary>
internal static class Biomes
{
    /// <summary>Returns whether shore preparation can open a mountain border for a raft or bridge.</summary>
    /// <remarks>Used by <see cref="World.DrawOcean"/> and <see cref="ContinentConnectorReservation.FindCandidates"/>.</remarks>
    internal static bool CanOpenMountainShore(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => false,
            Biome.VANILLA_SHUFFLE => false,
            Biome.VANILLALIKE => false,
            Biome.ISLANDS => false,
            Biome.CANYON => false,
            Biome.DRY_CANYON => false,
            Biome.COASTAL_ISLANDS => false,
            Biome.MOUNTAINOUS => true,
            Biome.VOLCANO => false,
            Biome.CALDERA => true,
            _ => false
        };
    }

    /// <summary>
    /// Returns the forward spacing bounds in tiles for both primary and companion Death Mountain cave pairs.
    /// The lower bound is inclusive and the upper bound is exclusive.
    /// </summary>
    /// <remarks>Used by <see cref="DeathMountain.TerraformCaveExpansion"/>.</remarks>
    internal static (int Minimum, int MaximumExclusive) GetDeathMountainCaveSpacing(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => (3, 10),
            Biome.VANILLA_SHUFFLE => (3, 10),
            Biome.VANILLALIKE => (3, 10),
            Biome.ISLANDS => (5, 12),
            Biome.CANYON => (3, 10),
            Biome.DRY_CANYON => (3, 10),
            Biome.COASTAL_ISLANDS => (5, 12),
            Biome.MOUNTAINOUS => (3, 10),
            Biome.CALDERA => (3, 10),
            _ => (3, 10)
        };
    }

    /// <summary>
    /// Returns the width of the random range added to Death Mountain's base growth
    /// openness factor of 1. A width of zero skips the roll.
    /// </summary>
    /// <remarks>Used by <see cref="TerrainGrowth.CreateDeathMountainClimate"/>.</remarks>
    internal static double GetDeathMountainOpennessRange(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => 0,
            Biome.VANILLA_SHUFFLE => 0,
            Biome.VANILLALIKE => 0,
            Biome.ISLANDS => .5,
            Biome.CANYON => .75,
            Biome.DRY_CANYON => 0,
            Biome.COASTAL_ISLANDS => .5,
            Biome.MOUNTAINOUS => 0,
            Biome.CALDERA => .75,
            _ => 0
        };
    }

    /// <summary>Returns the zero-based start column of Death Mountain's eastern sea, or <see langword="null"/> when it is absent.</summary>
    /// <remarks>Used by <see cref="DeathMountain.PrepareGeneratedMap"/>.</remarks>
    internal static int? GetEasternSeaStartColumn(this Biome biome, Continent continent)
    {
        if (continent == Continent.DM)
        {
            return biome switch
            {
                Biome.VANILLA => 29,
                Biome.VANILLA_SHUFFLE => 29,
                Biome.VANILLALIKE => 29,
                Biome.ISLANDS => null,
                Biome.CANYON => null,
                Biome.DRY_CANYON => null,
                Biome.COASTAL_ISLANDS => null,
                Biome.MOUNTAINOUS => 29,
                Biome.CALDERA => null,
                _ => 29
            };
        }

        return null;
    }

    /// <summary>Returns the initial values for <see cref="World.walkableTerrains"/>.</summary>
    internal static Terrain[] GetInitialPlacementTerrains(this Biome biome, Continent continent)
    {
        return (continent, biome) switch
        {
            (Continent.WEST or Continent.EAST,
                Biome.CANYON or
                Biome.DRY_CANYON or
                Biome.COASTAL_ISLANDS)
                => [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE],

            (Continent.WEST or Continent.EAST,
                Biome.VANILLA or
                Biome.VANILLA_SHUFFLE or
                Biome.VANILLALIKE or
                Biome.MOUNTAINOUS)
                => [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE],

            (Continent.WEST,
                Biome.ISLANDS or
                Biome.CALDERA)
                => [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE],

            (Continent.EAST,
                Biome.ISLANDS or
                Biome.VOLCANO)
                => [Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE],

            (Continent.DM,
                Biome.VANILLA or
                Biome.VANILLA_SHUFFLE or
                Biome.VANILLALIKE or
                Biome.ISLANDS or
                Biome.CANYON or
                Biome.DRY_CANYON or
                Biome.COASTAL_ISLANDS or
                Biome.MOUNTAINOUS or
                Biome.CALDERA)
                => [Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE],

            (Continent.WEST or Continent.EAST or Continent.DM, _)
                => throw new ArgumentOutOfRangeException(nameof(biome), biome,
                    $"Expected a resolved biome supported by {continent}. Resolve random configuration choices before creating a terrain profile."),

            _ => throw new ArgumentOutOfRangeException(nameof(continent))
        };
    }

    /// <summary>Returns the terrain used to paint the map perimeter.</summary>
    /// <remarks>Used by <see cref="World.FillMapBorder"/> and <see cref="TerrainLayout.PaintBorder"/>.</remarks>
    internal static Terrain GetMapBorderTerrain(this Biome biome, Terrain preplacedWater)
    {
        return biome switch
        {
            Biome.VANILLA => Terrain.MOUNTAIN,
            Biome.VANILLA_SHUFFLE => Terrain.MOUNTAIN,
            Biome.VANILLALIKE => Terrain.MOUNTAIN,
            Biome.ISLANDS => preplacedWater,
            Biome.CANYON => Terrain.MOUNTAIN,
            Biome.DRY_CANYON => Terrain.MOUNTAIN,
            Biome.COASTAL_ISLANDS => preplacedWater,
            Biome.MOUNTAINOUS => Terrain.MOUNTAIN,
            Biome.VOLCANO => Terrain.MOUNTAIN,
            Biome.CALDERA => Terrain.MOUNTAIN,
            _ => Terrain.MOUNTAIN
        };
    }

    /// <summary>Returns the exclusive crossing length limit in tiles and the attempt budget before Boots and climate adjustments.</summary>
    /// <remarks>Used by <see cref="World.RegionCrossingSearch"/>.</remarks>
    internal static (int MaximumLength, int MaximumAttempts) GetRegionCrossingLimits(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLALIKE => (10, 2000),
            Biome.ISLANDS => (10, 4000),
            Biome.CANYON => (10, 2000),
            Biome.DRY_CANYON => (10, 2000),
            Biome.COASTAL_ISLANDS => (15, 4000),
            Biome.MOUNTAINOUS => (15, 4000),
            Biome.VOLCANO => (10, 3000),
            Biome.CALDERA => (10, 3000),
            _ => throw new KeyNotFoundException($"No region crossing limits for biome {biome}.")
        };
    }

    /// <summary>Returns the terrain used for canyon channels and as the band to cross in region crossing searches.</summary>
    internal static Terrain GetRiverTerrain(this Biome biome, Continent continent, Terrain preplacedWater)
    {
        return (continent, biome) switch
        {
            (_, Biome.VANILLA) => Terrain.MOUNTAIN,
            (_, Biome.VANILLA_SHUFFLE) => Terrain.MOUNTAIN,
            (_, Biome.VANILLALIKE) => Terrain.MOUNTAIN,
            (_, Biome.ISLANDS) => preplacedWater,
            (_, Biome.CANYON) => preplacedWater,
            (Continent.DM, Biome.DRY_CANYON) => preplacedWater,
            (_, Biome.DRY_CANYON) => Terrain.DESERT,
            (_, Biome.COASTAL_ISLANDS) => preplacedWater,
            (_, Biome.MOUNTAINOUS) => Terrain.MOUNTAIN,
            (_, Biome.VOLCANO) => Terrain.MOUNTAIN,
            (_, Biome.CALDERA) => Terrain.MOUNTAIN,
            _ => Terrain.MOUNTAIN
        };
    }

    /// <summary>Returns the inclusive minimum and exclusive maximum forward spacing in tiles for a sane cave pair.</summary>
    /// <remarks>Used by <see cref="World.PlaceSaneCave"/>.</remarks>
    internal static (int Minimum, int MaximumExclusive) GetSaneCaveSpacing(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => (6, 18),
            Biome.VANILLA_SHUFFLE => (6, 18),
            Biome.VANILLALIKE => (6, 18),
            Biome.ISLANDS => (10, 20),
            Biome.CANYON => (6, 18),
            Biome.DRY_CANYON => (6, 18),
            Biome.COASTAL_ISLANDS => (10, 20),
            Biome.MOUNTAINOUS => (10, 20),
            Biome.VOLCANO => (5, 20),
            Biome.CALDERA => (5, 20),
            _ => throw new ArgumentOutOfRangeException(nameof(biome))
        };
    }

    /// <summary>Returns the near and far edge margins in tiles for sampling stripe rows and columns.</summary>
    /// <remarks>Used by <see cref="World.DrawTerrainStripes"/>.</remarks>
    internal static (int NearEdge, int FarEdge) GetStripeMargins(this Biome biome, Continent continent)
    {
        return (continent, biome) switch
        {
            (Continent.EAST, Biome.ISLANDS) => (1, 1),

            (Continent.EAST,
                Biome.VANILLA or
                Biome.VANILLA_SHUFFLE or
                Biome.VANILLALIKE or
                Biome.CANYON or
                Biome.DRY_CANYON or
                Biome.COASTAL_ISLANDS or
                Biome.MOUNTAINOUS or
                Biome.VOLCANO)
                => (10, 11),

            _ => (10, 11)
        };
    }

    /// <summary>Returns the terrain used to paint stripes within the map.</summary>
    /// <remarks>
    /// Used by <see cref="World.DrawTerrainStripes"/>, <see cref="TerrainLayout.DrawIslandStripes"/>,
    /// and <see cref="TerrainLayout.DrawCoastalIslands"/>.
    /// </remarks>
    internal static Terrain GetStripeTerrain(this Biome biome, Terrain preplacedWater)
    {
        return biome switch
        {
            Biome.VANILLA => Terrain.MOUNTAIN,
            Biome.VANILLA_SHUFFLE => Terrain.MOUNTAIN,
            Biome.VANILLALIKE => Terrain.MOUNTAIN,
            Biome.ISLANDS => preplacedWater,
            Biome.CANYON => Terrain.MOUNTAIN,
            Biome.DRY_CANYON => Terrain.MOUNTAIN,
            Biome.COASTAL_ISLANDS => preplacedWater,
            Biome.MOUNTAINOUS => Terrain.MOUNTAIN,
            Biome.VOLCANO => Terrain.MOUNTAIN,
            Biome.CALDERA => Terrain.MOUNTAIN,
            _ => Terrain.MOUNTAIN
        };
    }

    /// <summary>Returns whether the terrain seed filter uses preplaced water.</summary>
    /// <remarks>
    /// Used by <see cref="World.PlaceRandomTerrain"/>.
    /// Expandable water can let the Wetlands climate overwhelm West's island borders.
    /// </remarks>
    internal static bool SeedsPreplacedWater(this Biome biome, Continent continent)
    {
        return (continent, biome) switch
        {
            (Continent.WEST,
                Biome.ISLANDS or
                Biome.COASTAL_ISLANDS)
                => true,

            (Continent.WEST,
                Biome.VANILLA or
                Biome.VANILLA_SHUFFLE or
                Biome.VANILLALIKE or
                Biome.CANYON or
                Biome.DRY_CANYON or
                Biome.MOUNTAINOUS or
                Biome.CALDERA)
                => false,

            _ => false
        };
    }

    /// <summary>Returns the cap on the climate's requested terrain seed count.</summary>
    /// <remarks>Used by <see cref="Climates.CreateForWorld"/>.</remarks>
    internal static int SeedTerrainLimit(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => 300,
            Biome.VANILLA_SHUFFLE => 300,
            Biome.VANILLALIKE => 300,
            Biome.ISLANDS => 220,
            Biome.CANYON => 300,
            Biome.DRY_CANYON => 300,
            Biome.COASTAL_ISLANDS => 250,
            Biome.MOUNTAINOUS => 250,
            Biome.VOLCANO => 300,
            Biome.CALDERA => 300,
            _ => throw new Exception("Unrecognized Biome in SeedTerrainLimit")
        };
    }
}
