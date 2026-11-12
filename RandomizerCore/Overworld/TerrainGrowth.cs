using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

internal readonly record struct TerrainSeedingResult(int PlacedSeeds, bool Exhausted);

/// <summary>Seeds and grows terrain using explicit map, climate and RNG inputs, without World lifecycle state.</summary>
internal static class TerrainGrowth
{
    /// <summary>Clones a DM climate and applies its biome openness and road/land safety adjustment.</summary>
    /// <remarks>The source and supplied terrain filter are unchanged; only openness biomes consume RNG.</remarks>
    internal static Climate CreateDeathMountainClimate(Climate source, IEnumerable<Terrain> growthTerrains,
        Biome biome, Random rng)
    {
        var result = source.Clone();
        double opennessRange = biome.GetDeathMountainOpennessRange();
        float openness = opennessRange > 0 ? (float)(rng.NextDouble() * opennessRange + 1) : 1f;
        if (result.Type == ClimateEnum.SCRUBLAND) openness *= 2;
        result.ApplyDeathMountainSafety(growthTerrains, openness);
        return result;
    }

    /// <summary>Seeds empty logical cells in place using climate weights and a terrain membership filter.</summary>
    /// <remarks>
    /// Stops at the smaller seed limit or after 300 consecutive occupied-cell rolls.
    /// For a positive limit, the climate must offer at least one allowed terrain.
    /// No input validation or sampling policy is added by this extraction.
    /// </remarks>
    internal static TerrainSeedingResult Seed(OverworldMap map, int rows, int columns,
        Climate climate, IEnumerable<Terrain> allowedTerrains, Random rng, int seedCountMaximum = 500)
    {
        const int exhaustionLimit = 300;
        int consecutiveAttempts = 0;
        int placed = 0;
        while (placed < int.Min(climate.SeedTerrainCount, seedCountMaximum) && consecutiveAttempts < exhaustionLimit)
        {
            consecutiveAttempts = 0;
            Terrain terrain = climate.GetRandomTerrain(rng, allowedTerrains);
            int x, y;
            do
            {
                x = rng.Next(columns);
                y = rng.Next(rows);
                consecutiveAttempts++;
            } while (map[y, x] != Terrain.NONE && consecutiveAttempts < exhaustionLimit);
            if (consecutiveAttempts < exhaustionLimit)
            {
                map[y, x] = terrain;
                placed++;
            }
        }
        return new(placed, consecutiveAttempts >= exhaustionLimit);
    }

    /// <summary>Returns a new logical-sized map, filling NONE cells from the original fixed anchors.</summary>
    /// <remarks>
    /// Inputs are read-only except for RNG advancement. Distance coefficients scale
    /// each anchor's distance; ties sample the ordered anchor choices, including
    /// multiple anchors of the same terrain. The map must contain a logical anchor.
    /// Existing cells survive unchanged; backing-map padding is neither read nor copied.
    /// </remarks>
    internal static OverworldMap Grow(OverworldMap map, int rows, int columns,
        IReadOnlyList<float> distanceCoefficients, Random rng)
    {
        var output = new OverworldMap(rows, columns);
        var anchors = CollectAnchors(map, rows, columns, distanceCoefficients);
        const double epsilon = 1e-9;
        List<Terrain> choices = [];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Terrain existing = map[y, x];
                if (existing != Terrain.NONE)
                {
                    output[y, x] = existing;
                    continue;
                }

                choices.Clear();
                double minimumDistance = double.MaxValue;
                for (int i = 0; i < anchors.Length; i++)
                {
                    ref readonly var anchor = ref anchors[i];
                    double dx = (double)anchor.Position.X - x;
                    double dy = (double)anchor.Position.Y - y;
                    double distance = anchor.CoefficientSquared * (dx * dx + dy * dy);
                    if (distance > minimumDistance) continue;
                    if (distance + epsilon < minimumDistance)
                    {
                        choices.Clear();
                        choices.Add(anchor.Terrain);
                        minimumDistance = distance;
                    }
                    else
                    {
                        choices.Add(anchor.Terrain);
                    }
                }
                Debug.Assert(choices.Count > 0);
                output[y, x] = choices[rng.Next(choices.Count)];
            }
        }
        return output;
    }

    private readonly record struct TerrainAnchor(IntVector2 Position, Terrain Terrain, double CoefficientSquared);

    private static TerrainAnchor[] CollectAnchors(OverworldMap map, int rows, int columns,
        IReadOnlyList<float> distanceCoefficients)
    {
        var knownTerrains = Enum.GetValues<Terrain>();
        List<TerrainAnchor> anchors = [];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Terrain terrain = map[y, x];
                if (terrain == Terrain.NONE || !knownTerrains.Contains(terrain)) continue;
                double coefficient = distanceCoefficients[(int)terrain];
                anchors.Add(new(new(x, y), terrain, coefficient * coefficient));
            }
        }
        Debug.Assert(anchors.Count > 0);
        return anchors.ToArray();
    }
}
