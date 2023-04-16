using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer;

/// <summary>
/// Collection of singletons representing character sprite options. For now this is just a fake enum.
/// Eventually the plan is to refactor the sprite data out of Graphics and use this to be a proper representation
/// This will also allow us to construct sprites in ways other than having them hardcoded.
/// Ideally we create some standard file format so users can simply upload their sprite to the rando.
/// </summary>
public class CharacterSprite
{
    public int SelectionIndex { get; private set; }
    public string DisplayName { get; private set; }
    public string Path { get; private set; }
    public bool IsLegacy { get; private set; }
    public CharacterSprite(int selectionIndex, string displayName, bool isLegacy, string path)
    {
        SelectionIndex = selectionIndex;
        DisplayName = displayName;
        IsLegacy = isLegacy;
        Path = path;
    }

    public CharacterSprite(int selectionIndex, string displayName, bool isLegacy) : this(selectionIndex, displayName, isLegacy, null)
    {

    }

    public static readonly CharacterSprite LINK = new CharacterSprite(0, "Link", true);
    public static readonly CharacterSprite ZELDA = new CharacterSprite(1, "Zelda", true);
    public static readonly CharacterSprite IRON_KNUCKLE = new CharacterSprite(2, "Iron Knuckle", true);
    public static readonly CharacterSprite ERROR = new CharacterSprite(3, "Error", true);
    public static readonly CharacterSprite SAMUS = new CharacterSprite(4, "Samus", true);
    public static readonly CharacterSprite SIMON = new CharacterSprite(5, "Simon", true);
    public static readonly CharacterSprite STALFOS = new CharacterSprite(6, "Stalfos", true);
    public static readonly CharacterSprite VASE_LADY = new CharacterSprite(7, "Vase Lady", true);
    public static readonly CharacterSprite RUTO = new CharacterSprite(8, "Ruto", true);
    public static readonly CharacterSprite YOSHI = new CharacterSprite(9, "Yoshi", true);
    public static readonly CharacterSprite DRAGONLORD = new CharacterSprite(10, "Dragonlord", true);
    public static readonly CharacterSprite MIRIA = new CharacterSprite(11, "Miria", true);
    public static readonly CharacterSprite CRYSTALIS = new CharacterSprite(12, "Crystalis", true);
    public static readonly CharacterSprite TACO = new CharacterSprite(13, "Taco", true);
    public static readonly CharacterSprite PYRAMID = new CharacterSprite(14, "Pyramid", true);
    public static readonly CharacterSprite LADY_LINK = new CharacterSprite(15, "Lady Link", true);
    public static readonly CharacterSprite HOODIE_LINK = new CharacterSprite(16, "Hoodie Link", true);
    public static readonly CharacterSprite GLITCH_WITCH = new CharacterSprite(17, "GliitchWitch", true);

    public static List<CharacterSprite> Options()
    {
        List<CharacterSprite> options = new List<CharacterSprite>()
        {
            LINK,
            ZELDA,
            IRON_KNUCKLE,
            ERROR,
            SAMUS,
            SIMON,
            STALFOS,
            VASE_LADY,
            RUTO,
            YOSHI,
            DRAGONLORD,
            MIRIA,
            CRYSTALIS,
            TACO,
            PYRAMID,
            LADY_LINK,
            HOODIE_LINK,
            GLITCH_WITCH
        };

        if (Directory.Exists("Sprites"))
        {
            foreach (string spritePath in Directory.GetFiles("Sprites", "*.ips"))
            {
                string parsedName = System.IO.Path.GetFileNameWithoutExtension(spritePath).Replace("_", " ");
                options.Add(new CharacterSprite(options.Count, parsedName, false, spritePath));
            }
        }

        options.Add(new CharacterSprite(options.Count, "Random", false));

        return options;
    }

    public static CharacterSprite ByIndex(int index)
    {
        List<CharacterSprite> options = Options();
        if(index == options.Count - 1)
        {
            return Random();
        }
        return options[index];
    }

    public static CharacterSprite Random()
    {
        Random random = new Random();
        FieldInfo[] spriteOptions = typeof(CharacterSprite).GetFields().Where(i => i.FieldType == typeof(CharacterSprite) && i.IsStatic).ToArray();
        FieldInfo sprite = spriteOptions[random.Next(spriteOptions.Length - 1)];
        return (CharacterSprite)sprite.GetValue(null);
    }
};
