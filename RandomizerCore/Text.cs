using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using Z2Randomizer.RandomizerCore.Overworld;

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

    public static Text GenerateHelpfulHint(List<Location> allLocations, Location location, Collectable collectable, bool useTownSpecificHints)
    {
        string? hint = null;
        if (location.PalaceNumber == 1)
        {
            hint = "horsehead$neighs$with the$%%";
        }
        else if (location.PalaceNumber == 2)
        {
            hint = "helmethead$guards the$%%";
        }
        else if (location.PalaceNumber == 3)
        {
            hint = "rebonack$rides$with the$%%";
        }
        else if (location.PalaceNumber == 4)
        {
            hint = "carock$disappears$with the$%%";
        }
        else if (location.PalaceNumber == 5)
        {
            hint = "gooma sits$on the$%%";
        }
        else if (location.PalaceNumber == 6)
        {
            hint = "barba$slithers$with the$%%";
        }
        else if (useTownSpecificHints)
        {
            //For now bagu is not a town for hints. Could change in the future.
            if (location.ActualTown != null && location.ActualTown != Town.BAGU)
            {
                hint = $"{((Town)location.ActualTown).HintName()}$has the$%%";
            }
            //Saria table
            if(allLocations.First(i => i.ActualTown == Town.SARIA_NORTH).Children.Contains(location))
            {
                hint = $"{Town.SARIA_NORTH.HintName()}$has the$%%";
            }
            //Nabooru fountain
            if (allLocations.First(i => i.ActualTown == Town.NABOORU).Children.Contains(location))
            {
                hint = $"{Town.NABOORU.HintName()}$has the$%%";
            }
            //Spell Tower / Granny's Basement
            if (allLocations.First(i => i.ActualTown == Town.NEW_KASUTO).Children.Contains(location))
            {
                hint = $"{Town.NEW_KASUTO.HintName()}$has the$%%";
            }
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
}
