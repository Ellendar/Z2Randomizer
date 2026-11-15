namespace Z2Randomizer.RandomizerCore.Overworld;

/// <summary>
/// Location values terrain generation can edit. Candidate copies share immutable
/// requirement values; town, palace, collectables and location identity stay on Location.
/// </summary>
internal sealed record LocationTerrainState
{
    internal IntVector2 Position;
    internal Terrain Terrain;
    internal Requirements Requirements = Requirements.NONE;
    internal bool CanShuffle;
    internal bool Reachable;
    internal bool Hidden;
    internal bool Passthrough;
    internal bool ExternalWorld;
    internal int EntranceNumber;
}
