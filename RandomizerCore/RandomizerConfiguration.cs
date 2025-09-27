﻿using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Z2Randomizer.RandomizerCore.Flags;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore;

[AttributeUsage(AttributeTargets.Class)]
public class FlagSerializeAttribute : Attribute
{
}

/**
 * We don't need to bring in ReactiveUI to the base RandomizerCore if we just make our own source generator.
 * To keep the usage similar to the original ReactiveUI SourceGenerator, I kept the name `Reactive` for the attribute
 * in case we bail on this idea later.
 */
public class ReactiveAttribute : Attribute
{

}


[FlagSerialize]
public sealed partial class RandomizerConfiguration : INotifyPropertyChanged
{
    [IgnoreInFlags]
    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    [IgnoreInFlags]
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

    [IgnoreInFlags]
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
    [Limit(8)]
    [Minimum(1)]
    private int? startingHeartContainersMin;

    [Reactive]
    [Limit(8)]
    [Minimum(1)]
    private int? startingHeartContainersMax;

    [Reactive]
    private MaxHeartsOption maxHeartContainers;

    [Reactive]
    private StartingTechs startingTechniques;

    [Reactive]
    private StartingLives startingLives;

    [Reactive]
    [Limit(8)]
    [Minimum(1)]
    private int startingAttackLevel;

    [Reactive]
    [Limit(8)]
    [Minimum(1)]
    private int startingMagicLevel;

    [Reactive]
    [Limit(8)]
    [Minimum(1)]
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
    private Biome dmBiome;

    [Reactive]
    private Biome mazeBiome;

    [Reactive]
    [CustomFlagSerializer(typeof(ClimateFlagSerializer))]
    private Climate climate;

    [Reactive]
    private bool vanillaShuffleUsesActualTerrain;

    //Palaces
    [Reactive]
    private PalaceStyle normalPalaceStyle;

    [Reactive]
    private PalaceStyle gpStyle;

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
    [Limit(7)]
    private int palacesToCompleteMin;

    [Reactive]
    [Limit(7)]
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
    [Limit(8)]
    [Minimum(1)]
    private int attackLevelCap;

    [Reactive]
    [Limit(8)]
    [Minimum(1)]
    private int magicLevelCap;

    [Reactive]
    [Limit(8)]
    [Minimum(1)]
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
    [IgnoreInFlags]
    private bool useCommunityText;

    [Reactive]
    [IgnoreInFlags]
    private BeepFrequency beepFrequency;

    [Reactive]
    [IgnoreInFlags]
    private BeepThreshold beepThreshold;

    [Reactive]
    [IgnoreInFlags]
    private bool disableMusic;

    [Reactive]
    [IgnoreInFlags]
    private bool randomizeMusic;

    [Reactive]
    [IgnoreInFlags]
    private bool mixCustomAndOriginalMusic;
    
    [Reactive]
    [IgnoreInFlags]
    private bool includeDiverseMusic;

    [Reactive]
    [IgnoreInFlags]
    private bool disableUnsafeMusic;

    [Reactive]
    [IgnoreInFlags]
    private bool fastSpellCasting;

    [Reactive]
    [IgnoreInFlags]
    private bool upAOnController1;

    [Reactive]
    [IgnoreInFlags]
    private bool removeFlashing;

    [Reactive]
    [ IgnoreInFlags]
    private CharacterSprite sprite;

    [Reactive]
    [IgnoreInFlags]
    private string spriteName;


    [Reactive]
    [IgnoreInFlags]
    private bool changeItemSprites;

    [Reactive]
    [IgnoreInFlags]
    private CharacterColor tunic;

    [Reactive]
    [IgnoreInFlags]
    private CharacterColor tunicOutline;

    [Reactive]
    [IgnoreInFlags]
    private CharacterColor shieldTunic;

    [Reactive]
    [IgnoreInFlags]
    private BeamSprites beamSprite;

    [Reactive]
    [IgnoreInFlags]
    private bool useCustomRooms;

    [Reactive]
    [IgnoreInFlags]
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

    [Reactive]
    private bool revealWalkthroughWalls;

    //Meta
    [Reactive]
    [Required]
    [IgnoreInFlags]
    private string? seed;
    // public string Seed { get => seed ?? ""; set => SetField(ref seed, value); }

    [IgnoreInFlags]
    [JsonIgnore]
    public string Flags
    {
        get => Serialize();
        set => Deserialize(value?.Trim() ?? "");
    }

    public RandomizerConfiguration()
    {
        startingAttackLevel = 1;
        startingMagicLevel = 1;
        startingLifeLevel = 1;

        maxHeartContainers = MaxHeartsOption.EIGHT;
        startingHeartContainersMin = 8;
        startingHeartContainersMax = 8;

        attackLevelCap = 8;
        magicLevelCap = 8;
        lifeLevelCap = 8;

        disableMusic = false;
        randomizeMusic = false;
        mixCustomAndOriginalMusic = true;
        disableUnsafeMusic = true;
        fastSpellCasting = false;
        shuffleSpritePalettes = false;
        permanentBeamSword = false;
        upAOnController1 = false;
        removeFlashing = true;
        sprite = CharacterSprite.LINK;
        spriteName = CharacterSprite.LINK.DisplayName!;
        climate = Climates.Classic;
        if (sprite == null || climate == null)
        {
            throw new ImpossibleException();
        }
        tunic = CharacterColor.Default;
        tunicOutline = CharacterColor.Default;
        shieldTunic = CharacterColor.Default;
        beamSprite = BeamSprites.DEFAULT;
        useCustomRooms = false;
        disableHUDLag = false;
    }

    public RandomizerConfiguration(string flagstring) : this()
    {
        Deserialize(flagstring);
    }
    private bool DeserializeBool(FlagReader flags, string name)
    {
        return flags.ReadBool();
    }
    private bool? DeserializeNullableBool(FlagReader flags, string name)
    {
        return flags.ReadNullableBool();
    }
    private int DeserializeInt(FlagReader flags, string name, int limit, int? minimum)
    {
        int min = minimum ?? 0;
        return flags.ReadInt(limit) + min;
    }
    private int? DeserializeNullableInt(FlagReader flags, string name, int limit, int? minimum)
    {
        return flags.ReadNullableInt(limit, minimum);
    }

    private T DeserializeEnum<T>(FlagReader flags, string name) where T: Enum
    {
        var limit = GetEnumCount<T>();
        var index = flags.ReadInt(limit);
        return GetEnumFromIndex<T>(index)!;
    }
    private T? DeserializeNullableEnum<T>(FlagReader flags, string name) where T: Enum
    {
        var limit = GetEnumCount<T>();
        var index = flags.ReadNullableInt(limit);
        return index == null ? default : GetEnumFromIndex<T>(index.Value)!;
    }

    private T DeserializeCustom<Serializer, T>(FlagReader flags, string name) where Serializer : IFlagSerializer where T : class
    {
        IFlagSerializer serializer = GetSerializer<Serializer>();
        return (T)serializer.Deserialize(flags.ReadInt(serializer.GetLimit()))!;
    }

    private void SerializeBool(FlagBuilder flags, string name, bool? val, bool isNullable)
    {
        if (isNullable)
        {
            flags.Append(val);
        }
        else
        {
            bool v = val!.Value;
            flags.Append(v);
        }
    }

    private void SerializeInt(FlagBuilder flags, string name, int? val, bool isNullable, int limit, int? minimum)
    {
        // limit is checked for null in the flags source generator
        if (isNullable)
        {
            if (val != null && (val < minimum || val > minimum + limit))
            {
                logger.Warn($"Property ({name}) was out of range.");
            }
            flags.Append(val, limit, minimum);
        }
        else
        {
            var value = val!.Value;
            if (value < minimum || value > minimum + limit)
            {
                logger.Warn($"Property ({name}) was out of range.");
            }
            flags.Append(value, limit, minimum);
        }
    }

    private void SerializeEnum<T>(FlagBuilder flags, string name, T? val) where T: Enum
    {
        var index = GetEnumIndex<T>(val);
        var limit = GetEnumCount<T>();
        flags.Append(index, limit);
    }

    private void SerializeCustom<Serializer, T>(FlagBuilder flags, string name, T? val) where Serializer : IFlagSerializer where T : class
    {
        var serializer = GetSerializer<Serializer>();
        flags.Append(serializer.Serialize(val), serializer.GetLimit());
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
            Seed = seed
        };

        //Properties that can affect available minor item replacements
        do // while (!properties.HasEnoughSpaceToAllocateItems())
        {
            //Start Configuration
            ShuffleStartingCollectables(POSSIBLE_STARTING_ITEMS, startItemsLimit, shuffleStartingItems, properties, r);
            ShuffleStartingCollectables(POSSIBLE_STARTING_SPELLS, startSpellsLimit, shuffleStartingSpells, properties, r);

            if (gpStyle == PalaceStyle.RANDOM)
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
            else if (gpStyle == PalaceStyle.RANDOM_NO_VANILLA_OR_SHUFFLE)
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
                properties.PalaceStyles[6] = gpStyle;
            }

            if (normalPalaceStyle == PalaceStyle.RANDOM_ALL)
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
            else if (normalPalaceStyle == PalaceStyle.RANDOM_PER_PALACE)
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
                    properties.PalaceStyles[i] = normalPalaceStyle;
                }
            }

            properties.ShortenGP = shortenGP ?? GetIndeterminateFlagValue(r);
            properties.ShortenNormalPalaces = shortenNormalPalaces ?? GetIndeterminateFlagValue(r);
            properties.DarkLinkMinDistance = GetDarkLinkMinDistance();

            AssignPalaceItemCounts(properties, r);

            //Other starting attributes
            int startHeartsMin, startHeartsMax;
            if (startingHeartContainersMin == null)
            {
                startHeartsMin = r.Next(1, 9);
            }
            else
            {
                startHeartsMin = (int)startingHeartContainersMin;
            }
            if (startingHeartContainersMax == null)
            {
                startHeartsMax = r.Next(startHeartsMin, 9);
            }
            else
            {
                startHeartsMax = (int)startingHeartContainersMax;
            }
            properties.StartHearts = r.Next(startHeartsMin, startHeartsMax + 1);

            //+1/+2/+3
            if (maxHeartContainers == MaxHeartsOption.RANDOM)
            {
                properties.MaxHearts = r.Next(properties.StartHearts, 9);
            }
            else if ((int)maxHeartContainers <= 8)
            {
                properties.MaxHearts = (int)maxHeartContainers;
            }
            else
            {
                int additionalHearts = maxHeartContainers switch
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
        switch (fireOption)
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
        if (startingTechniques == StartingTechs.RANDOM)
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
            properties.StartWithDownstab = startingTechniques.StartWithDownstab();
            properties.StartWithUpstab = startingTechniques.StartWithUpstab();
        }
        properties.SwapUpAndDownStab = swapUpAndDownStab ?? GetIndeterminateFlagValue(r);


        properties.StartLives = startingLives switch
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
        properties.PermanentBeam = permanentBeamSword;
        properties.UseCommunityText = useCommunityText;
        properties.StartAtk = startingAttackLevel;
        properties.StartMag = startingMagicLevel;
        properties.StartLifeLvl = startingLifeLevel;

        //Overworld
        properties.ShuffleEncounters = shuffleEncounters ?? GetIndeterminateFlagValue(r);
        properties.AllowPathEnemies = allowUnsafePathEncounters;
        properties.IncludeLavaInEncounterShuffle = includeLavaInEncounterShuffle;
        properties.PalacesCanSwapContinent = palacesCanSwapContinents ?? GetIndeterminateFlagValue(r);
        properties.P7shuffle = shuffleGP ?? GetIndeterminateFlagValue(r);
        properties.HiddenPalace = hidePalace ?? GetIndeterminateFlagValue(r);
        properties.HiddenKasuto = hideKasuto ?? GetIndeterminateFlagValue(r);

        properties.EncounterRates = encounterRate;
        properties.ContinentConnections = continentConnectionType;
        properties.BoulderBlockConnections = allowConnectionCavesToBeBlocked;
        if (westBiome == Biome.RANDOM || westBiome == Biome.RANDOM_NO_VANILLA || westBiome == Biome.RANDOM_NO_VANILLA_OR_SHUFFLE)
        {
            int shuffleLimit = westBiome switch {
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
        else if(westBiome == Biome.CANYON)
        {
            properties.WestBiome = r.Next(2) == 0 ? Biome.CANYON : Biome.DRY_CANYON;
        }
        else {
            properties.WestBiome = westBiome;
        }
        if (eastBiome == Biome.RANDOM || eastBiome == Biome.RANDOM_NO_VANILLA || eastBiome == Biome.RANDOM_NO_VANILLA_OR_SHUFFLE)
        {
            int shuffleLimit = eastBiome switch { 
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
        else if (eastBiome == Biome.CANYON)
        {
            properties.EastBiome = r.Next(2) == 0 ? Biome.CANYON : Biome.DRY_CANYON;
        }
        else
        {
            properties.EastBiome = eastBiome;
        }
        if (dmBiome == Biome.RANDOM || dmBiome == Biome.RANDOM_NO_VANILLA || dmBiome == Biome.RANDOM_NO_VANILLA_OR_SHUFFLE)
        {
            int shuffleLimit = dmBiome switch {
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
        else if (dmBiome == Biome.CANYON)
        {
            properties.DmBiome = r.Next(2) == 0 ? Biome.CANYON : Biome.DRY_CANYON;
        }
        else
        {
            properties.DmBiome = dmBiome;
        }
        if (mazeBiome == Biome.RANDOM)
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
            properties.MazeBiome = mazeBiome;
        }
        if (climate == null)
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
            properties.Climate = climate;
        }
        properties.VanillaShuffleUsesActualTerrain = vanillaShuffleUsesActualTerrain;
        properties.ShuffleHidden = shuffleWhichLocationIsHidden ?? GetIndeterminateFlagValue(r);
        properties.CanWalkOnWaterWithBoots = goodBoots ?? GetIndeterminateFlagValue(r);
        properties.BagusWoods = generateBaguWoods ?? GetIndeterminateFlagValue(r);
        if(riverDevilBlockerOption == RiverDevilBlockerOption.RANDOM)
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
            properties.RiverDevilBlockerOption = riverDevilBlockerOption;
        }
        properties.EastRocks = eastRocks ?? GetIndeterminateFlagValue(r);

        properties.StartGems = r.Next(palacesToCompleteMin, palacesToCompleteMax + 1);
        properties.RequireTbird = tBirdRequired ?? GetIndeterminateFlagValue(r);
        properties.ShufflePalacePalettes = changePalacePallettes;
        properties.UpARestartsAtPalaces = restartAtPalacesOnGameOver;
        properties.Global5050JarDrop = global5050JarDrop ?? GetIndeterminateFlagValue(r);
        properties.ReduceDripperVariance = reduceDripperVariance;
        properties.RemoveTbird = removeTBird;
        properties.BossItem = randomizeBossItemDrop;

        //if all 3 room options are hard false, the seed can't generate. The UI tries to prevent this, but as a safety
        //if we get to this point, use vanilla rooms
        if(!((includeVanillaRooms ?? true) || (includev4_0Rooms ?? true) || (includev4_4Rooms ?? true)))
        {
            properties.AllowVanillaRooms = true;
        }
        while (!(properties.AllowVanillaRooms || properties.AllowV4Rooms || properties.AllowV4_4Rooms)) {
            properties.AllowVanillaRooms = includeVanillaRooms ?? GetIndeterminateFlagValue(r); ;
            properties.AllowV4Rooms = includev4_0Rooms ?? GetIndeterminateFlagValue(r); ;
            properties.AllowV4_4Rooms = includev4_4Rooms ?? GetIndeterminateFlagValue(r); ;
        }

        properties.BlockersAnywhere = blockingRoomsInAnyPalace;

        if (bossRoomsExitType == BossRoomsExitType.RANDOM_ALL)
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
        else if (bossRoomsExitType == BossRoomsExitType.RANDOM_PER_PALACE)
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
                properties.BossRoomsExits[i] = bossRoomsExitType;
            }
        }

        properties.NoDuplicateRooms = noDuplicateRoomsByEnemies;
        properties.NoDuplicateRoomsBySideview = noDuplicateRoomsByLayout;
        properties.GeneratorsAlwaysMatch = generatorsAlwaysMatch;
        properties.HardBosses = hardBosses;
        properties.RevealWalkthroughWalls = revealWalkthroughWalls;

        //Enemies
        properties.ShuffleEnemyHP = shuffleEnemyHP;
        properties.ShuffleEnemyStealExp = shuffleXPStealers;
        properties.ShuffleStealExpAmt = shuffleXPStolenAmount;
        properties.ShuffleSwordImmunity = shuffleSwordImmunity;
        properties.ShuffleOverworldEnemies = shuffleOverworldEnemies ?? GetIndeterminateFlagValue(r); ;
        properties.ShufflePalaceEnemies = shufflePalaceEnemies ?? GetIndeterminateFlagValue(r); ;
        properties.MixLargeAndSmallEnemies = mixLargeAndSmallEnemies ?? GetIndeterminateFlagValue(r); ;
        properties.ShuffleDripper = shuffleDripperEnemy;
        properties.ShuffleEnemyPalettes = shuffleSpritePalettes;
        properties.EnemyXPDrops = enemyXPDrops;

        //Levels
        properties.ShuffleAtkExp = shuffleAttackExperience;
        properties.ShuffleMagicExp = shuffleMagicExperience;
        properties.ShuffleLifeExp = shuffleLifeExperience;
        properties.AttackEffectiveness = attackEffectiveness;
        properties.MagicEffectiveness = magicEffectiveness;
        properties.LifeEffectiveness = lifeEffectiveness;
        properties.ShuffleLifeRefill = shuffleLifeRefillAmount;
        properties.ShuffleSpellLocations = shuffleSpellLocations ?? GetIndeterminateFlagValue(r);
        properties.DisableMagicRecs = disableMagicContainerRequirements ?? GetIndeterminateFlagValue(r);
        properties.AttackCap = attackLevelCap;
        properties.MagicCap = magicLevelCap;
        properties.LifeCap = lifeLevelCap;
        properties.ScaleLevels = scaleLevelRequirementsToCap;
        properties.HideLessImportantLocations = hideLessImportantLocations ?? GetIndeterminateFlagValue(r);
        properties.SaneCaves = restrictConnectionCaveShuffle ?? GetIndeterminateFlagValue(r);
        properties.SpellEnemy = randomizeSpellSpellEnemy ?? GetIndeterminateFlagValue(r);

        //Items
        properties.ShuffleOverworldItems = shuffleOverworldItems ?? GetIndeterminateFlagValue(r);
        properties.ShufflePalaceItems = shufflePalaceItems ?? GetIndeterminateFlagValue(r);
        properties.MixOverworldPalaceItems = mixOverworldAndPalaceItems ?? GetIndeterminateFlagValue(r);
        properties.RandomizeSmallItems = shuffleSmallItems;
        properties.ExtraKeys = palacesContainExtraKeys ?? GetIndeterminateFlagValue(r);
        properties.RandomizeNewKasutoBasementRequirement = randomizeNewKasutoJarRequirements;
        properties.AllowImportantItemDuplicates = allowImportantItemDuplicates;
        properties.PbagItemShuffle = includePBagCavesInItemShuffle ?? GetIndeterminateFlagValue(r);
        properties.StartWithSpellItems = removeSpellItems ?? GetIndeterminateFlagValue(r);
        properties.ShufflePbagXp = shufflePBagAmounts ?? GetIndeterminateFlagValue(r);
        properties.IncludeQuestItemsInShuffle = includeQuestItemsInShuffle ?? GetIndeterminateFlagValue(r);
        properties.IncludeSpellsInShuffle = includeSpellsInShuffle ?? GetIndeterminateFlagValue(r);
        properties.IncludeSwordTechsInShuffle = includeSwordTechsInShuffle ?? GetIndeterminateFlagValue(r);

        //Drops
        properties.ShuffleItemDropFrequency = shuffleItemDropFrequency;
        if (randomizeDrops)
        {
            do
            {
                properties.Smallbluejar = !smallEnemiesCanDropBlueJar && randomizeDrops ? r.Next(2) == 1 : smallEnemiesCanDropBlueJar;
                properties.Smallredjar = !smallEnemiesCanDropRedJar && randomizeDrops ? r.Next(2) == 1 : smallEnemiesCanDropRedJar;
                properties.Small50 = !smallEnemiesCanDropSmallBag && randomizeDrops ? r.Next(2) == 1 : smallEnemiesCanDropSmallBag;
                properties.Small100 = !smallEnemiesCanDropMediumBag && randomizeDrops ? r.Next(2) == 1 : smallEnemiesCanDropMediumBag;
                properties.Small200 = !smallEnemiesCanDropLargeBag && randomizeDrops ? r.Next(2) == 1 : smallEnemiesCanDropLargeBag;
                properties.Small500 = !smallEnemiesCanDropXLBag && randomizeDrops ? r.Next(2) == 1 : smallEnemiesCanDropXLBag;
                properties.Small1up = !smallEnemiesCanDrop1up && randomizeDrops ? r.Next(2) == 1 : smallEnemiesCanDrop1up;
                properties.Smallkey = !smallEnemiesCanDropKey && randomizeDrops ? r.Next(2) == 1 : smallEnemiesCanDropKey;
            } while (properties is { Smallbluejar: false, Smallredjar: false, Small50: false, Small100: false, Small200: false, Small500: false, Small1up: false, Smallkey: false });
        }
        if (randomizeDrops)
        {
            do
            {
                properties.Largebluejar = !largeEnemiesCanDropBlueJar && randomizeDrops ? r.Next(2) == 1 : largeEnemiesCanDropBlueJar;
                properties.Largeredjar = !largeEnemiesCanDropRedJar && randomizeDrops ? r.Next(2) == 1 : largeEnemiesCanDropRedJar;
                properties.Large50 = !largeEnemiesCanDropSmallBag && randomizeDrops ? r.Next(2) == 1 : largeEnemiesCanDropSmallBag;
                properties.Large100 = !largeEnemiesCanDropMediumBag && randomizeDrops ? r.Next(2) == 1 : largeEnemiesCanDropMediumBag;
                properties.Large200 = !largeEnemiesCanDropLargeBag && randomizeDrops ? r.Next(2) == 1 : largeEnemiesCanDropLargeBag;
                properties.Large500 = !largeEnemiesCanDropXLBag && randomizeDrops ? r.Next(2) == 1 : largeEnemiesCanDropXLBag;
                properties.Large1up = !largeEnemiesCanDrop1up && randomizeDrops ? r.Next(2) == 1 : largeEnemiesCanDrop1up;
                properties.Largekey = !largeEnemiesCanDropKey && randomizeDrops ? r.Next(2) == 1 : largeEnemiesCanDropKey;
            } while (properties is { Largebluejar: false, Largeredjar: false, Large50: false, Large100: false, Large200: false, Large500: false, Large1up: false, Largekey: false });
        }
        properties.StandardizeDrops = standardizeDrops;
        properties.RandomizeDrops = randomizeDrops;

        //Hints
        properties.SpellItemHints = enableSpellItemHints ?? GetIndeterminateFlagValue(r);
        properties.HelpfulHints = enableHelpfulHints ?? GetIndeterminateFlagValue(r);
        properties.TownNameHints = enableTownNameHints ?? GetIndeterminateFlagValue(r);

        //Misc.
        properties.BeepThreshold = beepThreshold switch
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
        properties.BeepFrequency = beepFrequency switch
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
        properties.JumpAlwaysOn = jumpAlwaysOn;
        properties.DashAlwaysOn = dashAlwaysOn;
        properties.FastCast = fastSpellCasting;
        properties.BeamSprite = beamSprite;
        properties.DisableMusic = disableMusic;
        properties.RandomizeMusic = randomizeMusic;
        properties.MixCustomAndOriginalMusic = mixCustomAndOriginalMusic;
        properties.IncludeDiverseMusic = includeDiverseMusic;
        properties.DisableUnsafeMusic = disableUnsafeMusic;
        properties.CharSprite = sprite;
        properties.ChangeItemSprites = changeItemSprites;
        properties.TunicColor = tunic;
        properties.OutlineColor = tunicOutline;
        properties.ShieldColor = shieldTunic;
        properties.UpAC1 = upAOnController1;
        properties.RemoveFlashing = removeFlashing;
        //Removed the option to select this for now.
        properties.UseCustomRooms = false;
        properties.DisableHUDLag = disableHUDLag;
        properties.RandomizeKnockback = randomizeKnockback;

        //"Server" side validation
        //This is a replication of a bunch of logic from the UI so that configurations from sources other than the UI (YAML?)
        //or indeterminately generated properties still correspond to sanity. Without this you get randomly ungeneratable seeds.

        bool rerollPalaceItemRoomCounts = false;

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
            rerollPalaceItemRoomCounts = true;
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
    
        if(rerollPalaceItemRoomCounts)
        {
            do
            {
                AssignPalaceItemCounts(properties, r);
            }
            while (!properties.HasEnoughSpaceToAllocateItems());
        }

        return properties;
    }

    public void AssignPalaceItemCounts(RandomizerProperties properties, Random r)
    {
        //I'm not sure whether I like the bias introduced in generating random values and then capping them
        //vs just determining min/max ranges and fair rolling between them. Keeping it for now.
        int[] palaceRoomItemsMax = [1, 1, 1, 1, 1, 1];
        switch (palaceItemRoomCount)
        {
            case PalaceItemRoomCount.RANDOM:
                palaceRoomItemsMax = properties.ShortenNormalPalaces ? [1, 2, 1, 2, 2, 2] : [2, 2, 2, 2, 3, 3];
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
                    // Limit shuffled vanilla palace style to 2 item rooms max.
                    // More than that often caused errors when generating.
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
                properties.PalaceItemRoomCounts = Enumerable.Repeat((int)palaceItemRoomCount, 6).ToArray();
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
    }

    /// Let the user know when their combination of flags will not be
    /// possible to achieve.
    /// 
    /// For indeterminate options, we will only check that the best case
    /// scenario should work.
    public void CheckForFlagConflicts()
    {
        int requiredMinorItemReplacements = 0;
        if ((startingHeartContainersMax ?? 8) < 4)
        {
            requiredMinorItemReplacements = 4 - (startingHeartContainersMax ?? 4);
        }
        if (palaceItemRoomCount == PalaceItemRoomCount.ZERO)
        {
            requiredMinorItemReplacements += 6;
        }
        if (CountPossibleMinorItems() < requiredMinorItemReplacements)
        {
            throw new UserFacingException("Impossible Item Flags", "Not enough possible item locations for removed palace items.\n\nAdd more starting items or more palace items.");
        }

        if (noDuplicateRoomsByLayout || noDuplicateRoomsByEnemies)
        {
            // if current palace generation logic changes, this should be updated
            int potentialRoomPools = 0;
            if (includeVanillaRooms != false) { potentialRoomPools++; }
            if (includev4_0Rooms != false) { potentialRoomPools++; }
            if (includev4_4Rooms != false) { potentialRoomPools++; }
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
        return useCustomRooms ? "CustomRooms.json" : "PalaceRooms.json";
    }

    private bool GetIndeterminateFlagValue(Random r)
    {
        return r.NextDouble() < indeterminateOptionRate switch
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
            Collectable.SHIELD_SPELL => startWithShield,
            Collectable.JUMP_SPELL => startWithJump,
            Collectable.LIFE_SPELL => startWithLife,
            Collectable.FAIRY_SPELL => startWithFairy,
            Collectable.FIRE_SPELL => startWithFire,
            Collectable.DASH_SPELL => startWithFire,
            Collectable.REFLECT_SPELL => startWithReflect,
            Collectable.SPELL_SPELL => startWithSpellSpell,
            Collectable.THUNDER_SPELL => startWithThunder,
            Collectable.CANDLE => startWithCandle,
            Collectable.GLOVE => startWithGlove,
            Collectable.RAFT => startWithRaft,
            Collectable.BOOTS => startWithBoots,
            Collectable.FLUTE => startWithFlute,
            Collectable.CROSS => startWithCross,
            Collectable.HAMMER => startWithHammer,
            Collectable.MAGIC_KEY => startWithMagicKey,
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

        hardStartItemsCount += shuffleStartingItems || startWithCandle ? 1 : 0;
        hardStartItemsCount += shuffleStartingItems || startWithBoots ? 1 : 0;
        hardStartItemsCount += shuffleStartingItems || startWithCross ? 1 : 0;
        hardStartItemsCount += shuffleStartingItems || startWithFlute ? 1 : 0;
        hardStartItemsCount += shuffleStartingItems || startWithGlove ? 1 : 0;
        hardStartItemsCount += shuffleStartingItems || startWithHammer ? 1 : 0;
        hardStartItemsCount += shuffleStartingItems || startWithMagicKey ? 1 : 0;
        hardStartItemsCount += shuffleStartingItems || startWithRaft ? 1 : 0;

        count += Math.Max(hardStartItemsCount, shuffleStartingItems ? startItemsLimit.AsInt() : 0);

        if(includeSpellsInShuffle ?? true)
        {
            hardStartItemsCount = 0;
            hardStartItemsCount += shuffleStartingSpells || startWithShield ? 1 : 0;
            hardStartItemsCount += shuffleStartingSpells || startWithJump ? 1 : 0;
            hardStartItemsCount += shuffleStartingSpells || startWithLife ? 1 : 0;
            hardStartItemsCount += shuffleStartingSpells || startWithFairy ? 1 : 0;
            hardStartItemsCount += shuffleStartingSpells || startWithFire ? 1 : 0;
            hardStartItemsCount += shuffleStartingSpells || startWithReflect ? 1 : 0;
            hardStartItemsCount += shuffleStartingSpells || startWithSpellSpell ? 1 : 0;
            hardStartItemsCount += shuffleStartingSpells || startWithThunder ? 1 : 0;

            count += Math.Max(hardStartItemsCount, shuffleStartingItems ? startItemsLimit.AsInt() : 0);
        }

        if(includeSwordTechsInShuffle ?? true)
        {
            hardStartItemsCount += startingTechniques switch
            {
                StartingTechs.DOWNSTAB => 1,
                StartingTechs.UPSTAB => 1,
                StartingTechs.BOTH => 2,
                StartingTechs.RANDOM => 2,
                StartingTechs.NONE => 0,
                _ => throw new Exception("Unrecognized starting tech option")
            };
        }

        int containerReplacementSmallItemsCount = maxHeartContainers switch
        {
            MaxHeartsOption.EIGHT => 4 - (8 - (startingHeartContainersMax ?? 8)),
            MaxHeartsOption.SEVEN => 4 - (7 - (startingHeartContainersMax ?? 8)),
            MaxHeartsOption.SIX => 4 - (6 - (startingHeartContainersMax ?? 8)),
            MaxHeartsOption.FIVE => 4 - (5 - (startingHeartContainersMax ?? 8)),
            MaxHeartsOption.FOUR => 4 - (4 - (startingHeartContainersMax ?? 8)),
            MaxHeartsOption.THREE => 4 - (3 - (startingHeartContainersMax ?? 8)),
            MaxHeartsOption.TWO => 4 - (2 - (startingHeartContainersMax ?? 8)),
            MaxHeartsOption.ONE => 4 - (1 - (startingHeartContainersMax ?? 8)),
            MaxHeartsOption.PLUS_ONE => 3,
            MaxHeartsOption.PLUS_TWO => 2,
            MaxHeartsOption.PLUS_THREE => 1,
            MaxHeartsOption.PLUS_FOUR => 0,
            MaxHeartsOption.RANDOM => 4 - (startingHeartContainersMax ?? 1),
            _ => throw new Exception("Unrecognized Max Hearts in CountPossibleMinorItems")
        };

        count += containerReplacementSmallItemsCount;

        return count;
    }

    private int GetDarkLinkMinDistance()
    {
        if (darkLinkMinDistance == BossRoomMinDistance.MAX)
        {
            // limiting here based on how long it takes to generate the seeds
            if (gpStyle == PalaceStyle.RECONSTRUCTED) { return 16; }
            if (shortenGP != false) { return 20; }
            return 24;
        }
        else
        {
            return (int)darkLinkMinDistance;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
