using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Z2Randomizer.Sidescroll;

public class Room
{
    private readonly Logger logger = LogManager.GetCurrentClassLogger();

    //private int map;
    //private byte[] Connections;
    //private byte[] Enemies;
    //private byte[] sideView;
    //private int leftByte;
    //private int rightByte;
    private int upByte;
    private int downByte;
    //private Room left;
    //private Room right;
    private Room up;
    private Room down;
    //private bool isRoot;
    //private bool isReachable;
    //private bool beforeTbird;
    //private int memAddr;
    //private bool isDeadEnd;
    //private bool isPlaced;
    private bool isUpDownReversed;
    //private bool fairyBlocked;
    //private bool upstabBlocked;
    //private bool downstabBlocked;
    //private bool gloveBlocked;
    //private bool jumpBlocked;
    //private bool HasItem;
    //private bool HasBoss;
    //private bool hasDrop;
    //private int ElevatorScreen;
    //private bool dropZone;
    //private int Newmap;

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

    private byte bitmask;

    public int Map { get; set; }

    public bool IsRoot { get; set; }
    public Room Left { get; set; }
    public Room Right { get; set; }
    public Room Up
    {
        get
        {
            if (isUpDownReversed)
            {
                return down;
            }
            return up;
        }

        set
        {
            if (isUpDownReversed)
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
            if (isUpDownReversed)
            {
                return up;
            }
            return down;
        }

        set
        {
            if (isUpDownReversed)
            {
                up = value;
                return;
            }
            down = value;
        }
    }
    public bool IsReachable { get; set; }
    public int MemAddr { get; set; }
    public byte[] Connections { get; set; }
    public bool IsDeadEnd { get; set; }
    public bool IsPlaced { get; set; }  
    public int LeftByte { get; set; }
    public int RightByte { get; set; }
    public int UpByte
    {
        get
        {
            if(isUpDownReversed)
            {
                return downByte;
            }
            return upByte;
        }

        set
        {
            if(isUpDownReversed)
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
            if(isUpDownReversed)
            {
                return upByte;
            }
            return downByte;
        }

        set
        {
            if(isUpDownReversed)
            {
                upByte = value;
                return;
            }
            downByte = value;
        }
    }

    public bool IsBeforeTbird { get; set; }

    public bool HasDrop { get; set; }
    public int ElevatorScreen { get; set; }
    public bool IsFairyBlocked { get; set; }
    public bool IsUpstabBlocked { get; set; }
    public bool IsDownstabBlocked { get; set; }
    public bool IsGloveBlocked { get; set; }
    public bool IsJumpBlocked { get; set; }
    public bool IsDropZone { get; set; }
    public bool HasItem { get; set; }
    public byte[] Enemies { get; set; }
    public byte[] SideView { get; set; }
    public int NewMap { get; set; }
    public bool HasBoss { get; set; }
    public string Name { get; set; }
    public string Group { get; set; }
    public bool Enabled { get; set; }

    public Room(
        int map, 
        byte[] conn, 
        byte[] enemies, 
        byte[] sideview, 
        byte bitmask, 
        bool fairyBlocked, 
        bool gloveBlocked, 
        bool downstabBlocked, 
        bool upstabBlocked, 
        bool jumpBlocked, 
        bool hasItem, 
        bool hasBoss, 
        bool hasDrop, 
        int elevatorScreen, 
        int memAddr, 
        bool upDownRev, 
        bool dropZone)
    {
        Map = map;
        Connections = conn;
        Enemies = enemies;
        SideView = sideview;
        IsGloveBlocked = gloveBlocked;
        IsDownstabBlocked = downstabBlocked;
        IsUpstabBlocked = upstabBlocked;
        IsFairyBlocked = fairyBlocked;
        IsJumpBlocked = jumpBlocked;
        HasBoss = hasBoss;
        HasItem = hasItem;
        LeftByte = conn[0];
        DownByte = conn[1];
        UpByte = conn[2];
        RightByte = conn[3];
        IsRoot = false;
        IsReachable = false;
        MemAddr = memAddr;
        IsPlaced = false;
        Left = null;
        Right = null;
        Up = null;
        Down = null;
        IsBeforeTbird = false;
        isUpDownReversed = upDownRev;
        this.bitmask = bitmask;
        HasDrop = hasDrop;
        ElevatorScreen = elevatorScreen;
        int numExits = 0;
        foreach (int con in Connections)
        {
            if (con < 0xFC && con > 3)
            {
                numExits++;
            }
        }
        //this.numExits = numExits;
        IsDeadEnd = numExits == 1;
        IsDropZone = dropZone;
    }

    public Room(string json)
    {
        dynamic roomData = JsonConvert.DeserializeObject(json);
        Name = roomData.name;
        Group = roomData.group;
        Map = roomData.map;
        Enabled = (bool)roomData.enabled;
        Connections = Convert.FromHexString(roomData.connections.ToString());
        Enemies = Convert.FromHexString(roomData.enemies.ToString());
        SideView = Convert.FromHexString(roomData.sideviewData.ToString());
        bitmask = Convert.FromHexString(roomData.bitmask.ToString())[0];
        IsFairyBlocked = roomData.isFairyBlocked;
        IsGloveBlocked = roomData.isGloveBlocked;
        IsDownstabBlocked = roomData.isDownstabBlocked;
        IsUpstabBlocked = roomData.isUpstabBlocked;
        IsJumpBlocked = roomData.isJumpBlocked;
        HasItem = roomData.hasItem;
        HasBoss = roomData.hasBoss;
        HasDrop = roomData.hasDrop;
        ElevatorScreen = roomData.elevatorScreen;
        MemAddr = Convert.ToInt32("0x" + roomData.memoryAddress, 16);
        isUpDownReversed = roomData.isUpDownReversed;
        IsDropZone = roomData.isDropZone;

        byte length = Convert.FromHexString(roomData.sideviewData.ToString())[0];
        if(SideView.Length != length)
        {
            throw new Exception("Room length header does not match actual length for sideview: " + roomData.sideviewData.ToString());
        }
    }

    public string Serialize()
    {
        dynamic result = new ExpandoObject();
        result.name = Name;
        result.group = Group;
        result.map = Map;
        result.enabled = Enabled;
        result.connections = BitConverter.ToString(Connections).Replace("-", "");
        result.enemies = BitConverter.ToString(Enemies).Replace("-", "");
        result.sideviewData = BitConverter.ToString(SideView).Replace("-", "");
        result.bitmask = BitConverter.ToString(new Byte[] { bitmask }).Replace("-", "");
        result.isFairyBlocked = IsFairyBlocked;
        result.isGloveBlocked = IsGloveBlocked;
        result.isDownstabBlocked = IsDownstabBlocked;
        result.isUpstabBlocked = IsUpstabBlocked;
        result.isJumpBlocked = IsJumpBlocked;
        result.hasItem = HasItem;
        result.hasBoss = HasBoss;
        result.hasDrop = HasDrop;
        result.elevatorScreen = ElevatorScreen;
        result.MemAddr = MemAddr;
        result.isUpDownReversed = isUpDownReversed;
        result.IsDropZone = IsDropZone;

        return System.Text.Json.JsonSerializer.Serialize(result);
    }

    public void UpdateBytes()
    {
        Connections[0] = (Byte)LeftByte;
        Connections[1] = (Byte)downByte;
        Connections[2] = (Byte)upByte;
        Connections[3] = (Byte)RightByte;
    }

    public void WriteSideViewPtr(int addr, int palSet, ROM ROMData)
    {
        if(palSet == 1)
        {
            int memAddr = addr - 0x8010;
            ROMData.Put(sideview1 + NewMap * 2, (byte)(memAddr & 0x00FF));
            ROMData.Put(sideview1 + NewMap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
        }
        else if(palSet == 2)
        {
            int memAddr = addr - 0x8010;
            ROMData.Put(sideview2 + NewMap * 2, (byte)(memAddr & 0x00FF));
            ROMData.Put(sideview2 + NewMap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
        }
        else
        {
            int memAddr = addr - 0xC010;
            if(addr > 0x1f310)
            {
                memAddr = addr - 0x10010;
            }
            ROMData.Put(sideview3 + NewMap * 2, (byte)(memAddr & 0x00FF));
            ROMData.Put(sideview3 + NewMap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));

        }
    }

    public void UpdateEnemies(int addr, int palSet, ROM ROMData)
    {
        if(Enemies.Length > 1 && palSet == 2)
        {
            for(int i = 2; i < Enemies.Length; i += 2)
            {
                if((Enemies[i] & 0x3F) == 0x0A && !HasBoss && !HasItem)
                {
                    Enemies[i] = (byte)(0x0F + (Enemies[i] & 0xC0));
                }
            }
        }
        ROMData.Put(addr, Enemies);
        if (palSet == 1)
        {
            int memAddr = addr - 0x98b0;
            ROMData.Put(enemyPtr1 + NewMap * 2, (byte)(memAddr & 0x00FF));
            ROMData.Put(enemyPtr1 + NewMap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
        }
        else if (palSet == 2)
        {
            int memAddr = addr - 0x98b0;
            ROMData.Put(enemyPtr2 + NewMap * 2, (byte)(memAddr & 0x00FF));
            ROMData.Put(enemyPtr2 + NewMap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
        }
        else
        {
            int memAddr = addr - 0xd8b0;
            ROMData.Put(enemyPtr3 + NewMap * 2, (byte)(memAddr & 0x00FF));
            ROMData.Put(enemyPtr3 + NewMap * 2 + 1, (byte)((memAddr >> 8) & 0xFF));

        }
    }

    public void UpdateBitmask(int palSet, ROM ROMData)
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
        if(NewMap % 2 == 0)
        {
            byte old = ROMData.GetByte(ptr + NewMap / 2);
            old = (byte)(old & 0x0F);
            old = (byte)((bitmask << 4) | old);
            ROMData.Put(ptr + NewMap / 2, old);
        }
        else
        {
            byte old = ROMData.GetByte(ptr + NewMap / 2);
            old = (byte)(old & 0xF0);
            old = (byte)((bitmask) | old);
            ROMData.Put(ptr + NewMap / 2, old);
        }
    }

    public void SetItem(Item it)
    {
        for(int i = 4; i < SideView.Length; i+=2)
        {
            int yPos = SideView[i] & 0xF0;
            yPos >>= 4;
            if(yPos < 13 && SideView[i+1] == 0x0F)
            {
                SideView[i + 2] = (byte)it;
                return;
            }
        }
    }

    public void UpdateItem(Item i, int palSet, ROM ROMData)
    {
        int sideViewPtr = (ROMData.GetByte(sideview1 + NewMap * 2) + (ROMData.GetByte(sideview1 + 1 + NewMap * 2) << 8)) + 0x8010;

        if (palSet == 2)
        {
            sideViewPtr = (ROMData.GetByte(sideview2 + NewMap * 2) + (ROMData.GetByte(sideview2 + 1 + NewMap * 2) << 8)) + 0x8010;
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


    public void UpdateConnectors(int palSet, ROM ROMData, bool entrance)
    {
        this.UpdateBytes();
        if(palSet == 1)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Connections[i] < 0xFC || entrance)
                {
                    ROMData.Put(connectors1 + NewMap * 4 + i, Connections[i]);
                }
            }
        }
        else if(palSet == 2)
        {
            for(int i = 0; i < 4; i++)
            {
                if (Connections[i] < 0xFC || entrance)
                {
                    ROMData.Put(connectors2 + NewMap * 4 + i, Connections[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                if (Connections[i] < 0xFC || entrance)
                {
                    ROMData.Put(connectors3 + NewMap * 4 + i, Connections[i]);
                }
            }
        }
    }

    /*
    public void Dump()
    {
        Console.Write("new Room(" + map + ", " + "new byte[] {" );
        
        for (int i = 0; i < Connections.Length; i++)
        {
            Console.Write("0x{0:X}", Connections[i]);
            if (i != Connections.Length - 1)
            {
                Console.Write(", ");
            }
        }
        Console.Write("}, new byte[] { ");
        for (int i = 0; i < Enemies.Length; i++)
        {
            Console.Write("0x{0:X}", Enemies[i]);
            if(i != Enemies.Length - 1)
            {
                Console.Write(", ");
            }
        }
        Console.Write("}, new byte[] { ");
        for (int i = 0; i < SideView.Length; i++)
        {
            Console.Write("0x{0:X}", sideView[i]);
            if (i != SideView.Length - 1)
            {
                Console.Write(", ");
            }
        }
        Console.Write("}, ");
        Console.Write("0x{0:X}", bitmask); 
        Console.Write(", " + fairyBlocked.ToString().ToLower() + ", " + gloveBlocked.ToString().ToLower() + ", " + downstabBlocked.ToString().ToLower() + ", " + upstabBlocked.ToString().ToLower() + ", " + jumpBlocked.ToString().ToLower() + ", " + HasItem.ToString().ToLower() + ", " + HasBoss.ToString().ToLower() + ", " + hasDrop.ToString().ToLower() + ", " + elevatorScreen + ", ");
        Console.Write("0x{0:X}", memAddr);
        logger.WriteLine(", " + isUpDownReversed.ToString().ToLower() + ", " + dropZone.ToString().ToLower() + "),");
    }
    */

    public bool HasUpExit()
    {
        if (!isUpDownReversed)
        {
            return (upByte < 0xFC && upByte > 0x03) || (Map == 4 && upByte == 0x02) || (Map == 1 && upByte == 0x02) || (Map == 2 && upByte == 0x03);
        }
        else
        {
            return (downByte < 0xFC && downByte > 0x03);
        }
    }

    public bool HasDownExit()
    {
        if (!isUpDownReversed)
        {
            return (downByte < 0xFC && downByte > 0x03);
        }
        else
        {
            return (upByte < 0xFC && upByte > 0x03);
        }
    }

    public bool HasLeftExit()
    {

        return (LeftByte < 0xFC && LeftByte > 0x03);

    }

    public bool HasRightExit()
    {

        return (RightByte < 0xFC && RightByte > 0x03);

    }

    public int CountOpenExits()
    {
        int exits = 0;
        if(HasRightExit() && Right == null)
        {
            exits++;
        }

        if (HasLeftExit() && Left == null)
        {
            exits++;
        }

        if (HasUpExit() && Up == null)
        {
            exits++;
        }

        if (HasDownExit() && Down == null)
        {
            exits++;
        }
        return exits;
    }

    public Room DeepCopy()
    {
        return new Room(Map, 
            (byte[])Connections.Clone(), 
            (byte[])Enemies.Clone(), 
            (byte[])SideView.Clone(), 
            bitmask, 
            IsFairyBlocked, 
            IsGloveBlocked, 
            IsDownstabBlocked, 
            IsUpstabBlocked, 
            IsJumpBlocked, 
            HasItem, 
            HasBoss, 
            HasDrop, 
            ElevatorScreen, 
            MemAddr, 
            isUpDownReversed, 
            IsDropZone);
    }

    public string Debug()
    {
        StringBuilder sb = new();
        sb.Append("Map: " + NewMap + " Name: " + Name + " Sideview: ");
        sb.Append(BitConverter.ToString(SideView).Replace("-", ""));
        return sb.ToString();
    }

}
