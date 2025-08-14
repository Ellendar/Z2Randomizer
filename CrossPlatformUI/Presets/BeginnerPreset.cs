using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace CrossPlatformUI.Presets;

public static class BeginnerPreset
{
    public static readonly RandomizerConfiguration Preset = new()
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
}
