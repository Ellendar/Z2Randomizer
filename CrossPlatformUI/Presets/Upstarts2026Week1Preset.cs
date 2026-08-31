using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class Upstarts2026Week1Preset
{
    public static string Name => "Upstarts Week 1 - Random Walking Calderas";

    public static string Description => """
Tournament preset for the first week of the 2026 Upstarts Tournament.

Starting items and spells: Candle, Life, Fairy, Downstab

- Calderas everywhere (large continents)
- Random Walk short palaces
- Vanilla continent connectors
- Spells in towns
- Quest items in vanilla locations
- River Devil blocks path
""";

    public static readonly RandomizerConfiguration Preset = new()
    {
        StartWithCandle = true,
        StartWithLife = true,
        StartWithFairy = true,
        StartingTechniques = StartingTechs.DOWNSTAB,
        EncounterRate = EncounterRate.HALF,
        HidePalace = false,
        HideKasuto = false,
        GoodBoots = true,
        GenerateBaguWoods = false,
        DmSize = DmSizeOption.MEDIUM,
        MazeSize = MazeSizeOption.MEDIUM,
        WestBiome = Biome.CALDERA,
        EastBiome = Biome.VOLCANO,
        DmBiome = Biome.CALDERA,
        MazeBiome = Biome.VANILLALIKE,
        WestClimate = ClimateEnum.CLASSIC,
        EastClimate = ClimateEnum.CLASSIC,
        NormalPalaceStyle = PalaceStyle.RANDOM_WALK,
        GpStyle = PalaceStyle.RANDOM_WALK,
        Includev4_0Rooms = true,
        Includev5_0Rooms = true,
        RemoveLongDeadEnds = true,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
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
        EnableSpellItemHints = true,
        EnableTownNameHints = true,
        GpLength = PalaceLengthOption.SHORT,
        NormalPalaceLength = PalaceLengthOption.SHORT,
        EastRocks = false,
        RevealWalkthroughWalls = true,
        RevealHiddenJars = true,
    };
}
