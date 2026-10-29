using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>A compatible cave pair, found without painting terrain or moving locations.</summary>
internal readonly record struct SaneCavePlacement(IntVector2 First, IntVector2 Second, IntVector2 DirectionIn)
{
    /// <summary>
    /// Searches empty origins and directions without replacement, then samples a
    /// compatible partner. An unlucky origin cannot reject an otherwise usable map.
    /// The finite search retains the legacy origin/partner margins, direction
    /// clearance, spacing and ±3 lateral offset. Crossing terrain, when required,
    /// may occur anywhere in the inclusive rectangle between the two entrances.
    /// </summary>
    internal static SaneCavePlacement? Find(OverworldMap map, int rows, int columns, Random rng,
        int minimumSpacing, int maximumSpacingExclusive, Terrain? crossingTerrain)
    {
        if (rows <= 10 || columns <= 10) return null;
        var empty = EmptyCenters(map, rows, columns);
        var crossings = crossingTerrain is { } terrain ? CrossingCounts(map, rows, columns, terrain) : null;
        List<IntVector2> origins = [];
        for (int y = 5; y < rows - 5; y++)
            for (int x = 5; x < columns - 5; x++)
                if (empty[y, x]) origins.Add(new(x, y));

        var clearance = new IntVector2(Math.Min(columns / 2 - 1, 15), Math.Min(rows / 2 - 1, 15));
        Span<IntVector2> directions = stackalloc IntVector2[4];
        List<IntVector2> partners = [];
        while (origins.Count > 0)
        {
            int selected = rng.Next(origins.Count);
            var first = origins[selected];
            origins[selected] = origins[^1];
            origins.RemoveAt(origins.Count - 1);
            IntVector2.CARDINALS.AsSpan().CopyTo(directions);
            rng.Shuffle(directions);
            foreach (var direction in directions)
            {
                var furthest = first + direction.ComponentMultiply(clearance);
                if (furthest.X < 0 || furthest.X >= columns || furthest.Y < 0 || furthest.Y >= rows) continue;
                partners.Clear();
                for (int spacing = minimumSpacing; spacing < maximumSpacingExclusive; spacing++)
                {
                    for (int lateral = -3; lateral <= 3; lateral++)
                    {
                        var second = first + spacing * direction + lateral * direction.Perpendicular();
                        if (second.X < 1 || second.X >= columns - 1 || second.Y < 1 || second.Y >= rows - 1
                            || !empty[second.Y, second.X]) continue;
                        if (crossings is not null && !ContainsCrossing(crossings, first, second)) continue;
                        partners.Add(second);
                    }
                }
                if (partners.Count > 0) return new(first, partners[rng.Next(partners.Count)], direction);
            }
        }
        return null;
    }

    /// <summary>Samples a partner in front of the first position with up to three tiles of sideways offset.</summary>
    internal static IntVector2? SamplePartner(Random rng, IntVector2 first, IntVector2 direction, int maximumAttempts,
        Func<int> rollSpacing, Func<IntVector2, bool> canPlacePartner)
    {
        var perpendicular = direction.Perpendicular();
        for (int attempt = 0; attempt < maximumAttempts; attempt++)
        {
            int spacing = rollSpacing();
            int lateralOffset = rng.Next(-3, 4);
            var position = first + spacing * direction + lateralOffset * perpendicular;
            if (canPlacePartner(position)) return position;
        }

        return null;
    }

    private static bool[,] EmptyCenters(OverworldMap map, int rows, int columns)
    {
        var empty = new bool[rows, columns];
        for (int y = 1; y < rows - 1; y++)
        {
            for (int x = 1; x < columns - 1; x++)
            {
                var pos = new IntVector2(x, y);
                if (map[pos] != Terrain.NONE) continue;
                empty[y, x] = map.AllTerrainIn3x3Equals(pos, Terrain.NONE);
            }
        }
        return empty;
    }

    // Summed-area counts keep each crossing query constant-time even when many
    // empty origins have no compatible partner. Rebuilt for each changed layout.
    private static int[,] CrossingCounts(OverworldMap map, int rows, int columns, Terrain terrain)
    {
        var counts = new int[rows + 1, columns + 1];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
                counts[y + 1, x + 1] = counts[y, x + 1] + counts[y + 1, x] - counts[y, x]
                    + (map[y, x] == terrain ? 1 : 0);
        return counts;
    }

    private static bool ContainsCrossing(int[,] counts, IntVector2 first, IntVector2 second)
    {
        int left = Math.Min(first.X, second.X), right = Math.Max(first.X, second.X) + 1;
        int top = Math.Min(first.Y, second.Y), bottom = Math.Max(first.Y, second.Y) + 1;
        return counts[bottom, right] - counts[top, right] - counts[bottom, left] + counts[top, left] > 0;
    }
}
