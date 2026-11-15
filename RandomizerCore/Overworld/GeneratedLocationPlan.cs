using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>
/// Stable generation-entry data for active locations, captured after constructor removals
/// and connector loading. CanShuffle is deliberately not an input: it may describe a
/// previous placement. Each terrain attempt gets fresh eligibility and progress.
/// This resets working inputs; the enclosing terrain candidate owns commit/discard.
/// </summary>
internal sealed class GeneratedLocationPlan
{
    private sealed record Entry(Location Location, Terrain Terrain, bool Passthrough,
        Requirements Requirements, bool Hidden, bool ExternalWorld, int EntranceNumber);

    private readonly Entry[] entries;
    private readonly Location[] minorLocations;

    internal GeneratedLocationPlan(IEnumerable<Location> locations, IEnumerable<Location> minorLocations)
    {
        entries = locations.Select(l => new Entry(l, l.TerrainType, l.IsPassthrough,
            l.AccessRequirements, l.IsHidden, l.IsExternalWorld, l.EntranceNumber)).ToArray();
        this.minorLocations = minorLocations.ToArray();
    }

    internal List<Location> CreateMinorLocations() => [.. minorLocations];

    internal IDisposable BeginAttempt()
    {
        foreach (var entry in entries)
        {
            var location = entry.Location;
            location.TerrainType = entry.Terrain;
            location.IsPassthrough = entry.Passthrough;
            location.AccessRequirements = entry.Requirements;
            location.IsHidden = entry.Hidden;
            location.IsExternalWorld = entry.ExternalWorld;
            location.EntranceNumber = entry.EntranceNumber;
            location.ResetCoords(false);
            location.PlacementState = new LocationPlacementState();
        }
        return new Attempt(entries);
    }

    private sealed class Attempt(Entry[] entries) : IDisposable
    {
        public void Dispose()
        {
            // Preserve the final public flags while detaching the temporary adapter.
            // Gameplay visitation and vanilla shuffling continue using the legacy flag.
            foreach (var entry in entries) entry.Location.EndPlacementAttempt();
        }
    }
}
