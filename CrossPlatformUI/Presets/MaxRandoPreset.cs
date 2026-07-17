using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class MaxRandoPreset
{
    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        ShuffleStartingItems = true,
        ShuffleStartingSpells = true,
        MaxHeartContainers = MaxHeartsOption.RANDOM,
        StartingHeartContainersMin = 1,
        StartingHeartContainersMax = 8,
        StartingMagicContainersMin = 1,
        StartingMagicContainersMax = 8,
        StartingTechniques = StartingTechs.RANDOM,
        StartingLives = StartingLives.LivesRandom,

        //Overworld
        PalacesCanSwapContinents = true,
        ShuffleGP = true,
        ShuffleEncounters = null,
        AllowUnsafePathEncounters = true,
        IncludeLavaInEncounterShuffle = true,
        EncounterRate = EncounterRate.HALF,
        RiverDevilBlockerOption = RiverDevilBlockerOption.RANDOM,
        EastRocks = null,
        GenerateBaguWoods = null,
        LessImportantLocationsOption = LessImportantLocationsOption.RANDOM,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = null,
        WestBiome = Biome.RANDOM_NO_VANILLA,
        EastBiome = Biome.RANDOM_NO_VANILLA,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM_NO_VANILLA,
        WestClimate = ClimateEnum.RANDOM,
        EastClimate = ClimateEnum.RANDOM,
        DmClimate = ClimateEnum.RANDOM,
        ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RANDOM_PER_PALACE,
        GpStyle = PalaceStyle.RANDOM,
        NormalPalaceLength = PalaceLengthOption.RANDOM,
        GpLength = PalaceLengthOption.RANDOM,
        Includev4_0Rooms = true,
        Includev5_0Rooms = true,
        IncludeExpertRooms = true,
        TBirdRequired = false,
        PalacesToCompleteMin = 0,
        RestartAtPalacesOnGameOver = true,
        Global5050JarDrop = true,
        ReduceDripperVariance = true,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = true,
        PalaceDropStyle = PalaceDropStyle.ANYTHING_GOES,
        BossRoomsExitType = BossRoomsExitType.RANDOM_PER_PALACE,
        NoDuplicateRoomsByLayout = true,
        BlockingRoomsInAnyPalace = true,
        HardBosses = true,
        PalaceItemRoomCount = PalaceItemRoomCount.RANDOM_INCLUDE_ZERO,

        //Levels
        ShuffleAttackExperience = true,
        ShuffleMagicExperience = true,
        ShuffleLifeExperience = true,

        AttackEffectiveness = AttackEffectiveness.AVERAGE,
        MagicEffectiveness = MagicEffectiveness.AVERAGE,
        LifeEffectiveness = LifeEffectiveness.AVERAGE,

        //Spells
        ShuffleLifeRefillAmount = true,
        ShuffleSpellLocations = true,
        DisableMagicContainerRequirements = true,
        RandomizeSpellSpellEnemy = true,
        FireOption = FireOption.RANDOM,

        //Enemies
        ShuffleOverworldEnemies = true,
        ShufflePalaceEnemies = true,
        DripperEnemyOption = DripperEnemyOption.ANY_GROUND_ENEMY,
        MixLargeAndSmallEnemies = true,

        ShuffleEnemyHP = EnemyLifeOption.MEDIUM,
        ShuffleBossHP = EnemyLifeOption.MEDIUM,
        ShuffleXPStealers = true,
        ShuffleXPStolenAmount = true,
        SwordImmunityOption = SwordImmunityOption.SHUFFLE,
        EnemyXPDrops = XPEffectiveness.WIDE,

        //Items
        ShufflePalaceItems = true,
        ShuffleOverworldItems = true,
        MixOverworldAndPalaceItems = true,
        IncludePBagCavesInItemShuffle = true,
        IncludeSwordTechsInShuffle = true,
        IncludeQuestItemsInShuffle = true,
        IncludeSpellsInShuffle = true,

        ShuffleSmallItems = true,
        ShufflePBagAmounts = true,
        RandomizeNewKasutoJarRequirements = true,
        PreventSpellItemChains = false,

        //Drops
        ShuffleItemDropFrequency = true,
        RandomizeDrops = true,
        StandardizeDrops = true,

        //Hints
        EnableHelpfulHints = true,
        EnableSpellItemHints = true,
        EnableTownNameHints = true,

    };
}
