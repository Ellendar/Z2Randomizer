using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class Upstarts2026Week5Preset
{
    public static string Name => "Upstarts Week 5 - Vanilla-like Reconstructed";

    public static string Description => """
Tournament preset for the fifth week of the 2026 Upstarts Tournament.

Starting items and spells: Candle

- Vanilla-like continents
- Reconstructed short palaces
- Anything goes continental connectors
- Connection caves can be blocked
- GP now in palace shuffle
- River Devil block is random
- Enemies shuffled WITH mixed small and large enemies
- Quest items and spells in the item pool
- Quest locations are checks
""";

    public static readonly RandomizerConfiguration Preset = new()
    {
        StartWithCandle = true,
        PalacesCanSwapContinents = true,
        ShuffleGP = true,
        EncounterRate = EncounterRate.HALF,
        HidePalace = null,
        HideKasuto = null,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        GenerateBaguWoods = false,
        ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES,
        DmSize = DmSizeOption.MEDIUM,
        MazeSize = MazeSizeOption.MEDIUM,
        WestBiome = Biome.VANILLALIKE,
        EastBiome = Biome.VANILLALIKE,
        DmBiome = Biome.VANILLALIKE,
        MazeBiome = Biome.VANILLALIKE,
        WestClimate = ClimateEnum.CLASSIC,
        EastClimate = ClimateEnum.CLASSIC,
        NormalPalaceStyle = PalaceStyle.RECONSTRUCTED,
        GpStyle = PalaceStyle.RECONSTRUCTED,
        Includev4_0Rooms = true,
        Includev5_0Rooms = true,
        BlockingRoomsInAnyPalace = true,
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
        MixLargeAndSmallEnemies = true,
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
        IncludeSpellsInShuffle = true,
        IncludeQuestItemsInShuffle = true,
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,
        HelpfulHints = HelpfulHintOption.TOWNS_SEPARATE,
        EnableSpellItemHints = true,
        EnableTownNameHints = true,
        GpLength = PalaceLengthOption.SHORT,
        NormalPalaceLength = PalaceLengthOption.SHORT,
        RiverDevilBlockerOption = RiverDevilBlockerOption.RANDOM,
        RevealWalkthroughWalls = true,
        RevealHiddenJars = true,
    };
}
