using Newtonsoft.Json;
using NLog;
using Z2Randomizer.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Dynamic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Drawing;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Z2Randomizer.Core.Sidescroll;

public class Room
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
    public bool isUpDownReversed;
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

    //private const int palace125EnemyPtr = 0x105B1;
    //private const int palace346EnemyPtr = 0x1208E;
    //private const int gpEnemyPtr = 0x145B1;
    private const int sideview1 = 0x10533;
    private const int sideview2 = 0x12010;
    private const int sideview3 = 0x14533;
    private const int bitmask1 = 0x17ba5;
    private const int bitmask2 = 0x17bc5;
    private const int bitmask3 = 0x17be5;
    private const int connectors1 = 0x1072b;
    private const int connectors2 = 0x12208;
    private const int connectors3 = 0x1472b;

    public byte bitmask;

    public int Map { get; set; }
    public int PalaceGroup { get; set; }

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
    public int ConnectionStartAddress { get; set; }
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
    //public bool IsFairyBlocked { get; set; }
    //public bool IsUpstabBlocked { get; set; }
    //public bool IsDownstabBlocked { get; set; }
    //public bool IsGloveBlocked { get; set; }
    //public bool IsJumpBlocked { get; set; }
    [JsonConverter(typeof(RequirementsJsonConverter))]
    public Requirements Requirements { get; set; }
    public bool IsDropZone { get; set; }
    public bool HasItem { get; set; }
    public byte[] Enemies { get; set; }
    public byte[] NewEnemies { get; set; }
    public byte[] SideView { get; set; }
    public int? NewMap { get; set; }
    public bool HasBoss { get; set; }
    public string Name { get; set; }
    public string Group { get; set; }
    public bool Enabled { get; set; }
    public bool IsEntrance { get; set; }

    public Room(
        int map,
        byte[] conn, 
        byte[] enemies, 
        byte[] sideview, 
        byte bitmask, 
        bool hasItem, 
        bool hasBoss, 
        bool hasDrop, 
        int elevatorScreen, 
        int memAddr, 
        bool upDownRev, 
        bool dropZone,
        bool isEntrance,
        Requirements requirements,
        string name
        )
    {
        Map = map;
        Connections = conn;
        Enemies = enemies;
        NewEnemies = new byte[enemies.Length];
        SideView = sideview;
        HasBoss = hasBoss;
        HasItem = hasItem;
        LeftByte = conn[0];
        DownByte = conn[1];
        UpByte = conn[2];
        RightByte = conn[3];
        IsRoot = false;
        IsReachable = false;
        ConnectionStartAddress = memAddr;
        IsPlaced = false;
        /*
        Left = null;
        Right = null;
        Up = null;
        Down = null;
        */
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
        IsEntrance = isEntrance;
        Requirements = requirements;
        Name = name;
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
        HasItem = roomData.hasItem;
        HasBoss = roomData.hasBoss;
        HasDrop = roomData.hasDrop;
        ElevatorScreen = roomData.elevatorScreen;
        //ConnectionStartAddress = Convert.ToInt32("0x" + roomData.memoryAddress, 16);
        ConnectionStartAddress = roomData.memoryAddress;
        isUpDownReversed = roomData.isUpDownReversed;
        IsDropZone = roomData.isDropZone;
        IsEntrance = roomData.isEntrance;
        Requirements = new Requirements(roomData.requirements.ToString());

        byte length = Convert.FromHexString(roomData.sideviewData.ToString())[0];
        if(SideView.Length != length)
        {
            throw new Exception("Room length header does not match actual length for sideview: " + roomData.sideviewData.ToString());
        }
    }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None, new RoomJsonConverter());
    }

    public void UpdateConnectionBytes()
    {
        Connections[0] = (byte)LeftByte;
        Connections[1] = (byte)downByte;
        Connections[2] = (byte)upByte;
        Connections[3] = (byte)RightByte;
    }

    public void WriteSideViewPtr(int addr, ROM ROMData)
    {
        if(PalaceGroup <= 0 || PalaceGroup > 3)
        {
            throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup);
        }
        if(PalaceGroup == 1)
        {
            int memAddr = addr - 0x8010;
            ROMData.Put(sideview1 + (NewMap ?? Map) * 2, (byte)(memAddr & 0x00FF));
            ROMData.Put(sideview1 + (NewMap ?? Map) * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
        }
        else if(PalaceGroup == 2)
        {
            int memAddr = addr - 0x8010;
            ROMData.Put(sideview2 + (NewMap ?? Map) * 2, (byte)(memAddr & 0x00FF));
            ROMData.Put(sideview2 + (NewMap ?? Map) * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
        }
        else
        {
            int memAddr = addr - 0xC010;
            if(addr > 0x1f310)
            {
                memAddr = addr - 0x10010;
            }
            ROMData.Put(sideview3 + (NewMap ?? Map) * 2, (byte)(memAddr & 0x00FF));
            ROMData.Put(sideview3 + (NewMap ?? Map) * 2 + 1, (byte)((memAddr >> 8) & 0xFF));

        }
    }

    public void UpdateEnemies(int enemyAddr, ROM ROMData, PalaceStyle palaceStyle)
    {
        byte[] enemiesToSave = NewEnemies[0] == 0 ? Enemies : NewEnemies;
        int enemyPtr = PalaceGroup switch
        {
            1 => Core.Enemies.Palace125EnemyPtr,
            2 => Core.Enemies.Palace346EnemyPtr,
            3 => Core.Enemies.GPEnemyPtr,
            _ => throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup)
        };

        int baseEnemyAddr = PalaceGroup switch
        {
            1 => Core.Enemies.NormalPalaceEnemyAddr,
            2 => Core.Enemies.NormalPalaceEnemyAddr,
            3 => Core.Enemies.GPEnemyAddr,
            _ => throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup)
        };

        if (enemiesToSave.Length > 1 && PalaceGroup == 2)
        {
            for (int i = 2; i < enemiesToSave.Length; i += 2)
            {
                if ((enemiesToSave[i] & 0x3F) == 0x0A && !HasBoss && !HasItem)
                {
                    enemiesToSave[i] = (byte)(0x0F + (enemiesToSave[i] & 0xC0));
                }
            }
        }

        //Reconstructed palaces require us to rewrite the enemy pointers table.
        if (palaceStyle == PalaceStyle.RECONSTRUCTED)
        {
            int memAddr = enemyAddr;
            //Write the updated pointers
            if (PalaceGroup == 1)
            {
                memAddr -= 0x98b0;
                ROMData.Put(Core.Enemies.Palace125EnemyPtr + (NewMap ?? Map) * 2, (byte)(memAddr & 0x00FF));
                ROMData.Put(Core.Enemies.Palace125EnemyPtr + (NewMap ?? Map) * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
            }
            else if (PalaceGroup == 2)
            {
                memAddr -= 0x98b0;
                ROMData.Put(Core.Enemies.Palace346EnemyPtr + (NewMap ?? Map) * 2, (byte)(memAddr & 0x00FF));
                ROMData.Put(Core.Enemies.Palace346EnemyPtr + (NewMap ?? Map) * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
            }
            else
            {
                memAddr -= 0xd8b0;
                ROMData.Put(Core.Enemies.GPEnemyPtr + (NewMap ?? Map) * 2, (byte)(memAddr & 0x00FF));
                ROMData.Put(Core.Enemies.GPEnemyPtr + (NewMap ?? Map) * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
            }
        }
        //For non-reconstructed, we're not rewriting the sideviews, so we need to use the vanilla
        //non-contiguous enemy data space. We can just read the vanilla enemy pointer and overwrite the
        //vanilla data inline
        else
        {
            int low = ROMData.GetByte(enemyPtr + (NewMap ?? Map) * 2);
            int high = ROMData.GetByte(enemyPtr + (NewMap ?? Map) * 2 + 1);
            high = high << 8;
            high = high & 0x0FFF;
            enemyAddr = high + low + baseEnemyAddr;
        }


        ROMData.Put(enemyAddr, enemiesToSave);
    }

    public void UpdateBitmask(ROM ROMData)
    {
        if (PalaceGroup <= 0 || PalaceGroup > 3)
        {
            throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup);
        }
        int ptr = bitmask1;

        if (PalaceGroup == 2)
        {
            ptr = bitmask2;
        }
        else if(PalaceGroup == 3)
        {
            ptr = bitmask3;
        }
        if(NewMap % 2 == 0)
        {
            byte old = ROMData.GetByte(ptr + (NewMap ?? Map) / 2);
            old = (byte)(old & 0x0F);
            old = (byte)((bitmask << 4) | old);
            ROMData.Put(ptr + (NewMap ?? Map) / 2, old);
        }
        else
        {
            byte old = ROMData.GetByte(ptr + (NewMap ?? Map) / 2);
            old = (byte)(old & 0xF0);
            old = (byte)((bitmask) | old);
            ROMData.Put(ptr + (NewMap ?? Map) / 2, old);
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

    public void UpdateItem(Item i, ROM ROMData)
    {
        if (PalaceGroup <= 0 || PalaceGroup > 3)
        {
            throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup);
        }
        int sideViewPtr = (ROMData.GetByte(sideview1 + (NewMap ?? Map) * 2) + (ROMData.GetByte(sideview1 + 1 + (NewMap ?? Map) * 2) << 8)) + 0x8010;

        if (PalaceGroup == 2)
        {
            sideViewPtr = (ROMData.GetByte(sideview2 + (NewMap ?? Map) * 2) + (ROMData.GetByte(sideview2 + 1 + (NewMap ?? Map) * 2) << 8)) + 0x8010;
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

    public void UpdateConnectors()
    {
        this.UpdateConnectionBytes();
        ConnectionStartAddress = PalaceGroup switch
        {
            1 => connectors1 + (NewMap ?? Map) * 4,
            2 => connectors2 + (NewMap ?? Map) * 4,
            3 => connectors3 + (NewMap ?? Map) * 4,
            _ => throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup)
        };
        //ROMData.Put(connectors1 + (NewMap ?? Map) * 4 + i, Connections[i]);
    }

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
            HasItem, 
            HasBoss, 
            HasDrop, 
            ElevatorScreen, 
            ConnectionStartAddress, 
            isUpDownReversed, 
            IsDropZone,
            IsEntrance,
            Requirements,
            Name);
    }

    public void RandomizeEnemies(bool mixEnemies, bool generatorsAlwaysMatch, Random RNG)
    {
        int[] allEnemies = PalaceGroup switch
        {
            1 => Z2Randomizer.Core.Enemies.Palace125Enemies,
            2 => Z2Randomizer.Core.Enemies.Palace346Enemies,
            3 => Z2Randomizer.Core.Enemies.GPEnemies,
            _ => throw new Exception("Invalid Palace Group: " + PalaceGroup)
        };
        int[] smallEnemies = PalaceGroup switch
        {
            1 => Z2Randomizer.Core.Enemies.Palace125SmallEnemies,
            2 => Z2Randomizer.Core.Enemies.Palace346SmallEnemies,
            3 => Z2Randomizer.Core.Enemies.GPSmallEnemies,
            _ => throw new Exception("Invalid Palace Group: " + PalaceGroup)
        };
        int[] largeEnemies = PalaceGroup switch
        {
            1 => Z2Randomizer.Core.Enemies.Palace125LargeEnemies,
            2 => Z2Randomizer.Core.Enemies.Palace346LargeEnemies,
            3 => Z2Randomizer.Core.Enemies.GPLargeEnemies,
            _ => throw new Exception("Invalid Palace Group: " + PalaceGroup)
        };
        int[] flyingEnemies = PalaceGroup switch
        {
            1 => Z2Randomizer.Core.Enemies.Palace125FlyingEnemies,
            2 => Z2Randomizer.Core.Enemies.Palace346FlyingEnemies,
            3 => Z2Randomizer.Core.Enemies.GPFlyingEnemies,
            _ => throw new Exception("Invalid Palace Group: " + PalaceGroup)
        };
        int[] generators = PalaceGroup switch
        {
            1 => Z2Randomizer.Core.Enemies.Palace125Generators,
            2 => Z2Randomizer.Core.Enemies.Palace346Generators,
            3 => Z2Randomizer.Core.Enemies.GPGenerators,
            _ => throw new Exception("Invalid Palace Group: " + PalaceGroup)
        };

        int? firstGenerator = null;
        int enemiesLength = (int)Enemies[0] - 1;
        NewEnemies[0] = Enemies[0];
        for (int i = 1; i <= enemiesLength; i+=2)
        {
            NewEnemies[i] = Enemies[i];
            int enemyNumber = Enemies[i + 1] & 0x3F;
            int enemyPage = Enemies[i + 1] & 0xC0;
            if (mixEnemies)
            {
                //If this is a shufflable enemy (As opposed to something like a crystal marker)
                if (allEnemies.Contains(enemyNumber))
                {
                    int swap = allEnemies[RNG.Next(0, allEnemies.Length)];
                    int ypos = Enemies[i] & 0xF0;
                    int xpos = Enemies[i] & 0x0F;
                    if (smallEnemies.Contains(enemyNumber) && largeEnemies.Contains(swap))
                    {
                        ypos = ypos - 16;
                        while (swap == 0x1D && ypos != 0x70 && PalaceGroup != 3)
                        {
                            swap = largeEnemies[RNG.Next(0, largeEnemies.Length)];
                        }
                    }
                    else
                    {
                        while (swap == 0x1D && ypos != 0x70 && PalaceGroup != 3)
                        {
                            swap = allEnemies[RNG.Next(0, allEnemies.Length)];
                        }
                    }

                    NewEnemies[i] = (byte)(ypos + xpos);
                    NewEnemies[i + 1] = (byte)(enemyPage + swap);
                }
            }
            else
            {
                if (largeEnemies.Contains(enemyNumber))
                {
                    int swap = RNG.Next(0, largeEnemies.Length);
                    int ypos = Enemies[i] & 0xF0;
                    int xpos = Enemies[i] & 0x0F;
                    while (largeEnemies[swap] == 0x1D && ypos != 0x70 && PalaceGroup != 3)
                    {
                        swap = RNG.Next(0, largeEnemies.Length);
                    }
                    NewEnemies[i + 1] = (byte)(largeEnemies[swap] + enemyPage);
                }
                else if (smallEnemies.Contains(enemyNumber))
                { 
                    int swap = RNG.Next(0, smallEnemies.Length);
                    NewEnemies[i+1] = (byte)(smallEnemies[swap] + enemyPage);
                }
            }


            if (flyingEnemies.Contains(enemyNumber))
            {
                int swap = RNG.Next(0, flyingEnemies.Length);
                while (enemyNumber == 0x07 && (flyingEnemies[swap] == 0x06 || flyingEnemies[swap] == 0x0E))
                {
                    swap = RNG.Next(0, flyingEnemies.Length);
                }
                NewEnemies[i + 1] = (byte)(flyingEnemies[swap] + enemyPage);
            }

            if (generators.Contains(enemyNumber))
            {
                int swap = RNG.Next(0, generators.Length);
                if(firstGenerator == null)
                {
                    firstGenerator = swap;
                }
                if (generatorsAlwaysMatch)
                {
                    NewEnemies[i + 1] = (byte)(generators[firstGenerator ?? 0] + enemyPage);
                }
                else
                {
                    NewEnemies[i + 1] = (byte)(generators[swap] + enemyPage);
                }
            }

            //This used to make specifically ra head generators (but not any other generator) more likely to be vanilla by adding
            //it to the shuffle pool an extra time. Why? Who the fuck knows. After polling the community, most didn't care
            //some wanted it removed, and nobody wanted to keep it. So it goes.
            /*
            if (enemyNumber == 0x0B)
            {
                int swap = RNG.Next(0, generators.Length + 1);
                if (swap != generators.Length)
                {
                    NewEnemies[i + 1] = (byte)(generators[swap] + enemyPage);
                }
            }
            */

            //If this is not a shufflable enemy, just copy it as is.
            if(!(allEnemies.Contains(enemyNumber)))
            {
                NewEnemies[i] = Enemies[i];
                NewEnemies[i + 1] = Enemies[i + 1];
            }
        }
    }

    public string Debug()
    {
        StringBuilder sb = new();
        sb.Append("Map: " + (NewMap ?? Map) + " Name: " + Name + " Sideview: ");
        sb.Append(BitConverter.ToString(SideView).Replace("-", ""));
        sb.Append(" Enemies: ");
        sb.Append(BitConverter.ToString(NewEnemies[0] == 0 ? Enemies : NewEnemies).Replace("-", ""));
        return sb.ToString();
    }

    public string PrintUnsatisfiedExits()
    {
        StringBuilder sb = new();
        if(HasLeftExit() && Left == null)
        {
            sb.Append("(Left) ");
        }
        if (HasRightExit() && Right == null)
        {
            sb.Append("(Right) ");
        }
        if (HasUpExit() && Up == null)
        {
            sb.Append("(Up) ");
        }
        if (HasDownExit() && Down == null)
        {
            sb.Append("(Down) ");
        }
        return sb.ToString();
    }

    /*
    private bool IsAppropriateBlocker(int palaceNumber)
    {
        if (Number == 1)
        {
            if (r.IsFairyBlocked
                || r.IsDownstabBlocked
                || r.IsUpstabBlocked
                || r.IsJumpBlocked
                || r.IsGloveBlocked
                || (DROPS_ARE_BLOCKERS && (r.HasDrop || r.IsDropZone))
                || r.HasBoss)
            {
                return false;
            }
        }

        if (Number == 2)
        {
            if (r.IsFairyBlocked
                || r.IsDownstabBlocked
                || r.IsUpstabBlocked
                || (DROPS_ARE_BLOCKERS && (r.HasDrop || r.IsDropZone))
                || r.HasBoss)
            {
                return false;
            }
        }

        if (Number == 3)
        {
            if (r.IsJumpBlocked
                || r.IsFairyBlocked
                || (DROPS_ARE_BLOCKERS && (r.HasDrop || r.IsDropZone)))
            {
                return false;
            }
        }

        if (Number == 4)
        {
            if (r.IsGloveBlocked
                || r.IsUpstabBlocked
                || r.IsDownstabBlocked)
            {
                return false;
            }
        }

        if (Number == 5)
        {
            if (r.IsGloveBlocked
                || r.IsUpstabBlocked
                || r.IsDownstabBlocked
                || (DROPS_ARE_BLOCKERS && (r.HasDrop || r.IsDropZone))
                || r.HasBoss)
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
        return true;
    }
    */

    public bool IsTraversable(IEnumerable<RequirementType> requireables)
    {
        return Requirements.AreSatisfiedBy(requireables);
    }
}

public class RoomJsonConverter : JsonConverter<Room>
{
    public override Room ReadJson(JsonReader reader, Type objectType, Room existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        //Eventually i'll replace the hacky custom serialization in the Room(string json) constructor. That day is not now.
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, Room value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("name");
        writer.WriteValue(value.Name);

        writer.WritePropertyName("group");
        writer.WriteValue(value.Group);

        writer.WritePropertyName("map");
        writer.WriteValue(value.Map);

        writer.WritePropertyName("enabled");
        writer.WriteValue(value.Enabled);

        writer.WritePropertyName("connections");
        writer.WriteValue(BitConverter.ToString(value.Connections).Replace("-", ""));

        writer.WritePropertyName("enemies");
        writer.WriteValue(BitConverter.ToString((value.NewEnemies == null || value.NewEnemies[0] == 0) ? value.Enemies : value.NewEnemies ).Replace("-", ""));

        writer.WritePropertyName("sideviewData");
        writer.WriteValue(BitConverter.ToString(value.SideView).Replace("-", ""));

        writer.WritePropertyName("bitmask");
        writer.WriteValue(BitConverter.ToString(new Byte[] { value.bitmask }).Replace("-", ""));

        writer.WritePropertyName("requirements");
        writer.WriteRawValue(value.Requirements.Serialize());

        writer.WritePropertyName("hasItem");
        writer.WriteValue(value.HasItem);

        writer.WritePropertyName("hasBoss");
        writer.WriteValue(value.HasBoss);

        writer.WritePropertyName("hasDrop");
        writer.WriteValue(value.HasDrop);

        writer.WritePropertyName("elevatorScreen");
        writer.WriteValue(value.ElevatorScreen); ;

        writer.WritePropertyName("memoryAddress");
        writer.WriteValue(value.ConnectionStartAddress);

        writer.WritePropertyName("isUpDownReversed");
        writer.WriteValue(value.isUpDownReversed);

        writer.WritePropertyName("isDropZone");
        writer.WriteValue(value.IsDropZone);

        writer.WriteEndObject();
    }
}
