using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Presets;

public static class MaxRando2025Preset
{
    public static readonly RandomizerConfiguration Preset = new()
    {
        //Start
        ShuffleStartingItems = true,
        ShuffleStartingSpells = true,
        MaxHeartContainers = MaxHeartsOption.RANDOM,
        StartingHeartContainersMin = 1,
        StartingHeartContainersMax = 8,
        StartingTechniques = StartingTechs.RANDOM,
        StartingLives = StartingLives.LivesRandom,

        //Overworld
        PalacesCanSwapContinents = true,
        ShuffleGP = true,
        ShuffleEncounters = true,
        AllowUnsafePathEncounters = true,
        IncludeLavaInEncounterShuffle = true,
        EncounterRate = EncounterRate.HALF,
        AllowConnectionCavesToBeBlocked = true,
        GoodBoots = true,
        HidePalace = null,
        HideKasuto = null,
        ShuffleWhichLocationIsHidden = true,
        WestBiome = Biome.RANDOM_NO_VANILLA,
        EastBiome = Biome.RANDOM_NO_VANILLA,
        MazeBiome = Biome.VANILLALIKE,
        DmBiome = Biome.RANDOM_NO_VANILLA,
        WestClimate = ClimateEnum.CHAOS,
        EastClimate = ClimateEnum.CHAOS,
        DmClimate = ClimateEnum.CHAOS,
        ContinentConnectionType = ContinentConnectionType.ANYTHING_GOES,

        //Palaces
        NormalPalaceStyle = PalaceStyle.RECONSTRUCTED,
        GpStyle = PalaceStyle.RECONSTRUCTED,
        GpLength = PalaceLengthOption.RANDOM,
        Includev4_0Rooms = true,
        IncludeExpertRooms = true,
        TBirdRequired = false,
        PalacesToCompleteMin = 0,
        RestartAtPalacesOnGameOver = true,
        ChangePalacePallettes = true,
        RandomizeBossItemDrop = true,
        PalaceDropStyle = PalaceDropStyle.ANY_EXIT,
        BossRoomsExitType = BossRoomsExitType.PALACE,
        NoDuplicateRoomsByEnemies = true,
        BlockingRoomsInAnyPalace = true,
        HardBosses = true,

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
        ShuffleSwordImmunity = true,
        EnemyXPDrops = XPEffectiveness.RANDOM,

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
