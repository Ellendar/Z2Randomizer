using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;
internal class ChaosPalaceGenerator : PalaceGenerator
{
    protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const int CONNECTION_ATTEMPT_LIMIT = 200;
    private static int debug = 0;

    internal override async Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        debug++;
        RoomPool roomPool = new(rooms);
        Palace palace = new(palaceNumber);
        var palaceGroup = Util.AsPalaceGrouping(palaceNumber);

        palace.Entrance = new(roomPool.Entrances[r.Next(roomPool.Entrances.Count)])
        {
            IsRoot = true,
            // PalaceGroup = palaceGroup,
        };
        if (palaceNumber != 7) { palace.Entrance.AdjustEntrance(props.PalaceItemRoomCounts[palaceNumber - 1], r); }
        palace.AllRooms.Add(palace.Entrance);

        palace.BossRoom = new(roomPool.BossRooms[r.Next(roomPool.BossRooms.Count)]);
        palace.BossRoom.Enemies = (byte[])roomPool.VanillaBossRoom.Enemies.Clone();
        palace.BossRoom.NewEnemies = palace.BossRoom.Enemies;
        // palace.BossRoom.PalaceGroup = palaceGroup;
        palace.AllRooms.Add(palace.BossRoom);

        if (palace.Number < 7 && props.BossRoomsExits[palace.Number - 1] == BossRoomsExitType.PALACE)
        {
            palace.BossRoom.HasRightExit = true;
            palace.BossRoom.AdjustContinuingBossRoom();
        }

        if (palaceNumber < 7)
        {
            palace.ItemRooms = [];
            for(int itemRoomNumber = 0; itemRoomNumber < props.PalaceItemRoomCounts[palaceNumber - 1]; itemRoomNumber++)
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
                palace.ItemRooms.Add(itemRoom);
                palace.AllRooms.Add(itemRoom);

                if (itemRoom.LinkedRoomName != null)
                {
                    Room segmentedItemRoom1, segmentedItemRoom2;
                    segmentedItemRoom1 = itemRoom;
                    segmentedItemRoom2 = new(roomPool.LinkedRooms[segmentedItemRoom1.LinkedRoomName]);
                    // segmentedItemRoom2.PalaceGroup = palaceGroup;
                    segmentedItemRoom2.LinkedRoom = segmentedItemRoom1;
                    segmentedItemRoom1.LinkedRoom = segmentedItemRoom2;
                    palace.AllRooms.Add(segmentedItemRoom2);
                    roomCount += 1;
                }
            }
        }

        if(palaceNumber == 7)
        {
            if (!props.RemoveTbird)
            {
                palace.TbirdRoom = new(roomPool.TbirdRooms[r.Next(roomPool.TbirdRooms.Count)]);
                // palace.TbirdRoom.PalaceGroup = PalaceGrouping.PalaceGp;
                palace.AllRooms.Add(palace.TbirdRoom);
            }
        }

        while (palace.AllRooms.Count < roomCount && roomPool.NormalRooms.Count > 0)
        {
            await Task.Yield();
            int roomIndex = r.Next(roomPool.NormalRooms.Count);
            Room newRoom = new(roomPool.NormalRooms[roomIndex]);
            if (props.NoDuplicateRoomsBySideview && AllowDuplicatePrevention(props, palaceNumber))
            {
                if (palace.AllRooms.Any(i => byteArrayEqualityComparer.Equals(i.SideView, newRoom.SideView)))
                {
                    roomPool.NormalRooms.RemoveAt(roomIndex);
                    continue;
                }
            }
            palace.AllRooms.Add(newRoom);
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
                if (room.Left == null)
                {
                    palace.IsValid = false;
                    return palace;
                }
            }
            if (room.HasRightExit)
            {
                room.Right = canEnterGoingRight.Sample(r);
                if (room.Right == null)
                {
                    palace.IsValid = false;
                    return palace;
                }
            }
            if (room.HasUpExit)
            {
                room.Up = canEnterGoingUp.Sample(r);
                if (room.Up == null)
                {
                    palace.IsValid = false;
                    return palace;
                }
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
                if (room.Down == null)
                {
                    palace.IsValid = false;
                    return palace;
                }
            }
        }
        List<Room> reachableRooms = palace.GetReachableRooms().ToList();
        List<Room> unreachableRooms = palace.AllRooms.Except(reachableRooms).ToList();
        int connectionCount = 0;
        while(unreachableRooms.Count > 0 && connectionCount++ < CONNECTION_ATTEMPT_LIMIT)
        {
            await Task.Yield();
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
