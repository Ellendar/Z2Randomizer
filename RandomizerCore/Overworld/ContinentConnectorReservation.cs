using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>
/// A raft/bridge route chosen before growth: water from an edge to the landing,
/// followed by a two-deep, three-wide land approach. Where space allows, water
/// beside the raft landing keeps the dock flush with the shore. A bridge needs
/// a continuous strip of water alongside its entire span.
/// </summary>
internal sealed class ContinentConnectorReservation
{
    internal World.ConnectorKind Kind { get; }
    internal Direction Edge { get; }
    internal IntVector2 Landing { get; }
    private readonly IntVector2[] route;
    private readonly IntVector2[] waterBesideConnector;
    private readonly IntVector2[] approach;

    private ContinentConnectorReservation(World.ConnectorKind kind, Direction edge, IntVector2 landing,
        IntVector2[] route, IntVector2[] waterBesideConnector, IntVector2[] approach)
    {
        Kind = kind;
        Edge = edge;
        Landing = landing;
        this.route = route;
        this.waterBesideConnector = waterBesideConnector;
        this.approach = approach;
    }

    internal bool Contains(IntVector2 position) => route.Contains(position) || waterBesideConnector.Contains(position) || approach.Contains(position);

    /// <summary>
    /// Enumerates shores in a stable order, then samples one viable route. A bridge
    /// keeps the existing 2..10 water-tile limit; rafts may cross a wider sea.
    /// Planning is read-only, including failure. Existing reservations cannot overlap.
    /// </summary>
    internal static ContinentConnectorReservation? Plan(Random rng, OverworldMap map, Biome biome, World.ConnectorKind kind, Direction edge,
        IReadOnlyCollection<Terrain> landTerrains, IEnumerable<IntVector2> occupied,
        IReadOnlyCollection<ContinentConnectorReservation> reservations)
    {
        var candidates = FindCandidates(map, biome, kind, edge, landTerrains, occupied, reservations);
        return candidates.Count == 0 ? null : candidates[rng.Next(candidates.Count)];
    }

    /// <summary>Checks shore feasibility without painting terrain or advancing the generator RNG.</summary>
    internal static bool CanPlan(OverworldMap map, Biome biome, World.ConnectorKind kind, Direction edge,
        IReadOnlyCollection<Terrain> landTerrains, IEnumerable<IntVector2> occupied,
        IReadOnlyCollection<ContinentConnectorReservation> reservations)
        => FindCandidates(map, biome, kind, edge, landTerrains, occupied, reservations).Count != 0;

    private static List<ContinentConnectorReservation> FindCandidates(OverworldMap map, Biome biome, World.ConnectorKind kind, Direction edge,
        IReadOnlyCollection<Terrain> landTerrains, IEnumerable<IntVector2> occupied,
        IReadOnlyCollection<ContinentConnectorReservation> reservations)
    {
        bool openMountainBorder = biome is Biome.MOUNTAINOUS or Biome.CALDERA;
        switch (kind)
        {
            case World.ConnectorKind.Raft:
            case World.ConnectorKind.Bridge:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
        var inward = -edge.ToIntVector2();
        var side = inward.Perpendicular();
        int rows = map.GetLength(0), columns = map.GetLength(1);
        int span = edge.IsHorizontal() ? rows : columns;
        var occupiedPositions = occupied.ToHashSet();
        List<ContinentConnectorReservation> candidates = [];
        List<ContinentConnectorReservation> anchoredCandidates = [];
        bool Inside(IntVector2 p) => p.X >= 0 && p.Y >= 0 && p.X < columns && p.Y < rows;
        bool Free(IntVector2 p) => !occupiedPositions.Contains(p)
            && !IntVector2.CARDINALS.Any(d => occupiedPositions.Contains(p + d))
            && !reservations.Any(r => r.Contains(p));
        bool Land(IntVector2 p) => map[p] == Terrain.NONE || landTerrains.Contains(map[p]) && IsApproachTerrain(map[p]);
        bool CanOpenCoast(IntVector2 position) => map[position].IsWater()
            || Land(position) && map[position] is not (Terrain.ROAD or Terrain.LAVA);

        // Keep the apron away from corners, including the far edge on narrow maps.
        for (int offset = 2; offset < span - 2; offset++)
        {
            IntVector2 start = edge switch
            {
                Direction.WEST => new(0, offset),
                Direction.EAST => new(columns - 1, offset),
                Direction.NORTH => new(offset, 0),
                Direction.SOUTH => new(offset, rows - 1),
                _ => throw new ArgumentOutOfRangeException(nameof(edge))
            };
            List<IntVector2> route = [];
            var shore = start;
            // Mountainous/caldera maps historically let DrawOcean open their
            // perimeter. Preserve that exception for the boundary tile only.
            if (openMountainBorder && map[shore] == Terrain.MOUNTAIN)
            {
                route.Add(shore);
                shore += inward;
            }
            while (Inside(shore) && map[shore].IsWater())
            {
                route.Add(shore);
                shore += inward;
            }
            // Seed a short inlet where growth has not yet chosen the shoreline.
            while (Inside(shore) && route.Count < 2 && map[shore] == Terrain.NONE)
            {
                route.Add(shore);
                shore += inward;
            }
            if (route.Count < 2 || kind == World.ConnectorKind.Bridge && route.Count > 10) continue;
            var apron = (from depth in Enumerable.Range(0, 2)
                         from lateral in Enumerable.Range(-1, 3)
                         select shore + depth * inward + lateral * side).ToArray();
            if (!route.All(Free) || !apron.All(p => Inside(p) && Free(p) && Land(p))) continue;
            var landing = route[^1];
            IntVector2[] waterBesideConnector;
            if (kind == World.ConnectorKind.Raft)
            {
                // Open the coast beside the dock where there is room.
                IntVector2[] flanks = [landing - side, landing + side];
                waterBesideConnector = flanks.Where(p => Free(p) && CanOpenCoast(p)).ToArray();
            }
            else
            {
                IntVector2[]? bestStrip = null;
                int fewestTilesToPaint = int.MaxValue;
                foreach (var lateral in new[] { -side, side })
                {
                    var strip = route.Select(p => p + lateral).ToArray();
                    if (!strip.All(Free)) { continue; }
                    // The first tile is on the perimeter and can use the same mountain exception as the route.
                    if (!CanOpenCoast(strip[0]) && !(openMountainBorder && map[strip[0]] == Terrain.MOUNTAIN)) { continue; }
                    if (!strip.Skip(1).All(CanOpenCoast)) { continue; }

                    int tilesToPaint = strip.Count(p => !map[p].IsWater());
                    if (tilesToPaint >= fewestTilesToPaint) { continue; }
                    bestStrip = strip;
                    fewestTilesToPaint = tilesToPaint;
                }
                if (bestStrip is null) { continue; }
                waterBesideConnector = bestStrip;
            }
            var candidate = new ContinentConnectorReservation(kind, edge, landing, route.ToArray(), waterBesideConnector, apron);
            candidates.Add(candidate);
            if (candidate.ExistingApproachTerrain(map) is not null) anchoredCandidates.Add(candidate);
        }
        // Prefer a shore already touching a land seed instead of creating an
        // isolated patch whose connection depends entirely on growth.
        return anchoredCandidates.Count > 0 ? anchoredCandidates : candidates;
    }

    internal Terrain? ExistingApproachTerrain(OverworldMap map)
        => approach.Select(p => (Terrain?)map[p]).FirstOrDefault(t => t != Terrain.NONE);

    internal void Seed(OverworldMap map, Terrain water, Terrain land)
    {
        if (!water.IsWater()) throw new ArgumentException("A connector needs water.", nameof(water));
        if (!IsApproachTerrain(land)) throw new ArgumentException("A connector needs ungated land.", nameof(land));
        foreach (var pos in route.Concat(waterBesideConnector)) if (!map[pos].IsWater()) map[pos] = water;
        foreach (var pos in approach) if (map[pos] == Terrain.NONE) map[pos] = land;
    }

    internal bool IsIntact(OverworldMap map, bool finalized)
    {
        if (!approach.All(p => IsApproachTerrain(map[p]))) return false;
        foreach (var pos in route)
        {
            bool bridgeTile = finalized && (Kind == World.ConnectorKind.Bridge || pos == Landing);
            if (bridgeTile ? map[pos] != Terrain.BRIDGE : !map[pos].IsWater()) return false;
        }
        return waterBesideConnector.All(p => map[p].IsWater());
    }

    internal void Finalize(OverworldMap map, Location location)
    {
        if (Kind == World.ConnectorKind.Raft) World.PlaceRaft(map, location, Landing.X, Landing.Y);
        else World.PlaceBridge(map, location, Landing.X, Landing.Y, Edge);
    }

    private static bool IsApproachTerrain(Terrain terrain)
        => terrain is Terrain.DESERT or Terrain.GRASS or Terrain.FOREST or Terrain.SWAMP or Terrain.GRAVE or Terrain.ROAD or Terrain.LAVA;
}
