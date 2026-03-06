using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace CrossPlatformUI.Presets;

public static class NormalPreset
{
    public static readonly RandomizerConfiguration Preset = new()
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
        ShuffleEncounters = false,
        AllowUnsafePathEncounters = false,
        IncludeLavaInEncounterShuffle = false,
        EncounterRate = EncounterRate.HALF,
        RiverDevilBlockerOption = RiverDevilBlockerOption.RANDOM,
        EastRocks = true,
        GenerateBaguWoods = false,
        LessImportantLocationsOption = LessImportantLocationsOption.REMOVE,
        RestrictConnectionCaveShuffle = true,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = false,
        WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        EastBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        WestClimate = ClimateEnum.VANILLA_WEIGHTED_WEST,
        EastClimate = ClimateEnum.VANILLA_WEIGHTED_EAST,
        DmClimate = ClimateEnum.CLASSIC,
        MazeClimate = ClimateEnum.CLASSIC,
        ContinentConnectionType = ContinentConnectionType.TRANSPORTATION_SHUFFLE,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RANDOM_WALK,
        GpStyle = PalaceStyle.RANDOM_WALK,
        NormalPalaceLength = PalaceLengthOption.MEDIUM,
        GpLength = PalaceLengthOption.SHORT,
        IncludeVanillaRooms = true,
        Includev4_0Rooms = true,
        Includev5_0Rooms = true,
        TBirdRequired = true,
        RemoveTBird = false,
        PalacesToCompleteMin = 6,
        PalacesToCompleteMax = 6,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
        ReduceDripperVariance = true,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = false,
        BossRoomsExitType = BossRoomsExitType.OVERWORLD,
        NoDuplicateRoomsByLayout = true,
        BlockingRoomsInAnyPalace = true,
        HardBosses = false,
        PalaceItemRoomCount = PalaceItemRoomCount.ONE,
        DarkLinkMinDistance = BossRoomMinDistance.SHORT,

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
        ShuffleLifeRefillAmount = false,
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        RandomizeSpellSpellEnemy = false,
        SwapUpAndDownStab = false,
        FireOption = FireOption.NORMAL,

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        DripperEnemyOption = DripperEnemyOption.EASIER_GROUND_ENEMIES,
        MixLargeAndSmallEnemies = false,
        GeneratorsAlwaysMatch = true,

        ShuffleEnemyHP = EnemyLifeOption.MEDIUM,
        ShuffleBossHP = EnemyLifeOption.MEDIUM,
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
        IncludeQuestItemsInShuffle = true,
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
        EnableTownNameHints = true,

        RevealWalkthroughWalls = false,
    };
}
