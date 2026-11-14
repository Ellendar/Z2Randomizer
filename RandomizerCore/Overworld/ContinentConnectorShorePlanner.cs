using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>
/// Selects raft and bridge shores for one preparation phase, without painting or
/// reserving terrain. World paints the raft ocean before selecting the bridge;
/// the planner reads that same live map so its feasibility checks see the ocean.
/// Create a new planner for each phase rather than retaining attempt state.
/// </summary>
internal sealed class ContinentConnectorShorePlanner(ContinentConnectorOrientationPolicy policy, Random rng, OverworldMap map,
    IReadOnlyCollection<Terrain> landTerrains, IEnumerable<IntVector2> occupied, Biome biome)
{
    internal Direction SelectRaftEdge()
        => policy.RaftEdges.Count == 1 ? policy.RaftEdges[0] : policy.RaftEdges[rng.Next(policy.RaftEdges.Count)];

    /// <summary>
    /// Prefers shores where a bridge approach currently fits, excluding the raft's
    /// edge. If none fits, later reservation rejects the candidate as usual.
    /// This selects an edge without painting terrain; later placement can still
    /// invalidate it, so reservation and final validation remain authoritative.
    /// </summary>
    internal Direction SelectBridgeEdge(Direction raftEdge)
    {
        IReadOnlyList<Direction> edges = policy.BridgeEdges;
        var viable = edges.Where(edge => ContinentConnectorReservation
            .CanPlan(map, biome, World.ConnectorKind.Bridge, edge, landTerrains, occupied, []))
            .ToArray();
        if (viable.Any(edge => edge != raftEdge))
        {
            edges = viable;
        }
        Direction bridgeEdge;
        do { bridgeEdge = edges[rng.Next(edges.Count)]; }
        while (bridgeEdge == raftEdge);
        return bridgeEdge;
    }
}
