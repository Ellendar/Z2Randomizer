using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.Core.Overworld;

public enum Biome
{ VANILLA, VANILLA_SHUFFLE, VANILLALIKE, ISLANDS, CANYON, DRY_CANYON, MOUNTAINOUS, VOLCANO, CALDERA, RANDOM_NO_VANILLA_OR_SHUFFLE, RANDOM_NO_VANILLA, RANDOM }

static class BiomeExtensions
{
    public static int SeedTerrainLimit(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => 300,
            Biome.VANILLA_SHUFFLE => 300,
            Biome.VANILLALIKE => 300,
            Biome.ISLANDS => 220,
            Biome.CANYON => 300,
            Biome.DRY_CANYON => 300,
            Biome.MOUNTAINOUS => 250,
            Biome.CALDERA => 300
        };
    }
}