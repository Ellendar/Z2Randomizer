using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

/// <summary>
/// Uses all the default values, which should always be vanilla.
/// </summary>
public static class VanillaPreset
{
    public static string Name => "Vanilla";

    public static string Description => "Sets everything as close to the base game of Zelda 2 as possible.";

    public static readonly RandomizerConfiguration Preset = new();
}
