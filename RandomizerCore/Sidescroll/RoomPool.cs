using System;
using System.Collections.Generic;
using System.Linq;
using SD.Tools.Algorithmia.GeneralDataStructures;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class RoomPool
{
    public List<Room> NormalRooms { get; set; } = [];
    public List<Room> Entrances { get; set; } = [];
    public List<Room> BossRooms { get; set; } = [];
    public List<Room> TbirdRooms { get; set; } = [];
    public Room VanillaBossRoom { get; set; }
    public Dictionary<string, Room> LinkedRooms { get; } = [];
    public MultiValueDictionary<Direction, Room> ItemRoomsByDirection { get; set; } = [];
    public MultiValueDictionary<RoomExitType, Room> ItemRoomsByShape { get; set; } = [];
    public Dictionary<RoomExitType, Room> DefaultStubsByDirection { get; set; } = [];

    private PalaceRooms palaceRooms;

#pragma warning disable CS8618 
    protected RoomPool() { }
#pragma warning restore CS8618 

    public RoomPool(RoomPool target)
    {
        palaceRooms = target.palaceRooms;
        NormalRooms.AddRange(target.NormalRooms);
        Entrances.AddRange(target.Entrances);
        BossRooms.AddRange(target.BossRooms);
        TbirdRooms.AddRange(target.TbirdRooms);
        VanillaBossRoom = target.VanillaBossRoom;
        foreach (var room in target.LinkedRooms)
        {
            LinkedRooms.Add(room.Key, room.Value);
        }
        foreach (var key in target.ItemRoomsByDirection.Keys)
        {
            ItemRoomsByDirection.AddRange(key, target.ItemRoomsByDirection[key]);
        }
        foreach (var key in target.ItemRoomsByShape.Keys)
        {
            ItemRoomsByShape.AddRange(key, target.ItemRoomsByShape[key]);
        }
        foreach (var key in target.DefaultStubsByDirection.Keys)
        {
            DefaultStubsByDirection.Add(key, target.DefaultStubsByDirection[key]);
        }
    }

    public RoomPool(PalaceRooms palaceRooms, int palaceNumber, RandomizerProperties props)
    {
        this.palaceRooms = palaceRooms;
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
            foreach(var room in palaceRooms.LinkedRooms(RoomGroup.VANILLA))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (var direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
            {
                ItemRoomsByDirection.AddRange(direction, palaceRooms.ItemRoomsByDirection(RoomGroup.VANILLA, direction).ToList());
            }
            foreach(RoomExitType itemRoomExitType in palaceRooms.ItemRooms(RoomGroup.VANILLA).Select(i => i.CategorizeExits()).Distinct())
            {
                ItemRoomsByShape.AddRange(itemRoomExitType, palaceRooms.ItemRoomsByShape(RoomGroup.VANILLA, itemRoomExitType));
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
            foreach (var room in palaceRooms.LinkedRooms(RoomGroup.V4_0))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (var direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
            {
                ItemRoomsByDirection.AddRange(direction, palaceRooms.ItemRoomsByDirection(RoomGroup.V4_0, direction).ToList());
            }
            foreach (RoomExitType itemRoomExitType in palaceRooms.ItemRooms(RoomGroup.V4_0).Select(i => i.CategorizeExits()).Distinct())
            {
                ItemRoomsByShape.AddRange(itemRoomExitType, palaceRooms.ItemRoomsByShape(RoomGroup.V4_0, itemRoomExitType));
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
            foreach (var room in palaceRooms.LinkedRooms(RoomGroup.V5_0))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (var direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
            {
                ItemRoomsByDirection.AddRange(direction, palaceRooms.ItemRoomsByDirection(RoomGroup.V5_0, direction).ToList());
            }
            foreach (RoomExitType itemRoomExitType in palaceRooms.ItemRooms(RoomGroup.V5_0).Select(i => i.CategorizeExits()).Distinct())
            {
                ItemRoomsByShape.AddRange(itemRoomExitType, palaceRooms.ItemRoomsByShape(RoomGroup.V5_0, itemRoomExitType));
            }
        }
        else
        {
            //If we are using these categorized exits to cap paths, there needs to always be a path of each type
            //Since vanilla and 4.0 don't normally contain up/down elevator deadends, we add some dummy ones
            DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_DOWN, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).Where(i => i.HasDownExit).First());
            DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_UP, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).Where(i => i.HasUpExit).First());
        }

        List<(RoomExitType, Room)> linkedRoomShapes = [];
        foreach(KeyValuePair<RoomExitType, HashSet<Room>> entry in ItemRoomsByShape)
        {
            foreach(Room room in entry.Value)
            {
                if(room.LinkedRoomName != null)
                {
                    RoomExitType newShape = room.CategorizeExits().Merge(LinkedRooms[room.LinkedRoomName].CategorizeExits());
                    linkedRoomShapes.Add((newShape, room));
                }
            }
        }
        foreach((RoomExitType, Room) linkedRoom in linkedRoomShapes)
        {
            ItemRoomsByShape[linkedRoom.Item2.CategorizeExits()].Remove(linkedRoom.Item2);
            ItemRoomsByShape.Add(linkedRoom.Item1, linkedRoom.Item2);
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

        RequirementType[] allowedBlockers = !props.BlockersAnywhere ? Palaces.ALLOWED_BLOCKERS_BY_PALACE[palaceNumber - 1] : Palaces.ALL_PALACE_ALLOWED_BLOCKERS;
        if(!props.ReplaceFireWithDash)
        {
            allowedBlockers = allowedBlockers.Where(r => r != RequirementType.DASH).ToArray();
        }
        if (props.RemoveItems.Contains(Collectable.FAIRY_SPELL))
        {
            allowedBlockers = allowedBlockers.Where(r => r != RequirementType.FAIRY).ToArray();
        }
        FilterRooms(room => room.IsTraversable(allowedBlockers));

        if (!props.IncludeDropRooms)
        {
            FilterRooms(room => !room.HasDrop);
        }
        if (!props.IncludeLongDeadEnds)
        {
            FilterRooms(room => room.Tags == null || !room.Tags.Contains("LongDeadEnd"));
        }
    }

    /// Remove rooms from the pool that does not satisfy `predicate`
    private void FilterRooms(Func<Room, bool> predicate)
    {
        NormalRooms.RemoveAll(room => !predicate(room) ||
            (room.LinkedRoomName != null && !predicate(LinkedRooms[room.LinkedRoomName])));

        foreach (var key in ItemRoomsByDirection.Keys.ToList())
        {
            var values = ItemRoomsByDirection[key];
            var toRemove = values
                .Where(room => !predicate(room)
                    || (room.LinkedRoomName != null && !predicate(LinkedRooms[room.LinkedRoomName])))
                .ToList();
            foreach (var room in toRemove)
            {
                ItemRoomsByDirection.Remove(key, room);
            }
        }
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
