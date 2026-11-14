using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Overworld;

internal enum CaveExitFailureReason
{
    Enclosed,
    SoleWalkableWaterExit
}

internal readonly record struct CaveExitFailure(IntVector2 CavePosition, CaveExitFailureReason Reason);

/// <summary>Checks local cave escape terrain, independently of sideview connections and inventory routing.</summary>
internal static class CaveExitSafety
{
    /// <summary>Replaces a cave's sole WALKABLEWATER exit with ROAD so leaving does not require Boots.</summary>
    /// <remarks>
    /// Scans logical map cells in row order and stops at the first enclosed cave,
    /// retaining the legacy partial-repair behavior on failure. Padding is ignored.
    /// Two water exits and PREPLACED_WATER_WALKABLE retain their existing treatment;
    /// this is a narrow repair rule, not a proof of inventory-aware solvability.
    /// </remarks>
    internal static void RepairSoleWaterExits(OverworldMap map, int rows, int columns)
    {
        foreach (var cave in EnumerateCaves(map, rows, columns))
        {
            var (count, soleExit) = InspectExits(map, rows, columns, cave);
            // Link cannot turn back after emerging from a cave, so an enclosed exit is fatal.
            if (count == 0)
                return;
            if (count == 1 && map[soleExit] == Terrain.WALKABLEWATER)
                map[soleExit] = Terrain.ROAD;
        }
    }

    /// <summary>Returns the first remaining failure, or null. Never changes terrain or gameplay state.</summary>
    internal static CaveExitFailure? FindFailure(OverworldMap map, int rows, int columns)
    {
        foreach (var cave in EnumerateCaves(map, rows, columns))
        {
            var (count, soleExit) = InspectExits(map, rows, columns, cave);
            if (count == 0)
                return new(cave, CaveExitFailureReason.Enclosed);
            if (count == 1 && map[soleExit] == Terrain.WALKABLEWATER)
                return new(cave, CaveExitFailureReason.SoleWalkableWaterExit);
        }
        return null;
    }

    private static IEnumerable<IntVector2> EnumerateCaves(OverworldMap map, int rows, int columns)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (map[y, x] == Terrain.CAVE) yield return new(x, y);
            }
        }
    }

    private static (int Count, IntVector2 SoleExit) InspectExits(OverworldMap map, int rows, int columns, IntVector2 cave)
    {
        int count = 0;
        IntVector2 soleExit = default;
        foreach (var direction in IntVector2.CARDINALS)
        {
            var exit = cave + direction;
            if (exit.X < 0 || exit.X >= columns || exit.Y < 0 || exit.Y >= rows)
                continue;
            if (!map[exit].IsWalkable(true))
                continue;
            count++;
            soleExit = exit;
        }
        return (count, soleExit);
    }
}
