using SD.Tools.Algorithmia.GeneralDataStructures;
using SD.Tools.BCLExtensions.CollectionsRelated;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.Core.Sidescroll;

internal class RoomPool
{
    public List<Room> NormalRooms { get; set; } = [];
    public List<Room> Entrances { get; set; } = [];
    public List<Room> BossRooms { get; set; } = [];
    public List<Room> TbirdRooms { get; set; } = [];
    public Room VanillaBossRoom { get; set; }
    public Dictionary<string, Room> LinkedRooms { get; } = [];
    public MultiValueDictionary<Direction, Room> ItemRoomsByDirection { get; set; } = [];

    protected RoomPool()
    {

    }
    public RoomPool(RoomPool target)
    {
        NormalRooms.AddRange(target.NormalRooms);
        Entrances.AddRange(target.Entrances);
        BossRooms.AddRange(target.BossRooms);
        TbirdRooms.AddRange(target.TbirdRooms);
        VanillaBossRoom = target.VanillaBossRoom;
        foreach (KeyValuePair<string, Room> room in target.LinkedRooms)
        {
            LinkedRooms.Add(room.Key, room.Value);
        }
        foreach (Direction key in target.ItemRoomsByDirection.Keys)
        {
            ItemRoomsByDirection.AddRange(key, target.ItemRoomsByDirection[key]);
        }
    }

    public RoomPool(PalaceRooms palaceRooms, int palaceNumber, RandomizerProperties props)
    {
        if (props.AllowVanillaRooms)
        {
            Entrances.AddRange(palaceRooms.Entrances(RoomGroup.VANILLA)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            BossRooms.AddRange(palaceRooms.BossRooms(RoomGroup.VANILLA)
                .Where(i => (i.PalaceNumber == null && palaceNumber < 6) || i.PalaceNumber == palaceNumber).ToList());
            TbirdRooms.AddRange(palaceRooms.ThunderBirdRooms(RoomGroup.VANILLA)
                .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
            foreach(KeyValuePair<string, Room> room in palaceRooms.LinkedRooms(RoomGroup.VANILLA))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (Direction direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
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
            foreach (KeyValuePair<string, Room> room in palaceRooms.LinkedRooms(RoomGroup.V4_0))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (Direction direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
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
            foreach (KeyValuePair<string, Room> room in palaceRooms.LinkedRooms(RoomGroup.V4_4))
            {
                LinkedRooms.Add(room.Key, room.Value);
            }
            foreach (Direction direction in DirectionExtensions.ITEM_ROOM_ORIENTATIONS)
            {
                ItemRoomsByDirection.AddRange(direction, palaceRooms.ItemRoomsByDirection(RoomGroup.V4_4, direction).ToList());
            }
        }

        //If we're using a room set that has no entraces, we still need to have something, so add the vanilla entrances.
        if (Entrances.Count == 0)
        {
            Entrances.AddRange(palaceRooms.Entrances(RoomGroup.VANILLA).Where(i => i.PalaceNumber == palaceNumber).ToList());
        }

        VanillaBossRoom = palaceRooms.VanillaBossRoom(palaceNumber);

        if (palaceNumber == 7)
        {
            if (props.AllowVanillaRooms)
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

    public Dictionary<RoomEditType, List<Room>> CategorizeNormalRoomExits()
    {

    }

}

