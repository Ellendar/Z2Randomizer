using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace RandomizerCore;

[DefaultValue(NONE)]
public enum StartingTechs
{
    [Description("None")]
    NONE,
    [Description("Downstab")]
    DOWNSTAB,
    [Description("Upstab")]
    UPSTAB,
    [Description("Both")]
    BOTH,
    [Description("Random")]
    RANDOM
}

[DefaultValue(VANILLA)]
public enum AttackEffectiveness
{
    [Description("Random")]
    AVERAGE,
    [Description("Low Attack")]
    LOW,
    [Description("Vanilla")]
    VANILLA,
    [Description("High Attack")]
    HIGH,
    [Description("Instant Kill")]
    OHKO
}

[DefaultValue(VANILLA)]
public enum MagicEffectiveness
{
    [Description("Random")]
    AVERAGE,
    [Description("High Spell Cost")]
    HIGH_COST,
    [Description("Vanilla")]
    VANILLA,
    [Description("Low Spell Cost")]
    LOW_COST,
    [Description("Free Spells")]
    FREE
}

[DefaultValue(VANILLA)]
public enum LifeEffectiveness
{
    [Description("Random")]
    AVERAGE,
    [Description("OHKO Link")]
    OHKO,
    [Description("Vanilla")]
    VANILLA,
    [Description("High Defense")]
    HIGH,
    [Description("Invincible")]
    INVINCIBLE
}

//I removed no XP drops because literally nobody played it and it saved a bit in the flag string
//If anyone complains I can put it back but... nobody will.
[DefaultValue(VANILLA)]
public enum XPEffectiveness
{
    [Description("Vanilla")]
    VANILLA,
    [Description("Low")]
    LOW,
    [Description("Average")]
    AVERAGE,
    [Description("High")]
    HIGH
}

//The old unified stateffectiveness is still used on the rando side, but moved
//to separate effectivenesses for the config mapping
public enum StatEffectiveness
{
    NONE, 
    LOW, 
    VANILLA, 
    AVERAGE, 
    HIGH, 
    MAX
}


[DefaultValue(NORMAL)]
public enum FireOption
{
    [Description("Normal")]
    NORMAL,
    [Description("Pair With Random")]
    PAIR_WITH_RANDOM,
    [Description("Replace With Dash")]
    REPLACE_WITH_DASH,
    [Description("Random")]
    RANDOM
}

[DefaultValue(VANILLA)]
public enum NormalPalaceStyle
{
    [Description("Vanilla")]
    VANILLA,
    [Description("Shuffled")]
    SHUFFLED,
    [Description("Reconstructed")]
    RECONSTRUCTED,
    [Description("Cartesian")]
    CARTESIAN,
    [Description("Chaos")]
    CHAOS,
    [Description("Random All")]
    RANDOM_ALL,
    [Description("Random Per Palace")]
    RANDOM_PER_PALACE
}

[DefaultValue(VANILLA)]
public enum GPStyle
{
    [Description("Vanilla")]
    VANILLA,
    [Description("Shuffled")]
    SHUFFLED,
    [Description("Reconstructed")]
    RECONSTRUCTED,
    [Description("Cartesian")]
    CARTESIAN,
    [Description("Chaos")]
    CHAOS,
    [Description("Random")]
    RANDOM
}

//The old unified PalaceStyle is still used on the rando side, but moved
//to separate effectivenesses for the config mapping
[DefaultValue(VANILLA)]
public enum PalaceStyle
{
    VANILLA,
    SHUFFLED,
    RECONSTRUCTED,
    CARTESIAN,
    CHAOS
}

public static class PalaceStyleExtensions
{
    public static bool UsesVanillaRoomPool(this PalaceStyle style)
    {
        return style switch
        {
            PalaceStyle.VANILLA => true,
            PalaceStyle.SHUFFLED => true,
            _ => false
        };
    }

    public static PalaceStyle AsPalaceStyle(this GPStyle style)
    {
        return style switch
        {
            GPStyle.CHAOS => PalaceStyle.CHAOS,
            GPStyle.CARTESIAN => PalaceStyle.CARTESIAN,
            GPStyle.RECONSTRUCTED => PalaceStyle.RECONSTRUCTED,
            GPStyle.VANILLA => PalaceStyle.VANILLA,
            GPStyle.SHUFFLED => PalaceStyle.SHUFFLED,
            //Random intentionally not here, GPPalaceStyle should only be used on the config side,
            //PalaceStyle on the rando side.
            _ => throw new Exception("Unrecognized GPPalaceStyle conversion")
        };
    }

    public static PalaceStyle AsPalaceStyle(this NormalPalaceStyle style)
    {
        return style switch
        {
            NormalPalaceStyle.CHAOS => PalaceStyle.CHAOS,
            NormalPalaceStyle.CARTESIAN => PalaceStyle.CARTESIAN,
            NormalPalaceStyle.RECONSTRUCTED => PalaceStyle.RECONSTRUCTED,
            NormalPalaceStyle.VANILLA => PalaceStyle.VANILLA,
            NormalPalaceStyle.SHUFFLED => PalaceStyle.SHUFFLED,
            _ => throw new Exception("Unrecognized NormalPalaceStyle conversion")
        };
    }
}

[DefaultValue(VANILLA)]
public enum Biome
{
    [Description("Vanilla")]
    VANILLA,
    [Description("Vanilla Shuffle")]
    VANILLA_SHUFFLE,
    [Description("Vanilla Like")]
    VANILLALIKE,
    [Description("Islands")]
    ISLANDS,
    [Description("Canyon")]
    CANYON,
    [Description("Dry Canyon")]
    DRY_CANYON,
    [Description("Mountainous")]
    MOUNTAINOUS,
    [Description("Volcano")]
    VOLCANO,
    [Description("Caldera")]
    CALDERA,
    [Description("Random (No Vanilla or Shuffle)")]
    RANDOM_NO_VANILLA_OR_SHUFFLE,
    [Description("Random (No Vanilla)")]
    RANDOM_NO_VANILLA,
    [Description("Random")]
    RANDOM
}

static class BiomeExtensions
{
    public static int SeedTerrainLimit(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => 300,
            Biome.VANILLA_SHUFFLE => 300,
            Biome.VANILLALIKE => 300,
            Biome.ISLANDS => 220,
            Biome.CANYON => 300,
            Biome.DRY_CANYON => 300,
            Biome.MOUNTAINOUS => 250,
            Biome.CALDERA => 300,
            Biome.VOLCANO => 300,
            _ => throw new Exception("Unrecognized Biome in SeedTerrainLimit")
        };
    }

    public static bool IsWestBiome(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => true,
            Biome.VANILLA_SHUFFLE => true,
            Biome.VANILLALIKE => true,
            Biome.ISLANDS => true,
            Biome.CANYON => true,
            Biome.CALDERA => true,
            Biome.MOUNTAINOUS => true,
            Biome.RANDOM => true,
            Biome.RANDOM_NO_VANILLA => true,
            Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => true,
            _ => false
        };
    }

    public static bool IsEastBiome(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => true,
            Biome.VANILLA_SHUFFLE => true,
            Biome.VANILLALIKE => true,
            Biome.ISLANDS => true,
            Biome.CANYON => true,
            Biome.VOLCANO => true,
            Biome.MOUNTAINOUS => true,
            Biome.RANDOM => true,
            Biome.RANDOM_NO_VANILLA => true,
            Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => true,
            _ => false
        };
    }

    public static bool IsMazeBiome(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => true,
            Biome.VANILLA_SHUFFLE => true,
            Biome.VANILLALIKE => true,
            Biome.RANDOM => true,
            _ => false
        };
    }

    public static bool IsDMBiome(this Biome biome)
    {
        return biome switch
        {
            Biome.VANILLA => true,
            Biome.VANILLA_SHUFFLE => true,
            Biome.VANILLALIKE => true,
            Biome.ISLANDS => true,
            Biome.CANYON => true,
            Biome.CALDERA => true,
            Biome.MOUNTAINOUS => true,
            Biome.RANDOM => true,
            Biome.RANDOM_NO_VANILLA => true,
            Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => true,
            _ => false
        };
    }
}

[DefaultValue(NORMAL)]
public enum ContinentConnectionType
{
    [Description("Normal")]
    NORMAL,
    [Description("Anything Goes")]
    ANYTHING_GOES,
    [Description("RB Border Shuffle")]
    RB_BORDER_SHUFFLE,
    [Description("Transportation Shuffle")]
    TRANSPORTATION_SHUFFLE
}

[DefaultValue(NONE)]
public enum EncounterRate
{
    [Description("None")]
    NONE,
    [Description("Half")]
    HALF,
    [Description("Normal")]
    NORMAL,
    [Description("Random")]
    RANDOM
}

[DefaultValue(Lives3)]
public enum StartingLives
{
    [Description("1")]
    Lives1,
    [Description("2")]
    Lives2,
    [Description("3")]
    Lives3,
    [Description("4")]
    Lives4,
    [Description("5")]
    Lives5,
    [Description("8")]
    Lives8,
    [Description("16")]
    Lives16,
    [Description("Random (2-5)")]
    LivesRandom,
}

[DefaultValue(Default)]
public enum CharacterColor
{
    [Description("Default")]
    Default,
    [Description("Green")]
    Green,
    [Description("Dark Green")]
    DarkGreen,
    [Description("Aqua")]
    Aqua,
    [Description("Dark Blue")]
    DarkBlue,
    [Description("Purple")]
    Purple,
    [Description("Pink")]
    Pink,
    [Description("Orange")]
    Orange,
    [Description("Red")]
    Red,
    [Description("Turd")]
    Turd,
    [Description("Random")]
    Random,
}

[DefaultValue(Default)]
public enum BeamSprites
{
    [Description("Default")]
    Default,
    [Description("Fire")]
    Fire,
    [Description("Bubble")]
    Bubble,
    [Description("Rock")]
    Rock,
    [Description("Axe")]
    Axe,
    [Description("Hammer")]
    Hammer,
    [Description("Wizzrobe Beam")]
    WizzrobeBeam,
    [Description("Random")]
    Random,
}

[DefaultValue(Normal)]
public enum BeepThreshold
{
    [Description("Normal")]
    Normal,
    [Description("Half Bar")]
    HalfBar,
    [Description("Quarter Bar")]
    QuarterBar,
    [Description("Two Bars")]
    TwoBars
}

[DefaultValue(Normal)]
public enum BeepFrequency
{
    [Description("Normal")]
    Normal,
    [Description("Half Speed")]
    HalfSpeed,
    [Description("Quarter Speed")]
    QuarterSpeed,
    [Description("Off")]
    Off
}
    
public class StringValueAttribute(string v) : Attribute
{
    public string Value => v;

    public static string? GetStringValue(Enum value)
    {
        var type = value.GetType();
        var fi = type.GetRuntimeField(value.ToString());
        return (fi?.GetCustomAttributes(typeof(StringValueAttribute), false).FirstOrDefault() as StringValueAttribute)?.Value;
    }
}

public record EnumDescription
{
    public object? Value { get; init; }

    public string? Description { get; init; }
    
    public string? Help { get; init; }
    
    public override string ToString()
    {
        return Description ?? "";
    }
}

public static class Enums
{
    public static IEnumerable<EnumDescription> StartingTechList { get; } = ToDescriptions<StartingTechs>();
    public static IEnumerable<EnumDescription> StartingLivesList { get; } = ToDescriptions<StartingLives>();
    public static IEnumerable<EnumDescription> AttackEffectivenessList { get; } = ToDescriptions<AttackEffectiveness>();
    public static IEnumerable<EnumDescription> MagicEffectivenessList { get; } = ToDescriptions<MagicEffectiveness>();
    public static IEnumerable<EnumDescription> LifeEffectivenessList { get; } = ToDescriptions<LifeEffectiveness>();
    public static IEnumerable<EnumDescription> XPEffectivenessList { get; } = ToDescriptions<XPEffectiveness>();
    public static IEnumerable<EnumDescription> FireOptionList { get; } = ToDescriptions<FireOption>();
    public static IEnumerable<EnumDescription> NormalPalaceStyleList { get; } = ToDescriptions<NormalPalaceStyle>();
    public static IEnumerable<EnumDescription> GpPalaceStyleList { get; } = ToDescriptions<GPStyle>();
    public static IEnumerable<EnumDescription> WestBiomeList { get; } = ToDescriptions<Biome>(i => i.IsWestBiome());
    public static IEnumerable<EnumDescription> EastBiomeList { get; } = ToDescriptions<Biome>(i => i.IsEastBiome());
    public static IEnumerable<EnumDescription> MazeBiomeList { get; } = ToDescriptions<Biome>(i => i.IsMazeBiome());
    public static IEnumerable<EnumDescription> DMBiomeList { get; } = ToDescriptions<Biome>(i => i.IsDMBiome());
    public static IEnumerable<EnumDescription> ContinentConnectionTypeList { get; } = ToDescriptions<ContinentConnectionType>();
    public static IEnumerable<EnumDescription> EncounterRateList { get; } = ToDescriptions<EncounterRate>();
    public static IEnumerable<EnumDescription> CharacterColorList { get; } = ToDescriptions<CharacterColor>();
    public static IEnumerable<EnumDescription> BeamSpritesList { get; } = ToDescriptions<BeamSprites>();
    public static IEnumerable<EnumDescription> BeepThresholdList { get; } = ToDescriptions<BeepThreshold>();
    public static IEnumerable<EnumDescription> BeepFrequencyList { get; } = ToDescriptions<BeepFrequency>();

    public static IEnumerable<EnumDescription> ToDescriptions<T>(Func<T, bool>? filterExpression = null) where T : Enum
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException($"{nameof(T)} must be an enum type");

        if(filterExpression == null)
        {
            return Enum.GetValues(typeof(T)).Cast<Enum>().Select(i => i.ToDescription());
        }
        return Enum.GetValues(typeof(T)).Cast<T>().Where(filterExpression).Select(i => i.ToDescription());
    }

    public static object ToDefault(this Type enumType)
    {
        var attributes = enumType.GetCustomAttributes(typeof(DefaultValueAttribute), false);
        if (attributes.Length == 0)
        {
            throw new ArgumentException("Generic Type 'T' must have DefaultValueAttribute.");
        }
        return Enum.ToObject(enumType, (attributes[0] as DefaultValueAttribute)?.Value!);
    }
    
    public static EnumDescription ToDescription(this Enum value)
    {
        string description;
        string? help = null;
        
        var attributes = value.GetType().GetField(value.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        if (attributes?.Length is > 0)
        {
            description = (attributes[0] as DescriptionAttribute)?.Description!;
        }
        else
        {
            var ti = CultureInfo.CurrentCulture.TextInfo;
            description = ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
        }
        
        if (description.IndexOf(';') is var index && index != -1)
        {
            help = description.Substring(index + 1);
            description = description.Substring(0, index);
        }

        return new EnumDescription() { Value = value, Description = description, Help = help };
    }

    public static string? GetStringValue(Enum value)
    {
        string? output = null;
        Type type = value.GetType();
        var fi = type.GetField(value.ToString());
        if (fi?.GetCustomAttributes(typeof(StringValueAttribute),
                false) is StringValueAttribute[] { Length: > 0 } attrs)
        {
            output = attrs[0].Value;
        }
        return output;
    }

    public static T? Parse<T>(string input) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("Generic Type 'T' must be an Enum.");
        }
        if (string.IsNullOrEmpty(input)) return null;
        if (Enum.GetNames(typeof(T)).Any(
                e => e.Trim().Equals(input.Trim(), StringComparison.InvariantCultureIgnoreCase)))
        {
            return (T)Enum.Parse(typeof(T), input, true);
        }
        return null;
    }
}