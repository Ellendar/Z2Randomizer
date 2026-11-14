using System;

namespace Z2Randomizer.RandomizerCore.Overworld;

internal enum RegionCrossingTermination
{
    CrossingLimitReached,
    // Pool empty on completion, including the legacy default-tuple fallback iterations.
    CandidatePoolExhausted,
    AttemptBudgetExhausted,
    LongBridgeEntranceBlocked,
    DaruniaEntranceBlocked
}

[Flags]
internal enum CrossingFeatures
{
    None = 0,
    Saria = 1,
    LongBridge = 2,
    DaruniaEncounters = 4
}

/// <summary>Reports why crossing placement stopped and what it placed.</summary>
/// <remarks>
/// An unavailable Darunia pair can consume a slot without increasing
/// <see cref="PlacedCrossings"/>. Callers check the features they require and
/// validate overall connectivity separately.
/// </remarks>
internal readonly record struct RegionCrossingResult(
    RegionCrossingTermination Termination,
    int PlacedCrossings,
    CrossingFeatures RequestedFeatures,
    CrossingFeatures PlacedFeatures,
    CrossingFeatures UnavailableFeatures)
{
    /// <summary>Requested features neither placed nor skipped as unavailable.</summary>
    internal CrossingFeatures PendingFeatures => RequestedFeatures & ~(PlacedFeatures | UnavailableFeatures);
}
