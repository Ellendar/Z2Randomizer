using NLog.Targets;
using SD.Tools.Algorithmia.GeneralDataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class RoomPool
{
    public List<Room> NormalRooms { get; set; } = [];
    public List<Room> Entrances { get; set; } = [];
    public List<Room> BossRooms { get; set; } = [];
    public List<Room> TbirdRooms { get; set; } = [];
    public List<Room> ItemRooms { get; set; } = [];
    public Room VanillaBossRoom { get; set; }
    public Dictionary<string, Room> LinkedRooms { get; } = [];
    public Dictionary<Direction, TableWeightedRandom<Room>> ItemRoomsByDirection { get; set; } = [];
    /// keeping this private so callers do not access the keys in non-deterministic ways
    private Dictionary<RoomExitType, List<Room>> ItemRoomsByShape { get; set; } = [];
    public Dictionary<RoomExitType, Room> DefaultStubsByDirection { get; set; } = [];
    public Room DefaultUpEntrance { get; }
    public Room DefaultDownBossRoom { get; }

#pragma warning disable CS8618 
    protected RoomPool() { }
#pragma warning restore CS8618 

    protected static readonly IEqualityComparer<byte[]> byteArrayEqualityComparer = new Util.StandardByteArrayEqualityComparer();

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
        foreach (var direction in target.ItemRoomsByDirection.Keys)
        {
            ItemRoomsByDirection[direction] = target.ItemRoomsByDirection[direction];
        }
        foreach (var shape in target.ItemRoomsByShape.Keys)
        {
            var ls = target.ItemRoomsByShape[shape];
            ItemRoomsByShape[shape] = ls.ToList();
        }
        foreach (var key in target.DefaultStubsByDirection.Keys)
        {
            DefaultStubsByDirection.Add(key, target.DefaultStubsByDirection[key]);
        }
    }

    public RoomPool(PalaceRooms palaceRooms, int palaceNumber, RandomizerProperties props)
    {
        Dictionary<Direction, List<(Room, int)>> roomDirectionWeightsByDirection = [];
        roomDirectionWeightsByDirection.Add(Direction.NORTH, []);
        roomDirectionWeightsByDirection.Add(Direction.SOUTH, []);
        roomDirectionWeightsByDirection.Add(Direction.WEST, []);
        roomDirectionWeightsByDirection.Add(Direction.EAST, []);

        if (props.AllowVanillaRooms
            //4.4 GP room pool is too shallow to create proper palaces from right now, so if you pick 4.4 only,
            //GP also has vanilla rooms added.
            || (palaceNumber == 7 && !props.AllowVanillaRooms && !props.AllowV4Rooms && props.AllowV5_0Rooms))
        {
            Entrances.AddRange(palaceRooms.Entrances(RoomGroup.VANILLA)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            BossRooms.AddRange(palaceRooms.BossRooms(RoomGroup.VANILLA)
                .Where(i => (i.PalaceNumber == null && palaceNumber < 6) || i.PalaceNumber == palaceNumber).ToList());
            TbirdRooms.AddRange(palaceRooms.ThunderBirdRooms(RoomGroup.VANILLA)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            ItemRooms.AddRange(palaceRooms.ItemRooms(RoomGroup.VANILLA)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            foreach (var room in palaceRooms.LinkedRooms(RoomGroup.VANILLA))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (var direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
            {
                foreach (Room room in palaceRooms.ItemRoomsByDirection(RoomGroup.VANILLA, direction).ToList())
                {
                    bool[] weights = [room.HasUpExit, room.HasDownExit, room.HasLeftExit, room.HasRightExit, room.IsDropZone];
                    roomDirectionWeightsByDirection[direction].Add((room, 5 - weights.Count(i => i)));
                }
            }
            foreach(var shape in palaceRooms.ItemRooms(RoomGroup.VANILLA).Select(i => i.CategorizeExits()).Distinct())
            {
                var newRooms = palaceRooms.ItemRoomsByShape(RoomGroup.VANILLA, shape);
                var ls = ItemRoomsByShape.GetValueOrDefault(shape, []);
                ls.AddRange(newRooms);
                ItemRoomsByShape[shape] = ls;
            }
        }

        if (props.AllowV4Rooms)
        {
            Entrances.AddRange(palaceRooms.Entrances(RoomGroup.V4_0)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            BossRooms.AddRange(palaceRooms.BossRooms(RoomGroup.V4_0)
                .Where(i => (i.PalaceNumber == null && palaceNumber < 6) || i.PalaceNumber == palaceNumber).ToList());
            TbirdRooms.AddRange(palaceRooms.ThunderBirdRooms(RoomGroup.V4_0)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            ItemRooms.AddRange(palaceRooms.ItemRooms(RoomGroup.V4_0)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            foreach (var room in palaceRooms.LinkedRooms(RoomGroup.V4_0))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (var direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
            {
                foreach (Room room in palaceRooms.ItemRoomsByDirection(RoomGroup.V4_0, direction).ToList())
                {
                    bool[] weights = [room.HasUpExit, room.HasDownExit, room.HasLeftExit, room.HasRightExit, room.IsDropZone];
                    roomDirectionWeightsByDirection[direction].Add((room, 5 - weights.Count(i => i)));
                }
            }
            foreach (var shape in palaceRooms.ItemRooms(RoomGroup.V4_0).Select(i => i.CategorizeExits()).Distinct())
            {
                var newRooms = palaceRooms.ItemRoomsByShape(RoomGroup.V4_0, shape);
                var ls = ItemRoomsByShape.GetValueOrDefault(shape, []);
                ls.AddRange(newRooms);
                ItemRoomsByShape[shape] = ls;
            }
        }

        if (props.AllowV5_0Rooms)
        {
            Entrances.AddRange(palaceRooms.Entrances(RoomGroup.V5_0)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            BossRooms.AddRange(palaceRooms.BossRooms(RoomGroup.V5_0)
                .Where(i => (i.PalaceNumber == null && palaceNumber < 6) || i.PalaceNumber == palaceNumber).ToList());
            TbirdRooms.AddRange(palaceRooms.ThunderBirdRooms(RoomGroup.V5_0)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            ItemRooms.AddRange(palaceRooms.ItemRooms(RoomGroup.V5_0)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            foreach (var room in palaceRooms.LinkedRooms(RoomGroup.V5_0))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (var direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
            {
                foreach (Room room in palaceRooms.ItemRoomsByDirection(RoomGroup.V5_0, direction).ToList())
                {
                    bool[] weights = [room.HasUpExit, room.HasDownExit, room.HasLeftExit, room.HasRightExit, room.IsDropZone];
                    roomDirectionWeightsByDirection[direction].Add((room, 5 - weights.Count(i => i)));
                }
            }
            foreach (var shape in palaceRooms.ItemRooms(RoomGroup.V5_0).Select(i => i.CategorizeExits()).Distinct())
            {
                var newRooms = palaceRooms.ItemRoomsByShape(RoomGroup.V5_0, shape);
                var ls = ItemRoomsByShape.GetValueOrDefault(shape, []);
                ls.AddRange(newRooms);
                ItemRoomsByShape[shape] = ls;
            }
        }
        else
        {
            //If we are using these categorized exits to cap paths, there needs to always be a path of each type
            //Since vanilla and 4.0 don't normally contain up/down elevator deadends, we add some dummy ones
            DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_DOWN, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).Where(i => i.HasDownExit).First());
            DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_UP, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).Where(i => i.HasUpExit).First());
        }
        foreach (var direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
        {
            if (roomDirectionWeightsByDirection[direction].Count > 0)
            {
                ItemRoomsByDirection[direction] = new TableWeightedRandom<Room>(roomDirectionWeightsByDirection[direction]);
            }
        }

        List<(RoomExitType, Room)> linkedRoomShapes = [];
        foreach (var pair in ItemRoomsByShape)
        {
            foreach(Room room in pair.Value)
            {
                if(room.LinkedRoomName != null)
                {
                    RoomExitType newShape = room.CategorizeExits().Merge(LinkedRooms[room.LinkedRoomName].CategorizeExits());
                    linkedRoomShapes.Add((newShape, room));
                }
            }
        }
        foreach((RoomExitType shape, Room room) in linkedRoomShapes)
        {
            ItemRoomsByShape[room.CategorizeExits()].Remove(room);
            var ls = ItemRoomsByShape.GetValueOrDefault(shape, []);
            ls.Add(room);
            ItemRoomsByShape[shape] = ls;
        }

        //If we're using a room set that has no entraces, we still need to have something, so add the vanilla entrances.
        if (Entrances.Count == 0)
        {
            Entrances.AddRange(palaceRooms.Entrances(RoomGroup.VANILLA).Where(i => i.PalaceNumber == palaceNumber).ToList());
        }

        //same with boss rooms
        VanillaBossRoom = palaceRooms.VanillaBossRoom(palaceNumber);
        if (BossRooms.Count == 0)
        {
            BossRooms.Add(VanillaBossRoom);
        }

        //for tower, we need a default Up entrance and Down boss room in case the pool doesn't contain them
        DefaultUpEntrance = palaceRooms.Entrances(RoomGroup.V5_0)
            .FirstOrDefault(i => i.IsEntrance && i.CategorizeExits() == RoomExitType.DEADEND_EXIT_UP && i.PalaceNumber == palaceNumber)!;
        Debug.Assert(DefaultUpEntrance != null);
        //in the 4.0 boss rooms, P6/7 have their own rooms, but 1-5 are generic
        if(palaceNumber >= 6)
        {
            DefaultDownBossRoom = palaceRooms.BossRooms(RoomGroup.V4_0)
                .First(i => i.IsBossRoom && i.CategorizeExits() == RoomExitType.DEADEND_EXIT_DOWN && i.PalaceNumber == palaceNumber);
        }
        else
        {
            DefaultDownBossRoom = palaceRooms.BossRooms(RoomGroup.V4_0)
                .First(i => i.IsBossRoom && i.CategorizeExits() == RoomExitType.DEADEND_EXIT_DOWN && i.PalaceNumber == null);
        }

        if (palaceNumber == 7)
        {
            if (props.AllowVanillaRooms
            //4.4 GP room pool is too shallow to create proper palaces from right now, so if you pick 4.4 only,
            //GP also has vanilla rooms added.
            || (!props.AllowVanillaRooms && !props.AllowV4Rooms && props.AllowV5_0Rooms))
            {
                NormalRooms.AddRange(palaceRooms.GpRoomsByGroup(RoomGroup.VANILLA));
            }

            if (props.AllowV4Rooms)
            {
                NormalRooms.AddRange(palaceRooms.GpRoomsByGroup(RoomGroup.V4_0));
            }

            if (props.AllowV5_0Rooms)
            {
                NormalRooms.AddRange(palaceRooms.GpRoomsByGroup(RoomGroup.V5_0));
            }
        }
        else
        {
            if (props.AllowVanillaRooms)
            {
                NormalRooms.AddRange(palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.VANILLA));
            }

            if (props.AllowV4Rooms)
            {
                NormalRooms.AddRange(palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.V4_0));
            }

            if (props.AllowV5_0Rooms)
            {
                NormalRooms.AddRange(palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.V5_0));
            }
        }

        if (!props.BlockersAnywhere)
        {
            RequirementType[] allowedBlockers = Palaces.ALLOWED_BLOCKERS_BY_PALACE[palaceNumber - 1];
            RemoveRooms(room => !room.IsTraversable(allowedBlockers));
            foreach (var key in ItemRoomsByDirection.Keys)
            {
                List<(Room, int)> updatedWeights = [];
                foreach (Room room in ItemRoomsByDirection[key].Keys())
                {
                    if (room.IsTraversable(allowedBlockers))
                    {
                        updatedWeights.Add((room, ItemRoomsByDirection[key].Weight(room)));
                    }
                }
                ItemRoomsByDirection[key] = new TableWeightedRandom<Room>(updatedWeights);
            }
        }
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
        ItemRooms.Remove(room);
        foreach(Direction direction in ItemRoomsByDirection.Keys)
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

    public void RemoveRooms(Predicate<Room> removalCondition)
    {
        NormalRooms.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        Entrances.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        BossRooms.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        TbirdRooms.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        ItemRooms.RemoveAll(room => RoomMatchesIncludingLinked(room, removalCondition));
        foreach (Direction direction in ItemRoomsByDirection.Keys)
        {
            var originalTable = ItemRoomsByDirection[direction];
            var newTable = originalTable;
            var keysCopy = newTable.Keys().ToList();
            foreach (Room room in keysCopy)
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
                throw new Exception($"Linked room \"{room.LinkedRoomName}\" is references but is not in LinkedRooms pool.");
            }
        }
        return false;
    }

    public Dictionary<RoomExitType, List<Room>> CategorizeNormalRoomExits(bool linkRooms = false)
    {
        Dictionary<RoomExitType, List<Room>> categorizedRooms = new Dictionary<RoomExitType, List<Room>>(NormalRooms.Count);
        foreach(Room room in NormalRooms)
        {
            RoomExitType type = room.CategorizeExits();
            if(room.LinkedRoomName != null)
            {
                type = type.Merge(LinkedRooms[room.LinkedRoomName].CategorizeExits());
            }
            if(!categorizedRooms.TryGetValue(type, out List<Room>? value))
            {
                value = new List<Room>(NormalRooms.Count);
                categorizedRooms[type] = value;
            }

            value.Add(room);
        }
        
        return categorizedRooms;
    }

    public List<Room> GetNormalRoomsForExitType(RoomExitType exitType, bool linkRooms = false)
    {
        return NormalRooms.Where(room =>
        {
            var roomType = room.CategorizeExits();
            if (room.LinkedRoomName != null)
            {
                roomType = roomType.Merge(LinkedRooms[room.LinkedRoomName].CategorizeExits());
            }
            return roomType == exitType;
        }).ToList();
    }
}
