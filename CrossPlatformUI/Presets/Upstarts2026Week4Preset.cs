using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class Upstarts2026Week4Preset
{
    public static string Name => "Upstarts Week 4 - Mountainous Towers";

    public static string Description => """
Tournament preset for the fourth week of the 2026 Upstarts Tournament.

Starting items and spells: Candle, Downstab

- Mountainous biomes
- Tower palaces, Full sized regular palaces, Medium sized GP
- Anything goes continental connectors
- Harder Carock
- Magic levels set to random
- Shuffle exp steal and sword immunity
- No more extra keys in palaces
- Quest items now in the item pool
- Quest locations are now checks
- Spells now in the item pool
""";

    public static readonly RandomizerConfiguration Preset = new()
    {
        StartWithCandle = true,
        StartingTechniques = StartingTechs.DOWNSTAB,
        PalacesCanSwapContinents = true,
        EncounterRate = EncounterRate.HALF,
        HidePalace = null,
        HideKasuto = null,
        GoodBoots = true,
        GenerateBaguWoods = false,
        ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES,
        WestSize = OverworldSizeOption.MEDIUM,
        EastSize = OverworldSizeOption.MEDIUM,
        DmSize = DmSizeOption.SMALL,
        MazeSize = MazeSizeOption.MEDIUM,
        WestBiome = Biome.MOUNTAINOUS,
        EastBiome = Biome.MOUNTAINOUS,
        DmBiome = Biome.MOUNTAINOUS,
        MazeBiome = Biome.VANILLALIKE,
        WestClimate = ClimateEnum.CLASSIC,
        EastClimate = ClimateEnum.CLASSIC,
        NormalPalaceStyle = PalaceStyle.TOWER,
        GpStyle = PalaceStyle.TOWER,
        Includev4_0Rooms = true,
        Includev5_0Rooms = true,
        BlockingRoomsInAnyPalace = true,
        RemoveLongDeadEnds = true,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
        ReduceDripperVariance = true,
        ChangePalacePallettes = true,
        NoDuplicateRoomsByLayout = true,
        HardBosses = true,
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,
        AttackEffectiveness = AttackEffectiveness.AVERAGE,
        MagicEffectiveness = MagicEffectiveness.AVERAGE,
        LifeEffectiveness = LifeEffectiveness.AVERAGE,
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        RandomizeSpellSpellEnemy = true,
        FireOption = FireOption.PAIR_WITH_RANDOM,
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        DripperEnemyOption = DripperEnemyOption.ANY_GROUND_ENEMY,
        ShuffleEnemyHP = EnemyLifeOption.MEDIUM,
        ShuffleBossHP = EnemyLifeOption.MEDIUM,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        EnemyXPDrops = XPEffectiveness.RANDOM,
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = true,
        ShuffleSmallItems = true,
        RandomizeNewKasutoJarRequirements = true,
        ShufflePBagAmounts = true,
        PreventSpellItemChains = true,
        IncludeSpellsInShuffle = true,
        IncludeQuestItemsInShuffle = true,
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,
        HelpfulHints = HelpfulHintOption.TOWNS_SEPARATE,
        EnableSpellItemHints = true,
        EnableTownNameHints = true,
        GpLength = PalaceLengthOption.MEDIUM,
        NormalPalaceLength = PalaceLengthOption.RANDOM,
        RiverDevilBlockerOption = RiverDevilBlockerOption.CAVE,
        RevealWalkthroughWalls = true,
        RevealHiddenJars = true,
    };
}
