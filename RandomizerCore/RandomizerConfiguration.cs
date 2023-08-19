using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Z2Randomizer.Core.Flags;
using Z2Randomizer.Core.Overworld;
using System.Text.Json;

namespace Z2Randomizer.Core;

public class RandomizerConfiguration
{
    Logger logger = LogManager.GetCurrentClassLogger();
    //Meta
    [IgnoreInFlags]
    public bool SaveRom { get; set; }
    [IgnoreInFlags]
    public int Seed { get; set; }
    [IgnoreInFlags]
    public string FileName { get; set; }

    //Start Configuration
    public bool ShuffleStartingItems { get; set; }
    public bool StartWithCandle { get; set; }
    public bool StartWithGlove { get; set; }
    public bool StartWithRaft { get; set; }
    public bool StartWithBoots { get; set; }
    public bool StartWithFlute { get; set; }
    public bool StartWithCross { get; set; }
    public bool StartWithHammer { get; set; }
    public bool StartWithMagicKey { get; set; }

    public bool ShuffleStartingSpells { get; set; }
    public bool StartWithShield { get; set; }
    public bool StartWithJump { get; set; }
    public bool StartWithLife { get; set; }
    public bool StartWithFairy { get; set; }
    public bool StartWithFire { get; set; }
    public bool StartWithReflect { get; set; }
    public bool StartWithSpell { get; set; }
    public bool StartWithThunder { get; set; }

    [Limit(8)]
    [Minimum(1)]
    public int? StartingHeartContainersMin { get; set; }
    [Limit(8)]
    [Minimum(1)]
    public int? StartingHeartContainersMax { get; set; }
    [Limit(11)]
    [Minimum(1)]
    public int? MaxHeartContainers { get; set; }
    public bool? StartWithUpstab { get; set; }
    public bool? StartWithDownstab { get; set; }
    public bool ShuffleStartingLives { get; set; }
    [Limit(8)]
    [Minimum(1)]
    public int StartingAttackLevel { get; set; }
    [Limit(8)]
    [Minimum(1)]
    public int StartingMagicLevel { get; set; }
    [Limit(8)]
    [Minimum(1)]
    public int StartingLifeLevel { get; set; }

    //Overworld
    public bool? PalacesCanSwapContinents { get; set; }
    public bool? ShuffleGP { get; set; }
    public bool? ShuffleEncounters { get; set; }
    public bool AllowUnsafePathEncounters { get; set; }
    public bool IncludeLavaInEncounterShuffle { get; set; }
    public EncounterRate EncounterRate { get; set; }
    public bool? HidePalace { get; set; }
    public bool? HideKasuto { get; set; }
    public bool? ShuffleWhichLocationIsHidden { get; set; }
    public bool? HideLessImportantLocations { get; set; }
    //Sane caves
    public bool? RestrictConnectionCaveShuffle { get; set; }
    public bool AllowConnectionCavesToBeBoulderBlocked { get; set; }
    public bool? GoodBoots { get; set; }
    public bool? GenerateBaguWoods { get; set; }
    public ContinentConnectionType ContinentConnectionType { get; set; }
    public Biome WestBiome { get; set; }
    public Biome EastBiome { get; set; }
    public Biome DMBiome { get; set; }
    public Biome MazeBiome { get; set; }
    //XXX: I need to work out how this is going to be stored in configuration / serialized. Maybe climate needs a 3rd type
    //I really wish C# had Java's Enums.
    [IgnoreInFlags]
    public Climate Climate { get; set; }
    public bool VanillaShuffleUsesActualTerrain { get; set; }

    //Palaces
    public PalaceStyle PalaceStyle { get; set; }
    public bool? IncludeCommunityRooms { get; set; }
    public bool BlockingRoomsInAnyPalace { get; set; }
    public bool? BossRoomsExitToPalace { get; set; }
    public bool? ShortGP { get; set; }
    public bool? TBirdRequired { get; set; }
    public bool RemoveTBird { get; set; }
    public bool RestartAtPalacesOnGameOver { get; set; }
    public bool ChangePalacePallettes { get; set; }
    public bool RandomizeBossItemDrop { get; set; }
    [Limit(7)]
    public int PalacesToCompleteMin { get; set; }
    [Limit(7)]
    public int PalacesToCompleteMax { get; set; }
    public bool NoDuplicateRooms { get; set; }
    public bool GeneratorsAlwaysMatch { get; set; }

    //Levels
    public bool ShuffleAttackExperience { get; set; }
    public bool ShuffleMagicExperience { get; set; }
    public bool ShuffleLifeExperience { get; set; }
    [Limit(8)]
    [Minimum(1)]
    public int AttackLevelCap { get; set; }
    [Limit(8)]
    [Minimum(1)]
    public int MagicLevelCap { get; set; }
    [Limit(8)]
    [Minimum(1)]
    public int LifeLevelCap { get; set; }
    public bool ScaleLevelRequirementsToCap { get; set; }
    public StatEffectiveness AttackEffectiveness { get; set; }
    public StatEffectiveness MagicEffectiveness { get; set; }
    public StatEffectiveness LifeEffectiveness { get; set; }

    //Spells
    public bool ShuffleLifeRefillAmount { get; set; }
    public bool? ShuffleSpellLocations { get; set; }
    public bool? DisableMagicContainerRequirements { get; set; }
    public bool? RandomizeSpellSpellEnemy { get; set; }
    public bool? SwapUpAndDownStab { get; set; }
    public FireOption FireOption { get; set; }

    //Enemies
    public bool? ShuffleOverworldEnemies { get; set; }
    public bool? ShufflePalaceEnemies { get; set; }
    public bool ShuffleDripperEnemy { get; set; }
    public bool? MixLargeAndSmallEnemies { get; set; }
    public bool ShuffleEnemyHP { get; set; }
    public bool ShuffleXPStealers { get; set; }
    public bool ShuffleXPStolenAmount { get; set; }
    public bool ShuffleSwordImmunity { get; set; }
    public StatEffectiveness EnemyXPDrops { get; set; }

    //Items
    public bool? ShufflePalaceItems { get; set; }
    public bool? ShuffleOverworldItems { get; set; }
    public bool? MixOverworldAndPalaceItems { get; set; }
    public bool? IncludePBagCavesInItemShuffle { get; set; }
    public bool ShuffleSmallItems { get; set; }
    public bool? PalacesContainExtraKeys { get; set; }
    public bool RandomizeNewKasutoJarRequirements { get; set; }
    public bool? RemoveSpellItems { get; set; }
    public bool? ShufflePBagAmounts { get; set; }

    //Drops
    public bool ShuffleItemDropFrequency { get; set; }
    public bool RandomizeDrops { get; set; }
    public bool StandardizeDrops { get; set; }
    public bool SmallEnemiesCanDropBlueJar { get; set; }
    public bool SmallEnemiesCanDropRedJar { get; set; }
    public bool SmallEnemiesCanDropSmallBag { get; set; }
    public bool SmallEnemiesCanDropMediumBag { get; set; }
    public bool SmallEnemiesCanDropLargeBag { get; set; }
    public bool SmallEnemiesCanDropXLBag { get; set; }
    public bool SmallEnemiesCanDrop1up { get; set; }
    public bool SmallEnemiesCanDropKey { get; set; }
    public bool LargeEnemiesCanDropBlueJar { get; set; }
    public bool LargeEnemiesCanDropRedJar { get; set; }
    public bool LargeEnemiesCanDropSmallBag { get; set; }
    public bool LargeEnemiesCanDropMediumBag { get; set; }
    public bool LargeEnemiesCanDropLargeBag { get; set; }
    public bool LargeEnemiesCanDropXLBag { get; set; }
    public bool LargeEnemiesCanDrop1up { get; set; }
    public bool LargeEnemiesCanDropKey { get; set; }

    //Hints
    public bool? EnableHelpfulHints { get; set; }
    public bool? EnableSpellItemHints { get; set; }
    public bool? EnableTownNameHints { get; set; }
    public bool UseCommunityHints { get; set; }

    //Misc
    [IgnoreInFlags]
    public byte BeepFrequency { get; set; }
    [IgnoreInFlags]
    public byte BeepThreshold { get; set; }
    [IgnoreInFlags]
    public bool DisableMusic { get; set; }
    public bool JumpAlwaysOn { get; set; }
    public bool DashAlwaysOn { get; set; }
    public bool PermanmentBeamSword { get; set; }
    [IgnoreInFlags]
    public bool FastSpellCasting { get; set; }
    public bool ShuffleSpritePalettes { get; set; }
    [IgnoreInFlags]
    public bool UpAOnController1 { get; set; }
    [IgnoreInFlags]
    public bool RemoveFlashing { get; set; }
    [IgnoreInFlags]
    public int Sprite { get; set; }
    [IgnoreInFlags]
    public string Tunic { get; set; }
    [IgnoreInFlags]
    public string ShieldTunic { get; set; }
    [IgnoreInFlags]
    public string BeamSprite { get; set; }
    [IgnoreInFlags]
    public bool UseCustomRooms { get; set; }


    //This is a lazy backwards implementation Digshake's base64 encoding system.
    //There should be a seperate class that does the full encode/decode cycle for both projects.
    private static Dictionary<char, int> BASE64_DECODE = new Dictionary<char, int>(64)
    {
        {'A', 0},
        {'B', 1},
        {'C', 2},
        {'D', 3},
        {'E', 4},
        {'F', 5},
        {'G', 6},
        {'H', 7},
        {'J', 8},
        {'K', 9},
        {'L', 10},
        {'M', 11},
        {'N', 12},
        {'O', 13},
        {'P', 14},
        {'Q', 15},
        {'R', 16},
        {'S', 17},
        {'T', 18},
        {'U', 19},
        {'V', 20},
        {'W', 21},
        {'X', 22},
        {'Y', 23},
        {'Z', 24},

        {'a', 25},
        {'b', 26},
        {'c', 27},
        {'d', 28},
        {'e', 29},
        {'f', 30},
        {'g', 31},
        {'h', 32},
        {'i', 33},
        {'j', 34},
        {'k', 35},
        {'m', 36},
        {'n', 37},
        {'o', 38},
        {'p', 39},
        {'q', 40},
        {'r', 41},
        {'s', 42},
        {'t', 43},
        {'u', 44},
        {'v', 45},
        {'w', 46},
        {'x', 47},
        {'y', 48},
        {'z', 49},

        {'1', 50},
        {'2', 51},
        {'3', 52},
        {'4', 53},
        {'5', 54},
        {'6', 55},
        {'7', 56},
        {'8', 57},
        {'9', 58},
        {'0', 59},
        {'!', 60},
        {'@', 61},
        {'#', 62},
        {'+', 63},
    };


    public RandomizerConfiguration()
    {
        SaveRom = true;

        StartingAttackLevel = 1;
        StartingMagicLevel = 1;
        StartingLifeLevel = 1;

        MaxHeartContainers = 8;
        StartingHeartContainersMin = 8;
        StartingHeartContainersMax = 8;

        AttackLevelCap = 8;
        MagicLevelCap = 8;
        LifeLevelCap = 8;

        FastSpellCasting = false;
        ShuffleSpritePalettes = false;
        PermanmentBeamSword = false;
        UpAOnController1 = false;
        RemoveFlashing = false;
        Sprite = 0;
        Tunic = "Default";
        ShieldTunic = "Orange";
        BeamSprite = "Default";
        UseCustomRooms = false;
    }

    public RandomizerConfiguration(string flags) : this()
    {
        FlagReader flagReader = new FlagReader(flags);
        PropertyInfo[] properties = this.GetType().GetProperties();
        Type thisType = typeof(RandomizerConfiguration);
        foreach (PropertyInfo property in properties)
        {
            Type propertyType = property.PropertyType;
            int limit = 0;
            bool isNullable = false;

            if (Attribute.IsDefined(property, typeof(IgnoreInFlagsAttribute)))
            {
                continue;
            }
            if (Attribute.IsDefined(property, typeof(LimitAttribute)))
            {
                LimitAttribute limitAttribute = (LimitAttribute)property.GetCustomAttribute(typeof(LimitAttribute));
                limit = limitAttribute.Limit;
            }
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = propertyType.GetGenericArguments()[0];
                isNullable = true;
            }
            if (propertyType == typeof(bool))
            {
                if (isNullable)
                {
                    property.SetValue(this, flagReader.ReadNullableBool());
                }
                else
                {
                    property.SetValue(this, flagReader.ReadBool());
                }
                flags.ToString();
            }
            else if (propertyType.IsEnum)
            {
                limit = System.Enum.GetValues(propertyType).Length;
                //int? index = Array.IndexOf(Enum.GetValues(propertyType), property.GetValue(this, null));
                if (isNullable)
                {
                    MethodInfo method = typeof(FlagReader).GetMethod("ReadNullableEnum")
                        .MakeGenericMethod(new Type[] { propertyType });
                    var methodResult = method.Invoke(flagReader, new object[] { });
                    property.SetValue(this, methodResult);
                }
                else
                {
                    MethodInfo method = typeof(FlagReader).GetMethod("ReadEnum")
                        .MakeGenericMethod(new Type[] { propertyType });
                    var methodResult = method.Invoke(flagReader, new object[] { });
                    property.SetValue(this, methodResult);
                }
            }
            else if (IsIntegerType(propertyType))
            {
                if (Attribute.IsDefined(property, typeof(LimitAttribute)))
                {
                    int minimum = 0;
                    if (Attribute.IsDefined(property, typeof(MinimumAttribute)))
                    {
                        minimum = ((MinimumAttribute)property.GetCustomAttribute(typeof(MinimumAttribute))).Minimum;
                    }

                    if (isNullable)
                    {
                        int? value = flagReader.ReadNullableInt(limit);
                        if (value != null)
                        {
                            value = (int)value + minimum;
                        }
                        property.SetValue(this, value);
                    }
                    else
                    {
                        property.SetValue(this, flagReader.ReadInt(limit) + minimum);
                    }
                }
                else
                {
                    logger.Error("Numeric Property " + property.Name + " is missing a limit!");
                }
            }
            else
            {
                logger.Error("Unrecognized configuration property type.");
            }
            //Debug.WriteLine(property.Name + "\t" + flagReader.index);
        }
    }

    public static RandomizerConfiguration FromLegacyFlags(string flags)
    {
        //4.3 updated encoding table.
        flags = flags.Replace("$", "+");
        RandomizerConfiguration config = new RandomizerConfiguration();
        BitArray bits;
        int i = 0;

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));

        config.ShuffleStartingItems = bits[0];
        config.StartWithCandle = bits[1];
        config.StartWithGlove = bits[2];
        config.StartWithRaft = bits[3];
        config.StartWithBoots = bits[4];
        config.ShuffleOverworldEnemies = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.StartWithFlute = bits[0];
        config.StartWithCross = bits[1];
        config.StartWithHammer = bits[2];
        config.StartWithMagicKey = bits[3];
        config.ShuffleStartingSpells = bits[4];
        config.HideLessImportantLocations = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.StartWithShield = bits[0];
        config.StartWithJump = bits[1];
        config.StartWithLife = bits[2];
        config.StartWithFairy = bits[3];
        config.StartWithFire = bits[4];
        bool mergeFireWithRandomSpell = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.StartWithReflect = bits[0];
        config.StartWithSpell = bits[1];
        config.StartWithThunder = bits[2];
        config.ShuffleStartingLives = bits[3];
        config.RemoveTBird = bits[4];
        config.RestrictConnectionCaveShuffle = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        //For some reason the low 3 bits of the heart container start setting are stored on one byte and the 4th bit is disjointed on the next bite...
        BitArray nextBits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.StartingHeartContainersMin = ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0) + (bits[2] ? 4 : 0) + (nextBits[2] ? 8 : 0) + 1);
        if (config.StartingHeartContainersMin == 9)
        {
            config.StartingHeartContainersMin = null;
        }
        config.StartingHeartContainersMax = config.StartingHeartContainersMin;
        switch ((bits[3] ? 1 : 0) + (bits[4] ? 2 : 0) + (bits[5] ? 4 : 0))
        {
            case 0:
                config.StartWithDownstab = false;
                config.StartWithUpstab = false;
                break;
            case 1:
                config.StartWithDownstab = true;
                config.StartWithUpstab = false;
                break;
            case 2:
                config.StartWithDownstab = false;
                config.StartWithUpstab = true;
                break;
            case 3:
                config.StartWithDownstab = true;
                config.StartWithUpstab = true;
                break;
            case 4:
                config.StartWithDownstab = null;
                config.StartWithUpstab = null;
                break;
        }

        config.ShuffleItemDropFrequency = nextBits[0];
        config.IncludePBagCavesInItemShuffle = nextBits[1];
        config.ShuffleGP = nextBits[3];
        config.ChangePalacePallettes = nextBits[4];
        config.ShuffleEncounters = nextBits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.PalacesContainExtraKeys = bits[0];
        config.PalacesCanSwapContinents = bits[1];
        switch ((bits[2] ? 1 : 0) + (bits[3] ? 2 : 0) + (bits[4] ? 4 : 0))
        {
            case 0:
                config.AttackEffectiveness = StatEffectiveness.AVERAGE;
                break;
            case 1:
                config.AttackEffectiveness = StatEffectiveness.LOW;
                break;
            case 2:
                config.AttackEffectiveness = StatEffectiveness.VANILLA;
                break;
            case 3:
                config.AttackEffectiveness = StatEffectiveness.HIGH;
                break;
            case 4:
                config.AttackEffectiveness = StatEffectiveness.MAX;
                break;
            default:
                throw new Exception("Invalid AttackEffectiveness setting");
        }
        config.AllowUnsafePathEncounters = bits[5];
        config.IncludeLavaInEncounterShuffle = true;

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.PermanmentBeamSword = bits[0];
        config.ShuffleDripperEnemy = bits[1];
        bool replaceFireWithDash = bits[2];
        if (replaceFireWithDash)
        {
            config.FireOption = FireOption.REPLACE_WITH_DASH;
        }
        else if (mergeFireWithRandomSpell)
        {
            config.FireOption = FireOption.PAIR_WITH_RANDOM;
        }
        else
        {
            config.FireOption = FireOption.NORMAL;
        }
        config.ShuffleEnemyHP = bits[3];
        //ShuffleAllExp = bits[4];
        config.ShufflePalaceEnemies = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.ShuffleAttackExperience = bits[0];
        config.ShuffleLifeExperience = bits[1];
        config.ShuffleMagicExperience = bits[2];
        config.RestartAtPalacesOnGameOver = bits[3];
        config.ShortGP = bits[4];
        config.TBirdRequired = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        switch ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0) + (bits[2] ? 4 : 0))
        {
            case 0:
                config.MagicEffectiveness = StatEffectiveness.AVERAGE;
                break;
            case 1:
                config.MagicEffectiveness = StatEffectiveness.LOW;
                break;
            case 2:
                config.MagicEffectiveness = StatEffectiveness.VANILLA;
                break;
            case 3:
                config.MagicEffectiveness = StatEffectiveness.HIGH;
                break;
            case 4:
                config.MagicEffectiveness = StatEffectiveness.MAX;
                break;
        }
        config.ShuffleXPStealers = bits[3];
        config.ShuffleXPStolenAmount = bits[4];
        config.ShuffleLifeRefillAmount = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.ShuffleSwordImmunity = bits[0];
        config.JumpAlwaysOn = bits[1];
        string startGemsString = ((bits[2] ? 1 : 0) + (bits[3] ? 2 : 0) + (bits[4] ? 4 : 0)).ToString();
        if (startGemsString == "7")
        {
            config.PalacesToCompleteMin = 0;
            config.PalacesToCompleteMax = 6;
        }
        else
        {
            config.PalacesToCompleteMin = Int32.Parse(startGemsString);
            config.PalacesToCompleteMax = Int32.Parse(startGemsString);
        }
        config.MixLargeAndSmallEnemies = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.ShufflePalaceItems = bits[0];
        config.ShuffleOverworldItems = bits[1];
        config.MixOverworldAndPalaceItems = bits[2];
        config.ShuffleSmallItems = bits[3];
        config.ShuffleSpellLocations = bits[4];
        config.DisableMagicContainerRequirements = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        switch ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0) + (bits[2] ? 4 : 0))
        {
            case 0:
                config.LifeEffectiveness = StatEffectiveness.AVERAGE;
                break;
            case 1:
                config.LifeEffectiveness = StatEffectiveness.NONE;
                break;
            case 2:
                config.LifeEffectiveness = StatEffectiveness.VANILLA;
                break;
            case 3:
                config.LifeEffectiveness = StatEffectiveness.HIGH;
                break;
            case 4:
                config.LifeEffectiveness = StatEffectiveness.MAX;
                break;
        }
        config.RandomizeNewKasutoJarRequirements = bits[3];
        config.UseCommunityHints = bits[4];
        config.ShuffleSpritePalettes = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        string maxHeartsString = ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0) + (bits[2] ? 4 : 0) + (bits[3] ? 8 : 0) + 1).ToString();
        if (maxHeartsString == "9")
        {
            config.MaxHeartContainers = null;
        }
        else
        {
            config.MaxHeartContainers = Int32.Parse(maxHeartsString);
        }
        switch ((bits[4] ? 1 : 0) + (bits[5] ? 2 : 0))
        {
            case 0:
                config.HidePalace = false;
                break;
            case 1:
                config.HidePalace = true;
                break;
            case 2:
                config.HidePalace = null;
                break;
        }

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        switch ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0))
        {
            case 0:
                config.HideKasuto = false;
                break;
            case 1:
                config.HideKasuto = true;
                break;
            case 2:
                config.HideKasuto = null;
                break;
        }
        //ShuffleEnemyDrops = bits[2];
        config.RemoveSpellItems = bits[3];
        config.SmallEnemiesCanDropBlueJar = bits[4];
        config.SmallEnemiesCanDropRedJar = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.SmallEnemiesCanDropSmallBag = bits[0];
        config.SmallEnemiesCanDropMediumBag = bits[1];
        config.SmallEnemiesCanDropLargeBag = bits[2];
        config.SmallEnemiesCanDropXLBag = bits[3];
        config.SmallEnemiesCanDrop1up = bits[4];
        config.SmallEnemiesCanDropKey = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.LargeEnemiesCanDropBlueJar = bits[0];
        config.LargeEnemiesCanDropRedJar = bits[1];
        config.LargeEnemiesCanDropSmallBag = bits[2];
        config.LargeEnemiesCanDropMediumBag = bits[3];
        config.LargeEnemiesCanDropLargeBag = bits[4];
        config.LargeEnemiesCanDropXLBag = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.LargeEnemiesCanDrop1up = bits[0];
        config.LargeEnemiesCanDropKey = bits[1];
        config.EnableHelpfulHints = bits[2];
        config.EnableSpellItemHints = bits[3];
        config.StandardizeDrops = bits[4];
        config.RandomizeDrops = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        nextBits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.ShufflePBagAmounts = bits[0];
        config.AttackLevelCap = (8 - (bits[1] ? 1 : 0) + (bits[2] ? 2 : 0) + (bits[3] ? 4 : 0));
        config.MagicLevelCap = (8 - (bits[4] ? 1 : 0) + (bits[5] ? 2 : 0) + (nextBits[0] ? 4 : 0));
        config.LifeLevelCap = (8 - (nextBits[1] ? 1 : 0) + (nextBits[2] ? 2 : 0) + (nextBits[3] ? 4 : 0));
        config.ScaleLevelRequirementsToCap = nextBits[4];
        config.EnableTownNameHints = nextBits[5];


        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        switch ((bits[0] ? 1 : 0) + (bits[1] ? 2 : 0))
        {
            case 0:
                config.EncounterRate = EncounterRate.NORMAL;
                break;
            case 1:
                config.EncounterRate = EncounterRate.HALF;
                break;
            case 2:
                config.EncounterRate = EncounterRate.NONE;
                break;
        }
        switch ((bits[2] ? 1 : 0) + (bits[3] ? 2 : 0) + (bits[4] ? 4 : 0))
        {
            case 0:
                config.EnemyXPDrops = StatEffectiveness.VANILLA;
                break;
            case 1:
                config.EnemyXPDrops = StatEffectiveness.NONE;
                break;
            case 2:
                config.EnemyXPDrops = StatEffectiveness.LOW;
                break;
            case 3:
                config.EnemyXPDrops = StatEffectiveness.AVERAGE;
                break;
            case 4:
                config.EnemyXPDrops = StatEffectiveness.HIGH;
                break;
        }
        bool startAttackLowBit = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.StartingAttackLevel = (startAttackLowBit ? 1 : 0) + (bits[0] ? 2 : 0) + (bits[1] ? 4 : 0) + 1;
        config.StartingMagicLevel = (bits[2] ? 1 : 0) + (bits[3] ? 2 : 0) + (bits[4] ? 4 : 0) + 1;
        bool startLifeLowBit = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.StartingLifeLevel = (startLifeLowBit ? 1 : 0) + (bits[0] ? 2 : 0) + (bits[1] ? 4 : 0) + 1;
        switch ((bits[2] ? 1 : 0) + (bits[3] ? 2 : 0))
        {
            case 0:
                config.ContinentConnectionType = ContinentConnectionType.NORMAL;
                break;
            case 1:
                config.ContinentConnectionType = ContinentConnectionType.RB_BORDER_SHUFFLE;
                break;
            case 2:
                config.ContinentConnectionType = ContinentConnectionType.TRANSPORTATION_SHUFFLE;
                break;
            case 3:
                config.ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES;
                break;
        }
        config.AllowConnectionCavesToBeBoulderBlocked = bits[4];
        bool westBiomeLowBit = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        switch ((westBiomeLowBit ? 1 : 0) + (bits[0] ? 2 : 0) + (bits[1] ? 4 : 0) + (bits[2] ? 8 : 0))
        {
            case 0:
                config.WestBiome = Biome.VANILLA;
                break;
            case 1:
                config.WestBiome = Biome.VANILLA_SHUFFLE;
                break;
            case 2:
                config.WestBiome = Biome.VANILLALIKE;
                break;
            case 3:
                config.WestBiome = Biome.ISLANDS;
                break;
            case 4:
                config.WestBiome = Biome.CANYON;
                break;
            case 5:
                config.WestBiome = Biome.CALDERA;
                break;
            case 6:
                config.WestBiome = Biome.MOUNTAINOUS;
                break;
            case 7:
                config.WestBiome = Biome.RANDOM_NO_VANILLA;
                break;
            case 8:
                config.WestBiome = Biome.RANDOM;
                break;
        }
        int dmBiome = (bits[3] ? 1 : 0) + (bits[4] ? 2 : 0) + (bits[5] ? 4 : 0);

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        switch (dmBiome + (bits[0] ? 8 : 0))
        {
            case 0:
                config.DMBiome = Biome.VANILLA;
                break;
            case 1:
                config.DMBiome = Biome.VANILLA_SHUFFLE;
                break;
            case 2:
                config.DMBiome = Biome.VANILLALIKE;
                break;
            case 3:
                config.DMBiome = Biome.ISLANDS;
                break;
            case 4:
                config.DMBiome = Biome.CANYON;
                break;
            case 5:
                config.DMBiome = Biome.CALDERA;
                break;
            case 6:
                config.DMBiome = Biome.MOUNTAINOUS;
                break;
            case 7:
                config.DMBiome = Biome.RANDOM_NO_VANILLA;
                break;
            case 8:
                config.DMBiome = Biome.RANDOM;
                break;
        }

        switch ((bits[1] ? 1 : 0) + (bits[2] ? 2 : 0) + (bits[3] ? 4 : 0) + (bits[4] ? 8 : 0))
        {
            case 0:
                config.EastBiome = Biome.VANILLA;
                break;
            case 1:
                config.EastBiome = Biome.VANILLA_SHUFFLE;
                break;
            case 2:
                config.EastBiome = Biome.VANILLALIKE;
                break;
            case 3:
                config.EastBiome = Biome.ISLANDS;
                break;
            case 4:
                config.EastBiome = Biome.CANYON;
                break;
            case 5:
                config.EastBiome = Biome.VOLCANO;
                break;
            case 6:
                config.EastBiome = Biome.MOUNTAINOUS;
                break;
            case 7:
                config.EastBiome = Biome.RANDOM_NO_VANILLA;
                break;
            case 8:
                config.EastBiome = Biome.RANDOM;
                break;
        }
        bool mazeBiomeLowBit = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        switch ((mazeBiomeLowBit ? 1 : 0) + (bits[0] ? 2 : 0))
        {
            case 0:
                config.MazeBiome = Biome.VANILLA;
                break;
            case 1:
                config.MazeBiome = Biome.VANILLA_SHUFFLE;
                break;
            case 2:
                config.MazeBiome = Biome.VANILLALIKE;
                break;
            case 3:
                config.MazeBiome = Biome.RANDOM;
                break;
        }
        config.VanillaShuffleUsesActualTerrain = bits[1];
        config.ShuffleWhichLocationIsHidden = bits[2];
        config.RandomizeBossItemDrop = bits[3];
        config.GoodBoots = bits[4];
        config.RandomizeSpellSpellEnemy = bits[5];

        bits = new BitArray(BitConverter.GetBytes(BASE64_DECODE[flags[i++]]));
        config.GenerateBaguWoods = bits[0];
        switch ((bits[1] ? 1 : 0) + (bits[5] ? 2 : 0))
        {
            case 0:
                config.PalaceStyle = PalaceStyle.VANILLA;
                break;
            case 1:
                config.PalaceStyle = PalaceStyle.SHUFFLED;
                break;
            case 2:
                config.PalaceStyle = PalaceStyle.RECONSTRUCTED;
                break;
        }
        config.IncludeCommunityRooms = bits[2];
        config.BlockingRoomsInAnyPalace = bits[3];
        config.BossRoomsExitToPalace = bits[4];

        //These properties aren't stored in the flags, but aren't defaulted out in properties and will break if they are null.
        //Probably properties at some point should stop being a struct and default these in the right place
        config.Sprite = 0;
        config.Tunic = "Default";
        config.ShieldTunic = "Orange";
        config.BeamSprite = "Default";
        config.UseCustomRooms = false;

        config.BeepFrequency = 0x30;
        config.BeepThreshold = 0x20;
        config.DisableMusic = false;
        config.FastSpellCasting = true;
        //ShuffleEn = false;
        //upaBox = false;

        //Legacy UI tracked individual drops and randomize / manual separately, so the flags often have stray incorrect data in them
        //That wasn't actually used by the rando. This section is to convert those legacy flags to a modern interpretation
        if (config.RandomizeDrops)
        {
            config.SmallEnemiesCanDrop1up = false;
            config.SmallEnemiesCanDropBlueJar = false;
            config.SmallEnemiesCanDropKey = false;
            config.SmallEnemiesCanDropLargeBag = false;
            config.SmallEnemiesCanDropMediumBag = false;
            config.SmallEnemiesCanDropRedJar = false;
            config.SmallEnemiesCanDropSmallBag = false;
            config.SmallEnemiesCanDropXLBag = false;

            config.LargeEnemiesCanDrop1up = false;
            config.LargeEnemiesCanDropBlueJar = false;
            config.LargeEnemiesCanDropKey = false;
            config.LargeEnemiesCanDropLargeBag = false;
            config.LargeEnemiesCanDropMediumBag = false;
            config.LargeEnemiesCanDropRedJar = false;
            config.LargeEnemiesCanDropSmallBag = false;
            config.LargeEnemiesCanDropXLBag = false;
        }

        //New flags that didn't exist in 4.0.4
        config.SwapUpAndDownStab = false;


        return config;
    }

    public string Serialize()
    {
        FlagBuilder flags = new FlagBuilder();
        PropertyInfo[] properties = this.GetType().GetProperties();
        Type thisType = typeof(RandomizerConfiguration);
        foreach (PropertyInfo property in properties)
        {
            Type propertyType = property.PropertyType;
            int limit = 0;
            bool isNullable = false;

            if (Attribute.IsDefined(property, typeof(IgnoreInFlagsAttribute)))
            {
                continue;
            }
            if (Attribute.IsDefined(property, typeof(LimitAttribute)))
            {
                LimitAttribute limitAttribute = (LimitAttribute)property.GetCustomAttribute(typeof(LimitAttribute));
                limit = limitAttribute.Limit;
            }
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = propertyType.GetGenericArguments()[0];
                isNullable = true;
            }
            if (propertyType == typeof(bool))
            {
                if (isNullable)
                {
                    flags.Append((bool?)property.GetValue(this, null));
                }
                else
                {
                    flags.Append((bool)property.GetValue(this, null));
                }
            }
            else if (propertyType.IsEnum)
            {
                limit = System.Enum.GetValues(propertyType).Length;
                int? index = Array.IndexOf(Enum.GetValues(propertyType), property.GetValue(this, null));
                if (isNullable && index == null)
                {
                    flags.Append((int)index + 1, limit + 1);
                }
                else
                {
                    flags.Append((int)index, limit);
                }
            }
            else if (IsIntegerType(propertyType))
            {
                if (limit == 0)
                {
                    logger.Error("Numeric Property " + property.Name + " is missing a limit!");
                }
                int minimum = 0;
                if (Attribute.IsDefined(property, typeof(MinimumAttribute)))
                {
                    minimum = ((MinimumAttribute)property.GetCustomAttribute(typeof(MinimumAttribute))).Minimum;
                }
                if (isNullable)
                {
                    int? value = (int?)property.GetValue(this, null);
                    if (value != null && (value < minimum || value > minimum + limit))
                    {
                        logger.Warn("Property (" + property.Name + " was out of range.");
                        value = minimum;
                    }
                    flags.Append(value - minimum, limit);
                }
                else
                {
                    int value = (int)property.GetValue(this, null);
                    if (value < minimum || value > minimum + limit)
                    {
                        logger.Warn("Property (" + property.Name + " was out of range.");
                        value = minimum;
                    }
                    flags.Append(value - minimum, limit);
                }
            }
            else
            {
                logger.Error("Unrecognized configuration property type.");
            }
            //Debug.WriteLine(property.Name + "\t" + flags.bits.Count);
        }

        return flags.ToString();
    }
    public RandomizerProperties Export(Random random)
    {
        RandomizerProperties properties = new RandomizerProperties();

        properties.WestIsHorizontal = random.Next(2) == 1;
        properties.EastIsHorizontal = random.Next(2) == 1;
        properties.DmIsHorizontal = random.Next(2) == 1;

        //ROM Info
        properties.Filename = FileName;
        properties.saveRom = true;
        properties.Seed = Seed;

        //Items
        properties.StartCandle = !StartWithCandle && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithCandle;
        properties.StartGlove = !StartWithGlove && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithGlove;
        properties.StartRaft = !StartWithRaft && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithRaft;
        properties.StartBoots = !StartWithBoots && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithBoots;
        properties.StartFlute = !StartWithFlute && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithFlute;
        properties.StartCross = !StartWithCross && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithCross;
        properties.StartHammer = !StartWithHammer && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithHammer;
        properties.StartKey = !StartWithMagicKey && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithMagicKey;

        //Spells
        properties.StartShield = !StartWithShield && ShuffleStartingSpells ? random.NextDouble() > .75 : StartWithShield;
        properties.StartJump = !StartWithJump && ShuffleStartingSpells ? random.NextDouble() > .75 : StartWithJump;
        properties.StartLife = !StartWithLife && ShuffleStartingSpells ? random.NextDouble() > .75 : StartWithLife;
        properties.StartFairy = !StartWithFairy && ShuffleStartingSpells ? random.NextDouble() > .75 : StartWithFairy;
        properties.StartFire = !StartWithFire && ShuffleStartingSpells ? random.NextDouble() > .75 : StartWithFire;
        properties.StartReflect = !StartWithReflect && ShuffleStartingSpells ? random.NextDouble() > .75 : StartWithReflect;
        properties.StartSpell = !StartWithSpell && ShuffleStartingSpells ? random.NextDouble() > .75 : StartWithSpell;
        properties.StartThunder = !StartWithThunder && ShuffleStartingSpells ? random.NextDouble() > .75 : StartWithThunder;
        switch (FireOption)
        {
            case FireOption.NORMAL:
                properties.CombineFire = false;
                properties.ReplaceFireWithDash = false;
                break;
            case FireOption.PAIR_WITH_RANDOM:
                properties.CombineFire = true;
                properties.ReplaceFireWithDash = false;
                break;
            case FireOption.REPLACE_WITH_DASH:
                properties.CombineFire = false;
                properties.ReplaceFireWithDash = true;
                break;
            case FireOption.RANDOM:
                switch (random.Next(3))
                {
                    case 0:
                        properties.CombineFire = false;
                        properties.ReplaceFireWithDash = false;
                        break;
                    case 1:
                        properties.CombineFire = true;
                        properties.ReplaceFireWithDash = false;
                        break;
                    case 2:
                        properties.CombineFire = false;
                        properties.ReplaceFireWithDash = true;
                        break;

                }
                break;
        }

        //Other starting attributes
        int startHeartsMin, startHeartsMax;
        if (StartingHeartContainersMin == null)
        {
            startHeartsMin = random.Next(1, 9);
        }
        else
        {
            startHeartsMin = (int)StartingHeartContainersMin;
        }
        if (StartingHeartContainersMax == null)
        {
            startHeartsMax = random.Next(startHeartsMin, 9);
        }
        else
        {
            startHeartsMax = (int)StartingHeartContainersMax;
        }
        properties.StartHearts = random.Next(startHeartsMin, startHeartsMax + 1);

        //+1/+2/+3
        if (MaxHeartContainers == null)
        {
            properties.MaxHearts = random.Next(properties.StartHearts, 9);
        }
        else if (MaxHeartContainers > 8)
        {
            properties.MaxHearts = Math.Min(properties.StartHearts + (int)MaxHeartContainers - 8, 8);
        }
        else
        {
            properties.MaxHearts = (int)MaxHeartContainers;
        }
        properties.MaxHearts = Math.Max(properties.MaxHearts, properties.StartHearts);

        //If both stabs are random, use the classic weightings
        if (StartWithDownstab == null && StartWithUpstab == null)
        {
            switch (random.Next(7))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    properties.StartWithDownstab = false;
                    properties.StartWithUpstab = false;
                    break;
                case 4:
                    properties.StartWithDownstab = true;
                    properties.StartWithUpstab = false;
                    break;
                case 5:
                    properties.StartWithDownstab = false;
                    properties.StartWithUpstab = true;
                    break;
                case 6:
                    properties.StartWithDownstab = true;
                    properties.StartWithUpstab = true;
                    break;
            }
        }
        //Otherwise I guess we'll use an independent 2/7ths as a rough approximation
        else
        {
            properties.StartWithDownstab = StartWithDownstab == null ? random.Next(7) >= 5 : (bool)StartWithDownstab;
            properties.StartWithUpstab = StartWithUpstab == null ? random.Next(7) >= 5 : (bool)StartWithUpstab;
        }
        properties.SwapUpAndDownStab = SwapUpAndDownStab == null ? random.Next(2) == 1 : (bool)SwapUpAndDownStab;


        properties.StartLives = ShuffleStartingLives ? random.Next(2, 6) : 3;
        properties.PermanentBeam = PermanmentBeamSword;
        properties.UseCommunityHints = UseCommunityHints;
        properties.StartAtk = StartingAttackLevel;
        properties.StartMag = StartingMagicLevel;
        properties.StartLifeLvl = StartingMagicLevel;

        //Overworld
        properties.ShuffleEncounters = ShuffleEncounters == null ? random.Next(2) == 1 : (bool)ShuffleEncounters;
        properties.AllowPathEnemies = AllowUnsafePathEncounters;
        properties.IncludeLavaInEncounterShuffle = IncludeLavaInEncounterShuffle;
        properties.SwapPalaceCont = PalacesCanSwapContinents == null ? random.Next(2) == 1 : (bool)PalacesCanSwapContinents;
        properties.P7shuffle = ShuffleGP == null ? random.Next(2) == 1 : (bool)ShuffleGP;
        properties.HiddenPalace = HidePalace == null ? random.Next(2) == 1 : (bool)HidePalace;
        properties.HiddenKasuto = HideKasuto == null ? random.Next(2) == 1 : (bool)HideKasuto;

        properties.EncounterRate = EncounterRate;
        properties.ContinentConnections = ContinentConnectionType;
        properties.BoulderBlockConnections = AllowConnectionCavesToBeBoulderBlocked;
        if (WestBiome == Biome.RANDOM || WestBiome == Biome.RANDOM_NO_VANILLA)
        {
            properties.WestBiome = random.Next(WestBiome == Biome.RANDOM ? 7 : 5) switch
            {
                0 => Biome.VANILLALIKE,
                1 => Biome.ISLANDS,
                2 => random.Next(2) == 1 ? Biome.CANYON : Biome.DRY_CANYON,
                3 => Biome.CALDERA,
                4 => Biome.MOUNTAINOUS,
                5 => Biome.VANILLA,
                6 => Biome.VANILLA_SHUFFLE,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else
        {
            properties.WestBiome = WestBiome;
        }
        if (EastBiome == Biome.RANDOM || EastBiome == Biome.RANDOM_NO_VANILLA)
        {
            properties.EastBiome = random.Next(EastBiome == Biome.RANDOM ? 7 : 5) switch
            {
                0 => Biome.VANILLALIKE,
                1 => Biome.ISLANDS,
                2 => random.Next(2) == 1 ? Biome.DRY_CANYON : Biome.DRY_CANYON,
                3 => Biome.VOLCANO,
                4 => Biome.MOUNTAINOUS,
                5 => Biome.VANILLA,
                6 => Biome.VANILLA_SHUFFLE,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else
        {
            properties.EastBiome = EastBiome;
        }
        if (DMBiome == Biome.RANDOM || DMBiome == Biome.RANDOM_NO_VANILLA)
        {
            properties.DmBiome = random.Next(DMBiome == Biome.RANDOM ? 7 : 5) switch
            {
                0 => Biome.VANILLALIKE,
                1 => Biome.ISLANDS,
                2 => random.Next(2) == 1 ? Biome.CANYON : Biome.DRY_CANYON,
                3 => Biome.CALDERA,
                4 => Biome.MOUNTAINOUS,
                5 => Biome.VANILLA,
                6 => Biome.VANILLA_SHUFFLE,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else
        {
            properties.DmBiome = DMBiome;
        }
        if (MazeBiome == Biome.RANDOM)
        {
            properties.MazeBiome = random.Next(3) switch
            {
                0 => Biome.VANILLA,
                1 => Biome.VANILLA_SHUFFLE,
                2 => Biome.VANILLALIKE,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else
        {
            properties.MazeBiome = MazeBiome;
        }
        if(Climate == null)
        {
            properties.Climate = random.Next(1) switch
            {
                0 => Climates.Classic,
                _ => throw new Exception("Unrecognized climate")
            };
        }
        else
        {
            properties.Climate = Climate;
        }
        properties.VanillaShuffleUsesActualTerrain = VanillaShuffleUsesActualTerrain;
        properties.ShuffleHidden = ShuffleWhichLocationIsHidden == null ? random.Next(2) == 1 : (bool)ShuffleWhichLocationIsHidden;
        properties.CanWalkOnWaterWithBoots = GoodBoots == null ? random.Next(2) == 1 : (bool)GoodBoots;
        properties.BagusWoods = GenerateBaguWoods == null ? random.Next(2) == 1 : (bool)GenerateBaguWoods;

        //Palaces
        if (PalaceStyle == PalaceStyle.RANDOM)
        {
            properties.PalaceStyle = random.Next(3) switch
            {
                0 => PalaceStyle.VANILLA,
                1 => PalaceStyle.SHUFFLED,
                2 => PalaceStyle.RECONSTRUCTED,
                _ => throw new Exception("Invalid PalaceStyle")
            };
        }
        else
        {
            properties.PalaceStyle = PalaceStyle;
        }
        properties.StartGems = random.Next(PalacesToCompleteMin, PalacesToCompleteMax + 1);
        properties.RequireTbird = TBirdRequired == null ? random.Next(2) == 1 : (bool)TBirdRequired;
        properties.ShufflePalacePalettes = ChangePalacePallettes;
        properties.UpARestartsAtPalaces = RestartAtPalacesOnGameOver;
        properties.ShortenGP = ShortGP == null ? random.Next(2) == 1 : (bool)ShortGP;
        properties.RemoveTbird = RemoveTBird;
        properties.BossItem = RandomizeBossItemDrop;
        properties.UseCommunityRooms = IncludeCommunityRooms == null ? random.Next(2) == 1 : (bool)IncludeCommunityRooms;
        properties.BlockersAnywhere = BlockingRoomsInAnyPalace;
        properties.BossRoomConnect = BossRoomsExitToPalace == null ? random.Next(2) == 1 : (bool)BossRoomsExitToPalace;
        properties.NoDuplicateRooms = NoDuplicateRooms == null ? random.Next(2) == 1 : (bool)NoDuplicateRooms;
        properties.GeneratorsAlwaysMatch = GeneratorsAlwaysMatch;

        //Enemies
        properties.ShuffleEnemyHP = ShuffleEnemyHP;
        properties.ShuffleEnemyStealExp = ShuffleXPStealers;
        properties.ShuffleStealExpAmt = ShuffleXPStolenAmount;
        properties.ShuffleSwordImmunity = ShuffleSwordImmunity;
        properties.ShuffleOverworldEnemies = ShuffleOverworldEnemies == null ? random.Next(2) == 1 : (bool)ShuffleOverworldEnemies;
        properties.ShufflePalaceEnemies = ShufflePalaceEnemies == null ? random.Next(2) == 1 : (bool)ShufflePalaceEnemies;
        properties.MixLargeAndSmallEnemies = MixLargeAndSmallEnemies == null ? random.Next(2) == 1 : (bool)MixLargeAndSmallEnemies;
        properties.ShuffleDripper = ShuffleDripperEnemy;
        properties.ShuffleEnemyPalettes = ShuffleSpritePalettes;
        properties.ExpLevel = EnemyXPDrops;

        //Levels
        properties.ShuffleAtkExp = ShuffleAttackExperience;
        properties.ShuffleMagicExp = ShuffleMagicExperience;
        properties.ShuffleLifeExp = ShuffleLifeExperience;
        if (AttackEffectiveness == null)
        {
            properties.AttackEffectiveness = random.Next(4) switch
            {
                0 => StatEffectiveness.LOW,
                1 => StatEffectiveness.VANILLA,
                2 => StatEffectiveness.HIGH,
                3 => StatEffectiveness.MAX,
                _ => throw new Exception("Invalid attack effectiveness")
            };
        }
        else
        {
            properties.AttackEffectiveness = AttackEffectiveness;
        }
        if (MagicEffectiveness == null)
        {
            properties.MagicEffectiveness = random.Next(4) switch
            {
                0 => StatEffectiveness.LOW,
                1 => StatEffectiveness.VANILLA,
                2 => StatEffectiveness.HIGH,
                3 => StatEffectiveness.MAX,
                _ => throw new Exception("Invalid magic effectiveness")
            };
        }
        else
        {
            properties.MagicEffectiveness = MagicEffectiveness;
        }
        if (LifeEffectiveness == null)
        {
            properties.LifeEffectiveness = random.Next(4) switch
            {
                0 => StatEffectiveness.NONE,
                1 => StatEffectiveness.VANILLA,
                2 => StatEffectiveness.HIGH,
                3 => StatEffectiveness.MAX,
                _ => throw new Exception("Invalid life effectiveness")
            };
        }
        else
        {
            properties.LifeEffectiveness = LifeEffectiveness;
        }
        properties.ShuffleLifeRefill = ShuffleLifeRefillAmount;
        properties.ShuffleSpellLocations = ShuffleSpellLocations == null ? random.Next(2) == 1 : (bool)ShuffleSpellLocations;
        properties.DisableMagicRecs = DisableMagicContainerRequirements == null ? random.Next(2) == 1 : (bool)DisableMagicContainerRequirements;
        properties.AttackCap = AttackLevelCap;
        properties.MagicCap = MagicLevelCap;
        properties.LifeCap = LifeLevelCap;
        properties.ScaleLevels = ScaleLevelRequirementsToCap;
        properties.HideLessImportantLocations = HideLessImportantLocations == null ? random.Next(2) == 1 : (bool)HideLessImportantLocations;
        properties.SaneCaves = RestrictConnectionCaveShuffle == null ? random.Next(2) == 1 : (bool)RestrictConnectionCaveShuffle;
        properties.SpellEnemy = RandomizeSpellSpellEnemy == null ? random.Next(2) == 1 : (bool)RandomizeSpellSpellEnemy;

        //Items
        properties.ShuffleOverworldItems = ShuffleOverworldItems == null ? random.Next(2) == 1 : (bool)ShuffleOverworldItems;
        properties.ShufflePalaceItems = ShufflePalaceItems == null ? random.Next(2) == 1 : (bool)ShufflePalaceItems;
        properties.MixOverworldPalaceItems = MixOverworldAndPalaceItems == null ? random.Next(2) == 1 : (bool)MixOverworldAndPalaceItems;
        properties.ShuffleSmallItems = ShuffleSmallItems;
        properties.ExtraKeys = PalacesContainExtraKeys == null ? random.Next(2) == 1 : (bool)PalacesContainExtraKeys;
        properties.KasutoJars = RandomizeNewKasutoJarRequirements;
        properties.PbagItemShuffle = IncludePBagCavesInItemShuffle == null ? random.Next(2) == 1 : (bool)IncludePBagCavesInItemShuffle;
        properties.RemoveSpellItems = RemoveSpellItems == null ? random.Next(2) == 1 : (bool)RemoveSpellItems;
        properties.ShufflePbagXp = ShufflePBagAmounts == null ? random.Next(2) == 1 : (bool)ShufflePBagAmounts;

        //Drops
        properties.ShuffleItemDropFrequency = ShuffleItemDropFrequency;
        do
        {
            properties.Smallbluejar = !SmallEnemiesCanDropBlueJar && RandomizeDrops ? random.Next(2) == 1 : SmallEnemiesCanDropBlueJar;
            properties.Smallredjar = !SmallEnemiesCanDropRedJar && RandomizeDrops ? random.Next(2) == 1 : SmallEnemiesCanDropRedJar;
            properties.Small50 = !SmallEnemiesCanDropSmallBag && RandomizeDrops ? random.Next(2) == 1 : SmallEnemiesCanDropSmallBag;
            properties.Small100 = !SmallEnemiesCanDropMediumBag && RandomizeDrops ? random.Next(2) == 1 : SmallEnemiesCanDropMediumBag;
            properties.Small200 = !SmallEnemiesCanDropLargeBag && RandomizeDrops ? random.Next(2) == 1 : SmallEnemiesCanDropLargeBag;
            properties.Small500 = !SmallEnemiesCanDropXLBag && RandomizeDrops ? random.Next(2) == 1 : SmallEnemiesCanDropXLBag;
            properties.Small1up = !SmallEnemiesCanDrop1up && RandomizeDrops ? random.Next(2) == 1 : SmallEnemiesCanDrop1up;
            properties.Smallkey = !SmallEnemiesCanDropKey && RandomizeDrops ? random.Next(2) == 1 : SmallEnemiesCanDropKey;
        } while (!properties.Smallbluejar && !properties.Smallredjar && !properties.Small50 && !properties.Small100 &&
            !properties.Small200 && !properties.Small500 && !properties.Small1up && !properties.Smallkey);
        do
        {
            properties.Largebluejar = !LargeEnemiesCanDropBlueJar && RandomizeDrops ? random.Next(2) == 1 : LargeEnemiesCanDropBlueJar;
            properties.Largeredjar = !LargeEnemiesCanDropRedJar && RandomizeDrops ? random.Next(2) == 1 : LargeEnemiesCanDropRedJar;
            properties.Large50 = !LargeEnemiesCanDropSmallBag && RandomizeDrops ? random.Next(2) == 1 : LargeEnemiesCanDropSmallBag;
            properties.Large100 = !LargeEnemiesCanDropMediumBag && RandomizeDrops ? random.Next(2) == 1 : LargeEnemiesCanDropMediumBag;
            properties.Large200 = !LargeEnemiesCanDropLargeBag && RandomizeDrops ? random.Next(2) == 1 : LargeEnemiesCanDropLargeBag;
            properties.Large500 = !LargeEnemiesCanDropXLBag && RandomizeDrops ? random.Next(2) == 1 : LargeEnemiesCanDropXLBag;
            properties.Large1up = !LargeEnemiesCanDrop1up && RandomizeDrops ? random.Next(2) == 1 : LargeEnemiesCanDrop1up;
            properties.Largekey = !LargeEnemiesCanDropKey && RandomizeDrops ? random.Next(2) == 1 : LargeEnemiesCanDropKey;
        } while (!properties.Largebluejar && !properties.Largeredjar && !properties.Large50 && !properties.Large100 &&
            !properties.Large200 && !properties.Large500 && !properties.Large1up && !properties.Largekey);
        properties.StandardizeDrops = StandardizeDrops;

        //Hints
        properties.SpellItemHints = EnableSpellItemHints == null ? random.Next(2) == 1 : (bool)EnableSpellItemHints;
        properties.HelpfulHints = EnableHelpfulHints == null ? random.Next(2) == 1 : (bool)EnableHelpfulHints;
        properties.TownNameHints = EnableTownNameHints == null ? random.Next(2) == 1 : (bool)EnableTownNameHints;

        //Misc.
        properties.BeepThreshold = BeepThreshold;
        properties.BeepFrequency = BeepFrequency;
        properties.JumpAlwaysOn = JumpAlwaysOn;
        properties.DashAlwaysOn = DashAlwaysOn;
        properties.FastCast = FastSpellCasting;
        properties.BeamSprite = BeamSprite;
        properties.DisableMusic = DisableMusic;
        properties.CharSprite = CharacterSprite.ByIndex(Sprite);
        properties.TunicColor = Tunic;
        properties.ShieldColor = ShieldTunic;
        properties.UpAC1 = UpAOnController1;
        properties.RemoveFlashing = RemoveFlashing;
        properties.UseCustomRooms = UseCustomRooms;

        //"Server" side validation
        //This is a replication of a bunch of logic from the UI so that configurations from sources other than the UI (YAML)
        //or indeterminately generated properties still correspond to sanity. Without this you get randomly ungeneratable seeds.
        if (!properties.ShuffleEncounters)
        {
            properties.AllowPathEnemies = false;
            properties.IncludeLavaInEncounterShuffle = false;
        }

        if (!properties.ShuffleOverworldEnemies && !properties.ShufflePalaceEnemies)
        {
            properties.MixLargeAndSmallEnemies = false;
        }

        if (!properties.ShufflePalaceItems || !properties.ShuffleOverworldItems)
        {
            properties.MixOverworldPalaceItems = false;
        }

        if (!properties.ShuffleOverworldItems)
        {
            properties.PbagItemShuffle = false;
        }

        if (properties.RequireTbird)
        {
            properties.RemoveTbird = false;
        }

        if (!properties.SwapPalaceCont)
        {
            properties.P7shuffle = false;
        }

        if (properties.RemoveSpellItems)
        {
            properties.SpellItemHints = false;
        }

        //if (eastBiome.SelectedIndex == 0 || (hiddenPalaceList.SelectedIndex == 0 && hideKasutoList.SelectedIndex == 0))
        if (properties.EastBiome == Biome.VANILLA || (!properties.HiddenPalace && !properties.HiddenKasuto))
        {
            properties.ShuffleHidden = false;
        }

        if (properties.WestBiome == Biome.VANILLA || properties.WestBiome == Biome.VANILLA_SHUFFLE)
        {
            properties.BagusWoods = false;
        }

        if (properties.PalaceStyle == PalaceStyle.VANILLA || properties.PalaceStyle == PalaceStyle.SHUFFLED)
        {
            properties.UseCommunityRooms = false;
            properties.BlockersAnywhere = false;
            properties.BossRoomConnect = false;
        }

        if (properties.PalaceStyle == PalaceStyle.VANILLA)
        {
            properties.ShortenGP = false;
            properties.RequireTbird = true;
        }

        if (properties.ReplaceFireWithDash)
        {
            properties.CombineFire = false;
        }

        //Non-reconstructed is incompatable with no duplicate rooms.
        //Also, if community rooms is off, vanilla doesn't contain enough non-duplciate rooms to properly cover the number
        //of required rooms, often even in short GP.
        if(properties.PalaceStyle != PalaceStyle.RECONSTRUCTED || !properties.UseCommunityRooms)
        {
            properties.NoDuplicateRooms = false;
        }

        string debug = JsonSerializer.Serialize(properties);
        return properties;
    }

    public static bool IsIntegerType(Type type)
    {
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                return true;
            default:
                return false;
        }
    }

    public string GetRoomsFile()
    {
        return UseCustomRooms ? "CustomRooms.json" : "PalaceRooms.json";
    }

}
