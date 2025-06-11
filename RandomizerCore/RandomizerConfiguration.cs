using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using NLog;
using Z2Randomizer.RandomizerCore.Flags;
using System.ComponentModel.DataAnnotations;
using Z2Randomizer.RandomizerCore.Overworld;

namespace Z2Randomizer.RandomizerCore;

public sealed class RandomizerConfiguration : INotifyPropertyChanged
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

    private string? seed;
    
    private bool shuffleStartingItems;
    private bool startWithCandle;
    private bool startWithGlove;
    private bool startWithRaft;
    private bool startWithBoots;
    private bool startWithFlute;
    private bool startWithCross;
    private bool startWithHammer;
    private bool startWithMagicKey;
    private bool shuffleStartingSpells;
    private StartingResourceLimit startItemsLimit;
    private bool startWithShield;
    private bool startWithJump;
    private bool startWithLife;
    private bool startWithFairy;
    private bool startWithFire;
    private bool startWithReflect;
    private bool startWithSpellSpell;
    private bool startWithThunder;
    private StartingResourceLimit startSpellsLimit;
    private int? startingHeartContainersMin;
    private int? startingHeartContainersMax;
    private StartingHeartsMaxOption maxHeartContainers;
    private StartingTechs startingTechs;
    private StartingLives startingLives;
    private int startingAttackLevel;
    private int startingMagicLevel;
    private int startingLifeLevel;
    private bool? palacesCanSwapContinents;
    private bool? shuffleGP;
    private bool? shuffleEncounters;
    private bool allowUnsafePathEncounters;
    private bool includeLavaInEncounterShuffle;
    private EncounterRate encounterRate;
    private bool? hidePalace;
    private bool? hideKasuto;
    private bool? shuffleWhichLocationIsHidden;
    private bool? hideLessImportantLocations;
    private bool? restrictConnectionCaveShuffle;
    private bool allowConnectionCavesToBeBlocked;
    private bool? goodBoots;
    private bool? generateBaguWoods;
    private ContinentConnectionType continentConnectionType;
    private Biome westBiome;
    private Biome eastBiome;
    private Biome dmBiome1;
    private Biome mazeBiome;
    private Climate climate;
    private bool vanillaShuffleUsesActualTerrain;
    private PalaceStyle normalPalaceStyle;
    private PalaceStyle gpStyle;
    private bool? includeVanillaRooms;
    private bool? includev40Rooms;
    private bool? includev44Rooms;
    private bool blockingRoomsInAnyPalace;
    private BossRoomsExitType bossRoomsExitType;
    private bool? birdRequired;
    private bool removeTBird;
    private bool restartAtPalacesOnGameOver;
    private bool? global5050JarDrop = false;
    private bool reduceDripperVariance = false;
    private bool changePalacePallettes;
    private bool randomizeBossItemDrop;
    private PalaceItemRoomCount palaceItemRoomCount;
    private int palacesToCompleteMin;
    private int palacesToCompleteMax;
    private bool noDuplicateRoomsByLayout;
    private bool noDuplicateRoomsByEnemies;
    private bool generatorsAlwaysMatch;
    private bool hardBosses;
    private bool shuffleAttackExperience;
    private bool shuffleMagicExperience;
    private bool shuffleLifeExperience;
    private int attackLevelCap;
    private int magicLevelCap;
    private int lifeLevelCap;
    private bool scaleLevelRequirementsToCap;
    private AttackEffectiveness attackEffectiveness;
    private MagicEffectiveness magicEffectiveness;
    private LifeEffectiveness lifeEffectiveness;
    private bool shuffleLifeRefillAmount;
    private bool? shuffleSpellLocations;
    private bool? disableMagicContainerRequirements;
    private bool? randomizeSpellSpellEnemy;
    private bool? swapUpAndDownStab;
    private FireOption fireOption;
    private bool? shuffleOverworldEnemies;
    private bool? shufflePalaceEnemies;
    private bool shuffleDripperEnemy;
    private bool? mixLargeAndSmallEnemies;
    private bool shuffleEnemyHp;
    private bool shuffleXpStealers;
    private bool shuffleXpStolenAmount;
    private bool shuffleSwordImmunity;
    private XPEffectiveness enemyXpDrops;
    private bool? shufflePalaceItems;
    private bool? shuffleOverworldItems;
    private bool? mixOverworldAndPalaceItems;
    private bool? includePBagCavesInItemShuffle;
    private bool shuffleSmallItems;
    private bool? palacesContainExtraKeys;
    private bool randomizeNewKasutoJarRequirements;
    private bool? removeSpellItems;
    private bool? shufflePBagAmounts;
    private bool? includeSpellsInShuffle;
    private bool? includeSwordTechsInShuffle;
    private bool? includeQuestItemsInShuffle;
    private bool shuffleItemDropFrequency;
    private bool randomizeDrops;
    private bool standardizeDrops;
    private bool smallEnemiesCanDropBlueJar;
    private bool smallEnemiesCanDropRedJar;
    private bool smallEnemiesCanDropSmallBag;
    private bool smallEnemiesCanDropMediumBag;
    private bool smallEnemiesCanDropLargeBag;
    private bool smallEnemiesCanDropXlBag;
    private bool smallEnemiesCanDrop1Up;
    private bool smallEnemiesCanDropKey;
    private bool largeEnemiesCanDropBlueJar;
    private bool largeEnemiesCanDropRedJar;
    private bool largeEnemiesCanDropSmallBag;
    private bool largeEnemiesCanDropMediumBag;
    private bool largeEnemiesCanDropLargeBag;
    private bool largeEnemiesCanDropXlBag;
    private bool largeEnemiesCanDrop1Up;
    private bool largeEnemiesCanDropKey;
    private bool? enableHelpfulHints;
    private bool? enableSpellItemHints;
    private bool? enableTownNameHints;
    private bool jumpAlwaysOn;
    private bool dashAlwaysOn;
    private bool shuffleSpritePalettes;
    private bool permanmentBeamSword;
    private bool useCommunityText;
    private BeepFrequency beepFrequency;
    private BeepThreshold beepThreshold;
    private bool disableMusic;
    private bool randomizeMusic;
    private bool mixCustomAndOriginalMusic;
    private bool disableUnsafeMusic;
    private bool fastSpellCasting;
    private bool upAOnController1;
    private bool removeFlashing;
    private CharacterSprite sprite;
    private string spriteName;
    private bool sanitizeSprite;
    private bool changeItemSprites;
    private CharacterColor tunic;
    private CharacterColor tunicOutline;
    private CharacterColor shieldTunic;
    private BeamSprites beamSprite;
    private bool useCustomRooms;
    private bool disableHudLag;
    private bool randomizeKnockback;
    private bool? shortenGP;
    private bool? shortenNormalPalaces;
    private IndeterminateOptionRate indeterminateOptionRate;
    private RiverDevilBlockerOption riverDevilBlockerOption;
    private bool? eastRocks;
    private bool generateSpoiler;

    //Meta
    [Required]
    [IgnoreInFlags]
    public string Seed { get => seed ?? ""; set => SetField(ref seed, value); }
    [IgnoreInFlags]
    [JsonIgnore]
    public string Flags
    {
        get => Serialize();
        set {
            ConvertFlags(value?.Trim() ?? "", this);
        }
    }

    //Start Configuration
    public bool ShuffleStartingItems { get => shuffleStartingItems; set => SetField(ref shuffleStartingItems, value); }
    public bool StartWithCandle { get => startWithCandle; set => SetField(ref startWithCandle, value); }
    public bool StartWithGlove { get => startWithGlove; set => SetField(ref startWithGlove, value); }
    public bool StartWithRaft { get => startWithRaft; set => SetField(ref startWithRaft, value); }
    public bool StartWithBoots { get => startWithBoots; set => SetField(ref startWithBoots, value); }
    public bool StartWithFlute { get => startWithFlute; set => SetField(ref startWithFlute, value); }
    public bool StartWithCross { get => startWithCross; set => SetField(ref startWithCross, value); }
    public bool StartWithHammer { get => startWithHammer; set => SetField(ref startWithHammer, value); }
    public bool StartWithMagicKey { get => startWithMagicKey; set => SetField(ref startWithMagicKey, value); }
    
    public bool ShuffleStartingSpells { get => shuffleStartingSpells; set => SetField(ref shuffleStartingSpells, value); }
    public StartingResourceLimit StartItemsLimit { get => startItemsLimit; set => SetField(ref startItemsLimit, value); }
    public bool StartWithShield { get => startWithShield; set => SetField(ref startWithShield, value); }
    public bool StartWithJump { get => startWithJump; set => SetField(ref startWithJump, value); }
    public bool StartWithLife { get => startWithLife; set => SetField(ref startWithLife, value); }
    public bool StartWithFairy { get => startWithFairy; set => SetField(ref startWithFairy, value); }
    public bool StartWithFire { get => startWithFire; set => SetField(ref startWithFire, value); }
    public bool StartWithReflect { get => startWithReflect; set => SetField(ref startWithReflect, value); }
    public bool StartWithSpellSpell { get => startWithSpellSpell; set => SetField(ref startWithSpellSpell, value); }
    public bool StartWithThunder { get => startWithThunder; set => SetField(ref startWithThunder, value); }
    public StartingResourceLimit StartSpellsLimit { get => startSpellsLimit; set => SetField(ref startSpellsLimit, value); }

    [Limit(8)]
    [Minimum(1)]
    public int? StartingHeartContainersMin { get => startingHeartContainersMin; set => SetField(ref startingHeartContainersMin, value); }
    
    [Limit(8)]
    [Minimum(1)]
    public int? StartingHeartContainersMax { get => startingHeartContainersMax; set => SetField(ref startingHeartContainersMax, value); }

    public StartingHeartsMaxOption MaxHeartContainers { get => maxHeartContainers; set => SetField(ref maxHeartContainers, value); }

    public StartingTechs StartingTechniques { get => startingTechs; set => SetField(ref startingTechs, value); }

    public StartingLives StartingLives { get => startingLives; set => SetField(ref startingLives, value); }

    [Limit(8)]
    [Minimum(1)]
    public int StartingAttackLevel { get => startingAttackLevel; set => SetField(ref startingAttackLevel, value); }

    [Limit(8)]
    [Minimum(1)]
    public int StartingMagicLevel { get => startingMagicLevel; set => SetField(ref startingMagicLevel, value); }

    [Limit(8)]
    [Minimum(1)]
    public int StartingLifeLevel { get => startingLifeLevel; set => SetField(ref startingLifeLevel, value); }
    public IndeterminateOptionRate IndeterminateOptionRate
    {
        get => indeterminateOptionRate;
        set => SetField(ref indeterminateOptionRate, value);
    }


    //Overworld
    public bool? PalacesCanSwapContinents { get => palacesCanSwapContinents; set => SetField(ref palacesCanSwapContinents, value); }
    public bool? ShuffleGP { get => shuffleGP; set => SetField(ref shuffleGP, value); }
    public bool? ShuffleEncounters { get => shuffleEncounters; set => SetField(ref shuffleEncounters, value); }

    public bool AllowUnsafePathEncounters
    {
        get => allowUnsafePathEncounters;
        set => SetField(ref allowUnsafePathEncounters, value);
    }

    public bool IncludeLavaInEncounterShuffle
    {
        get => includeLavaInEncounterShuffle;
        set => SetField(ref includeLavaInEncounterShuffle, value);
    }

    public EncounterRate EncounterRate
    {
        get => encounterRate;
        set => SetField(ref encounterRate, value);
    }

    public bool? HidePalace
    {
        get => hidePalace;
        set => SetField(ref hidePalace, value);
    }

    public bool? HideKasuto
    {
        get => hideKasuto;
        set => SetField(ref hideKasuto, value);
    }

    public bool? ShuffleWhichLocationIsHidden
    {
        get => shuffleWhichLocationIsHidden;
        set => SetField(ref shuffleWhichLocationIsHidden, value);
    }

    public bool? HideLessImportantLocations
    {
        get => hideLessImportantLocations;
        set => SetField(ref hideLessImportantLocations, value);
    }

    public RiverDevilBlockerOption RiverDevilBlockerOption
    {
        get => riverDevilBlockerOption;
        set => SetField(ref riverDevilBlockerOption, value);
    }


    //Sane caves
    public bool? RestrictConnectionCaveShuffle
    {
        get => restrictConnectionCaveShuffle;
        set => SetField(ref restrictConnectionCaveShuffle, value);
    }

    public bool? EastRocks
    {
        get => eastRocks;
        set => SetField(ref eastRocks, value);
    }

    public bool AllowConnectionCavesToBeBlocked
    {
        get => allowConnectionCavesToBeBlocked;
        set => SetField(ref allowConnectionCavesToBeBlocked, value);
    }

    public bool? GoodBoots
    {
        get => goodBoots;
        set => SetField(ref goodBoots, value);
    }

    public bool? GenerateBaguWoods
    {
        get => generateBaguWoods;
        set => SetField(ref generateBaguWoods, value);
    }

    public ContinentConnectionType ContinentConnectionType
    {
        get => continentConnectionType;
        set => SetField(ref continentConnectionType, value);
    }

    public Biome WestBiome
    {
        get => westBiome;
        set => SetField(ref westBiome, value);
    }

    public Biome EastBiome
    {
        get => eastBiome;
        set => SetField(ref eastBiome, value);
    }

    public Biome DMBiome
    {
        get => dmBiome1;
        set => SetField(ref dmBiome1, value);
    }

    public Biome MazeBiome
    {
        get => mazeBiome;
        set => SetField(ref mazeBiome, value);
    }

    [CustomFlagSerializer(typeof(ClimateFlagSerializer))]
    public Climate Climate
    {
        get => climate;
        set => SetField(ref climate, value);
    }

    public bool VanillaShuffleUsesActualTerrain
    {
        get => vanillaShuffleUsesActualTerrain;
        set => SetField(ref vanillaShuffleUsesActualTerrain, value);
    }

    //Palaces
    public bool? ShortenGP
    {
        get => shortenGP;
        set => SetField(ref shortenGP, value);
    }
    public bool? ShortenNormalPalaces
    {
        get => shortenNormalPalaces;
        set => SetField(ref shortenNormalPalaces, value);
    }
    public PalaceStyle NormalPalaceStyle
    {
        get => normalPalaceStyle;
        set => SetField(ref normalPalaceStyle, value);
    }

    public PalaceStyle GPStyle
    {
        get => gpStyle;
        set => SetField(ref gpStyle, value);
    }

    public bool? IncludeVanillaRooms
    {
        get => includeVanillaRooms;
        set => SetField(ref includeVanillaRooms, value);
    }

    public bool? Includev4_0Rooms
    {
        get => includev40Rooms;
        set => SetField(ref includev40Rooms, value);
    }

    public bool? Includev4_4Rooms
    {
        get => includev44Rooms;
        set => SetField(ref includev44Rooms, value);
    }

    public bool BlockingRoomsInAnyPalace
    {
        get => blockingRoomsInAnyPalace;
        set => SetField(ref blockingRoomsInAnyPalace, value);
    }

    public BossRoomsExitType BossRoomsExitType
    {
        get => bossRoomsExitType;
        set => SetField(ref bossRoomsExitType, value);
    }

    public bool? TBirdRequired
    {
        get => birdRequired;
        set => SetField(ref birdRequired, value);
    }

    public bool RemoveTBird
    {
        get => removeTBird;
        set => SetField(ref removeTBird, value);
    }

    public bool RestartAtPalacesOnGameOver
    {
        get => restartAtPalacesOnGameOver;
        set => SetField(ref restartAtPalacesOnGameOver, value);
    }

    public bool? Global5050JarDrop
    {
        get => global5050JarDrop;
        set => SetField(ref global5050JarDrop, value);
    }

    public bool ReduceDripperVariance
    {
        get => reduceDripperVariance;
        set => SetField(ref reduceDripperVariance, value);
    }

    public bool ChangePalacePallettes
    {
        get => changePalacePallettes;
        set => SetField(ref changePalacePallettes, value);
    }

    public bool RandomizeBossItemDrop
    {
        get => randomizeBossItemDrop;
        set => SetField(ref randomizeBossItemDrop, value);
    }

    public PalaceItemRoomCount PalaceItemRoomCount
    {
        get => palaceItemRoomCount;
        set => SetField(ref palaceItemRoomCount, value);
    }

    [Limit(7)]
    public int PalacesToCompleteMin
    {
        get => palacesToCompleteMin;
        set => SetField(ref palacesToCompleteMin, value);
    }

    [Limit(7)]
    public int PalacesToCompleteMax
    {
        get => palacesToCompleteMax;
        set => SetField(ref palacesToCompleteMax, value);
    }

    public bool NoDuplicateRoomsByLayout
    {
        get => noDuplicateRoomsByLayout;
        set => SetField(ref noDuplicateRoomsByLayout, value);
    }

    public bool NoDuplicateRoomsByEnemies
    {
        get => noDuplicateRoomsByEnemies;
        set => SetField(ref noDuplicateRoomsByEnemies, value);
    }

    public bool GeneratorsAlwaysMatch
    {
        get => generatorsAlwaysMatch;
        set => SetField(ref generatorsAlwaysMatch, value);
    }

    public bool HardBosses
    {
        get => hardBosses;
        set => SetField(ref hardBosses, value);
    }

    //Levels
    public bool ShuffleAttackExperience
    {
        get => shuffleAttackExperience;
        set => SetField(ref shuffleAttackExperience, value);
    }

    public bool ShuffleMagicExperience
    {
        get => shuffleMagicExperience;
        set => SetField(ref shuffleMagicExperience, value);
    }

    public bool ShuffleLifeExperience
    {
        get => shuffleLifeExperience;
        set => SetField(ref shuffleLifeExperience, value);
    }

    [Limit(8)]
    [Minimum(1)]
    public int AttackLevelCap
    {
        get => attackLevelCap;
        set => SetField(ref attackLevelCap, value);
    }

    [Limit(8)]
    [Minimum(1)]
    public int MagicLevelCap
    {
        get => magicLevelCap;
        set => SetField(ref magicLevelCap, value);
    }

    [Limit(8)]
    [Minimum(1)]
    public int LifeLevelCap
    {
        get => lifeLevelCap;
        set => SetField(ref lifeLevelCap, value);
    }

    public bool ScaleLevelRequirementsToCap
    {
        get => scaleLevelRequirementsToCap;
        set => SetField(ref scaleLevelRequirementsToCap, value);
    }

    public AttackEffectiveness AttackEffectiveness
    {
        get => attackEffectiveness;
        set => SetField(ref attackEffectiveness, value);
    }

    public MagicEffectiveness MagicEffectiveness
    {
        get => magicEffectiveness;
        set => SetField(ref magicEffectiveness, value);
    }

    public LifeEffectiveness LifeEffectiveness
    {
        get => lifeEffectiveness;
        set => SetField(ref lifeEffectiveness, value);
    }

    //Spells
    public bool ShuffleLifeRefillAmount
    {
        get => shuffleLifeRefillAmount;
        set => SetField(ref shuffleLifeRefillAmount, value);
    }

    public bool? ShuffleSpellLocations
    {
        get => shuffleSpellLocations;
        set => SetField(ref shuffleSpellLocations, value);
    }

    public bool? DisableMagicContainerRequirements
    {
        get => disableMagicContainerRequirements;
        set => SetField(ref disableMagicContainerRequirements, value);
    }

    public bool? RandomizeSpellSpellEnemy
    {
        get => randomizeSpellSpellEnemy;
        set => SetField(ref randomizeSpellSpellEnemy, value);
    }

    public bool? SwapUpAndDownStab
    {
        get => swapUpAndDownStab;
        set => SetField(ref swapUpAndDownStab, value);
    }

    public FireOption FireOption
    {
        get => fireOption;
        set => SetField(ref fireOption, value);
    }

    //Enemies
    public bool? ShuffleOverworldEnemies
    {
        get => shuffleOverworldEnemies;
        set => SetField(ref shuffleOverworldEnemies, value);
    }

    public bool? ShufflePalaceEnemies
    {
        get => shufflePalaceEnemies;
        set => SetField(ref shufflePalaceEnemies, value);
    }

    public bool ShuffleDripperEnemy
    {
        get => shuffleDripperEnemy;
        set => SetField(ref shuffleDripperEnemy, value);
    }

    public bool? MixLargeAndSmallEnemies
    {
        get => mixLargeAndSmallEnemies;
        set => SetField(ref mixLargeAndSmallEnemies, value);
    }

    public bool ShuffleEnemyHP
    {
        get => shuffleEnemyHp;
        set => SetField(ref shuffleEnemyHp, value);
    }

    public bool ShuffleXPStealers
    {
        get => shuffleXpStealers;
        set => SetField(ref shuffleXpStealers, value);
    }

    public bool ShuffleXPStolenAmount
    {
        get => shuffleXpStolenAmount;
        set => SetField(ref shuffleXpStolenAmount, value);
    }

    public bool ShuffleSwordImmunity
    {
        get => shuffleSwordImmunity;
        set => SetField(ref shuffleSwordImmunity, value);
    }

    public XPEffectiveness EnemyXPDrops
    {
        get => enemyXpDrops;
        set => SetField(ref enemyXpDrops, value);
    }

    //Items
    public bool? ShufflePalaceItems
    {
        get => shufflePalaceItems;
        set => SetField(ref shufflePalaceItems, value);
    }

    public bool? ShuffleOverworldItems
    {
        get => shuffleOverworldItems;
        set => SetField(ref shuffleOverworldItems, value);
    }

    public bool? MixOverworldAndPalaceItems
    {
        get => mixOverworldAndPalaceItems;
        set => SetField(ref mixOverworldAndPalaceItems, value);
    }

    public bool? IncludePBagCavesInItemShuffle
    {
        get => includePBagCavesInItemShuffle;
        set => SetField(ref includePBagCavesInItemShuffle, value);
    }

    public bool ShuffleSmallItems
    {
        get => shuffleSmallItems;
        set => SetField(ref shuffleSmallItems, value);
    }

    public bool? PalacesContainExtraKeys
    {
        get => palacesContainExtraKeys;
        set => SetField(ref palacesContainExtraKeys, value);
    }

    public bool RandomizeNewKasutoJarRequirements
    {
        get => randomizeNewKasutoJarRequirements;
        set => SetField(ref randomizeNewKasutoJarRequirements, value);
    }

    public bool? RemoveSpellItems
    {
        get => removeSpellItems;
        set => SetField(ref removeSpellItems, value);
    }

    public bool? ShufflePBagAmounts
    {
        get => shufflePBagAmounts;
        set => SetField(ref shufflePBagAmounts, value);
    }

    public bool? IncludeSpellsInShuffle
    {
        get => includeSpellsInShuffle;
        set => SetField(ref includeSpellsInShuffle, value);
    }

    public bool? IncludeSwordTechsInShuffle
    {
        get => includeSwordTechsInShuffle;
        set => SetField(ref includeSwordTechsInShuffle, value);
    }

    //Bagu's note / fountain water / saria mirror
    public bool? IncludeQuestItemsInShuffle
    {
        get => includeQuestItemsInShuffle;
        set => SetField(ref includeQuestItemsInShuffle, value);
    }

    //Drops
    public bool ShuffleItemDropFrequency
    {
        get => shuffleItemDropFrequency;
        set => SetField(ref shuffleItemDropFrequency, value);
    }

    public bool RandomizeDrops
    {
        get => randomizeDrops;
        set => SetField(ref randomizeDrops, value);
    }

    public bool StandardizeDrops
    {
        get => standardizeDrops;
        set => SetField(ref standardizeDrops, value);
    }

    public bool SmallEnemiesCanDropBlueJar
    {
        get => smallEnemiesCanDropBlueJar;
        set => SetField(ref smallEnemiesCanDropBlueJar, value);
    }

    public bool SmallEnemiesCanDropRedJar
    {
        get => smallEnemiesCanDropRedJar;
        set => SetField(ref smallEnemiesCanDropRedJar, value);
    }

    public bool SmallEnemiesCanDropSmallBag
    {
        get => smallEnemiesCanDropSmallBag;
        set => SetField(ref smallEnemiesCanDropSmallBag, value);
    }

    public bool SmallEnemiesCanDropMediumBag
    {
        get => smallEnemiesCanDropMediumBag;
        set => SetField(ref smallEnemiesCanDropMediumBag, value);
    }

    public bool SmallEnemiesCanDropLargeBag
    {
        get => smallEnemiesCanDropLargeBag;
        set => SetField(ref smallEnemiesCanDropLargeBag, value);
    }

    public bool SmallEnemiesCanDropXLBag
    {
        get => smallEnemiesCanDropXlBag;
        set => SetField(ref smallEnemiesCanDropXlBag, value);
    }

    public bool SmallEnemiesCanDrop1up
    {
        get => smallEnemiesCanDrop1Up;
        set => SetField(ref smallEnemiesCanDrop1Up, value);
    }

    public bool SmallEnemiesCanDropKey
    {
        get => smallEnemiesCanDropKey;
        set => SetField(ref smallEnemiesCanDropKey, value);
    }

    public bool LargeEnemiesCanDropBlueJar
    {
        get => largeEnemiesCanDropBlueJar;
        set => SetField(ref largeEnemiesCanDropBlueJar, value);
    }

    public bool LargeEnemiesCanDropRedJar
    {
        get => largeEnemiesCanDropRedJar;
        set => SetField(ref largeEnemiesCanDropRedJar, value);
    }

    public bool LargeEnemiesCanDropSmallBag
    {
        get => largeEnemiesCanDropSmallBag;
        set => SetField(ref largeEnemiesCanDropSmallBag, value);
    }

    public bool LargeEnemiesCanDropMediumBag
    {
        get => largeEnemiesCanDropMediumBag;
        set => SetField(ref largeEnemiesCanDropMediumBag, value);
    }

    public bool LargeEnemiesCanDropLargeBag
    {
        get => largeEnemiesCanDropLargeBag;
        set => SetField(ref largeEnemiesCanDropLargeBag, value);
    }

    public bool LargeEnemiesCanDropXLBag
    {
        get => largeEnemiesCanDropXlBag;
        set => SetField(ref largeEnemiesCanDropXlBag, value);
    }

    public bool LargeEnemiesCanDrop1up
    {
        get => largeEnemiesCanDrop1Up;
        set => SetField(ref largeEnemiesCanDrop1Up, value);
    }

    public bool LargeEnemiesCanDropKey
    {
        get => largeEnemiesCanDropKey;
        set => SetField(ref largeEnemiesCanDropKey, value);
    }

    //Misc
    public bool? EnableHelpfulHints
    {
        get => enableHelpfulHints;
        set => SetField(ref enableHelpfulHints, value);
    }

    public bool? EnableSpellItemHints
    {
        get => enableSpellItemHints;
        set => SetField(ref enableSpellItemHints, value);
    }

    public bool? EnableTownNameHints
    {
        get => enableTownNameHints;
        set => SetField(ref enableTownNameHints, value);
    }

    public bool JumpAlwaysOn
    {
        get => jumpAlwaysOn;
        set => SetField(ref jumpAlwaysOn, value);
    }

    public bool DashAlwaysOn
    {
        get => dashAlwaysOn;
        set => SetField(ref dashAlwaysOn, value);
    }

    public bool ShuffleSpritePalettes
    {
        get => shuffleSpritePalettes;
        set => SetField(ref shuffleSpritePalettes, value);
    }

    public bool PermanmentBeamSword
    {
        get => permanmentBeamSword;
        set => SetField(ref permanmentBeamSword, value);
    }

    //Custom
    [IgnoreInFlags]
    public bool UseCommunityText
    {
        get => useCommunityText;
        set => SetField(ref useCommunityText, value);
    }

    [IgnoreInFlags]
    public BeepFrequency BeepFrequency
    {
        get => beepFrequency;
        set => SetField(ref beepFrequency, value);
    }

    [IgnoreInFlags]
    public BeepThreshold BeepThreshold
    {
        get => beepThreshold;
        set => SetField(ref beepThreshold, value);
    }

    [IgnoreInFlags]
    public bool DisableMusic
    {
        get => disableMusic;
        set => SetField(ref disableMusic, value);
    }

    [IgnoreInFlags]
    public bool RandomizeMusic
    {
        get => randomizeMusic;
        set => SetField(ref randomizeMusic, value);
    }

    [IgnoreInFlags]
    public bool MixCustomAndOriginalMusic
    {
        get => mixCustomAndOriginalMusic;
        set => SetField(ref mixCustomAndOriginalMusic, value);
    }

    [IgnoreInFlags]
    public bool DisableUnsafeMusic
    {
        get => disableUnsafeMusic;
        set => SetField(ref disableUnsafeMusic, value);
    }

    [IgnoreInFlags]
    public bool FastSpellCasting
    {
        get => fastSpellCasting;
        set => SetField(ref fastSpellCasting, value);
    }

    [IgnoreInFlags]
    public bool UpAOnController1
    {
        get => upAOnController1;
        set => SetField(ref upAOnController1, value);
    }

    [IgnoreInFlags]
    public bool RemoveFlashing
    {
        get => removeFlashing;
        set => SetField(ref removeFlashing, value);
    }

    [IgnoreInFlags]
    public CharacterSprite Sprite
    {
        get => sprite;
        set => SetField(ref sprite, value);
    }
    [IgnoreInFlags]
    public string SpriteName
    {
        get => spriteName;
        set => SetField(ref spriteName, value);
    }

    [IgnoreInFlags]
    public bool SanitizeSprite
    {
        get => sanitizeSprite;
        set => SetField(ref sanitizeSprite, value);
    }

    [IgnoreInFlags]
    public bool ChangeItemSprites
    {
        get => changeItemSprites;
        set => SetField(ref changeItemSprites, value);
    }

    [IgnoreInFlags]
    public CharacterColor Tunic
    {
        get => tunic;
        set => SetField(ref tunic, value);
    }

    [IgnoreInFlags]
    public CharacterColor TunicOutline
    {
        get => tunicOutline;
        set => SetField(ref tunicOutline, value);
    }

    [IgnoreInFlags]
    public CharacterColor ShieldTunic
    {
        get => shieldTunic;
        set => SetField(ref shieldTunic, value);
    }

    [IgnoreInFlags]
    public BeamSprites BeamSprite
    {
        get => beamSprite;
        set => SetField(ref beamSprite, value);
    }

    [IgnoreInFlags]
    public bool UseCustomRooms
    {
        get => useCustomRooms;
        set => SetField(ref useCustomRooms, value);
    }

    [IgnoreInFlags]
    public bool DisableHUDLag
    {
        get => disableHudLag;
        set => SetField(ref disableHudLag, value);
    }

    public bool RandomizeKnockback
    {
        get => randomizeKnockback;
        set => SetField(ref randomizeKnockback, value);
    }
    public bool GenerateSpoiler
    {
        get => generateSpoiler;
        set => SetField(ref generateSpoiler, value);
    }

    public RandomizerConfiguration()
    {
        StartingAttackLevel = 1;
        StartingMagicLevel = 1;
        StartingLifeLevel = 1;

        MaxHeartContainers = StartingHeartsMaxOption.EIGHT;
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
        PermanmentBeamSword = false;
        UpAOnController1 = false;
        RemoveFlashing = false;
        Sprite = CharacterSprite.LINK;
        Climate = Climates.Classic;
        //This is a NOP, but it satisfies a quirk in the analyzer
        sprite = Sprite;
        climate = Climate;
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
                logger.Error("Unrecognized configuration property type.");
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
                logger.Error("Unrecognized configuration property type.");
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

        //Start Configuration
        ShuffleStartingCollectables(POSSIBLE_STARTING_ITEMS, StartItemsLimit, ShuffleStartingItems, properties, r);
        ShuffleStartingCollectables(POSSIBLE_STARTING_SPELLS, StartSpellsLimit, ShuffleStartingSpells, properties, r);

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
        if (MaxHeartContainers == StartingHeartsMaxOption.RANDOM)
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
                StartingHeartsMaxOption.PLUS_ONE => 1,
                StartingHeartsMaxOption.PLUS_TWO => 2,
                StartingHeartsMaxOption.PLUS_THREE => 3,
                StartingHeartsMaxOption.PLUS_FOUR => 4,
                _ => throw new ImpossibleException("Invalid heart container max configuration")
            };
            properties.MaxHearts = Math.Min(properties.StartHearts + additionalHearts, 8);
        }
        properties.MaxHearts = Math.Max(properties.MaxHearts, properties.StartHearts);

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
        properties.PermanentBeam = PermanmentBeamSword;
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

        //Palaces

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
        else if(NormalPalaceStyle == PalaceStyle.RANDOM_PER_PALACE)
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
        properties.StartGems = r.Next(PalacesToCompleteMin, PalacesToCompleteMax + 1);
        properties.RequireTbird = TBirdRequired ?? GetIndeterminateFlagValue(r);
        properties.ShufflePalacePalettes = ChangePalacePallettes;
        properties.UpARestartsAtPalaces = RestartAtPalacesOnGameOver;
        properties.Global5050JarDrop = Global5050JarDrop ?? GetIndeterminateFlagValue(r);
        properties.ReduceDripperVariance = ReduceDripperVariance;
        properties.RemoveTbird = RemoveTBird;
        properties.BossItem = RandomizeBossItemDrop;
        properties.PalaceItemRoomCount = PalaceItemRoomCount == PalaceItemRoomCount.RANDOM ? r.Next(3) : (int)PalaceItemRoomCount;

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
        properties.SanitizeSprite = SanitizeSprite;
        properties.ChangeItemSprites = ChangeItemSprites;
        properties.TunicColor = Tunic;
        properties.OutlineColor = TunicOutline;
        properties.ShieldColor = ShieldTunic;
        properties.UpAC1 = UpAOnController1;
        properties.RemoveFlashing = RemoveFlashing;
        properties.UseCustomRooms = UseCustomRooms;
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

        // string debug = JsonSerializer.Serialize(properties);
        return properties;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged(nameof(Flags));
        return true;
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
        int itemLimit = limit switch
        {
            StartingResourceLimit.ONE => 1,
            StartingResourceLimit.TWO => 2,
            StartingResourceLimit.FOUR => 4,
            StartingResourceLimit.NO_LIMIT => 8,
            _ => throw new Exception("Unrecognized StartingResourceLimit in Export")
        };
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
}
