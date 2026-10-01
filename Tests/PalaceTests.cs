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

    private static Room AddRoom(
    Palace palace, int x, int y, string name,
    bool isEntrance = false, bool hasItem = false,
    bool hasBoss = false, bool isBossRoom = false, bool isThunderBirdRoom = false,
    bool hasDrop = false, bool isDropZone = false,
    bool hasLeftExit = false, bool hasRightExit = false,
    bool hasUpExit = false, bool hasDownExit = false)
    {
        Room room = new()
        {
            Name = name,
            Group = RoomGroup.VANILLA,
            Enabled = true,
            IsEntrance = isEntrance,
            HasItem = hasItem,
            HasBoss = hasBoss,
            IsBossRoom = isBossRoom,
            IsThunderBirdRoom = isThunderBirdRoom,
            HasDrop = hasDrop,
            IsDropZone = isDropZone,
            coords = new(x, y),
            SideView = [0x04, 0x60, 0x00, 0x08],
            Enemies = [0x01],
            ItemGetBits = [0x0F],
            Connections = BuildConnections(hasLeftExit, hasRightExit, hasUpExit, hasDownExit),
        };
        room.OnDeserialized();
        palace.AllRooms.Add(room);
        if (isEntrance) { palace.Entrance = room; }
        if (isBossRoom) { palace.BossRoom = room; }
        return room;
    }

    private static byte[] BuildConnections(bool hasLeftExit=false, bool hasRightExit=false, bool hasUpExit=false, bool hasDownExit=false)
    {
        return [
                hasLeftExit ? (byte)0 : (byte)0xFC,
                hasDownExit ? (byte)0 : (byte)0xFC,
                hasUpExit ? (byte)0 : (byte)0xFC,
                hasRightExit ? (byte)0 : (byte)0xFC,
            ];
    }

    // Base palace skeleton
    //
    //   E
    //   |
    //   A--C--D--F--I
    //   |
    //   G--b--H--B
    //
    private static Palace PalaceSkeleton(int palaceNumber)
    {
        Palace palace = new(palaceNumber, false);

        AddRoom(palace, 0, 0, "Entrance", isEntrance: true, hasDownExit: true);
        AddRoom(palace, 0, -1, "A", hasUpExit: true, hasRightExit: true, hasDownExit: true);
        AddRoom(palace, 1, -1, "C", hasLeftExit: true, hasRightExit: true);
        AddRoom(palace, 2, -1, "D", hasLeftExit: true, hasRightExit: true);
        AddRoom(palace, 3, -1, "F", hasLeftExit: true, hasRightExit: true);
        AddRoom(palace, 4, -1, "I", hasLeftExit: true, hasItem: true);
        AddRoom(palace, 0, -2, "G", hasUpExit: true, hasRightExit: true);
        if (palaceNumber == 7)
        {
            AddRoom(palace, 1, -2, "T", hasLeftExit: true, hasRightExit: true, isThunderBirdRoom: true);
        }
        else
        {
            AddRoom(palace, 1, -2, "b", hasLeftExit: true, hasRightExit: true, hasBoss: true);
        }
        AddRoom(palace, 2, -2, "H", hasLeftExit: true, hasRightExit: true);
        AddRoom(palace, 3, -2, "B", hasLeftExit: true, hasBoss: true, isBossRoom: true);
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    // Palace continues after final boss
    //
    //   E
    //   |
    //   A--C--D--F--I
    //   |
    //   G--b--H--B--J
    //
    private static Palace ContinueAfterBoss(int palaceNumber)
    {
        Palace palace = PalaceSkeleton(palaceNumber);
        AddRoom(palace, 4, -2, "J", hasLeftExit: true);
        var B = palace.AllRooms.First(room => room.Name == "B");
        B.SetConnections(BuildConnections(hasLeftExit: true, hasRightExit: true));
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    //   E
    //   |
    //   A--C--D--F--I
    //   |     v     v
    //   G--b--H--B--J
    //
    private static Palace BuildDoubleDropBypassPalace(int palaceNumber)
    {
        Palace palace = ContinueAfterBoss(palaceNumber);
        var D = palace.AllRooms.First(room => room.Name == "D");
        var H = palace.AllRooms.First(room => room.Name == "H");
        D.SetConnections(BuildConnections(hasLeftExit: true, hasRightExit: true, hasDownExit: true));
        D.HasDrop = true;
        H.IsDropZone = true;
        var I = palace.AllRooms.First(room => room.Name == "I");
        var J = palace.AllRooms.First(room => room.Name == "J");
        I.SetConnections(BuildConnections(hasLeftExit: true, hasDownExit: true));
        I.HasDrop = true;
        J.IsDropZone = true;
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    // Drop bypass MiniBoss (b) (palace 7 draws T in b's slot)
    //
    //   E
    //   |
    //   A--C--D--F--I
    //   |     v
    //   G--b--H--B--J
    //
    private static Palace BuildDropBypassPalace(int palaceNumber)
    {
        Palace palace = ContinueAfterBoss(palaceNumber);
        var D = palace.AllRooms.First(room => room.Name == "D");
        var H = palace.AllRooms.First(room => room.Name == "H");
        D.SetConnections(BuildConnections(hasLeftExit: true, hasRightExit: true, hasDownExit: true));
        D.HasDrop = true;
        H.IsDropZone = true;
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    // Drop bypass FinalBoss
    //
    //   E
    //   |
    //   A--C--D--F--I
    //   |           v
    //   G--b--H--B--J
    //
    private static Palace BuildFinalBossDropBypassPalace(int palaceNumber)
    {
        Palace palace = ContinueAfterBoss(palaceNumber);
        var I = palace.AllRooms.First(room => room.Name == "I");
        var J = palace.AllRooms.First(room => room.Name == "J");
        I.SetConnections(BuildConnections(hasLeftExit: true, hasDownExit: true));
        I.HasDrop = true;
        J.IsDropZone = true;
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    // Elevator past MiniBoss
    //
    //   E
    //   |
    //   A--C--D--F--I
    //   |     |
    //   G--b--H--B--J
    //
    private static Palace BuildCorridorBypassPalace(int palaceNumber)
    {
        Palace palace = ContinueAfterBoss(palaceNumber);
        var D = palace.AllRooms.First(room => room.Name == "D");
        var H = palace.AllRooms.First(room => room.Name == "H");
        D.SetConnections(BuildConnections(hasLeftExit: true, hasRightExit: true, hasDownExit: true));
        H.SetConnections(BuildConnections(hasLeftExit: true, hasRightExit: true, hasUpExit: true));
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    // Elevator past FinalBoss
    //
    //   E
    //   |
    //   A--C--D--F--I
    //   |           |
    //   G--b--H--B--J
    //
    private static Palace BuildFinalBossCorridorBypassPalace(int palaceNumber)
    {
        Palace palace = ContinueAfterBoss(palaceNumber);
        var I = palace.AllRooms.First(room => room.Name == "I");
        var J = palace.AllRooms.First(room => room.Name == "J");
        I.SetConnections(BuildConnections(hasLeftExit: true, hasDownExit: true));
        J.SetConnections(BuildConnections(hasLeftExit: true, hasUpExit: true));
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    // Mini boss no entrance going right (the door left of b is removed):
    //
    //   E
    //   |
    //   A--C--D--F--I
    //   |     v
    //   G  b--H--B--J
    //
    private static Palace BuildLeftOnlyBossPalace(int palaceNumber)
    {
        Palace palace = ContinueAfterBoss(palaceNumber);
        var D = palace.AllRooms.First(room => room.Name == "D");
        var H = palace.AllRooms.First(room => room.Name == "H");
        D.SetConnections(BuildConnections(hasLeftExit: true, hasRightExit: true, hasDownExit: true));
        D.HasDrop = true;
        H.IsDropZone = true;
        var G = palace.AllRooms.First(room => room.Name == "G");
        string miniName = palaceNumber == 7 ? "T" : "b";
        var mini = palace.AllRooms.First(room => room.Name == miniName);
        G.SetConnections(BuildConnections(hasUpExit: true));
        mini.SetConnections(BuildConnections(hasRightExit: true));
        G.Right = null;
        mini.Left = null;
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    // FinalBoss (the door left of B is removed):
    //
    //   E
    //   |
    //   A--C--D--F--I
    //   |           v
    //   G--b--H  B--J
    //
    private static Palace BuildFinalBossLeftOnlyPalace(int palaceNumber)
    {
        Palace palace = ContinueAfterBoss(palaceNumber);
        var I = palace.AllRooms.First(room => room.Name == "I");
        var J = palace.AllRooms.First(room => room.Name == "J");
        I.SetConnections(BuildConnections(hasLeftExit: true, hasDownExit: true));
        I.HasDrop = true;
        J.IsDropZone = true;
        var H = palace.AllRooms.First(room => room.Name == "H");
        var B = palace.AllRooms.First(room => room.Name == "B");
        H.SetConnections(BuildConnections(hasLeftExit: true));
        B.SetConnections(BuildConnections(hasRightExit: true));
        H.Right = null;
        B.Left = null;
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    // An unsolvable palace: the final boss's half of the map is walled off from the entrance,
    // so no amount of boss-entry leniency can make every room reachable. The miniboss b sits in
    // the reachable half, entered from the west with no room east of it, so it passes every boss
    // check and only the missing rooms can reject the palace.
    //
    //   E--A--b     B--C
    //
    private static Palace BuildUnreachableBossPalace()
    {
        Palace palace = new(1, false);
        AddRoom(palace, 0, 0, "Entrance", isEntrance: true, hasRightExit: true);
        AddRoom(palace, 1, 0, "A", hasLeftExit: true, hasRightExit: true);
        AddRoom(palace, 2, 0, "b", hasBoss: true, hasLeftExit: true);
        AddRoom(palace, 4, 0, "B", hasBoss: true, isBossRoom: true, hasRightExit: true);
        AddRoom(palace, 5, 0, "C", hasLeftExit: true);
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    // Palace 7 only. When tbirdGates, the Thunderbird room sits between the entrance and the
    // final boss; otherwise it hangs off a side branch and the boss is reachable without it.
    // The miniboss hangs above the final boss in both cases, past the Thunderbird either way.
    //
    // tbirdGates:
    //
    //        b
    //        |
    //   E--T--B
    //
    // side branch:
    //
    //     b
    //     |
    //   E--B
    //   |
    //   T
    private static Palace BuildThunderbirdGatePalace(bool tbirdGates)
    {
        Palace palace = new(7, false);
        if (tbirdGates)
        {
            AddRoom(palace, 0, 1, "Entrance", isEntrance: true, hasRightExit: true);
            AddRoom(palace, 1, 1, "T", isThunderBirdRoom: true, hasLeftExit: true, hasRightExit: true);
            AddRoom(palace, 2, 1, "B", hasBoss: true, isBossRoom: true, hasLeftExit: true, hasUpExit: true);
            AddRoom(palace, 2, 2, "b", hasBoss: true, hasDownExit: true);
        }
        else
        {
            AddRoom(palace, 0, 1, "Entrance", isEntrance: true, hasRightExit: true, hasDownExit: true);
            AddRoom(palace, 1, 1, "B", hasBoss: true, isBossRoom: true, hasLeftExit: true, hasUpExit: true);
            AddRoom(palace, 1, 2, "b", hasBoss: true, hasDownExit: true);
            AddRoom(palace, 0, 0, "T", isThunderBirdRoom: true, hasUpExit: true);
        }
        ShapeFirstCoordinatePalaceGenerator.ConnectRooms(palace).Wait();
        return palace;
    }

    [TestMethod]
    public void DropBypass_BossWithCorridorToItsEast_IsNeverReachable()
    {
        Palace palace = BuildCorridorBypassPalace(1);
        palace.AllReachable(dropsMayBypassBosses: false).Should().BeFalse();
        palace.AllReachable(dropsMayBypassBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_BossEastSideReachableOnlyByDrop_RequiresOption()
    {
        Palace palace = BuildDropBypassPalace(1);
        Room boss = palace.AllRooms.First(room => room.Name == "b");

        //Without the option, the drop does not excuse reaching the boss's east side.
        palace.AllReachable(dropsMayBypassBosses: false).Should().BeFalse();

        //With the option the drop bypass is allowed, and the boss is still enterable going right.
        palace.AllReachable(dropsMayBypassBosses: true).Should().BeTrue();
        palace.GetReachableRooms(dropsMayBypassMiniBosses: true).Should().Contain([boss]);
    }

    [TestMethod]
    public void DropBypass_FinalBossRoom_IsNotRelaxedByMiniOption()
    {
        Palace palace = BuildFinalBossDropBypassPalace(1);
        palace.AllReachable(dropsMayBypassBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_FinalOption_DoesNotRelaxMiniBosses()
    {
        Palace palace = BuildDropBypassPalace(1);
        palace.AllReachable(dropsMayBypassFinalBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_FinalBossCorridor_IsRejectedEvenWithFinalOption()
    {
        Palace palace = BuildFinalBossCorridorBypassPalace(1);
        palace.AllReachable(dropsMayBypassFinalBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_FinalBossOnlyEnterableGoingLeft_IsRejectedEvenWithFinalOption()
    {
        Palace palace = BuildFinalBossLeftOnlyPalace(1);
        palace.AllReachable(dropsMayBypassFinalBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_FinalOption_RelaxesRequiredThunderbird()
    {
        //The required Thunderbird is in the final bucket for the drop options, just as it is for
        //the enter-going-left leniency; the mini option never covers it (see the test below).
        Palace palace = BuildDropBypassPalace(7);
        palace.AllReachable(dropsMayBypassFinalBosses: true, tBirdRequired: true).Should().BeTrue();
    }

    [TestMethod]
    public void DropBypass_MiniOption_DoesNotRelaxRequiredThunderbird()
    {
        Palace palace = BuildDropBypassPalace(7);
        palace.AllReachable(dropsMayBypassBosses: true, tBirdRequired: true).Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_OptionalThunderbird_IsRelaxed()
    {
        Palace palace = BuildDropBypassPalace(7);
        palace.AllReachable(dropsMayBypassBosses: true, tBirdRequired: false).Should().BeTrue();
    }

    [TestMethod]
    public void DropBypass_BossOnlyEnterableGoingLeft_IsRejectedEvenWithOption()
    {
        Palace palace = BuildLeftOnlyBossPalace(1);
        palace.AllReachable(dropsMayBypassBosses: true).Should().BeFalse();
    }

    [TestMethod]
    public void AllowMiniBossEnterGoingLeft_AcceptsBossBypassPalaces()
    {
        //For mini bosses the option turns off the strict check, the drop-aware check and the
        //"every boss needs a non-left entry" requirement, leaving only plain reachability. The
        //final boss is still validated (see the _IsRejected test below).
        Palace corridor = BuildCorridorBypassPalace(1);
        corridor.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeTrue();

        Palace drop = BuildDropBypassPalace(1);
        drop.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeTrue();

        Palace leftOnly = BuildLeftOnlyBossPalace(1);
        leftOnly.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeTrue();
    }

    [TestMethod]
    public void AllowMiniBossEnterGoingLeft_RequiredThunderbird_IsNotRelaxed()
    {
        //A Thunderbird that isn't required is a mini boss, so the mini option covers it. A
        //required one is a gate, so the mini option leaves the strict check in place: the drop
        //route reaches the east side without fighting it, which rejects the layout.
        Palace tbird = BuildDropBypassPalace(7);
        tbird.AllReachable(allowMiniBossEnterGoingLeft: true, tBirdRequired: true).Should().BeFalse();
        tbird.AllReachable(allowMiniBossEnterGoingLeft: true, tBirdRequired: false).Should().BeTrue();
    }

    [TestMethod]
    public void AllowFinalBossEnterGoingLeft_AcceptsRequiredThunderbird()
    {
        //The required Thunderbird is in the final bucket for both leniencies, since it is not a
        //mini boss (see DropBypass_FinalOption_RelaxesRequiredThunderbird).
        Palace tbird = BuildDropBypassPalace(7);
        tbird.AllReachable(allowFinalBossEnterGoingLeft: true, tBirdRequired: true).Should().BeTrue();
    }

    [TestMethod]
    public void AllowMiniBossEnterGoingLeft_FinalBossEnteredGoingLeft_IsRejected()
    {
        //The miniboss option is out of scope for the final boss: it must never be enterable going
        //left, so it is still validated even when the miniboss option is on.
        Palace finalBoss = BuildFinalBossDropBypassPalace(1);
        finalBoss.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeFalse();
    }

    [TestMethod]
    public void AllowFinalBossEnterGoingLeft_Tests()
    {
        Palace p1 = BuildFinalBossDropBypassPalace(1);
        Room bossRoom = p1.AllRooms.First(room => room.Name == "B");
        p1.AllReachable().Should().BeFalse();

        // tbh this is not a realistic case, allowing the final boss but not mini bosses
        p1.AllReachable(allowMiniBossEnterGoingLeft: false, allowFinalBossEnterGoingLeft: true).Should().BeFalse();
        p1.AllReachable(dropsMayBypassBosses: false, dropsMayBypassFinalBosses: true).Should().BeFalse();

        p1.AllReachable(allowMiniBossEnterGoingLeft: true, allowFinalBossEnterGoingLeft: true).Should().BeTrue();
        p1.AllReachable(dropsMayBypassBosses: true, dropsMayBypassFinalBosses: true).Should().BeTrue();

        Palace p7 = BuildFinalBossDropBypassPalace(7);
        p7.AllReachable().Should().BeFalse();

        p7.AllReachable(allowMiniBossEnterGoingLeft: false, allowFinalBossEnterGoingLeft: true).Should().BeFalse();
        p7.AllReachable(dropsMayBypassBosses: false, dropsMayBypassFinalBosses: true).Should().BeFalse();

        p7.AllReachable(allowMiniBossEnterGoingLeft: true, allowFinalBossEnterGoingLeft: true).Should().BeTrue();
        p7.AllReachable(dropsMayBypassBosses: true, dropsMayBypassFinalBosses: true).Should().BeTrue();
    }

    [TestMethod]
    public void AllowMiniBossEnterGoingLeft_UnsolvablePalace_StillReturnsFalse()
    {
        Palace palace = BuildUnreachableBossPalace();
        palace.AllReachable().Should().BeFalse();
        palace.AllReachable(allowMiniBossEnterGoingLeft: true).Should().BeFalse();
    }

    [TestMethod]
    public void RequiredThunderbird_StillGatesTheBoss()
    {
        //The gate is enforced by RequiresThunderbird (a tree walk that treats the Thunderbird
        //room as a wall), not by AllReachable, so boss-entry leniency cannot bypass it.
        Palace gated = BuildThunderbirdGatePalace(tbirdGates: true);
        gated.RequiresThunderbird().Should().BeTrue();

        Palace sideBranch = BuildThunderbirdGatePalace(tbirdGates: false);
        sideBranch.RequiresThunderbird().Should().BeFalse();
    }

    [TestMethod]
    public void DropBypass_BothDropRoutes_RequireBothOptions()
    {
        //Each boss is east of its own drop; each option excuses only its own bucket.
        Palace palace = BuildDoubleDropBypassPalace(1);
        palace.AllReachable().Should().BeFalse();
        palace.AllReachable(dropsMayBypassBosses: true).Should().BeFalse();
        palace.AllReachable(dropsMayBypassFinalBosses: true).Should().BeFalse();
        palace.AllReachable(dropsMayBypassBosses: true, dropsMayBypassFinalBosses: true).Should().BeTrue();
    }

    #endregion
}
