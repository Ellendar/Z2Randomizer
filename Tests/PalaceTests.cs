using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.Tests;

[TestClass]
public class PalaceTests
{
    private static readonly (PalaceLengthOption Option, bool VanillaPool, int[] Min, int[] Max)[] EXPECTED_LENGTHS = [
        (PalaceLengthOption.SHORT, true,   [11, 16, 10, 19, 23, 20, 31], [12, 16, 13, 19, 23, 20, 37]),
        (PalaceLengthOption.MEDIUM, true,  [11, 16, 10, 19, 23, 20, 33], [14, 18, 14, 19, 23, 21, 45]),
        (PalaceLengthOption.FULL, true,    [14, 21, 15, 21, 28, 27, 55], [14, 21, 15, 21, 28, 27, 55]),
        (PalaceLengthOption.RANDOM, true,  [11, 16, 10, 19, 23, 20, 31], [14, 21, 15, 21, 28, 27, 55]),
        (PalaceLengthOption.SHORT, false,  [9, 10, 9, 10, 12, 12, 28],   [12, 15, 13, 15, 17, 17, 37]),
        (PalaceLengthOption.MEDIUM, false, [10, 13, 10, 13, 16, 15, 33], [14, 18, 14, 18, 22, 21, 45]),
        (PalaceLengthOption.FULL, false,   [12, 18, 13, 18, 24, 23, 47], [17, 25, 18, 25, 21, 20, 61]),
        (PalaceLengthOption.RANDOM, false, [8, 10, 8, 10, 14, 14, 28],   [17, 25, 18, 25, 21, 20, 61]),
    ];

    [TestMethod]
    public void RollPalaceLengths_AllLowSeed_MatchesExpected()
    {
        foreach ((PalaceLengthOption option, bool vanillaPool, int[] min, _) in EXPECTED_LENGTHS)
        {
            int[] sizes = Palaces.RollPalaceLengths(
                BuildConfig(option), BuildProps(vanillaPool), new MockRandomMinRoll());

            sizes.Should().Equal(min, $"{option} pool={vanillaPool}");
        }
    }

    [TestMethod]
    public void RollPalaceLengths_AllHighSeed_MatchesExpected()
    {
        foreach ((PalaceLengthOption option, bool vanillaPool, _, int[] max) in EXPECTED_LENGTHS)
        {
            int[] sizes = Palaces.RollPalaceLengths(
                BuildConfig(option), BuildProps(vanillaPool), new MockRandomMaxRoll());

            sizes.Should().Equal(max, $"{option} pool={vanillaPool}");
        }
    }

    private static RandomizerConfiguration BuildConfig(PalaceLengthOption option) => new()
    {
        NormalPalaceLength = option,
        GpLength = option
    };

    private static RandomizerProperties BuildProps(bool vanillaPool)
    {
        PalaceStyle style = vanillaPool ? PalaceStyle.VANILLA : PalaceStyle.SEQUENTIAL;
        return new RandomizerProperties
        {
            PalaceStyles = Enumerable.Range(0, 7).Select(_ => style).ToArray()
        };
    }

    #region Drop bypasses boss

    // Every test palace below shares one skeleton, laid out on a 2-row grid. Reading it as a
    // map: E is the entrance, B is the boss (passthrough or final), v is a one-way drop, and
    // -- / | are ordinary two-way doors. A gap where a door would be means a wall.
    //
    //   E--A--B--C       E--A--B--C       E--A  B--C
    //      |     |          v     |          v     |
    //      D--F--G          D--F--G          D--F--G
    //    corridor          drop           left-only
    //
    // In all three, B is a miniboss room, and C sits directly east of the boss, so stepping
    // left from C walks into the boss. The three differ only in how C is reached:
    //   corridor  - a normal door loop (A->D->F->G->C) with no drop;
    //   drop      - the loop hangs off a drop out of A (A drops to D), so C is only reachable
    //               after taking a drop;
    //   left-only - same drop loop, but the boss has no opening on its left, so the only way in
    //               is walking left out of C.

    private static (Palace Palace, Room Boss) BuildCorridorBypassPalace()
        => BuildBossBypassSkeleton(dropFromA: false, bossEnterableGoingRight: true);

    private static (Palace Palace, Room Boss) BuildDropBypassPalace(
        bool isBossRoom = false, bool isThunderBirdRoom = false)
        => BuildBossBypassSkeleton(dropFromA: true, bossEnterableGoingRight: true,
                                   isBossRoom: isBossRoom, isThunderBirdRoom: isThunderBirdRoom);

    private static (Palace Palace, Room Boss) BuildLeftOnlyBossPalace()
        => BuildBossBypassSkeleton(dropFromA: true, bossEnterableGoingRight: false);

    private static (Palace Palace, Room Boss) BuildBossBypassSkeleton(
        bool dropFromA, bool bossEnterableGoingRight,
        bool isBossRoom = false, bool isThunderBirdRoom = false)
    {
        Room e = BuildRoom("Entrance", isEntrance: true, hasRightExit: true);
        Room a = BuildRoom("A", hasLeftExit: true, hasRightExit: bossEnterableGoingRight,
                           hasDownExit: true, hasDrop: dropFromA);
        Room b = BuildRoom("Boss", hasLeftExit: bossEnterableGoingRight, hasRightExit: true,
                           hasBoss: true, isBossRoom: isBossRoom, isThunderBirdRoom: isThunderBirdRoom);
        Room c = BuildRoom("C", hasLeftExit: true, hasDownExit: true);
        Room d = BuildRoom("D", hasRightExit: true, hasUpExit: !dropFromA, isDropZone: dropFromA);
        Room f = BuildRoom("F", hasLeftExit: true, hasRightExit: true);
        Room g = BuildRoom("G", hasLeftExit: true, hasUpExit: true);

        e.Right = a;
        a.Left = e;
        a.Down = d;
        if (bossEnterableGoingRight)
        {
            a.Right = b;
            b.Left = a;
        }
        b.Right = c;
        c.Left = b;
        c.Down = g;
        if (!dropFromA)
        {
            //A drop is one-way; an ordinary down door is not.
            d.Up = a;
        }
        d.Right = f;
        f.Left = d;
        f.Right = g;
        g.Left = f;
        g.Up = c;

        return (BuildPalace(e, [e, a, b, c, d, f, g]), b);
    }

    [TestMethod]
    public void DropBypass_BossWithCorridorToItsEast_IsNeverReachable()
    {
        (Palace palace, _) = BuildCorridorBypassPalace();
        palace.AllReachable(dropsMayBypassBosses: false).Should().BeFalse();
        palace.AllReachable(dropsMayBypassBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_BossEastSideReachableOnlyByDrop_RequiresOption()
    {
        (Palace palace, Room boss) = BuildDropBypassPalace();

        //Without the option, the drop does not excuse reaching the boss's east side.
        palace.AllReachable(dropsMayBypassBosses: false).Should().BeFalse();

        //With the option the drop bypass is allowed, and the boss is still enterable going right.
        palace.AllReachable(dropsMayBypassBosses: true).Should().BeTrue();
        palace.GetReachableRooms(dropsMayBypassMiniBosses: true).Should().Contain([boss]);
    }

    [TestMethod]
    public void DropBypass_FinalBossRoom_IsNotRelaxedByMiniOption()
    {
        (Palace palace, _) = BuildDropBypassPalace(isBossRoom: true);
        palace.AllReachable(dropsMayBypassBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_FinalBossRoom_IsRelaxedByFinalOption()
    {
        (Palace palace, Room boss) = BuildDropBypassPalace(isBossRoom: true);

        //The mini boss option does not excuse the final boss.
        palace.AllReachable(dropsMayBypassBosses: true, dropsMayBypassFinalBosses: false).Should().BeFalse();

        //The final boss has its own switch, exercised here even though no generator passes it yet.
        palace.AllReachable(dropsMayBypassFinalBosses: true).Should().BeTrue();
        palace.GetReachableRooms(dropsMayBypassFinalBosses: true).Should().Contain([boss]);
    }

    [TestMethod]
    public void DropBypass_FinalOption_DoesNotRelaxMiniBosses()
    {
        (Palace palace, _) = BuildDropBypassPalace();
        palace.AllReachable(dropsMayBypassFinalBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_FinalBossCorridor_IsRejectedEvenWithFinalOption()
    {
        (Palace palace, _) = BuildBossBypassSkeleton(dropFromA: false, bossEnterableGoingRight: true,
                                                    isBossRoom: true);
        palace.AllReachable(dropsMayBypassFinalBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_FinalBossOnlyEnterableGoingLeft_IsRejectedEvenWithFinalOption()
    {
        (Palace palace, _) = BuildBossBypassSkeleton(dropFromA: true, bossEnterableGoingRight: false,
                                                    isBossRoom: true);
        palace.AllReachable(dropsMayBypassFinalBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_FinalOption_DoesNotRelaxRequiredThunderbird()
    {
        (Palace palace, _) = BuildDropBypassPalace(isThunderBirdRoom: true);
        palace.AllReachable(dropsMayBypassFinalBosses: true, tBirdRequired: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_RequiredThunderbird_IsNeverRelaxed()
    {
        (Palace palace, _) = BuildDropBypassPalace(isThunderBirdRoom: true);
        palace.AllReachable(dropsMayBypassBosses: true, tBirdRequired: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_OptionalThunderbird_IsRelaxed()
    {
        (Palace palace, _) = BuildDropBypassPalace(isThunderBirdRoom: true);
        palace.AllReachable(dropsMayBypassBosses: true, tBirdRequired: false).Should().BeTrue();
    }

    [TestMethod]
    public void DropBypass_BossOnlyEnterableGoingLeft_IsRejectedEvenWithOption()
    {
        (Palace palace, _) = BuildLeftOnlyBossPalace();
        palace.AllReachable(dropsMayBypassBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void AllowMiniBossEnterGoingLeft_AcceptsBossBypassPalaces()
    {
        //For mini bosses the option turns off the strict check, the drop-aware check and the
        //"every boss needs a non-left entry" requirement, leaving only plain reachability. The
        //final boss is still validated (see the _IsRejected test below).
        (Palace corridor, _) = BuildCorridorBypassPalace();
        corridor.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeTrue();

        (Palace drop, _) = BuildDropBypassPalace();
        drop.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeTrue();

        (Palace leftOnly, _) = BuildLeftOnlyBossPalace();
        leftOnly.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeTrue();
    }

    [TestMethod]
    public void AllowMiniBossEnterGoingLeft_AcceptsRequiredThunderbird()
    {
        (Palace thunderbird, _) = BuildDropBypassPalace(isThunderBirdRoom: true);
        thunderbird.AllReachable(allowMiniBossEnterGoingLeft: true, tBirdRequired: true).Should().BeTrue();
    }

    [TestMethod]
    public void AllowMiniBossEnterGoingLeft_FinalBossEnteredGoingLeft_IsRejected()
    {
        //The miniboss option is out of scope for the final boss: it must never be enterable going
        //left, so it is still validated even when the miniboss option is on.
        (Palace finalBoss, _) = BuildDropBypassPalace(isBossRoom: true);
        finalBoss.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeFalse();
    }

    [TestMethod]
    public void AllowFinalBossEnterGoingLeft_AcceptsFinalBossBypass_ButNotMiniBossBypass()
    {
        //No generator passes this yet: the game cannot support final bosses entered going left, so
        //the logic is kept ready behind its own switch. It must not leak into miniboss behaviour.
        (Palace finalBoss, _) = BuildDropBypassPalace(isBossRoom: true);
        finalBoss.AllReachable(allowFinalBossEnterGoingLeft: true).Should().BeTrue();

        (Palace miniBoss, _) = BuildDropBypassPalace();
        miniBoss.AllReachable(allowFinalBossEnterGoingLeft: true).Should().BeFalse();
    }

    [TestMethod]
    public void AllowMiniBossEnterGoingLeft_UnsolvablePalace_StillReturnsFalse()
    {
        (Palace palace, _) = BuildUnreachableBossPalace();
        palace.AllReachable().Should().BeFalse();
        palace.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeFalse();
    }

    [TestMethod]
    public void RequiredThunderbird_StillGatesTheBoss()
    {
        //The gate is enforced by RequiresThunderbird (a tree walk that treats the Thunderbird
        //room as a wall), not by AllReachable, so boss-entry leniency cannot bypass it.
        (Palace gated, _) = BuildThunderbirdGatePalace(tbirdGates: true);
        gated.RequiresThunderbird().Should().BeTrue();

        (Palace sideBranch, _) = BuildThunderbirdGatePalace(tbirdGates: false);
        sideBranch.RequiresThunderbird().Should().BeFalse();
    }

    // An unsolvable palace: the boss's half of the map is walled off from the entrance, so no
    // amount of boss-entry leniency can make every room reachable.
    //
    //   E--A   B--C
    //
    private static (Palace Palace, Room Boss) BuildUnreachableBossPalace()
    {
        Room e = BuildRoom("Entrance", isEntrance: true, hasRightExit: true);
        Room a = BuildRoom("A", hasLeftExit: true);
        Room b = BuildRoom("Boss", isBossRoom: true, hasBoss: true, hasRightExit: true);
        Room c = BuildRoom("C", hasLeftExit: true);

        e.Right = a;
        a.Left = e;
        b.Right = c;
        c.Left = b;

        return (BuildPalace(e, [e, a, b, c]), b);
    }

    // Palace 7 only. When tbirdGates, the Thunderbird room sits between the entrance and the
    // boss; otherwise it hangs off a side branch and the boss is reachable without it.
    //
    //   E--T--B        E--B
    //                  |
    //                  T
    private static (Palace Palace, Room Boss) BuildThunderbirdGatePalace(bool tbirdGates)
    {
        Room e = BuildRoom("Entrance", isEntrance: true, hasRightExit: true, hasDownExit: !tbirdGates);
        Room t = BuildRoom("Thunderbird", isThunderBirdRoom: true,
                           hasLeftExit: tbirdGates, hasRightExit: tbirdGates, hasUpExit: !tbirdGates);
        Room b = BuildRoom("Boss", isBossRoom: true, hasBoss: true, hasLeftExit: true);
        t.PalaceNumber = 7;

        if (tbirdGates)
        {
            e.Right = t;
            t.Left = e;
            t.Right = b;
            b.Left = t;
        }
        else
        {
            e.Right = b;
            e.Down = t;
            b.Left = e;
            t.Up = e;
        }

        return (BuildPalace(e, [e, t, b], number: 7, bossRoom: b), b);
    }

    private static Palace BuildPalace(Room entrance, List<Room> rooms,
        int number = 1, Room? bossRoom = null)
    {
        Palace palace = new(number, false);
        palace.AllRooms.AddRange(rooms);
        palace.Entrance = entrance;
        palace.BossRoom = bossRoom;
        return palace;
    }

    private static Room BuildRoom(string name, bool isEntrance = false, bool hasBoss = false,
        bool isBossRoom = false, bool isThunderBirdRoom = false, bool hasDrop = false,
        bool isDropZone = false, bool hasLeftExit = false, bool hasRightExit = false,
        bool hasUpExit = false, bool hasDownExit = false)
    {
        Room room = new()
        {
            Name = name,
            Group = RoomGroup.VANILLA,
            Enabled = true,
            IsEntrance = isEntrance,
            HasBoss = hasBoss,
            IsBossRoom = isBossRoom,
            IsThunderBirdRoom = isThunderBirdRoom,
            HasDrop = hasDrop,
            IsDropZone = isDropZone,
            SideView = [0x04, 0x60, 0x00, 0x08],
            Enemies = [0x01],
            ItemGetBits = [0x0F],
            Connections =
            [
                hasLeftExit ? (byte)0 : (byte)0xFC,
                hasDownExit ? (byte)0 : (byte)0xFC,
                hasUpExit ? (byte)0 : (byte)0xFC,
                hasRightExit ? (byte)0 : (byte)0xFC,
            ],
        };
        room.OnDeserialized();
        return room;
    }

    #endregion
}
