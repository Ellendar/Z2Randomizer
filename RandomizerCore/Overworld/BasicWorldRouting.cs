using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>Checks terrain connectivity assuming continent entrances and all capabilities are available, except Boots when requested.</summary>
internal static class BasicWorldRouting
{
    /// <summary>
    /// Returns whether every required location can be reached from the supplied roots.
    /// Traverses orthogonal walkable terrain and ordered, directed sideview exits.
    /// Does not evaluate inventory, location requirements, or town traversal rules.
    /// </summary>
    /// <remarks>
    /// Coordinates are zero-based. Roots are processed even on blocked terrain.
    /// Empty roots and invalid root/exit coordinates retain the existing exceptions.
    /// Only the first still-unreached required location at a coordinate is consumed;
    /// exits are followed only for that location. Collocated targets are not merged.
    /// The work list and visitation grid are local; inputs and world visitation remain unchanged.
    /// Walkable water only counts as terrain when <paramref name="assumeBoots"/> is set,
    /// because crossing it needs Boots.
    /// </remarks>
    internal static bool CanReachRequiredLocations(OverworldMap map, int rows, int columns,
        IReadOnlyList<Location> roots, IEnumerable<Location> requiredLocations,
        Func<Location, IEnumerable<Location>> exits,
        bool assumeBoots = true)
    {
        var remaining = requiredLocations.ToList();
        var visited = new bool[rows, columns];
        var pending = new Queue<IntVector2>();
        foreach (var root in roots)
        {
            pending.Enqueue(root.Pos);
        }

        // Intentionally process a first root unconditionally: empty roots currently throw.
        do
        {
            var position = pending.Dequeue();
            int y = position.Y, x = position.X;

            if (visited[y, x])
                continue;
            visited[y, x] = true;
            var here = remaining.FirstOrDefault(l => l.Pos == position);
            if (here is not null)
            {
                remaining.Remove(here);
                foreach (var exit in exits(here))
                {
                    pending.Enqueue(exit.Pos);
                }
            }

            if (x > 0 && map[y, x - 1].IsWalkable(assumeBoots))           { pending.Enqueue(new(x - 1, y)); }
            if (x < columns - 1 && map[y, x + 1].IsWalkable(assumeBoots)) { pending.Enqueue(new(x + 1, y)); }
            if (y > 0 && map[y - 1, x].IsWalkable(assumeBoots))           { pending.Enqueue(new(x, y - 1)); }
            if (y < rows - 1 && map[y + 1, x].IsWalkable(assumeBoots))    { pending.Enqueue(new(x, y + 1)); }
        } while (pending.Count > 0);

        return remaining.Count == 0;
    }
}
