using FluentAssertions;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;
using Random = Z2Randomizer.RandomizerCore.Random;

namespace Tests;

[TestClass]
public class RoomPoolTests
{
    Room CreateMockRoom(string name, RoomGroup group, bool enabled = true, bool hasItem = false,
        bool isBossRoom = false, bool isEntrance = false, bool isThunderBirdRoom = false,
        int? palaceNumber = null, string? linkedRoomName = null, string[]? tags = null,
        bool hasUpExit = false, bool hasDownExit = false, bool hasLeftExit = false,
        bool hasRightExit = false, bool hasDrop = false, bool isDropZone = false,
        byte map = 0, string duplicateGroup = "")
    {
        var connections = new byte[] {
            hasLeftExit ? (byte)0 : (byte)0xFC,
            hasDownExit ? (byte)0 : (byte)0xFC,
            hasUpExit ? (byte)0 : (byte)0xFC,
            hasRightExit ? (byte)0 : (byte)0xFC
        };

        var room = new Room
        {
            Name = name,
            Group = group,
            Enabled = enabled,
            HasItem = hasItem,
            IsBossRoom = isBossRoom,
            IsEntrance = isEntrance,
            IsThunderBirdRoom = isThunderBirdRoom,
            PalaceNumber = palaceNumber,
            LinkedRoomName = linkedRoomName,
            Tags = tags?.ToList() ?? [],
            HasDrop = hasDrop,
            IsDropZone = isDropZone,
            DuplicateGroup = duplicateGroup,
            SideView = [0x04, 0x60, 0x00, 0x08],
            Enemies = [0x01],
            ItemGetBits = [0x0F],
            Connections = connections,
            Requirements = Requirements.NONE,
            Map = map
        };
        room.OnDeserialized();
        return room;
    }

    RandomizerProperties CreateMockProps(bool allowVanilla = true, bool allowV4 = false,
        bool allowV5 = false, bool removeLongDeadEnds = false, bool includeExpert = true,
        bool blockersAnywhere = true, bool replaceFireWithDash = true,
        HashSet<Collectable>? removeItems = null)
    {
        return new RandomizerProperties
        {
            AllowVanillaRooms = allowVanilla,
            AllowV4Rooms = allowV4,
            AllowV5_0Rooms = allowV5,
            RemoveLongDeadEnds = removeLongDeadEnds,
            IncludeExpertRooms = includeExpert,
            BlockersAnywhere = blockersAnywhere,
            ReplaceFireWithDash = replaceFireWithDash,
            NoDuplicateRooms = false,
            NoDuplicateRoomsBySideview = false
        };
    }

    /// <summary>
    /// Creates PalaceRooms as required for FinalizePool
    /// </summary>
    PalaceRooms CreateMockPalaceRooms(List<Room>? vanillaRooms = null, List<Room>? v4Rooms = null, List<Room>? v5Rooms = null, int palaceNumber = 1)
    {
        var allRooms = new List<Room>();
        allRooms.AddRange(vanillaRooms ?? []);
        allRooms.AddRange([
            CreateMockRoom("VanillaDownEntrance", RoomGroup.VANILLA, isEntrance: true, palaceNumber: palaceNumber, hasDownExit: true),
            CreateMockRoom("VanillaP1Boss", RoomGroup.VANILLA, isBossRoom: true, palaceNumber: 1, map: 13, hasLeftExit: true),
            CreateMockRoom("VanillaP1Item", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasRightExit: true),
        ]);
        allRooms.AddRange([
            CreateMockRoom("V4DownBoss", RoomGroup.V4_0, isBossRoom: true, hasDownExit: true, palaceNumber: palaceNumber >= 6 ? palaceNumber : null)
        ]);
        allRooms.AddRange(v4Rooms ?? []);
        allRooms.AddRange([
            CreateMockRoom("V5UpEntrance", RoomGroup.V5_0, isEntrance: true, palaceNumber: palaceNumber, hasUpExit: true)
        ]);
        allRooms.AddRange(v5Rooms ?? []);
        allRooms.AddRange([
            CreateMockRoom("StubDown", RoomGroup.STUBS, hasDownExit: true),
            CreateMockRoom("StubUp", RoomGroup.STUBS, hasUpExit: true)
        ]);
        return new PalaceRooms(allRooms);
    }

    #region Constructor - Props-based

    [TestMethod]
    public void Constructor_Props_SplitsRoomsIntoCorrectCategories()
    {
        var normalRoom = CreateMockRoom("Normal1", RoomGroup.V4_0, palaceNumber: 1);
        var entranceRoom = CreateMockRoom("Entrance1", RoomGroup.V4_0, isEntrance: true, palaceNumber: 1);
        var bossRoom = CreateMockRoom("Boss1", RoomGroup.V4_0, isBossRoom: true, palaceNumber: 1);
        var itemRoom = CreateMockRoom("Item1", RoomGroup.V4_0, hasItem: true, palaceNumber: 1, hasUpExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([normalRoom, entranceRoom, bossRoom, itemRoom]);
        var props = CreateMockProps(allowVanilla: false, allowV4: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.NormalRooms.Should().Contain(r => r.Name == "Normal1");
        pool.Entrances.Should().Contain(r => r.Name == "Entrance1");
        pool.BossRooms.Should().Contain(r => r.Name == "Boss1");
        pool.ItemRooms.Should().Contain(r => r.Name == "Item1");
    }

    [TestMethod]
    public void Constructor_Props_IncludesTbirdRoomsForPalace7()
    {
        var tbirdRoom = CreateMockRoom("Tbird1", RoomGroup.VANILLA, isThunderBirdRoom: true, palaceNumber: 7);
        var bossRoom = CreateMockRoom("Boss7", RoomGroup.VANILLA, isBossRoom: true, palaceNumber: 7);

        var mockPalaceRooms = CreateMockPalaceRooms([tbirdRoom, bossRoom], palaceNumber: 7);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 7, props);

        pool.TbirdRooms.Should().ContainSingle().Which.Name.Should().Be("Tbird1");
    }

    [TestMethod]
    public void Constructor_Props_OnlyIncludesRoomsForMatchingPalace()
    {
        var p1Room = CreateMockRoom("P1Room", RoomGroup.VANILLA, palaceNumber: 1);
        var p2Room = CreateMockRoom("P2Room", RoomGroup.VANILLA, palaceNumber: 2);
        var genericRoom = CreateMockRoom("GenericRoom", RoomGroup.VANILLA);
        var bossRoom = CreateMockRoom("Boss1", RoomGroup.VANILLA, isBossRoom: true, palaceNumber: 1);

        var mockPalaceRooms = CreateMockPalaceRooms([p1Room, p2Room, genericRoom, bossRoom]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.NormalRooms.Should().Contain(r => r.Name == "P1Room");
        pool.NormalRooms.Should().Contain(r => r.Name == "GenericRoom");
        pool.NormalRooms.Should().NotContain(r => r.Name == "P2Room");
    }

    [TestMethod]
    public void Constructor_Props_RespectsAllowV4Flag()
    {
        var v4Room = CreateMockRoom("V4Room", RoomGroup.V4_0, palaceNumber: 1);

        var mockPalaceRooms = CreateMockPalaceRooms([], [v4Room]);

        var propsWithV4 = CreateMockProps(allowVanilla: true, allowV4: true);
        var poolWithV4 = new RoomPool(mockPalaceRooms, 1, propsWithV4);
        poolWithV4.NormalRooms.Should().Contain(r => r.Name == "V4Room");

        var propsWithoutV4 = CreateMockProps(allowVanilla: true, allowV4: false);
        var poolWithoutV4 = new RoomPool(mockPalaceRooms, 1, propsWithoutV4);
        poolWithoutV4.NormalRooms.Should().NotContain(r => r.Name == "V4Room");
    }

    [TestMethod]
    public void Constructor_Props_RespectsAllowV5Flag()
    {
        var v5Room = CreateMockRoom("V5Room", RoomGroup.V5_0, palaceNumber: 1);

        var mockPalaceRooms = CreateMockPalaceRooms([], v5Rooms: [v5Room]);

        var propsWithV5 = CreateMockProps(allowVanilla: true, allowV5: true);
        var poolWithV5 = new RoomPool(mockPalaceRooms, 1, propsWithV5);
        poolWithV5.NormalRooms.Should().Contain(r => r.Name == "V5Room");

        var propsWithoutV5 = CreateMockProps(allowVanilla: true, allowV5: false);
        var poolWithoutV5 = new RoomPool(mockPalaceRooms, 1, propsWithoutV5);
        poolWithoutV5.NormalRooms.Should().NotContain(r => r.Name == "V5Room");
    }

    [TestMethod]
    public void Constructor_Props_RemovesLongDeadEndsWhenConfigured()
    {
        var longDeadEndRoom = CreateMockRoom("LongDeadEnd", RoomGroup.VANILLA, palaceNumber: 1, tags: ["LongDeadEnd"]);
        var normalRoom = CreateMockRoom("Normal1", RoomGroup.VANILLA, palaceNumber: 1);
        var bossRoom = CreateMockRoom("Boss1", RoomGroup.VANILLA, isBossRoom: true, palaceNumber: 1);

        var mockPalaceRooms = CreateMockPalaceRooms([longDeadEndRoom, normalRoom, bossRoom]);
        var props = CreateMockProps(allowVanilla: true, removeLongDeadEnds: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.NormalRooms.Should().NotContain(r => r.Name == "LongDeadEnd");
        pool.NormalRooms.Should().Contain(r => r.Name == "Normal1");
    }

    [TestMethod]
    public void Constructor_Props_RemovesExpertRoomsWhenConfigured()
    {
        var expertRoom = CreateMockRoom("Expert1", RoomGroup.VANILLA, palaceNumber: 1, tags: ["Expert"]);
        var normalRoom = CreateMockRoom("Normal1", RoomGroup.VANILLA, palaceNumber: 1);

        var mockPalaceRooms = CreateMockPalaceRooms([expertRoom, normalRoom]);
        var props = CreateMockProps(allowVanilla: true, includeExpert: false);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.NormalRooms.Should().NotContain(r => r.Name == "Expert1");
        pool.NormalRooms.Should().Contain(r => r.Name == "Normal1");
    }

    [TestMethod]
    public void Constructor_Props_FallsBackToVanillaEntrancesWhenNoneInPool()
    {
        var entranceRoom = CreateMockRoom("VanillaEntrance", RoomGroup.VANILLA, isEntrance: true, palaceNumber: 1, hasDownExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([entranceRoom]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.Entrances.Should().Contain(r => r.Name == "VanillaEntrance");
    }

    #endregion

    #region Copy constructor

    [TestMethod]
    public void CopyConstructor_CopiesAllCollections()
    {
        var normalRoom = CreateMockRoom("Normal1", RoomGroup.VANILLA, palaceNumber: 1);
        var entranceRoom = CreateMockRoom("Entrance1", RoomGroup.VANILLA, isEntrance: true, palaceNumber: 1);
        var bossRoom = CreateMockRoom("Boss1", RoomGroup.VANILLA, isBossRoom: true, palaceNumber: 1);
        var tbirdRoom = CreateMockRoom("Tbird1", RoomGroup.VANILLA, isThunderBirdRoom: true, palaceNumber: 7);
        var itemRoom = CreateMockRoom("Item1", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasUpExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([normalRoom, entranceRoom, bossRoom, tbirdRoom, itemRoom]);
        var props = CreateMockProps(allowVanilla: true);
        var original = new RoomPool(mockPalaceRooms, 1, props);

        var copy = new RoomPool(original);

        copy.TbirdRooms.Should().BeEmpty();
        copy.LinkedRooms.Should().BeEmpty();
        copy.NormalRooms.Should().BeEquivalentTo(original.NormalRooms);
        copy.Entrances.Should().BeEquivalentTo(original.Entrances);
        copy.BossRooms.Should().BeEquivalentTo(original.BossRooms);
        copy.TbirdRooms.Should().BeEquivalentTo(original.TbirdRooms);
        copy.ItemRooms.Should().BeEquivalentTo(original.ItemRooms);
        copy.VanillaBossRoom.Should().Be(original.VanillaBossRoom);
        copy.LinkedRooms.Should().BeEquivalentTo(original.LinkedRooms);
    }

    [TestMethod]
    public void CopyConstructor_ShallowClonesCollections()
    {
        var normalRoom = CreateMockRoom("Normal1", RoomGroup.VANILLA, palaceNumber: 1);

        var mockPalaceRooms = CreateMockPalaceRooms([normalRoom]);
        var props = CreateMockProps(allowVanilla: true);
        var original = new RoomPool(mockPalaceRooms, 1, props);
        var copy = new RoomPool(original);

        copy.NormalRooms.Should().NotBeSameAs(original.NormalRooms);
        copy.Entrances.Should().NotBeSameAs(original.Entrances);
        copy.BossRooms.Should().NotBeSameAs(original.BossRooms);
        copy.NormalRooms.First().Should().BeSameAs(original.NormalRooms.First());
        copy.BossRooms.First().Should().BeSameAs(original.BossRooms.First());
    }

    #endregion

    #region RemoveRoom / RemoveRooms

    [TestMethod]
    public void RemoveRoom_RemovesFromCorrectCategory()
    {
        var normalRoom = CreateMockRoom("Normal1", RoomGroup.VANILLA, palaceNumber: 1);

        var mockPalaceRooms = CreateMockPalaceRooms([normalRoom]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.RemoveRoom(normalRoom);

        pool.NormalRooms.Should().NotContain(normalRoom);
    }

    [TestMethod]
    public void RemoveRoom_RemovesItemRoomFromAllCollections()
    {
        var itemRoom = CreateMockRoom("Item1", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasUpExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([itemRoom]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.ItemRooms.Should().Contain(itemRoom);
        pool.RemoveRoom(itemRoom);
        pool.ItemRooms.Should().NotContain(itemRoom);
    }

    [TestMethod]
    public void RemoveRooms_WithPredicate_RemovesMatchingRooms()
    {
        var room1 = CreateMockRoom("Room1", RoomGroup.VANILLA, palaceNumber: 1, tags: ["Remove"]);
        var room2 = CreateMockRoom("Room2", RoomGroup.VANILLA, palaceNumber: 1);

        var mockPalaceRooms = CreateMockPalaceRooms([room1, room2]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.RemoveRooms(r => r.HasTag("Remove"));

        pool.NormalRooms.Should().NotContain(r => r.Name == "Room1");
        pool.NormalRooms.Should().Contain(r => r.Name == "Room2");
    }

    #endregion

    #region RemoveDuplicates

    [TestMethod]
    public void RemoveDuplicates_NoDuplicateRooms_RemovesSingleRoom()
    {
        var itemRoom1 = CreateMockRoom("Item1", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasUpExit: true);
        var itemRoom2 = CreateMockRoom("Item2", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasUpExit: true);
        var bossRoom = CreateMockRoom("Boss1", RoomGroup.VANILLA, isBossRoom: true, palaceNumber: 1);

        var mockPalaceRooms = CreateMockPalaceRooms([itemRoom1, itemRoom2, bossRoom]);
        var props = CreateMockProps(allowVanilla: true);
        props.NoDuplicateRooms = true;
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.ItemRooms.Should().Contain(itemRoom1);
        pool.RemoveDuplicates(props, itemRoom1);
        pool.ItemRooms.Should().NotContain(itemRoom1);
        pool.ItemRooms.Should().Contain(itemRoom2);
    }

    [TestMethod]
    public void RemoveDuplicates_NoDuplicateRoomsBySideview_RemovesMatchingSideview()
    {
        var itemRoom1 = CreateMockRoom("ItemVariant1", RoomGroup.V4_0, duplicateGroup: "ItemVariantGroup1", hasItem: true, palaceNumber: 1, hasUpExit: true);
        var itemRoom2 = CreateMockRoom("ItemVariant2", RoomGroup.V4_0, duplicateGroup: "ItemVariantGroup1", hasItem: true, palaceNumber: 1, hasUpExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([itemRoom1, itemRoom2]);
        var props = CreateMockProps(allowVanilla: false, allowV4: true);
        props.NoDuplicateRoomsBySideview = true;
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.ItemRooms.Should().HaveCount(2);
        pool.RemoveDuplicates(props, itemRoom1);
        pool.ItemRooms.Should().BeEmpty();
    }

    #endregion

    #region LinkedRooms

    [TestMethod]
    public void Constructor_GathersLinkedRooms()
    {
        var primaryRoom = CreateMockRoom("Primary", RoomGroup.VANILLA, palaceNumber: 1,
            linkedRoomName: "Secondary");
        var secondaryRoom = CreateMockRoom("Secondary", RoomGroup.VANILLA, palaceNumber: 1,
            linkedRoomName: "Primary", enabled: false);

        var mockPalaceRooms = CreateMockPalaceRooms([primaryRoom, secondaryRoom]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.LinkedRooms.Should().ContainKey("Primary");
        pool.LinkedRooms.Should().ContainKey("Secondary");
        pool.NormalRooms.Should().Contain(primaryRoom);
        pool.NormalRooms.Should().NotContain(secondaryRoom);
    }

    #endregion

    #region CategorizeNormalRoomExits / GetNormalRoomsForExitType

    [TestMethod]
    public void CategorizeNormalRoomExits_ReturnsCategorizedRooms()
    {
        var deadendRight = CreateMockRoom("DeadendRight", RoomGroup.VANILLA, palaceNumber: 1, hasRightExit: true);
        var deadendLeft = CreateMockRoom("DeadendLeft", RoomGroup.VANILLA, palaceNumber: 1, hasLeftExit: true);
        var passThrough = CreateMockRoom("PassThrough", RoomGroup.VANILLA, palaceNumber: 1, hasLeftExit: true, hasRightExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([deadendRight, deadendLeft, passThrough]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        var categorized = pool.CategorizeNormalRoomExits();

        categorized[RoomExitType.DEADEND_EXIT_RIGHT].Should().ContainSingle().Which.Name.Should().Be("DeadendRight");
        categorized[RoomExitType.DEADEND_EXIT_LEFT].Should().ContainSingle().Which.Name.Should().Be("DeadendLeft");
        categorized[RoomExitType.HORIZONTAL_PASSTHROUGH].Should().ContainSingle().Which.Name.Should().Be("PassThrough");
    }

    [TestMethod]
    public void GetNormalRoomsForExitType_ReturnsMatchingRooms()
    {
        var deadendRight = CreateMockRoom("DeadendRight", RoomGroup.VANILLA, palaceNumber: 1, hasRightExit: true);
        var deadendLeft = CreateMockRoom("DeadendLeft", RoomGroup.VANILLA, palaceNumber: 1, hasLeftExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([deadendRight, deadendLeft]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        var rightRooms = pool.GetNormalRoomsForExitType(RoomExitType.DEADEND_EXIT_RIGHT);
        rightRooms.Should().ContainSingle().Which.Name.Should().Be("DeadendRight");

        var leftRooms = pool.GetNormalRoomsForExitType(RoomExitType.DEADEND_EXIT_LEFT);
        leftRooms.Should().ContainSingle().Which.Name.Should().Be("DeadendLeft");
    }

    #endregion

    #region GetItemRoomShapes / GetItemRoomsForShape

    [TestMethod]
    public void GetItemRoomShapes_ReturnsDistinctShapes()
    {
        var itemRight = CreateMockRoom("ItemRight", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasRightExit: true);
        var itemUp = CreateMockRoom("ItemUp", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasUpExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([itemRight, itemUp]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        var shapes = pool.GetItemRoomShapes().ToList();
        shapes.Should().Contain(RoomExitType.DEADEND_EXIT_RIGHT);
        shapes.Should().Contain(RoomExitType.DEADEND_EXIT_UP);
    }

    [TestMethod]
    public void GetItemRoomsForShape_ReturnsMatchingRooms()
    {
        var itemRight = CreateMockRoom("ItemRight", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasRightExit: true);
        var itemUp = CreateMockRoom("ItemUp", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasUpExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([itemRight, itemUp]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        var rightRooms = pool.GetItemRoomsForShape(RoomExitType.DEADEND_EXIT_RIGHT);
        rightRooms.Should().Contain(r => r.Name == "ItemRight");

        var upRooms = pool.GetItemRoomsForShape(RoomExitType.DEADEND_EXIT_UP);
        upRooms.Should().Contain(r => r.Name == "ItemUp");
    }

    #endregion

    #region GetMergedExitType

    [TestMethod]
    public void GetMergedExitType_ReturnsMergedTypeForLinkedRoom()
    {
        var primaryRoom = CreateMockRoom("Primary", RoomGroup.VANILLA, palaceNumber: 1,
            hasRightExit: true, linkedRoomName: "Secondary");
        var secondaryRoom = CreateMockRoom("Secondary", RoomGroup.VANILLA, palaceNumber: 1,
            hasLeftExit: true, linkedRoomName: "Primary", enabled: false);

        var mockPalaceRooms = CreateMockPalaceRooms([primaryRoom, secondaryRoom]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        var mergedType = pool.GetMergedExitType(primaryRoom);
        mergedType.Should().Be(RoomExitType.HORIZONTAL_PASSTHROUGH);
    }

    [TestMethod]
    public void GetMergedExitType_ReturnsOwnTypeWhenNoLinkedRoom()
    {
        var normalRoom = CreateMockRoom("Normal1", RoomGroup.VANILLA, palaceNumber: 1, hasRightExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([normalRoom]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        var exitType = pool.GetMergedExitType(normalRoom);
        exitType.Should().Be(RoomExitType.DEADEND_EXIT_RIGHT);
    }

    #endregion

    #region RefillNormalRoomsForExitType

    [TestMethod]
    public void RefillNormalRoomsForExitType_AddsRoomsFromSourcePool()
    {
        var room1 = CreateMockRoom("Room1", RoomGroup.VANILLA, palaceNumber: 1, hasRightExit: true);
        var room2 = CreateMockRoom("Room2", RoomGroup.VANILLA, palaceNumber: 1, hasRightExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([room1, room2]);
        var props = CreateMockProps(allowVanilla: true);
        var sourcePool = new RoomPool(mockPalaceRooms, 1, props);
        var targetPool = new RoomPool(mockPalaceRooms, 1, props);

        targetPool.NormalRooms.Clear();

        targetPool.RefillNormalRoomsForExitType(sourcePool, RoomExitType.DEADEND_EXIT_RIGHT);

        targetPool.NormalRooms.Should().HaveCount(2);
        targetPool.NormalRooms.Should().Contain(r => r.Name == "Room1");
        targetPool.NormalRooms.Should().Contain(r => r.Name == "Room2");
    }

    #endregion

    #region ItemRoomsByDirection

    [TestMethod]
    public void FinalizePool_PopulatesItemRoomsByDirection()
    {
        var itemUp = CreateMockRoom("ItemUp", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasUpExit: true);
        var itemDown = CreateMockRoom("ItemDown", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasDownExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([itemUp, itemDown]);
        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.ItemRoomsByDirection.Should().ContainKey(Direction.NORTH);
        pool.ItemRoomsByDirection.Should().ContainKey(Direction.SOUTH);
        pool.ItemRoomsByDirection[Direction.NORTH].Keys().Should().Contain(itemUp);
        pool.ItemRoomsByDirection[Direction.SOUTH].Keys().Should().Contain(itemDown);
    }

    [TestMethod]
    public void RemoveRoom_RemovesFromItemRoomsByDirection()
    {
        var itemUp = CreateMockRoom("ItemUp", RoomGroup.VANILLA, hasItem: true, palaceNumber: 1, hasUpExit: true);

        var mockPalaceRooms = CreateMockPalaceRooms([itemUp]);

        var props = CreateMockProps(allowVanilla: true);
        var pool = new RoomPool(mockPalaceRooms, 1, props);

        pool.ItemRoomsByDirection.Should().ContainKey(Direction.NORTH);
        pool.ItemRoomsByDirection[Direction.NORTH].Keys().Should().Contain(itemUp);
        pool.RemoveRoom(itemUp);
        pool.ItemRoomsByDirection.Should().NotContainKey(Direction.NORTH);
    }

    #endregion
}
