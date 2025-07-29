using System;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// Collection of singletons representing character sprite options.
/// Sprites are now all IPS patches, which are loaded in from the /Sprites folder when installed
/// so users can load their own sprites and they will automatically populate the menu
/// </summary>
public class CharacterSprite
{
    // public int SelectionIndex { get; private set; }
    public string? DisplayName { get; private set; }
    public byte[]? Patch { get; private set; }
    // public bool IsLegacy { get; private set; }
    public CharacterSprite(string? displayName, byte[]? patch = null)
    {
        // SelectionIndex = selectionIndex;
        DisplayName = displayName;
        // IsLegacy = isLegacy;
        Patch = patch;
    }

    public static readonly CharacterSprite LINK = new ("Link");
    public static readonly CharacterSprite RANDOM = new ("Random");

    // public static CharacterSprite[] Options()
    // {
    //     List<CharacterSprite> options = new List<CharacterSprite>()
    //     {
    //         LINK,
    //     };
    //
    //     if (Directory.Exists("Sprites"))
    //     {
    //         foreach (string spritePath in Directory.GetFiles("Sprites", "*.ips"))
    //         {
    //             string parsedName = System.IO.Path.GetFileNameWithoutExtension(spritePath).Replace("_", " ");
    //             options.Add(new CharacterSprite(options.Count, parsedName, false, spritePath));
    //         }
    //     }
    //
    //     options.Add(new CharacterSprite(options.Count, "Random", false));
    //
    //     return options.ToArray();
    // }
    // public static CharacterSprite ByIndex(int index)
    // {
    //     CharacterSprite[] options = Options();
    //     if (index == options.Length - 1)
    //     {
    //         return Random(options);
    //     }
    //     return options[index];
    // }

    public static CharacterSprite Random(CharacterSprite[] options)
    {
        Random random = new Random();
        return options[random.Next(options.Length - 1)];
    }
};
