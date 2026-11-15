using System;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract partial class World
{
    private const int MinimumLengthRequiringWidePassage = 6;

    private readonly record struct MountainLane(IntVector2 Side, int FirstTile, int LastTile)
    {
        public int Length => LastTile - FirstTile + 1;
    }

    private MountainLane[] PlanMountainFunnel(RegionCrossingSpan span, int firstTile, int lastTile, bool openAtFirstTile, IntVector2 preferredSide)
    {
        if (FindMountainLane(span, firstTile, lastTile, preferredSide) is not { } lane) { return []; }

        // Keep the shoulder two wide until it reaches the mouth, then open the other side.
        int flareLength = Math.Max(1, lane.Length / 3);
        int flareFirst = openAtFirstTile ? lane.FirstTile : lane.LastTile - flareLength + 1;
        int flareLast = openAtFirstTile ? lane.FirstTile + flareLength - 1 : lane.LastTile;
        var flare = FindMountainLaneOnSide(span, flareFirst, flareLast, -lane.Side, minimumLength: 1);
        return flare is { } extra ? [lane, extra] : [lane];
    }

    private MountainLane[] PlanMountainDoubleNeck(RegionCrossingSpan span, int firstTile, int lastTile, IntVector2 preferredSide)
    {
        if (lastTile - firstTile < 1) { return []; }

        // Shift the shoulder to the other side through a three-tile-wide middle.
        int centerLength = Math.Max(1, (lastTile - firstTile + 1) / 3);
        int middle = firstTile + (lastTile - firstTile) / 2;
        int centerFirst = middle - (centerLength - 1) / 2;
        int centerLast = centerFirst + centerLength - 1;

        foreach (var side in new[] { preferredSide, -preferredSide })
        {
            var firstLane = FindMountainLaneOnSide(span, firstTile, centerLast, side, minimumLength: 2);
            var secondLane = FindMountainLaneOnSide(span, centerFirst, lastTile, -side, minimumLength: 2);
            if (firstLane is { } first && secondLane is { } second
                && first.LastTile >= second.FirstTile
                && second.LastTile >= first.FirstTile)
            {
                return [first, second];
            }
        }

        // Tight terrain can fit a single shoulder between the necks.
        return FindMountainLane(span, firstTile, lastTile, preferredSide) is { } lane ? [lane] : [];
    }

    private MountainLane? FindMountainLane(RegionCrossingSpan span, int firstTile, int lastTile, IntVector2 preferredSide)
    {
        var preferred = FindMountainLaneOnSide(span, firstTile, lastTile, preferredSide, minimumLength: 2);
        var alternate = FindMountainLaneOnSide(span, firstTile, lastTile, -preferredSide, minimumLength: 2);
        if (alternate is { } other && (preferred is null || other.Length > preferred.Value.Length))
        {
            return other;
        }
        return preferred;
    }

    private MountainLane? FindMountainLaneOnSide(RegionCrossingSpan span, int firstTile, int lastTile, IntVector2 side,
        int minimumLength)
    {
        MountainLane? longest = null;
        int runStart = firstTile;
        for (int tileIndex = firstTile; tileIndex <= lastTile; tileIndex++)
        {
            if (!CanWidenMountainTile(span, tileIndex, side))
            {
                runStart = tileIndex + 1;
                continue;
            }

            int runLength = tileIndex - runStart + 1;
            if (runLength >= minimumLength && (longest is null || runLength > longest.Value.Length))
            {
                longest = new MountainLane(side, runStart, tileIndex);
            }
        }
        return longest;
    }

    private bool CanWidenMountainTile(RegionCrossingSpan span, int tileIndex, IntVector2 side)
    {
        IntVector2 center = MountainCrossingTile(span, tileIndex);
        IntVector2 target = center + side;
        if (!WithinMapBounds(target, 1)
            || map[target] != Terrain.MOUNTAIN
            || map[target + side].IsWalkable(true)
            || map[center - side].IsWalkable(true)
            || TouchesContinentConnectorReservation(target)
            || LocationsAtOrOrthogonalTo(target).Any())
        {
            return false;
        }

        if (tileIndex == 0)
            return walkableTerrains.Contains(map[span.End + side]);
        if (tileIndex == span.Length - 1)
            return walkableTerrains.Contains(map[span.Start + side]);
        return true;
    }

    private MountainLane[] PlanDaruniaMountainLanes(RegionCrossingSpan span, IntVector2 farEncounter, int farTile)
    {
        int neckLength = MountainNeckLength(span.Length - farTile);
        // A passable flank needs an extra tile of separation from the encounter.
        int farNeck = HasPassableMountainFlank(farEncounter, span.Side) ? Math.Max(2, neckLength) : neckLength;
        int nearNeck = HasPassableMountainFlank(span.Start + span.Forward, span.Side) ? Math.Max(2, neckLength) : neckLength;
        return PlanMountainDoubleNeck(span, farTile + farNeck, span.Length - 1 - nearNeck, span.Side);
    }

    private static int MountainNeckLength(int length) => Math.Max(1, length / 4);

    private bool HasPassableMountainFlank(IntVector2 position, IntVector2 side) =>
        map[position + side].IsWalkable(true) || map[position - side].IsWalkable(true);

    private static IntVector2 MountainCrossingTile(RegionCrossingSpan span, int tileIndex) =>
        span.End - (tileIndex + 1) * span.Forward;
}
