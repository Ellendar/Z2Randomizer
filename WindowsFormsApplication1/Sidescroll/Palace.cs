using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Z2Randomizer.Sidescroll;

namespace Z2Randomizer.Sidescroll;

public class Palace
{
    private readonly Logger logger = LogManager.GetCurrentClassLogger();
    private Room root;
    private Room itemRoom;
    private Room bossRoom;
    private Room tbird;
    private SortedDictionary<int, List<Room>> rooms;
    private List<Room> upExits;
    private List<Room> downExits;
    private List<Room> leftExits;
    private List<Room> rightExits;
    private List<Room> allRooms;
    private List<Room> onlyDownExits;
    private List<Room> onlyp7DownExits;
    //private ROM ROMData;
    private int numRooms;
    private int baseAddr;
    private int connAddr;
    private bool needJumpOrFairy;
    private bool needFairy;
    private bool needGlove;
    private bool needDstab;
    private bool needUstab;
    private bool needReflect;
    private List<Room> openRooms;
    private int maxRooms;
    private int netDeadEnds;
    private bool useCustomRooms;

    internal List<Room> AllRooms
    {
        get
        {
            return allRooms;
        }

        set
        {
            allRooms = value;
        }
    }

    public bool NeedJumpOrFairy { get => needJumpOrFairy; set => needJumpOrFairy = value; }
    public bool NeedFairy { get => needFairy; set => needFairy = value; }
    public bool NeedGlove { get => needGlove; set => needGlove = value; }
    public bool NeedDstab { get => needDstab; set => needDstab = value; }
    public Room Root { get => root; set => root = value; }
    public Room ItemRoom { get => itemRoom; set => itemRoom = value; }
    public Room BossRoom { get => bossRoom; set => bossRoom = value; }
    public bool NeedUstab { get => needUstab; set => needUstab = value; }
    public int NumRooms { get => numRooms; set => numRooms = value; }
    public int MaxRooms { get => maxRooms; set => maxRooms = value; }
    public int Number { get; set; }
    internal Room Tbird { get => tbird; set => tbird = value; }
    public bool NeedReflect { get => needReflect; set => needReflect = value; }
    

    //DEBUG
    public int Generations { get; set; }

    public Palace(int number, int baseAddr, int connAddr)
    {
        Number = number;
        root = null;
        upExits = new List<Room>();
        downExits = new List<Room>();
        leftExits = new List<Room>();
        rightExits = new List<Room>();
        onlyDownExits = new List<Room>();
        onlyp7DownExits = new List<Room>();
        rooms = new SortedDictionary<int, List<Room>>();
        allRooms = new List<Room>();
        numRooms = 0;
        this.baseAddr = baseAddr;
        this.connAddr = connAddr;
        //this.ROMData = ROMData;
        needDstab = false;
        needFairy = false;
        needGlove = false;
        needJumpOrFairy = false;
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

    /*
    public static void DumpMaps(ROM ROMData)
    {
        int[] connAddr = new int[] { 0x1072B, 0x12208, 0x1472B };
        int[] side = new int[] { 0x10533, 0x12010, 0x14533 };
        int[] enemy = new int[] { 0x105b1, 0x1208E, 0x145b1 };
        int[] bit = new int[] { 0x17ba5, 0x17bc5, 0x17be5 };
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 63; i++)
            {
                int addr = connAddr[j] + i * 4;
                byte[] connectBytes = new Byte[4];
                for (int k = 0; k < 4; k++)
                {
                    connectBytes[k] = ROMData.GetByte(addr + k);

                }
                Room room;
                int sideViewPtr = (ROMData.GetByte(side[j] + i * 2) + (ROMData.GetByte(side[j] + 1 + i * 2) << 8)) + 0x8010;
                if (j == 2)
                {
                    sideViewPtr = (ROMData.GetByte(side[j] + i * 2) + (ROMData.GetByte(side[j] + 1 + i * 2) << 8)) + 0xC010;
                }
                int sideViewLength = ROMData.GetByte(sideViewPtr);
                byte[] sideView = ROMData.GetBytes(sideViewPtr, sideViewPtr + sideViewLength);

                int enemyPtr = ROMData.GetByte(enemy[j] + i * 2) + (ROMData.GetByte(enemy[j] + 1 + i * 2) << 8) + 0x98b0;
                if (j == 2)
                {
                    enemyPtr = ROMData.GetByte(enemy[j] + i * 2) + (ROMData.GetByte(enemy[j] + 1 + i * 2) << 8) + 0xd8b0;
                }

                int enemyLength = ROMData.GetByte(enemyPtr);
                byte[] enemies = ROMData.GetBytes(enemyPtr, enemyPtr + enemyLength);

                Byte bitmask = ROMData.GetByte(bit[j] + i / 2);
            
                if (i % 2 == 0)
                {
                    bitmask = (byte)(bitmask & 0xF0);
                    bitmask = (byte)(bitmask >> 4);
                }
                else
                {
                    bitmask = (byte)(bitmask & 0x0F);
                }


                //room = new Room(i, connectBytes, enemies, sideView, bitmask, false, false, false, false, false, false, false, false, -1, addr, false, false);

                //room.Dump();
            }
        }
    }
    */

    public int GetOpenRooms()
    {
        return openRooms.Count;
    }
    public void UpdateBlocks()
    {
        List<Room> itemPath = CheckBlocks();
        foreach (Room r in itemPath)
        {
            if (Number == 4 && r == BossRoom)
            {
                this.NeedReflect = true;
            }
            if (r.IsFairyBlocked)
            {
                this.needFairy = true;
            }
            if (r.IsDownstabBlocked)
            {
                this.needDstab = true;
            }
            if (r.IsUpstabBlocked)
            {
                this.needUstab = true;
            }
            if (r.IsJumpBlocked)
            {
                this.needJumpOrFairy = true;
            }
            if (r.IsGloveBlocked)
            {
                this.needGlove = true;
            }
        }
    }

    public bool AddRoom(Room r, bool blocker)
    {
        bool placed = false;



        if (netDeadEnds > 3 && r.IsDeadEnd)
        {
            return false;
        }

        if (netDeadEnds < -3 && r.CountOpenExits() > 2)
        {
            return false;
        }

        if (!AppropriateBlocker(r, blocker))
        {
            return false;
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
        allRooms.Add(r);
        SortRoom(r);
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

    public void UpdateItem(Item i, ROM ROMData)
    {
        if (Number == 1 || Number == 2 || Number == 5)
        {
            itemRoom.UpdateItem(i, 1, ROMData);
        }
        else
        {
            itemRoom.UpdateItem(i, 2, ROMData);
        }
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

    private bool AppropriateBlocker(Room r, bool blockers)
    {
        if (!blockers)
        {
            if (Number == 1)
            {
                if (r.IsFairyBlocked || r.IsDownstabBlocked || r.IsUpstabBlocked || r.IsJumpBlocked || r.IsGloveBlocked || r.HasDrop || r.IsDropZone || r.HasBoss)
                {
                    return false;
                }
            }

            if (Number == 2)
            {
                if (r.IsFairyBlocked || r.IsDownstabBlocked || r.IsUpstabBlocked || r.HasDrop || r.IsDropZone || r.HasBoss)
                {
                    return false;
                }
            }

            if (Number == 3)
            {
                if (r.IsJumpBlocked || r.IsFairyBlocked || r.HasDrop || r.IsDropZone)
                {
                    return false;
                }
            }

            if (Number == 4)
            {
                if (r.IsGloveBlocked || r.IsUpstabBlocked || r.IsDownstabBlocked)
                {
                    return false;
                }
            }

            if (Number == 5)
            {
                if (r.IsGloveBlocked || r.IsUpstabBlocked || r.IsDownstabBlocked || r.HasDrop || r.IsDropZone || r.HasBoss)
                {
                    return false;
                }
            }

            if (Number == 6)
            {
                if (r.IsUpstabBlocked || r.IsDownstabBlocked)
                {
                    return false;
                }
            }
        }
        else
        {
            if ((Number == 1 || Number == 2 || Number == 5 || Number == 7) && r.HasBoss)
            {
                return false;
            }
        }
        return true;
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
            open.RightByte = r.NewMap * 4;

            r.Left = open;
            r.LeftByte = open.NewMap * 4 + 3;

            placed = true;
        }
        //Left open into r
        if (!placed && open.HasLeftExit() && open.Left == null && r.HasRightExit() && r.Right == null)
        {
            open.Left = r;
            open.LeftByte = r.NewMap * 4 + 3;

            r.Right = open;
            r.RightByte = open.NewMap * 4;

            placed = true;
        }
        //Elevator Up from open
        if (!placed && open.HasUpExit() && open.Up == null && r.HasDownExit() && r.Down == null && !r.HasDrop)
        {
            open.Up = r;
            open.UpByte = r.NewMap * 4 + r.ElevatorScreen;

            r.Down = open;
            r.DownByte = open.NewMap * 4 + open.ElevatorScreen;

            placed = true;
        }
        //Down Elevator from open
        if (!placed && open.HasDownExit() && !open.HasDrop && open.Down == null && r.HasUpExit() && r.Up == null)
        {

            open.Down = r;
            open.DownByte = r.NewMap * 4 + r.ElevatorScreen;

            r.Up = open;
            r.UpByte = open.NewMap * 4 + open.ElevatorScreen;

            placed = true;
        }
        //Drop from open into r
        if (!placed && open.HasDownExit() && open.HasDrop && open.Down == null && r.IsDropZone)
        {

            open.Down = r;
            open.DownByte = r.NewMap * 4;
            r.IsDropZone = false;
            placed = true;
        }
        //Drop from r into open 
        if (!placed && open.IsDropZone && r.HasDrop && r.Down == null && r.HasDownExit())
        {

            r.Down = open;
            r.DownByte = open.NewMap * 4;
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



    public void SortRoom(Room r)
    {
        if (r.HasDownExit())
        {
            if (r.HasDrop)
            {
                onlyDownExits.Add(r);
            }
            else
            {
                downExits.Add(r);
            }
        }

        if (r.HasLeftExit())
        {
            leftExits.Add(r);
        }

        if (r.HasRightExit())
        {
            rightExits.Add(r);
        }

        if (r.HasUpExit())
        {
            upExits.Add(r);
        }
    }

    public bool RequiresThunderbird()
    {
        CheckSpecialPaths(root, 2);
        return !bossRoom.IsBeforeTbird;
    }

    public bool HasDeadEnd()
    {
        if (onlyDownExits.Count == 0)
        {
            return false;
        }
        Room end = BossRoom;
        foreach (Room r in onlyDownExits)
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
            if ((Number == 7) && r.Map == PalaceRooms.Thunderbird(useCustomRooms).Map)
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
        CheckPaths(root, 2);
        foreach (Room r in allRooms)
        {
            if (!r.IsPlaced)
            {
                return false;
            }
        }
        return true;
    }
    //0 = up, 1 = down, 2 = left, 3 = right
    //WARNING: This is different from the overworld direction order... For some reason.
    private void CheckPaths(Room r, int dir)
    {
        if (!r.IsPlaced)
        {
            if ((Number == 7) && r.Map == PalaceRooms.Thunderbird(useCustomRooms).Map)
            {
                if (dir == 3)
                {
                    r.IsPlaced = false;
                    return;
                }
            }
            r.IsPlaced = true;
            if (r.Left != null)
            {
                CheckPaths(r.Left, 3);
            }

            if (r.Right != null)
            {
                CheckPaths(r.Right, 2);
            }

            if (r.Up != null)
            {
                CheckPaths(r.Up, 1);
            }

            if (r.Down != null)
            {
                CheckPaths(r.Down, 0);
            }
        }
    }

    private bool CanEnterBossFromLeft(Room b)
    {
        List<Room> reachable = new List<Room>();
        List<Room> roomsToCheck = new List<Room>();
        reachable.Add(root.Down);
        roomsToCheck.Add(root.Down);

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
        //Digshake - This method is so ugly and i hate it.
        for (int i = 0; i < upExits.Count; i++)
        {
            int swap = r.Next(i, upExits.Count);
            Room temp = upExits[i].Up;
            Room down1 = upExits[swap].Up;
            temp.Down = upExits[swap];
            down1.Down = upExits[i];
            upExits[i].Up = down1;
            upExits[swap].Up = temp;

            int tempByte = upExits[i].UpByte;
            upExits[i].UpByte = upExits[swap].UpByte;
            upExits[swap].UpByte = tempByte;

            tempByte = temp.DownByte;
            temp.DownByte = down1.DownByte;
            down1.DownByte = tempByte;
        }
        for (int i = 0; i < onlyDownExits.Count; i++)
        {
            int swap = r.Next(i, onlyDownExits.Count);

            Room temp = onlyDownExits[i].Down;
            int tempByte = onlyDownExits[i].DownByte;

            onlyDownExits[i].Down = onlyDownExits[swap].Down;
            onlyDownExits[i].DownByte = onlyDownExits[swap].DownByte;
            onlyDownExits[swap].Down = temp;
            onlyDownExits[swap].DownByte = tempByte;
        }

        for (int i = 0; i < downExits.Count; i++)
        {
            int swap = r.Next(i, downExits.Count);
            Room temp = downExits[i].Down;
            Room down1 = downExits[swap].Down;
            temp.Up = downExits[swap];
            down1.Up = downExits[i];
            downExits[i].Down = down1;
            downExits[swap].Down = temp;

            int tempByte = downExits[i].DownByte;
            downExits[i].DownByte = downExits[swap].DownByte;
            downExits[swap].DownByte = tempByte;

            tempByte = temp.UpByte;
            temp.UpByte = down1.UpByte;
            down1.UpByte = tempByte;
        }

        for (int i = 0; i < leftExits.Count; i++)
        {
            int swap = r.Next(i, leftExits.Count);
            Room temp = leftExits[i].Left;
            Room down1 = leftExits[swap].Left;
            temp.Right = leftExits[swap];
            down1.Right = leftExits[i];
            leftExits[i].Left = down1;
            leftExits[swap].Left = temp;

            int tempByte = leftExits[i].LeftByte;
            leftExits[i].LeftByte = leftExits[swap].LeftByte;
            leftExits[swap].LeftByte = tempByte;

            tempByte = temp.RightByte;
            temp.RightByte = down1.RightByte;
            down1.RightByte = tempByte;
        }

        for (int i = 0; i < rightExits.Count; i++)
        {
            int swap = r.Next(i, rightExits.Count);
            Room temp = rightExits[i].Right;
            Room down1 = rightExits[swap].Right;
            temp.Left = rightExits[swap];
            down1.Left = rightExits[i];
            rightExits[i].Right = down1;
            rightExits[swap].Right = temp;

            int tempByte = rightExits[i].RightByte;
            rightExits[i].RightByte = rightExits[swap].RightByte;
            rightExits[swap].RightByte = tempByte;

            tempByte = temp.LeftByte;
            temp.LeftByte = down1.LeftByte;
            down1.LeftByte = tempByte;
        }
        if (Number == 6)
        {
            foreach (Room room in onlyDownExits)
            {
                if (room.Down.Map != 0xBC)
                {
                    int db = room.DownByte;
                    room.DownByte = (db & 0xFC) + 1;
                }
                else
                {
                    int db = room.DownByte;
                    room.DownByte = (db & 0xFC) + 2;
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
        foreach (Room r in allRooms)
        {
            r.UpdateBytes();
            for (int i = 0; i < 4; i++)
            {
                if (r.Connections[i] < 0xFC)
                {
                    ROMData.Put(r.MemAddr + i, r.Connections[i]);
                }
            }
        }
    }

    public void CreateTree(bool removeTbird)
    {
        foreach (Room r in allRooms)
        {
            if (rooms.ContainsKey(r.Map * 4))
            {
                List<Room> l = rooms[r.Map * 4];
                l.Add(r);
                rooms.Remove(r.Map * 4);
                rooms.Add(r.Map * 4, l);
            }
            else
            {
                List<Room> l = new List<Room> { r };
                rooms.Add(r.Map * 4, l);
            }
            SortRoom(r);
        }
        foreach (Room r in allRooms)
        {
            if (r.Left == null && (r.HasLeftExit()))
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

            if (r.Right == null && (r.HasRightExit()))
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

            if (r.Up == null && (r.HasUpExit()))
            {
                List<Room> l = rooms[r.UpByte & 0xFC];
                foreach (Room r2 in l)
                {
                    if ((r2.DownByte & 0xFC) / 4 == r.Map)
                    {
                        r.Up = r2;
                    }
                }
            }

            if (r.Down == null && (r.HasDownExit()))
            {
                List<Room> l = rooms[r.DownByte & 0xFC];
                foreach (Room r2 in l)
                {
                    if (r2.Map == (r.DownByte & 0xFC) / 4)
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
            leftExits.Remove(Tbird);
            rightExits.Remove(Tbird);
            allRooms.Remove(Tbird);
        }
    }

    public void Shorten(Random random)
    {
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

            if (onlyDownExits.Contains(remove) || remove.Map == PalaceRooms.Thunderbird(useCustomRooms).Map || remove == bossRoom)
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

                if (hasLeft && hasRight && (onlyDownExits[0].Down != remove && onlyDownExits[1].Down != remove && onlyDownExits[2].Down != remove && onlyDownExits[3].Down != remove))
                {
                    remove.Left.Right = remove.Right;
                    remove.Right.Left = remove.Left;
                    remove.Left.RightByte = remove.RightByte;
                    remove.Right.LeftByte = remove.LeftByte;
                    rooms--;
                    //logger.WriteLine("removed 1 room");
                    leftExits.Remove(remove);
                    rightExits.Remove(remove);
                    allRooms.Remove(remove);
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
                    allRooms.Remove(remove);
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
                        allRooms.Remove(remove);
                        allRooms.Remove(remove.Left);
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
                        allRooms.Remove(remove);
                        allRooms.Remove(remove.Right);
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

                        if (remove.Left.Down == null || onlyDownExits.Contains(remove.Left))
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
                        allRooms.Remove(remove);
                        allRooms.Remove(remove.Left);
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

                        if (remove.Right.Down == null || onlyDownExits.Contains(remove.Right))
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
                        allRooms.Remove(remove);
                        allRooms.Remove(remove.Right);
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

    public void ShuffleSmallItems(int world, bool first, Random r, bool shuffleSmallItems, bool extraKeys, bool newMap, ROM ROMData)
    {
        List<int> addresses = new List<int>();
        List<int> items = new List<int>();
        int startAddr;
        if (first)
        {
            startAddr = 0x8523 - 0x8000 + (world * 0x4000) + 0x10;
        }
        else
        {
            startAddr = 0xA000 - 0x8000 + (world * 0x4000) + 0x10;
        }
        
        foreach (Room room in allRooms)
        {
            int i = startAddr + (room.Map * 2);
            if(newMap)
            {
                i = startAddr + (room.NewMap * 2);
            }
            int low = ROMData.GetByte(i);
            int hi = ROMData.GetByte(i + 1) * 256;
            int numBytes = ROMData.GetByte(hi + low + 16 - 0x8000 + (world * 0x4000));
            for (int j = 4; j < numBytes; j = j + 2)
            {
                int yPos = ROMData.GetByte(hi + low + j + 16 - 0x8000 + (world * 0x4000)) & 0xF0;
                yPos = yPos >> 4;
                if (ROMData.GetByte(hi + low + j + 1 + 16 - 0x8000 + (world * 0x4000)) == 0x0F && yPos < 13)
                {
                    int addr = hi + low + j + 2 + 16 - 0x8000 + (world * 0x4000);
                    int item = ROMData.GetByte(addr);
                    if (item == 8 || (item > 9 && item < 14) || (item > 15 && item < 19) && !addresses.Contains(addr))
                    {
                        addresses.Add(addr);
                        items.Add(item);
                    }

                    j++;
                }
            }
        }
        for (int i = 0; i < items.Count; i++)
        {
            int swap = r.Next(i, items.Count);
            int temp = items[swap];
            items[swap] = items[i];
            items[i] = temp;
        }
        for (int i = 0; i < addresses.Count; i++)
        {
            if (shuffleSmallItems)
            {
                ROMData.Put(addresses[i], (Byte)items[i]);
            }

            if (extraKeys && Number != 7)
            {
                ROMData.Put(addresses[i], (Byte)0x08);
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
}
