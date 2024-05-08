using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using RandomizerCore;

namespace Z2Randomizer.Core.Sidescroll;

public class Palace
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const bool DROPS_ARE_BLOCKERS = false;
    private const int OUTSIDE_ROOM_EXIT = 0b11111100;
    private Room root;
    private Room itemRoom;
    private Room bossRoom;
    private Room tbird;
    private readonly SortedDictionary<int, List<Room>> rooms;

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
    /*
    private List<Room> upExits;
    private List<Room> downExits;
    private List<Room> leftExits;
    private List<Room> rightExits;
    private List<Room> dropExits;
    private List<Room> onlyp7DownExits;
    */
    //private ROM ROMData;
    private int numRooms;
    private int baseAddr;
    private int connAddr;
    private List<Room> openRooms;
    private int maxRooms;
    private int netDeadEnds;
    private bool useCustomRooms;

    internal List<Room> AllRooms { get; private set; }

    public Room Root { get => root; set => root = value; }
    public Room ItemRoom { get => itemRoom; set => itemRoom = value; }
    public Room BossRoom { get => bossRoom; set => bossRoom = value; }
    public int NumRooms { get => numRooms; set => numRooms = value; }
    public int MaxRooms { get => maxRooms; set => maxRooms = value; }
    public int Number { get; set; }
    internal Room Tbird { get => tbird; set => tbird = value; }
    
    //DEBUG
    public int Generations { get; set; }

    public Palace(int number, int baseAddr, int connAddr, bool useCustomRooms)
    {
        Number = number;
        root = null;
        /*
        upExits = new List<Room>();
        downExits = new List<Room>();
        leftExits = new List<Room>();
        rightExits = new List<Room>();
        dropExits = new List<Room>();
        onlyp7DownExits = new List<Room>();
        */
        rooms = new SortedDictionary<int, List<Room>>();
        AllRooms = new List<Room>();
        numRooms = 0;
        this.baseAddr = baseAddr;
        this.connAddr = connAddr;
        //this.ROMData = ROMData;
        openRooms = new List<Room>();
        this.useCustomRooms = useCustomRooms;
        //dumpMaps();
        //createTree();
        if (Number < 7)
        {
            netDeadEnds = 3;
        }
        else
        {
            netDeadEnds = 2;
        }
    }

    public int GetOpenRooms()
    {
        return openRooms.Count;
    }

    public bool AddRoom(Room r, bool blockersAnywhere)
    {
        bool placed;
        r.PalaceGroup = GetPalaceGroup();

        if (netDeadEnds > 3 && r.IsDeadEnd)
        {
            return false;
        }

        if (netDeadEnds < -3 && r.CountOpenExits() > 2)
        {
            return false;
        }

        if(!blockersAnywhere)
        {
            RequirementType[] allowedBlockers = Palaces.ALLOWED_BLOCKERS_BY_PALACE[Number-1];
            if(!r.IsTraversable(allowedBlockers))
            {
                return false;
            }
            if ((Number == 1 || Number == 2 || Number == 5 || Number == 7) && r.HasBoss)
            {
                return false;
            }
        }

        if (openRooms.Count == 0)
        {
            openRooms.Add(r);
            ProcessRoom(r);
            return true;
        }
        //#13: Iterate over a copy of the open rooms list to prevent concurrent modification if AttachToOpen both removes an entry from openRooms and returns false
        foreach (Room open in openRooms.ToList())
        {
            placed = AttachToOpen(r, open);

            if (placed)
            {
                ProcessRoom(r);
                return true;
            }

        }
        return false;
    }

    private void ProcessRoom(Room r)
    {
        if (r.IsDeadEnd)
        {
            netDeadEnds++;
        }
        else if (r.CountOpenExits() > 1)
        {
            netDeadEnds--;
        }
        AllRooms.Add(r);
        //SortRoom(r);
        numRooms++;

        if (Number != 7 && openRooms.Count > 1 && itemRoom.CountOpenExits() > 0)
        {
            foreach (Room open2 in openRooms)
            {
                bool item = AttachToOpen(open2, itemRoom);
                if (item)
                {
                    break;
                }
            }
        }
        if (openRooms.Count > 1 && bossRoom.CountOpenExits() > 0)
        {
            foreach (Room open2 in openRooms)
            {
                bool boss = AttachToOpen(open2, bossRoom);
                if (boss)
                {
                    break;
                }
            }
        }
        if (Number == 7 && openRooms.Count > 1 && Tbird != null && Tbird.CountOpenExits() > 1)
        {
            foreach (Room open2 in openRooms)
            {
                bool boss = AttachToOpen(open2, tbird);
                if (boss)
                {
                    break;
                }
            }
        }
    }

    public void UpdateItem(Collectable i, ROM ROMData)
    {
        itemRoom.UpdateItem(i, ROMData);
    }

    public void Consolidate()
    {
        Room[] openCopy = new Room[openRooms.Count];
        openRooms.CopyTo(openCopy);
        foreach (Room r2 in openCopy)
        {
            foreach (Room r3 in openCopy)
            {
                if (r2 != r3 && openRooms.Contains(r2) && openRooms.Contains(r3))
                {
                    AttachToOpen(r2, r3);
                }
            }
        }
    }

    /// <summary>
    /// Attach the provided room to the open room if there is a compatable pair of exits between the two rooms.
    /// Rooms attempt to use the exits in the following order (from the perspective of open):  
    /// </summary>
    /// <param name="r"></param> The room to be attached
    /// <param name="open"></param> The room onto which R is attached
    /// <returns>Whether or not the room was actually able to be attached.</returns>
    private bool AttachToOpen(Room r, Room open)
    {
        bool placed = false;
        //Right from open into r
        if (!placed && open.HasRightExit() && open.Right == null && r.HasLeftExit() && r.Left == null)
        {
            open.Right = r;
            open.RightByte = (byte)((r.NewMap ?? r.Map) * 4);

            r.Left = open;
            r.LeftByte = (byte)((open.NewMap ?? open.Map) * 4 + 3);

            placed = true;
        }
        //Left open into r
        if (!placed && open.HasLeftExit() && open.Left == null && r.HasRightExit() && r.Right == null)
        {
            open.Left = r;
            open.LeftByte = (byte)((r.NewMap ?? r.Map) * 4 + 3);

            r.Right = open;
            r.RightByte = (byte)((open.NewMap ?? open.Map) * 4);

            placed = true;
        }
        //Elevator Up from open
        if (!placed && open.HasUpExit() && open.Up == null && r.HasDownExit() && r.Down == null && !r.HasDrop)
        {
            open.Up = r;
            open.UpByte = (byte)((r.NewMap ?? r.Map) * 4 + r.ElevatorScreen);

            r.Down = open;
            r.DownByte = (byte)((open.NewMap ?? open.Map) * 4 + open.ElevatorScreen);

            placed = true;
        }
        //Down Elevator from open
        if (!placed && open.HasDownExit() && !open.HasDrop && open.Down == null && r.HasUpExit() && r.Up == null)
        {

            open.Down = r;
            open.DownByte = (byte)((r.NewMap ?? r.Map) * 4 + r.ElevatorScreen);

            r.Up = open;
            r.UpByte = (byte)((open.NewMap ?? open.Map) * 4 + open.ElevatorScreen);

            placed = true;
        }
        //Drop from open into r
        if (!placed && open.HasDownExit() && open.HasDrop && open.Down == null && r.IsDropZone)
        {

            open.Down = r;
            open.DownByte = (byte)((r.NewMap ?? r.Map) * 4);
            r.IsDropZone = false;
            placed = true;
        }
        //Drop from r into open 
        if (!placed && open.IsDropZone && r.HasDrop && r.Down == null && r.HasDownExit())
        {

            r.Down = open;
            r.DownByte = (byte)((open.NewMap ?? open.Map) * 4);
            open.IsDropZone = false;
            placed = true;
        }
        //If the room doesn't have any open exits anymore, remove it from the list
        //#13: If the room doesn't have any open exits, how did it get into the 
        if (open.CountOpenExits() == 0)
        {
            openRooms.Remove(open);
        }
        //Otherwise, if the open room isn't in the open rooms list (What? How?) and the pending openings hasn't been met,
        //put the open room in openRooms where it belongs, and then for some reason mark that we successfully placed the room even though we didn't.
        else if (!openRooms.Contains(open) && (openRooms.Count < 3 || placed))
        {
            openRooms.Add(open);
            placed = true;
        }
        //If the room itself is already in the open rooms list (How?), but we filled the last exit, remove it from the open rooms list.
        if (r.CountOpenExits() == 0)
        {
            openRooms.Remove(r);
        }
        //Otherwise, if the room being added still has unmatched openings, and the maximum pending openings hasn't been met, add this room to the open rooms list
        else if (!openRooms.Contains(r) && (openRooms.Count < 3 || placed))
        {
            openRooms.Add(r);
            placed = true;
        }

        return placed;
    }

    public bool RequiresThunderbird()
    {
        CheckSpecialPaths(root, 2);
        return !bossRoom.IsBeforeTbird;
    }

    public bool HasDeadEnd()
    {
        List<Room> dropExits = AllRooms.Where(i => i.HasDownExit() && i.HasDrop).ToList();
        if (dropExits.Count == 0 || dropExits.Any(i => i.Down == null))
        {
            return false;
        }
        Room end = BossRoom;
        foreach (Room r in dropExits)
        {
            List<Room> reachable = new List<Room>();
            List<Room> roomsToCheck = new List<Room>();
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
            if (!reachable.Contains(root) && !reachable.Contains(end))
            {
                return true;
            }
        }
        return false;
    }



    private void CheckSpecialPaths(Room r, int dir)
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
                CheckSpecialPaths(r.Left, 3);
            }

            if (r.Right != null)
            {
                CheckSpecialPaths(r.Right, 2);
            }

            if (r.Up != null)
            {
                CheckSpecialPaths(r.Up, 1);
            }

            if (r.Down != null)
            {
                CheckSpecialPaths(r.Down, 0);
            }
        }
    }

    public bool AllReachable()
    {
        foreach (Room r in AllRooms)
        {
            if (r.HasBoss && CanEnterBossFromLeft(r))
            {
                return false;
            }
        }
        CheckPaths(root, Direction.WEST);
        foreach (Room r in AllRooms)
        {
            if (!r.IsPlaced)
            {
                return false;
            }
        }
        return true;
    }

    private void CheckPaths(Room r, Direction originDirection)
    {
        if (!r.IsPlaced)
        {
            //Debug.WriteLine(r.PalaceGroup + " " + r.Map);
            //For required thunderbird, you can't path backwards into tbird room
            if ((Number == 7) && r.IsThunderBirdRoom)
            {
                if (originDirection == Direction.EAST)
                {
                    r.IsPlaced = false;
                    return;
                }
            }
            r.IsPlaced = true;
            if (r.Left != null)
            {
                CheckPaths(r.Left, Direction.EAST);
            }

            if (r.Right != null)
            {
                CheckPaths(r.Right, Direction.WEST);
            }

            if (r.Up != null)
            {
                CheckPaths(r.Up, Direction.SOUTH);
            }

            if (r.Down != null)
            {
                CheckPaths(r.Down, Direction.NORTH);
            }
        }
    }

    private bool CanEnterBossFromLeft(Room b)
    {
        List<Room> reachable = [];
        List<Room> roomsToCheck = [];

        Room entrance = AllRooms.First(i => i.IsEntrance);

        if(entrance.Down != null)
        {
            reachable.Add(entrance.Down);
            roomsToCheck.Add(entrance.Down);
        }

        if (entrance.Up != null)
        {
            reachable.Add(entrance.Up);
            roomsToCheck.Add(entrance.Up);
        }

        if (entrance.Right != null)
        {
            reachable.Add(entrance.Right);
            roomsToCheck.Add(entrance.Right);
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
        List<Room> roomsWithUpExits = AllRooms.Where(i => i.HasUpExit()).ToList();
        List<Room> roomsWithDownExits = AllRooms.Where(i => i.HasDownExit() && !i.HasDrop).ToList();
        List<Room> roomsWithLeftExits = AllRooms.Where(i => i.HasLeftExit()).ToList();
        List<Room> roomsWithRightExits = AllRooms.Where(i => i.HasRightExit()).ToList();
        List<Room> roomsWithDropExits = AllRooms.Where(i => i.HasDrop).ToList();

        for (int i = 0; i < roomsWithUpExits.Count; i++)
        {
            Room selectedRoom = roomsWithUpExits[i];
            Room swapRoom = roomsWithUpExits.Sample(r);
            selectedRoom.Up.Down = swapRoom;
            swapRoom.Up.Down = selectedRoom;

            (selectedRoom.Up, swapRoom.Up) = (swapRoom.Up, selectedRoom.Up);
            (selectedRoom.UpByte, swapRoom.UpByte) = (swapRoom.UpByte, selectedRoom.UpByte);
            (swapRoom.Up.DownByte, selectedRoom.Up.DownByte) = (selectedRoom.Up.DownByte, swapRoom.Up.DownByte);

            if (roomsWithUpExits.Any(i => i.Up == null))
            {
                logger.Error("Up Exits desynched!");
            }
        }
        for (int i = 0; i < roomsWithDropExits.Count; i++)
        {
            int swap = r.Next(i, roomsWithDropExits.Count);

            Room temp = roomsWithDropExits[i].Down;
            byte tempByte = roomsWithDropExits[i].DownByte;

            roomsWithDropExits[i].Down = roomsWithDropExits[swap].Down;
            roomsWithDropExits[i].DownByte = roomsWithDropExits[swap].DownByte;
            roomsWithDropExits[swap].Down = temp;
            roomsWithDropExits[swap].DownByte = tempByte;
        }

        for (int i = 0; i < roomsWithDownExits.Count; i++)
        {
            Room selectedRoom = roomsWithDownExits[i];
            Room swapRoom = roomsWithDownExits.Sample(r);
            selectedRoom.Down.Up = swapRoom;
            swapRoom.Down.Up = selectedRoom;

            (selectedRoom.Down, swapRoom.Down) = (swapRoom.Down, selectedRoom.Down);
            (selectedRoom.DownByte, swapRoom.DownByte) = (swapRoom.DownByte, selectedRoom.DownByte);
            (swapRoom.Down.UpByte, selectedRoom.Down.UpByte) = (selectedRoom.Down.UpByte, swapRoom.Down.UpByte);

            if (roomsWithDownExits.Any(i => i.Down == null))
            {
                logger.Error("Down Exits desynched!");
            }
        }

        for (int i = 0; i < roomsWithLeftExits.Count; i++)
        {

            Room selectedRoom = roomsWithLeftExits[i];
            Room swapRoom = roomsWithLeftExits.Sample(r);
            selectedRoom.Left.Right = swapRoom;
            swapRoom.Left.Right = selectedRoom;

            (selectedRoom.Left, swapRoom.Left) = (swapRoom.Left, selectedRoom.Left);
            (selectedRoom.LeftByte, swapRoom.LeftByte) = (swapRoom.LeftByte, selectedRoom.LeftByte);
            (swapRoom.Left.RightByte, selectedRoom.Left.RightByte) = (selectedRoom.Left.RightByte, swapRoom.Left.RightByte);

            if (roomsWithLeftExits.Any(i => i.Left == null))
            {
                logger.Error("Left Exits desynched!");
            }
        }

        for (int i = 0; i < roomsWithRightExits.Count; i++)
        {
            Room selectedRoom = roomsWithRightExits[i];
            Room swapRoom = roomsWithRightExits.Sample(r);
            selectedRoom.Right.Left = swapRoom;
            swapRoom.Right.Left = selectedRoom;

            (selectedRoom.Right, swapRoom.Right) = (swapRoom.Right, selectedRoom.Right);
            (selectedRoom.RightByte, swapRoom.RightByte) = (swapRoom.RightByte, selectedRoom.RightByte);
            (swapRoom.Right.LeftByte, selectedRoom.Right.LeftByte) = (selectedRoom.Right.LeftByte, swapRoom.Right.LeftByte);

            if (roomsWithRightExits.Any(i => i.Right == null))
            {
                logger.Error("Right Exits desynched!");
            }
        }
        if (Number == 6)
        {
            foreach (Room room in roomsWithDropExits)
            {
                if (room.Down.Map == 0xBC)
                {
                    int db = room.DownByte;
                    room.DownByte = (byte)((db & 0xFC) + 2);
                }
                else
                {
                    int db = room.DownByte;
                    room.DownByte = (byte)((db & 0xFC) + 1);
                }
            }
        }
    }

    public void SetOpenRoom(Room r)
    {
        openRooms.Add(r);
    }

    public void UpdateRom(ROM ROMData)
    {
        List<Room> roomsToRemove = new();
        //Unify the parts of segmented rooms back together
        foreach (Room room in AllRooms.ToList())
        {
            //If this is the primary room of a linked room pair
            if (room.LinkedRoomName != null && room.Enabled && room.LinkedRoom != null)
            {
                Room linkedRoom = room.LinkedRoom;
                if(room.isUpDownReversed != linkedRoom.isUpDownReversed)
                {
                    throw new Exception("Inconsistent isUpDownReversed in linked rooms");
                }
                //set each blank exit on the master room that has a counterpart in the linked room
                if (linkedRoom.HasLeftExit() && room.Left == null && linkedRoom.Left != null)
                {
                    room.Left = linkedRoom.Left;
                    room.LeftByte = linkedRoom.LeftByte;
                }
                if (linkedRoom.HasRightExit() && room.Right == null && linkedRoom.Right != null)
                {
                    room.Right = linkedRoom.Right;
                    room.RightByte = linkedRoom.RightByte;
                }
                if (linkedRoom.HasUpExit() && room.Up == null && linkedRoom.Up != null)
                {
                    room.Up = linkedRoom.Up;
                    room.UpByte = linkedRoom.UpByte;
                }
                if (linkedRoom.HasDownExit() && room.Down == null && linkedRoom.Down != null)
                {
                    room.Down = linkedRoom.Down;
                    room.DownByte = linkedRoom.DownByte;
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
        AllRooms.ForEach(r => r.UpdateConnectionBytes());

        //Update connections
        foreach(Room room in AllRooms.Where(r => r.PageCount != 4))
        {
            if (room.PageCount <= 1)
            {
                throw new Exception("Palaces cannot have fewer than 2 pages");
            }
            if (room.PageCount > 4)
            {
                throw new Exception("Palaces cannot have more than 4 pages");
            }
            if (room.PageCount == 2)
            {
                if(room.Right != null)
                {
                    room.Right.Connections[0] = (byte)((room.NewMap ?? room.Map) * 4 + 1);
                }
                if (room.Up != null || room.Down != null)
                {
                    throw new Exception("Up/Down not supported for 2 screen rooms");
                }
            }
            if (room.PageCount == 3)
            {
                if (room.Right != null)
                {
                    room.Right.Connections[0] = (byte)((room.NewMap ?? room.Map) * 4 + 2);
                }
                if (room.Up != null || room.Down != null)
                {
                    logger.Debug("Up/Down in 3 screen rooms is weird");
                }
            }
        }

        foreach (Room r in AllRooms)
        {

            //If a room is fewer than 4 pages, "right" doesn't actually work.
            //Exits in Z2 point you not based on how you exit, but what page you exit from.
            //Because of that the right exit from a 2 page map is actually the down connection
            //this standardizes the exits so 2 page rooms are always left / right
            //3 page rooms are always left / down / right
            //and then 4 page rooms are always left / down / up / right
            ROMData.Put(r.ConnectionStartAddress + 0, r.Connections[0]);
            if(r.PageCount == 2)
            {
                ROMData.Put(r.ConnectionStartAddress + 1, r.Connections[3]);
                ROMData.Put(r.ConnectionStartAddress + 2, 0xFF);
                ROMData.Put(r.ConnectionStartAddress + 3, 0xFF);
                continue;
            }
            ROMData.Put(r.ConnectionStartAddress + 1, r.Connections[1]);
            if (r.PageCount == 3)
            {
                ROMData.Put(r.ConnectionStartAddress + 2, r.Connections[3]);
                ROMData.Put(r.ConnectionStartAddress + 3, 0xFF);
                continue;
            }
            ROMData.Put(r.ConnectionStartAddress + 2, r.Connections[2]);
            ROMData.Put(r.ConnectionStartAddress + 3, r.Connections[3]);
        }
    }


    public void CreateTree(bool removeTbird)
    {
        foreach (Room r in AllRooms)
        {
            if (rooms.ContainsKey(r.Map * 4))
            {
                /*
                List<Room> l = rooms[r.Map * 4];
                l.Add(r);
                rooms.Remove(r.Map * 4);
                rooms.Add(r.Map * 4, l);
                */
                rooms[r.Map * 4].Add(r);
            }
            else
            {
                List<Room> l = new List<Room> { r };
                rooms.Add(r.Map * 4, l);
            }
            //SortRoom(r);
        }
        foreach (Room r in AllRooms)
        {
            if (r.Left == null && r.HasLeftExit())
            {
                List<Room> l = rooms[r.LeftByte & 0xFC];
                foreach (Room r2 in l)
                {
                    if ((r2.RightByte & 0xFC) / 4 == r.Map)
                    {
                        r.Left = r2;
                    }
                }
            }

            if (r.Right == null && r.HasRightExit())
            {
                List<Room> l = rooms[r.RightByte & 0xFC];
                foreach (Room r2 in l)
                {
                    if ((r2.LeftByte & 0xFC) / 4 == r.Map)
                    {
                        r.Right = r2;
                    }
                }
            }

            if (r.Up == null && r.HasUpExit())
            {
                List<Room> l = rooms[r.UpByte & 0xFC];
                foreach (Room r2 in l)
                {
                    if ((r2.DownByte & 0xFC) / 4 == r.Map)
                    {
                        r.Up = r2;
                    }
                    else if (r2.Map == (r.UpByte & 0xFC) / 4 && r2.IsDropZone) {
                        r.Up = r2;
                    }
                }
            }

            if (r.Down == null && r.HasDownExit())
            {
                List<Room> l = rooms[r.DownByte & 0xFC];
                foreach (Room r2 in l)
                {
                    if ((r2.UpByte & 0xFC) / 4 == r.Map)
                    {
                        r.Down = r2;
                    }
                    else if (r2.Map == (r.DownByte & 0xFC) / 4 && r2.IsDropZone)
                    {
                        r.Down = r2;
                    }
                }
            }
            if ((r.UpByte & 0xFC) == 0 && (root.DownByte & 0xFC) / 4 == r.Map)
            {
                r.Up = root;
            }
        }
        if (removeTbird)
        {
            Tbird.Left.RightByte = Tbird.RightByte;
            Tbird.Right.LeftByte = Tbird.LeftByte;
            Tbird.Left.Right = Tbird.Right;
            Tbird.Right.Left = Tbird.Left;
            //leftExits.Remove(Tbird);
            //rightExits.Remove(Tbird);
            AllRooms.Remove(Tbird);
        }
    }

    public void Shorten(Random random)
    {
        List<Room> upExits = AllRooms.Where(i => i.HasUpExit()).ToList();
        List<Room> downExits = AllRooms.Where(i => i.HasDownExit() && !i.HasDrop).ToList();
        List<Room> leftExits = AllRooms.Where(i => i.HasLeftExit()).ToList();
        List<Room> rightExits = AllRooms.Where(i => i.HasRightExit()).ToList();
        List<Room> dropExits = AllRooms.Where(i => i.HasDownExit() && i.HasDrop).ToList();

        int target = random.Next(numRooms / 2, (numRooms * 3) / 4) + 1;
        int rooms = numRooms;
        int tries = 0;
        while (rooms > target && tries < 100000)
        {
            int r = random.Next(rooms);
            Room remove = null;
            if (leftExits.Count < rightExits.Count)
            {
                remove = rightExits[random.Next(rightExits.Count)];
            }

            if (r < leftExits.Count)
            {
                remove = leftExits[r];
            }

            r -= leftExits.Count;
            if (r < upExits.Count && r >= 0)
            {
                remove = upExits[r];
            }
            r -= upExits.Count;
            if (r < rightExits.Count && r >= 0)
            {
                remove = rightExits[r];
            }
            r -= rightExits.Count;
            if (r < downExits.Count && r >= 0)
            {
                remove = downExits[r];
            }

            if (dropExits.Contains(remove) || remove.IsThunderBirdRoom || remove == bossRoom)
            {
                tries++;
                continue;

            }
            else
            {
                bool hasRight = remove.Right != null;
                bool hasLeft = remove.Left != null;
                bool hasUp = remove.Up != null;
                bool hasDown = remove.Down != null;

                int n = 0;
                n = hasRight ? n + 1 : n;
                n = hasLeft ? n + 1 : n;
                n = hasUp ? n + 1 : n;
                n = hasDown ? n + 1 : n;

                //logger.WriteLine(n);

                if (n >= 3 || n == 1)
                {
                    tries++;
                    continue;
                }

                if (hasLeft && hasRight && (dropExits[0].Down != remove && dropExits[1].Down != remove && dropExits[2].Down != remove && dropExits[3].Down != remove))
                {
                    remove.Left.Right = remove.Right;
                    remove.Right.Left = remove.Left;
                    remove.Left.RightByte = remove.RightByte;
                    remove.Right.LeftByte = remove.LeftByte;
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
                    remove.Up.Down = remove.Down;
                    remove.Down.Up = remove.Up;
                    remove.Up.DownByte = remove.DownByte;
                    remove.Down.UpByte = remove.UpByte;
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
                        int count = 1;
                        count = remove.Left.Up != null ? count + 1 : count;
                        count = remove.Left.Down != null ? count + 1 : count;
                        count = remove.Left.Left != null ? count + 1 : count;
                        if (count >= 3 || count == 1)
                        {
                            tries++;
                            continue;
                        }

                        if (remove.Left.Up == null || remove.Left.Up != root)
                        {
                            tries++;
                            continue;
                        }

                        remove.Left.Up.Down = remove.Down;
                        remove.Left.Up.DownByte = remove.DownByte;
                        remove.Down.Up = remove.Left.Up;
                        remove.Down.UpByte = remove.Left.UpByte;

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
                        int count = 1;
                        count = remove.Right.Up != null ? count + 1 : count;
                        count = remove.Right.Down != null ? count + 1 : count;
                        count = remove.Right.Right != null ? count + 1 : count;
                        if (count >= 3 || count == 1)
                        {
                            tries++;
                            continue;
                        }

                        if (remove.Right.Up == null || remove.Right.Up == root)
                        {
                            tries++;
                            continue;
                        }

                        remove.Right.Up.Down = remove.Down;
                        remove.Right.Up.DownByte = remove.DownByte;
                        remove.Down.Up = remove.Right.Up;
                        remove.Down.UpByte = remove.Right.UpByte;

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
                        int count = 1;
                        count = remove.Left.Up != null ? count + 1 : count;
                        count = remove.Left.Down != null ? count + 1 : count;
                        count = remove.Left.Left != null ? count + 1 : count;
                        if (count >= 3 || count == 1)
                        {
                            tries++;
                            continue;
                        }

                        if (remove.Left.Down == null || dropExits.Contains(remove.Left))
                        {
                            tries++;
                            continue;
                        }

                        remove.Left.Down.Up = remove.Up;
                        remove.Left.Down.UpByte = remove.UpByte;
                        remove.Up.Down = remove.Left.Down;
                        remove.Up.DownByte = remove.Left.DownByte;

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
                        int count = 1;
                        count = remove.Right.Up != null ? count + 1 : count;
                        count = remove.Right.Down != null ? count + 1 : count;
                        count = remove.Right.Right != null ? count + 1 : count;
                        if (count >= 3 || count == 1)
                        {
                            tries++;
                            continue;
                        }

                        if (remove.Right.Down == null || dropExits.Contains(remove.Right))
                        {
                            tries++;
                            continue;
                        }

                        remove.Right.Down.Up = remove.Up;
                        remove.Right.Down.UpByte = remove.UpByte;
                        remove.Up.Down = remove.Right.Down;
                        remove.Up.DownByte = remove.Right.DownByte;

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
        }
        logger.Debug("Target: " + target + " Rooms: " + rooms);
    }

    public void RandomizeSmallItems(Random r, bool extraKeys)
    {
        foreach (Room room in AllRooms)
        {
            int sideviewIndex = room.PalaceNumber == 7 ? 5 : 4; //Header bytes
            while(sideviewIndex < room.SideView.Length)
            {
                int firstByte = room.SideView[sideviewIndex++];
                int secondByte = room.SideView[sideviewIndex++];
                int ypos = (firstByte & 0xF0) >> 4;
                if(secondByte == 15 && ypos < 13)
                {
                    int thirdByte = room.SideView[sideviewIndex++];
                    if (!SHUFFLABLE_SMALL_ITEMS.Contains((Collectable)thirdByte))
                    {
                        continue;
                    }
                    double d = r.NextDouble();
                    if(room.PalaceNumber == 7)
                    {
                        if (d <= 1)
                        {
                            room.SideView[sideviewIndex - 1] = (int)Collectable.ONEUP;
                        }
                    }
                    else
                    {
                        if(extraKeys)
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
                        else if(d <= .35)
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
    }

    public List<Room> CheckBlocks()
    {
        return CheckBlocksHelper(new List<Room>(), new List<Room>(), root);
    }

    private List<Room> CheckBlocksHelper(List<Room> c, List<Room> blockers, Room r)
    {
        if (c.Contains(this.itemRoom))
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
            r.IsPlaced = false;
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
                Room duplicateRoom = null;
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
            coveredRooms.Add(room);
            pendingRooms.Remove(room);
            if (room.Left != null && room.Left.IsTraversable(requireables) && !coveredRooms.Contains(room.Left))
            {
                pendingRooms.Add(room.Left);
            }
            if (room.Right != null && room.Right.IsTraversable(requireables) && !coveredRooms.Contains(room.Right))
            {
                pendingRooms.Add(room.Right);
            }
            if (room.Up != null && room.Up.IsTraversable(requireables) && !coveredRooms.Contains(room.Up))
            {
                pendingRooms.Add(room.Up);
            }
            if (room.Down != null && room.Down.IsTraversable(requireables) && !coveredRooms.Contains(room.Down))
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
        foreach(Room r in AllRooms)
        {
            //If this room connects to any rooms that aren't in the same palace, it's leftover stray data from the vanilla versions of the rooms.
            //While often these connections are inaccessible, the can freak out Z2Edit's sideview graph.
            //No reason not to remove them.
            if (r.Left == null || !AllRooms.Contains(r.Left))
            {
                r.LeftByte = OUTSIDE_ROOM_EXIT;
            }
            if (r.Right == null || !AllRooms.Contains(r.Right))
            {
                r.RightByte = OUTSIDE_ROOM_EXIT;
            }
            if (r.Up == null || !AllRooms.Contains(r.Up))
            {
                r.UpByte = OUTSIDE_ROOM_EXIT;
            }
            if (r.Down == null || !AllRooms.Contains(r.Down))
            {
                r.DownByte = OUTSIDE_ROOM_EXIT;
            }
        }
    }
}
