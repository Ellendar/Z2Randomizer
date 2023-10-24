using Newtonsoft.Json;
using NLog;
using RandomizerCore.Sidescroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Z2Randomizer.Core.Sidescroll;

public class Room
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private int upByte;
    private int downByte;
    private Room up;
    private Room down;
    public bool isUpDownReversed;

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
    public bool IsDeadEnd {
        get
        {
            return (HasLeftExit() ? 1 : 0) + (HasRightExit() ? 1 : 0) + (HasUpExit() ? 1 : 0) + (HasDownExit() ? 1 : 0) == 1;
        } 
    }
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
    //Whether the room contains a boss, which is true of boss rooms, but also boss-containing passthrough/item rooms.
    public bool HasBoss { get; set; }
    //Specifically indicates this is a "boss room" i.e. boss then a gem statue, then an exit.
    public bool IsBossRoom { get; set; }
    public string Name { get; set; }
    //public string Group { get; set; }
    public string Author { get; set; }
    public RoomGroup Group { get; set; }
    public bool IsThunderBirdRoom { get; set; }
    public bool Enabled { get; set; }
    public bool IsEntrance { get; set; }
    public int? PalaceNumber { get; set; }
    public string LinkedRoomName { get; set; }

    public Room(Room room)
    {
        Map = room.Map;
        Connections = (byte[])room.Connections.Clone();
        Enemies = (byte[])room.Enemies.Clone();
        NewEnemies = new byte[Enemies.Length];
        SideView = (byte[])room.SideView.Clone();
        bitmask = room.bitmask;
        HasItem = room.HasItem;
        HasBoss = room.HasBoss;
        IsBossRoom = room.IsBossRoom;
        HasDrop = room.HasDrop;
        ElevatorScreen = room.ElevatorScreen;
        ConnectionStartAddress = room.ConnectionStartAddress;
        isUpDownReversed = room.isUpDownReversed;
        IsDropZone = room.IsDropZone;
        IsEntrance = room.IsEntrance;
        IsThunderBirdRoom = room.IsThunderBirdRoom;
        Requirements = room.Requirements;
        Name = room.Name;
        LinkedRoomName = room.LinkedRoomName;
        Author = room.Author;
        Enabled = room.Enabled;
        Group = room.Group;

        LeftByte = room.Connections[0];
        downByte = room.Connections[1];
        upByte = room.Connections[2];
        RightByte = room.Connections[3];
    }

    public Room(string json)
    {
        dynamic roomData = JsonConvert.DeserializeObject(json);
        Name = roomData.name;
        Group = Enum.Parse(typeof(RoomGroup), roomData.group.ToString().ToUpper());
        Author = roomData.author;
        Map = roomData.map;
        Enabled = (bool)roomData.enabled;
        Connections = Convert.FromHexString(roomData.connections.ToString());
        LeftByte = Connections[0];
        DownByte = Connections[1];
        upByte = Connections[2];
        downByte = Connections[3];
        Enemies = Convert.FromHexString(roomData.enemies.ToString());
        NewEnemies = Enemies;
        SideView = Convert.FromHexString(roomData.sideviewData.ToString());
        bitmask = Convert.FromHexString(roomData.bitmask.ToString())[0];
        HasItem = roomData.hasItem;
        HasBoss = roomData.hasBoss;
        IsBossRoom = roomData.isBossRoom;
        HasDrop = roomData.hasDrop;
        ElevatorScreen = roomData.elevatorScreen;
        //ConnectionStartAddress = Convert.ToInt32("0x" + roomData.memoryAddress, 16);
        ConnectionStartAddress = roomData.memoryAddress;
        isUpDownReversed = roomData.isUpDownReversed;
        IsDropZone = roomData.isDropZone;
        IsEntrance = roomData.isEntrance;
        IsThunderBirdRoom = roomData.isThunderBirdRoom;
        PalaceNumber = roomData.palaceNumber;
        PalaceGroup = (int?)roomData.palaceGroup.Value ?? 0;
        Requirements = new Requirements(roomData.requirements.ToString());

        IsPlaced = false;
        IsRoot = false;
        IsReachable = false;
        IsBeforeTbird = false;

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

    public void UpdateEnemies(int enemyAddr, ROM ROMData, PalaceStyle normalPalaceStyle, PalaceStyle gpStyle)
    {
        //If we're using vanilla enemies, just clone them to newenemies so the logic can be the same for shuffled vs not
        if(NewEnemies[0] == 0)
        {
            NewEnemies = Enemies;
        }        
        //#76: If the item room is a boss item room, and it's in palace group 1, move the boss up 1 tile.
        //For some reason a bunch of the boss item rooms are fucked up in a bunch of different ways, so i'm keeping digshake's catch-all
        //though repositioned into the place it belongs.
        if (PalaceGroup == 1 && HasItem && IsBossRoom)
        {
            NewEnemies[1] = 0x6C;
        }
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

        if (NewEnemies.Length > 1 && PalaceGroup == 2)
        {
            for (int i = 2; i < NewEnemies.Length; i += 2)
            {
                if ((NewEnemies[i] & 0x3F) == 0x0A && !HasBoss && !HasItem)
                {
                    NewEnemies[i] = (byte)(0x0F + (NewEnemies[i] & 0xC0));
                }
            }
        }

        //Reconstructed palaces require us to rewrite the enemy pointers table.
        //XXX: This is probably wrong, but needs testing.
        if (true)//palaceStyle == PalaceStyle.RECONSTRUCTED)
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


        ROMData.Put(enemyAddr, NewEnemies);
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

    public void RandomizeEnemies(bool mixEnemies, bool generatorsAlwaysMatch, Random RNG)
    {
        int[] allEnemies;
        int[] smallEnemies;
        int[] largeEnemies;
        int[] flyingEnemies;
        int[] generators;
        //Because a 125 room could be shuffled into 346 or vice versa, we have to check if the enemy is that type in either
        //palace group, and if so, shuffle that enemy into a new enemy specifically appropriate to that palace
        int[] shufflableEnemies;
        int[] shufflableSmallEnemies;
        int[] shufflableLargeEnemies;
        int[] shufflableFlyingEnemies;
        int[] shufflableGenerators;
        switch (PalaceGroup)
        {
            case 1:
                allEnemies = Core.Enemies.Palace125Enemies;
                smallEnemies = Core.Enemies.Palace125SmallEnemies;
                largeEnemies = Core.Enemies.Palace125LargeEnemies;
                flyingEnemies = Core.Enemies.Palace125FlyingEnemies;
                generators = Core.Enemies.Palace125Generators;
                shufflableEnemies = Core.Enemies.StandardPalaceEnemies;
                shufflableSmallEnemies = Core.Enemies.StandardPalaceSmallEnemies;
                shufflableLargeEnemies = Core.Enemies.StandardPalaceLargeEnemies;
                shufflableFlyingEnemies = Core.Enemies.StandardPalaceFlyingEnemies;
                shufflableGenerators = Core.Enemies.StandardPalaceGenerators;
                break;
            case 2:
                allEnemies = Core.Enemies.Palace346Enemies;
                smallEnemies = Core.Enemies.Palace346SmallEnemies;
                largeEnemies = Core.Enemies.Palace346LargeEnemies;
                flyingEnemies = Core.Enemies.Palace346FlyingEnemies;
                generators = Core.Enemies.Palace346Generators;
                shufflableEnemies = Core.Enemies.StandardPalaceEnemies;
                shufflableSmallEnemies = Core.Enemies.StandardPalaceSmallEnemies;
                shufflableLargeEnemies = Core.Enemies.StandardPalaceLargeEnemies;
                shufflableFlyingEnemies = Core.Enemies.StandardPalaceFlyingEnemies;
                shufflableGenerators = Core.Enemies.StandardPalaceGenerators;
                break;
            case 3:
                allEnemies = Core.Enemies.GPEnemies;
                smallEnemies = Core.Enemies.GPSmallEnemies;
                largeEnemies = Core.Enemies.GPLargeEnemies;
                flyingEnemies = Core.Enemies.GPFlyingEnemies;
                generators = Core.Enemies.GPGenerators;
                shufflableEnemies = Core.Enemies.GPEnemies;
                shufflableSmallEnemies = Core.Enemies.GPSmallEnemies;
                shufflableLargeEnemies = Core.Enemies.GPLargeEnemies;
                shufflableFlyingEnemies = Core.Enemies.GPFlyingEnemies;
                shufflableGenerators = Core.Enemies.GPGenerators;
                break;
            default:
                throw new ImpossibleException("Invalid Palace Group");
        }

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
                if (shufflableEnemies.Contains(enemyNumber))
                {
                    int swapEnemy = allEnemies[RNG.Next(0, allEnemies.Length)];
                    int ypos = Enemies[i] & 0xF0;
                    int xpos = Enemies[i] & 0x0F;
                    if (shufflableSmallEnemies.Contains(enemyNumber) && shufflableLargeEnemies.Contains(swapEnemy))
                    {
                        ypos -= 16;
                        while (swapEnemy == 0x1D && ypos != 0x70 && PalaceGroup != 3)
                        {
                            swapEnemy = largeEnemies[RNG.Next(0, largeEnemies.Length)];
                        }
                    }
                    else
                    {
                        while (swapEnemy == 0x1D && ypos != 0x70 && PalaceGroup != 3)
                        {
                            swapEnemy = allEnemies[RNG.Next(0, allEnemies.Length)];
                        }
                    }

                    NewEnemies[i] = (byte)(ypos + xpos);
                    NewEnemies[i + 1] = (byte)(enemyPage + swapEnemy);
                    continue;
                }
            }
            else
            {
                if (shufflableLargeEnemies.Contains(enemyNumber))
                {
                    int swap = RNG.Next(0, largeEnemies.Length);
                    int ypos = Enemies[i] & 0xF0;
                    int xpos = Enemies[i] & 0x0F;
                    while (largeEnemies[swap] == 0x1D && ypos != 0x70 && PalaceGroup != 3)
                    {
                        swap = RNG.Next(0, largeEnemies.Length);
                    }
                    NewEnemies[i + 1] = (byte)(largeEnemies[swap] + enemyPage);
                    continue;
                }
                else if (shufflableSmallEnemies.Contains(enemyNumber))
                { 
                    int swap = RNG.Next(0, smallEnemies.Length);
                    NewEnemies[i+1] = (byte)(smallEnemies[swap] + enemyPage);
                    continue;
                }
            }


            if (shufflableFlyingEnemies.Contains(enemyNumber))
            {
                int swap = RNG.Next(0, flyingEnemies.Length);
                while (enemyNumber == 0x07 && (flyingEnemies[swap] == 0x06 || flyingEnemies[swap] == 0x0E))
                {
                    swap = RNG.Next(0, flyingEnemies.Length);
                }
                NewEnemies[i + 1] = (byte)(flyingEnemies[swap] + enemyPage);
                continue;
            }

            if (shufflableGenerators.Contains(enemyNumber))
            {
                int swap = RNG.Next(0, generators.Length);
                firstGenerator ??= swap;
                if (generatorsAlwaysMatch)
                {
                    NewEnemies[i + 1] = (byte)(generators[firstGenerator ?? 0] + enemyPage);
                    continue;
                }
                else
                {
                    NewEnemies[i + 1] = (byte)(generators[swap] + enemyPage);
                    continue;
                }
            }

            //If this is not a shufflable enemy, just copy it as is.
            NewEnemies[i] = Enemies[i];
            NewEnemies[i + 1] = Enemies[i + 1];
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
        writer.WriteValue(value.Group.ToString());

        writer.WritePropertyName("map");
        writer.WriteValue(value.Map);

        writer.WritePropertyName("enabled");
        writer.WriteValue(value.Enabled);

        writer.WritePropertyName("connections");
        writer.WriteValue(BitConverter.ToString(value.Connections).Replace("-", ""));

        writer.WritePropertyName("enemies");
        writer.WriteValue(BitConverter.ToString((value.NewEnemies == null || value.NewEnemies.Length == 0 || value.NewEnemies[0] == 0) ? value.Enemies : value.NewEnemies).Replace("-", ""));

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

        writer.WritePropertyName("isBossRoom");
        writer.WriteValue(value.IsBossRoom);

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

        writer.WritePropertyName("isThunderBirdRoom");
        writer.WriteValue(value.IsThunderBirdRoom);

        writer.WritePropertyName("isEntrance");
        writer.WriteValue(value.IsEntrance);

        writer.WritePropertyName("palaceNumber");
        writer.WriteValue(value.PalaceNumber);

        writer.WritePropertyName("linkedRoomName");
        writer.WriteValue(value.PalaceNumber);

        writer.WritePropertyName("author");
        writer.WriteValue(value.Author);

        writer.WriteEndObject();
    }
}
