using System;
using System.Collections.Generic;
using Z2Randomizer.RandomizerCore;
using Random = Z2Randomizer.RandomizerCore.Random;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.Tests;

[TestClass]
public class KeyLogicTests
{
    [TestMethod]
    [DataRow(KeyLogicOption.KEYS_PER_PALACE_SOLVABLE, false, false)]
    [DataRow(KeyLogicOption.KEYS_PER_PALACE_SOLVABLE, true, true)]
    [DataRow(KeyLogicOption.FAIRY_OR_MAGICKEY, false, false)]
    [DataRow(KeyLogicOption.FAIRY_OR_MAGICKEY, true, false)]
    public void SmallKeyValidationFollowsPalaceAndConfiguration(
        KeyLogicOption keyLogic, bool blockersAnywhere, bool validateP6)
    {
        var props = new RandomizerProperties
        {
            KeyLogic = keyLogic,
            BlockersAnywhere = blockersAnywhere,
        };
        for (int number = 1; number <= 5; number++)
        {
            Assert.AreEqual(keyLogic == KeyLogicOption.KEYS_PER_PALACE_SOLVABLE,
                props.RequiresSmallKeyValidation(number));
        }
        Assert.AreEqual(validateP6, props.RequiresSmallKeyValidation(6));
        Assert.IsFalse(props.RequiresSmallKeyValidation(7));
    }

    private static Room MakeRoom(string name, int keys = 0, int lockedDoors = 0, bool isEntrance = false, bool hasBoss = false)
    {
        List<byte> bytes =
        [
            0, 0x60, 0x0E, 0x08,   // header (length patched below)
            0x08, 0x44,            // a floor block so the room parses cleanly
        ];
        for (int i = 0; i < keys; i++)
        {
            // 3 byte item command with the small key collectable
            bytes.Add(0x82); bytes.Add(0x0F); bytes.Add((byte)Collectable.KEY);
        }
        for (int i = 0; i < lockedDoors; i++)
        {
            // 2 byte command whose tile is the locked door
            bytes.Add(0x83); bytes.Add(0x05);
        }
        bytes[0] = (byte)bytes.Count;
        return new Room
        {
            Name = name,
            Author = name,
            SideView = bytes.ToArray(),
            ItemPresenceBits = [0x0F],
            Enemies = [0x01],
            IsEntrance = isEntrance,
            HasBoss = hasBoss,
            LinkRequirements = Requirements.NONE,
        };
    }

    private static Palace MakePalace(params Room[] rooms)
    {
        Room entrance = rooms.First(room => room.IsEntrance);
        var palace = new Palace(1, false);
        palace.AllRooms.AddRange(rooms);
        palace.Entrance = entrance;
        palace.BossRoom = rooms.FirstOrDefault(room => room.HasBoss);
        return palace;
    }

     [TestMethod]
    public void PalaceWithoutLockedDoorsIsAlwaysSolvable()
    {
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var a = MakeRoom("A");
        entrance.Right = a;
        var palace = MakePalace(entrance, a);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 0, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void LinearChainWithKeysBetweenDoorsIsSolvable()
    {
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var a = MakeRoom("A", keys: 1, lockedDoors: 1);
        var b = MakeRoom("B", keys: 1, lockedDoors: 1);
        entrance.Left = a;
        a.Left = b;
        var palace = MakePalace(entrance, a, b);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 1, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void LinearChainWithTooFewKeysDeadlocks()
    {
        // Only one starting key, but the two doors in a row have no keys between them.
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var a = MakeRoom("A", keys: 0, lockedDoors: 1);
        var b = MakeRoom("B", keys: 5, lockedDoors: 1);
        entrance.Left = a;
        a.Left = b;
        var palace = MakePalace(entrance, a, b);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 1, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void StartingKeysDecideOutcome()
    {
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var a = MakeRoom("A", keys: 0, lockedDoors: 1);
        var b = MakeRoom("B", keys: 2, lockedDoors: 1);
        entrance.Left = a;
        a.Left = b;
        var palace = MakePalace(entrance, a, b);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 1, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 2, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void WorstCaseOrderingCanDeadlock()
    {
        // From the entrance there is a keyless door leading to another keyless door, and a door
        // leading to a treasure room. A player who opens the two keyless doors in a row runs out
        // of keys before reaching any treasure, so worst-case key usage is not solvable.
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var a = MakeRoom("A", keys: 0, lockedDoors: 1);
        var c = MakeRoom("C", keys: 5, lockedDoors: 1);
        var d = MakeRoom("D", keys: 0, lockedDoors: 1);
        entrance.Left = a;
        entrance.Right = c;
        a.Left = d;
        var palace = MakePalace(entrance, a, c, d);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 2, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void BranchingDoorsWithKeysOnEveryPathIsSolvable()
    {
        var entrance = MakeRoom("E", keys: 1, isEntrance: true);
        var a = MakeRoom("A", keys: 2, lockedDoors: 1);
        var b = MakeRoom("B", keys: 1, lockedDoors: 1);
        entrance.Left = b;
        entrance.Right = a;
        var palace = MakePalace(entrance, a, b);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 0, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void MultipleLockedDoorsInOneRoomCostMultipleKeys()
    {
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var d = MakeRoom("D", keys: 0, lockedDoors: 2);
        var a = MakeRoom("A", keys: 2, lockedDoors: 1);
        entrance.Left = d;
        d.Left = a;
        var palace = MakePalace(entrance, d, a);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 2, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 3, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ItemReachabilityUsesTheSameBossKeyPolicyAsGeneration(bool bossItem)
    {
        var entrance = MakeRoom("E", keys: bossItem ? 1 : 0, isEntrance: true);
        var boss = MakeRoom("Boss", hasBoss: true);
        var item = MakeRoom("Item", lockedDoors: 1);
        item.Collectable = Collectable.MAGIC_CONTAINER;
        item.LinkRequirements = new Requirements([RequirementType.KEY]);
        entrance.Right = boss;
        boss.Left = entrance;
        boss.Right = item;
        item.Left = boss;
        var palace = MakePalace(entrance, boss, item);
        palace.ItemRooms.Add(item);
        var props = new RandomizerProperties
        {
            KeyLogic = KeyLogicOption.KEYS_PER_PALACE_SOLVABLE,
            BossItem = bossItem,
        };

        // A randomized boss item needs a floor key; a vanilla boss supplies its own.
        Assert.IsTrue(new ReconstructedPalaceGenerator(System.Threading.CancellationToken.None).AddKeysForKeyLogic(props, palace, createKeys: true, r: new Random(0)));
        CollectionAssert.AreEqual(new[] { Collectable.MAGIC_CONTAINER },
            palace.GetGettableItems(props, new HashSet<RequirementType>(), false));
    }

    [TestMethod]
    public void BossDropsKeys()
    {
        // Two locked doors in a row, but the only keys are the starting key and the one the
        // boss drops. Whether the boss drops a key decides the outcome.
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var boss = MakeRoom("Boss", keys: 0, lockedDoors: 1, hasBoss: true);
        var after = MakeRoom("C", keys: 0, lockedDoors: 1);
        entrance.Down = boss;
        boss.Right = after;
        var palace = MakePalace(entrance, boss, after);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 1, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);

        palace.SolveKeyLogic([], bossesDropKeys: true, startingKeys: 1, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    [DataRow(true, 0, false)]
    [DataRow(true, 1, true)]
    [DataRow(false, 0, true)]
    public void ContinuingBossKeyRequiresAnAccessibleEntry(bool enterFromEast, int startingKeys, bool expected)
    {
        var entrance = MakeRoom("E", isEntrance: true);
        var boss = MakeRoom("Boss", hasBoss: true);
        boss.AdjustContinuingBossRoom();
        var after = MakeRoom("D", lockedDoors: 1);
        if (enterFromEast)
        {
            entrance.Left = boss;
            boss.Left = after;
        }
        else
        {
            entrance.Right = boss;
            boss.Right = after;
        }
        var palace = MakePalace(entrance, boss, after);

        Assert.AreEqual(expected, palace.SolveKeyLogic([], bossesDropKeys: true, startingKeys: startingKeys, createKeys: false, r: null));
    }

    [TestMethod]
    public void BossDropRespectsKeyRequirements()
    {
        var entrance = MakeRoom("E", isEntrance: true);
        var boss = MakeRoom("Boss", hasBoss: true);
        boss.LinkRequirements = new Requirements([], [],
            [new ComplexRequirement(RequirementTarget.KEY, [RequirementType.GLOVE], [])]);
        var after = MakeRoom("D", lockedDoors: 1);
        entrance.Right = boss;
        boss.Right = after;
        var palace = MakePalace(entrance, boss, after);

        Assert.IsFalse(palace.SolveKeyLogic([], bossesDropKeys: true, startingKeys: 0, createKeys: false, r: null));
        Assert.IsTrue(palace.SolveKeyLogic([RequirementType.GLOVE], bossesDropKeys: true, startingKeys: 0, createKeys: false, r: null));
    }

    [TestMethod]
    public void BossKeyReachableFromBothSidesIsCountedOnce()
    {
        var entrance = MakeRoom("E", isEntrance: true);
        var boss = MakeRoom("Boss", hasBoss: true);
        boss.AdjustContinuingBossRoom();
        var approach = MakeRoom("A");
        var after = MakeRoom("D", lockedDoors: 2);
        entrance.Left = boss; // First visit is from the blocked east side.
        entrance.Right = approach;
        approach.Right = boss; // A later visit reaches the key from the west.
        boss.Up = after;
        var palace = MakePalace(entrance, boss, approach, after);

        Assert.IsFalse(palace.SolveKeyLogic([], bossesDropKeys: true, startingKeys: 0, createKeys: false, r: null));
        Assert.IsTrue(palace.SolveKeyLogic([], bossesDropKeys: true, startingKeys: 1, createKeys: false, r: null));
    }

    [TestMethod]
    [DataRow(Direction.WEST, true)]
    [DataRow(Direction.NORTH, true)]
    [DataRow(Direction.SOUTH, true)]
    [DataRow(Direction.EAST, false)]
    public void LeavingAndReturningToBossUsesTheReturnEntry(Direction exit, bool expected)
    {
        var entrance = MakeRoom("E", isEntrance: true);
        var boss = MakeRoom("Boss", hasBoss: true);
        boss.AdjustContinuingBossRoom();
        var sideRoom = MakeRoom("Side");
        var door = MakeRoom("D", lockedDoors: 1);
        entrance.Left = boss;
        boss.Right = entrance;
        entrance.Up = door;
        switch (exit)
        {
            case Direction.WEST: boss.Left = sideRoom; sideRoom.Right = boss; break;
            case Direction.NORTH: boss.Up = sideRoom; sideRoom.Down = boss; break;
            case Direction.SOUTH: boss.Down = sideRoom; sideRoom.Up = boss; break;
            // The entrance already provides an east-side round trip.
        }
        var palace = MakePalace(entrance, boss, sideRoom, door);

        Assert.AreEqual(expected, palace.SolveKeyLogic([], bossesDropKeys: true, startingKeys: 0, createKeys: false, r: null));
    }

    [TestMethod]
    [DataRow(false, 1, false)]
    [DataRow(true, 0, false)]
    [DataRow(true, 1, true)]
    public void LockedBossDoorMustBePaidBeforeRoundTripCanRecoverKey(bool canReturn, int startingKeys, bool expected)
    {
        var entrance = MakeRoom("E", isEntrance: true);
        var boss = MakeRoom("Boss", hasBoss: true, lockedDoors: 1);
        boss.AdjustContinuingBossRoom();
        var sideRoom = MakeRoom("Side");
        var after = MakeRoom("D", lockedDoors: 1);
        entrance.Left = boss;
        boss.Right = entrance;
        boss.Left = sideRoom;
        if (canReturn)
        {
            sideRoom.Right = boss;
        }
        sideRoom.Left = after;
        var palace = MakePalace(entrance, boss, sideRoom, after);

        Assert.AreEqual(expected, palace.SolveKeyLogic([], bossesDropKeys: true, startingKeys: startingKeys, createKeys: false, r: null));
    }

    private static Room MakeKeyPlacementRoom(string name, bool isEntrance = false, int minorItems = 0)
    {
        var room = MakeRoom(name, isEntrance: isEntrance);
        // Flat floor with a left exit; each blue jar is on page zero.
        List<byte> bytes = [0, 0x60, 0x00, 0x08];
        for (int i = 0; i < minorItems; i++)
        {
            bytes.AddRange([0x82, 0x0F, (byte)Collectable.BLUE_JAR]);
        }
        bytes[0] = (byte)bytes.Count;
        room.SideView = bytes.ToArray();
        room.SetConnections([0, 0xFC, 0xFC, 0xFC]);
        return room;
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void KeyPlacementClonesMutableBuffers(int minorItems)
    {
        var entrance = MakeKeyPlacementRoom("E", isEntrance: true, minorItems: minorItems);
        // Floor placement must turn an initially clear presence bit on.
        if (minorItems == 0) entrance.ItemPresenceBits = [0];
        var door = MakeRoom("Door", lockedDoors: 1);
        entrance.Right = door;
        var palace = MakePalace(entrance, door);
        var sharedSideView = entrance.SideView;
        var sharedBits = entrance.ItemPresenceBits;
        var originalSideView = sharedSideView.ToArray();
        var originalBits = sharedBits.ToArray();

        Assert.IsTrue(palace.SolveKeyLogic([], false, 0, true, new Random(0)));
        Assert.AreEqual(1, entrance.Keys([], false));
        Assert.AreNotSame(sharedSideView, entrance.SideView);
        Assert.AreNotSame(sharedBits, entrance.ItemPresenceBits);
        CollectionAssert.AreEqual(originalSideView, sharedSideView);
        CollectionAssert.AreEqual(originalBits, sharedBits);
        Assert.IsTrue(palace.SolveKeyLogic([], false, 0, false, null));
    }

    [TestMethod]
    public void FullInventoryCannotSatisfyOutOfLogic()
    {
        Assert.IsFalse(RequirementTypeExtensions.ALL.Contains(RequirementType.OUT_OF_LOGIC));
        foreach (var requirement in Enum.GetValues<RequirementType>().Where(r => r != RequirementType.OUT_OF_LOGIC))
        {
            Assert.IsTrue(RequirementTypeExtensions.ALL.Contains(requirement), $"Missing {requirement}");
        }
        Assert.IsFalse(new Requirements([RequirementType.OUT_OF_LOGIC]).AreSatisfiedBy(RequirementTypeExtensions.ALL));
    }

    [TestMethod]
    public void GenerationDoesNotRelyOnOutOfLogicKeys()
    {
        var entrance = MakeKeyPlacementRoom("E", isEntrance: true, minorItems: 1);
        var inaccessibleKey = MakeRoom("Out of logic key", keys: 1);
        inaccessibleKey.LinkRequirements = new Requirements([], [],
            [new ComplexRequirement(RequirementTarget.KEY, [RequirementType.OUT_OF_LOGIC], [], [])]);
        var door = MakeRoom("Door", lockedDoors: 1);
        entrance.Right = inaccessibleKey;
        inaccessibleKey.Right = door;
        var palace = MakePalace(entrance, inaccessibleKey, door);
        var props = new RandomizerProperties { KeyLogic = KeyLogicOption.KEYS_PER_PALACE_SOLVABLE };
        var generator = new ReconstructedPalaceGenerator(System.Threading.CancellationToken.None);

        Assert.IsTrue(generator.AddKeysForKeyLogic(props, palace, createKeys: true, r: new Random(0)));
        Assert.AreEqual(1, entrance.Keys([], false));
        Assert.IsTrue(generator.AddKeysForKeyLogic(props, palace, createKeys: false, r: new Random(0)));
    }

    [TestMethod]
    public void ValidateKeyLogicHonorsCreateKeys()
    {
        var entrance = MakeKeyPlacementRoom("E", isEntrance: true, minorItems: 1);
        var door = MakeRoom("Door", lockedDoors: 1);
        entrance.Right = door;
        var palace = MakePalace(entrance, door);
        var props = new RandomizerProperties { KeyLogic = KeyLogicOption.KEYS_PER_PALACE_SOLVABLE };
        var generator = new ReconstructedPalaceGenerator(System.Threading.CancellationToken.None);
        var originalSideView = entrance.SideView;
        var originalBits = entrance.ItemPresenceBits;

        Assert.IsFalse(generator.AddKeysForKeyLogic(props, palace, createKeys: false, r: new Random(0)));
        Assert.AreSame(originalSideView, entrance.SideView);
        Assert.AreSame(originalBits, entrance.ItemPresenceBits);
        Assert.AreEqual(0, entrance.Keys([], false));
        Assert.IsTrue(generator.AddKeysForKeyLogic(props, palace, createKeys: true, r: new Random(0)));
        Assert.AreEqual(1, entrance.Keys([], false));
    }

    [TestMethod]
    public void TransmutationAcrossRoomsPrecedesFloorPlacement()
    {
        // Try several shuffles so either room can be considered first.
        for (int seed = 0; seed < 12; seed++)
        {
            var entrance = MakeKeyPlacementRoom("E", isEntrance: true);
            var jars = MakeKeyPlacementRoom("Jars", minorItems: 2);
            var door = MakeRoom("D", lockedDoors: 2);
            entrance.Right = jars;
            entrance.Up = door;
            var palace = MakePalace(entrance, jars, door);
            var originalEntrance = entrance.SideView.ToArray();

            Assert.IsTrue(palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 0, createKeys: true, r: new Random(seed)));
            Assert.AreEqual(2, jars.Keys([], false));
            CollectionAssert.AreEqual(originalEntrance, entrance.SideView);
        }
    }

    [TestMethod]
    public void FloorPlacementIsUsedAfterTransmutationRunsOut()
    {
        var entrance = MakeKeyPlacementRoom("E", isEntrance: true);
        var jars = MakeKeyPlacementRoom("Jars", minorItems: 1);
        var door = MakeRoom("D", lockedDoors: 2);
        entrance.Right = jars;
        entrance.Up = door;
        var palace = MakePalace(entrance, jars, door);

        Assert.IsTrue(palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 0, createKeys: true, r: new Random(0)));
        Assert.AreEqual(1, jars.Keys([], false));
        Assert.AreEqual(1, entrance.Keys([], false));
    }

    [TestMethod]
    public void KeyTargetRequirementPreventsBothKindsOfKeyCreation()
    {
        var jars = MakeKeyPlacementRoom("Jars", minorItems: 1);
        var floor = MakeKeyPlacementRoom("Floor", isEntrance: true);
        foreach (var room in new[] { jars, floor })
        {
            // This requirement is satisfied and only applies to one entry; placement
            // must still skip the entire room.
            room.LinkRequirements = new Requirements([], [],
                [new ComplexRequirement(RequirementTarget.KEY, [RequirementType.GLOVE], [],
                    [new RequirementRoute([Direction.EAST], null)])]);
            var original = room.SideView.ToArray();
            Assert.IsFalse(room.TryTransmuteMinorItemToKey(new Random(0)));
            Assert.IsFalse(room.TryAddKeyOnFloor());
            CollectionAssert.AreEqual(original, room.SideView);
        }
        floor.Right = jars;
        var door = MakeRoom("D", lockedDoors: 1);
        floor.Up = door;
        var palace = MakePalace(floor, jars, door);
        Assert.IsFalse(palace.SolveKeyLogic([RequirementType.GLOVE], bossesDropKeys: false, startingKeys: 0, createKeys: true, r: new Random(0)));
    }

    [TestMethod]
    public void NoKeyTransformStillAllowsFloorPlacement()
    {
        var room = MakeKeyPlacementRoom("E", isEntrance: true);
        room.Tags.Add("NoKeyTransform");
        Assert.IsFalse(room.TryTransmuteMinorItemToKey(new Random(0)));
        Assert.IsTrue(room.TryAddKeyOnFloor());
    }

    [TestMethod]
    public void RequirementsGateWhetherKeysAreFindable()
    {
        var entrance = MakeRoom("E", keys: 1, isEntrance: true);
        var h = MakeRoom("H");
        var b = MakeRoom("B", keys: 1, lockedDoors: 1);
        var a = MakeRoom("A", keys: 2, lockedDoors: 1);
        h.LinkRequirements = new Requirements([RequirementType.JUMP]);
        entrance.Left = b;
        entrance.Right = h;
        h.Right = a;
        var palace = MakePalace(entrance, h, b, a);

        // Without JUMP the treasure room is unreachable, so its door can never be paid for.
        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 0, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);
        // With JUMP (and its implicit 2-container requirement) both branches are reachable.
        var withJump = new HashSet<RequirementType> { RequirementType.JUMP, RequirementType.TWO_CONTAINERS };
        palace.SolveKeyLogic(withJump, bossesDropKeys: false, startingKeys: 0, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void ComplexKeyRequirementFreesUnkeyedExits()
    {
        // The door room's complex requirements gate only its EAST exit with a KEY requirement in
        // some capacity (a small key). Its WEST exit has no KEY requirement, so the treasure
        // behind it can be reached without spending any of the door's small keys; the keys are
        // then used to open the door's EAST exit.
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var d = MakeRoom("D", keys: 0, lockedDoors: 1);
        d.LinkRequirements = EastGateComplexRequirements(RequirementType.KEY);
        var treasure = MakeRoom("T", keys: 2);
        var x = MakeRoom("X");
        entrance.Right = d;
        d.Left = treasure;
        d.Right = x;
        var palace = MakePalace(entrance, d, treasure, x);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 0, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void KeyGatedExitStillCostsASmallKey()
    {
        // The door room's only exit is gated by a KEY requirement and there are no other ways in.
        // Without a small key the player is stuck; with one, opening the door lets them through.
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var d = MakeRoom("D", keys: 0, lockedDoors: 1);
        d.LinkRequirements = EastGateComplexRequirements(RequirementType.KEY);
        var a = MakeRoom("A");
        entrance.Right = d;
        d.Right = a;
        var palace = MakePalace(entrance, d, a);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 0, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 1, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void ComplexRequirementsWithoutKeyDoNotFreeExits()
    {
        // A locked-door room whose complex requirements contain no KEY requirement is still a hard
        // barrier: no carried requirement opens it without spending a small key on the door.
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var d = MakeRoom("D", keys: 0, lockedDoors: 1);
        d.LinkRequirements = EastGateComplexRequirements(RequirementType.GLOVE);
        var a = MakeRoom("A", keys: 1);
        entrance.Right = d;
        d.Right = a;
        var palace = MakePalace(entrance, d, a);

        palace.SolveKeyLogic([], bossesDropKeys: false, startingKeys: 0, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);

        palace.SolveKeyLogic([RequirementType.GLOVE], bossesDropKeys: false, startingKeys: 0, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void MagicKeyDoesNotStandInForSmallKey()
    {
        // The magic key must not stand in for a small key: this check confirms the palace is
        // solvable with the keys found within it, not by leaning on the magic key.
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var d = MakeRoom("D", keys: 0, lockedDoors: 1);
        var a = MakeRoom("A");
        entrance.Right = d;
        d.Right = a;
        var palace = MakePalace(entrance, d, a);

        palace.SolveKeyLogic([RequirementType.MAGIC_KEY], bossesDropKeys: false, startingKeys: 0, createKeys: true, new Random(0));
        Assert.IsFalse(palace.KeyLogicSolved);

        palace.SolveKeyLogic([RequirementType.MAGIC_KEY], bossesDropKeys: false, startingKeys: 1, createKeys: true, new Random(0));
        Assert.IsTrue(palace.KeyLogicSolved);
    }

    [TestMethod]
    public void MagicKeyCollectableMapsToMagicKeyRequirement()
    {
        Assert.AreEqual(RequirementType.MAGIC_KEY, Collectable.MAGIC_KEY.AsRequirement());
    }

    [TestMethod]
    public void SmallKeyCollectableIsNotARequireable()
    {
        Assert.IsNull(Collectable.KEY.AsRequirement());
    }

    [TestMethod]
    public void MagicKeyRequirementMapsBackToItsCollectable()
    {
        Assert.AreEqual(Collectable.MAGIC_KEY, RequirementType.MAGIC_KEY.AsCollectable());
    }

    [TestMethod]
    public void LockedDoorsOnlyCountsDoorsOnPagesWithItemBitSet()
    {
        // Create a room with a locked door on page 3
        var room = MakeRoom("triforceOfCourage M12");
        room.SideView = Convert.FromHexString("24600E08D20892737473F01F5473543FF81F583FD808F01FD808507374739473D60E8005");
        room.ItemPresenceBits = [0x0F];
        // The locked door should be counted since page 3 has item bit set
        Assert.IsTrue(room.GetItemBit(3));
        Assert.AreEqual(1, room.LockedDoors());

        // Now clear the item bit for page 3
        room.ItemPresenceBits = [0x0E];
        Assert.IsFalse(room.GetItemBit(3));
        Assert.AreEqual(0, room.LockedDoors());
        
        // Clear all item bits
        room.ItemPresenceBits = [0x00];
        Assert.IsFalse(room.GetItemBit(3));
        Assert.AreEqual(0, room.LockedDoors());
    }

    [TestMethod]
    public void SmallKeyRequirementIsNotACollectable()
    {
        Assert.IsNull(RequirementType.KEY.AsCollectable());
    }

    [TestMethod]
    public void DefaultPalaceRequirementsAreSatisfiedByFairyOrMagicKey()
    {
        Requirements requirements = Requirements.DEFAULT_PALACE_REQUIREMENTS;

        // FAIRY carries an implicit FOUR_CONTAINERS requirement, hence the extra requireable.
        Assert.IsTrue(requirements.AreSatisfiedBy([RequirementType.FAIRY, RequirementType.FOUR_CONTAINERS]));
        Assert.IsFalse(requirements.AreSatisfiedBy([RequirementType.FAIRY]));
        Assert.IsTrue(requirements.AreSatisfiedBy([RequirementType.MAGIC_KEY]));
        Assert.IsFalse(requirements.AreSatisfiedBy([RequirementType.KEY]));
        Assert.IsFalse(requirements.AreSatisfiedBy(new HashSet<RequirementType>()));
    }

    [TestMethod]
    public void MagicKeyGatedAccessIsNotSatisfiedByASmallKey()
    {
        // Mirrors P6's AccessRequirements under KEYS_PER_PALACE_SOLVABLE.
        Requirements magicKeyGate = new([RequirementType.MAGIC_KEY]);

        Assert.IsTrue(magicKeyGate.AreSatisfiedBy([RequirementType.MAGIC_KEY]));
        Assert.IsFalse(magicKeyGate.AreSatisfiedBy([RequirementType.KEY]));
        Assert.IsFalse(magicKeyGate.AreSatisfiedBy(new HashSet<RequirementType>()));
    }

    private static Requirements EastGateComplexRequirements(params RequirementType[] individual)
    {
        return new Requirements(
            [],
            [],
            [
                new ComplexRequirement(
                    RequirementTarget.EXIT,
                    individual,
                    [],
                    [new RequirementRoute([Direction.EAST], []), new RequirementRoute([], [Direction.EAST])]),
            ]);
    }
}
