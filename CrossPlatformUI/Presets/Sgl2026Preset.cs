using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class Sgl2026Preset
{
    public static string Name => "SGL 2026";

    public static string Description => """
Tournament preset used for SpeedGaming Live 2026
""";

    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        StartWithCandle = true,
        StartWithCross = true,
        ShuffleStartingItems = false,
        ShuffleStartingSpells = false,
        MaxHeartContainers = MaxHeartsOption.SIX,
        StartingHeartContainersMin = 3,
        StartingHeartContainersMax = 3,
        StartingMagicContainersMin = 3,
        StartingMagicContainersMax = 3,
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
        EastRocks = true,
        GenerateBaguWoods = true,
        LessImportantLocationsOption = LessImportantLocationsOption.REMOVE,
        RestrictConnectionCaveShuffle = true,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = false,
        WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        EastBiome = Biome.RANDOM_UNIQUE,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        WestClimate = ClimateEnum.VANILLA_WEIGHTED,
        EastClimate = ClimateEnum.VANILLA_WEIGHTED,
        DmClimate = ClimateEnum.CLASSIC,
        ContinentConnectionType = ContinentConnectionType.TRANSPORTATION_SHUFFLE,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RECONSTRUCTED,
        GpStyle = PalaceStyle.RECONSTRUCTED,
        NormalPalaceLength = PalaceLengthOption.MEDIUM,
        GpLength = PalaceLengthOption.MEDIUM,
        IncludeVanillaRooms = true,
        Includev4_0Rooms = false,
        Includev5_0Rooms = false,
        IncludeExpertRooms = false,
        PalacesToCompleteMin = 6,
        PalacesToCompleteMax = 6,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
        ReduceDripperVariance = true,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = false,
        PalaceDropStyle = PalaceDropStyle.ANY_EXIT,
        BossRoomsExitType = BossRoomsExitType.OVERWORLD,
        NoDuplicateRoomsByLayout = true,
        NoDuplicateRoomsByEnemies = false,
        KeyLogic = KeyLogicOption.FAIRY_OR_MAGICKEY,
        BlockingRoomsInAnyPalace = true,
        HardBosses = true,
        AggressiveTbird = true,
        PalaceItemRoomCount = PalaceItemRoomCount.ONE,
        DarkLinkMinDistance = BossRoomMinDistance.MEDIUM,

        //Levels
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,

        AttackLevelCap = 8,
        MagicLevelCap = 8,
        LifeLevelCap = 8,
        ScaleLevelRequirementsToCap = false,
        AttackEffectiveness = AttackEffectiveness.BALANCED,
        MagicEffectiveness = MagicEffectiveness.AVERAGE_CONTROLLED,
        LifeEffectiveness = LifeEffectiveness.VANILLA,

        //Spells
        ShuffleLifeRefillAmount = false,
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        RandomizeSpellSpellEnemy = false,
        SwapUpAndDownStab = true,
        FireOption = FireOption.NORMAL,

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        DripperEnemyOption = DripperEnemyOption.ONLY_BOTS,
        MixLargeAndSmallEnemies = true,
        GeneratorsAlwaysMatch = true,

        ShuffleEnemyHP = EnemyLifeOption.NARROW,
        ShuffleBossHP = EnemyLifeOption.SLIGHTLY_HIGH,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        SwordImmunityOption = SwordImmunityOption.NONE,
        EnemyXPDrops = XPEffectiveness.SLIGHTLY_HIGH,

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = false,
        IncludeSwordTechsInShuffle = false,
        IncludeQuestItemsInShuffle = true,
        IncludeSpellsInShuffle = false,
        TownQuestLocationsAreMinorItems = true,

        ShuffleSmallItems = false,
        RemoveSpellItems = false,
        ShufflePBagAmounts = false,
        PalacesContainExtraKeys = false,
        RandomizeNewKasutoJarRequirements = true, // was fixed 6 required
        FastItemPickup = true,
        AllowImportantItemDuplicates = false,

        //Drops
        ShuffleItemDropFrequency = false,
        RandomizeDrops = false,
        StandardizeDrops = true,
        SmallEnemiesCanDropBlueJar = true,
        LargeEnemiesCanDropRedJar = true,
        LargeEnemiesCanDropLargeBag = true,

        //Hints
        EnableSpellItemHints = true,
        EnableTownNameHints = true,
    };
}
