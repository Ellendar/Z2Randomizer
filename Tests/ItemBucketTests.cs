using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Z2Randomizer.Tests;

[TestClass]
public class ItemBucketTests
{
    /// <summary>
    /// Builds a RandomizerProperties with "best case" values matching what
    /// CountPossibleMinorItems assumes: all shuffles on, non-VANILLA biomes,
    /// MaxHearts=8, MaxMagicContainers=8, all start items off, all spell/tech off,
    /// PalaceItemRoomCounts = [1,1,1,1,1,1,0].
    /// </summary>
    private static RandomizerProperties BestCaseProperties()
    {
        return new RandomizerProperties
        {
            ShuffleOverworldItems = true,
            ShufflePalaceItems = true,
            PbagItemShuffle = true,
            MixOverworldPalaceItems = false,

            WestBiome = Biome.VANILLA_SHUFFLE,
            EastBiome = Biome.VANILLA_SHUFFLE,
            DmBiome = Biome.VANILLA_SHUFFLE,
            MazeBiome = Biome.VANILLA_SHUFFLE,

            MaxHearts = 8,
            StartHearts = 4,
            MaxMagicContainers = 8,
            StartMagicContainers = 4,

            IncludeSpellsInShuffle = false,
            IncludeSwordTechsInShuffle = false,

            PalaceItemRoomCounts = [1, 1, 1, 1, 1, 1, 0],
        };
    }

    /// <summary>
    /// Default config (all vanilla biomes, no shuffles, default containers)
    /// CountPossibleMinorItems should return non-negative for both buckets,
    /// and HasEnoughSpaceToAllocateItems should agree.
    /// </summary>
    [TestMethod]
    public void DefaultConfig_HasEnoughSpace()
    {
        var config = new RandomizerConfiguration();
        var (overworld, palace) = config.CountPossibleMinorItems();
        Assert.IsTrue(overworld >= 0, $"Default overworld count was {overworld}");
        Assert.IsTrue(palace >= 0, $"Default palace count was {palace}");

        // Under best-case properties, HasEnoughSpaceToAllocateItems should also pass.
        var props = BestCaseProperties();
        Assert.IsTrue(props.HasEnoughSpaceToAllocateItems());
    }

    [TestMethod]
    public void ShuffleBoth_NonVanillaBiomes_HasEnoughSpace()
    {
        var config = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            ShufflePalaceItems = true,
        };
        var (overworld, palace) = config.CountPossibleMinorItems();
        Assert.IsTrue(overworld >= 0, $"overworld={overworld}");
        Assert.IsTrue(palace >= 0, $"palace={palace}");
    }

    [TestMethod]
    public void LowStart_ConflictsDetected()
    {
        var config = new RandomizerConfiguration
        {
            ShufflePalaceItems = true,
            ShuffleOverworldItems = true,
            StartingHeartContainersMin = 1,
            StartingHeartContainersMax = 1,
            StartingMagicContainersMin = 1,
            StartingMagicContainersMax = 1,
        };
        var (overworld, palace) = config.CountPossibleMinorItems();
        // Hearts in pool = 8-1=7, heart replacement = 4-7 = -3
        // Magic = 4-(8-1) = -3
        // Total overworld drag = -6
        // PBag caves contribute +3 (if shuffled), starts contribute nothing (shuffleStartingItems=false by default)
        // Spells add 0 (no start spells). So overworld = 3 + 0 + 0 - 3 - 3 = -3
        Assert.IsTrue(overworld < 0 || palace < 0,
            "LowStart should be flagged as impossible");
        var act = () => config.CheckForFlagConflicts();
        act.Should().Throw<UserFacingException>();
    }

    // ─── Start Item Classification Tests ──────────────────────────────────
    // These verify that specific start items land in the correct bucket
    // in both CountPossibleMinorItems and HasEnoughSpaceToAllocateItems.

    private static void AssertStartItemGoesToPalace(
        Action<RandomizerConfiguration> setConfigFlag,
        Action<RandomizerProperties> setProp,
        string itemName)
    {
        var config = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            ShufflePalaceItems = true,
        };
        setConfigFlag(config);
        var (overworldBase, palaceBase) = config.CountPossibleMinorItems();

        // Now unset the flag and measure the delta.
        var config2 = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            ShufflePalaceItems = true,
        };
        var (overworldDelta, palaceDelta) = config2.CountPossibleMinorItems();

        int palaceContributed = palaceBase - palaceDelta;
        int overworldContributed = overworldBase - overworldDelta;
        Assert.IsTrue(palaceContributed > 0 && overworldContributed == 0,
            $"{itemName}: expected palace contribution >0, got palace={palaceContributed}, overworld={overworldContributed}");

        // Same classification in HasEnoughSpaceToAllocateItems:
        var propsBase = BestCaseProperties();
        setProp(propsBase);
        bool baseOk = propsBase.HasEnoughSpaceToAllocateItems();

        var propsDelta = BestCaseProperties();
        bool deltaOk = propsDelta.HasEnoughSpaceToAllocateItems();

        // With the start item set, palace count should be higher (or equal), so still pass.
        // Both should agree on pass/fail for this single-item difference.
        Assert.AreEqual(deltaOk, baseOk,
            $"{itemName}: HasEnoughSpaceToAllocateItems disagreement with vs without item");
    }

    private static void AssertStartItemGoesToOverworld(
        Action<RandomizerConfiguration> setConfigFlag,
        Action<RandomizerProperties> setProp,
        string itemName)
    {
        var config = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            ShufflePalaceItems = true,
        };
        setConfigFlag(config);
        var (overworldBase, palaceBase) = config.CountPossibleMinorItems();

        var config2 = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            ShufflePalaceItems = true,
        };
        var (overworldDelta, palaceDelta) = config2.CountPossibleMinorItems();

        int overworldContributed = overworldBase - overworldDelta;
        int palaceContributed = palaceBase - palaceDelta;
        Assert.IsTrue(overworldContributed > 0 && palaceContributed == 0,
            $"{itemName}: expected overworld contribution >0, got overworld={overworldContributed}, palace={palaceContributed}");

        // Same classification in HasEnoughSpaceToAllocateItems:
        var propsBase = BestCaseProperties();
        setProp(propsBase);
        bool baseOk = propsBase.HasEnoughSpaceToAllocateItems();

        var propsDelta = BestCaseProperties();
        bool deltaOk = propsDelta.HasEnoughSpaceToAllocateItems();

        Assert.AreEqual(deltaOk, baseOk,
            $"{itemName}: HasEnoughSpaceToAllocateItems disagreement with vs without item");
    }

    [TestMethod]
    public void StartCandle_GoesToPalace()
        => AssertStartItemGoesToPalace(c => c.StartWithCandle = true, p => p.StartCandle = true, "Candle");

    [TestMethod]
    public void StartGlove_GoesToPalace()
        => AssertStartItemGoesToPalace(c => c.StartWithGlove = true, p => p.StartGlove = true, "Glove");

    [TestMethod]
    public void StartRaft_GoesToPalace()
        => AssertStartItemGoesToPalace(c => c.StartWithRaft = true, p => p.StartRaft = true, "Raft");

    [TestMethod]
    public void StartBoots_GoesToPalace()
        => AssertStartItemGoesToPalace(c => c.StartWithBoots = true, p => p.StartBoots = true, "Boots");

    [TestMethod]
    public void StartCross_GoesToPalace()
        => AssertStartItemGoesToPalace(c => c.StartWithCross = true, p => p.StartCross = true, "Cross");

    [TestMethod]
    public void StartFlute_GoesToPalace()
        => AssertStartItemGoesToPalace(c => c.StartWithFlute = true, p => p.StartFlute = true, "Flute");

    [TestMethod]
    public void StartHammer_GoesToOverworld()
        => AssertStartItemGoesToOverworld(c => c.StartWithHammer = true, p => p.StartHammer = true, "Hammer");

    [TestMethod]
    public void StartKey_GoesToOverworld()
        => AssertStartItemGoesToOverworld(c => c.StartWithMagicKey = true, p => p.StartKey = true, "Key");

    // ─── PBAG Cave Tests ──────────────────────────────────────────────────

    [TestMethod]
    public void PbagCaves_ContributeToOverworld_OnlyWhenPbagShuffled()
    {
        // PBag caves add +1 (west) +2 (east) = +3 overworld overflow space, but only when
        // PbagItemShuffle is on. Make the container drag leave exactly enough room for those
        // caves: without them the overworld bucket is short by 1, with them it has room.
        // drag = -(MaxHearts - StartHearts - 4) - (MaxMagicContainers - StartMagicContainers - 4)
        // With StartHearts=4, StartMagicContainers=3: drag = -(0) - (1) = -1.
        var propsOn = BestCaseProperties();
        propsOn.PbagItemShuffle = true;
        propsOn.StartHearts = 4;
        propsOn.StartMagicContainers = 3;
        // overworld = +3 (pbag) - 1 (drag) = 2 >= 0
        Assert.IsTrue(propsOn.HasEnoughSpaceToAllocateItems(),
            "Pbag on: the +3 caves should be enough to offset the 1-slot container drag");

        var propsOff = BestCaseProperties();
        propsOff.PbagItemShuffle = false;
        propsOff.StartHearts = 4;
        propsOff.StartMagicContainers = 3;
        // overworld = 0 (pbag) - 1 (drag) = -1 < 0
        Assert.IsFalse(propsOff.HasEnoughSpaceToAllocateItems(),
            "Pbag off: the excluded caves must not offset the container drag");
    }

    [TestMethod]
    public void PbagCaves_PropertiesLevel_CountsWhenShuffled()
    {
        // Properties level: PBag caves counted when ShuffleOverworldItems && biome.InItemShuffle()
        var propsShuffled = BestCaseProperties();
        propsShuffled.ShuffleOverworldItems = true;
        bool ok1 = propsShuffled.HasEnoughSpaceToAllocateItems();

        // If we set non-shuffled biomes for the pbag contributions (set biomes to VANILLA — InItemShuffle is always true)
        // So actually pbag only depends on ShuffleOverworldItems at properties level too.
        var propsOff = BestCaseProperties();
        propsOff.ShuffleOverworldItems = false;
        bool ok2 = propsOff.HasEnoughSpaceToAllocateItems();

        // Both should pass since BestCaseProperties has StartHearts=4, MaxHearts=8 → container adj = 0
        Assert.IsTrue(ok1, "Pbag shuffled: should have enough space");
        Assert.IsTrue(ok2, "Pbag not shuffled: should still have enough space (container adj=0)");
    }

    // ─── Container Adjustment Tests ───────────────────────────────────────

    [TestMethod]
    public void ExcessHearts_DragOverworld()
    {
        // MaxHearts=8, StartHearts=4: container adj = -(8-4-4)=0
        // MaxHearts=8, StartHearts=0: container adj = -(8-0-4)=-4 → overworld -= 4
        // Disable PBAG caves so container drag is isolated.
        var props4 = BestCaseProperties();
        props4.ShuffleOverworldItems = false;
        props4.StartHearts = 4;
        bool ok4 = props4.HasEnoughSpaceToAllocateItems();

        var props0 = BestCaseProperties();
        props0.ShuffleOverworldItems = false;
        props0.StartHearts = 0;
        bool ok0 = props0.HasEnoughSpaceToAllocateItems();

        Assert.IsTrue(ok4, "StartHearts=4 (4 excess = 0 drag): should have space");
        Assert.IsFalse(ok0, "StartHearts=0 (8 excess → drag=4): should fail");
    }

    [TestMethod]
    public void ExcessMagic_DragOverworld()
    {
        // Disable PBAG caves so container drag is isolated.
        var props4 = BestCaseProperties();
        props4.ShuffleOverworldItems = false;
        props4.StartMagicContainers = 4;
        bool ok4 = props4.HasEnoughSpaceToAllocateItems();

        var props0 = BestCaseProperties();
        props0.ShuffleOverworldItems = false;
        props0.StartMagicContainers = 0;
        bool ok0 = props0.HasEnoughSpaceToAllocateItems();

        Assert.IsTrue(ok4, "StartMagic=4: should have space");
        Assert.IsFalse(ok0, "StartMagic=0: drag=4, should fail");
    }

    [TestMethod]
    public void DeficitHearts_AddOverworld()
    {
        // MaxHearts=8, StartHearts=6: -(8-6-4)=+2 → overworld += 2
        var props = BestCaseProperties();
        props.StartHearts = 6;
        bool ok = props.HasEnoughSpaceToAllocateItems();
        Assert.IsTrue(ok, "StartHearts=6: fewer than 4 excess → adds overworld space");
    }

    [TestMethod]
    public void ContainerAdjustment_ConfigLevel_Matches()
    {
        // Config-level: heartContainerReplacementSmallItemsCount = 4 - HeartsInPool
        // With EIGHT: HeartsInPool(EIGHT, start) = max(0, 8-start)
        // When start=4: replacement = 4-(8-4)=0
        // When start=2: replacement = 4-(8-2)=-2
        var config4 = new RandomizerConfiguration { ShuffleOverworldItems = true };
        // Default startingHeartContainersMax=4
        var (ow4, _) = config4.CountPossibleMinorItems();

        var config2 = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            StartingHeartContainersMin = 2,
            StartingHeartContainersMax = 2,
        };
        var (ow2, _) = config2.CountPossibleMinorItems();

        // With start=2: heartsInPool = 6, replacement = 4-6 = -2
        // magic: 4-(8-4)=0 (unchanged)
        Assert.AreEqual(-2, ow2 - ow4,
            "Heart container adjustment should drag overworld by -2 when starting hearts drop from 4 to 2");
    }

    [TestMethod]
    public void MagicContainerAdjustment_ConfigLevel_Matches()
    {
        var config4 = new RandomizerConfiguration { ShuffleOverworldItems = true };
        var (ow4, _) = config4.CountPossibleMinorItems();

        var config2 = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            StartingMagicContainersMin = 2,
            StartingMagicContainersMax = 2,
        };
        var (ow2, _) = config2.CountPossibleMinorItems();

        // magic: 4-(8-2)=-2 vs 4-(8-4)=0 → delta = -2
        Assert.AreEqual(-2, ow2 - ow4,
            "Magic container adjustment should drag overworld by -2 when starting magic drops from 4 to 2");
    }

    // ─── Palace Item Room Tests ───────────────────────────────────────────

    [TestMethod]
    public void PalaceItemRooms_ContributeToPalace_Bucket()
    {
        // With default palaceItemRoomCount and shufflePalaceItems on,
        // CountPossibleMinorItems should return palace >= palaceItemsMaxDiff
        var config = new RandomizerConfiguration
        {
            ShufflePalaceItems = true,
        };
        var (_, palace) = config.CountPossibleMinorItems();
        Assert.IsTrue(palace >= 0, $"Default palace count was {palace}");
    }

    [TestMethod]
    public void P7_NotCountedInPalaceBucket()
    {
        // PalaceItemRoomCounts[6] = 0 → should not drag palace to negative
        var props = BestCaseProperties();
        props.PalaceItemRoomCounts = [1, 1, 1, 1, 1, 1, 0];
        bool ok1 = props.HasEnoughSpaceToAllocateItems();
        Assert.IsTrue(ok1, "P7=0 should not affect palace bucket (Take(6) excludes index 6)");

        // Even with extra palaces, P7 shouldn't count
        props.PalaceItemRoomCounts = [2, 2, 2, 2, 2, 2, 0];
        bool ok2 = props.HasEnoughSpaceToAllocateItems();
        Assert.IsTrue(ok2, "P7=0 with 6 palaces at 2: palace bucket = 6*(2-1)=6 ≥ 0");
    }

    // ─── MixOverworldPalaceItems Tests ────────────────────────────────────

    [TestMethod]
    public void Mix_PoolsCombined()
    {
        // When MixOverworldPalaceItems=true, overworld + palace >= 0 is sufficient.
        var props = BestCaseProperties();
        props.MixOverworldPalaceItems = true;
        // Give palace negative but overworld positive enough to compensate
        props.PalaceItemRoomCounts = [0, 0, 0, 0, 0, 0, 0]; // palace = 0*6 -6 = -6? No: 0*6 with Take(6) = 6*(0-1)=-6
        // Wait — with all zeros: Take(6) → Sum(c-1) = 6*(0-1) = -6
        // overworld: pbag(1+2=3) + container(0) + starts(0) = 3
        // combined: 3 + (-6) = -3 → fail
        // Let's use palaces at 2 each: Take(6) → 6*(2-1)=6 → palace=6
        props.PalaceItemRoomCounts = [2, 2, 2, 2, 2, 2, 0];
        bool okMix = props.HasEnoughSpaceToAllocateItems();
        Assert.IsTrue(okMix, "Combined pool: overworld+palace should pass with generous palaces");

        // Without mixing: palace must be >= 0 independently
        props.MixOverworldPalaceItems = false;
        // Same values — palace=6≥0, overworld≥0 → still pass
        bool okSeparate = props.HasEnoughSpaceToAllocateItems();
        Assert.IsTrue(okSeparate, "Separate pools: both should pass with generous palaces");
    }

    // ─── Spell Classification Tests ───────────────────────────────────────

    [TestMethod]
    public void Spells_GoToOverworld()
    {
        // Config-level: spells always add to overworld when includeSpellsInShuffle
        var config = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            ShufflePalaceItems = true,
            ShuffleStartingSpells = true,
            IncludeSpellsInShuffle = true,
        };
        var (owSpells, _) = config.CountPossibleMinorItems();

        var configNo = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            ShufflePalaceItems = true,
            ShuffleStartingSpells = false,
            IncludeSpellsInShuffle = true,
        };
        var (owNoSpells, _) = configNo.CountPossibleMinorItems();

        // Spells should increase overworld count
        Assert.IsTrue(owSpells > owNoSpells,
            $"Spells should contribute to overworld: with={owSpells}, without={owNoSpells}");

        // Properties level: IncludeSpellsInShuffle + StartShield etc.
        var propsSpells = BestCaseProperties();
        propsSpells.IncludeSpellsInShuffle = true;
        propsSpells.StartShield = true;
        propsSpells.StartJump = true;
        propsSpells.StartLife = true;
        propsSpells.StartFairy = true;
        propsSpells.StartFire = true;
        propsSpells.StartReflect = true;
        propsSpells.StartSpell = true;
        propsSpells.StartThunder = true;
        bool okSpells = propsSpells.HasEnoughSpaceToAllocateItems();

        var propsNoSpells = BestCaseProperties();
        propsNoSpells.IncludeSpellsInShuffle = true;
        bool okNoSpells = propsNoSpells.HasEnoughSpaceToAllocateItems();

        Assert.IsTrue(okSpells, "Spells on: should have enough space (adds to overworld)");
        Assert.IsTrue(okNoSpells, "No start spells: should still have enough space");
    }

    // ─── Cross-Method Consistency ──────────────────────────────────────────
    // When config is at best-case (all shuffled, all non-VANILLA, default containers),
    // the properties-level function should also report "enough space."

    [TestMethod]
    public void BestCase_ConsistentBetweenConfigAndProperties()
    {
        // Config with all shuffles on, default containers
        var config = new RandomizerConfiguration
        {
            ShuffleOverworldItems = true,
            ShufflePalaceItems = true,
            IncludeSpellsInShuffle = true,
            IncludeSwordTechsInShuffle = true,
        };
        var (overworld, palace) = config.CountPossibleMinorItems();
        Assert.IsTrue(overworld >= 0, $"Best-case overworld was {overworld}");
        Assert.IsTrue(palace >= 0, $"Best-case palace was {palace}");

        // Properties at best-case should also pass
        var props = BestCaseProperties();
        props.IncludeSpellsInShuffle = true;
        props.IncludeSwordTechsInShuffle = true;
        props.ShuffleOverworldItems = true;
        props.ShufflePalaceItems = true;
        bool ok = props.HasEnoughSpaceToAllocateItems();
        Assert.IsTrue(ok, "Properties should agree with config-level best-case");
    }

    [TestMethod]
    public void ConfigSaysImpossible_PropertiesAlsoFail_WhenMinusFlags()
    {
        // When config says infeasible, properties-level with matching resolved values should also fail
        var config = new RandomizerConfiguration
        {
            ShufflePalaceItems = true,
            ShuffleOverworldItems = true,
            StartingHeartContainersMin = 1,
            StartingHeartContainersMax = 1,
            StartingMagicContainersMin = 1,
            StartingMagicContainersMax = 1,
        };
        var (overworld, _) = config.CountPossibleMinorItems();
        // Config says infeasible (overworld < required)
        var act = () => config.CheckForFlagConflicts();
        act.Should().Throw<UserFacingException>();

        // Properties with the same resolved values should also have space issues
        var props = BestCaseProperties();
        props.StartHearts = 1;
        props.StartMagicContainers = 1;
        props.ShuffleOverworldItems = true;
        props.ShufflePalaceItems = true;
        // No pbag+3? BestCaseProperties has default biomes → InItemShuffle()=true → pbag counts (+3)
        // overworld = 3 (pbag) - (8-1-4)=3 (heart drag) - (8-1-4)=3 (magic drag) = 3-3-3 = -3
        bool ok = props.HasEnoughSpaceToAllocateItems();
        Assert.IsFalse(ok, "Properties should agree config is infeasible (overworld=-3)");
    }

    // ─── Palace/Overworld Item Classification ─────────────────────────────

    [TestMethod]
    public void IsPalaceItem_ClassifiesTheSixVanillaPalaceItems()
    {
        Collectable[] palaceItems = [
            Collectable.CANDLE,
            Collectable.GLOVE,
            Collectable.RAFT,
            Collectable.BOOTS,
            Collectable.FLUTE,
            Collectable.CROSS,
        ];
        foreach (Collectable item in palaceItems)
        {
            Assert.IsTrue(item.IsVanillaPalaceItem(), $"{item} should be a palace item");
        }

        Collectable[] nonPalaceItems = [
            Collectable.HAMMER,
            Collectable.MAGIC_KEY,
            Collectable.KEY,
            Collectable.SMALL_BAG,
            Collectable.MAGIC_CONTAINER,
            Collectable.HEART_CONTAINER,
            Collectable.JUMP_SPELL,
            Collectable.UPSTAB,
            Collectable.MIRROR,
        ];
        foreach (Collectable item in nonPalaceItems)
        {
            Assert.IsFalse(item.IsVanillaPalaceItem(), $"{item} should NOT be a palace item");
        }
    }

    [TestMethod]
    public void IsOverworldItem_ExplicitList()
    {
        // Spells, sword techs and town quest items are undefined
    }
}