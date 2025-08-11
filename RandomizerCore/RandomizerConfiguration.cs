using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Z2Randomizer.RandomizerCore.Flags;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore;

public sealed partial class RandomizerConfiguration : ReactiveObject
{
    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly static Collectable[] POSSIBLE_STARTING_ITEMS = [
        Collectable.CANDLE,
        Collectable.GLOVE,
        Collectable.RAFT,
        Collectable.BOOTS,
        Collectable.FLUTE,
        Collectable.CROSS,
        Collectable.HAMMER,
        Collectable.MAGIC_KEY
    ];

    private readonly static Collectable[] POSSIBLE_STARTING_SPELLS = [
        Collectable.SHIELD_SPELL,
        Collectable.JUMP_SPELL,
        Collectable.LIFE_SPELL,
        Collectable.FAIRY_SPELL,
        Collectable.FIRE_SPELL,
        Collectable.REFLECT_SPELL,
        Collectable.SPELL_SPELL,
        Collectable.THUNDER_SPELL
    ];


    //Start Configuration
    [Reactive]
    private bool shuffleStartingItems;

    [Reactive]
    private bool startWithCandle;

    [Reactive]
    private bool startWithGlove;

    [Reactive]
    private bool startWithRaft;

    [Reactive]
    private bool startWithBoots;

    [Reactive]
    private bool startWithFlute;

    [Reactive]
    private bool startWithCross;

    [Reactive]
    private bool startWithHammer;

    [Reactive]
    private bool startWithMagicKey;

    [Reactive]
    private bool shuffleStartingSpells;

    [Reactive]
    private StartingResourceLimit startItemsLimit;

    [Reactive]
    private bool startWithShield;

    [Reactive]
    private bool startWithJump;

    [Reactive]
    private bool startWithLife;

    [Reactive]
    private bool startWithFairy;

    [Reactive]
    private bool startWithFire;

    [Reactive]
    private bool startWithReflect;

    [Reactive]
    private bool startWithSpellSpell;

    [Reactive]
    private bool startWithThunder;

    [Reactive]
    private StartingResourceLimit startSpellsLimit;

    [Reactive]
    [property:Limit(8)]
    [property:Minimum(1)]
    private int? startingHeartContainersMin;

    [Reactive]
    [property:Limit(8)]
    [property:Minimum(1)]
    private int? startingHeartContainersMax;

    [Reactive]
    private MaxHeartsOption maxHeartContainers;

    [Reactive]
    private StartingTechs startingTechniques;

    [Reactive]
    private StartingLives startingLives;

    [Reactive]
    [property:Limit(8)]
    [property:Minimum(1)]
    private int startingAttackLevel;

    [Reactive]
    [property:Limit(8)]
    [property:Minimum(1)]
    private int startingMagicLevel;

    [Reactive]
    [property:Limit(8)]
    [property:Minimum(1)]
    private int startingLifeLevel;

    [Reactive]
    private IndeterminateOptionRate indeterminateOptionRate;

    //Overworld
    [Reactive]
    private bool? palacesCanSwapContinents;

    [Reactive]
    private bool? shuffleGP;

    [Reactive]
    private bool? shuffleEncounters;

    [Reactive]
    private bool allowUnsafePathEncounters;

    [Reactive]
    private bool includeLavaInEncounterShuffle;

    [Reactive]
    private EncounterRate encounterRate;

    [Reactive]
    private bool? hidePalace;

    [Reactive]
    private bool? hideKasuto;

    [Reactive]
    private bool? shuffleWhichLocationIsHidden;

    [Reactive]
    private bool? hideLessImportantLocations;

    [Reactive]
    private bool? restrictConnectionCaveShuffle;

    [Reactive]
    private bool allowConnectionCavesToBeBlocked;

    [Reactive]
    private bool? goodBoots;

    [Reactive]
    private bool? generateBaguWoods;

    [Reactive]
    private ContinentConnectionType continentConnectionType;

    [Reactive]
    private Biome westBiome;

    [Reactive]
    private Biome eastBiome;

    [Reactive]
    private Biome dMBiome;

    [Reactive]
    private Biome mazeBiome;

    [Reactive]
    [property:CustomFlagSerializer(typeof(ClimateFlagSerializer))]
    private Climate climate;

    [Reactive]
    private bool vanillaShuffleUsesActualTerrain;

    //Palaces
    [Reactive]
    private PalaceStyle normalPalaceStyle;

    [Reactive]
    private PalaceStyle gPStyle;

    [Reactive]
    private bool? includeVanillaRooms;

    [Reactive]
    private bool? includev4_0Rooms;

    [Reactive]
    private bool? includev4_4Rooms;

    [Reactive]
    private bool blockingRoomsInAnyPalace;

    [Reactive]
    private BossRoomsExitType bossRoomsExitType;

    [Reactive]
    private bool? tBirdRequired;

    [Reactive]
    private bool removeTBird;

    [Reactive]
    private bool restartAtPalacesOnGameOver;

    [Reactive]
    private bool? global5050JarDrop = false;

    [Reactive]
    private bool reduceDripperVariance = false;

    [Reactive]
    private bool changePalacePallettes;

    [Reactive]
    private bool randomizeBossItemDrop;

    [Reactive]
    private BossRoomMinDistance darkLinkMinDistance;

    [Reactive]
    private PalaceItemRoomCount palaceItemRoomCount;

    [Reactive]
    [property:Limit(7)]
    private int palacesToCompleteMin;

    [Reactive]
    [property:Limit(7)]
    private int palacesToCompleteMax;

    [Reactive]
    private bool noDuplicateRoomsByLayout;

    [Reactive]
    private bool noDuplicateRoomsByEnemies;

    [Reactive]
    private bool generatorsAlwaysMatch;

    [Reactive]
    private bool hardBosses;

    //Levels
    [Reactive]
    private bool shuffleAttackExperience;

    [Reactive]
    private bool shuffleMagicExperience;

    [Reactive]
    private bool shuffleLifeExperience;

    [Reactive]
    [property:Limit(8)]
    [property:Minimum(1)]
    private int attackLevelCap;

    [Reactive]
    [property:Limit(8)]
    [property:Minimum(1)]
    private int magicLevelCap;

    [Reactive]
    [property:Limit(8)]
    [property:Minimum(1)]
    private int lifeLevelCap;

    [Reactive]
    private bool scaleLevelRequirementsToCap;

    [Reactive]
    private AttackEffectiveness attackEffectiveness;

    [Reactive]
    private MagicEffectiveness magicEffectiveness;

    [Reactive]
    private LifeEffectiveness lifeEffectiveness;

    //Spells
    [Reactive]
    private bool shuffleLifeRefillAmount;

    [Reactive]
    private bool? shuffleSpellLocations;

    [Reactive]
    private bool? disableMagicContainerRequirements;

    [Reactive]
    private bool? randomizeSpellSpellEnemy;

    [Reactive]
    private bool? swapUpAndDownStab;

    [Reactive]
    private FireOption fireOption;

    //Enemies
    [Reactive]
    private bool? shuffleOverworldEnemies;

    [Reactive]
    private bool? shufflePalaceEnemies;

    [Reactive]
    private bool shuffleDripperEnemy;

    [Reactive]
    private bool? mixLargeAndSmallEnemies;

    [Reactive]
    private bool shuffleEnemyHP;

    [Reactive]
    private bool shuffleXPStealers;

    [Reactive]
    private bool shuffleXPStolenAmount;

    [Reactive]
    private bool shuffleSwordImmunity;

    [Reactive]
    private XPEffectiveness enemyXPDrops;

    //Items
    [Reactive]
    private bool? shufflePalaceItems;

    [Reactive]
    private bool? shuffleOverworldItems;

    [Reactive]
    private bool? mixOverworldAndPalaceItems;

    [Reactive]
    private bool? includePBagCavesInItemShuffle;

    [Reactive]
    private bool shuffleSmallItems;

    [Reactive]
    private bool? palacesContainExtraKeys;

    [Reactive]
    private bool randomizeNewKasutoJarRequirements;

    [Reactive]
    private bool allowImportantItemDuplicates;

    [Reactive]
    private bool? removeSpellItems;

    [Reactive]
    private bool? shufflePBagAmounts;

    [Reactive]
    private bool? includeSpellsInShuffle;

    [Reactive]
    private bool? includeSwordTechsInShuffle;

    [Reactive]
    private bool? includeQuestItemsInShuffle;

    //Drops
    [Reactive]
    private bool shuffleItemDropFrequency;

    [Reactive]
    private bool randomizeDrops;

    [Reactive]
    private bool standardizeDrops;

    [Reactive]
    private bool smallEnemiesCanDropBlueJar;

    [Reactive]
    private bool smallEnemiesCanDropRedJar;

    [Reactive]
    private bool smallEnemiesCanDropSmallBag;

    [Reactive]
    private bool smallEnemiesCanDropMediumBag;

    [Reactive]
    private bool smallEnemiesCanDropLargeBag;

    [Reactive]
    private bool smallEnemiesCanDropXLBag;

    [Reactive]
    private bool smallEnemiesCanDrop1up;

    [Reactive]
    private bool smallEnemiesCanDropKey;

    [Reactive]
    private bool largeEnemiesCanDropBlueJar;

    [Reactive]
    private bool largeEnemiesCanDropRedJar;

    [Reactive]
    private bool largeEnemiesCanDropSmallBag;

    [Reactive]
    private bool largeEnemiesCanDropMediumBag;

    [Reactive]
    private bool largeEnemiesCanDropLargeBag;

    [Reactive]
    private bool largeEnemiesCanDropXLBag;

    [Reactive]
    private bool largeEnemiesCanDrop1up;

    [Reactive]
    private bool largeEnemiesCanDropKey;

    //Misc
    [Reactive]
    private bool? enableHelpfulHints;

    [Reactive]
    private bool? enableSpellItemHints;

    [Reactive]
    private bool? enableTownNameHints;

    [Reactive]
    private bool jumpAlwaysOn;

    [Reactive]
    private bool dashAlwaysOn;

    [Reactive]
    private bool shuffleSpritePalettes;

    [Reactive]
    private bool permanentBeamSword;

    //Custom
    [Reactive]
    [property:IgnoreInFlags]
    private bool useCommunityText;

    [Reactive]
    [property:IgnoreInFlags]
    private BeepFrequency beepFrequency;

    [Reactive]
    [property:IgnoreInFlags]
    private BeepThreshold beepThreshold;

    [Reactive]
    [property:IgnoreInFlags]
    private bool disableMusic;

    [Reactive]
    [property:IgnoreInFlags]
    private bool randomizeMusic;

    [Reactive]
    [property:IgnoreInFlags]
    private bool mixCustomAndOriginalMusic;

    [Reactive]
    [property:IgnoreInFlags]
    private bool disableUnsafeMusic;

    [Reactive]
    [property:IgnoreInFlags]
    private bool fastSpellCasting;

    [Reactive]
    [property:IgnoreInFlags]
    private bool upAOnController1;

    [Reactive]
    [property:IgnoreInFlags]
    private bool removeFlashing;

    [Reactive]
    [property: IgnoreInFlags]
    private CharacterSprite sprite;

    [Reactive]
    [property:IgnoreInFlags]
    private string spriteName;


    [Reactive]
    [property:IgnoreInFlags]
    private bool changeItemSprites;

    [Reactive]
    [property:IgnoreInFlags]
    private CharacterColor tunic;

    [Reactive]
    [property:IgnoreInFlags]
    private CharacterColor tunicOutline;

    [Reactive]
    [property:IgnoreInFlags]
    private CharacterColor shieldTunic;

    [Reactive]
    [property:IgnoreInFlags]
    private BeamSprites beamSprite;

    [Reactive]
    [property:IgnoreInFlags]
    private bool useCustomRooms;

    [Reactive]
    [property:IgnoreInFlags]
    private bool disableHUDLag;

    [Reactive]
    private bool randomizeKnockback;

    [Reactive]
    private bool? shortenGP;

    [Reactive]
    private bool? shortenNormalPalaces;


    [Reactive]
    private RiverDevilBlockerOption riverDevilBlockerOption;

    [Reactive]
    private bool? eastRocks;

    [Reactive]
    private bool generateSpoiler;

    //Meta
    [Reactive]
    [property:Required]
    [property:IgnoreInFlags]
    private string? seed;
    // public string Seed { get => seed ?? ""; set => SetField(ref seed, value); }

    [IgnoreInFlags]
    [JsonIgnore]
    public string Flags
    {
        get => Serialize();
        set => ConvertFlags(value?.Trim() ?? "", this);
    }

    public RandomizerConfiguration()
    {
        StartingAttackLevel = 1;
        StartingMagicLevel = 1;
        StartingLifeLevel = 1;

        MaxHeartContainers = MaxHeartsOption.EIGHT;
        StartingHeartContainersMin = 8;
        StartingHeartContainersMax = 8;

        AttackLevelCap = 8;
        MagicLevelCap = 8;
        LifeLevelCap = 8;

        DisableMusic = false;
        RandomizeMusic = false;
        MixCustomAndOriginalMusic = true;
        DisableUnsafeMusic = true;
        FastSpellCasting = false;
        ShuffleSpritePalettes = false;
        PermanentBeamSword = false;
        UpAOnController1 = false;
        RemoveFlashing = false;
        Sprite = CharacterSprite.LINK;
        Climate = Climates.Classic;
        if (Sprite == null || Climate == null)
        {
            throw new ImpossibleException();
        }
        Tunic = CharacterColor.Default;
        TunicOutline = CharacterColor.Default;
        ShieldTunic = CharacterColor.Default;
        BeamSprite = BeamSprites.DEFAULT;
        UseCustomRooms = false;
        DisableHUDLag = false;
        this.WhenAnyPropertyChanged()
            .Throttle(TimeSpan.FromMilliseconds(10))
            .Select(_ => Serialize())
            .ToProperty(this, nameof(Flags));
    }

    public RandomizerConfiguration(string flagstring) : this()
    {
        ConvertFlags(flagstring, this);
    }
    
    [method: DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(RandomizerConfiguration))]
    private void ConvertFlags(string flagstring, RandomizerConfiguration? newThis = null)
    {
        //seed - climate - sprite
        var config = newThis ?? new RandomizerConfiguration();
        FlagReader flagReader = new FlagReader(flagstring);
        PropertyInfo[] properties = GetType().GetProperties();
        Type thisType = typeof(RandomizerConfiguration);
        foreach (PropertyInfo property in properties)
        {
            Type propertyType = property.PropertyType;
            int limit;
            bool isNullable = false;

            if (Attribute.IsDefined(property, typeof(IgnoreInFlagsAttribute)))
            {
                continue;
            }
            //Now that the config is a ReactiveObject, some properties get inherited that should be ignored.
            if (property.DeclaringType!.FullName == "ReactiveUI.ReactiveObject")
            {
                continue;
            }
            LimitAttribute? limitAttribute = (LimitAttribute?)property.GetCustomAttribute(typeof(LimitAttribute));
            limit = limitAttribute?.Limit ?? 0;

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = propertyType.GetGenericArguments()[0];
                isNullable = true;
            }
//The analyzer simultaneously complains about this warning that doesn't matter,
//and then complains about the warning suppression being unnecessary once it's suppressed.
#pragma warning disable IL2072
            CustomFlagSerializerAttribute? attribute = 
                (CustomFlagSerializerAttribute?)property.GetCustomAttribute(typeof(CustomFlagSerializerAttribute));
            if (attribute != null)
            {
                IFlagSerializer? serializer = (IFlagSerializer?)Activator.CreateInstance(attribute.Type);
                property.SetValue(config, serializer?.Deserialize(flagReader.ReadInt(serializer.GetLimit())));
            }
#pragma warning restore IL2072 
            else if (propertyType == typeof(bool))
            {
                property.SetValue(config, isNullable ? flagReader.ReadNullableBool() : flagReader.ReadBool());
            }
            else if (propertyType.IsEnum)
            {
                var methodType = isNullable ? "ReadNullableEnum" : "ReadEnum";
                MethodInfo method = typeof(FlagReader).GetMethod(methodType)!
                    .MakeGenericMethod([propertyType]);
                var methodResult = method.Invoke(flagReader, []);
                property.SetValue(config, methodResult);
            }
            else if (IsIntegerType(propertyType))
            {
                if (Attribute.IsDefined(property, typeof(LimitAttribute)))
                {
                    int minimum = ((MinimumAttribute?)property.GetCustomAttribute(typeof(MinimumAttribute)))?.Minimum ?? 0;

                    if (isNullable)
                    {
                        int? value = flagReader.ReadNullableInt(limit);
                        value += minimum;
                        property.SetValue(config, value);
                    }
                    else
                    {
                        property.SetValue(config, flagReader.ReadInt(limit) + minimum);
                    }
                }
                else
                {
                    logger.Error("Numeric Property " + property.Name + " is missing a limit!");
                }
            }
            else
            {
                logger.Error($"Unrecognized configuration property type: {propertyType}");
            }
            //Debug.WriteLine(property.Name + "\t" + flagReader.index);
        }
    }

    public string Serialize()
    {
        FlagBuilder flags = new FlagBuilder();
        PropertyInfo[] properties = this.GetType().GetProperties();
        Type thisType = typeof(RandomizerConfiguration);
        foreach (PropertyInfo property in properties)
        {
            Type propertyType = property.PropertyType;
            bool isNullable = false;

            if (Attribute.IsDefined(property, typeof(IgnoreInFlagsAttribute)))
            {
                continue;
            }
            //Now that the config is a ReactiveObject, some properties get inherited that should be ignored.
            if(property.DeclaringType!.FullName == "ReactiveUI.ReactiveObject")
            {
                continue;
            }
            LimitAttribute? limitAttribute = (LimitAttribute?)property.GetCustomAttribute(typeof(LimitAttribute));
            int limit = limitAttribute?.Limit ?? 0;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = propertyType.GetGenericArguments()[0];
                isNullable = true;
            }
#pragma warning disable IL2072
            CustomFlagSerializerAttribute? attribute = 
                (CustomFlagSerializerAttribute?)property.GetCustomAttribute(typeof(CustomFlagSerializerAttribute));
            if(attribute != null)
            {
                IFlagSerializer serializer = (IFlagSerializer)Activator.CreateInstance(attribute.Type)!;
                if(serializer?.GetLimit() == null)
                {
                    throw new Exception("Missing limit on serializer");
                }
                flags.Append(serializer?.Serialize(property.GetValue(this, null)), serializer!.GetLimit());
            }
#pragma warning restore IL2072
            else if (propertyType == typeof(bool))
            {
                if (isNullable)
                {
                    flags.Append((bool?)property.GetValue(this, null));
                }
                else
                {
                    flags.Append((bool)property.GetValue(this, null)!);
                }
            }
            else if (propertyType.IsEnum)
            {
                limit = Enum.GetValues(propertyType).Length;
                int index = Array.IndexOf(Enum.GetValues(propertyType), property.GetValue(this, null));
                if (isNullable)
                {
                    flags.Append(index == -1 ? null : index, limit + 1);
                }
                else
                {
                    flags.Append(index, limit);
                }
            }
            else if (IsIntegerType(propertyType))
            {
                if (limit == 0)
                {
                    logger.Error("Numeric Property " + property.Name + " is missing a limit!");
                }
                int minimum = ((MinimumAttribute?)property.GetCustomAttribute(typeof(MinimumAttribute)))?.Minimum ?? 0;
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
                    int value = (int)property.GetValue(this, null)!;
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
                logger.Error($"Unrecognized configuration property type: {property.Name}");
            }
            //Debug.WriteLine(property.Name + "\t" + flags.bits.Count);
        }

        return flags.ToString();
    }
    public RandomizerProperties Export(Random r)
    {
        RandomizerProperties properties = new()
        {
            Flags = Flags,

            WestIsHorizontal = r.Next(2) == 1,
            EastIsHorizontal = r.Next(2) == 1,
            DmIsHorizontal = r.Next(2) == 1,
            EastRockIsPath = r.Next(2) == 1,

            //ROM Info
            Seed = Seed
        };

        //Properties that can affect available minor item replacements
        do // while (!properties.HasEnoughSpaceToAllocateItems())
        {
            //Start Configuration
            ShuffleStartingCollectables(POSSIBLE_STARTING_ITEMS, StartItemsLimit, ShuffleStartingItems, properties, r);
            ShuffleStartingCollectables(POSSIBLE_STARTING_SPELLS, StartSpellsLimit, ShuffleStartingSpells, properties, r);

            if (GPStyle == PalaceStyle.RANDOM)
            {
                properties.PalaceStyles[6] = r.Next(5) switch
                {
                    0 => PalaceStyle.VANILLA,
                    1 => PalaceStyle.SHUFFLED,
                    2 => PalaceStyle.RECONSTRUCTED,
                    3 => PalaceStyle.SEQUENTIAL,
                    4 => PalaceStyle.RANDOM_WALK,
                    _ => throw new Exception("Invalid PalaceStyle")
                };
            }
            else if (GPStyle == PalaceStyle.RANDOM_NO_VANILLA_OR_SHUFFLE)
            {
                properties.PalaceStyles[6] = r.Next(3) switch
                {
                    0 => PalaceStyle.RECONSTRUCTED,
                    1 => PalaceStyle.SEQUENTIAL,
                    2 => PalaceStyle.RANDOM_WALK,
                    _ => throw new Exception("Invalid PalaceStyle")
                };
            }
            else
            {
                properties.PalaceStyles[6] = GPStyle;
            }

            if (NormalPalaceStyle == PalaceStyle.RANDOM_ALL)
            {
                PalaceStyle style = r.Next(5) switch
                {
                    0 => PalaceStyle.VANILLA,
                    1 => PalaceStyle.SHUFFLED,
                    2 => PalaceStyle.RECONSTRUCTED,
                    3 => PalaceStyle.SEQUENTIAL,
                    4 => PalaceStyle.RANDOM_WALK,
                    _ => throw new Exception("Invalid PalaceStyle")
                };
                for (int i = 0; i < 6; i++)
                {
                    properties.PalaceStyles[i] = style;
                }
            }
            else if (NormalPalaceStyle == PalaceStyle.RANDOM_PER_PALACE)
            {
                for (int i = 0; i < 6; i++)
                {
                    PalaceStyle style = r.Next(5) switch
                    {
                        0 => PalaceStyle.VANILLA,
                        1 => PalaceStyle.SHUFFLED,
                        2 => PalaceStyle.RECONSTRUCTED,
                        3 => PalaceStyle.SEQUENTIAL,
                        4 => PalaceStyle.RANDOM_WALK,
                        _ => throw new Exception("Invalid PalaceStyle")
                    };
                    properties.PalaceStyles[i] = style;
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    properties.PalaceStyles[i] = NormalPalaceStyle;
                }
            }

            properties.ShortenGP = ShortenGP ?? GetIndeterminateFlagValue(r);
            properties.ShortenNormalPalaces = ShortenNormalPalaces ?? GetIndeterminateFlagValue(r);
            properties.DarkLinkMinDistance = GetDarkLinkMinDistance();

            //Other starting attributes
            int startHeartsMin, startHeartsMax;
            if (StartingHeartContainersMin == null)
            {
                startHeartsMin = r.Next(1, 9);
            }
            else
            {
                startHeartsMin = (int)StartingHeartContainersMin;
            }
            if (StartingHeartContainersMax == null)
            {
                startHeartsMax = r.Next(startHeartsMin, 9);
            }
            else
            {
                startHeartsMax = (int)StartingHeartContainersMax;
            }
            properties.StartHearts = r.Next(startHeartsMin, startHeartsMax + 1);

            //+1/+2/+3
            if (MaxHeartContainers == MaxHeartsOption.RANDOM)
            {
                properties.MaxHearts = r.Next(properties.StartHearts, 9);
            }
            else if ((int)MaxHeartContainers <= 8)
            {
                properties.MaxHearts = (int)MaxHeartContainers;
            }
            else
            {
                int additionalHearts = MaxHeartContainers switch
                {
                    MaxHeartsOption.PLUS_ONE => 1,
                    MaxHeartsOption.PLUS_TWO => 2,
                    MaxHeartsOption.PLUS_THREE => 3,
                    MaxHeartsOption.PLUS_FOUR => 4,
                    _ => throw new ImpossibleException("Invalid heart container max configuration")
                };
                properties.MaxHearts = Math.Min(properties.StartHearts + additionalHearts, 8);
            }
            properties.MaxHearts = Math.Max(properties.MaxHearts, properties.StartHearts);
        } while (!properties.HasEnoughSpaceToAllocateItems());

        //Handle Fire
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
                switch (r.Next(3))
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

        //If both stabs are random, use the classic weightings
        if (StartingTechniques == StartingTechs.RANDOM)
        {
            switch (r.Next(7))
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
        else
        {
            properties.StartWithDownstab = StartingTechniques.StartWithDownstab();
            properties.StartWithUpstab = StartingTechniques.StartWithUpstab();
        }
        properties.SwapUpAndDownStab = SwapUpAndDownStab ?? GetIndeterminateFlagValue(r);


        properties.StartLives = StartingLives switch
        {
            StartingLives.Lives1 => 1,
            StartingLives.Lives2 => 2,
            StartingLives.Lives3 => 3,
            StartingLives.Lives4 => 4,
            StartingLives.Lives5 => 5,
            StartingLives.Lives8 => 8,
            StartingLives.Lives16 => 16,
            _ => r.Next(2, 6)
        };
        properties.PermanentBeam = PermanentBeamSword;
        properties.UseCommunityText = UseCommunityText;
        properties.StartAtk = StartingAttackLevel;
        properties.StartMag = StartingMagicLevel;
        properties.StartLifeLvl = StartingLifeLevel;

        //Overworld
        properties.ShuffleEncounters = ShuffleEncounters ?? GetIndeterminateFlagValue(r);
        properties.AllowPathEnemies = AllowUnsafePathEncounters;
        properties.IncludeLavaInEncounterShuffle = IncludeLavaInEncounterShuffle;
        properties.PalacesCanSwapContinent = PalacesCanSwapContinents ?? GetIndeterminateFlagValue(r);
        properties.P7shuffle = ShuffleGP ?? GetIndeterminateFlagValue(r);
        properties.HiddenPalace = HidePalace ?? GetIndeterminateFlagValue(r);
        properties.HiddenKasuto = HideKasuto ?? GetIndeterminateFlagValue(r);

        properties.EncounterRates = EncounterRate;
        properties.ContinentConnections = ContinentConnectionType;
        properties.BoulderBlockConnections = AllowConnectionCavesToBeBlocked;
        if (WestBiome == Biome.RANDOM || WestBiome == Biome.RANDOM_NO_VANILLA || WestBiome == Biome.RANDOM_NO_VANILLA_OR_SHUFFLE)
        {
            int shuffleLimit = WestBiome switch {
                Biome.RANDOM => 7,
                Biome.RANDOM_NO_VANILLA => 6,
                Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => 5,
                _ => throw new ImpossibleException()
            };
            properties.WestBiome = r.Next(shuffleLimit) switch
            {
                0 => Biome.VANILLALIKE,
                1 => Biome.ISLANDS,
                2 => r.Next(2) == 1 ? Biome.CANYON : Biome.DRY_CANYON,
                3 => Biome.CALDERA,
                4 => Biome.MOUNTAINOUS,
                5 => Biome.VANILLA_SHUFFLE,
                6 => Biome.VANILLA,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else if(WestBiome == Biome.CANYON)
        {
            properties.WestBiome = r.Next(2) == 0 ? Biome.CANYON : Biome.DRY_CANYON;
        }
        else {
            properties.WestBiome = WestBiome;
        }
        if (EastBiome == Biome.RANDOM || EastBiome == Biome.RANDOM_NO_VANILLA || EastBiome == Biome.RANDOM_NO_VANILLA_OR_SHUFFLE)
        {
            int shuffleLimit = EastBiome switch { 
                Biome.RANDOM => 7, 
                Biome.RANDOM_NO_VANILLA => 6, 
                Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => 5,
                _ => throw new ImpossibleException()
            };
            properties.EastBiome = r.Next(shuffleLimit) switch
            {
                0 => Biome.VANILLALIKE,
                1 => Biome.ISLANDS,
                2 => r.Next(2) == 1 ? Biome.CANYON : Biome.DRY_CANYON,
                3 => Biome.VOLCANO,
                4 => Biome.MOUNTAINOUS,
                5 => Biome.VANILLA_SHUFFLE,
                6 => Biome.VANILLA,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else if (EastBiome == Biome.CANYON)
        {
            properties.EastBiome = r.Next(2) == 0 ? Biome.CANYON : Biome.DRY_CANYON;
        }
        else
        {
            properties.EastBiome = EastBiome;
        }
        if (DMBiome == Biome.RANDOM || DMBiome == Biome.RANDOM_NO_VANILLA || DMBiome == Biome.RANDOM_NO_VANILLA_OR_SHUFFLE)
        {
            int shuffleLimit = DMBiome switch {
                Biome.RANDOM => 7,
                Biome.RANDOM_NO_VANILLA => 6,
                Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => 5,
                _ => throw new ImpossibleException()
            };
            properties.DmBiome = r.Next(shuffleLimit) switch
            {
                0 => Biome.VANILLALIKE,
                1 => Biome.ISLANDS,
                2 => r.Next(2) == 1 ? Biome.CANYON : Biome.DRY_CANYON,
                3 => Biome.CALDERA,
                4 => Biome.MOUNTAINOUS,
                5 => Biome.VANILLA_SHUFFLE,
                6 => Biome.VANILLA,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else if (DMBiome == Biome.CANYON)
        {
            properties.DmBiome = r.Next(2) == 0 ? Biome.CANYON : Biome.DRY_CANYON;
        }
        else
        {
            properties.DmBiome = DMBiome;
        }
        if (MazeBiome == Biome.RANDOM)
        {
            properties.MazeBiome = r.Next(3) switch
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
        if (Climate == null)
        {
            properties.Climate = r.Next(5) switch
            {
                0 => Climates.Classic,
                1 => Climates.Chaos,
                2 => Climates.Wetlands,
                3 => Climates.GreatLakes,
                4 => Climates.Scrubland,
                _ => throw new Exception("Unrecognized climate")
            };
        }
        else
        {
            properties.Climate = Climate;
        }
        properties.VanillaShuffleUsesActualTerrain = VanillaShuffleUsesActualTerrain;
        properties.ShuffleHidden = ShuffleWhichLocationIsHidden ?? GetIndeterminateFlagValue(r);
        properties.CanWalkOnWaterWithBoots = GoodBoots ?? GetIndeterminateFlagValue(r);
        properties.BagusWoods = GenerateBaguWoods ?? GetIndeterminateFlagValue(r);
        if(RiverDevilBlockerOption == RiverDevilBlockerOption.RANDOM)
        {
            properties.RiverDevilBlockerOption = r.Next(3) switch
            {
                0 => RiverDevilBlockerOption.PATH,
                1 => RiverDevilBlockerOption.CAVE,
                2 => RiverDevilBlockerOption.SIEGE,
                _ => throw new ImpossibleException("Invalid RiverDevilBlockerOption random option in Export")
            };
        }
        else
        {
            properties.RiverDevilBlockerOption = RiverDevilBlockerOption;
        }
        properties.EastRocks = EastRocks ?? GetIndeterminateFlagValue(r);

        properties.StartGems = r.Next(PalacesToCompleteMin, PalacesToCompleteMax + 1);
        properties.RequireTbird = TBirdRequired ?? GetIndeterminateFlagValue(r);
        properties.ShufflePalacePalettes = ChangePalacePallettes;
        properties.UpARestartsAtPalaces = RestartAtPalacesOnGameOver;
        properties.Global5050JarDrop = Global5050JarDrop ?? GetIndeterminateFlagValue(r);
        properties.ReduceDripperVariance = ReduceDripperVariance;
        properties.RemoveTbird = RemoveTBird;
        properties.BossItem = RandomizeBossItemDrop;

        //if all 3 room options are hard false, the seed can't generate. The UI tries to prevent this, but as a safety
        //if we get to this point, use vanilla rooms
        if(!((IncludeVanillaRooms ?? true) || (Includev4_0Rooms ?? true) || (Includev4_4Rooms ?? true)))
        {
            properties.AllowVanillaRooms = true;
        }
        while (!(properties.AllowVanillaRooms || properties.AllowV4Rooms || properties.AllowV4_4Rooms)) {
            properties.AllowVanillaRooms = IncludeVanillaRooms ?? GetIndeterminateFlagValue(r); ;
            properties.AllowV4Rooms = Includev4_0Rooms ?? GetIndeterminateFlagValue(r); ;
            properties.AllowV4_4Rooms = Includev4_4Rooms ?? GetIndeterminateFlagValue(r); ;
        }

        properties.BlockersAnywhere = BlockingRoomsInAnyPalace;

        if (BossRoomsExitType == BossRoomsExitType.RANDOM_ALL)
        {
            BossRoomsExitType option = r.Next(2) switch
            {
                0 => BossRoomsExitType.OVERWORLD,
                1 => BossRoomsExitType.PALACE,
                _ => throw new Exception("Invalid BossRoomsExit")
            };
            for (int i = 0; i < 6; i++)
            {
                properties.BossRoomsExits[i] = option;
            }
        }
        else if (BossRoomsExitType == BossRoomsExitType.RANDOM_PER_PALACE)
        {
            for (int i = 0; i < 6; i++)
            {
                BossRoomsExitType option = r.Next(2) switch
                {
                    0 => BossRoomsExitType.OVERWORLD,
                    1 => BossRoomsExitType.PALACE,
                    _ => throw new Exception("Invalid BossRoomsExit")
                };
                properties.BossRoomsExits[i] = option;
            }
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                properties.BossRoomsExits[i] = BossRoomsExitType;
            }
        }

        properties.NoDuplicateRooms = NoDuplicateRoomsByEnemies;
        properties.NoDuplicateRoomsBySideview = NoDuplicateRoomsByLayout;
        properties.GeneratorsAlwaysMatch = GeneratorsAlwaysMatch;
        properties.HardBosses = HardBosses;

        //Enemies
        properties.ShuffleEnemyHP = ShuffleEnemyHP;
        properties.ShuffleEnemyStealExp = ShuffleXPStealers;
        properties.ShuffleStealExpAmt = ShuffleXPStolenAmount;
        properties.ShuffleSwordImmunity = ShuffleSwordImmunity;
        properties.ShuffleOverworldEnemies = ShuffleOverworldEnemies ?? GetIndeterminateFlagValue(r); ;
        properties.ShufflePalaceEnemies = ShufflePalaceEnemies ?? GetIndeterminateFlagValue(r); ;
        properties.MixLargeAndSmallEnemies = MixLargeAndSmallEnemies ?? GetIndeterminateFlagValue(r); ;
        properties.ShuffleDripper = ShuffleDripperEnemy;
        properties.ShuffleEnemyPalettes = ShuffleSpritePalettes;
        properties.EnemyXPDrops = EnemyXPDrops;

        //Levels
        properties.ShuffleAtkExp = ShuffleAttackExperience;
        properties.ShuffleMagicExp = ShuffleMagicExperience;
        properties.ShuffleLifeExp = ShuffleLifeExperience;
        properties.AttackEffectiveness = AttackEffectiveness;
        properties.MagicEffectiveness = MagicEffectiveness;
        properties.LifeEffectiveness = LifeEffectiveness;
        properties.ShuffleLifeRefill = ShuffleLifeRefillAmount;
        properties.ShuffleSpellLocations = ShuffleSpellLocations ?? GetIndeterminateFlagValue(r);
        properties.DisableMagicRecs = DisableMagicContainerRequirements ?? GetIndeterminateFlagValue(r);
        properties.AttackCap = AttackLevelCap;
        properties.MagicCap = MagicLevelCap;
        properties.LifeCap = LifeLevelCap;
        properties.ScaleLevels = ScaleLevelRequirementsToCap;
        properties.HideLessImportantLocations = HideLessImportantLocations ?? GetIndeterminateFlagValue(r);
        properties.SaneCaves = RestrictConnectionCaveShuffle ?? GetIndeterminateFlagValue(r);
        properties.SpellEnemy = RandomizeSpellSpellEnemy ?? GetIndeterminateFlagValue(r);

        //Items
        properties.ShuffleOverworldItems = ShuffleOverworldItems ?? GetIndeterminateFlagValue(r);
        properties.ShufflePalaceItems = ShufflePalaceItems ?? GetIndeterminateFlagValue(r);
        properties.MixOverworldPalaceItems = MixOverworldAndPalaceItems ?? GetIndeterminateFlagValue(r);
        properties.RandomizeSmallItems = ShuffleSmallItems;
        properties.ExtraKeys = PalacesContainExtraKeys ?? GetIndeterminateFlagValue(r);
        properties.RandomizeNewKasutoBasementRequirement = RandomizeNewKasutoJarRequirements;
        properties.AllowImportantItemDuplicates = AllowImportantItemDuplicates;
        properties.PbagItemShuffle = IncludePBagCavesInItemShuffle ?? GetIndeterminateFlagValue(r);
        properties.StartWithSpellItems = RemoveSpellItems ?? GetIndeterminateFlagValue(r);
        properties.ShufflePbagXp = ShufflePBagAmounts ?? GetIndeterminateFlagValue(r);
        properties.IncludeQuestItemsInShuffle = IncludeQuestItemsInShuffle ?? GetIndeterminateFlagValue(r);
        properties.IncludeSpellsInShuffle = IncludeSpellsInShuffle ?? GetIndeterminateFlagValue(r);
        properties.IncludeSwordTechsInShuffle = IncludeSwordTechsInShuffle ?? GetIndeterminateFlagValue(r);

        //Drops
        properties.ShuffleItemDropFrequency = ShuffleItemDropFrequency;
        if (randomizeDrops)
        {
            do
            {
                properties.Smallbluejar = !SmallEnemiesCanDropBlueJar && RandomizeDrops ? r.Next(2) == 1 : SmallEnemiesCanDropBlueJar;
                properties.Smallredjar = !SmallEnemiesCanDropRedJar && RandomizeDrops ? r.Next(2) == 1 : SmallEnemiesCanDropRedJar;
                properties.Small50 = !SmallEnemiesCanDropSmallBag && RandomizeDrops ? r.Next(2) == 1 : SmallEnemiesCanDropSmallBag;
                properties.Small100 = !SmallEnemiesCanDropMediumBag && RandomizeDrops ? r.Next(2) == 1 : SmallEnemiesCanDropMediumBag;
                properties.Small200 = !SmallEnemiesCanDropLargeBag && RandomizeDrops ? r.Next(2) == 1 : SmallEnemiesCanDropLargeBag;
                properties.Small500 = !SmallEnemiesCanDropXLBag && RandomizeDrops ? r.Next(2) == 1 : SmallEnemiesCanDropXLBag;
                properties.Small1up = !SmallEnemiesCanDrop1up && RandomizeDrops ? r.Next(2) == 1 : SmallEnemiesCanDrop1up;
                properties.Smallkey = !SmallEnemiesCanDropKey && RandomizeDrops ? r.Next(2) == 1 : SmallEnemiesCanDropKey;
            } while (properties is { Smallbluejar: false, Smallredjar: false, Small50: false, Small100: false, Small200: false, Small500: false, Small1up: false, Smallkey: false });
        }
        if (randomizeDrops)
        {
            do
            {
                properties.Largebluejar = !LargeEnemiesCanDropBlueJar && RandomizeDrops ? r.Next(2) == 1 : LargeEnemiesCanDropBlueJar;
                properties.Largeredjar = !LargeEnemiesCanDropRedJar && RandomizeDrops ? r.Next(2) == 1 : LargeEnemiesCanDropRedJar;
                properties.Large50 = !LargeEnemiesCanDropSmallBag && RandomizeDrops ? r.Next(2) == 1 : LargeEnemiesCanDropSmallBag;
                properties.Large100 = !LargeEnemiesCanDropMediumBag && RandomizeDrops ? r.Next(2) == 1 : LargeEnemiesCanDropMediumBag;
                properties.Large200 = !LargeEnemiesCanDropLargeBag && RandomizeDrops ? r.Next(2) == 1 : LargeEnemiesCanDropLargeBag;
                properties.Large500 = !LargeEnemiesCanDropXLBag && RandomizeDrops ? r.Next(2) == 1 : LargeEnemiesCanDropXLBag;
                properties.Large1up = !LargeEnemiesCanDrop1up && RandomizeDrops ? r.Next(2) == 1 : LargeEnemiesCanDrop1up;
                properties.Largekey = !LargeEnemiesCanDropKey && RandomizeDrops ? r.Next(2) == 1 : LargeEnemiesCanDropKey;
            } while (properties is { Largebluejar: false, Largeredjar: false, Large50: false, Large100: false, Large200: false, Large500: false, Large1up: false, Largekey: false });
        }
        properties.StandardizeDrops = StandardizeDrops;
        properties.RandomizeDrops = randomizeDrops;

        //Hints
        properties.SpellItemHints = EnableSpellItemHints ?? GetIndeterminateFlagValue(r);
        properties.HelpfulHints = EnableHelpfulHints ?? GetIndeterminateFlagValue(r);
        properties.TownNameHints = EnableTownNameHints ?? GetIndeterminateFlagValue(r);

        //Misc.
        properties.BeepThreshold = BeepThreshold switch
        {
            //Normal
            BeepThreshold.Normal => 0x20,
            //Half Speed
            BeepThreshold.HalfBar => 0x10,
            //Quarter Speed
            BeepThreshold.QuarterBar => 0x08,
            //Off
            BeepThreshold.TwoBars => 0x40,
            _ => 0x20
        };
        properties.BeepFrequency = BeepFrequency switch
        {
            //Normal
            BeepFrequency.Normal => 0x30,
            //Half Speed
            BeepFrequency.HalfSpeed => 0x60,
            //Quarter Speed
            BeepFrequency.QuarterSpeed => 0xC0,
            //Off
            BeepFrequency.Off => 0,
            _ => 0x30
        };
        properties.JumpAlwaysOn = JumpAlwaysOn;
        properties.DashAlwaysOn = DashAlwaysOn;
        properties.FastCast = FastSpellCasting;
        properties.BeamSprite = BeamSprite;
        properties.DisableMusic = DisableMusic;
        properties.RandomizeMusic = RandomizeMusic;
        properties.MixCustomAndOriginalMusic = MixCustomAndOriginalMusic;
        properties.DisableUnsafeMusic = DisableUnsafeMusic;
        properties.CharSprite = Sprite;
        properties.ChangeItemSprites = ChangeItemSprites;
        properties.TunicColor = Tunic;
        properties.OutlineColor = TunicOutline;
        properties.ShieldColor = ShieldTunic;
        properties.UpAC1 = UpAOnController1;
        properties.RemoveFlashing = RemoveFlashing;
        //Removed the option to select this for now.
        properties.UseCustomRooms = false;
        properties.DisableHUDLag = DisableHUDLag;
        properties.RandomizeKnockback = RandomizeKnockback;

        //"Server" side validation
        //This is a replication of a bunch of logic from the UI so that configurations from sources other than the UI (YAML)
        //or indeterminately generated properties still correspond to sanity. Without this you get randomly ungeneratable seeds.
        if (!properties.ShuffleEncounters)
        {
            properties.AllowPathEnemies = false;
            properties.IncludeLavaInEncounterShuffle = false;
        }

        if(properties.IncludeSwordTechsInShuffle)
        {
            properties.SwapUpAndDownStab = false;
        }

        if (properties is { ShuffleOverworldEnemies: false, ShufflePalaceEnemies: false })
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

        //#180 Remove tbird doesn't currently work with vanilla, so make sure even if it comes up on random it works properly.
        if (properties.PalaceStyles[6] == PalaceStyle.VANILLA)
        {
            properties.RemoveTbird = false;
        }

        if (!properties.PalacesCanSwapContinent)
        {
            properties.P7shuffle = false;
        }

        if (properties.StartWithSpellItems)
        {
            properties.SpellItemHints = false;
        }

        //if (eastBiome.SelectedIndex == 0 || (hiddenPalaceList.SelectedIndex == 0 && hideKasutoList.SelectedIndex == 0))
        if (properties.EastBiome == Biome.VANILLA || properties is { HiddenPalace: false, HiddenKasuto: false })
        {
            properties.ShuffleHidden = false;
        }

        if (properties.WestBiome is Biome.VANILLA or Biome.VANILLA_SHUFFLE)
        {
            properties.BagusWoods = false;
        }

        if (properties.ReplaceFireWithDash)
        {
            properties.CombineFire = false;
        }

        //If spells are in the shuffle pool, shuffle spells means nothing, so diable it
        if(properties.IncludeSpellsInShuffle)
        {
            properties.ShuffleSpellLocations = false;
        }

        //Same principle with sword techs in the pool meaning swap stabs is meaningless.
        if (properties.IncludeSwordTechsInShuffle)
        {
            properties.SwapUpAndDownStab = false;
        }

        
        //--This needed to be moved to the bottom because the validation could adjust some properties this relies on.--
        
        //I'm not sure whether I like the bias introduced in generating random values and then capping them
        //vs just determining min/max ranges and fair rolling between them. Keeping it for now.
        int[] palaceRoomItemsMax = [1, 1, 1, 1, 1, 1];
        switch (PalaceItemRoomCount)
        {
            case PalaceItemRoomCount.RANDOM:
                palaceRoomItemsMax = properties.ShortenNormalPalaces ? [1, 2, 1, 2, 2, 2] : [2, 3, 2, 3, 4, 4];
                for (int i = 0; i < 6; i++)
                {
                    properties.PalaceItemRoomCounts[i] = r.Next(1, palaceRoomItemsMax[i] + 1);
                    // Limit vanilla palace style to 1 item rooms max
                    // Rationale:
                    // The benefit of the vanilla palace style is that you can use vanilla
                    // knowledge to know exactly where to go. Changing random rooms into
                    // additonal item rooms ruins this. So, unless the user specifically
                    // sets two items per, we should not do it.
                    //
                    // This way we can combine the fun of having both style and item count
                    // set to random, for Max Rando players, without the downside of having
                    // to track down which room was changed in a vanilla palace.
                    if (properties.PalaceStyles[i] == PalaceStyle.VANILLA)
                    {
                        properties.PalaceItemRoomCounts[i] = Math.Min(properties.PalaceItemRoomCounts[i], 1);
                    }
                    // Limit shuffled vanilla palace style to 2 item rooms max
                    // Technically, non-shortened P4 & P5 can have 3 item rooms,
                    // but lets keep it simple.
                    else if (properties.PalaceStyles[i] == PalaceStyle.SHUFFLED)
                    {
                        properties.PalaceItemRoomCounts[i] = Math.Min(properties.PalaceItemRoomCounts[i], 2);
                    }
                }
                properties.UsePalaceItemRoomCountIndicator = true;
                break;
            default:
                properties.PalaceItemRoomCounts = Enumerable.Repeat((int)PalaceItemRoomCount, 6).ToArray();
                properties.UsePalaceItemRoomCountIndicator = false;
                break;
        }

        //If shuffle palace items is off, the minimum number of palace rooms for a palace must be 1
        //otherwise it is impossible to place the palace items.

        if (!properties.ShufflePalaceItems)
        {
            for (int i = 0; i < 6; i++)
            {
                properties.PalaceItemRoomCounts[i] = int.Max(properties.PalaceItemRoomCounts[i], 1);
            }
        }

        //If mixed palace/overworld items is off, places must contain at least 6 items total so there is a place to put the items
        if (!properties.MixOverworldPalaceItems)
        {
            while (properties.PalaceItemRoomCounts.Sum() < 6)
            {
                int i = r.Next(6);
                if (properties.PalaceItemRoomCounts[i] < palaceRoomItemsMax[i])
                {
                    properties.PalaceItemRoomCounts[i]++;
                }
            }
        }

        // string debug = JsonSerializer.Serialize(properties);
        return properties;
    }

    /// Let the user know when their combination of flags will not be
    /// possible to achieve.
    /// 
    /// For indeterminate options, we will only check that the best case
    /// scenario should work.
    public void CheckForFlagConflicts()
    {
        int requiredMinorItemReplacements = 0;
        if ((StartingHeartContainersMax ?? 8) < 4)
        {
            requiredMinorItemReplacements = 4 - (StartingHeartContainersMax ?? 4);
        }
        if (PalaceItemRoomCount == PalaceItemRoomCount.ZERO)
        {
            requiredMinorItemReplacements += 6;
        }
        if (CountPossibleMinorItems() < requiredMinorItemReplacements)
        {
            throw new UserFacingException("Impossible Item Flags", "Not enough possible item locations for removed palace items.\n\nAdd more starting items or more palace items.");
        }

        if (NoDuplicateRoomsByLayout || NoDuplicateRoomsByEnemies)
        {
            // if current palace generation logic changes, this should be updated
            int potentialRoomPools = 0;
            if (IncludeVanillaRooms != false) { potentialRoomPools++; }
            if (Includev4_0Rooms != false) { potentialRoomPools++; }
            if (Includev4_4Rooms != false) { potentialRoomPools++; }
            if (potentialRoomPools < 2)
            {
                throw new UserFacingException("Incompatible Palace Flags", "Not enough palace rooms in the pool.\n\nUnder the Palaces tab, include more room groups or disable No Duplicate Rooms.");
            }
        }
    }

    public static bool IsIntegerType(Type type)
    {
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Byte => true,
            TypeCode.Int16 => true,
            TypeCode.Int32 => true,
            TypeCode.Int64 => true,
            TypeCode.SByte => true,
            TypeCode.UInt16 => true,
            TypeCode.UInt32 => true,
            TypeCode.UInt64 => true,
            _ => false
        };
    }

    public string GetRoomsFile()
    {
        return UseCustomRooms ? "CustomRooms.json" : "PalaceRooms.json";
    }

    private bool GetIndeterminateFlagValue(Random r)
    {
        return r.NextDouble() < IndeterminateOptionRate switch
        {
            IndeterminateOptionRate.QUARTER => .25,
            IndeterminateOptionRate.HALF => .50,
            IndeterminateOptionRate.THREE_QUARTERS => .75,
            IndeterminateOptionRate.NINETY_PERCENT => .90,
            _ => throw new Exception("Unrecognized IndeterminateOptionRate")
        };
    }

    public bool StartsWithCollectable(Collectable collectable)
    {
        return collectable switch
        {
            Collectable.SHIELD_SPELL => StartWithShield,
            Collectable.JUMP_SPELL => StartWithJump,
            Collectable.LIFE_SPELL => StartWithLife,
            Collectable.FAIRY_SPELL => StartWithFairy,
            Collectable.FIRE_SPELL => StartWithFire,
            Collectable.DASH_SPELL => StartWithFire,
            Collectable.REFLECT_SPELL => StartWithReflect,
            Collectable.SPELL_SPELL => StartWithSpellSpell,
            Collectable.THUNDER_SPELL => StartWithThunder,
            Collectable.CANDLE => StartWithCandle,
            Collectable.GLOVE => StartWithGlove,
            Collectable.RAFT => StartWithRaft,
            Collectable.BOOTS => StartWithBoots,
            Collectable.FLUTE => StartWithFlute,
            Collectable.CROSS => StartWithCross,
            Collectable.HAMMER => StartWithHammer,
            Collectable.MAGIC_KEY => StartWithMagicKey,
            _ => throw new ImpossibleException("Unrecognized collectable")
        };
    }

    private void ShuffleStartingCollectables(Collectable[] possibleCollectables, StartingResourceLimit limit, bool shuffleRandom, 
        RandomizerProperties properties, Random r)
    {
        int itemLimit = limit.AsInt();

        List<Collectable> startingItems = [];

        Collectable[] randomPossibleCollectables = new Collectable[possibleCollectables.Length];
        Array.Copy(possibleCollectables, randomPossibleCollectables, possibleCollectables.Length);
        r.Shuffle(randomPossibleCollectables);
        foreach (Collectable collectable in randomPossibleCollectables)
        {
            if (startingItems.Count >= itemLimit)
            {
                break;
            }
            if (StartsWithCollectable(collectable))
            {
                startingItems.Add(collectable);
            }
        }

        if (shuffleRandom)
        {
            foreach (Collectable collectable in randomPossibleCollectables)
            {
                if (startingItems.Count >= itemLimit)
                {
                    break;
                }
                if (!StartsWithCollectable(collectable))
                {
                    if (r.Next(4) == 0)
                    {
                        startingItems.Add(collectable);
                    }
                }
            }
        }

        foreach (Collectable collectable in startingItems)
        {
            properties.SetStartingCollectable(collectable);
        }
    }

    private int CountPossibleMinorItems()
    {
        int count = 3, hardStartItemsCount = 0;

        hardStartItemsCount += ShuffleStartingItems || StartWithCandle ? 1 : 0;
        hardStartItemsCount += ShuffleStartingItems || StartWithBoots ? 1 : 0;
        hardStartItemsCount += ShuffleStartingItems || StartWithCross ? 1 : 0;
        hardStartItemsCount += ShuffleStartingItems || StartWithFlute ? 1 : 0;
        hardStartItemsCount += ShuffleStartingItems || StartWithGlove ? 1 : 0;
        hardStartItemsCount += ShuffleStartingItems || StartWithHammer ? 1 : 0;
        hardStartItemsCount += ShuffleStartingItems || StartWithMagicKey ? 1 : 0;
        hardStartItemsCount += ShuffleStartingItems || StartWithRaft ? 1 : 0;

        count += Math.Max(hardStartItemsCount, ShuffleStartingItems ? StartItemsLimit.AsInt() : 0);

        if(IncludeSpellsInShuffle ?? true)
        {
            hardStartItemsCount = 0;
            hardStartItemsCount += ShuffleStartingSpells || StartWithShield ? 1 : 0;
            hardStartItemsCount += ShuffleStartingSpells || StartWithJump ? 1 : 0;
            hardStartItemsCount += ShuffleStartingSpells || StartWithLife ? 1 : 0;
            hardStartItemsCount += ShuffleStartingSpells || StartWithFairy ? 1 : 0;
            hardStartItemsCount += ShuffleStartingSpells || StartWithFire ? 1 : 0;
            hardStartItemsCount += ShuffleStartingSpells || StartWithReflect ? 1 : 0;
            hardStartItemsCount += ShuffleStartingSpells || StartWithSpellSpell ? 1 : 0;
            hardStartItemsCount += ShuffleStartingSpells || StartWithThunder ? 1 : 0;

            count += Math.Max(hardStartItemsCount, ShuffleStartingItems ? StartItemsLimit.AsInt() : 0);
        }

        if(IncludeSwordTechsInShuffle ?? true)
        {
            hardStartItemsCount += StartingTechniques switch
            {
                StartingTechs.DOWNSTAB => 1,
                StartingTechs.UPSTAB => 1,
                StartingTechs.BOTH => 2,
                StartingTechs.RANDOM => 2,
                StartingTechs.NONE => 0,
                _ => throw new Exception("Unrecognized starting tech option")
            };
        }

        int containerReplacementSmallItemsCount = MaxHeartContainers switch
        {
            MaxHeartsOption.EIGHT => 4 - (8 - (StartingHeartContainersMax ?? 8)),
            MaxHeartsOption.SEVEN => 4 - (7 - (StartingHeartContainersMax ?? 8)),
            MaxHeartsOption.SIX => 4 - (6 - (StartingHeartContainersMax ?? 8)),
            MaxHeartsOption.FIVE => 4 - (5 - (StartingHeartContainersMax ?? 8)),
            MaxHeartsOption.FOUR => 4 - (4 - (StartingHeartContainersMax ?? 8)),
            MaxHeartsOption.THREE => 4 - (3 - (StartingHeartContainersMax ?? 8)),
            MaxHeartsOption.TWO => 4 - (2 - (StartingHeartContainersMax ?? 8)),
            MaxHeartsOption.ONE => 4 - (1 - (StartingHeartContainersMax ?? 8)),
            MaxHeartsOption.PLUS_ONE => 3,
            MaxHeartsOption.PLUS_TWO => 2,
            MaxHeartsOption.PLUS_THREE => 1,
            MaxHeartsOption.PLUS_FOUR => 0,
            MaxHeartsOption.RANDOM => 4 - (StartingHeartContainersMax ?? 1),
            _ => throw new Exception("Unrecognized Max Hearts in CountPossibleMinorItems")
        };

        count += containerReplacementSmallItemsCount;

        return count;
    }

    private int GetDarkLinkMinDistance()
    {
        if (DarkLinkMinDistance == BossRoomMinDistance.MAX)
        {
            // limiting here based on how long it takes to generate the seeds
            if (GPStyle == PalaceStyle.RECONSTRUCTED) { return 16; }
            if (ShortenGP != false) { return 20; }
            if (GPStyle == PalaceStyle.SEQUENTIAL) { return 24; }
            return 28;
        }
        else
        {
            return (int)DarkLinkMinDistance;
        }
    }
}
