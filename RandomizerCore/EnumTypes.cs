using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace RandomizerCore;

[DefaultValue(EIGHT)]
public enum StartingHeartsMaxOption
{
    [Description("1")]
    ONE = 1,
    [Description("2")]
    TWO = 2,
    [Description("3")]
    THREE = 3,
    [Description("4")]
    FOUR = 4,
    [Description("5")]
    FIVE = 5,
    [Description("6")]
    SIX = 6,
    [Description("7")]
    SEVEN = 7,
    [Description("8")]
    EIGHT = 8,
    [Description("+1")]
    PLUS_ONE = 9,
    [Description("+2")]
    PLUS_TWO = 10,
    [Description("+3")]
    PLUS_THREE = 11,
    [Description("+4")]
    PLUS_FOUR = 12,
    [Description("Random")]
    RANDOM = 13,
}

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

public static class StartingTechsExtensions
{
    public static bool StartWithDownstab(this StartingTechs techs)
    {
        return techs switch
        {
            StartingTechs.BOTH => true,
            StartingTechs.DOWNSTAB => true,
            _ => false
        };
    }
    public static bool StartWithUpstab(this StartingTechs techs)
    {
        return techs switch
        {
            StartingTechs.BOTH => true,
            StartingTechs.UPSTAB => true,
            _ => false
        };
    }
}

[DefaultValue(VANILLA)]
public enum AttackEffectiveness
{
    [Description("Vanilla")]
    VANILLA,
    [Description("Low Attack")]
    LOW,
    [Description("Randomize (Low)")]
    AVERAGE_LOW,
    [Description("Randomize")]
    AVERAGE,
    [Description("Randomize (High)")]
    AVERAGE_HIGH,
    [Description("High Attack")]
    HIGH,
    [Description("Instant Kill")]
    OHKO
}

[DefaultValue(VANILLA)]
public enum MagicEffectiveness
{
    [Description("Vanilla")]
    VANILLA,
    [Description("High Spell Cost")]
    HIGH_COST,
    [Description("Randomize (High Cost)")]
    AVERAGE_HIGH_COST,
    [Description("Randomize")]
    AVERAGE,
    [Description("Randomize (Low Cost)")]
    AVERAGE_LOW_COST,
    [Description("Low Spell Cost")]
    LOW_COST,
    [Description("Free Spells")]
    FREE
}

[DefaultValue(VANILLA)]
public enum LifeEffectiveness
{
    [Description("Vanilla")]
    VANILLA,
    [Description("OHKO Link")]
    OHKO,
    [Description("Randomize (Low)")]
    AVERAGE_LOW,
    [Description("Randomize")]
    AVERAGE,
    [Description("Randomize (High)")]
    AVERAGE_HIGH,
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
    RANDOM_LOW,
    [Description("Average")]
    RANDOM,
    [Description("High")]
    RANDOM_HIGH,
    [Description("None")]
    NONE
}

public static class XPEffectivenessExtensions
{
    public static bool IsRandom(this XPEffectiveness effectiveness)
    {
        return effectiveness switch
        {
            XPEffectiveness.RANDOM_LOW => true,
            XPEffectiveness.RANDOM_HIGH => true,
            XPEffectiveness.RANDOM => true,
            XPEffectiveness.NONE => false,
            XPEffectiveness.VANILLA => false,
            _ => throw new Exception("Unrecognized XPEffectiveness")
        };
    }
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

/*
[DefaultValue(VANILLA)]
public enum NormalPalaceStyle
{
    [Description("Vanilla")]
    VANILLA,
    [Description("Shuffled")]
    SHUFFLED,
    [Description("Reconstructed")]
    RECONSTRUCTED,
    [Description("Sequential")]
    SEQUENTIAL,
    [Description("Condensed")]
    CONDENSED,
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
    [Description("Sequential")]
    SEQUENTIAL,
    [Description("Condensed")]
    CONDENSED,
    [Description("Chaos")]
    CHAOS,
    [Description("Random")]
    RANDOM
}
*/

[DefaultValue(VANILLA)]
public enum PalaceStyle
{
    [Description("Vanilla")]
    VANILLA,
    [Description("Shuffled")]
    SHUFFLED,
    [Description("Reconstructed")]
    RECONSTRUCTED,
    [Description("Sequential")]
    SEQUENTIAL,
    [Description("Random Walk")]
    RANDOM_WALK,
    [Description("Chaos")]
    CHAOS,
    [Description("Random")]
    RANDOM,
    [Description("Random(All Same)")]
    RANDOM_ALL,
    [Description("Random(Per Palace)")]
    RANDOM_PER_PALACE
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
}

[DefaultValue(OVERWORLD)]
public enum BossRoomsExitType
{
    [Description("Overworld")]
    OVERWORLD,
    [Description("More Palace")]
    PALACE,
    [Description("Random(All Same)")]
    RANDOM_ALL,
    [Description("Random(Per Palace)")]
    RANDOM_PER_PALACE
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
    [Description("Transportation Shuffle")]
    TRANSPORTATION_SHUFFLE,
    [Description("Anything Goes")]
    ANYTHING_GOES
}

[DefaultValue(NO_LIMIT)]
public enum StartingResourceLimit
{
    [Description("No Limit")]
    NO_LIMIT,
    [Description("1")]
    ONE,
    [Description("2")]
    TWO,
    [Description("4")]
    FOUR,
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
    [Description("Light Blue")]
    LightBlue,
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
    [Description("White")]
    White,
    [Description("Light Gray")]
    LightGray,
    [Description("Dark Gray")]
    DarkGray,
    [Description("Black")]
    Black,
    [Description("Random")]
    Random,
}

[DefaultValue(DEFAULT)]
public enum BeamSprites
{
    [Description("Default")]
    DEFAULT,
    [Description("Fire")]
    FIRE,
    [Description("Bubble")]
    BUBBLE,
    [Description("Rock")]
    ROCK,
    [Description("Axe")]
    AXE,
    [Description("Hammer")]
    HAMMER,
    [Description("Wizzrobe Beam")]
    WIZZROBE_BEAM,
    [Description("Random")]
    RANDOM,
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

[DefaultValue(HALF)]
public enum IndeterminateOptionRate
{
    [Description("25%")]
    QUARTER,
    [Description("50%")]
    HALF,
    [Description("75%")]
    THREE_QUARTERS,
    [Description("90%")]
    NINETY_PERCENT
}

[DefaultValue(PATH)]
public enum RiverDevilBlockerOption
{
    [Description("Blocks Path")]
    PATH,
    [Description("Blocks Cave")]
    CAVE,
    [Description("Blocks Town")]
    SIEGE,
    [Description("Random")]
    RANDOM
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
    public static IEnumerable<EnumDescription> NormalPalaceStyleList { get; }  = ToDescriptions<PalaceStyle>(i => i != PalaceStyle.RANDOM);
    public static IEnumerable<EnumDescription> GpPalaceStyleList { get; } 
        = ToDescriptions<PalaceStyle>(i => i != PalaceStyle.RANDOM_PER_PALACE && i != PalaceStyle.RANDOM_ALL);
    public static IEnumerable<EnumDescription> BossRoomsExitTypeList { get; } = ToDescriptions<BossRoomsExitType>();

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
    public static IEnumerable<EnumDescription> StartingHeartsMaxOptionList { get; } = ToDescriptions<StartingHeartsMaxOption>();
    public static IEnumerable<EnumDescription> IndeterminateOptionRateList { get; } = ToDescriptions<IndeterminateOptionRate>();
    public static IEnumerable<EnumDescription> RiverDevilBlockerOptionList { get; } = ToDescriptions<RiverDevilBlockerOption>();
    public static IEnumerable<EnumDescription> StartingResourceLimitList { get; } = ToDescriptions<StartingResourceLimit>();
    

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