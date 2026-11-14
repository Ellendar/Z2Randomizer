using System;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

public abstract partial class World
{
    private TerrainCandidate? terrainCandidate;
    private protected TerrainRomPatch TerrainWrites => terrainCandidate?.Writes
        ?? throw new InvalidOperationException("Terrain writes require an active candidate.");

    internal TerrainCandidate BeginTerrainCandidate(ROM destination) => new(this, destination);

    /// <summary>Captures continent-specific fields before a terrain candidate begins.</summary>
    /// <returns>An action that restores the captured values. The default action does nothing.</returns>
    /// <remarks>
    /// Override for fields that terrain generation can change, such as the caldera center
    /// or hidden-location references. <see cref="TerrainCandidate"/> handles shared World
    /// state and location drafts. The action runs when the candidate is disposed without
    /// a successful commit. Map-size retries share the same candidate and snapshot.
    /// </remarks>
    private protected virtual Action CaptureTerrainScratchState() => () => { };

    /// <summary>
    /// Adapts existing serial helpers to a candidate map, location drafts and queued
    /// ROM edits. Only Commit publishes location values/ROM bytes. Rejection restores
    /// World working references; RNG and diagnostic counters are intentionally excluded.
    /// </summary>
    internal sealed class TerrainCandidate : IDisposable
    {
        private readonly World world;
        private readonly ROM destination;
        private readonly Location[] locations;
        private readonly Action restoreWorld;
        private bool accepted;
        private bool disposed;
        internal TerrainRomPatch Writes { get; } = new();

        internal TerrainCandidate(World world, ROM destination)
        {
            if (world.terrainCandidate is not null) throw new InvalidOperationException("World already has a terrain candidate.");
            this.world = world;
            this.destination = destination;
            locations = world.AllLocations.Concat(world.RemovedLocations).ToArray();
            var previousMap = world.map;
            var previousVisitation = world.visitation;
            var previousPlacementFilter = world.walkableTerrains;
            var previousSeedFilter = world.randomTerrainFilter;
            var previousMinorLocations = world.unimportantLocs;
            var previousReservations = world.continentConnectorReservations;
            var previousConnectorsFinalized = world.continentConnectorsFinalized;
            var restoreScratch = world.CaptureTerrainScratchState();
            restoreWorld = () =>
            {
                world.map = previousMap;
                world.visitation = previousVisitation;
                world.walkableTerrains = previousPlacementFilter;
                world.randomTerrainFilter = previousSeedFilter;
                world.unimportantLocs = previousMinorLocations;
                world.continentConnectorReservations = previousReservations;
                world.continentConnectorsFinalized = previousConnectorsFinalized;
                restoreScratch();
            };
            foreach (var location in locations) location.BeginTerrainCandidate();
            // Copies only terrain/visitation data. Location/Town/Palace graphs and the
            // ROM remain the original objects. New generation replaces this map copy.
            world.map = previousMap?.Clone()!;
            world.visitation = (bool[,]?)previousVisitation?.Clone()!;
            world.walkableTerrains = previousPlacementFilter?.ToList()!;
            world.randomTerrainFilter = previousSeedFilter?.ToList()!;
            world.unimportantLocs = previousMinorLocations.ToList();
            world.continentConnectorReservations = [];
            world.continentConnectorsFinalized = false;
            world.terrainCandidate = this;
        }

        internal void RestartGeneratedAttempt()
        {
            foreach (var location in locations) location.RestartTerrainCandidate();
            Writes.Clear();
        }

        internal void Commit()
        {
            if (disposed || accepted) throw new InvalidOperationException("Terrain candidate is no longer pending.");
            if (locations.Any(location => location.PlacementState is not null))
                throw new InvalidOperationException("Finish placement before committing the terrain candidate.");
            // Apply validates every destination first. Location publication afterward
            // consists only of swapping their draft state references.
            Writes.Apply(destination);
            foreach (var location in locations) location.EndTerrainCandidate(true);
            accepted = true;
        }

        public void Dispose()
        {
            if (disposed) return;
            if (!accepted)
            {
                foreach (var location in locations) location.EndTerrainCandidate(false);
                restoreWorld();
            }
            world.terrainCandidate = null;
            disposed = true;
        }
    }
}
