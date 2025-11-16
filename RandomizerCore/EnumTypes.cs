using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Z2Randomizer.RandomizerCore;

[DefaultValue(EIGHT)]
public enum MaxHeartsOption
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

[DefaultValue(VANILLA)]
public enum XPEffectiveness
{
    [Description("Vanilla (No Randomization)"), RandomRangeInt(Low = 0, High = 0)]
    VANILLA,
    [Description("Low [-3 to +1]"), RandomRangeInt(Low = -3, High = 1), IsRandom]
    RANDOM_LOW,
    [Description("Average [-2 to +2]"), RandomRangeInt(Low = -2, High = 2), IsRandom]
    RANDOM,
    [Description("Average (Low Variance) [-1 to +1]"), RandomRangeInt(Low = -1, High = 1), IsRandom]
    LESS_VARIANCE,
    [Description("Above Average (Low Variance) [0 to +1]"), RandomRangeInt(Low = 0, High = 1), IsRandom]
    SLIGHTLY_HIGH,
    [Description("High [-1 to +3]"), RandomRangeInt(Low = -1, High = 3), IsRandom]
    RANDOM_HIGH,
    [Description("Wide [-4 to +4]"), RandomRangeInt(Low = -4, High = 4), IsRandom]
    WIDE,
    [Description("None"), RandomRangeInt(Low = -15, High = -15)]
    NONE
}

public enum EnemyLifeOption
{
    [Description("Vanilla")]
    VANILLA,
    [Description("Medium [-50% to +50%]"), RandomRangeDouble(Low = 0.5, High = 1.5)]
    MEDIUM,
    [Description("High [-0% to +100%]"), RandomRangeDouble(Low = 1.0, High = 2.0)]
    HIGH,
    [Description("Medium High [-50% to +100%]"), RandomRangeDouble(Low = 0.5, High = 2.0)]
    MEDIUM_HIGH,
    [Description("Medium High [-75% to +200%]"), RandomRangeDouble(Low = 0.25, High = 3.0)]
    WIDE,
}

[DefaultValue(ONLY_BOTS)]
public enum DripperEnemyOption
{
    [Description("Only Bots")]
    ONLY_BOTS,
    [Description("All Ground Enemies")]
    ANY_GROUND_ENEMY,
    [Description("Easier Ground Enemies")]
    EASIER_GROUND_ENEMIES,
    [Description("Easier Ground Enemies, Full HP")]
    EASIER_GROUND_ENEMIES_FULL_HP,
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
    [Description("Random (No Vanilla or Shuffle)")]
    RANDOM_NO_VANILLA_OR_SHUFFLE,
    [Description("Random (All Same)")]
    RANDOM_ALL,
    [Description("Random (Per Palace)")]
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
    public static bool IsCoordinateBased(this PalaceStyle style)
    {
        return style switch
        {
            PalaceStyle.SEQUENTIAL => true,
            PalaceStyle.RANDOM_WALK => true,
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
    [Description("Random (All Same)")]
    RANDOM_ALL,
    [Description("Random (Per Palace)")]
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
    [Description("Random (No Vanilla or Shuffle)"), IsRandom]
    RANDOM_NO_VANILLA_OR_SHUFFLE,
    [Description("Random (No Vanilla)"), IsRandom]
    RANDOM_NO_VANILLA,
    [Description("Random"), IsRandom]
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
}

[DefaultValue(CLASSIC)]
public enum ClimateEnum
{
    [Description("Classic")]
    CLASSIC,
    [Description("Vanilla-Weighted")]
    VANILLA_WEIGHTED_WEST,
    [Description("Vanilla-Weighted")]
    VANILLA_WEIGHTED_EAST,
    [Description("Chaos")]
    CHAOS,
    [Description("Wetlands")]
    WETLANDS,
    [Description("Great Lakes")]
    GREAT_LAKES,
    [Description("Scrubland")]
    SCRUBLAND,
    [Description("Random")]
    RANDOM
}

static class ClimateExtensions
{
    public static bool IsWestClimate(this ClimateEnum climate)
    {
        return climate switch
        {
            ClimateEnum.VANILLA_WEIGHTED_WEST => true,
            ClimateEnum.VANILLA_WEIGHTED_EAST => false,
            _ => true,
        };
    }

    public static bool IsEastClimate(this ClimateEnum climate)
    {
        return climate switch
        {
            ClimateEnum.VANILLA_WEIGHTED_WEST => false,
            ClimateEnum.VANILLA_WEIGHTED_EAST => true,
            _ => true,
        };
    }

    public static bool IsDmClimate(this ClimateEnum climate)
    {
        return climate switch
        {
            ClimateEnum.VANILLA_WEIGHTED_WEST => false,
            ClimateEnum.VANILLA_WEIGHTED_EAST => false,
            _ => true,
        };
    }

    public static bool IsMazeClimate(this ClimateEnum climate)
    {
        return climate switch
        {
            ClimateEnum.VANILLA_WEIGHTED_WEST => false,
            ClimateEnum.VANILLA_WEIGHTED_EAST => false,
            _ => true,
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

[AttributeUsage(AttributeTargets.Field)]
public class OverworldSizeMetaAttribute : Attribute
{
    public int Width { get; init; }
    public int Height { get; init; }
}
[DefaultValue(LARGE)]
public enum OverworldSizeOption
{
    [Description("Large (75x60)"), OverworldSizeMeta(Width = 75, Height = 60)]
    LARGE,
    [Description("Medium (52x52)"), OverworldSizeMeta(Width = 52, Height = 52)]
    MEDIUM,
    [Description("Small (44x44)"), OverworldSizeMeta(Width = 44, Height = 44)]
    SMALL,
}
[DefaultValue(LARGE)]
public enum DmSizeOption
{
    [Description("Large  (64x45 ~37 caves)"), OverworldSizeMeta(Width = 64, Height = 45)]
    LARGE,
    [Description("Medium (40x40 ~27 caves)"), OverworldSizeMeta(Width = 52, Height = 52)]
    MEDIUM,
    [Description("Small  (34x34 ~17 caves)"), OverworldSizeMeta(Width = 34, Height = 34)]
    SMALL,
    [Description("Tiny   (26x26  ~9 caves)"), OverworldSizeMeta(Width = 26, Height = 26)]
    TINY,
}
[DefaultValue(LARGE)]
public enum MazeSizeOption
{
    [Description("Large  (23x23)"), OverworldSizeMeta(Width = 23, Height = 23)]
    LARGE,
    [Description("Medium (19x19)"), OverworldSizeMeta(Width = 19, Height = 19)]
    MEDIUM,
    [Description("Small  (15x15)"), OverworldSizeMeta(Width = 15, Height = 15)]
    SMALL,
}
public static class OverworldSizeExtensions
{
    public static OverworldSizeMetaAttribute GetMeta(this OverworldSizeOption size)
    {
        var member = typeof(OverworldSizeOption).GetMember(size.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<OverworldSizeMetaAttribute>() ?? new OverworldSizeMetaAttribute();
    }

    public static OverworldSizeMetaAttribute GetMeta(this DmSizeOption size)
    {
        var member = typeof(DmSizeOption).GetMember(size.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<OverworldSizeMetaAttribute>() ?? new OverworldSizeMetaAttribute();
    }

    public static OverworldSizeMetaAttribute GetMeta(this MazeSizeOption size)
    {
        var member = typeof(MazeSizeOption).GetMember(size.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<OverworldSizeMetaAttribute>() ?? new OverworldSizeMetaAttribute();
    }
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

public static class StartingResourceLimitExtensions
{
    public static int AsInt(this StartingResourceLimit resourceLimit)
    {
        return resourceLimit switch
        {
            StartingResourceLimit.ONE => 1,
            StartingResourceLimit.TWO => 2,
            StartingResourceLimit.FOUR => 4,
            StartingResourceLimit.NO_LIMIT => 8,
            _ => throw new Exception("Unrecognized StartingResourceLimit")
        };
    }
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
public enum NesColor
{
    [Description("Default")] Default = -1,
    [Description("Random")] Random = -2,

    [Description("Granite Gray")] GraniteGray = 0x00, // #656565
    [Description("Navy Blue")] NavyBlue = 0x01, // #00127d
    [Description("Neon Blue")] NeonBlue = 0x02, // #18008e
    [Description("Indigo")] Indigo = 0x03, // #360082
    [Description("Violet")] Violet = 0x04,
    [Description("Dark Scarlet")] DarkScarlet = 0x05, // #5a0018
    [Description("Dark Rust")] DarkRust = 0x06, // #4f0500
    [Description("Brown")] Brown = 0x07,
    [Description("Dark Olive")] DarkOlive = 0x08, // #1d3100
    [Description("Dark Green")] DarkGreen = 0x09, // #003d00
    [Description("Forest Green")] ForestGreen = 0x0A, // #004100
    [Description("Pond")] Pond = 0x0B, // #003b17
    [Description("Lagoon")] Lagoon = 0x0C, // #002e55

    [Description("Light Gray")] LightGray = 0x10,
    [Description("Lake")] Lake = 0x11, // #194ec8
    [Description("Violet Blue")] VioletBlue = 0x12,
    [Description("Vivid Purple")] VividPurple = 0x13,
    [Description("Deep Violet")] DeepViolet = 0x14, // #931bae
    [Description("Jazzberry Jam")] JazzberryJam = 0x15, // #9e1a5e
    [Description("Burnt Sienna")] BurntSienna = 0x16, // #993200
    [Description("Dark Bronze")] DarkBronze = 0x17, // #7b4b00
    [Description("Mud Green")] MudGreen = 0x18, // #5b6700
    [Description("Grass")] Grass = 0x19, // #267a00
    [Description("Green")] Green = 0x1A,
    [Description("Emerald Green")] EmeraldGreen = 0x1B,
    [Description("Sea Blue")] SeaBlue = 0x1C, // #006e8a
    [Description("Black")] Black = 0x1D,

    [Description("White")] White20 = 0x20,
    [Description("Sky Blue")] SkyBlue = 0x21,
    [Description("Periwinkle")] Periwinkle = 0x22, // #8e89ff
    [Description("Orchid Purple")] OrchidPurple = 0x23,
    [Description("Bubblegum Pink")] BubblegumPink = 0x24,
    [Description("Rose Pink")] RosePink = 0x25,
    [Description("Coral")] Coral = 0x26,
    [Description("Goldenrod")] Goldenrod = 0x27,
    [Description("Olive Yellow")] OliveYellow = 0x28, // #b9b40a
    [Description("Lime Green")] LimeGreen = 0x29,
    [Description("Bright Green")] BrightGreen = 0x2A,
    [Description("Mint Green")] MintGreen = 0x2B,
    [Description("Turquoise")] Turquoise = 0x2C,
    [Description("Medium Gray")] MediumGray = 0x2D,

    [Description("White")] White = 0x30,
    [Description("Pale Sky Blue")] PaleSkyBlue = 0x31,
    [Description("Light Lavender")] LightLavender = 0x32,
    [Description("Lilac")] Lilac = 0x33,
    [Description("Light Pink")] LightPink = 0x34,
    [Description("Pale Rose")] PaleRose = 0x35,
    [Description("Peach")] Peach = 0x36,
    [Description("Tan")] Tan = 0x37, // #f8dfb1
    [Description("Medium Champagne")] MediumChampagne = 0x38, // #edeaa4
    [Description("Pale Green")] PaleGreen = 0x39,
    [Description("Light Green")] LightGreen = 0x3A,
    [Description("Seafoam Green")] SeafoamGreen = 0x3B,
    [Description("Light Cyan")] LightCyan = 0x3C,
    [Description("Silver Gray")] SilverGray = 0x3D,
}

public enum BeamPalette
{
    Unspecified = -2,
    Flashing = -1,
    Link = 0,
    Orange = 1,
    Red = 2,  // bad idea to use since this palette is not visible in caves without the candle
    Blue = 3, // ^
}

public enum BeamRotation
{
    None = 0x00,
    FlipVertical = 0x01,
    Rotate90 = 0xC0,
}

[AttributeUsage(AttributeTargets.Field)]
public class BeamSpriteMetaAttribute : Attribute
{
    /// <summary>
    /// If non-zero, the 8x16 sprite at this address will replace the fire sprite.
    /// </summary>
    public int ChrAddress { get; init; } = 0;

    /// <summary>
    /// Primary palette used by this beam.
    /// </summary>
    public BeamPalette BeamPalette { get; init; } = BeamPalette.Flashing;

    /// <summary>
    /// Optional secondary palette for the fire projectile.
    /// If not explicitly set, falls back to BeamPalette, or Orange if BeamPalette is Flashing.
    /// </summary>
    public BeamPalette FirePalette { get; init; } = BeamPalette.Unspecified;

    /// <summary>
    /// Sprite rotation/flip flag.
    /// </summary>
    public BeamRotation Rotate { get; init; } = BeamRotation.None;

    public BeamSpriteMetaAttribute() { }
}

[DefaultValue(DEFAULT)]
public enum BeamSprites
{
    [Description("Default")]
    DEFAULT,

    [Description("Fire"), BeamSpriteMeta(BeamPalette = BeamPalette.Orange, Rotate = BeamRotation.FlipVertical)]
    FIRE,

    [Description("Bubble"), BeamSpriteMeta(ChrAddress = 0xaa0)]
    BUBBLE,

    [Description("Rock"), BeamSpriteMeta(ChrAddress = 0x2ae0)]
    ROCK,

    [Description("Energy Ball"), BeamSpriteMeta(ChrAddress = 0x0ce0)]
    ENERGY_BALL,

    [Description("Wizard Beam"), BeamSpriteMeta(ChrAddress = 0x14dc0)]
    WIZARD_BEAM,

    // (Red Daira's axe projectile switches between sprite f6 and fa for smoother rotation, we can't emulate that)
    [Description("Daira Axe"), BeamSpriteMeta(ChrAddress = 0x2f60, BeamPalette = BeamPalette.Orange, Rotate = BeamRotation.Rotate90)]
    AXE,

    [Description("Doomknocker Mace"), BeamSpriteMeta(ChrAddress = 0x12ee0, Rotate = BeamRotation.Rotate90)]
    HAMMER,

    [Description("Geru Mace"), BeamSpriteMeta(ChrAddress = 0x5260, Rotate = BeamRotation.Rotate90)]
    GERU_MACE,

    [Description("Guma Mace"), BeamSpriteMeta(ChrAddress = 0xaee0, Rotate = BeamRotation.Rotate90)]
    GUMA_MACE,

    [Description("Boomerang"), BeamSpriteMeta(ChrAddress = 0x30c0, Rotate = BeamRotation.Rotate90)]
    BOOMERANG,

    [Description("Spicy Chicken Fire"), BeamSpriteMeta(ChrAddress = 0xd3e0, BeamPalette = BeamPalette.Orange, Rotate = BeamRotation.Rotate90)]
    SPICY_CHICKEN,

    [Description("Random")]
    RANDOM,
}

public static class BeamSpriteExtensions
{
    public static BeamSpriteMetaAttribute GetMeta(this BeamSprites sprite)
    {
        var member = typeof(BeamSprites).GetMember(sprite.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<BeamSpriteMetaAttribute>() ?? new BeamSpriteMetaAttribute();
    }
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

[DefaultValue(HIDE)]
public enum LessImportantLocationsOption
{
    [Description("Blend In")]
    HIDE,
    [Description("Isolate")]
    ISOLATE,
    [Description("Remove")]
    REMOVE,
    [Description("Random")]
    RANDOM,
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

public enum BossRoomMinDistance
{
    [Description("None")]
    NONE = 0,
    [Description("8 rooms")]
    SHORT = 8,
    [Description("12 rooms")]
    MEDIUM = 12,
    [Description("Max")]
    MAX = 24,
}

public enum PalaceLengthOption
{
    [Description("Short  (50%-75%)")]
    SHORT = 0,
    [Description("Medium (65%-85%)")]
    MEDIUM = 1,
    [Description("Full")]
    FULL = 2,
    [Description("Random (50%-115%)")]
    RANDOM = 3
}

public enum PalaceItemRoomCount
{
    [Description("Zero")]
    ZERO = 0,
    [Description("One")]
    ONE = 1,
    [Description("Two")]
    TWO = 2,
    [Description("Random")]
    RANDOM_NOT_ZERO = 3,
    [Description("Random (Include Zero)")]
    RANDOM_INCLUDE_ZERO = 4
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

public class IsRandomAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class RandomRangeDoubleAttribute : Attribute
{
    /// <summary>
    /// Inclusive lower end of randomized value
    /// </summary>
    public double Low { get; init; }
    /// <summary>
    /// Inclusive upper end of randomized value
    /// </summary>
    public double High { get; init; }
}

[AttributeUsage(AttributeTargets.Field)]
public class RandomRangeIntAttribute : Attribute
{
    /// <summary>
    /// Inclusive lower end of randomized value
    /// </summary>
    public int Low { get; init; }
    /// <summary>
    /// Inclusive upper end of randomized value
    /// </summary>
    public int High { get; init; }
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
    public static IEnumerable<EnumDescription> DripperEnemyOptionList { get; } = ToDescriptions<DripperEnemyOption>();
    public static IEnumerable<EnumDescription> EnemyLifeOptionList { get; } = ToDescriptions<EnemyLifeOption>();
    public static IEnumerable<EnumDescription> BossLifeOptionList { get; } = ToDescriptions<EnemyLifeOption>(i => i != EnemyLifeOption.WIDE);
    public static IEnumerable<EnumDescription> FireOptionList { get; } = ToDescriptions<FireOption>();
    public static IEnumerable<EnumDescription> BossRoomMinDistanceOptions { get; } = ToDescriptions<BossRoomMinDistance>();
    public static IEnumerable<EnumDescription> PalaceLengthOptionList { get; } = ToDescriptions<PalaceLengthOption>();
    public static IEnumerable<EnumDescription> PalaceItemRoomCountOptions { get; } = ToDescriptions<PalaceItemRoomCount>();
    public static IEnumerable<EnumDescription> NormalPalaceStyleList { get; }
        = ToDescriptions<PalaceStyle>(i => i != PalaceStyle.RANDOM && i != PalaceStyle.RANDOM_NO_VANILLA_OR_SHUFFLE);
    public static IEnumerable<EnumDescription> GpPalaceStyleList { get; } 
        = ToDescriptions<PalaceStyle>(i => i != PalaceStyle.RANDOM_PER_PALACE && i != PalaceStyle.RANDOM_ALL);
    public static IEnumerable<EnumDescription> BossRoomsExitTypeList { get; } = ToDescriptions<BossRoomsExitType>();

    public static IEnumerable<EnumDescription> WestBiomeList { get; } = ToDescriptions<Biome>(i => i.IsWestBiome());
    public static IEnumerable<EnumDescription> EastBiomeList { get; } = ToDescriptions<Biome>(i => i.IsEastBiome());
    public static IEnumerable<EnumDescription> DMBiomeList { get; } = ToDescriptions<Biome>(i => i.IsDMBiome());
    public static IEnumerable<EnumDescription> MazeBiomeList { get; } = ToDescriptions<Biome>(i => i.IsMazeBiome());
    public static IEnumerable<EnumDescription> WestClimateList { get; } = ToDescriptions<ClimateEnum>(i => i.IsWestClimate());
    public static IEnumerable<EnumDescription> EastClimateList { get; } = ToDescriptions<ClimateEnum>(i => i.IsEastClimate());
    public static IEnumerable<EnumDescription> DmClimateList { get; } = ToDescriptions<ClimateEnum>(i => i.IsDmClimate());
    public static IEnumerable<EnumDescription> MazeClimateList { get; } = ToDescriptions<ClimateEnum>(i => i.IsMazeClimate());
    public static IEnumerable<EnumDescription> ContinentConnectionTypeList { get; } = ToDescriptions<ContinentConnectionType>();
    public static IEnumerable<EnumDescription> OverworldSizeList { get; } = ToDescriptions<OverworldSizeOption>();
    public static IEnumerable<EnumDescription> DmSizeList { get; } = ToDescriptions<DmSizeOption>();
    public static IEnumerable<EnumDescription> MazeSizeList { get; } = ToDescriptions<MazeSizeOption>();
    public static IEnumerable<EnumDescription> LessImportantLocationsOptionList { get; } = ToDescriptions<LessImportantLocationsOption>();

    public static IEnumerable<EnumDescription> EncounterRateList { get; } = ToDescriptions<EncounterRate>();
    public static IEnumerable<EnumDescription> CharacterColorList { get; } = ToDescriptions<NesColor>();
    public static IEnumerable<EnumDescription> BeamSpritesList { get; } = ToDescriptions<BeamSprites>();
    public static IEnumerable<EnumDescription> BeepThresholdList { get; } = ToDescriptions<BeepThreshold>();
    public static IEnumerable<EnumDescription> BeepFrequencyList { get; } = ToDescriptions<BeepFrequency>();
    public static IEnumerable<EnumDescription> MaxHeartsOptionList { get; } = ToDescriptions<MaxHeartsOption>();
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

    public static bool IsRandom(this Enum self)
    {
        Type type = self.GetType();
        string? name = Enum.GetName(type, self);
        if (name == null) { return false; }
        FieldInfo? fieldInfo = type.GetField(name);
        if (fieldInfo == null) { return false; }
        return fieldInfo.IsDefined(typeof(IsRandomAttribute), inherit: false);
    }

    public static RandomRangeDoubleAttribute? GetRandomRangeDouble(this Enum self)
    {
        Type type = self.GetType();
        string? name = Enum.GetName(type, self);
        if (name == null) { return null; }
        FieldInfo? fieldInfo = type.GetField(name);
        if (fieldInfo == null) { return null; }
        return fieldInfo.GetCustomAttribute<RandomRangeDoubleAttribute>(inherit: false);
    }

    public static RandomRangeIntAttribute? GetRandomRangeInt(this Enum self)
    {
        Type type = self.GetType();
        string? name = Enum.GetName(type, self);
        if (name == null) { return null; }
        FieldInfo? fieldInfo = type.GetField(name);
        if (fieldInfo == null) { return null; }
        return fieldInfo.GetCustomAttribute<RandomRangeIntAttribute>(inherit: false);
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