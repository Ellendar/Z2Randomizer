namespace Z2Randomizer.RandomizerCore.Overworld;

internal enum HiddenLocationTrigger { Flute, Hammer }

/// <summary>
/// A Flute- or Hammer-hidden destination and its terrain before/after revealing.
/// Refers to the owning world's live Location. Emit it only with the ROM for that
/// world layout; regenerating the world also updates these location references.
/// </summary>
internal sealed record HiddenLocation
{
    internal Location Location { get; }
    internal HiddenLocationTrigger Trigger { get; init; }
    internal Terrain UnrevealedTerrain { get; init; }
    internal Terrain RevealedTerrain { get; init; }

    internal HiddenLocation(Location location, HiddenLocationTrigger trigger, Terrain unrevealedTerrain,
        Terrain revealedTerrain)
    {
        Location = location;
        Trigger = trigger;
        UnrevealedTerrain = unrevealedTerrain;
        RevealedTerrain = revealedTerrain;
    }

    internal static HiddenLocation FromLocation(Location location, HiddenLocationTrigger trigger,
        Terrain? revealedTerrain = null, Terrain? unrevealedTerrain = null)
        => new(location, trigger, unrevealedTerrain ?? location.TerrainType, revealedTerrain ?? location.TerrainType);

    internal int LocationIndex => Location.ID.GetIndex();
    internal byte AreaByte => (byte)(Location.YRaw + (Location.IsExternalWorld ? 0x80 : 0));
}
