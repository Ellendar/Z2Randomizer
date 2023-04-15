using System;
using System.Collections.Generic;
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
    public CharacterSprite(int selectionIndex)
    {
        SelectionIndex= selectionIndex;
    }

    public static readonly CharacterSprite LINK = new CharacterSprite(0);
    public static readonly CharacterSprite ZELDA = new CharacterSprite(1);
    public static readonly CharacterSprite IRON_KNUCKLE = new CharacterSprite(2);
    public static readonly CharacterSprite ERROR = new CharacterSprite(3);
    public static readonly CharacterSprite SAMUS = new CharacterSprite(4);
    public static readonly CharacterSprite SIMON = new CharacterSprite(5);
    public static readonly CharacterSprite STALFOS = new CharacterSprite(6);
    public static readonly CharacterSprite VASE_LADY = new CharacterSprite(7);
    public static readonly CharacterSprite RUTO = new CharacterSprite(8);
    public static readonly CharacterSprite YOSHI = new CharacterSprite(9);
    public static readonly CharacterSprite DRAGONLORD = new CharacterSprite(10);
    public static readonly CharacterSprite MIRIA = new CharacterSprite(11);
    public static readonly CharacterSprite CRYSTALIS = new CharacterSprite(12);
    public static readonly CharacterSprite TACO = new CharacterSprite(13);
    public static readonly CharacterSprite PYRAMID = new CharacterSprite(14);
    public static readonly CharacterSprite LADY_LINK = new CharacterSprite(15);
    public static readonly CharacterSprite HOODIE_LINK = new CharacterSprite(16);
    public static readonly CharacterSprite GLITCH_WITCH = new CharacterSprite(17);
    public static readonly CharacterSprite RANDOM = new CharacterSprite(18);

    public static CharacterSprite Random()
    {
        Random random = new Random();
        FieldInfo[] spriteOptions = typeof(CharacterSprite).GetFields().Where(i => i.FieldType == typeof(CharacterSprite) && i.IsStatic).ToArray();
        FieldInfo sprite = spriteOptions[random.Next(spriteOptions.Length - 1)];
        return (CharacterSprite)sprite.GetValue(null);
    }
};
