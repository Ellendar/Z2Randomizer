using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

/// <summary>
/// Beginner and Default preset.
/// </summary>
public static class BeginnerPreset
{
    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        StartWithCandle = true,
        StartWithLife = true,
        StartingTechniques = StartingTechs.DOWNSTAB,
        StartingLives = StartingLives.Lives5,

        //Overworld
        EncounterRate = EncounterRate.HALF,
        EastRocks = true,
        GenerateBaguWoods = false,
        LessImportantLocationsOption = LessImportantLocationsOption.REMOVE,
        GoodBoots = true,
        HidePalace = false,
        HideKasuto = false,
        WestSize = OverworldSizeOption.MEDIUM,
        EastSize = OverworldSizeOption.MEDIUM,
        DmSize = DmSizeOption.SMALL,
        MazeSize = MazeSizeOption.MEDIUM,
        WestBiome = Biome.VANILLALIKE,
        EastBiome = Biome.VANILLALIKE,
        DmBiome = Biome.VANILLALIKE,
        MazeBiome = Biome.VANILLALIKE,
        WestClimate = ClimateEnum.VANILLA_WEIGHTED_WEST,
        EastClimate = ClimateEnum.VANILLA_WEIGHTED_EAST,

        //Palaces
        NormalPalaceStyle = PalaceStyle.VANILLA_WEIGHTED,
        GpStyle = PalaceStyle.TOWER,
        NormalPalaceLength = PalaceLengthOption.MEDIUM,
        GpLength = PalaceLengthOption.SHORT,
        Includev4_0Rooms = true,
        Includev5_0Rooms = true,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
        ReduceDripperVariance = true,
        ChangePalacePallettes = true,
        NoDuplicateRoomsByLayout = true,
        DarkLinkMinDistance = BossRoomMinDistance.SHORT,

        //Levels
        AttackEffectiveness = AttackEffectiveness.HIGH,
        MagicEffectiveness = MagicEffectiveness.LOW_COST,
        LifeEffectiveness = LifeEffectiveness.HIGH,

        //Spells
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,

        EnemyXPDrops = XPEffectiveness.RANDOM_HIGH,

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = true,

        ShuffleSmallItems = true,
        PalacesContainExtraKeys = true,
        RandomizeNewKasutoJarRequirements = true,

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
