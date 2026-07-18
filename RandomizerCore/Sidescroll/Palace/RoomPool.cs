using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Sidescroll.Palace;

public class RoomPool
{
    public List<Room> NormalRooms { get; protected set; } = [];
    public List<Room> Entrances { get; protected set; } = [];
    public List<Room> BossRooms { get; protected set; } = [];
    public List<Room> TbirdRooms { get; protected set; } = [];
    public List<Room> ItemRooms { get; protected set; } = [];
    public Room VanillaBossRoom { get; protected set; } = null!;
    public Dictionary<string, Room> LinkedRooms { get; } = [];
    public Dictionary<Direction, TableWeightedRandom<Room>> ItemRoomsByDirection { get; protected set; } = [];
    /// keeping this private so callers do not access the keys in non-deterministic ways
    private Dictionary<RoomExitType, List<Room>> ItemRoomsByShape { get; set; } = [];
    public Dictionary<RoomExitType, Room> DefaultStubsByDirection { get; protected set; } = [];
    public Room DefaultUpEntrance { get; protected set; } = null!;
    public Room DefaultDownBossRoom { get; protected set; } = null!;

    protected RoomPool() { }

    protected static readonly IEqualityComparer<byte[]> byteArrayEqualityComparer = new Util.StandardByteArrayEqualityComparer();

    public RoomPool(PalaceRooms palaceRooms, int palaceNumber, RandomizerProperties props)
    {
        var allRooms = GatherRoomsFromProps(palaceRooms, palaceNumber, props);
        ApplyPropertyExclusions(props);
        GatherLinkedRooms(allRooms, palaceRooms);
        SplitRooms(allRooms, palaceNumber);
        FinalizePool(palaceRooms, palaceNumber, props);
    }

    static List<Room> GatherRoomsFromProps(PalaceRooms palaceRooms, int palaceNumber, RandomizerProperties props)
    {
        var roomSet = new List<Room>();

            //4.4 GP room pool is too shallow to create proper palaces from right now, so if you pick 4.4 only,
            //GP also has vanilla rooms added.
        bool allowVanilla = props.AllowVanillaRooms
            || (palaceNumber == 7 && !props.AllowVanillaRooms && !props.AllowV4Rooms && props.AllowV5_0Rooms);

        if (allowVanilla)
        {
            AddGroup(roomSet, palaceRooms, RoomGroup.VANILLA);
        }

        if (props.AllowV4Rooms)
        {
            AddGroup(roomSet, palaceRooms, RoomGroup.V4_0);
        }

        if (props.AllowV5_0Rooms)
        {
            AddGroup(roomSet, palaceRooms, RoomGroup.V5_0);
        }

        return roomSet.ToList();
    }

    private static void AddGroup(List<Room> roomSet, PalaceRooms palaceRooms, RoomGroup group)
    {
        foreach (var room in palaceRooms.RoomsByGroup(group))
        {
            roomSet.Add(room);
        }
    }

    private void ApplyPropertyExclusions(RandomizerProperties props)
    {
        if (props.RemoveLongDeadEnds)
        {
            RemoveRooms(r => r.HasTag("LongDeadEnd"));
        }
        if (!props.IncludeExpertRooms)
        {
            RemoveRooms(r => r.HasTag("Expert"));
        }
    }

    private void SplitRooms(List<Room> allRooms, int palaceNumber)
    {
        foreach (var room in allRooms)
        {
            if (!room.InPoolForPalace(palaceNumber))
            {
                continue;
            }

            if (room.IsEntrance)
            {
                Entrances.Add(room);
            }
            else if (room.HasItem)
            {
                ItemRooms.Add(room);
            }
            else if (room.IsBossRoom)
        {
                BossRooms.Add(room);
            }
            else if (room.IsThunderBirdRoom)
            {
                TbirdRooms.Add(room);
            }
            else
            {
                NormalRooms.Add(room);
            }
        }
    }

    private void GatherLinkedRooms(List<Room> allRooms, PalaceRooms palaceRooms)
    {
        foreach (var room in allRooms)
        {
            if (room.Enabled && room.LinkedRoomName != null)
            {
                LinkedRooms[room.LinkedRoomName] = palaceRooms.GetRoomByName(room.LinkedRoomName)!;
                LinkedRooms[room.Name] = room;
            }
        }
    }

    /// Initializes a new <see cref="RoomPool"/> by copying an existing instance.
    /// Collections are shallow-cloned. Contained <see cref="Room"/> objects are
    /// shared between instances.
    public RoomPool(RoomPool target)
    {
        NormalRooms.AddRange(target.NormalRooms);
        Entrances.AddRange(target.Entrances);
        BossRooms.AddRange(target.BossRooms);
        TbirdRooms.AddRange(target.TbirdRooms);
        ItemRooms.AddRange(target.ItemRooms);

        VanillaBossRoom = target.VanillaBossRoom;
        DefaultUpEntrance = target.DefaultUpEntrance;
        DefaultDownBossRoom = target.DefaultDownBossRoom;

        foreach (var pair in target.LinkedRooms)
        {
            LinkedRooms.Add(pair.Key, pair.Value);
        }
        foreach (var pair in target.ItemRoomsByDirection)
        {
            ItemRoomsByDirection[pair.Key] = (TableWeightedRandom<Room>)pair.Value.Clone();
        }
        foreach (var pair in target.ItemRoomsByShape)
        {
            ItemRoomsByShape[pair.Key] = [.. pair.Value];
        }
        foreach (var pair in target.DefaultStubsByDirection)
        {
            DefaultStubsByDirection.Add(pair.Key, pair.Value);
        }
    }

    private void RemoveBlockedRooms(int palaceNumber, RandomizerProperties props)
    {
        var palaceBlockers = !props.BlockersAnywhere ? Palaces.ALLOWED_BLOCKERS_BY_PALACE[palaceNumber - 1] : Palaces.ALL_PALACE_ALLOWED_BLOCKERS;
        HashSet<RequirementType> allowedBlockers = [.. palaceBlockers];
        if (!props.ReplaceFireWithDash)
        {
            allowedBlockers.Remove(RequirementType.DASH);
        }
        RemoveRooms(room => !room.IsTraversable(allowedBlockers));
    }

    void FinalizePool(PalaceRooms palaceRooms, int palaceNumber, RandomizerProperties props)
    {
        DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_DOWN, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).First(i => i.HasDownExit));
        DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_UP, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).First(i => i.HasUpExit));

        foreach (var direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
        {
            var weightedRooms = ItemRooms
                .Where(room => room.HasExitInDirection(direction))
                .Select(room => (room, 5 - RoomExitCount(room))) // (room, weight) pair where weight is higher with less exits
                .ToList();

            if (weightedRooms.Count > 0)
            {
                ItemRoomsByDirection[direction] = new TableWeightedRandom<Room>(weightedRooms);
            }
        }

        var exitTypes = ItemRooms.Select(r => r.CategorizeExits()).Distinct();
        foreach (var shape in exitTypes)
        {
            var shapeRooms = ItemRooms.Where(r => r.CategorizeExits() == shape).ToList();
            var ls = ItemRoomsByShape.GetValueOrDefault(shape, []);
            ls.AddRange(shapeRooms);
            ItemRoomsByShape[shape] = ls;
        }

        // this should probably be done per-room before creating the collections above instead of rebuilding the lists after
        var linkedRoomShapes = ItemRoomsByShape
            .SelectMany(pair => pair.Value, (shape, room) => (shape, room))
            .Where(r => r.room.LinkedRoomName != null)
            .Select(r => (GetMergedExitType(r.room), r.room))
            .ToList();

        foreach ((RoomExitType newShape, Room room) in linkedRoomShapes)
        {
            var originalShape = room.CategorizeExits();
            ItemRoomsByShape[originalShape].Remove(room);
            var ls = ItemRoomsByShape.GetValueOrDefault(newShape, []);
            ls.Add(room);
            ItemRoomsByShape[newShape] = ls;
        }

        if (Entrances.Count == 0)
        {
            Entrances.AddRange(palaceRooms.Entrances(RoomGroup.VANILLA).Where(i => i.PalaceNumber == palaceNumber));
        }

        VanillaBossRoom = palaceRooms.VanillaBossRoom(palaceNumber);
        if (BossRooms.Count == 0)
        {
            BossRooms.Add(VanillaBossRoom);
        }

        DefaultUpEntrance = palaceRooms.Entrances(RoomGroup.V5_0)
            .FirstOrDefault(i => i.IsEntrance && i.CategorizeExits() == RoomExitType.DEADEND_EXIT_UP && i.PalaceNumber == palaceNumber)!;
        Debug.Assert(DefaultUpEntrance != null);

        DefaultDownBossRoom = palaceRooms.BossRooms(RoomGroup.V4_0)
            .First(i => i.IsBossRoom && i.CategorizeExits() == RoomExitType.DEADEND_EXIT_DOWN
                     && (palaceNumber >= 6 ? i.PalaceNumber == palaceNumber : i.PalaceNumber == null));

        RemoveBlockedRooms(palaceNumber, props);
    }

    static int RoomExitCount(Room room)
    {
        bool[] exits = [room.HasUpExit, room.HasDownExit, room.HasLeftExit, room.HasRightExit, room.IsDropZone];
        return exits.Count(i => i);
    }

    public IEnumerable<RoomExitType> GetItemRoomShapes()
    {
        var keys = ItemRoomsByShape.Keys;
        // workaround for our map type not having a deterministic order
        return RoomExitTypeExtensions.ALL.Where(keys.Contains);
    }

    public List<Room> GetItemRoomsForShape(RoomExitType itemRoomExitType)
    {
        return ItemRoomsByShape[itemRoomExitType];
    }

    public void RemoveDuplicates(RandomizerProperties props, Room roomThatWasUsed)
    {
        //Default stubs are used when no deadend rooms of that direction exist. They are always allowed to
        //be duplicates and should never be removed from the pool.
        if (DefaultStubsByDirection.TryGetValue(roomThatWasUsed.CategorizeExits(), out Room? directionStub))
        {
            if (directionStub == roomThatWasUsed)
            {
                return;
            }
        }

        if (props.NoDuplicateRoomsBySideview)
        {
            var sideviewBytes = roomThatWasUsed.SideView;
            RemoveRooms(room => byteArrayEqualityComparer.Equals(room.SideView, sideviewBytes));
        }
        else if (props.NoDuplicateRooms)
        {
            RemoveRoom(roomThatWasUsed);
        }
    }

    public void RemoveRoom(Room room)
    {
        NormalRooms.Remove(room);
        Entrances.Remove(room);
        BossRooms.Remove(room);
        TbirdRooms.Remove(room);
        RemoveFromItemRooms(room);
    }

    public void RemoveRooms(Predicate<Room> removalCondition)
    {
        NormalRooms.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        Entrances.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        BossRooms.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        TbirdRooms.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        ItemRooms.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        RemoveFromItemRooms(removalCondition);
    }

    void RemoveFromItemRooms(Room room)
    {
        ItemRooms.Remove(room);
        foreach (Direction direction in ItemRoomsByDirection.Keys)
        {
            var originalTable = ItemRoomsByDirection[direction];
            var newTable = (TableWeightedRandom<Room>)originalTable.Subtract(room);
            if (newTable != originalTable)
            {
                ItemRoomsByDirection[direction] = newTable;
            }
        }
        foreach (var key in ItemRoomsByShape.Keys)
        {
            ItemRoomsByShape[key].Remove(room);
        }
    }

    void RemoveFromItemRooms(Predicate<Room> removalCondition)
    {
        foreach (Direction direction in ItemRoomsByDirection.Keys)
        {
            var originalTable = ItemRoomsByDirection[direction];
            var newTable = originalTable;
            foreach (Room room in originalTable.Keys().ToList())
            {
                if (RoomMatchesIncludingLinked(room, removalCondition))
                {
                    newTable = (TableWeightedRandom<Room>)newTable.Subtract(room);
                }
            }
            if (newTable != originalTable)
            {
                ItemRoomsByDirection[direction] = newTable;
            }
        }

        foreach (var key in ItemRoomsByShape.Keys)
        {
            ItemRoomsByShape[key].RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        }
    }

    private bool RoomMatchesIncludingLinked(Room room, Predicate<Room> match)
    {
        if (match(room)) { return true; }

        if (room.LinkedRoomName != null)
        {
            if (LinkedRooms.TryGetValue(room.LinkedRoomName, out var linked))
            {
                return match(linked);
            }
            else
            {
                throw new Exception($"Linked room \"{room.LinkedRoomName}\" is referenced but is not in LinkedRooms collection.");
            }
        }

        return false;
    }

    public Dictionary<RoomExitType, List<Room>> CategorizeNormalRoomExits(bool linkRooms = false)
    {
        Dictionary<RoomExitType, List<Room>> categorizedRooms = new Dictionary<RoomExitType, List<Room>>(NormalRooms.Count);
        foreach(Room room in NormalRooms)
        {
            var type = GetMergedExitType(room);
            if(!categorizedRooms.TryGetValue(type, out List<Room>? value))
            {
                value = new List<Room>(NormalRooms.Count);
                categorizedRooms[type] = value;
            }
            value.Add(room);
        }
        return categorizedRooms;
    }

    RoomExitType GetMergedExitType(Room room)
    {
        var type = room.CategorizeExits();
        if (room.LinkedRoomName != null && LinkedRooms.TryGetValue(room.LinkedRoomName, out var linked))
        {
            type = type.Merge(linked.CategorizeExits());
        }
        return type;
    }

    public List<Room> GetNormalRoomsForExitType(RoomExitType exitType, bool linkRooms = false)
    {
        return NormalRooms.Where(room => GetMergedExitType(room) == exitType).ToList();
    }

    public void RefillNormalRoomsForExitType(RoomPool rooms, RoomExitType exitType)
    {
        var originalRooms = rooms.GetNormalRoomsForExitType(exitType);
        NormalRooms.AddRange(originalRooms);
    }
}
