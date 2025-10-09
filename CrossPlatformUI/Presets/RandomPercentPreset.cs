using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace CrossPlatformUI.Presets;

public static class RandomPercentPreset
{
    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        ShuffleStartingItems = true,
        ShuffleStartingSpells = true,
        MaxHeartContainers = MaxHeartsOption.RANDOM,
        StartingHeartContainersMin = 3,
        StartingHeartContainersMax = 8,
        StartingTechniques = StartingTechs.RANDOM,
        StartingLives = StartingLives.LivesRandom,
        IndeterminateOptionRate = IndeterminateOptionRate.HALF,

        //Overworld
        PalacesCanSwapContinents = null,
        ShuffleGP = null,
        ShuffleEncounters = null,
        AllowUnsafePathEncounters = false,
        IncludeLavaInEncounterShuffle = false,
        EncounterRate = EncounterRate.HALF,
        RiverDevilBlockerOption = RiverDevilBlockerOption.RANDOM,
        EastRocks = null,
        GenerateBaguWoods = null,
        HideLessImportantLocations = null,
        RestrictConnectionCaveShuffle = null,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = null,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = null,
        ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES,

        Climate = Climates.Classic,
        WestBiome = Biome.RANDOM,
        EastBiome = Biome.RANDOM,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM,
        VanillaShuffleUsesActualTerrain = true,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RANDOM_PER_PALACE,
        GpStyle = PalaceStyle.RANDOM,
        ShortenNormalPalaces = false,
        ShortenGP = null,
        IncludeVanillaRooms = null,
        Includev4_0Rooms = null,
        Includev4_4Rooms = null,
        TBirdRequired = null,
        RemoveTBird = false,
        PalacesToCompleteMin = 0,
        PalacesToCompleteMax = 6,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = null,
        ReduceDripperVariance = false,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = true,
        BossRoomsExitType = BossRoomsExitType.RANDOM_PER_PALACE,
        NoDuplicateRoomsByLayout = true,
        BlockingRoomsInAnyPalace = true,
        HardBosses = false,
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
        ShuffleSpellLocations = null,
        DisableMagicContainerRequirements = null,
        RandomizeSpellSpellEnemy = null,
        SwapUpAndDownStab = null,
        FireOption = FireOption.RANDOM,

        //Enemies
        ShuffleOverworldEnemies = null,
        ShufflePalaceEnemies = null,
        ShuffleDripperEnemy = true,
        MixLargeAndSmallEnemies = null,
        GeneratorsAlwaysMatch = false,

        ShuffleEnemyHP = true,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        ShuffleSwordImmunity = true,
        EnemyXPDrops = XPEffectiveness.RANDOM,

        //Items
        ShufflePalaceItems = null,
        ShuffleOverworldItems = null,
        MixOverworldAndPalaceItems = null,
        IncludePBagCavesInItemShuffle = null,
        IncludeSwordTechsInShuffle = null,
        IncludeQuestItemsInShuffle = null,
        IncludeSpellsInShuffle = null,

        ShuffleSmallItems = true,
        RemoveSpellItems = null,
        ShufflePBagAmounts = null,
        PalacesContainExtraKeys = null,
        RandomizeNewKasutoJarRequirements = true,
        AllowImportantItemDuplicates = false,

        //Drops
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,

        //Hints
        EnableHelpfulHints = null,
        EnableSpellItemHints = null,
        EnableTownNameHints = null,

        RevealWalkthroughWalls = false,
    };
}
