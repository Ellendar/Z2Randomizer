using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class Upstarts2026Week2Preset
{
    public static string Name => "Upstarts Week 2 - Sequential Canyons";

    public static string Description => """
Tournament preset for the second week of the 2026 Upstarts Tournament.

Starting items and spells: Candle, Shield, Downstab

- Canyon biomes (large)
- Sequential short palaces
- Transportation shuffle continental connectors
- Spells in towns
- River Devil blocks town
- Shuffle enemies, not mixed
""";

    public static readonly RandomizerConfiguration Preset = new()
    {
        StartWithCandle = true,
        StartWithShield = true,
        StartingTechniques = StartingTechs.DOWNSTAB,
        EncounterRate = EncounterRate.HALF,
        HidePalace = false,
        HideKasuto = false,
        GoodBoots = true,
        GenerateBaguWoods = false,
        ContinentConnectionType = ContinentConnectionType.TRANSPORTATION_SHUFFLE,
        DmSize = DmSizeOption.MEDIUM,
        MazeSize = MazeSizeOption.MEDIUM,
        WestBiome = Biome.CANYON,
        EastBiome = Biome.CANYON,
        DmBiome = Biome.CANYON,
        MazeBiome = Biome.VANILLALIKE,
        WestClimate = ClimateEnum.CLASSIC,
        EastClimate = ClimateEnum.CLASSIC,
        NormalPalaceStyle = PalaceStyle.SEQUENTIAL,
        GpStyle = PalaceStyle.SEQUENTIAL,
        Includev4_0Rooms = true,
        Includev5_0Rooms = true,
        RemoveLongDeadEnds = true,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        ReduceDripperVariance = true,
        ChangePalacePallettes = true,
        NoDuplicateRoomsByLayout = true,
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,
        AttackEffectiveness = AttackEffectiveness.AVERAGE_HIGH,
        MagicEffectiveness = MagicEffectiveness.AVERAGE_LOW_COST,
        LifeEffectiveness = LifeEffectiveness.AVERAGE_HIGH,
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        RandomizeSpellSpellEnemy = true,
        FireOption = FireOption.PAIR_WITH_RANDOM,
        GeneratorsAlwaysMatch = true,
        ShuffleEnemyHP = EnemyLifeOption.MEDIUM,
        ShuffleBossHP = EnemyLifeOption.MEDIUM,
        EnemyXPDrops = XPEffectiveness.RANDOM_HIGH,
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = true,
        ShuffleSmallItems = true,
        PalacesContainExtraKeys = true,
        RandomizeNewKasutoJarRequirements = true,
        ShufflePBagAmounts = true,
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,
        HelpfulHints = HelpfulHintOption.TOWNS_SEPARATE,
        RiverDevilBlockerOption = RiverDevilBlockerOption.SIEGE,
        EnableSpellItemHints = true,
        EnableTownNameHints = true,
        GpLength = PalaceLengthOption.SHORT,
        NormalPalaceLength = PalaceLengthOption.SHORT,
        EastRocks = false,
        RevealWalkthroughWalls = true,
        RevealHiddenJars = true,
    };
}
