using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

internal readonly record struct LocationSearchResult(IntVector2? Position, int Samples);

/// <summary>Bounded, read-only placement searches. Callers own eligibility, painting and location updates.</summary>
internal static class LocationPlacementSearch
{
    /// <summary>Samples interior centers with an empty 3x3 area, optionally excluding one column.</summary>
    /// <remarks>Reports actual coordinate samples so a caller can share one budget across locations.</remarks>
    internal static LocationSearchResult FindEmptyArea(OverworldMap map, int rows, int columns,
        Random rng, int sampleBudget, int? excludedColumn = null)
    {
        int samples = 0;
        for (; samples < sampleBudget; samples++)
        {
            var position = new IntVector2(rng.Next(columns - 2) + 1, rng.Next(rows - 2) + 1);
            if (!map.AllTerrainIn3x3Equals(position, Terrain.NONE) || position.X == excludedColumn) continue;
            return new(position, samples + 1);
        }
        return new(null, samples);
    }

    /// <summary>Samples matching terrain without colliding with current locations or connector reservations.</summary>
    /// <remarks>
    /// Edge cells and diagonal neighbors are allowed. Cardinal bridge/cave/rock/river-devil
    /// neighbors block placement; other terrain types block only an exact overlap.
    /// The moving location may retain its own tile. Location positions are read live,
    /// so later calls observe earlier placements. Map, locations and reservations are unchanged.
    /// </remarks>
    internal static LocationSearchResult FindHiddenPosition(OverworldMap map, int rows, int columns,
        Location moving, IReadOnlyList<Location> locations, IReadOnlyCollection<ContinentConnectorReservation> reservations,
        Random rng, int sampleBudget = 2000)
    {
        int samples = 0;
        for (; samples < sampleBudget; samples++)
        {
            var position = IntVector2.Random(rng, columns, rows);
            if (reservations.Any(r => r.Contains(position))) continue;
            if (map[position] != moving.TerrainType) continue;
            if (locations.Any(other => other != moving
                && (other.Pos - position).ManhattanLength <= 1
                && (other.Pos == position || BlocksAdjacentPlacement(other.TerrainType)))) continue;
            return new(position, samples + 1);
        }
        return new(null, samples);
    }

    private static bool BlocksAdjacentPlacement(Terrain terrain)
        => terrain is Terrain.BRIDGE or Terrain.CAVE or Terrain.ROCK or Terrain.RIVER_DEVIL;
}
