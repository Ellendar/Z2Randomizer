using RandomizerCore;
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
        Collectable hintItem = location.Collectable;
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

        hint += hintItem.SingleLineText();

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
