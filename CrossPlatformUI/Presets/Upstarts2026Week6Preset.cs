using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class Upstarts2026Week6Preset
{
    public static string Name => "Upstarts Week 6 / Brackets";

    public static string Description => """
Tournament preset for the sixth week and brackets of the 2026 Upstarts Tournament.

Starting items and spells: Candle

- Random biomes
- Each continent will pick a biome independently
- All biomes set to large except Death Mountain (Medium)
- Random style palaces (random length normal palaces, short GP)
- Each palace will pick a style independently
- Anything goes continental connectors
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
        WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        EastBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        DmBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        MazeBiome = Biome.VANILLALIKE,
        WestClimate = ClimateEnum.CLASSIC,
        EastClimate = ClimateEnum.CLASSIC,
        NormalPalaceStyle = PalaceStyle.RANDOM_PER_PALACE,
        GpStyle = PalaceStyle.RANDOM,
        Includev4_0Rooms = true,
        Includev5_0Rooms = true,
        BlockingRoomsInAnyPalace = true,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
        ReduceDripperVariance = true,
        ChangePalacePallettes = true,
        NoDuplicateRoomsByEnemies = true,
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
        PreventSpellItemChains = true,
        IncludeSpellsInShuffle = true,
        IncludeQuestItemsInShuffle = true,
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,
        HelpfulHints = HelpfulHintOption.TOWNS_SEPARATE,
        EnableSpellItemHints = true,
        EnableTownNameHints = true,
        GpLength = PalaceLengthOption.SHORT,
        NormalPalaceLength = PalaceLengthOption.RANDOM,
        RiverDevilBlockerOption = RiverDevilBlockerOption.RANDOM,
        RevealWalkthroughWalls = true,
        RevealHiddenJars = true,
    };
}
