using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

namespace CrossPlatformUI.Presets;

public static class RandomPercentPreset
{
    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        ShuffleStartingItems = true,
        ShuffleStartingSpells = true,
        MaxHeartContainers = MaxHeartsOption.RANDOM,
        StartingHeartContainersMin = 3,
        StartingHeartContainersMax = 8,
        StartingTechniques = StartingTechs.RANDOM,
        StartingLives = StartingLives.LivesRandom,

        //Overworld
        PalacesCanSwapContinents = null,
        ShuffleGP = null,
        ShuffleEncounters = null,
        EncounterRate = EncounterRate.HALF,
        RiverDevilBlockerOption = RiverDevilBlockerOption.RANDOM,
        EastRocks = null,
        GenerateBaguWoods = null,
        LessImportantLocationsOption = LessImportantLocationsOption.RANDOM,
        RestrictConnectionCaveShuffle = null,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = null,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = null,
        WestBiome = Biome.RANDOM,
        EastBiome = Biome.RANDOM,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM,
        WestClimate = ClimateEnum.RANDOM,
        EastClimate = ClimateEnum.RANDOM,
        DmClimate = ClimateEnum.RANDOM,
        ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RANDOM_PER_PALACE,
        GpStyle = PalaceStyle.RANDOM,
        RandomStylesAllowVanilla = true,
        NormalPalaceLength = PalaceLengthOption.RANDOM,
        GpLength = PalaceLengthOption.RANDOM,
        IncludeVanillaRooms = null,
        Includev4_0Rooms = null,
        Includev5_0Rooms = null,
        TBirdRequired = null,
        PalacesToCompleteMin = 0,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = null,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = true,
        PalaceDropStyle = PalaceDropStyle.ANYTHING_GOES,
        BossRoomsExitType = BossRoomsExitType.RANDOM_PER_PALACE,
        NoDuplicateRoomsByLayout = true,
        BlockingRoomsInAnyPalace = true,

        //Levels
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,

        AttackEffectiveness = AttackEffectiveness.AVERAGE,
        MagicEffectiveness = MagicEffectiveness.AVERAGE,
        LifeEffectiveness = LifeEffectiveness.AVERAGE,

        //Spells
        ShuffleLifeRefillAmount = true,
        ShuffleSpellLocations = null,
        DisableMagicContainerRequirements = null,
        RandomizeSpellSpellEnemy = null,
        SwapUpAndDownStab = null,
        FireOption = FireOption.RANDOM,

        //Enemies
        ShuffleOverworldEnemies = null,
        ShufflePalaceEnemies = null,
        DripperEnemyOption = DripperEnemyOption.ANY_GROUND_ENEMY,
        MixLargeAndSmallEnemies = null,
        GeneratorsAlwaysMatch = false,

        ShuffleEnemyHP = EnemyLifeOption.MEDIUM,
        ShuffleBossHP = EnemyLifeOption.MEDIUM,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        ShuffleSwordImmunity = true,
        EnemyXPDrops = XPEffectiveness.RANDOM,

        //Items
        ShufflePalaceItems = null,
        ShuffleOverworldItems = null,
        MixOverworldAndPalaceItems = null,
        IncludePBagCavesInItemShuffle = null,
        IncludeSwordTechsInShuffle = null,
        IncludeQuestItemsInShuffle = null,
        IncludeSpellsInShuffle = null,

        ShuffleSmallItems = true,
        RemoveSpellItems = null,
        ShufflePBagAmounts = null,
        PalacesContainExtraKeys = null,
        RandomizeNewKasutoJarRequirements = true,
        PreventSpellItemChains = false,

        //Drops
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,

        //Hints
        EnableHelpfulHints = null,
        EnableSpellItemHints = null,
        EnableTownNameHints = null,

    };
}
