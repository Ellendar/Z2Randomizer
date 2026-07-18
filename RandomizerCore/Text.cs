using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll.Town;

namespace Z2Randomizer.RandomizerCore;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Text : IEquatable<Text>
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    protected static readonly IEqualityComparer<byte[]> byteArrayEqualityComparer = new Util.StandardByteArrayEqualityComparer();

    public string RawText { get; private set; }
    public byte[] EncodedText { get; private set; }

    public Text()
    {
        RawText = "I know$nothing";
        EncodedText = Util.ToGameText("I know$nothing", true);
    }

    public Text(byte[] bytes)
    {
        RawText = Util.FromGameText(bytes);
        EncodedText = bytes;
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

    private Text(string rawText, byte[] encodedText)
    {
        RawText = rawText;
        EncodedText = encodedText;
    }

    public bool ValidateDialogText()
    {
        var lines = RawText.Split("$");
        if (lines.Count() > 6)
        {
            logger.Error($"Dialog text \"{RawText}\" has too many lines");
            return false;
        }
        foreach (var line in lines)
        {
            if (line.Count() > 11)
            {
                logger.Error($"Dialog line \"{line}\" is too long.");
                return false;
            }
        }
        return true;
    }

    public static Text GenerateHelpfulHint(Location location, Collectable collectable, bool useTownSpecificHints)
    {
        string? hint = null;
        if (location.Palace?.Number == 1)
        {
            hint = "horsehead$neighs$with the$%%";
        }
        else if (location.Palace?.Number == 2)
        {
            hint = "helmethead$guards the$%%";
        }
        else if (location.Palace?.Number == 3)
        {
            hint = "rebonack$rides$with the$%%";
        }
        else if (location.Palace?.Number == 4)
        {
            hint = "carock$disappears$with the$%%";
        }
        else if (location.Palace?.Number == 5)
        {
            hint = "gooma sits$on the$%%";
        }
        else if (location.Palace?.Number == 6)
        {
            hint = "barba$slithers$with the$%%";
        }
        else if (useTownSpecificHints && location.Town != null)
        {
            hint = $"{((TownType)location.Town.Type!).HintName()}$has the$%%";
        }
        if(hint == null)
        {
            if (location.Continent == Continent.EAST)
            {
                hint = "go east to$find the$%%";
            }
            else if (location.Continent == Continent.WEST)
            {
                hint = "go west to$find the$%%";
            }
            else if (location.Continent == Continent.DM)
            {
                hint = "death$mountain$holds the$%%";
            }
            else if(location.Continent == Continent.MAZE)
            {
                hint = "in a maze$lies the$%%";
            }
            else
            {
                throw new ImpossibleException("Unknown continent generating hints");
            }
        }

        hint ??= "ERROR$GENERATING$HINT";
        return new Text(hint, collectable);
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
        return other is not null && byteArrayEqualityComparer.Equals(EncodedText, other.EncodedText);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EncodedText);
    }

    public Text Clone()
    {
        return new(RawText, EncodedText);
    }
}
