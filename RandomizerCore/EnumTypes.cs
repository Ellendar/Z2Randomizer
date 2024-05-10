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

[DefaultValue(NONE)]
public enum StatEffectiveness
{
    [Description("None")]
    NONE,
    [Description("Low")]
    LOW,
    [Description("Vanilla")]
    VANILLA,
    [Description("Average")]
    AVERAGE,
    [Description("High")]
    HIGH,
    [Description("Max")]
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
public enum PalaceStyle
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
    public static IEnumerable<EnumDescription> StartingTechList { get; } = ToDescriptions(typeof(StartingTechs));
    public static IEnumerable<EnumDescription> StatEffectivenessList { get; } = ToDescriptions(typeof(StatEffectiveness));
    public static IEnumerable<EnumDescription> FireOptionList { get; } = ToDescriptions(typeof(FireOption));
    public static IEnumerable<EnumDescription> PalaceStyleList { get; } = ToDescriptions(typeof(PalaceStyle));
    public static IEnumerable<EnumDescription> BiomeList { get; } = ToDescriptions(typeof(Biome));
    public static IEnumerable<EnumDescription> ContinentConnectionTypeList { get; } = ToDescriptions(typeof(ContinentConnectionType));
    public static IEnumerable<EnumDescription> EncounterRateList { get; } = ToDescriptions(typeof(EncounterRate));
        
    public static IEnumerable<EnumDescription> ToDescriptions(Type t)
    {
        if (!t.IsEnum)
            throw new ArgumentException($"{nameof(t)} must be an enum type");
    
        return Enum.GetValues(t).Cast<Enum>().Select(ToDescription).ToList();
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