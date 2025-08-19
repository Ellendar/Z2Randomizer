using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace CrossPlatformUI.Presets;

public static class MaxRandoPreset
{
    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        ShuffleStartingItems = true,
        ShuffleStartingSpells = true,
        MaxHeartContainers = MaxHeartsOption.RANDOM,
        StartingHeartContainersMin = 1,
        StartingHeartContainersMax = 8,
        StartingTechniques = StartingTechs.RANDOM,
        StartingLives = StartingLives.LivesRandom,
        IndeterminateOptionRate = IndeterminateOptionRate.HALF,

        //Overworld
        PalacesCanSwapContinents = true,
        ShuffleGP = true,
        ShuffleEncounters = true,
        AllowUnsafePathEncounters = true,
        IncludeLavaInEncounterShuffle = true,
        EncounterRate = EncounterRate.HALF,
        RiverDevilBlockerOption = RiverDevilBlockerOption.RANDOM,
        EastRocks = null,
        GenerateBaguWoods = false,
        LessImportantLocationsOption = LessImportantLocationsOption.HIDE,
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
        NormalPalaceStyle = PalaceStyle.RANDOM_PER_PALACE,
        GPStyle = PalaceStyle.RANDOM_NO_VANILLA_OR_SHUFFLE,
        ShortenNormalPalaces = false,
        ShortenGP = null,
        IncludeVanillaRooms = true,
        Includev4_0Rooms = true,
        Includev4_4Rooms = true,
        TBirdRequired = false,
        RemoveTBird = false,
        PalacesToCompleteMin = 0,
        PalacesToCompleteMax = 6,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = false,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = true,
        BossRoomsExitType = BossRoomsExitType.RANDOM_PER_PALACE,
        NoDuplicateRoomsByLayout = true,
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
}
