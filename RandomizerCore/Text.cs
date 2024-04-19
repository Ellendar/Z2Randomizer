using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Z2Randomizer.Core.Overworld;

namespace Z2Randomizer.Core;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Text : IEquatable<Text>
{
    public string RawText { get; private set; }
    public List<char> TextChars { get; private set; }

    public Text()
    {
        TextChars = Util.ToGameText("I know$nothing", true);
    }

    public Text(List<char> text)
    {
        RawText = Util.FromGameText(text);
        TextChars = text;
    }
    public Text(string text)
    {
        RawText = text;
        TextChars = Util.ToGameText(text, true);
    }

    public void GenerateHelpfulHint(Location location)
    {
        Item hintItem = location.Collectable;
        string hint = "";
        if (location.PalaceNumber == 1)
        {
            hint += "horsehead$neighs$with the$";
        }
        else if (location.PalaceNumber == 2)
        {
            hint += "helmethead$guards the$";
        }
        else if (location.PalaceNumber == 3)
        {
            hint += "rebonack$rides$with the$";
        }
        else if (location.PalaceNumber == 4)
        {
            hint += "carock$disappears$with the$";
        }
        else if (location.PalaceNumber == 5)
        {
            hint += "gooma sits$on the$";
        }
        else if (location.PalaceNumber == 6)
        {
            hint += "barba$slithers$with the$";
        }
        else if (location.Continent == Continent.EAST)
        {
            hint += "go east to$find the$";
        }
        else if (location.Continent == Continent.WEST)
        {
            hint += "go west to$find the$";
        }
        else if (location.Continent == Continent.DM)
        {
            hint += "death$mountain$holds the$";
        }
        else
        {
            hint += "in a maze$lies the$";
        }

        switch (hintItem)
        {
            case (Item.BLUE_JAR):
                hint += "blue jar";
                break;
            case (Item.BOOTS):
                hint += "boots";
                break;
            case (Item.CANDLE):
                hint += "candle";
                break;
            case (Item.CROSS):
                hint += "cross";
                break;
            case (Item.XL_BAG):
            case (Item.LARGE_BAG):
            case (Item.MEDIUM_BAG):
            case (Item.SMALL_BAG):
                hint += "pbag";
                break;
            case (Item.GLOVE):
                hint += "glove";
                break;
            case (Item.HAMMER):
                hint += "hammer";
                break;
            case (Item.HEART_CONTAINER):
                hint += "heart";
                break;
            case (Item.FLUTE):
                hint += "flute";
                break;
            case (Item.KEY):
                hint += "small key";
                break;
            case (Item.CHILD):
                hint += "child";
                break;
            case (Item.MAGIC_CONTAINER):
                hint += "magic jar";
                break;
            case (Item.MAGIC_KEY):
                hint += "magic key";
                break;
            case (Item.MEDICINE):
                hint += "medicine";
                break;
            case (Item.ONEUP):
                hint += "link doll";
                break;
            case (Item.RAFT):
                hint += "raft";
                break;
            case (Item.RED_JAR):
                hint += "red jar";
                break;
            case (Item.TROPHY):
                hint += "trophy";
                break;
        }

        RawText = hint;
        TextChars = Util.ToGameText(hint, true);
    }

    public string GetDebuggerDisplay()
    {
        return RawText;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Text);
    }

    public bool Equals(Text other)
    {
        return other is not null &&
               EqualityComparer<List<char>>.Default.Equals(TextChars, other.TextChars);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TextChars);
    }
}
