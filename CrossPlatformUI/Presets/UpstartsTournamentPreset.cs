using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace CrossPlatformUI.Presets;

public static class UpstartsTournamentPreset
{
    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        StartWithCandle = true,
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
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = true,
        ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES,

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
        NoDuplicateRoomsByLayout = false,
        NoDuplicateRoomsByEnemies = true,
        BlockingRoomsInAnyPalace = true,
        HardBosses = true,
        PalaceItemRoomCount = PalaceItemRoomCount.ONE,

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
        RandomizeSpellSpellEnemy = false,
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
