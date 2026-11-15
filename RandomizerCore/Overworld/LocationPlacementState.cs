namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>Availability for ordinary placement, separate from a location's progress in one terrain attempt.</summary>
internal sealed class LocationPlacementState
{
    internal bool Eligible { get; private set; } = true;
    internal PlacementProgress Progress { get; private set; }
    internal bool CanPlace => Eligible && Progress == PlacementProgress.Unplaced;

    internal void Reserve() => Eligible = false;
    internal void Disable() => Progress = PlacementProgress.Disabled;

    // Adapter for existing placement helpers. Reopening progress cannot undo a reservation
    // or a deliberate removal; a new attempt receives an entirely new state instead.
    internal void SetAvailable(bool available)
    {
        if (Progress != PlacementProgress.Disabled)
            Progress = available ? PlacementProgress.Unplaced : PlacementProgress.Placed;
    }
}

internal enum PlacementProgress { Unplaced, Placed, Disabled }
