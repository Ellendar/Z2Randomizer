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
    public Terrain MapBorder { get; }
    public Terrain StripeTerrain { get; }
    public int NearEdgeMargin { get; }
    public int FarEdgeMargin { get; }

    private BiomeTerrainProfile(Continent continent, Biome biome, bool goodBoots, Climate climate, bool isHorizontal,
        Terrain[] placement)
    {
        Climate = climate;
        IsHorizontal = isHorizontal;
        Water = goodBoots ? Terrain.WALKABLEWATER : Terrain.WATER;
        PreplacedWater = goodBoots ? Terrain.PREPLACED_WATER_WALKABLE : Terrain.PREPLACED_WATER;
        RiverTerrain = biome.GetRiverTerrain(continent, PreplacedWater);

        Terrain seedWater = biome.SeedsPreplacedWater(continent) ? PreplacedWater : Water;
        InitialPlacementTerrains = Array.AsReadOnly(placement);
        SeedTerrains = Array.AsReadOnly([.. placement, Terrain.MOUNTAIN, seedWater]);
        CanyonBankTerrains = Array.AsReadOnly<Terrain>(continent == Continent.DM
            ? [Terrain.GRASS, Terrain.GRAVE, Terrain.FOREST, Terrain.DESERT, Terrain.MOUNTAIN]
            : [.. placement, Terrain.MOUNTAIN]);

        EasternSeaStartColumn = biome.GetEasternSeaStartColumn(continent);
        MapBorder = biome.GetMapBorderTerrain(PreplacedWater);
        StripeTerrain = biome.GetStripeTerrain(PreplacedWater);
        (NearEdgeMargin, FarEdgeMargin) = biome.GetStripeMargins(continent);
    }

    /// <summary>Creates terrain inputs for a concrete biome supported by the selected continent.</summary>
    internal static BiomeTerrainProfile Resolve(Continent continent, Biome biome, bool goodBoots, Climate climate, bool isHorizontal)
    {
        Terrain[] placement = biome.GetInitialPlacementTerrains(continent);
        ArgumentNullException.ThrowIfNull(climate);
        return new(continent, biome, goodBoots, climate, isHorizontal, placement);
    }
}
