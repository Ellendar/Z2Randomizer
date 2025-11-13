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
        ShuffleStartingItems = false,
        ShuffleStartingSpells = false,
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
        AllowUnsafePathEncounters = false,
        IncludeLavaInEncounterShuffle = false,
        EncounterRate = EncounterRate.HALF,
        RiverDevilBlockerOption = RiverDevilBlockerOption.PATH,
        EastRocks = false,
        GenerateBaguWoods = false,
        LessImportantLocationsOption = LessImportantLocationsOption.HIDE,
        RestrictConnectionCaveShuffle = true,
        AllowConnectionCavesToBeBlocked = false,
        GoodBoots = true,
        HidePalace = false,
        HideKasuto = false,
        Climate = Climates.Classic,
        WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        EastBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RANDOM_WALK,
        GpStyle = PalaceStyle.RANDOM_WALK,
        ShortenNormalPalaces = false,
        ShortenGP = true,
        IncludeVanillaRooms = true,
        Includev4_0Rooms = true,
        Includev5_0Rooms = false,
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
        BlockingRoomsInAnyPalace = false,
        HardBosses = false,
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
        FireOption = FireOption.NORMAL,

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        DripperEnemyOption = DripperEnemyOption.ONLY_BOTS,
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
        EnableTownNameHints = true,

        RevealWalkthroughWalls = true,
    };
}
