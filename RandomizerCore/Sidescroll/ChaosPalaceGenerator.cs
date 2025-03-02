using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomizerCore.Sidescroll;
internal class ChaosPalaceGenerator : PalaceGenerator
{
    protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const int CONNECTION_ATTEMPT_LIMIT = 200;

    internal override Palace GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        RoomPool roomPool = new(rooms);
        Palace palace = new(palaceNumber);
        int palaceGroup = palaceNumber switch
        {
            1 => 1,
            2 => 1,
            3 => 2,
            4 => 2,
            5 => 1,
            6 => 2,
            7 => 3,
            _ => throw new ImpossibleException("Invalid palace number: " + palaceNumber)
        };

        palace.Entrance = new(roomPool.Entrances[r.Next(roomPool.Entrances.Count)])
        {
            IsRoot = true,
            PalaceGroup = palaceGroup
        };
        palace.AllRooms.Add(palace.Entrance);

        palace.BossRoom = new(roomPool.BossRooms[r.Next(roomPool.BossRooms.Count)]);
        palace.BossRoom.Enemies = (byte[])roomPool.VanillaBossRoom.Enemies.Clone();
        palace.BossRoom.NewEnemies = palace.BossRoom.Enemies;
        palace.BossRoom.PalaceGroup = palaceGroup;
        palace.AllRooms.Add(palace.BossRoom);

        if (props.BossRoomConnect)
        {
            palace.BossRoom.HasRightExit = true;
            palace.BossRoom.ReplaceExitStatueWithCurtains();
        }

        if (palaceNumber < 7)
        {
            Direction itemRoomDirection;
            Room? itemRoom = null;
            while (itemRoom == null)
            {
                itemRoomDirection = DirectionExtensions.RandomItemRoomOrientation(r);
                if (!roomPool.ItemRoomsByDirection.ContainsKey(itemRoomDirection))
                {
                    continue;
                }
                itemRoom = new(roomPool.ItemRoomsByDirection[itemRoomDirection].ElementAt(r.Next(roomPool.ItemRoomsByDirection[itemRoomDirection].Count)));
            }
            palace.ItemRoom = itemRoom;
            palace.ItemRoom.PalaceGroup = palaceGroup;
            palace.AllRooms.Add(palace.ItemRoom);
        }

        if (palaceNumber < 7 && palace.ItemRoom!.LinkedRoomName != null)
        {
            Room segmentedItemRoom1, segmentedItemRoom2;
            segmentedItemRoom1 = palace.ItemRoom;
            segmentedItemRoom2 = new(roomPool.LinkedRooms[segmentedItemRoom1.LinkedRoomName]);
            segmentedItemRoom2.PalaceGroup = palaceGroup;
            segmentedItemRoom2.LinkedRoom = segmentedItemRoom1;
            segmentedItemRoom1.LinkedRoom = segmentedItemRoom2;
            palace.AllRooms.Add(segmentedItemRoom2);
            roomCount += 1;
        }

        if(palaceNumber == 7)
        {
            if (!props.RemoveTbird)
            {
                palace.TbirdRoom = new(roomPool.TbirdRooms[r.Next(roomPool.TbirdRooms.Count)]);
                palace.TbirdRoom.PalaceGroup = 3;
                palace.AllRooms.Add(palace.TbirdRoom);
            }
        }

        while (palace.AllRooms.Count < roomCount)
        {
            int roomIndex = r.Next(roomPool.NormalRooms.Count);
            Room newRoom = new(roomPool.NormalRooms[roomIndex]);
            palace.AllRooms.Add(newRoom);
            if (props.NoDuplicateRoomsBySideview && AllowDuplicatePrevention(props, palaceNumber))
            {
                if (palace.AllRooms.Any(i => byteArrayEqualityComparer.Equals(i.SideView, newRoom.SideView)))
                {
                    continue;
                }
            }
            if (props.NoDuplicateRooms && AllowDuplicatePrevention(props, palaceNumber))
            {
                roomPool.NormalRooms.RemoveAt(roomIndex);
            }
        }

        Dictionary<Room, RoomExitType> roomExits = [];
        foreach (Room room in palace.AllRooms)
        {
            roomExits.Add(room, room.CategorizeExits());
        }
        List<Room> canEnterGoingUp = roomExits.Where(i => i.Value.ContainsDown() && i.Key.ElevatorScreen >= 0).Select(i => i.Key).ToList();
        List<Room> canEnterGoingDown = roomExits.Where(i => i.Value.ContainsUp()).Select(i => i.Key).ToList();
        List<Room> canEnterGoingLeft = roomExits.Where(i => i.Value.ContainsRight()).Select(i => i.Key).ToList();
        List<Room> canEnterGoingRight = roomExits.Where(i => i.Value.ContainsLeft()).Select(i => i.Key).ToList();
        List<Room> canDropInto = roomExits.Where(i => i.Key.IsDropZone).Select(i => i.Key).ToList();

        //int connectionAttempt = 0;
        foreach (Room room in palace.AllRooms)
        {
            if (room.HasLeftExit)
            {
                room.Left = canEnterGoingLeft.Sample(r);
            }
            if (room.HasRightExit)
            {
                room.Right = canEnterGoingRight.Sample(r);
            }
            if (room.HasUpExit)
            {
                room.Up = canEnterGoingUp.Sample(r);
            }
            if (room.HasDownExit)
            {
                if (room.HasDrop)
                {
                    room.Down = canDropInto.Sample(r);
                }
                else
                {
                    room.Down = canEnterGoingDown.Sample(r);
                }
            }
        }
        List<Room> reachableRooms = palace.GetReachableRooms().ToList();
        List<Room> unreachableRooms = palace.AllRooms.Except(reachableRooms).ToList();
        int connectionCount = 0;
        while(unreachableRooms.Count > 0 && connectionCount++ < CONNECTION_ATTEMPT_LIMIT)
        {
            unreachableRooms.FisherYatesShuffle(r);
            foreach(Room unreachableRoom in unreachableRooms)
            {
                Room reachableRoom = reachableRooms.Sample(r) ?? throw new Exception("No reachable rooms remain)");
                reachableRoom.ConnectRandomly(unreachableRoom, r);
            }
            reachableRooms = palace.GetReachableRooms().ToList();
            unreachableRooms = palace.AllRooms.Except(reachableRooms).ToList();
        };

        palace.IsValid = palace.AllReachable(true);
        return palace;
    }


}
