using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

internal enum OceanPaintResult
{
    Completed,
    Blocked,
    BackingMapBoundsFailure
}

/// <summary>Shared biome geometry with explicit map, dimension, climate and RNG inputs.</summary>
/// <remarks>Operations paint in place. Callers own phase ordering, reservations and supported map dimensions.</remarks>
internal static class TerrainLayout
{
    /// <summary>Paints the logical perimeter, preserving the interior and backing padding.</summary>
    internal static void PaintBorder(OverworldMap map, int rows, int columns, Terrain terrain)
    {
        for (int x = 0; x < columns; x++)
        {
            map[0, x] = terrain;
            map[rows - 1, x] = terrain;
        }
        for (int y = 0; y < rows; y++)
        {
            map[y, 0] = terrain;
            map[y, columns - 1] = terrain;
        }
    }

    /// <summary>Paints the eight neighbors of an interior cell, retaining its center and all other cells.</summary>
    internal static void PaintSurroundings(OverworldMap map, IntVector2 center, Terrain terrain)
    {
        foreach (var offset in IntVector2.DIRECTIONS) map[center + offset] = terrain;
    }

    /// <summary>Paints a cave with mountains behind it and on either side, leaving its approach open.</summary>
    internal static void DrawCaveEntrance(OverworldMap map, IntVector2 position, IntVector2 intoMountain)
    {
        var side = intoMountain.Perpendicular();
        map[position] = Terrain.CAVE;
        map[position + intoMountain] = Terrain.MOUNTAIN;
        map[position - side] = Terrain.MOUNTAIN;
        map[position + side] = Terrain.MOUNTAIN;
    }

    /// <summary>Seeds a sampled stretch of one map edge, retaining earlier writes if painting stops.</summary>
    /// <remarks>
    /// Non-cardinal directions retain the south-edge fallback. Only the decreasing
    /// east/west scan reports backing-map bounds failures instead of throwing.
    /// </remarks>
    internal static OceanPaintResult DrawOcean(OverworldMap map, int rows, int columns,
        Direction edge, Terrain water, bool overwrite, Random rng)
    {
        bool vertical = edge is Direction.EAST or Direction.WEST;
        int edgeLength = vertical ? rows : columns;
        int startCoordinate = rng.Next(edgeLength);
        int length = rng.Next(Math.Max(startCoordinate, edgeLength - startCoordinate));
        var start = vertical
            ? new IntVector2(edge == Direction.WEST ? 0 : columns - 1, startCoordinate)
            : new IntVector2(startCoordinate, edge == Direction.NORTH ? 0 : rows - 1);
        bool increasing = startCoordinate < edgeLength / 2;
        var step = vertical ? IntVector2.SOUTH : IntVector2.EAST;
        if (!increasing) step = -step;

        OceanPaintResult Paint()
        {
            for (int i = 0; i < length; i++)
            {
                var position = start + i * step;
                Terrain current = map[position];
                if (current != Terrain.NONE && !overwrite) return OceanPaintResult.Blocked;
                map[position] = water;
            }
            return OceanPaintResult.Completed;
        }

        if (vertical && !increasing)
        {
            try { return Paint(); }
            catch (IndexOutOfRangeException) { return OceanPaintResult.BackingMapBoundsFailure; }
        }
        return Paint();
    }

    /// <summary>Paints a two-leg river on empty forward cells, with reservation-aware sideways expansion.</summary>
    internal static void DrawRiver(OverworldMap map, int rows, int columns, Terrain water,
        Func<IntVector2, Terrain, bool> isReserved, Random rng)
    {
        bool InBounds(IntVector2 pos) => pos.X >= 0 && pos.X < columns && pos.Y >= 0 && pos.Y < rows;

        Direction direction1 = DirectionExtensions.RandomCardinal(rng);
        Direction direction2;
        do
        {
            direction2 = DirectionExtensions.RandomCardinal(rng);
        } while (direction1 == direction2);

        (IntVector2 Start, IntVector2 Delta) ChooseEntry(Direction direction)
        {
            int rx = rng.Next(columns / 3, (columns / 3) * 2);
            int ry = rng.Next(rows / 3, (rows / 3) * 2);
            IntVector2 start = direction switch
            {
                Direction.SOUTH => new(rx, 0),
                Direction.WEST => new(columns - 1, ry),
                Direction.NORTH => new(rx, rows - 1),
                Direction.EAST => new(0, ry),
                _ => throw new Exception("Invalid Direction")
            };
            IntVector2 delta = direction.ToIntVector2();
            return (start, delta);
        }

        IntVector2 PaintStep(IntVector2 pos, IntVector2 sideDir)
        {
            // Forward painting requires an empty cell; only sideways expansion consults reservations.
            if (map[pos] != Terrain.NONE) { return pos; }
            map[pos] = water;
            int minAdj = (sideDir.X != 0 && pos.X == 1) || (sideDir.Y != 0 && pos.Y == 1) ? 0 : -1;
            int maxAdj = (sideDir.X != 0 && pos.X == columns - 2) || (sideDir.Y != 0 && pos.Y == rows - 2) ? 0 : 1;
            int adjust = rng.Next(minAdj, maxAdj + 1);
            IntVector2 adjusted = pos + adjust * sideDir;
            if (InBounds(adjusted) && !isReserved(adjusted, map[adjusted]))
            {
                map[adjusted] = water;
            }
            return adjusted;
        }

        var (start, delta) = ChooseEntry(direction1);
        IntVector2 sideDir = delta.Perpendicular();
        int stopping = delta.Y != 0 ? rng.Next(rows / 3, (rows / 3) * 2) : rng.Next(columns / 3, (columns / 3) * 2);

        for (int i = 0; i < stopping; i++)
        {
            start = PaintStep(start, sideDir);
            start += delta;
        }

        // Retain the second entry sample even though the turn continues from the current position.
        var (_, delta2) = ChooseEntry(direction2);
        delta = delta2;
        sideDir = delta.Perpendicular();

        while (InBounds(start))
        {
            start = PaintStep(start, sideDir);
            start += delta;
        }
    }

    /// <summary>Paints a meandering canyon and climate-sampled banks, overwriting existing cells where geometry is drawn.</summary>
    internal static void DrawCanyon(OverworldMap map, int rows, int columns, Terrain riverTerrain,
        Climate climate, IEnumerable<Terrain> bankTerrains, bool horizontal, Random rng)
        => DrawCanyon(map, rows, columns, riverTerrain, climate, bankTerrains, horizontal, rng, islandStarts: null);

    private static void DrawCanyon(OverworldMap map, int rows, int columns, Terrain riverTerrain,
        Climate climate, IEnumerable<Terrain> bankTerrains, bool horizontal, Random rng, int[]? islandStarts)
    {
        IntVector2 forward = horizontal ? IntVector2.EAST : IntVector2.SOUTH;
        IntVector2 side = horizontal ? IntVector2.SOUTH : IntVector2.EAST;
        int length = horizontal ? columns : rows;
        int width = horizontal ? rows : columns;
        bool coastal = islandStarts is not null;
        int margin = Math.Min(width / 2 - 1, 15);
        int minimumRiverOffset = coastal ? width / 4 : margin;
        int maximumRiverOffset = coastal ? width * 3 / 5 : width - margin - 1;
        int oceanBandWidth = Math.Clamp(width / 12, 2, 5);
        int leftBankSteps = rng.Next(0, 5);
        int rightBankSteps = coastal ? 0 : rng.Next(0, 5);
        Terrain leftBankTerrain = climate.GetRandomTerrain(rng, bankTerrains);
        Terrain rightBankTerrain = coastal ? Terrain.NONE : climate.GetRandomTerrain(rng, bankTerrains);
        int riverOffset = rng.Next(minimumRiverOffset, maximumRiverOffset + 1);

        for (int step = 0; step < length; step++)
        {
            leftBankSteps++;
            IntVector2 basePos = step * forward;
            map[basePos + riverOffset * side] = riverTerrain;
            map[basePos + (riverOffset + 1) * side] = riverTerrain;

            // Retain the different horizontal/vertical wandering ranges.
            int shift = rng.Next(-3, horizontal ? 3 : 4);
            int leftMountainDistance = rng.Next(14, 17);
            if (coastal) leftMountainDistance = Math.Min(leftMountainDistance, width / 4);
            if (riverOffset - leftMountainDistance > 0)
            {
                // Keep a 3x3 placement area open on the narrow coastal bank.
                int seedDistanceFromCliff = coastal && width < 44 ? 1 : 3;
                map[basePos + (riverOffset - leftMountainDistance + seedDistanceFromCliff) * side] = leftBankTerrain;
            }
            if (leftBankSteps % 5 == 0)
            {
                leftBankTerrain = climate.GetRandomTerrain(rng, bankTerrains);
            }
            int leftMountainEdge = riverOffset - leftMountainDistance;
            if (coastal) leftMountainEdge = Math.Max(0, leftMountainEdge);
            for (int i = leftMountainEdge; i >= 0; i--)
            {
                map[basePos + i * side] = Terrain.MOUNTAIN;
            }
            if (!coastal)
            {
                rightBankSteps++;
                int rightMountainDistance = rng.Next(14, 17);
                if (riverOffset + rightMountainDistance < width)
                {
                    map[basePos + (riverOffset + rightMountainDistance - 3) * side] = rightBankTerrain;
                }
                if (rightBankSteps % 5 == 0)
                {
                    rightBankTerrain = climate.GetRandomTerrain(rng, bankTerrains);
                }
                for (int i = riverOffset + 1 + rightMountainDistance; i < width; i++)
                {
                    map[basePos + i * side] = Terrain.MOUNTAIN;
                }
            }
            while (riverOffset + shift > maximumRiverOffset || riverOffset + shift < minimumRiverOffset)
            {
                shift = rng.Next(-1, 2);
            }
            int previousOffset = riverOffset;
            riverOffset += shift;
            for (int s = previousOffset; shift > 0 ? s < riverOffset : s > riverOffset; s += Math.Sign(shift))
            {
                map[basePos + s * side] = riverTerrain;
            }

            if (islandStarts is not null)
            {
                // The outer water tile follows the actual river step, not the prepainted border.
                int lastRiverTile = Math.Max(previousOffset + 1, riverOffset - 1);
                int islandStart = lastRiverTile + 1 + oceanBandWidth;
                islandStarts[step] = islandStart;
                for (int i = lastRiverTile + 1; i < islandStart; i++)
                {
                    map[basePos + i * side] = riverTerrain;
                }
            }
        }
    }

    /// <summary>Seeds a mountain-backed coast, an open channel and water stripes for offshore islands.</summary>
    internal static void DrawCoastalIslands(OverworldMap map, int rows, int columns, Terrain water,
        Climate climate, IEnumerable<Terrain> bankTerrains, bool horizontal, Random rng)
    {
        PaintBorder(map, rows, columns, water);
        IntVector2 forward = horizontal ? IntVector2.EAST : IntVector2.SOUTH;
        IntVector2 seaward = horizontal ? IntVector2.SOUTH : IntVector2.EAST;
        int length = horizontal ? columns : rows;
        int width = horizontal ? rows : columns;

        // Canyon drawing supplies the first offshore tile for each step. Unseeded
        // mainland tiles remain available to place towns, caves and palaces.
        var islandStarts = new int[length];
        DrawCanyon(map, rows, columns, water, climate, bankTerrains, horizontal, rng, islandStarts);

        int crossStripes = length >= 44 ? 3 : 2;
        for (int stripe = 1; stripe <= crossStripes; stripe++)
        {
            int step = stripe * length / (crossStripes + 1) + rng.Next(-2, 3);
            for (int offshore = islandStarts[step]; offshore < width - 1; offshore++)
            {
                map[step * forward + offshore * seaward] = water;
            }
        }

        int furthestCoast = 0;
        foreach (int islandStart in islandStarts) furthestCoast = Math.Max(furthestCoast, islandStart);
        // Keep room for a 3x3 location on either side of the offshore divider.
        if (furthestCoast + 3 < width - 4)
        {
            int offshoreStripe = rng.Next(furthestCoast + 3, width - 4);
            for (int step = 1; step < length - 1; step++)
            {
                map[step * forward + offshoreStripe * seaward] = water;
            }
        }
    }

    /// <summary>Paints the existing central mountain shape. Callers must supply dimensions that fit its fixed profile.</summary>
    internal static void DrawCentralMountain(OverworldMap map, int rows, int columns, bool horizontal)
    {
        IntVector2 forward = horizontal ? IntVector2.SOUTH : IntVector2.EAST;
        IntVector2 side = horizontal ? IntVector2.EAST : IntVector2.SOUTH;
        int forwardLen = horizontal ? rows : columns;
        int sideLen = horizontal ? columns : rows;
        int top = (forwardLen - 35) / 2;
        int bottom = forwardLen - top;
        int sideCenter = sideLen / 2;

        // Block out stripes outside caldera zone
        for (int f = 0; f < forwardLen; f++)
        {
            if (f < top || f > bottom)
            {
                for (int s = 0; s < sideLen; s++)
                {
                    map[f * forward + s * side] = Terrain.MOUNTAIN;
                }
            }
        }

        // Top triangle (widening)
        for (int y = 0; y < 8; y++)
        {
            int half = 3 + y;
            for (int s = sideCenter - half; s < sideCenter + half; s++)
            {
                map[(top + y) * forward + s * side] = Terrain.MOUNTAIN;
            }
        }

        // Middle rectangle
        for (int i = 0; i < 19; i++)
        {
            for (int s = sideCenter - 10; s < sideCenter + 10; s++)
            {
                map[(top + 8 + i) * forward + s * side] = Terrain.MOUNTAIN;
            }
        }

        // Bottom triangle (narrowing), retaining the existing narrower far tip.
        for (int i = 0; i < 8; i++)
        {
            int half = 3 + (6 - i);
            for (int s = sideCenter - half; s < sideCenter + half; s++)
            {
                map[(top + 27 + i) * forward + s * side] = Terrain.MOUNTAIN;
            }
        }
    }

    /// <summary>Walks the interior of a cardinal mountain band, excluding both endpoints.</summary>
    /// <remarks>
    /// Each step reduces the greatest remaining axis distance; ties advance X.
    /// The caller paints the starting tile separately and leaves the final endpoint alone.
    /// </remarks>
    internal static IEnumerable<IntVector2> MountainBandInterior(IntVector2 start, IntVector2 end)
    {
        var current = start;
        while (current != end)
        {
            var remaining = end - current;
            current += Math.Abs(remaining.X) >= Math.Abs(remaining.Y)
                ? new IntVector2(Math.Sign(remaining.X), 0)
                : new IntVector2(0, Math.Sign(remaining.Y));
            if (current != end) yield return current;
        }
    }

    /// <summary>Seeds distinct columns then rows on NONE cells. Each axis must offer at least three positions.</summary>
    internal static void DrawStripes(OverworldMap map, int rows, int columns, Terrain terrain,
        int nearEdgeMargin, int farEdgeMargin, Random rng)
    {
        int remainingColumns = rng.Next(2, 4);
        int remainingRows = rng.Next(2, 4);
        List<int> selectedRows = [];

        DrawColumnStripes(map, rows, columns, terrain, nearEdgeMargin, farEdgeMargin, remainingColumns, rng);

        while (remainingRows > 0)
        {
            int row = rng.Next(nearEdgeMargin, rows - farEdgeMargin);
            if (!selectedRows.Contains(row))
            {
                for (int i = 0; i < columns; i++)
                {
                    if (map[row, i] == Terrain.NONE)
                    {
                        map[row, i] = terrain;
                    }
                }
                selectedRows.Add(row);
                remainingRows--;
            }
        }
    }

    /// <summary>
    /// Seeds water between islands: straight columns followed by rows that can
    /// shift vertically when they encounter water. Only unseeded tiles are painted.
    /// </summary>
    internal static void DrawIslandStripes(OverworldMap map, int rows, int columns, Terrain water, Random rng)
    {
        // Roll both counts before choosing positions to retain the RNG sequence.
        int remainingColumnStripes = rng.Next(2, 4);
        int remainingRowStripes = rng.Next(2, 4);
        List<int> rowStripeEndRows = [];

        // The right border may be selected and count as a stripe without new painting.
        DrawColumnStripes(map, rows, columns, water, 1, 0, remainingColumnStripes, rng);

        while (remainingRowStripes > 0)
        {
            int currentRow = rng.Next(5, rows - 6);
            // Legacy rule: reject starts matching a previous stripe's END row,
            // not its start row or every row it crossed.
            if (rowStripeEndRows.Contains(currentRow)) continue;

            for (int x = 0; x < columns; x++)
            {
                if (map[currentRow, x] == Terrain.NONE)
                {
                    map[currentRow, x] = water;
                }
                else if (map[currentRow, x] == water)
                {
                    // At existing water (including the border), choose a shift
                    // for subsequent columns. Zero is allowed; no vertical segment
                    // is painted here. Retry shifts that would leave the interior.
                    int rowShift = rng.Next(-3, 4);
                    while (currentRow + rowShift < 1 || currentRow + rowShift > rows - 2)
                    {
                        rowShift = rng.Next(-3, 4);
                    }
                    currentRow += rowShift;
                }
            }
            rowStripeEndRows.Add(currentRow);
            remainingRowStripes--;
        }
    }

    /// <summary>Samples distinct columns within the supplied margins, painting only NONE cells.</summary>
    private static void DrawColumnStripes(OverworldMap map, int rows, int columns, Terrain terrain,
        int nearMargin, int farMargin, int remaining, Random rng)
    {
        List<int> selectedColumns = [];
        while (remaining > 0)
        {
            int col = rng.Next(nearMargin, columns - farMargin);
            if (!selectedColumns.Contains(col))
            {
                for (int i = 0; i < rows; i++)
                {
                    if (map[i, col] == Terrain.NONE)
                    {
                        map[i, col] = terrain;
                    }
                }
                selectedColumns.Add(col);
                remaining--;
            }
        }
    }
}
