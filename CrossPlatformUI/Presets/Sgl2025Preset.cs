using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class Sgl2025Preset
{
    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        StartWithCandle = true,
        StartWithCross = true,
        StartWithFire = true,
        MaxHeartContainers = MaxHeartsOption.SIX,
        StartingHeartContainersMin = 3,
        StartingHeartContainersMax = 3,

        //Overworld
        PalacesCanSwapContinents = true,
        ShuffleGP = true,
        EncounterRate = EncounterRate.HALF,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        HidePalace = null,
        HideKasuto = null,
        WestBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        EastBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM_NO_VANILLA_OR_SHUFFLE,
        ContinentConnectionType = ContinentConnectionType.TRANSPORTATION_SHUFFLE,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RECONSTRUCTED,
        GpStyle = PalaceStyle.RECONSTRUCTED,
        NormalPalaceLength = PalaceLengthOption.SHORT,
        GpLength = PalaceLengthOption.MEDIUM,
        Includev4_0Rooms = true,
        IncludeExpertRooms = true,
        RestartAtPalacesOnGameOver = true,
        ChangePalacePallettes = true,
        PalaceDropStyle = PalaceDropStyle.ANY_EXIT,
        NoDuplicateRoomsByEnemies = true,
        BlockingRoomsInAnyPalace = true,
        HardBosses = true,
        AggressiveTbird = true,

        //Levels
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,

        AttackLevelCap = 6,
        AttackEffectiveness = AttackEffectiveness.AVERAGE_LOW, // was AttackValues = [2, 3, 4, 6, 8, 12, 14, 16];
        MagicEffectiveness = MagicEffectiveness.AVERAGE,

        //Spells
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        SwapUpAndDownStab = true,
        // +"Expensive Thunder"

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        MixLargeAndSmallEnemies = true,

        ShuffleEnemyHP = EnemyLifeOption.MEDIUM, // was 100-150%
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        EnemyXPDrops = XPEffectiveness.SLIGHTLY_HIGH, // was +0 to +2

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,


        //Drops
        StandardizeDrops = true,
        SmallEnemiesCanDropBlueJar = true,
        LargeEnemiesCanDropRedJar = true,
        LargeEnemiesCanDropLargeBag = true,

        //Hints
        EnableSpellItemHints = true,
        EnableTownNameHints = true,

    };
}
