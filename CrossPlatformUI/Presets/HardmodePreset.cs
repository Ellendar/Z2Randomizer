using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace CrossPlatformUI.Presets;

public static class HardmodePreset
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
        IncludeLavaInEncounterShuffle = true,
        EncounterRate = EncounterRate.HALF,
        RiverDevilBlockerOption = RiverDevilBlockerOption.RANDOM,
        EastRocks = true,
        GenerateBaguWoods = true,
        HideLessImportantLocations = true,
        RestrictConnectionCaveShuffle = true,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = false,
        ContinentConnectionType = ContinentConnectionType.TRANSPORTATION_SHUFFLE,
        Climate = Climates.Classic,
        WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        EastBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RANDOM_WALK,
        GpStyle = PalaceStyle.RANDOM_WALK,
        ShortenNormalPalaces = false,
        ShortenGP = false,
        IncludeVanillaRooms = true,
        Includev4_0Rooms = true,
        Includev4_4Rooms = true,
        TBirdRequired = true,
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
        HardBosses = true,
        PalaceItemRoomCount = PalaceItemRoomCount.ONE,
        DarkLinkMinDistance = BossRoomMinDistance.MEDIUM,

        //Levels
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,

        AttackLevelCap = 7,
        MagicLevelCap = 8,
        LifeLevelCap = 8,
        AttackEffectiveness = AttackEffectiveness.AVERAGE_LOW,
        MagicEffectiveness = MagicEffectiveness.AVERAGE,
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
        ShuffleDripperEnemy = false,
        MixLargeAndSmallEnemies = true,
        GeneratorsAlwaysMatch = true,

        ShuffleEnemyHP = true,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        ShuffleSwordImmunity = true,
        EnemyXPDrops = XPEffectiveness.VANILLA,

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = false,
        IncludeSwordTechsInShuffle = false,
        IncludeQuestItemsInShuffle = false,
        IncludeSpellsInShuffle = true,

        ShuffleSmallItems = true,
        RemoveSpellItems = false,
        ShufflePBagAmounts = false,
        PalacesContainExtraKeys = false,
        RandomizeNewKasutoJarRequirements = true,
        AllowImportantItemDuplicates = false,

        //Drops
        ShuffleItemDropFrequency = false,
        RandomizeDrops = false,
        StandardizeDrops = true,
        // Keeping the small drop pool empty for the 1/8 P-Bag chance
        LargeEnemiesCanDropRedJar = true,
        LargeEnemiesCanDropLargeBag = true,

        //Hints
        EnableHelpfulHints = false,
        EnableSpellItemHints = true,
        EnableTownNameHints = true,

        RevealWalkthroughWalls = false,
    };
}
