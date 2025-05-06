using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.RandomizerCore;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Text : IEquatable<Text>
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public string RawText { get; private set; }
    public List<char> EncodedText { get; private set; }

    public Text()
    {
        RawText = "I know$nothing";
        EncodedText = Util.ToGameText("I know$nothing", true);
    }

    public Text(List<char> text)
    {
        RawText = Util.FromGameText(text);
        EncodedText = text;
    }
    public Text(string text)
    {
        RawText = text;
        EncodedText = Util.ToGameText(text, true);
    }

    public Text(string text, Collectable collectable)
    {
        RawText = text;
        if (RawText.Contains("%%"))
        {
            RawText = RawText.Replace("%%", collectable.EnglishText());
        }
        else if (RawText.Contains('%'))
        {
            RawText = RawText.Replace("%", collectable.SingleLineText());
        }
        else
        {
            logger.Warn("Invalid collectable in hint generation");
            RawText = "THIS TEXT$IS BROKEN$TELL$ELLENDAR";
        }
        EncodedText = Util.ToGameText(RawText, true);
    }

    public static Text GenerateHelpfulHint(Location location)
    {
        string hint = "";
        if (location.PalaceNumber == 1)
        {
            hint += "horsehead$neighs$with the$%";
        }
        else if (location.PalaceNumber == 2)
        {
            hint += "helmethead$guards the$%%";
        }
        else if (location.PalaceNumber == 3)
        {
            hint += "rebonack$rides$with the$%";
        }
        else if (location.PalaceNumber == 4)
        {
            hint += "carock$disappears$with the$%";
        }
        else if (location.PalaceNumber == 5)
        {
            hint += "gooma sits$on the$%%";
        }
        else if (location.PalaceNumber == 6)
        {
            hint += "barba$slithers$with the$%";
        }
        else if (location.Continent == Continent.EAST)
        {
            hint += "go east to$find the$%%";
        }
        else if (location.Continent == Continent.WEST)
        {
            hint += "go west to$find the$%%";
        }
        else if (location.Continent == Continent.DM)
        {
            hint += "death$mountain$holds the$%";
        }
        else
        {
            hint += "in a maze$lies the$%%";
        }

        return new Text(hint, location.Collectable);
    }

    public string GetDebuggerDisplay()
    {
        return RawText;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Text);
    }

    public bool Equals(Text? other)
    {
        return other is not null &&
               EqualityComparer<List<char>>.Default.Equals(EncodedText, other.EncodedText);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EncodedText);
    }
}
