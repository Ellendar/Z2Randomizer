using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;

namespace RandomizerCore.Sidescroll;

public class Palace
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    //private const bool DROPS_ARE_BLOCKERS = false;
    private const byte OUTSIDE_ROOM_EXIT = 0b11111100;
    private readonly SortedDictionary<int, List<Room>> rooms;
    internal bool IsValid { get; set; } = false;

    public static readonly Collectable[] SHUFFLABLE_SMALL_ITEMS = [
        Collectable.KEY,
        Collectable.SMALL_BAG,
        Collectable.MEDIUM_BAG,
        Collectable.LARGE_BAG,
        Collectable.XL_BAG,
        Collectable.BLUE_JAR,
        Collectable.RED_JAR,
        Collectable.ONEUP
    ];

    internal List<Room> AllRooms { get; private set; }

    public Room? Entrance { get; set; }
    public Room? ItemRoom { get; set; }
    public Room? BossRoom { get; set; }
    public int Number { get; set; }
    internal Room? TbirdRoom { get; set; }

    //DEBUG
    public int Generations { get; set; }

    public Palace(int number)
    {
        Number = number;
        Entrance = null;
        rooms = [];
        AllRooms = [];
    }

    public void UpdateSideviewItem(Collectable collectable)
    {
        if(Number == 7)
        {
            return;
        }
        if(ItemRoom == null)
        {
            throw new Exception("Unable to update item for a palace with no item room"); 
        }
        byte[] sideView = ItemRoom.SideView;
        for (int sideviewIndex = 4; sideviewIndex < sideView.Length; sideviewIndex += 2)
        {
            int yPos = sideView[sideviewIndex] & 0xF0;
            yPos >>= 4;
            //item
            if (yPos < 13 && sideView[sideviewIndex + 1] == 0x0F)
            {
                int collectableId = sideView[sideviewIndex + 2];
                if (!((Collectable)collectableId).IsMinorItem())
                {
                    sideView[sideviewIndex + 2] = (byte)collectable;
                }
                sideviewIndex++;
            }
        }
    }

    public void UpdateRomItem(Collectable collectable, ROM ROMData)
    {
        ItemRoom?.UpdateRomItem(collectable, ROMData);
    }

    public bool RequiresThunderbird()
    {
        CheckSpecialPaths(Entrance!);
        return !BossRoom!.IsBeforeTbird;
    }

    public bool HasDeadEnd()
    {

        List<Room> dropExits = AllRooms.Where(i => i.HasDownExit && i.HasDrop).ToList();
        if (dropExits.Count == 0 || dropExits.Any(i => i.Down == null))
        {
            return false;
        }
        Room end = BossRoom!;
        foreach (Room r in dropExits)
        {
            if(r.Down == null)
            {
                continue;
            }
            List<Room> reachable = [];
            List<Room> roomsToCheck = [];
            reachable.Add(r.Down);
            roomsToCheck.Add(r.Down);

            while (roomsToCheck.Count > 0)
            {
                Room c = roomsToCheck[0];
                if (c.Left != null && !reachable.Contains(c.Left))
                {
                    reachable.Add(c.Left);
                    roomsToCheck.Add(c.Left);
                }
                if (c.Right != null && !reachable.Contains(c.Right))
                {
                    reachable.Add(c.Right);
                    roomsToCheck.Add(c.Right);
                }
                if (c.Up != null && !reachable.Contains(c.Up))
                {
                    reachable.Add(c.Up);
                    roomsToCheck.Add(c.Up);
                }
                if (c.Down != null && !reachable.Contains(c.Down))
                {
                    reachable.Add(c.Down);
                    roomsToCheck.Add(c.Down);
                }
                roomsToCheck.Remove(c);
            }
            if (!reachable.Contains(Entrance!) && !reachable.Contains(end))
            {
                return true;
            }
        }
        return false;
    }



    private void CheckSpecialPaths(Room r)
    {
        if (!r.IsBeforeTbird)
        {
            if ((Number == 7) && r.IsThunderBirdRoom)
            {
                r.IsBeforeTbird = true;
                return;
            }

            r.IsBeforeTbird = true;
            if (r.Left != null)
            {
                CheckSpecialPaths(r.Left);
            }

            if (r.Right != null)
            {
                CheckSpecialPaths(r.Right);
            }

            if (r.Up != null)
            {
                CheckSpecialPaths(r.Up);
            }

            if (r.Down != null)
            {
                CheckSpecialPaths(r.Down);
            }
        }
    }

    public IEnumerable<Room> GetReachableRooms(bool allowBacktracking = false)
    {
        if(Entrance == null)
        {
            throw new Exception("Palace Entrance is missing");
        }
        foreach (Room r in AllRooms)
        {
            if (r.HasBoss && CanEnterBossFromLeft(r))
            {
                return [Entrance];
            }
        }
        List<Room> reachedRooms = [];
        List<(Room, Direction)> roomsToCheck = [(Entrance, Direction.WEST)];
        while (roomsToCheck.Count > 0)
        {
            Room room = roomsToCheck.Last().Item1;
            Direction originDirection = roomsToCheck.Last().Item2;
            //For required thunderbird, you can't path backwards into tbird room
            if ((Number == 7 && room.IsThunderBirdRoom) 
                || (Number < 7 && room.IsBossRoom))
            {
                if (originDirection == Direction.EAST)
                {
                    return new Room[] { Entrance };
                }
            }
            roomsToCheck.Remove(roomsToCheck.Last());
            if (reachedRooms.Contains(room))
            {
                continue;
            }
            reachedRooms.Add(room);
            if (room.Left != null && (originDirection != Direction.WEST || allowBacktracking))
            {
                roomsToCheck.Add((room.Left, Direction.EAST));
            }

            if (room.Right != null && (originDirection != Direction.EAST || allowBacktracking))
            {
                roomsToCheck.Add((room.Right, Direction.WEST));
            }

            if (room.Up != null && (originDirection != Direction.NORTH || allowBacktracking))
            {
                roomsToCheck.Add((room.Up, Direction.SOUTH));
            }

            if (room.Down != null && (originDirection != Direction.SOUTH || allowBacktracking))
            {
                roomsToCheck.Add((room.Down, Direction.NORTH));
            }
        }
        return reachedRooms;
    }

    public bool AllReachable(bool allowBacktracking = false)
    {
        return !AllRooms.Any(i => !GetReachableRooms(allowBacktracking).Contains(i));
    }

    private bool CanEnterBossFromLeft(Room b)
    {
        List<Room> reachable = [];
        List<Room> roomsToCheck = [];

        Room Entrance = AllRooms.First(i => i.IsEntrance);

        if(Entrance.Down != null)
        {
            reachable.Add(Entrance.Down);
            roomsToCheck.Add(Entrance.Down);
        }

        if (Entrance.Up != null)
        {
            reachable.Add(Entrance.Up);
            roomsToCheck.Add(Entrance.Up);
        }

        if (Entrance.Right != null)
        {
            reachable.Add(Entrance.Right);
            roomsToCheck.Add(Entrance.Right);
        }

        while (roomsToCheck.Count > 0)
        {
            Room c = roomsToCheck[0];
            if (c.Left != null && c.Left == b)
            {
                return true;
            }
            if (c.Left != null && !reachable.Contains(c.Left))
            {
                reachable.Add(c.Left);
                roomsToCheck.Add(c.Left);
            }
            if (c.Right != null && !reachable.Contains(c.Right) && c.Right != b)
            {
                reachable.Add(c.Right);
                roomsToCheck.Add(c.Right);
            }
            if (c.Up != null && !reachable.Contains(c.Up) && c.Up != b)
            {
                reachable.Add(c.Up);
                roomsToCheck.Add(c.Up);
            }
            if (c.Down != null && !reachable.Contains(c.Down) && c.Down != b)
            {
                reachable.Add(c.Down);
                roomsToCheck.Add(c.Down);
            }
            roomsToCheck.Remove(c);
        }
        return false;
    }
    public void ShuffleRooms(Random r)
    {
        List<Room> roomsWithUpExits = AllRooms.Where(i => i.HasUpExit).ToList();
        List<Room> roomsWithDownExits = AllRooms.Where(i => i.HasDownExit && !i.HasDrop).ToList();
        List<Room> roomsWithLeftExits = AllRooms.Where(i => i.HasLeftExit).ToList();
        List<Room> roomsWithRightExits = AllRooms.Where(i => i.HasRightExit).ToList();
        List<Room> roomsWithDropExits = AllRooms.Where(i => i.HasDrop).ToList();

        if (AllRooms.Any(i => !i.ValidateExits()))
        {
            throw new Exception("Invalid room connections while shuffling");
        }
        for (int i = 0; i < roomsWithUpExits.Count; i++)
        {
            Room selectedRoom = roomsWithUpExits[i];
            Room swapRoom = roomsWithUpExits.Sample(r)!;
            selectedRoom.Up!.Down = swapRoom;
            swapRoom.Up!.Down = selectedRoom;

            (selectedRoom.Up, swapRoom.Up) = (swapRoom.Up, selectedRoom.Up);
            if (AllRooms.Any(i => !i.ValidateExits()))
            {
                throw new Exception("Invalid room connections while shuffling");
            }
        }
        for (int i = 0; i < roomsWithDropExits.Count; i++)
        {
            int swap = r.Next(i, roomsWithDropExits.Count);

            Room temp = roomsWithDropExits[i].Down!;

            roomsWithDropExits[i].Down = roomsWithDropExits[swap].Down;
            roomsWithDropExits[swap].Down = temp;
            if (AllRooms.Any(i => !i.ValidateExits()))
            {
                throw new Exception("Invalid room connections while shuffling");
            }
        }

        for (int i = 0; i < roomsWithDownExits.Count; i++)
        {
            Room selectedRoom = roomsWithDownExits[i];
            Room swapRoom = roomsWithDownExits.Sample(r)!;

            selectedRoom.Down!.Up = swapRoom;
            swapRoom.Down!.Up = selectedRoom;

            (selectedRoom.Down, swapRoom.Down) = (swapRoom.Down, selectedRoom.Down);
            if (AllRooms.Any(i => !i.ValidateExits()))
            {
                throw new Exception("Invalid room connections while shuffling");
            }
        }

        for (int i = 0; i < roomsWithLeftExits.Count; i++)
        {

            Room selectedRoom = roomsWithLeftExits[i];
            Room swapRoom = roomsWithLeftExits.Sample(r)!;
            selectedRoom.Left!.Right = swapRoom;
            swapRoom.Left!.Right = selectedRoom;

            (selectedRoom.Left, swapRoom.Left) = (swapRoom.Left, selectedRoom.Left);
            if (AllRooms.Any(i => !i.ValidateExits()))
            {
                throw new Exception("Invalid room connections while shuffling");
            }
        }

        for (int i = 0; i < roomsWithRightExits.Count; i++)
        {
            Room selectedRoom = roomsWithRightExits[i];
            Room swapRoom = roomsWithRightExits.Sample(r)!;
            selectedRoom.Right!.Left = swapRoom;
            swapRoom.Right!.Left = selectedRoom;

            (selectedRoom.Right, swapRoom.Right) = (swapRoom.Right, selectedRoom.Right);
            if (AllRooms.Any(i => !i.ValidateExits()))
            {
                throw new Exception("Invalid room connections while shuffling");
            }
        }
    }

    public void WriteConnections(ROM ROMData)
    {
        List<Room> roomsToRemove = [];
        //Unify the parts of segmented rooms back together
        foreach (Room room in AllRooms.ToList())
        {
            //If this is the primary room of a linked room pair
            if (room.LinkedRoomName != null && room.Enabled && room.LinkedRoom != null)
            {
                Room linkedRoom = room.LinkedRoom;
                if(room.IsUpDownReversed != linkedRoom.IsUpDownReversed)
                {
                    throw new Exception("Inconsistent IsUpDownReversed in linked rooms");
                }
                //set each blank exit on the master room that has a counterpart in the linked room
                if (linkedRoom.HasLeftExit && room.Left == null && linkedRoom.Left != null)
                {
                    room.Left = linkedRoom.Left;
                }
                if (linkedRoom.HasRightExit && room.Right == null && linkedRoom.Right != null)
                {
                    room.Right = linkedRoom.Right;
                }
                if (linkedRoom.HasUpExit && room.Up == null && linkedRoom.Up != null)
                {
                    room.Up = linkedRoom.Up;
                }
                if (linkedRoom.HasDownExit && room.Down == null && linkedRoom.Down != null)
                {
                    room.Down = linkedRoom.Down;
                }

                //set each room that links to the secondary room to point to the master room instead
                AllRooms.Where(i => i.Left == linkedRoom).ToList().ForEach(i => i.Left = room);
                AllRooms.Where(i => i.Right == linkedRoom).ToList().ForEach(i => i.Right = room);
                AllRooms.Where(i => i.Up == linkedRoom).ToList().ForEach(i => i.Up = room);
                AllRooms.Where(i => i.Down == linkedRoom).ToList().ForEach(i => i.Down = room);

                //remove the linked room from the rooms pool
                roomsToRemove.Add(linkedRoom);
            }
        }
        AllRooms = AllRooms.Except(roomsToRemove).ToList();

        foreach (Room room in AllRooms)
        {
            byte leftByte = (byte)(room.Left == null ? OUTSIDE_ROOM_EXIT : (room.Left.Map * 4 + 3));
            byte downByte = (byte)(room.Down == null ? OUTSIDE_ROOM_EXIT : (room.Down.Map * 4));
            byte upByte = (byte)(room.Up == null ? OUTSIDE_ROOM_EXIT : (room.Up.Map * 4));
            byte rightByte = (byte)(room.Right == null ? OUTSIDE_ROOM_EXIT : (room.Right.Map * 4));
            int pageCount = ((room.SideView[1] & 0b01100000) >> 5) + 1;

            //if the up/down exit is an elevator type, go to the page the elevator is on on that screen
            if (room.Down != null && room.Down.ElevatorScreen >= 0)
            {
                downByte = (byte)(downByte + room.Down.ElevatorScreen);
            }
            if (room.Up != null && room.Up.ElevatorScreen >= 0)
            {
                upByte = (byte)(upByte + room.Up.ElevatorScreen);
            }

            if (room.IsUpDownReversed)
            {
                (downByte, upByte) = (upByte, downByte);
            }

            //if the room on the left is a 2/3 page room, make the left exit go to the rightmost page in that room
            if (room.Left != null)
            {
                int leftPageCount = ((room.Left.SideView[1] & 0b01100000) >> 5) + 1;
                if (leftPageCount < 4)
                {
                    leftByte -= (byte)(4 - leftPageCount);
                }
            }

            if (pageCount <= 1)
            {
                throw new Exception("Palace rooms cannot have fewer than 2 pages");
            }
            if (pageCount > 4)
            {
                throw new Exception("Palace rooms cannot have more than 4 pages");
            }

            //If a room is fewer than 4 pages, "right" doesn't actually work.
            //Exits in Z2 point you not based on how you exit, but what page you exit from.
            //Because of that the right exit from a 2 page map is actually the down connection
            //this standardizes the exits so 2 page rooms are always left / right
            //3 page rooms are always left / down / right
            //and then 4 page rooms are always left / down / up / right
            ROMData.Put(room.ConnectionStartAddress + 0, leftByte);
            if (pageCount == 2)
            {
                ROMData.Put(room.ConnectionStartAddress + 1, rightByte);
                ROMData.Put(room.ConnectionStartAddress + 2, 0xFF);
                ROMData.Put(room.ConnectionStartAddress + 3, 0xFF);
                continue;
            }
            ROMData.Put(room.ConnectionStartAddress + 1, downByte);
            if (pageCount == 3)
            {
                ROMData.Put(room.ConnectionStartAddress + 2, rightByte);
                ROMData.Put(room.ConnectionStartAddress + 3, 0xFF);
                continue;
            }
            ROMData.Put(room.ConnectionStartAddress + 2, upByte);
            ROMData.Put(room.ConnectionStartAddress + 3, rightByte);

            //Debug.WriteLine("Wrote Map " + room.Map + "(" + room.PalaceGroup + ") " +
            //   room.PalaceNumber + " at address " + room.ConnectionStartAddress + " : "
            //    + Util.ByteArrayToHexString([leftByte, downByte, upByte, rightByte]));
        }
    }


    public void CreateTree(bool removeTbird)
    {
        foreach (Room r in AllRooms)
        {
            if (rooms.ContainsKey(r.Map * 4))
            {
                rooms[r.Map * 4].Add(r);
            }
            else
            {
                List<Room> l = new List<Room> { r };
                rooms.Add(r.Map * 4, l);
            }
        }
        foreach (Room r in AllRooms)
        {
            if (r.Left == null && r.HasLeftExit)
            {
                List<Room> l = rooms[r.Connections[0] & 0xFC];
                foreach (Room r2 in l)
                {
                    if ((r2.Connections[3] & 0xFC) / 4 == r.Map)
                    {
                        r.Left = r2;
                    }
                }
            }

            if (r.Right == null && r.HasRightExit)
            {
                List<Room> l = rooms[r.Connections[3] & 0xFC];
                foreach (Room r2 in l)
                {
                    if ((r2.Connections[0] & 0xFC) / 4 == r.Map)
                    {
                        r.Right = r2;
                    }
                }
            }

            if (r.Up == null && r.HasUpExit)
            {
                List<Room> l = rooms[(r.IsUpDownReversed ? r.Connections[1] : r.Connections[2]) & 0xFC];
                foreach (Room r2 in l)
                {
                    if (((r2.IsUpDownReversed ? r2.Connections[2] : r2.Connections[1]) & 0xFC) / 4 == r.Map)
                    {
                        r.Up = r2;
                    }
                    else if (r2.Map == ((r.IsUpDownReversed ? r.Connections[1] : r.Connections[2]) & 0xFC) / 4 && r2.IsDropZone) {
                        r.Up = r2;
                    }
                }
            }

            if (r.Down == null && r.HasDownExit)
            {
                List<Room> l = rooms[(r.IsUpDownReversed ? r.Connections[2] : r.Connections[1]) & 0xFC];
                foreach (Room r2 in l)
                {
                    if (((r2.IsUpDownReversed ? r2.Connections[1] : r2.Connections[2]) & 0xFC) / 4 == r.Map)
                    {
                        r.Down = r2;
                    }
                    else if (r2.Map == ((r.IsUpDownReversed ? r.Connections[2] : r.Connections[1]) & 0xFC) / 4 && r2.IsDropZone)
                    {
                        r.Down = r2;
                    }
                }
            }
            if (((r.IsUpDownReversed ? r.Connections[1] : r.Connections[2]) & 0xFC) == 0 
                && ((Entrance!.IsUpDownReversed ? Entrance.Connections[2] : Entrance.Connections[1]) & 0xFC) / 4 == r.Map)
            {
                r.Up = Entrance;
            }
        }
        if (removeTbird)
        {
            if(TbirdRoom!.Left == null || TbirdRoom.Right == null)
            {
                throw new Exception("Invalid vanilla tbird room");
            }
            TbirdRoom.Left.Connections[3] = TbirdRoom.Connections[3];
            TbirdRoom.Right.Connections[0] = TbirdRoom.Connections[0];
            TbirdRoom.Left.Right = TbirdRoom.Right;
            TbirdRoom.Right.Left = TbirdRoom.Left;
            //leftExits.Remove(Tbird);
            //rightExits.Remove(Tbird);
            AllRooms.Remove(TbirdRoom);
        }
    }

    public void Shorten(Random random)
    {
        List<Room> upExits = AllRooms.Where(i => i.HasUpExit).ToList();
        List<Room> downExits = AllRooms.Where(i => i.HasDownExit && !i.HasDrop).ToList();
        List<Room> leftExits = AllRooms.Where(i => i.HasLeftExit).ToList();
        List<Room> rightExits = AllRooms.Where(i => i.HasRightExit).ToList();
        List<Room> dropExits = AllRooms.Where(i => i.HasDownExit && i.HasDrop).ToList();
        int numRooms = AllRooms.Count;

        int target = random.Next(numRooms / 2, (numRooms * 3) / 4) + 1;
        int rooms = numRooms;
        int tries = 0;
        while (rooms > target && tries < 100000)
        {
            //remove rooms without bias
            //don't remove important rooms
            Room remove = AllRooms.Where(i => !i.IsEntrance 
                && !i.HasItem 
                && !i.IsBossRoom 
                && !i.IsThunderBirdRoom
                && !i.HasDrop).ToArray().Sample(random)!;

            bool hasRight = remove.Right != null;
            bool hasLeft = remove.Left != null;
            bool hasUp = remove.Up != null;
            bool hasDown = remove.Down != null;

            bool[] exits = [hasRight, hasLeft, hasUp, hasDown];
            int exitCount = exits.Count(i => i);

            //logger.WriteLine(n);

            if (exitCount != 2)
            {
                tries++;
                continue;
            }

            if (hasLeft && hasRight && !dropExits.Any(i => i.Down == remove))
            {
                remove.Left!.Right = remove.Right;
                remove.Right!.Left = remove.Left;
                rooms--;
                //logger.WriteLine("removed 1 room");
                leftExits.Remove(remove);
                rightExits.Remove(remove);
                AllRooms.Remove(remove);
                tries = 0;
                continue;
            }

            if (hasUp && hasDown)
            {
                remove.Up!.Down = remove.Down;
                remove.Down!.Up = remove.Up;
                //logger.WriteLine("removed 1 room");
                rooms--;
                upExits.Remove(remove);
                downExits.Remove(remove);
                AllRooms.Remove(remove);
                tries = 0;
                continue;
            }

            if (hasDown)
            {
                if (hasLeft)
                {
                    exits = [hasLeft, hasUp, hasDown];
                    exitCount = exits.Count(i => i);

                    if (exitCount != 2)
                    {
                        tries++;
                        continue;
                    }

                    if (remove.Left!.Up == null || remove.Left.Up != Entrance)
                    {
                        tries++;
                        continue;
                    }

                    remove.Left.Up.Down = remove.Down;
                    remove.Down!.Up = remove.Left.Up;

                    downExits.Remove(remove);
                    leftExits.Remove(remove);
                    rightExits.Remove(remove.Left);
                    upExits.Remove(remove.Left);
                    AllRooms.Remove(remove);
                    AllRooms.Remove(remove.Left);
                    //logger.WriteLine("removed 2 room");
                    rooms = rooms - 2;
                    tries = 0;
                    continue;
                }
                else
                {
                    exits = [hasRight, hasUp, hasDown];
                    exitCount = exits.Count(i => i);

                    if (exitCount != 2)
                    {
                        tries++;
                        continue;
                    }

                    if (remove.Right!.Up == null || remove.Right.Up == Entrance)
                    {
                        tries++;
                        continue;
                    }

                    remove.Right.Up.Down = remove.Down;
                    remove.Down!.Up = remove.Right.Up;

                    downExits.Remove(remove);
                    rightExits.Remove(remove);
                    leftExits.Remove(remove.Right);
                    upExits.Remove(remove.Right);
                    AllRooms.Remove(remove);
                    AllRooms.Remove(remove.Right);
                    //logger.WriteLine("removed 2 room");

                    rooms = rooms - 2;
                    tries = 0;
                    continue;
                }
            }
            else
            {
                if (hasLeft)
                {
                    exits = [hasLeft, hasUp, hasDown];
                    exitCount = exits.Count(i => i);

                    if (exitCount != 2)
                    {
                        tries++;
                        continue;
                    }

                    if (remove.Left!.Down == null || dropExits.Contains(remove.Left))
                    {
                        tries++;
                        continue;
                    }

                    remove.Left.Down.Up = remove.Up;
                    remove.Up!.Down = remove.Left.Down;

                    upExits.Remove(remove);
                    leftExits.Remove(remove);
                    rightExits.Remove(remove.Left);
                    downExits.Remove(remove.Left);
                    AllRooms.Remove(remove);
                    AllRooms.Remove(remove.Left);
                    //logger.WriteLine("removed 2 room");

                    rooms = rooms - 2;
                    tries = 0;
                    continue;
                }
                else
                {
                    exits = [hasRight, hasUp, hasDown];
                    exitCount = exits.Count(i => i);

                    if (exitCount != 2)
                    {
                        tries++;
                        continue;
                    }

                    if (remove.Right!.Down == null || dropExits.Contains(remove.Right))
                    {
                        tries++;
                        continue;
                    }

                    remove.Right.Down.Up = remove.Up;
                    remove.Up!.Down = remove.Right.Down;

                    upExits.Remove(remove);
                    rightExits.Remove(remove);
                    leftExits.Remove(remove.Right);
                    downExits.Remove(remove.Right);
                    AllRooms.Remove(remove);
                    AllRooms.Remove(remove.Right);
                    //logger.WriteLine("removed 2 room");

                    rooms = rooms - 2;
                    tries = 0;
                    continue;
                }
            }
        }
        logger.Debug("Target: " + target + " Rooms: " + rooms);
    }

    public void RandomizeSmallItems(Random r, bool extraKeys)
    {
        foreach (Room room in AllRooms)
        {
            try {
                int sideviewIndex = 4; //Header bytes
                while (sideviewIndex < room.SideView.Length)
                {
                    int firstByte = room.SideView[sideviewIndex++];
                    int secondByte = room.SideView[sideviewIndex++];
                    int ypos = (firstByte & 0xF0) >> 4;
                    if (secondByte == 15 && ypos < 13)
                    {
                        int thirdByte = room.SideView[sideviewIndex++];
                        if (!SHUFFLABLE_SMALL_ITEMS.Contains((Collectable)thirdByte))
                        {
                            continue;
                        }
                        double d = r.NextDouble();
                        if (room.PalaceNumber == 7)
                        {
                            if (d <= 1)
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.ONEUP;
                            }
                        }
                        else
                        {
                            if (extraKeys)
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.KEY;
                            }
                            //35 Key
                            //10 BlueJar
                            //10 RedJar
                            //10 Small bag
                            //15 Medium Bag
                            //10 Large Bag
                            //5 XL bag
                            //5 1Up
                            else if (d <= .35)
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.KEY;
                            }
                            else if (d <= .45)
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.BLUE_JAR;
                            }
                            else if (d <= .55)
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.RED_JAR;
                            }
                            else if (d <= .65)
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.SMALL_BAG;
                            }
                            else if (d <= .80)
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.MEDIUM_BAG;
                            }
                            else if (d <= .90)
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.LARGE_BAG;
                            }
                            else if (d <= .95)
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.XL_BAG;
                            }
                            else
                            {
                                room.SideView[sideviewIndex - 1] = (int)Collectable.ONEUP;
                            }
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                logger.Error("Failed to parse sideview: " + Util.ByteArrayToHexString(room.SideView));
            }
        }
    }

    public List<Room> CheckBlocks()
    {
        return CheckBlocksHelper([], [], Entrance!);
    }

    private List<Room> CheckBlocksHelper(List<Room> c, List<Room> blockers, Room r)
    {
        if (c.Contains(ItemRoom!))
        {
            return c;
        }
        c.Add(r);
        if (r.Up != null && !c.Contains(r.Up))
        {
            CheckBlocksHelper(c, blockers, r.Up);
        }
        if (r.Down != null && !c.Contains(r.Down))
        {
            CheckBlocksHelper(c, blockers, r.Down);
        }
        if (r.Left != null && !c.Contains(r.Left))
        {
            CheckBlocksHelper(c, blockers, r.Left);
        }
        if (r.Right != null && !c.Contains(r.Right))
        {
            CheckBlocksHelper(c, blockers, r.Right);
        }
        return c;
    }

    public void ResetRooms()
    {
        foreach (Room r in AllRooms)
        {
            r.IsReachable = false;
            r.IsBeforeTbird = false;
        }
    }

    public void RandomizeEnemies(RandomizerProperties props, Random r)
    {
        int count = 0;
        int ENEMY_SHUFFLE_LIMIT = 10;
        foreach(Room room in AllRooms)
        {
            room.RandomizeEnemies(props.MixLargeAndSmallEnemies, props.GeneratorsAlwaysMatch, r);
            if (props.NoDuplicateRooms)
            {
                Room? duplicateRoom = null;
                while(duplicateRoom == null && count++ < ENEMY_SHUFFLE_LIMIT)
                {
                    duplicateRoom = AllRooms.FirstOrDefault(i =>
                        Util.byteArrayEqualityComparer.Equals(room.SideView, i.SideView)
                        && Util.byteArrayEqualityComparer.Equals(room.NewEnemies, i.NewEnemies)
                        && i.Enemies.Length > 1
                        && i != room);
                    if (duplicateRoom != null)
                    {
                        Debug.WriteLine("Room# " + room.ConnectionStartAddress + " (" + Util.ByteArrayToHexString(room.SideView) + "/" + Util.ByteArrayToHexString(room.NewEnemies) + ") " +
                            " is a duplicate of Room# " + duplicateRoom.ConnectionStartAddress + " (" + Util.ByteArrayToHexString(duplicateRoom.SideView) + "/" + Util.ByteArrayToHexString(duplicateRoom.Enemies) + ")");
                        room.RandomizeEnemies(props.MixLargeAndSmallEnemies, props.GeneratorsAlwaysMatch, r);
                    }
                }
                if(count == ENEMY_SHUFFLE_LIMIT)
                {
                    logger.Warn("Room# " + room.ConnectionStartAddress + " (" + Util.ByteArrayToHexString(room.SideView) + "/" + Util.ByteArrayToHexString(room.Enemies) +
                        ") Exceeded the enemy shuffle limit");
                }
            }
        }
    }

    public bool CanClearAllRooms(IEnumerable<RequirementType> requireables, Collectable palaceItem)
    {
        //If the palace's item can be reached with the current items, it can be used to clear the rest of the palace.
        RequirementType? palaceItemRequirement = palaceItem.AsRequirement();
        if(palaceItemRequirement != null)
        {
            if (CanGetItem(requireables))
            {
                requireables = new List<RequirementType>(requireables);
                ((List<RequirementType>)requireables).Add((RequirementType)palaceItemRequirement);
            }
            else return false;
        }

        List<Room> unclearableRooms = AllRooms.Where(i => !i.Requirements.AreSatisfiedBy(requireables)).ToList();
        return unclearableRooms.Count == 0;
    }

    public bool CanGetItem(IEnumerable<RequirementType> requireables)
    {
        List<Room> pendingRooms = new() { AllRooms.First(i => i.IsEntrance) };
        List<Room> coveredRooms = new();

        while (pendingRooms.Count > 0)
        {
            Room room = pendingRooms.First();
            if (room == ItemRoom)
            {
                return true;
            }
            if (coveredRooms.Contains(room))
            {
                pendingRooms.Remove(room);
                continue;
            }
            coveredRooms.Add(room);
            pendingRooms.Remove(room);
            if (room.Left != null && room.Left.IsTraversable(requireables))
            {
                pendingRooms.Add(room.Left);
            }
            if (room.Right != null && room.Right.IsTraversable(requireables))
            {
                pendingRooms.Add(room.Right);
            }
            if (room.Up != null && room.Up.IsTraversable(requireables))
            {
                pendingRooms.Add(room.Up);
            }
            if (room.Down != null && room.Down.IsTraversable(requireables))
            {
                pendingRooms.Add(room.Down);
            }
        }

        return false;
    }

    public int GetPalaceGroup()
    {
        return Number switch
        {
            1 => 1,
            2 => 1,
            3 => 2,
            4 => 2,
            5 => 1,
            6 => 2,
            7 => 3,
            _ => throw new ImpossibleException("Invalid palace number: " + Number)
        };
    }

    public void ValidateRoomConnections()
    {
        foreach(Room room in AllRooms)
        {
            //If this room connects to any rooms that aren't in the same palace,
            //or that it knows are invalid, it's probably a bug
            //since we purged all the extraneous vanilla connection data.
            //Remove it. (But also probably freak out)
            if (room.Left != null && (!AllRooms.Contains(room.Left) || !room.HasLeftExit))
            {
                room.Left = null;
                logger.Warn("Invalid Room connection removed");
            }
            if (room.Right != null && (!AllRooms.Contains(room.Right) || !room.HasRightExit))
            {
                room.Right = null;
                logger.Warn("Invalid Room connection removed");
            }
            if (room.Up != null && (!AllRooms.Contains(room.Up) || !room.HasUpExit))
            {
                room.Up = null;
                logger.Warn("Invalid Room connection removed");
            }
            if (room.Down != null && (!AllRooms.Contains(room.Down) || !room.HasDownExit))
            {
                room.Down = null;
                logger.Warn("Invalid Room connection removed");
            }
        }
    }

    public byte AssignMapNumbers(byte currentMap, bool isGP, bool isVanilla)
    {
        if(isVanilla)
        {
            return AllRooms.Max(i => (byte)(i.Map + 1));
        }
        if (!AllRooms.Contains(Entrance!))
        {
            throw new Exception("Palace lost its entrance");
        }
        if (AllRooms.Any(i => i.IsEntrance && i != Entrance))
        {
            throw new Exception("Palace has an extra entrance");
        }
        Entrance!.Map = currentMap++;
        if (BossRoom == null || !AllRooms.Contains(BossRoom))
        {
            throw new Exception("Palace lost its boss room");
        }
        if (AllRooms.Count(i => i.IsBossRoom) > 1)
        {
            throw new Exception("Palace has an extra boss room");
        }
        BossRoom.Map = currentMap++;
        if (isGP)
        {
            if (TbirdRoom == null || !AllRooms.Contains(TbirdRoom))
            {
                throw new Exception("Palace lost its tbird room");
            }
            if (AllRooms.Count(i => i.IsThunderBirdRoom) > 1)
            {
                throw new Exception("Palace has an extra tbird room");
            }
            TbirdRoom.Map = currentMap++;
        }
        else
        {
            if (!AllRooms.Contains(ItemRoom!))
            {
                throw new Exception("Palace lost its item room");
            }
            if(AllRooms.Any(i => i.HasItem && i != ItemRoom))
            {
                throw new Exception("Palace has an extra item room");
            }
            ItemRoom!.Map = currentMap++;
        }
        List<Room> normalRooms = AllRooms.Where(i => i.IsNormalRoom()).ToList();
        foreach(Room room in normalRooms)
        {
            //Only set the room number on the primary room of a linked room pair
            if(room.LinkedRoom == null || room.Enabled)
            {
                room.Map = currentMap++;
            }
        }
        //Linked room secondaries share a map number with their primary
        foreach(Room room in AllRooms.Where(i => i.LinkedRoom != null && !i.Enabled))
        {
            room.Map = room.LinkedRoom!.Map;
        }
        if(currentMap > 63)
        {
            throw new Exception("Map number has exceeded maximum");
        }
        return currentMap;
    }

    public void ReplaceRoom(Room roomToReplace, Room newRoom)
    {
        newRoom.coords = roomToReplace.coords;
        newRoom.PalaceGroup = roomToReplace.PalaceGroup;

        foreach(Room room in AllRooms.Where(i => i.Left == roomToReplace))
        {
            room.Left = newRoom;
        }
        foreach (Room room in AllRooms.Where(i => i.Down == roomToReplace))
        {
            room.Down = newRoom;
        }
        foreach (Room room in AllRooms.Where(i => i.Up == roomToReplace))
        {
            room.Up = newRoom;
        }
        foreach (Room room in AllRooms.Where(i => i.Right == roomToReplace))
        {
            room.Right = newRoom;
        }

        newRoom.Left = roomToReplace.Left;
        newRoom.Down = roomToReplace.Down;
        newRoom.Up = roomToReplace.Up;
        newRoom.Right = roomToReplace.Right;

        AllRooms.Remove(roomToReplace);
        AllRooms.Add(newRoom);
    }
}
