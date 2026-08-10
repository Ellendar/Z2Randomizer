using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.Tests;

[TestClass]
public class KeyLogicTests
{
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

        Assert.IsTrue(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 0));
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

        Assert.IsTrue(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 1));
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

        Assert.IsFalse(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 1));
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

        Assert.IsFalse(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 1));
        Assert.IsTrue(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 2));
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

        Assert.IsFalse(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 2));
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

        Assert.IsTrue(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 0));
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

        Assert.IsFalse(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 2));
        Assert.IsTrue(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 3));
    }

    [TestMethod]
    public void BossDropsKeys()
    {
        // Two locked doors in a row, but the only keys are the starting key and the one the
        // boss drops. Whether the boss drops a key decides the outcome.
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var boss = MakeRoom("Boss", keys: 0, lockedDoors: 1, hasBoss: true);
        var after = MakeRoom("C", keys: 0, lockedDoors: 1);
        entrance.Left = boss;
        boss.Left = after;
        var palace = MakePalace(entrance, boss, after);

        Assert.IsFalse(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 1));
        Assert.IsTrue(palace.HasSolvableKeyLogic([], bossesDropKeys: true, startingKeys: 1));
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
        Assert.IsFalse(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 0));
        // With JUMP (and its implicit 2-container requirement) both branches are reachable.
        var withJump = new HashSet<RequirementType> { RequirementType.JUMP, RequirementType.TWO_CONTAINERS };
        Assert.IsTrue(palace.HasSolvableKeyLogic(withJump, bossesDropKeys: false, startingKeys: 0));
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

        Assert.IsTrue(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 0));
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

        Assert.IsFalse(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 0));
        Assert.IsTrue(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 1));
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

        Assert.IsFalse(palace.HasSolvableKeyLogic([], bossesDropKeys: false, startingKeys: 0));
        Assert.IsFalse(palace.HasSolvableKeyLogic([RequirementType.GLOVE], bossesDropKeys: false, startingKeys: 0));
    }

    [TestMethod]
    public void HasSolvableKeyLogicIgnoresTheMagicKey()
    {
        // The magic key must not stand in for a small key: this check confirms the palace is
        // solvable with the keys found within it, not by leaning on the magic key.
        var entrance = MakeRoom("E", keys: 0, isEntrance: true);
        var d = MakeRoom("D", keys: 0, lockedDoors: 1);
        var a = MakeRoom("A");
        entrance.Right = d;
        d.Right = a;
        var palace = MakePalace(entrance, d, a);

        Assert.IsFalse(palace.HasSolvableKeyLogic([RequirementType.MAGIC_KEY], bossesDropKeys: false, startingKeys: 0));
        Assert.IsTrue(palace.HasSolvableKeyLogic([RequirementType.MAGIC_KEY], bossesDropKeys: false, startingKeys: 1));
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
