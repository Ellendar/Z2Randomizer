using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Z2Randomizer.Core.Flags;
using Z2Randomizer.Core.Overworld;
using RandomizerCore.Flags;

namespace Z2Randomizer.Core;

public class RandomizerConfiguration : INotifyPropertyChanged
{
    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    private int seed;
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
    private bool startWithShield;
    private bool startWithJump;
    private bool startWithLife;
    private bool startWithFairy;
    private bool startWithFire;
    private bool startWithReflect;
    private bool startWithSpell;
    private bool startWithThunder;
    private int? startingHeartContainersMin;
    private int? startingHeartContainersMax;
    private int? maxHeartContainers;
    private bool? startWithUpstab;
    private bool? startWithDownstab;
    private int? startingLives;
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
    private bool allowConnectionCavesToBeBoulderBlocked;
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
    private bool? bossRoomsExitToPalace;
    private bool? birdRequired;
    private bool removeTBird;
    private bool restartAtPalacesOnGameOver;
    private bool changePalacePallettes;
    private bool randomizeBossItemDrop;
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
    private StatEffectiveness attackEffectiveness;
    private StatEffectiveness magicEffectiveness;
    private StatEffectiveness lifeEffectiveness;
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
    private StatEffectiveness enemyXpDrops;
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
    private byte beepFrequency;
    private byte beepThreshold;
    private bool disableMusic;
    private bool fastSpellCasting;
    private bool upAOnController1;
    private bool removeFlashing;
    private int sprite;
    private string tunic;
    private string shieldTunic;
    private string beamSprite;
    private bool useCustomRooms;
    private bool disableHudLag;
    private bool randomizeKnockback;
    
    //Meta
    [IgnoreInFlags]
    public int Seed { get => seed; set => SetField(ref seed, value); }

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
    public bool StartWithShield { get => startWithShield; set => SetField(ref startWithShield, value); }
    public bool StartWithJump { get => startWithJump; set => SetField(ref startWithJump, value); }
    public bool StartWithLife { get => startWithLife; set => SetField(ref startWithLife, value); }
    public bool StartWithFairy { get => startWithFairy; set => SetField(ref startWithFairy, value); }
    public bool StartWithFire { get => startWithFire; set => SetField(ref startWithFire, value); }
    public bool StartWithReflect { get => startWithReflect; set => SetField(ref startWithReflect, value); }
    public bool StartWithSpell { get => startWithSpell; set => SetField(ref startWithSpell, value); }
    public bool StartWithThunder { get => startWithThunder; set => SetField(ref startWithThunder, value); }

    [Limit(8)]
    [Minimum(1)]
    public int? StartingHeartContainersMin { get => startingHeartContainersMin; set => SetField(ref startingHeartContainersMin, value); }
    
    [Limit(8)]
    [Minimum(1)]
    public int? StartingHeartContainersMax { get => startingHeartContainersMax; set => SetField(ref startingHeartContainersMax, value); }

    [Limit(11)]
    [Minimum(1)]
    public int? MaxHeartContainers { get => maxHeartContainers; set => SetField(ref maxHeartContainers, value); }

    public bool? StartWithUpstab { get => startWithUpstab; set => SetField(ref startWithUpstab, value); }
    public bool? StartWithDownstab { get => startWithDownstab; set => SetField(ref startWithDownstab, value); }

    [CustomFlagSerializer(typeof(StartingLivesSerializer))]
    public int? StartingLives { get => startingLives; set => SetField(ref startingLives, value); }

    [Limit(8)]
    [Minimum(1)]
    public int StartingAttackLevel { get => startingAttackLevel; set => SetField(ref startingAttackLevel, value); }

    [Limit(8)]
    [Minimum(1)]
    public int StartingMagicLevel { get => startingMagicLevel; set => SetField(ref startingMagicLevel, value); }

    [Limit(8)]
    [Minimum(1)]
    public int StartingLifeLevel { get => startingLifeLevel; set => SetField(ref startingLifeLevel, value); }


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

    //Sane caves
    public bool? RestrictConnectionCaveShuffle
    {
        get => restrictConnectionCaveShuffle;
        set => SetField(ref restrictConnectionCaveShuffle, value);
    }

    public bool AllowConnectionCavesToBeBoulderBlocked
    {
        get => allowConnectionCavesToBeBoulderBlocked;
        set => SetField(ref allowConnectionCavesToBeBoulderBlocked, value);
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

    //public bool? IncludeCommunityRooms { get; set; }
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

    public bool? BossRoomsExitToPalace
    {
        get => bossRoomsExitToPalace;
        set => SetField(ref bossRoomsExitToPalace, value);
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

    public StatEffectiveness AttackEffectiveness
    {
        get => attackEffectiveness;
        set => SetField(ref attackEffectiveness, value);
    }

    public StatEffectiveness MagicEffectiveness
    {
        get => magicEffectiveness;
        set => SetField(ref magicEffectiveness, value);
    }

    public StatEffectiveness LifeEffectiveness
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

    public StatEffectiveness EnemyXPDrops
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
    public byte BeepFrequency
    {
        get => beepFrequency;
        set => SetField(ref beepFrequency, value);
    }

    [IgnoreInFlags]
    public byte BeepThreshold
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
    public int Sprite
    {
        get => sprite;
        set => SetField(ref sprite, value);
    }

    [IgnoreInFlags]
    public string Tunic
    {
        get => tunic;
        set => SetField(ref tunic, value);
    }

    [IgnoreInFlags]
    public string ShieldTunic
    {
        get => shieldTunic;
        set => SetField(ref shieldTunic, value);
    }

    [IgnoreInFlags]
    public string BeamSprite
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
        DisableHUDLag = false;
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
            if(Attribute.IsDefined(property, typeof(CustomFlagSerializerAttribute)))
            {
                CustomFlagSerializerAttribute attribute = (CustomFlagSerializerAttribute)property.GetCustomAttribute(typeof(CustomFlagSerializerAttribute));
                IFlagSerializer serializer = (IFlagSerializer)Activator.CreateInstance(attribute.Type);
                property.SetValue(this, serializer.Deserialize(flagReader.ReadInt(serializer.GetLimit())));
            }
            else if (propertyType == typeof(bool))
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
        config.StartingLives = bits[3] ? 7 : 3;
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
        bool ShortGP = bits[4];
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
        config.UseCommunityText = bits[4];
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
                config.WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE;
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
                config.DMBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE;
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
                config.EastBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE;
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
                config.NormalPalaceStyle = PalaceStyle.VANILLA;
                config.GPStyle = PalaceStyle.VANILLA;
                break;
            case 1:
                config.NormalPalaceStyle = PalaceStyle.SHUFFLED;
                config.GPStyle = PalaceStyle.SHUFFLED;
                break;
            case 2:
                config.NormalPalaceStyle = PalaceStyle.RECONSTRUCTED;
                config.GPStyle = ShortGP ? PalaceStyle.RECONSTRUCTED_SHORTENED : PalaceStyle.RECONSTRUCTED;
                break;
        }
        config.IncludeVanillaRooms = true;
        config.Includev4_0Rooms = bits[2];
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
        config.HardBosses = false;
        config.RandomizeKnockback = false;
        config.DisableHUDLag = false;


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
            if (Attribute.IsDefined(property, typeof(CustomFlagSerializerAttribute)))
            {
                CustomFlagSerializerAttribute attribute = (CustomFlagSerializerAttribute)property.GetCustomAttribute(typeof(CustomFlagSerializerAttribute));
                IFlagSerializer serializer = (IFlagSerializer)Activator.CreateInstance(attribute.Type);
                flags.Append(serializer.Serialize(property.GetValue(this, null)), serializer.GetLimit());
            }
            else if (propertyType == typeof(bool))
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
                limit = Enum.GetValues(propertyType).Length;
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
        properties.Seed = Seed;

        //Start Configuration
        properties.StartCandle = !StartWithCandle && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithCandle;
        properties.StartGlove = !StartWithGlove && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithGlove;
        properties.StartRaft = !StartWithRaft && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithRaft;
        properties.StartBoots = !StartWithBoots && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithBoots;
        properties.StartFlute = !StartWithFlute && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithFlute;
        properties.StartCross = !StartWithCross && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithCross;
        properties.StartHammer = !StartWithHammer && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithHammer;
        properties.StartKey = !StartWithMagicKey && ShuffleStartingItems ? random.NextDouble() > .75 : StartWithMagicKey;

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
            properties.StartWithDownstab = StartWithDownstab ?? random.Next(7) >= 5;
            properties.StartWithUpstab = StartWithUpstab ?? random.Next(7) >= 5;
        }
        properties.SwapUpAndDownStab = SwapUpAndDownStab ?? random.Next(2) == 1;


        properties.StartLives = StartingLives ?? random.Next(2, 6);
        properties.PermanentBeam = PermanmentBeamSword;
        properties.UseCommunityText = UseCommunityText;
        properties.StartAtk = StartingAttackLevel;
        properties.StartMag = StartingMagicLevel;
        properties.StartLifeLvl = StartingMagicLevel;

        //Overworld
        properties.ShuffleEncounters = ShuffleEncounters ?? random.Next(2) == 1;
        properties.AllowPathEnemies = AllowUnsafePathEncounters;
        properties.IncludeLavaInEncounterShuffle = IncludeLavaInEncounterShuffle;
        properties.SwapPalaceCont = PalacesCanSwapContinents ?? random.Next(2) == 1;
        properties.P7shuffle = ShuffleGP ?? random.Next(2) == 1;
        properties.HiddenPalace = HidePalace ?? random.Next(2) == 1;
        properties.HiddenKasuto = HideKasuto ?? random.Next(2) == 1;

        properties.EncounterRate = EncounterRate;
        properties.ContinentConnections = ContinentConnectionType;
        properties.BoulderBlockConnections = AllowConnectionCavesToBeBoulderBlocked;
        if (WestBiome == Biome.RANDOM || WestBiome == Biome.RANDOM_NO_VANILLA || WestBiome == Biome.RANDOM_NO_VANILLA_OR_SHUFFLE)
        {
            int shuffleLimit = WestBiome switch { Biome.RANDOM => 7, Biome.RANDOM_NO_VANILLA => 6, Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => 5 };
            properties.WestBiome = random.Next(shuffleLimit) switch
            {
                0 => Biome.VANILLALIKE,
                1 => Biome.ISLANDS,
                2 => random.Next(2) == 1 ? Biome.CANYON : Biome.DRY_CANYON,
                3 => Biome.CALDERA,
                4 => Biome.MOUNTAINOUS,
                5 => Biome.VANILLA_SHUFFLE,
                6 => Biome.VANILLA,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else if(WestBiome == Biome.CANYON)
        {
            properties.WestBiome = random.Next(2) == 0 ? Biome.CANYON : Biome.DRY_CANYON;
        }
        else {
            properties.WestBiome = WestBiome;
        }
        if (EastBiome == Biome.RANDOM || EastBiome == Biome.RANDOM_NO_VANILLA || EastBiome == Biome.RANDOM_NO_VANILLA_OR_SHUFFLE)
        {
            int shuffleLimit = EastBiome switch { Biome.RANDOM => 7, Biome.RANDOM_NO_VANILLA => 6, Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => 5 };
            properties.EastBiome = random.Next(shuffleLimit) switch
            {
                0 => Biome.VANILLALIKE,
                1 => Biome.ISLANDS,
                2 => random.Next(2) == 1 ? Biome.CANYON : Biome.DRY_CANYON,
                3 => Biome.VOLCANO,
                4 => Biome.MOUNTAINOUS,
                5 => Biome.VANILLA_SHUFFLE,
                6 => Biome.VANILLA,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else if (EastBiome == Biome.CANYON)
        {
            properties.EastBiome = random.Next(2) == 0 ? Biome.CANYON : Biome.DRY_CANYON;
        }
        else
        {
            properties.EastBiome = EastBiome;
        }
        if (DMBiome == Biome.RANDOM || DMBiome == Biome.RANDOM_NO_VANILLA || DMBiome == Biome.RANDOM_NO_VANILLA_OR_SHUFFLE)
        {
            int shuffleLimit = DMBiome switch { Biome.RANDOM => 7, Biome.RANDOM_NO_VANILLA => 6, Biome.RANDOM_NO_VANILLA_OR_SHUFFLE => 5 };
            properties.DmBiome = random.Next(shuffleLimit) switch
            {
                0 => Biome.VANILLALIKE,
                1 => Biome.ISLANDS,
                2 => random.Next(2) == 1 ? Biome.CANYON : Biome.DRY_CANYON,
                3 => Biome.CALDERA,
                4 => Biome.MOUNTAINOUS,
                5 => Biome.VANILLA_SHUFFLE,
                6 => Biome.VANILLA,
                _ => throw new Exception("Invalid Biome")
            };
        }
        else if (DMBiome == Biome.CANYON)
        {
            properties.DmBiome = random.Next(2) == 0 ? Biome.CANYON : Biome.DRY_CANYON;
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
        if (Climate == null)
        {
            properties.Climate = random.Next(5) switch
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
        properties.ShuffleHidden = ShuffleWhichLocationIsHidden ?? random.Next(2) == 1;
        properties.CanWalkOnWaterWithBoots = GoodBoots ?? random.Next(2) == 1;
        properties.BagusWoods = GenerateBaguWoods ?? random.Next(2) == 1;

        //Palaces
        if (GPStyle == PalaceStyle.RANDOM)
        {
            properties.GPStyle = random.Next(4) switch
            {
                0 => PalaceStyle.VANILLA,
                1 => PalaceStyle.SHUFFLED,
                2 => PalaceStyle.RECONSTRUCTED,
                3 => PalaceStyle.RECONSTRUCTED_SHORTENED,
                _ => throw new Exception("Invalid PalaceStyle")
            };
        }
        else if (GPStyle == PalaceStyle.RECONSTRUCTED_RANDOM_LENGTH)
        {
            properties.GPStyle = random.Next(2) == 0 ? PalaceStyle.RECONSTRUCTED : PalaceStyle.RECONSTRUCTED_SHORTENED;
        }
        else 
        {
            properties.GPStyle = GPStyle;
        }

        if (NormalPalaceStyle == PalaceStyle.RANDOM)
        {
            properties.NormalPalaceStyle = random.Next(3) switch
            {
                0 => PalaceStyle.VANILLA,
                1 => PalaceStyle.SHUFFLED,
                2 => PalaceStyle.RECONSTRUCTED,
                _ => throw new Exception("Invalid PalaceStyle")
            };
        }
        else
        {
            properties.NormalPalaceStyle = NormalPalaceStyle;
        }

        properties.StartGems = random.Next(PalacesToCompleteMin, PalacesToCompleteMax + 1);
        properties.RequireTbird = TBirdRequired ?? random.Next(2) == 1;
        properties.ShufflePalacePalettes = ChangePalacePallettes;
        properties.UpARestartsAtPalaces = RestartAtPalacesOnGameOver;
        properties.RemoveTbird = RemoveTBird;
        properties.BossItem = RandomizeBossItemDrop;

        //if all 3 room options are hard false, the seed can't generate. The UI tries to prevent this, but as a safety
        //if we get to this point, use vanilla rooms
        if(!((IncludeVanillaRooms ?? true) || (Includev4_0Rooms ?? true) || (Includev4_4Rooms ?? true)))
        {
            properties.AllowVanillaRooms = true;
        }
        while (!(properties.AllowVanillaRooms || properties.AllowV4Rooms || properties.AllowV4_4Rooms)) {
            properties.AllowVanillaRooms = IncludeVanillaRooms ?? random.Next(2) == 1;
            properties.AllowV4Rooms = Includev4_0Rooms ?? random.Next(2) == 1;
            //temporarily, so we can test rooms, UseCustomRooms automatically turn on v4_4 since there's no toggle
            properties.AllowV4_4Rooms = UseCustomRooms;
            //properties.AllowV4_4Rooms = Includev4_4Rooms == null ? random.Next(2) == 1 : (bool)IncludeVanillaRooms;
        }

        properties.BlockersAnywhere = BlockingRoomsInAnyPalace;
        properties.BossRoomConnect = BossRoomsExitToPalace ?? random.Next(2) == 1;
        properties.NoDuplicateRooms = NoDuplicateRoomsByEnemies == null ? random.Next(2) == 1 : (bool)NoDuplicateRoomsByEnemies;
        properties.NoDuplicateRoomsBySideview = NoDuplicateRoomsByLayout == null ? random.Next(2) == 1 : (bool)NoDuplicateRoomsByLayout;
        properties.GeneratorsAlwaysMatch = GeneratorsAlwaysMatch;
        properties.HardBosses = HardBosses;

        //Enemies
        properties.ShuffleEnemyHP = ShuffleEnemyHP;
        properties.ShuffleEnemyStealExp = ShuffleXPStealers;
        properties.ShuffleStealExpAmt = ShuffleXPStolenAmount;
        properties.ShuffleSwordImmunity = ShuffleSwordImmunity;
        properties.ShuffleOverworldEnemies = ShuffleOverworldEnemies ?? random.Next(2) == 1;
        properties.ShufflePalaceEnemies = ShufflePalaceEnemies ?? random.Next(2) == 1;
        properties.MixLargeAndSmallEnemies = MixLargeAndSmallEnemies ?? random.Next(2) == 1;
        properties.ShuffleDripper = ShuffleDripperEnemy;
        properties.ShuffleEnemyPalettes = ShuffleSpritePalettes;
        properties.ExpLevel = EnemyXPDrops;

        //Levels
        properties.ShuffleAtkExp = ShuffleAttackExperience;
        properties.ShuffleMagicExp = ShuffleMagicExperience;
        properties.ShuffleLifeExp = ShuffleLifeExperience;
        if (AttackEffectiveness == StatEffectiveness.NONE)
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
        if (MagicEffectiveness == StatEffectiveness.NONE)
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
        if (LifeEffectiveness == StatEffectiveness.NONE)
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
        properties.ShuffleSpellLocations = ShuffleSpellLocations ?? random.Next(2) == 1;
        properties.DisableMagicRecs = DisableMagicContainerRequirements ?? random.Next(2) == 1;
        properties.AttackCap = AttackLevelCap;
        properties.MagicCap = MagicLevelCap;
        properties.LifeCap = LifeLevelCap;
        properties.ScaleLevels = ScaleLevelRequirementsToCap;
        properties.HideLessImportantLocations = HideLessImportantLocations ?? random.Next(2) == 1;
        properties.SaneCaves = RestrictConnectionCaveShuffle ?? random.Next(2) == 1;
        properties.SpellEnemy = RandomizeSpellSpellEnemy ?? random.Next(2) == 1;

        //Items
        properties.ShuffleOverworldItems = ShuffleOverworldItems ?? random.Next(2) == 1;
        properties.ShufflePalaceItems = ShufflePalaceItems ?? random.Next(2) == 1;
        properties.MixOverworldPalaceItems = MixOverworldAndPalaceItems ?? random.Next(2) == 1;
        properties.ShuffleSmallItems = ShuffleSmallItems;
        properties.ExtraKeys = PalacesContainExtraKeys ?? random.Next(2) == 1;
        properties.KasutoJars = RandomizeNewKasutoJarRequirements;
        properties.PbagItemShuffle = IncludePBagCavesInItemShuffle ?? random.Next(2) == 1;
        properties.StartWithSpellItems = RemoveSpellItems ?? random.Next(2) == 1;
        properties.ShufflePbagXp = ShufflePBagAmounts ?? random.Next(2) == 1;
        properties.IncludeQuestItemsInShuffle = IncludeQuestItemsInShuffle ?? random.Next(2) == 1;
        properties.IncludeSpellsInShuffle = IncludeSpellsInShuffle ?? random.Next(2) == 1;
        properties.IncludeSwordTechsInShuffle = IncludeSwordTechsInShuffle ?? random.Next(2) == 1;

        //Drops
        properties.ShuffleItemDropFrequency = ShuffleItemDropFrequency;
        if (properties is not
            {
                Smallbluejar: false, Smallredjar: false, Small50: false, Small100: false, Small200: false,
                Small500: false, Small1up: false, Smallkey: false
            })
        {
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
            } while (properties is { Smallbluejar: false, Smallredjar: false, Small50: false, Small100: false, Small200: false, Small500: false, Small1up: false, Smallkey: false });
        }
        if (properties is not
            {
                Largebluejar: false, Largeredjar: false, Large50: false, Large100: false, Large200: false, Large500: false, Large1up: false, Largekey: false
            })
        {
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
            } while (properties is { Largebluejar: false, Largeredjar: false, Large50: false, Large100: false, Large200: false, Large500: false, Large1up: false, Largekey: false });
        }
        properties.StandardizeDrops = StandardizeDrops;

        //Hints
        properties.SpellItemHints = EnableSpellItemHints ?? random.Next(2) == 1;
        properties.HelpfulHints = EnableHelpfulHints ?? random.Next(2) == 1;
        properties.TownNameHints = EnableTownNameHints ?? random.Next(2) == 1;

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
        if(properties.GPStyle == PalaceStyle.VANILLA)
        {
            properties.RemoveTbird = false;
        }

        if (!properties.SwapPalaceCont)
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

        if (properties.NormalPalaceStyle is PalaceStyle.VANILLA or PalaceStyle.SHUFFLED)
        {
            properties.AllowV4Rooms = false;
            properties.AllowV4_4Rooms = false;
            properties.BlockersAnywhere = false;
            properties.BossRoomConnect = false;
        }

        if (properties.GPStyle == PalaceStyle.VANILLA)
        {
            properties.RequireTbird = true;
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

        //Non-reconstructed is incompatable with no duplicate rooms.
        //Also, if community rooms is off, vanilla doesn't contain enough non-duplciate rooms to properly cover the number
        //of required rooms, often even in short GP.
        if (!properties.NormalPalaceStyle.IsReconstructed() || properties is { AllowV4Rooms: false, AllowV4_4Rooms: false })
        {
            properties.NoDuplicateRooms = false;
            properties.NoDuplicateRoomsBySideview = false;
        }

        // string debug = JsonSerializer.Serialize(properties);
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
