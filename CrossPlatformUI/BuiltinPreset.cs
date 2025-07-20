using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace CrossPlatformUI;

public static class BuiltinPreset
{

    public static readonly RandomizerConfiguration BeginnerPreset = new()
    {
        //Start
        StartWithCandle = true,
        StartWithLife = true,
        MaxHeartContainers = MaxHeartsOption.EIGHT,
        StartingHeartContainersMin = 4,
        StartingHeartContainersMax = 4,
        StartingTechniques = StartingTechs.DOWNSTAB,
        StartingLives = StartingLives.Lives5,
        IndeterminateOptionRate = IndeterminateOptionRate.HALF,

        //Overworld
        PalacesCanSwapContinents = false,
        ShuffleGP = false,
        ShuffleEncounters = false,
        EncounterRate = EncounterRate.HALF,
        EastRocks = false,
        GenerateBaguWoods = false,
        HideLessImportantLocations = true,
        RestrictConnectionCaveShuffle = true,
        GoodBoots = true,
        HidePalace = false,
        HideKasuto = false,
        Climate = Climates.Classic,
        WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        EastBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        MazeBiome = Biome.VANILLALIKE,
        DMBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RECONSTRUCTED,
        GPStyle = PalaceStyle.RECONSTRUCTED,
        ShortenNormalPalaces = false,
        ShortenGP = true,
        IncludeVanillaRooms = true,
        Includev4_0Rooms = true,
        Includev4_4Rooms = false,
        TBirdRequired = true,
        PalacesToCompleteMin = 6,
        PalacesToCompleteMax = 6,
        RestartAtPalacesOnGameOver = true,
        ChangePalacePallettes = true,
        BossRoomsExitType = BossRoomsExitType.OVERWORLD,
        NoDuplicateRoomsByLayout = true,
        PalaceItemRoomCount = PalaceItemRoomCount.ONE,
        DarkLinkMinDistance = BossRoomMinDistance.SHORT,

        //Levels
        AttackLevelCap = 8,
        MagicLevelCap = 8,
        LifeLevelCap = 8,
        AttackEffectiveness = AttackEffectiveness.HIGH,
        MagicEffectiveness = MagicEffectiveness.LOW_COST,
        LifeEffectiveness = LifeEffectiveness.HIGH,

        //Spells
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        RandomizeSpellSpellEnemy = false,
        SwapUpAndDownStab = false,
        FireOption = FireOption.PAIR_WITH_RANDOM,

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        MixLargeAndSmallEnemies = false,
        GeneratorsAlwaysMatch = true,
        EnemyXPDrops = XPEffectiveness.RANDOM_HIGH,

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = true,
        IncludeSwordTechsInShuffle = false,
        IncludeQuestItemsInShuffle = false,
        IncludeSpellsInShuffle = false,

        ShuffleSmallItems = true,
        RemoveSpellItems = false,
        ShufflePBagAmounts = false,
        PalacesContainExtraKeys = true,
        RandomizeNewKasutoJarRequirements = true,
        AllowImportantItemDuplicates = false,

        //Drops
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,

        //Hints
        EnableHelpfulHints = true,
        EnableSpellItemHints = true,
        EnableTownNameHints = true
    };
    public static readonly RandomizerConfiguration StandardPreset = new()
    {
        //Start
        StartWithCandle = true,
        ShuffleStartingItems = false,
        ShuffleStartingSpells = false,
        MaxHeartContainers = MaxHeartsOption.EIGHT,
        StartingHeartContainersMin = 4,
        StartingHeartContainersMax = 4,
        StartingTechniques = StartingTechs.NONE,
        StartingLives = StartingLives.Lives3,
        IndeterminateOptionRate = IndeterminateOptionRate.HALF,

        //Overworld
        PalacesCanSwapContinents = true,
        ShuffleGP = true,
        ShuffleEncounters = true,
        AllowUnsafePathEncounters = true,
        IncludeLavaInEncounterShuffle = true,
        EncounterRate = EncounterRate.HALF,
        EastRocks = false,
        GenerateBaguWoods = true,
        HideLessImportantLocations = true,
        RestrictConnectionCaveShuffle = true,
        GoodBoots = true,
        AllowConnectionCavesToBeBlocked = true,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = false,
        ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES,

        Climate = Climates.Classic,
        WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        EastBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        MazeBiome = Biome.VANILLALIKE,
        DMBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        VanillaShuffleUsesActualTerrain = false,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RECONSTRUCTED,
        GPStyle = PalaceStyle.RECONSTRUCTED,
        ShortenNormalPalaces = false,
        ShortenGP = true,
        IncludeVanillaRooms = true,
        Includev4_0Rooms = true,
        Includev4_4Rooms = false,
        TBirdRequired = true,
        RemoveTBird = false,
        PalacesToCompleteMin = 6,
        PalacesToCompleteMax = 6,
        RestartAtPalacesOnGameOver = true,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = true,
        BossRoomsExitType = BossRoomsExitType.OVERWORLD,
        NoDuplicateRoomsByLayout = false,
        NoDuplicateRoomsByEnemies = true,
        BlockingRoomsInAnyPalace = true,
        HardBosses = false,
        PalaceItemRoomCount = PalaceItemRoomCount.ONE,
        DarkLinkMinDistance = BossRoomMinDistance.MEDIUM,

        //Levels
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,

        AttackLevelCap = 8,
        MagicLevelCap = 8,
        LifeLevelCap = 8,
        AttackEffectiveness = AttackEffectiveness.AVERAGE,
        MagicEffectiveness = MagicEffectiveness.AVERAGE,
        LifeEffectiveness = LifeEffectiveness.AVERAGE,

        //Spells
        ShuffleLifeRefillAmount = true,
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        RandomizeSpellSpellEnemy = true,
        SwapUpAndDownStab = false,
        FireOption = FireOption.PAIR_WITH_RANDOM,

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        ShuffleDripperEnemy = true,
        MixLargeAndSmallEnemies = true,
        GeneratorsAlwaysMatch = true,

        ShuffleEnemyHP = true,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        ShuffleSwordImmunity = true,
        EnemyXPDrops = XPEffectiveness.RANDOM,

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = true,
        IncludeSwordTechsInShuffle = false,
        IncludeQuestItemsInShuffle = false,
        IncludeSpellsInShuffle = false,

        ShuffleSmallItems = true,
        RemoveSpellItems = false,
        ShufflePBagAmounts = false,
        PalacesContainExtraKeys = false,
        RandomizeNewKasutoJarRequirements = true,
        AllowImportantItemDuplicates = false,

        //Drops
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,

        //Hints
        EnableHelpfulHints = true,
        EnableSpellItemHints = true,
        EnableTownNameHints = true
    };
    public static readonly RandomizerConfiguration MaxRandoPreset = new ()
    {
        //Start
        ShuffleStartingItems = true,
        ShuffleStartingSpells = true,
        MaxHeartContainers = MaxHeartsOption.RANDOM,
        StartingHeartContainersMin = 1,
        StartingHeartContainersMax = 8,
        StartingTechniques = StartingTechs.NONE,
        StartingLives = StartingLives.LivesRandom,
        IndeterminateOptionRate = IndeterminateOptionRate.HALF,

        //Overworld
        PalacesCanSwapContinents = true,
        ShuffleGP = true,
        ShuffleEncounters = true,
        AllowUnsafePathEncounters = true,
        IncludeLavaInEncounterShuffle = true,
        EncounterRate = EncounterRate.HALF,
        EastRocks = false,
        GenerateBaguWoods = false,
        HideLessImportantLocations = true,
        RestrictConnectionCaveShuffle = true,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = true,
        ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES,
        
        Climate = Climates.Classic,
        WestBiome = Biome.RANDOM_NO_VANILLA,
        EastBiome = Biome.RANDOM_NO_VANILLA,
        MazeBiome = Biome.VANILLALIKE,
        DMBiome = Biome.RANDOM_NO_VANILLA,
        VanillaShuffleUsesActualTerrain = true,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RECONSTRUCTED,
        GPStyle = PalaceStyle.RECONSTRUCTED,
        ShortenNormalPalaces = false,
        ShortenGP = null,
        IncludeVanillaRooms = true,
        Includev4_0Rooms = true,
        Includev4_4Rooms = false,
        TBirdRequired = false,
        RemoveTBird = false,
        PalacesToCompleteMin = 0,
        PalacesToCompleteMax = 6,
        RestartAtPalacesOnGameOver = true,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = true,
        BossRoomsExitType = BossRoomsExitType.RANDOM_PER_PALACE,
        NoDuplicateRoomsByLayout = false,
        BlockingRoomsInAnyPalace = true,
        HardBosses = true,
        PalaceItemRoomCount = PalaceItemRoomCount.ONE,
        DarkLinkMinDistance = BossRoomMinDistance.NONE,

        //Levels
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,

        AttackLevelCap = 8,
        MagicLevelCap = 8,
        LifeLevelCap = 8,
        AttackEffectiveness = AttackEffectiveness.AVERAGE,
        MagicEffectiveness = MagicEffectiveness.AVERAGE,
        LifeEffectiveness = LifeEffectiveness.AVERAGE,

        //Spells
        ShuffleLifeRefillAmount = true,
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        RandomizeSpellSpellEnemy = true,
        SwapUpAndDownStab = false,
        FireOption = FireOption.RANDOM,

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        ShuffleDripperEnemy = true,
        MixLargeAndSmallEnemies = true,
        GeneratorsAlwaysMatch = true,

        ShuffleEnemyHP = true,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        ShuffleSwordImmunity = true,
        EnemyXPDrops = XPEffectiveness.RANDOM,

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = true,
        IncludeSwordTechsInShuffle = false,
        IncludeQuestItemsInShuffle = false,
        IncludeSpellsInShuffle = false,

        ShuffleSmallItems = true,
        RemoveSpellItems = false,
        ShufflePBagAmounts = true,
        PalacesContainExtraKeys = false,
        RandomizeNewKasutoJarRequirements = true,
        AllowImportantItemDuplicates = false,

        //Drops
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,

        //Hints
        EnableHelpfulHints = true,
        EnableSpellItemHints = true,
        EnableTownNameHints = true
    };

    public static readonly RandomizerConfiguration RandoPercentPreset = new ()
    {
        
    };
}