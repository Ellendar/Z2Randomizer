using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Z2Randomizer.Sidescroll;

namespace Z2Randomizer
{
    public class Palace
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private int num;
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
        private ROM ROMData;
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
        internal Room Tbird { get => tbird; set => tbird = value; }
        public bool NeedReflect { get => needReflect; set => needReflect = value; }

        public Palace(int number, int b, int c, ROM ROMData)
        {
            num = number;
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
            baseAddr = b;
            connAddr = c;
            this.ROMData = ROMData;
            needDstab = false;
            needFairy = false;
            needGlove = false;
            needJumpOrFairy = false;
            openRooms = new List<Room>();
            //dumpMaps();
            //createTree();
            if (num < 7)
            {
                netDeadEnds = 3;
            }
            else
            {
                netDeadEnds = 2;
            }
        }

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
                    Byte[] connectBytes = new Byte[4];
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
                    Byte[] sideView = ROMData.GetBytes(sideViewPtr, sideViewPtr + sideViewLength);

                    int enemyPtr = ROMData.GetByte(enemy[j] + i * 2) + (ROMData.GetByte(enemy[j] + 1 + i * 2) << 8) + 0x98b0;
                    if (j == 2)
                    {
                        enemyPtr = ROMData.GetByte(enemy[j] + i * 2) + (ROMData.GetByte(enemy[j] + 1 + i * 2) << 8) + 0xd8b0;
                    }

                    int enemyLength = ROMData.GetByte(enemyPtr);
                    Byte[] enemies = ROMData.GetBytes(enemyPtr, enemyPtr + enemyLength);

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

        public int GetOpenRooms()
        {
            return openRooms.Count;
        }
        public void UpdateBlocks()
        {
            List<Room> itemPath = CheckBlocks();
            foreach (Room r in itemPath)
            {
                if (num == 4 && r == BossRoom)
                {
                    this.NeedReflect = true;
                }
                if (r.FairyBlocked)
                {
                    this.needFairy = true;
                }
                if (r.DownstabBlocked)
                {
                    this.needDstab = true;
                }
                if (r.UpstabBlocked)
                {
                    this.needUstab = true;
                }
                if (r.JumpBlocked)
                {
                    this.needJumpOrFairy = true;
                }
                if (r.GloveBlocked)
                {
                    this.needGlove = true;
                }
            }
        }

        public bool AddRoom(Room r, bool blocker)
        {
            Boolean placed = false;



            if (netDeadEnds > 3 && r.IsDeadEnd)
            {
                return false;
            }

            if (netDeadEnds < -3 && r.getOpenExits() > 2)
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
            foreach (Room open in openRooms)
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
            else if (r.getOpenExits() > 1)
            {
                netDeadEnds--;
            }
            allRooms.Add(r);
            SortRoom(r);
            numRooms++;

            if (num != 7 && openRooms.Count > 1 && itemRoom.getOpenExits() > 0)
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
            if (openRooms.Count > 1 && bossRoom.getOpenExits() > 0)
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
            if (num == 7 && openRooms.Count > 1 && Tbird != null && Tbird.getOpenExits() > 1)
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

        public void UpdateItem(Item i)
        {
            if (num == 1 || num == 2 || num == 5)
            {
                itemRoom.updateItem(i, 1, ROMData);
            }
            else
            {
                itemRoom.updateItem(i, 2, ROMData);
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
                if (num == 1)
                {
                    if (r.FairyBlocked || r.DownstabBlocked || r.UpstabBlocked || r.JumpBlocked || r.GloveBlocked || r.HasDrop || r.DropZone || r.HasBoss)
                    {
                        return false;
                    }
                }

                if (num == 2)
                {
                    if (r.FairyBlocked || r.DownstabBlocked || r.UpstabBlocked || r.HasDrop || r.DropZone || r.HasBoss)
                    {
                        return false;
                    }
                }

                if (num == 3)
                {
                    if (r.JumpBlocked || r.FairyBlocked || r.HasDrop || r.DropZone)
                    {
                        return false;
                    }
                }

                if (num == 4)
                {
                    if (r.GloveBlocked || r.UpstabBlocked || r.DownstabBlocked)
                    {
                        return false;
                    }
                }

                if (num == 5)
                {
                    if (r.GloveBlocked || r.UpstabBlocked || r.DownstabBlocked || r.HasDrop || r.DropZone || r.HasBoss)
                    {
                        return false;
                    }
                }

                if (num == 6)
                {
                    if (r.UpstabBlocked || r.DownstabBlocked)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if ((num == 1 || num == 2 || num == 5 || num == 7) && r.HasBoss)
                {
                    return false;
                }
            }
            return true;
        }

        private bool AttachToOpen(Room r, Room open)
        {
            bool placed = false;
            if (!placed && open.hasRightExit() && open.Right == null && r.hasLeftExit() && r.Left == null)
            {
                open.Right = r;
                open.RightByte = r.Newmap * 4;

                r.Left = open;
                r.LeftByte = open.Newmap * 4 + 3;

                placed = true;
            }

            if (!placed && open.hasLeftExit() && open.Left == null && r.hasRightExit() && r.Right == null)
            {
                open.Left = r;
                open.LeftByte = r.Newmap * 4 + 3;

                r.Right = open;
                r.RightByte = open.Newmap * 4;

                placed = true;
            }

            if (!placed && open.hasUpExit() && open.Up == null && r.hasDownExit() && r.Down == null && !r.HasDrop)
            {
                open.Up = r;
                open.UpByte = r.Newmap * 4 + r.ElevatorScreen;

                r.Down = open;
                r.DownByte = open.Newmap * 4 + open.ElevatorScreen;

                placed = true;
            }

            if (!placed && open.hasDownExit() && !open.HasDrop && open.Down == null && r.hasUpExit() && r.Up == null)
            {

                open.Down = r;
                open.DownByte = r.Newmap * 4 + r.ElevatorScreen;

                r.Up = open;
                r.UpByte = open.Newmap * 4 + open.ElevatorScreen;

                placed = true;
            }

            if (!placed && open.hasDownExit() && open.HasDrop && open.Down == null && r.DropZone)
            {

                open.Down = r;
                open.DownByte = r.Newmap * 4;
                r.DropZone = false;
                placed = true;
            }

            if (!placed && open.DropZone && r.HasDrop && r.Down == null && r.hasDownExit())
            {

                r.Down = open;
                r.DownByte = open.Newmap * 4;
                open.DropZone = false;
                placed = true;
            }

            if (open.getOpenExits() == 0)
            {
                openRooms.Remove(open);
            }
            else if (!openRooms.Contains(open) && (openRooms.Count < 3 || placed))
            {
                openRooms.Add(open);
                placed = true;
            }
            if (r.getOpenExits() == 0)
            {
                openRooms.Remove(r);
            }
            else if (!openRooms.Contains(r) && (openRooms.Count < 3 || placed))
            {
                openRooms.Add(r);
                placed = true;
            }

            return placed;
        }



        public void SortRoom(Room r)
        {
            if (r.hasDownExit())
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

            if (r.hasLeftExit())
            {
                leftExits.Add(r);
            }

            if (r.hasRightExit())
            {
                rightExits.Add(r);
            }

            if (r.hasUpExit())
            {
                upExits.Add(r);
            }
        }

        public Boolean RequiresThunderbird()
        {
            CheckSpecialPaths(root, 2);
            return !bossRoom.BeforeTbird;
        }

        public Boolean HasDeadEnd()
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
            if (!r.BeforeTbird)
            {
                if ((num == 7) && r.Map == PalaceRooms.thunderBird.Map)
                {
                    r.BeforeTbird = true;
                    return;
                }

                r.BeforeTbird = true;
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

        public Boolean AllReachable()
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
        private void CheckPaths(Room r, int dir)
        {
            if (!r.IsPlaced)
            {
                if ((num == 7) && r.Map == PalaceRooms.thunderBird.Map)
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

        private Boolean CanEnterBossFromLeft(Room b)
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

        public void ShuffleRooms(Random R)
        {
            //This method is so ugly and i hate it.
            for (int i = 0; i < upExits.Count; i++)
            {
                int swap = R.Next(i, upExits.Count);
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
                int swap = R.Next(i, onlyDownExits.Count);

                Room temp = onlyDownExits[i].Down;
                int tempByte = onlyDownExits[i].DownByte;

                onlyDownExits[i].Down = onlyDownExits[swap].Down;
                onlyDownExits[i].DownByte = onlyDownExits[swap].DownByte;
                onlyDownExits[swap].Down = temp;
                onlyDownExits[swap].DownByte = tempByte;
            }

            for (int i = 0; i < downExits.Count; i++)
            {
                int swap = R.Next(i, downExits.Count);
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
                int swap = R.Next(i, leftExits.Count);
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
                int swap = R.Next(i, rightExits.Count);
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
            if (num == 6)
            {
                foreach (Room r in onlyDownExits)
                {
                    if (r.Down.Map != 0xBC)
                    {
                        int db = r.DownByte;
                        r.DownByte = (db & 0xFC) + 1;
                    }
                    else
                    {
                        int db = r.DownByte;
                        r.DownByte = (db & 0xFC) + 2;
                    }
                }
            }
        }

        public void SetOpenRoom(Room r)
        {
            openRooms.Add(r);
        }

        public void UpdateRom()
        {
            foreach (Room r in allRooms)
            {
                r.updateBytes();
                for (int i = 0; i < 4; i++)
                {
                    if (r.Connections[i] < 0xFC)
                    {
                        this.ROMData.Put(r.MemAddr + i, r.Connections[i]);
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
                if (r.Left == null && (r.hasLeftExit()))
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

                if (r.Right == null && (r.hasRightExit()))
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

                if (r.Up == null && (r.hasUpExit()))
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

                if (r.Down == null && (r.hasDownExit()))
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

        public void Shorten(Random R)
        {
            int target = R.Next(numRooms / 2, (numRooms * 3) / 4) + 1;
            int rooms = numRooms;
            int tries = 0;
            while (rooms > target && tries < 100000)
            {
                int r = R.Next(rooms);
                Room remove = null;
                if (leftExits.Count < rightExits.Count)
                {
                    remove = rightExits[R.Next(rightExits.Count)];
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

                if (onlyDownExits.Contains(remove) || remove.Map == PalaceRooms.thunderBird.Map || remove == bossRoom)
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

        public void ShuffleSmallItems(int world, bool first, Random R, bool shuffleSmallItems, bool extraKeys, bool newMap)
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
            
            foreach (Room r in allRooms)
            {
                int i = startAddr + (r.Map * 2);
                if(newMap)
                {
                    i = startAddr + (r.Newmap * 2);
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
                int swap = R.Next(i, items.Count);
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

                if (extraKeys && num != 7)
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
                r.BeforeTbird = false;
            }
        }
    }
}
