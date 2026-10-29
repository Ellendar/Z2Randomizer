namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>
/// Requests within-continent passages across a terrain band, such as water or mountains.
/// A crossing is one passage between its two banks: it may be a bridge, road,
/// walkable water, Saria's linked town entrances or a path with paired encounters.
/// </summary>
/// <remarks>
/// These passages stay on the same continent; raft, bridge and cave continent connectors
/// are placed separately.
/// </remarks>
internal sealed record RegionCrossingOptions
{
    /// <summary>
    /// Maximum passage slots this single placement call may consume. Existing map
    /// crossings do not count toward the limit; each later call starts its own quota.
    /// </summary>
    /// <remarks>
    /// One whole passage uses one slot, regardless of its tile count or paired entrances.
    /// Simple crossings and requested Saria/long-bridge/Darunia features share the quota.
    /// The call can place fewer passages: search can exhaust.
    /// The Darunia trap tile pair consumes a slot even if it fails to place.
    /// </remarks>
    public required int MaximumCrossings { get; init; }

    /// <summary>The terrain band to cross; the passage's painted terrain is chosen separately.</summary>
    public required Terrain CrossingTerrain { get; init; }
    public required Biome Biome { get; init; }
    public required bool CanWalkOnWaterWithBoots { get; init; }
    public CrossingFeatures Features { get; init; }
    public bool RiverDevilBlocker { get; init; }
    public bool RockBlocker { get; init; }
    public RegionCrossingDeadZone? DeadZone { get; init; }
}

/// <summary>
/// Inclusive coordinate bands excluded from starting-shore selection. A shore must
/// be outside both bands: this excludes either axis band, not just their intersection.
/// </summary>
internal readonly record struct RegionCrossingDeadZone(int MinimumX, int MaximumX, int MinimumY, int MaximumY);
