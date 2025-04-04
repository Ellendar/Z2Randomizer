using System.Collections.Generic;
using System.Linq;
using SD.Tools.Algorithmia.GeneralDataStructures;

namespace RandomizerCore.Sidescroll;

public class RoomPool
{
    public List<Room> NormalRooms { get; set; } = [];
    public List<Room> Entrances { get; set; } = [];
    public List<Room> BossRooms { get; set; } = [];
    public List<Room> TbirdRooms { get; set; } = [];
    public Room VanillaBossRoom { get; set; }
    public Dictionary<string, Room> LinkedRooms { get; } = [];
    public MultiValueDictionary<Direction, Room> ItemRoomsByDirection { get; set; } = [];
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
            || (palaceNumber == 7 && !props.AllowVanillaRooms && !props.AllowV4Rooms && props.AllowV4_4Rooms))
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
        }

        if (props.AllowV4_4Rooms)
        {
            Entrances.AddRange(palaceRooms.Entrances(RoomGroup.V4_4)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            BossRooms.AddRange(palaceRooms.BossRooms(RoomGroup.V4_4)
                .Where(i => (i.PalaceNumber == null && palaceNumber < 6) || i.PalaceNumber == palaceNumber).ToList());
            TbirdRooms.AddRange(palaceRooms.ThunderBirdRooms(RoomGroup.V4_4)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            foreach (var room in palaceRooms.LinkedRooms(RoomGroup.V4_4))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (var direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
            {
                ItemRoomsByDirection.AddRange(direction, palaceRooms.ItemRoomsByDirection(RoomGroup.V4_4, direction).ToList());
            }
        }
        else
        {
            //If we are using these categorized exits to cap paths, there needs to always be a path of each type
            //Since vanilla and 4.0 don't normally contain up/down elevator deadends, we add some dummy ones
            DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_DOWN, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).Where(i => i.HasDownExit).First());
            DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_UP, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).Where(i => i.HasUpExit).First());
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
            || (!props.AllowVanillaRooms && !props.AllowV4Rooms && props.AllowV4_4Rooms))
            {
                NormalRooms.AddRange(palaceRooms.GpRoomsByGroup(RoomGroup.VANILLA));
            }

            if (props.AllowV4Rooms)
            {
                NormalRooms.AddRange(palaceRooms.GpRoomsByGroup(RoomGroup.V4_0));
            }

            if (props.AllowV4_4Rooms)
            {
                NormalRooms.AddRange(palaceRooms.GpRoomsByGroup(RoomGroup.V4_4));
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

            if (props.AllowV4_4Rooms)
            {
                NormalRooms.AddRange(palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.V4_4));
            }
        }
    }

    public Dictionary<RoomExitType, List<Room>> CategorizeNormalRoomExits(bool linkRooms = false)
    {
        Dictionary<RoomExitType, List<Room>> categorizedRooms = [];
        foreach(Room room in NormalRooms)
        {
            RoomExitType type = room.CategorizeExits();
            if(room.LinkedRoomName != null)
            {
                type = type.Merge(LinkedRooms[room.LinkedRoomName].CategorizeExits());
            }
            if(!categorizedRooms.TryGetValue(type, out List<Room>? value))
            {
                value = ([]);
                categorizedRooms[type] = value;
            }

            value.Add(room);
        }
        
        return categorizedRooms;
    }

}

