using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace CrossPlatformUI.Presets;

public static class NormalPreset
{
    public static string Name => "Normal";

    public static string Description => """
A good starting point if you can beat the vanilla game without save states and enjoy the game's challenging combat.
""";

    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        StartWithCandle = true,

        //Overworld
        PalacesCanSwapContinents = true,
        ShuffleGP = true,
        EncounterRate = EncounterRate.HALF,
        RiverDevilBlockerOption = RiverDevilBlockerOption.RANDOM,
        EastRocks = true,
        GenerateBaguWoods = false,
        LessImportantLocationsOption = LessImportantLocationsOption.REMOVE,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        HidePalace = null,
        HideKasuto = null,
        DmSize = DmSizeOption.MEDIUM,
        WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        EastBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        WestClimate = ClimateEnum.VANILLA_WEIGHTED,
        EastClimate = ClimateEnum.VANILLA_WEIGHTED,
        ContinentConnectionType = ContinentConnectionType.TRANSPORTATION_SHUFFLE,

        //Palaces
        NormalPalaceStyle = PalaceStyle.VANILLA_WEIGHTED,
        GpStyle = PalaceStyle.RANDOM_WALK,
        NormalPalaceLength = PalaceLengthOption.MEDIUM,
        GpLength = PalaceLengthOption.SHORT,
        RoomPoolOption = RoomPoolOption.V5_0_NO_EXPERT,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
        ReduceDripperVariance = true,
        ChangePalacePallettes = true,
        PalaceDropStyle = PalaceDropStyle.RANDOM,
        NoDuplicateRoomsByLayout = true,
        BlockingRoomsInAnyPalace = true,
        HardBosses = true,
        DarkLinkMinDistance = BossRoomMinDistance.SHORT,

        //Levels
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,

        AttackEffectiveness = AttackEffectiveness.AVERAGE,
        MagicEffectiveness = MagicEffectiveness.AVERAGE,
        LifeEffectiveness = LifeEffectiveness.AVERAGE,

        //Spells
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        DripperEnemyOption = DripperEnemyOption.EASIER_GROUND_ENEMIES,

        ShuffleEnemyHP = EnemyLifeOption.MEDIUM,
        ShuffleBossHP = EnemyLifeOption.MEDIUM,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        SwordImmunityOption = SwordImmunityOption.SHUFFLE,
        EnemyXPDrops = XPEffectiveness.RANDOM,

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = true,
        IncludeQuestItemsInShuffle = true,

        ShuffleSmallItems = true,
        RandomizeNewKasutoJarRequirements = true,
        PreventSpellItemChains = false,

        //Drops
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,

        //Hints
        HelpfulHints = HelpfulHintOption.CONTINENT_ONLY,
        EnableSpellItemHints = true,
        EnableTownNameHints = true,

    };
}
