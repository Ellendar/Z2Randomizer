using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class Upstarts2026Week3Preset
{
    public static string Name => "Upstarts Week 3 - Vanilla-Weighted Islands";

    public static string Description => """
Tournament preset for the third week of the 2026 Upstarts Tournament.

Starting items and spells: Candle, Shield, Downstab

- Vanilla-Weighted palaces
- Medium Islands west/DM, Vanilla-like east
- River Devil blocks cave
- Palaces can swap continents, GP in Valley of Death
- Shuffle enemies, not mixed
- Attack and life levels set to random
""";

    public static readonly RandomizerConfiguration Preset = new()
    {
        StartWithCandle = true,
        StartWithShield = true,
        StartingTechniques = StartingTechs.DOWNSTAB,
        PalacesCanSwapContinents = true,
        EncounterRate = EncounterRate.HALF,
        HidePalace = false,
        HideKasuto = false,
        GoodBoots = true,
        GenerateBaguWoods = false,
        ContinentConnectionType = ContinentConnectionType.TRANSPORTATION_SHUFFLE,
        WestSize = OverworldSizeOption.MEDIUM,
        EastSize = OverworldSizeOption.MEDIUM,
        DmSize = DmSizeOption.SMALL,
        MazeSize = MazeSizeOption.MEDIUM,
        WestBiome = Biome.ISLANDS,
        EastBiome = Biome.VANILLALIKE,
        DmBiome = Biome.ISLANDS,
        MazeBiome = Biome.VANILLALIKE,
        WestClimate = ClimateEnum.CLASSIC,
        EastClimate = ClimateEnum.CLASSIC,
        NormalPalaceStyle = PalaceStyle.VANILLA_WEIGHTED,
        GpStyle = PalaceStyle.RANDOM_WALK,
        Includev4_0Rooms = true,
        Includev5_0Rooms = true,
        BlockingRoomsInAnyPalace = true,
        RemoveLongDeadEnds = true,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
        ReduceDripperVariance = true,
        ChangePalacePallettes = true,
        NoDuplicateRoomsByLayout = true,
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,
        AttackEffectiveness = AttackEffectiveness.AVERAGE,
        MagicEffectiveness = MagicEffectiveness.AVERAGE_LOW_COST,
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
        EnemyXPDrops = XPEffectiveness.RANDOM,
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
        NormalPalaceLength = PalaceLengthOption.MEDIUM,
        RiverDevilBlockerOption = RiverDevilBlockerOption.CAVE,
        EastRocks = false,
        RevealWalkthroughWalls = true,
        RevealHiddenJars = true,
    };
}
