using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Z2Randomizer.RandomizerCore.Diagnostics;

namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract partial class World
{
    protected bool[,] visitation;
    public bool AllReached { get; set; }

    /// <summary>Checks that the continent connectors can reach the standard required locations with all capabilities available.</summary>
    /// <remarks>Requires at least one loaded continent connector.</remarks>
    public bool ValidateBasicRouting()
        => BasicWorldRouting.CanReachRequiredLocations(map, MapRows, MapColumns,
            GetContinentConnections(), RequiredLocations(false, false), GetLocalConnectionExits);

    //Should the visibility calculation table even be persistent? Why is this not just in scope of the calculation itself?
    /// <summary>Clears tile/location reachability and re-enables shuffling, retaining layout, start, and AllReached.</summary>
    public void ResetVisitabilityState()
    {
        ClearVisitationTiles();
        foreach (Location location in AllLocations)
        {
            location.CanShuffle = true;
            location.Reachable = false;
        }
    }

    /// <summary>Creates a logical-sized visitation grid without changing location state.</summary>
    protected void InitializeVisitation()
    {
        visitation = new bool[MapRows, MapColumns];
        ClearVisitationTiles();
    }

    /// <summary>Clears only tile visitation, retaining location flags, positions, start, and AllReached.</summary>
    protected void ClearVisitationTiles()
    {
        for (int i = 0; i < MapRows; i++)
        {
            for (int j = 0; j < MapColumns; j++)
            {
                visitation[i, j] = false;
            }
        }
    }

    /// <summary>Seeds gameplay visitation from the selected starting location.</summary>
    internal void VisitStart()
    {
        if (startLocation is not { } start) return;

        visitation[start.Y, start.Xpos] = true;
        start.Reachable = true;
    }

    public void VisitRaft()
    {
        if (raft != null)
        {
            visitation[raft.Y, raft.Xpos] = true;
        }
    }

    public void VisitBridge()
    {
        if (bridge != null)
        {
            visitation[bridge.Y, bridge.Xpos] = true;
        }
    }

    public void VisitCave1()
    {
        if (cave1 != null)
        {
            visitation[cave1.Y, cave1.Xpos] = true;
        }
    }

    public void VisitCave2()
    {
        if (cave2 != null)
        {
            visitation[cave2.Y, cave2.Xpos] = true;
        }
    }

    /// <summary>
    /// Updates the visitation matrix and location reachability
    /// </summary>
    public virtual void UpdateVisit(IReadOnlySet<RequirementType> requireables)
    {
        VisitStart();
        UpdateReachable(requireables);
        MarkReachableLocations(requireables);
    }

    protected void UpdateReachable(IReadOnlySet<RequirementType> requireables)
    {
        bool[,] covered = new bool[MapRows, MapColumns];
        foreach (Location start in GetPathingStarts())
        {
            if (start.Y >= 0 && start.Xpos >= 0)
            {
                VisitTerrain(covered, start.Pos, requireables);
            }
        }

        OnUpdateReachableTrigger();
    }

    /// <summary>Visits terrain from a starting row and column using the caller's coverage grid.</summary>
    protected void UpdateReachable(ref bool[,] covered, int start_y, int start_x, IReadOnlySet<RequirementType> requireables)
        => VisitTerrain(covered, new IntVector2(start_x, start_y), requireables);

    private void VisitTerrain(bool[,] covered, IntVector2 start, IReadOnlySet<RequirementType> requireables)
    {
        Stack<IntVector2> pending = new();
        pending.Push(start);
        while (pending.TryPop(out var position))
        {
            if (covered[position.Y, position.X]) continue;
            covered[position.Y, position.X] = true;

            if (!CanVisitTerrain(map[position], requireables)) continue;
            Location? location = GetLocationAt(position);
            if (location is not null && !location.AccessRequirements.AreSatisfiedBy(requireables)) continue;

            visitation[position.Y, position.X] = true;
            foreach (var direction in IntVector2.CARDINALS)
            {
                var neighbor = position + direction;
                if (WithinMapBounds(neighbor) && !covered[neighbor.Y, neighbor.X])
                {
                    pending.Push(neighbor);
                }
            }
        }
    }

    protected bool CanVisitTerrain(Terrain terrain, IReadOnlySet<RequirementType> requireables)
        => terrain is Terrain.LAVA or Terrain.BRIDGE or Terrain.CAVE or Terrain.ROAD or Terrain.PALACE or Terrain.TOWN
            || walkableTerrains.Contains(terrain)
            || ((terrain is Terrain.WALKABLEWATER or Terrain.PREPLACED_WATER_WALKABLE) && requireables.Contains(RequirementType.BOOTS))
            || (terrain == Terrain.ROCK && requireables.Contains(RequirementType.HAMMER))
            || (terrain == Terrain.RIVER_DEVIL && requireables.Contains(RequirementType.FLUTE));

    /// <summary>Extension hook after the shared terrain-visitation update.</summary>
    protected virtual void OnUpdateReachableTrigger()
    {
    }

    /// <summary>Marks visited locations and follows their local exits when access requirements are met.</summary>
    private protected void MarkReachableLocations(IReadOnlySet<RequirementType> requireables)
    {
        foreach (Location location in AllLocations)
        {
            if (location.Y <= 0 || !visitation[location.Y, location.Xpos]
                || !location.AccessRequirements.AreSatisfiedBy(requireables))
            {
                continue;
            }

            location.Reachable = true;
            var exits = GetLocalConnectionExits(location);
            if (exits.Count == 0 || (location.Town is not null && !location.Town.CanBeTraversed(requireables)))
            {
                continue;
            }

            foreach (Location exit in exits)
            {
                exit.Reachable = true;
                visitation[exit.Y, exit.Xpos] = true;
            }
        }
    }

    private protected virtual IReadOnlyList<Location> GetLocalConnectionExits(Location location)
        => connections.TryGetValue(location, out var exit) ? [exit] : [];

    public void UpdateAllReached()
    {
        if (AllReached)
        {
            return;
        }
        else
        {
            AllReached = true;
            foreach (Location location in AllLocations)
            {
                //TODO: Doing this off terrain type is a very bad idea for stuff like vanilla shuffle no actual terrain.
                if (location.TerrainType == Terrain.PALACE
                    || location.TerrainType == Terrain.TOWN
                    || location.GetAllCollectables().Any(i => !i.IsInternalUse()))
                {
                    if (!location.Reachable)
                    {
                        AllReached = false;
                        return;
                    }
                }
            }
        }
    }

    [Conditional("Z2R_DIAGNOSTICS")]
    public void DebugVisitation()
    {
        StringBuilder sb = new();
        for (int y = 0; y < MapRows; y++)
        {
            for (int x = 0; x < MapColumns; x++)
            {
                sb.Append(visitation[y, x] ? 'x' : ' ');
            }
            sb.AppendLine();
        }
        Diag.Block("terrain.visitation", sb.ToString());
    }
}
