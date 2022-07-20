using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    class Room
    {
        private int map;
        private Byte[] connections;
        private Byte[] enemies;
        private Byte[] sideView;
        private int leftByte;
        private int rightByte;
        private int upByte;
        private int downByte;
        private Room left;
        private Room right;
        private Room up;
        private Room down;
        private Boolean isRoot;
        private Boolean isReachable;
        private Boolean beforeTbird;
        private int memAddr;
        private Boolean isDeadEnd;
        private Boolean isPlaced;
        private Boolean udRev;
        private Boolean fairyBlocked;
        private Boolean upstabBlocked;
        private Boolean downstabBlocked;
        private Boolean gloveBlocked;
        private Boolean jumpBlocked;
        private Boolean hasItem;
        private Boolean hasBoss;
        private Boolean hasDrop;
        private int elevatorScreen;
        private Byte bitmask;
        private int numExits;
        private bool dropZone;
        private int newmap;

        private readonly int enemyPtr1 = 0x105B1;
        private readonly int enemyPtr2 = 0x1208E;
        private readonly int enemyPtr3 = 0x145B1;
        private readonly int sideview1 = 0x10533;
        private readonly int sideview2 = 0x12010;
        private readonly int sideview3 = 0x14533;
        private readonly int bitmask1 = 0x17ba5;
        private readonly int bitmask2 = 0x17bc5;
        private readonly int bitmask3 = 0x17be5;
        private readonly int connectors1 = 0x1072b;
        private readonly int connectors2 = 0x12208;
        private readonly int connectors3 = 0x1472b;

        public int Map
        {
            get
            {
                return map;
            }

            set
            {
                map = value;
            }
        }

        public bool IsRoot
        {
            get
            {
                return isRoot;
            }

            set
            {
                isRoot = value;
            }
        }

        public Room Left
        {
            get
            {
                return left;
            }

            set
            {
                left = value;
            }
        }

        public Room Right
        {
            get
            {
                return right;
            }

            set
            {
                right = value;
            }
        }

        public Room Up
        {
            get
            {
                if(udRev)
                {
                    return down;
                }
                return up;
            }

            set
            {
                if(udRev)
                {
                    down = value;
                    return;
                }
                up = value;
            }
        }

        public Room Down
        {
            get
            {
                if(udRev)
                {
                    return up;
                }
                return down;
            }

            set
            {
                if(udRev)
                {
                    up = value;
                    return;
                }
                down = value;
            }
        }

        public bool IsReachable
        {
            get
            {
                return isReachable;
            }

            set
            {
                isReachable = value;
            }
        }

        public int MemAddr
        {
            get
            {
                return memAddr;
            }

            set
            {
                memAddr = value;
            }
        }

        public byte[] Connections
        {
            get
            {
                return connections;
            }

            set
            {
                connections = value;
            }
        }









        public bool IsDeadEnd
        {
            get
            {
                return isDeadEnd;
            }

            set
            {
                isDeadEnd = value;
            }
        }

        public bool IsPlaced
        {
            get
            {
                return isPlaced;
            }

            set
            {
                isPlaced = value;
            }
        }

        public int LeftByte
        {
            get
            {
                return leftByte;
            }

            set
            {
                leftByte = value;
            }
        }

        public int RightByte
        {
            get
            {
                return rightByte;
            }

            set
            {
                rightByte = value;
            }
        }

        public int UpByte
        {
            get
            {
                if(udRev)
                {
                    return downByte;
                }
                return upByte;
            }

            set
            {
                if(udRev)
                {
                    downByte = value;
                    return;
                }
                upByte = value;
            }
        }

        public int DownByte
        {
            get
            {
                if(udRev)
                {
                    return upByte;
                }
                return downByte;
            }

            set
            {
                if(udRev)
                {
                    upByte = value;
                    return;
                }
                downByte = value;
            }
        }

        public bool BeforeTbird
        {
            get
            {
                return beforeTbird;
            }

            set
            {
                beforeTbird = value;
            }
        }

        public bool HasDrop { get => hasDrop; set => hasDrop = value; }
        public int ElevatorScreen { get => elevatorScreen; set => elevatorScreen = value; }
        public bool FairyBlocked { get => fairyBlocked; set => fairyBlocked = value; }
        public bool UpstabBlocked { get => upstabBlocked; set => upstabBlocked = value; }
        public bool DownstabBlocked { get => downstabBlocked; set => downstabBlocked = value; }
        public bool GloveBlocked { get => gloveBlocked; set => gloveBlocked = value; }
        public bool JumpBlocked { get => jumpBlocked; set => jumpBlocked = value; }
        public bool DropZone { get => dropZone; set => dropZone = value; }
        public byte[] Enemies { get => enemies; set => enemies = value; }
        public byte[] SideView { get => sideView; set => sideView = value; }
        public int Newmap { get => newmap; set => newmap = value; }
        public bool HasBoss { get => hasBoss; set => hasBoss = value; }

        public Room(int map, Byte[] conn, Byte[] enemies, Byte[] sideview, Byte bitmask, Boolean fairyBlocked, Boolean gloveBlocked, Boolean downstabBlocked, Boolean upstabBlocked, Boolean jumpBlocked, Boolean hasItem, Boolean hasBoss, Boolean hasDrop, int elevatorScreen, int memAddr, bool upDownRev, bool dropZone)
        {
            this.map = map;
            connections = conn;
            this.enemies = enemies;
            this.sideView = sideview;
            this.gloveBlocked = gloveBlocked;
            this.downstabBlocked = downstabBlocked;
            this.upstabBlocked = upstabBlocked;
            this.fairyBlocked = fairyBlocked;
            this.hasBoss = hasBoss;
            this.hasItem = hasItem;
            leftByte = conn[0];
            downByte = conn[1];
            upByte = conn[2];
            rightByte = conn[3];
            isRoot = false;
            isReachable = false;
            MemAddr = memAddr;
            isPlaced = false;
            left = null;
            right = null;
            up = null;
            down = null;
            beforeTbird = false;
            udRev = upDownRev;
            this.bitmask = bitmask;
            this.hasDrop = hasDrop;
            this.elevatorScreen = elevatorScreen;
            int numExits = 0;
            foreach(int con in connections)
            {
                if(con < 0xFC && con > 3)
                {
                    numExits++;
                }
            }
            this.numExits = numExits;
            this.isDeadEnd = numExits == 1;
            this.dropZone = dropZone;
        }

        public void updateBytes()
        {
            connections[0] = (Byte)leftByte;
            connections[1] = (Byte)downByte;
            connections[2] = (Byte)upByte;
            connections[3] = (Byte)rightByte;
        }

        public void writeSideViewPtr(int addr, int palSet, ROM ROMData)
        {
            if(palSet == 1)
            {
                int memAddr = addr - 0x8010;
                ROMData.Put(sideview1 + Newmap * 2, (byte)(memAddr & 0x00FF));
                ROMData.Put(sideview1 + Newmap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
            }
            else if(palSet == 2)
            {
                int memAddr = addr - 0x8010;
                ROMData.Put(sideview2 + Newmap * 2, (byte)(memAddr & 0x00FF));
                ROMData.Put(sideview2 + Newmap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
            }
            else
            {
                int memAddr = addr - 0xC010;
                if(addr > 0x1f310)
                {
                    memAddr = addr - 0x10010;
                }
                ROMData.Put(sideview3 + Newmap * 2, (byte)(memAddr & 0x00FF));
                ROMData.Put(sideview3 + Newmap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));

            }
        }

        public void updateEnemies(int addr, int palSet, ROM ROMData)
        {
            if(enemies.Length > 1 && palSet == 2)
            {
                for(int i = 2; i < enemies.Length; i += 2)
                {
                    if((enemies[i] & 0x3F) == 0x0A && !hasBoss && !hasItem)
                    {
                        enemies[i] = (byte)(0x0F + (enemies[i] & 0xC0));
                    }
                }
            }
            ROMData.Put(addr, enemies);
            if (palSet == 1)
            {
                int memAddr = addr - 0x98b0;
                ROMData.Put(enemyPtr1 + Newmap * 2, (byte)(memAddr & 0x00FF));
                ROMData.Put(enemyPtr1 + Newmap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
            }
            else if (palSet == 2)
            {
                int memAddr = addr - 0x98b0;
                ROMData.Put(enemyPtr2 + Newmap * 2, (byte)(memAddr & 0x00FF));
                ROMData.Put(enemyPtr2 + Newmap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
            }
            else
            {
                int memAddr = addr - 0xd8b0;
                ROMData.Put(enemyPtr3 + Newmap * 2, (byte)(memAddr & 0x00FF));
                ROMData.Put(enemyPtr3 + Newmap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));

            }
        }

        public void updateBitmask(int palSet, ROM ROMData)
        {
            int ptr = bitmask1;

            if (palSet == 2)
            {
                ptr = bitmask2;
            }
            else if(palSet == 3)
            {
                ptr = bitmask3;
            }
            if(Newmap % 2 == 0)
            {
                byte old = ROMData.GetByte(ptr + newmap / 2);
                old = (byte)(old & 0x0F);
                old = (byte)((bitmask << 4) | old);
                ROMData.Put(ptr + newmap / 2, old);
            }
            else
            {
                byte old = ROMData.GetByte(ptr + newmap / 2);
                old = (byte)(old & 0xF0);
                old = (byte)((bitmask) | old);
                ROMData.Put(ptr + newmap / 2, old);
            }
        }

        public void setItem(Items it)
        {
            for(int i = 4; i < sideView.Length; i+=2)
            {
                int yPos = sideView[i] & 0xF0;
                yPos = yPos >> 4;
                if(yPos < 13 && sideView[i+1] == 0x0F)
                {
                    sideView[i + 2] = (byte)it;
                    return;
                }
            }
        }

        public void updateItem(Items i, int palSet, ROM ROMData)
        {
            int sideViewPtr = (ROMData.GetByte(sideview1 + newmap * 2) + (ROMData.GetByte(sideview1 + 1 + newmap * 2) << 8)) + 0x8010;

            if (palSet == 2)
            {
                sideViewPtr = (ROMData.GetByte(sideview2 + newmap * 2) + (ROMData.GetByte(sideview2 + 1 + newmap * 2) << 8)) + 0x8010;
            }
            int ptr = sideViewPtr + 4;
            byte data = ROMData.GetByte(ptr);
            data = (byte)(data & 0xF0);
            data = (byte)(data >> 4);
            byte data2 = ROMData.GetByte(ptr+1);
            while (data >= 13 || data2 != 0x0F)
            {
                ptr+=2;
                data = ROMData.GetByte(ptr);
                data = (byte)(data & 0xF0);
                data = (byte)(data >> 4);
                data2 = ROMData.GetByte(ptr + 1);
            }
            ROMData.Put(ptr + 2, (byte)i);

        }


        public void updateConnectors(int palSet, ROM ROMData, bool entrance)
        {
            this.updateBytes();
            if(palSet == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (connections[i] < 0xFC || entrance)
                    {
                        ROMData.Put(connectors1 + newmap * 4 + i, connections[i]);
                    }
                }
            }
            else if(palSet == 2)
            {
                for(int i = 0; i < 4; i++)
                {
                    if (connections[i] < 0xFC || entrance)
                    {
                        ROMData.Put(connectors2 + newmap * 4 + i, connections[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (connections[i] < 0xFC || entrance)
                    {
                        ROMData.Put(connectors3 + newmap * 4 + i, connections[i]);
                    }
                }
            }
        }

        public void dump()
        {
            Console.Write("new Room(" + map + ", " + "new Byte[] {" );
            
            for (int i = 0; i < connections.Length; i++)
            {
                Console.Write("0x{0:X}", connections[i]);
                if (i != connections.Length - 1)
                {
                    Console.Write(", ");
                }
            }
            Console.Write("}, new Byte[] { ");
            for (int i = 0; i < enemies.Length; i++)
            {
                Console.Write("0x{0:X}", enemies[i]);
                if(i != enemies.Length - 1)
                {
                    Console.Write(", ");
                }
            }
            Console.Write("}, new Byte[] { ");
            for (int i = 0; i < sideView.Length; i++)
            {
                Console.Write("0x{0:X}", sideView[i]);
                if (i != sideView.Length - 1)
                {
                    Console.Write(", ");
                }
            }
            Console.Write("}, ");
            Console.Write("0x{0:X}", bitmask); 
            Console.Write(", " + fairyBlocked.ToString().ToLower() + ", " + gloveBlocked.ToString().ToLower() + ", " + downstabBlocked.ToString().ToLower() + ", " + upstabBlocked.ToString().ToLower() + ", " + jumpBlocked.ToString().ToLower() + ", " + hasItem.ToString().ToLower() + ", " + hasBoss.ToString().ToLower() + ", " + hasDrop.ToString().ToLower() + ", " + elevatorScreen + ", ");
            Console.Write("0x{0:X}", memAddr);
            Console.WriteLine(", " + udRev.ToString().ToLower() + ", " + dropZone.ToString().ToLower() + "),");

        }

        public bool hasUpExit()
        {
            if (!udRev)
            {
                return (upByte < 0xFC && upByte > 0x03) || (map == 4 && upByte == 0x02) || (map == 1 && upByte == 0x02) || (map == 2 && upByte == 0x03);
            }
            else
            {
                return (downByte < 0xFC && downByte > 0x03);
            }
        }

        public bool hasDownExit()
        {
            if (!udRev)
            {
                return (downByte < 0xFC && downByte > 0x03);
            }
            else
            {
                return (upByte < 0xFC && upByte > 0x03);
            }
        }

        public bool hasLeftExit()
        {

            return (leftByte < 0xFC && leftByte > 0x03);

        }

        public bool hasRightExit()
        {

            return (rightByte < 0xFC && rightByte > 0x03);

        }

        public int getOpenExits()
        {
            int exits = 0;
            if(hasRightExit() && Right == null)
            {
                exits++;
            }

            if (hasLeftExit() && Left == null)
            {
                exits++;
            }

            if (hasUpExit() && Up == null)
            {
                exits++;
            }

            if (hasDownExit() && Down == null)
            {
                exits++;
            }
            return exits;
        }

        public Room deepCopy()
        {
            return new Room(Map, (Byte[])Connections.Clone(), (Byte[])Enemies.Clone(), (Byte[])SideView.Clone(), bitmask, FairyBlocked, GloveBlocked, downstabBlocked, UpstabBlocked, JumpBlocked, hasItem, hasBoss, HasDrop, ElevatorScreen, MemAddr, udRev, DropZone);
        }
    }
}
