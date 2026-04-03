using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class Sgl2025Preset
{
    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        StartWithCandle = true,
        StartWithCross = true,
        ShuffleStartingItems = false,
        StartWithFire = true,
        ShuffleStartingSpells = false,
        MaxHeartContainers = MaxHeartsOption.SIX,
        StartingHeartContainersMin = 3,
        StartingHeartContainersMax = 3,
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
        RiverDevilBlockerOption = RiverDevilBlockerOption.PATH,
        EastRocks = false,
        GenerateBaguWoods = true,
        LessImportantLocationsOption = LessImportantLocationsOption.HIDE,
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
        WestClimate = ClimateEnum.CLASSIC,
        EastClimate = ClimateEnum.CLASSIC,
        DmClimate = ClimateEnum.CLASSIC,
        ContinentConnectionType = ContinentConnectionType.TRANSPORTATION_SHUFFLE,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RECONSTRUCTED,
        GpStyle = PalaceStyle.RECONSTRUCTED,
        NormalPalaceLength = PalaceLengthOption.SHORT,
        GpLength = PalaceLengthOption.MEDIUM,
        IncludeVanillaRooms = true,
        Includev4_0Rooms = true,
        Includev5_0Rooms = false,
        IncludeExpertRooms = true,
        TBirdRequired = true,
        PalacesToCompleteMin = 6,
        PalacesToCompleteMax = 6,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = false,
        ReduceDripperVariance = false,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = false,
        PalaceDropStyle = PalaceDropStyle.ANY_EXIT,
        BossRoomsExitType = BossRoomsExitType.OVERWORLD,
        NoDuplicateRoomsByLayout = false,
        NoDuplicateRoomsByEnemies = true,
        BlockingRoomsInAnyPalace = true,
        HardBosses = true,
        // +"Aggressive Thunderbird"
        PalaceItemRoomCount = PalaceItemRoomCount.ONE,
        DarkLinkMinDistance = BossRoomMinDistance.NONE,

        //Levels
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,

        AttackLevelCap = 6,
        MagicLevelCap = 8,
        LifeLevelCap = 8,
        AttackEffectiveness = AttackEffectiveness.AVERAGE_LOW, // was AttackValues = [2, 3, 4, 6, 8, 12, 14, 16];
        MagicEffectiveness = MagicEffectiveness.AVERAGE,
        LifeEffectiveness = LifeEffectiveness.VANILLA,

        //Spells
        ShuffleLifeRefillAmount = false,
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        RandomizeSpellSpellEnemy = false,
        SwapUpAndDownStab = true,
        FireOption = FireOption.NORMAL,
        // +"Expensive Thunder"

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        DripperEnemyOption = DripperEnemyOption.ONLY_BOTS,
        MixLargeAndSmallEnemies = true,
        GeneratorsAlwaysMatch = true,

        ShuffleEnemyHP = EnemyLifeOption.MEDIUM, // was 100-150%
        ShuffleBossHP = EnemyLifeOption.VANILLA,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        ShuffleSwordImmunity = false,
        EnemyXPDrops = XPEffectiveness.SLIGHTLY_HIGH, // was +0 to +2

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = false,
        IncludeSwordTechsInShuffle = false,
        IncludeQuestItemsInShuffle = false,
        IncludeSpellsInShuffle = false,

        ShuffleSmallItems = false,
        RemoveSpellItems = false,
        ShufflePBagAmounts = false,
        PalacesContainExtraKeys = false,
        RandomizeNewKasutoJarRequirements = false,
        AllowImportantItemDuplicates = false,

        //Drops
        ShuffleItemDropFrequency = false,
        RandomizeDrops = false,
        StandardizeDrops = true,
        SmallEnemiesCanDropBlueJar = true,
        LargeEnemiesCanDropRedJar = true,
        LargeEnemiesCanDropLargeBag = true,

        //Hints
        EnableHelpfulHints = false,
        EnableSpellItemHints = true,
        EnableTownNameHints = true,

        RevealWalkthroughWalls = false,
    };
}
