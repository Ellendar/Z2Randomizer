using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>
/// Resolved terrain inputs, not attempt progress. Collections are membership
/// filters; selection weights belong to Climate. Climate retains its world-owned
/// lifetime so resolving a profile does not reset retry state or consume RNG.
/// </summary>
internal sealed class BiomeTerrainProfile
{
    public Climate Climate { get; }
    public bool IsHorizontal { get; }
    public int? EasternSeaStartColumn { get; }
    public Terrain Water { get; }
    public Terrain PreplacedWater { get; }
    public Terrain RiverTerrain { get; }
    public IReadOnlyList<Terrain> InitialPlacementTerrains { get; }
    public IReadOnlyList<Terrain> SeedTerrains { get; }
    public IReadOnlyList<Terrain> CanyonBankTerrains { get; }
    public Terrain StripeTerrain { get; }
    public int NearEdgeMargin { get; }
    public int FarEdgeMargin { get; }

    private BiomeTerrainProfile(Continent continent, Biome biome, bool goodBoots, Climate climate, bool isHorizontal)
    {
        // local shorthands
        bool canyon = biome is Biome.CANYON or Biome.DRY_CANYON;
        bool coastal = biome == Biome.COASTAL_ISLANDS;
        bool seaBiome = biome == Biome.ISLANDS || coastal;
        bool dm = continent == Continent.DM;
        bool east = continent == Continent.EAST;

        Climate = climate;
        IsHorizontal = isHorizontal;
        Water = goodBoots ? Terrain.WALKABLEWATER : Terrain.WATER;
        PreplacedWater = goodBoots ? Terrain.PREPLACED_WATER_WALKABLE : Terrain.PREPLACED_WATER;
        RiverTerrain = seaBiome || canyon ? PreplacedWater : Terrain.MOUNTAIN;
        if (!dm && biome == Biome.DRY_CANYON) RiverTerrain = Terrain.DESERT;

        Terrain[] placement = continent switch
        {
            Continent.DM => [Terrain.DESERT, Terrain.FOREST, Terrain.GRAVE],
            Continent.WEST => biome switch
            {
                Biome.CANYON or
                Biome.DRY_CANYON or
                Biome.COASTAL_ISLANDS
                    => [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE],

                Biome.VANILLA or
                Biome.VANILLA_SHUFFLE or
                Biome.VANILLALIKE or
                Biome.ISLANDS or
                Biome.MOUNTAINOUS or
                Biome.CALDERA
                    => [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE],

                _ => throw new ArgumentOutOfRangeException(nameof(biome))
            },
            Continent.EAST => biome switch
            {
                Biome.CANYON or
                Biome.DRY_CANYON or
                Biome.COASTAL_ISLANDS
                    => [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.GRAVE],

                Biome.ISLANDS or
                Biome.VOLCANO
                    => [Terrain.LAVA, Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE],

                Biome.VANILLA or
                Biome.VANILLA_SHUFFLE or
                Biome.VANILLALIKE or
                Biome.MOUNTAINOUS
                    => [Terrain.DESERT, Terrain.GRASS, Terrain.FOREST, Terrain.SWAMP, Terrain.GRAVE],

                _ => throw new ArgumentOutOfRangeException(nameof(biome))
            },
            _ => throw new ArgumentOutOfRangeException(nameof(continent))
        };
        // West islands seed preplaced water; ordinary expandable water otherwise
        // makes the wetlands climate overwhelm the intended island borders.
        Terrain seedWater = continent == Continent.WEST && seaBiome ? PreplacedWater : Water;
        InitialPlacementTerrains = Array.AsReadOnly(placement);
        SeedTerrains = Array.AsReadOnly([.. placement, Terrain.MOUNTAIN, seedWater]);
        CanyonBankTerrains = Array.AsReadOnly<Terrain>(dm
            ? [Terrain.GRASS, Terrain.GRAVE, Terrain.FOREST, Terrain.DESERT, Terrain.MOUNTAIN]
            : [.. placement, Terrain.MOUNTAIN]);
        // Classic DM's land occupies the first 29 columns; the rest is seeded sea.
        EasternSeaStartColumn = dm && !canyon && !seaBiome && biome != Biome.CALDERA ? 29 : null;
        StripeTerrain = seaBiome ? PreplacedWater : Terrain.MOUNTAIN;
        NearEdgeMargin = east && biome == Biome.ISLANDS ? 1 : 10;
        FarEdgeMargin = east && biome == Biome.ISLANDS ? 1 : 11;
    }

    /// <summary>Creates terrain inputs for a concrete biome supported by the selected continent.</summary>
    internal static BiomeTerrainProfile Resolve(Continent continent, Biome biome, bool goodBoots, Climate climate, bool isHorizontal)
    {
        switch (continent)
        {
            case Continent.WEST:
            case Continent.EAST:
            case Continent.DM:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(continent));
        }

        // Explicit support list: adding a biome requires updating this switch
        // and reviewing the profile rules above. Random configuration choices
        // must already have been resolved; this method does not choose a biome.
        switch (biome)
        {
            case Biome.VANILLA:
            case Biome.VANILLA_SHUFFLE:
            case Biome.VANILLALIKE:
            case Biome.ISLANDS:
            case Biome.CANYON:
            case Biome.DRY_CANYON:
            case Biome.COASTAL_ISLANDS:
            case Biome.MOUNTAINOUS:
                break;
            // These are continent-specific.
            case Biome.VOLCANO when continent == Continent.EAST:
            case Biome.CALDERA when continent is Continent.WEST or Continent.DM:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(biome), biome,
                    $"Expected a resolved biome supported by {continent}. Resolve random configuration choices before creating a terrain profile.");
        }

        ArgumentNullException.ThrowIfNull(climate);

        return new(continent, biome, goodBoots, climate, isHorizontal);
    }
}
