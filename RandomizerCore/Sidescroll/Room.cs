using Newtonsoft.Json;
using NLog;
using RandomizerCore;
using Z2Randomizer.Core.Sidescroll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RandomizerCore.Asm;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Z2Randomizer.Core.Sidescroll;

[DataContract]
[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Room
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private byte upByte;
    private byte downByte;
    private Room? up;
    private Room? down;
    public bool isUpDownReversed;
    internal (int, int) coords;

    private const int sideview1 = 0x10533;
    private const int sideview2 = 0x12010;
    private const int sideview3 = 0x14533;
    private const int Group1ItemGetStartAddress = 0x17ba5;
    private const int Group2ItemGetStartAddress = 0x17bc5;
    private const int Group3ItemGetStartAddress = 0x17be5;
    private const int connectors1 = 0x1072b;
    private const int connectors2 = 0x12208;
    private const int connectors3 = 0x1472b;

    [DataMember(Name = "bitmask")]
    [Newtonsoft.Json.JsonConverter(typeof(HexStringConverter))]
    public byte[] ItemGetBits { get; set; }

    [DataMember]
    public int Map { get; set; }
    [DataMember]
    public int? PalaceGroup { get; set; }
    [DataMember]
    public bool IsRoot { get; set; }
    public Room LinkedRoom { get; set; }
    public Room? Left { get; set; }
    public Room? Right { get; set; }
    public Room? Up
    {
        get => isUpDownReversed ? down : up;

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
    public Room? Down
    {
        get => isUpDownReversed ? up : down;

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
    [DataMember]
    public bool IsReachable { get; set; }
    [DataMember]
    public int ConnectionStartAddress { get; set; }
    [DataMember]
    [Newtonsoft.Json.JsonConverter(typeof(HexStringConverter))]
    public byte[] Connections { get; set; }

    public bool IsDeadEnd => (HasLeftExit() ? 1 : 0) + (HasRightExit() ? 1 : 0) + (HasUpExit() ? 1 : 0) + (HasDownExit() ? 1 : 0) == 1;
    [DataMember]
>>>>>>> 71e219295cccd646719a977df6c28b66103caded
    public bool IsPlaced { get; set; }
    public byte LeftByte { get; set; }
    public byte RightByte { get; set; }
    public byte UpByte
    {
        get => isUpDownReversed ? downByte : upByte;
		
        set
        {
            if (isUpDownReversed)
            {
                downByte = value;
                return;
            }
            upByte = value;
        }
    }
    public byte DownByte
    {
        get => isUpDownReversed ? upByte : downByte;

        set
        {
            if (isUpDownReversed)
            {
                upByte = value;
                return;
            }
            downByte = value;
        }
    }
    public bool IsBeforeTbird { get; set; }

    [DataMember]
    public bool HasDrop { get; set; }
    [DataMember]
    public int ElevatorScreen { get; set; }
    //public bool IsFairyBlocked { get; set; }
    //public bool IsUpstabBlocked { get; set; }
    //public bool IsDownstabBlocked { get; set; }
    //public bool IsGloveBlocked { get; set; }
    //public bool IsJumpBlocked { get; set; }
    [DataMember]
    [Newtonsoft.Json.JsonConverter(typeof(RequirementsJsonConverter))]
    public Requirements Requirements { get; set; }
    [DataMember]
    public bool IsDropZone { get; set; }
    [DataMember]
    public bool HasItem { get; set; }
    [DataMember]
    [Newtonsoft.Json.JsonConverter(typeof(HexStringConverter))]
    public byte[] Enemies { get; set; }
    public byte[] NewEnemies { get; set; }
    [DataMember(Name = "sideviewData")]
    [Newtonsoft.Json.JsonConverter(typeof(HexStringConverter))]
    public byte[] SideView { get; set; }
    //public int? NewMap { get; set; }
    //Whether the room contains a boss, which is true of boss rooms, but also boss-containing passthrough/item rooms.
    [DataMember]
    public bool HasBoss { get; set; }
    //Specifically indicates this is a "boss room" i.e. boss then a gem statue, then an exit.
    [DataMember]
    public bool IsBossRoom { get; set; }
    [DataMember]
    public string Name { get; set; }
    //public string Group { get; set; }
    [DataMember]
    public string Author { get; set; }
    [DataMember]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public RoomGroup Group { get; set; }
    [DataMember]
    public bool IsThunderBirdRoom { get; set; }
    [DataMember]
    public bool Enabled { get; set; }
    [DataMember]
    public bool IsEntrance { get; set; }
    [DataMember]
    public int? PalaceNumber { get; set; }
    [DataMember]
    public string LinkedRoomName { get; set; }
    [DataMember]
    public int PageCount { get; private set; }

    public Room() {}
    
    public Room(Room room)
    {
        CopyFrom(room);
    }

    public Room(string json)
    {
        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };
        var options = new JsonSerializerSettings
        {
            ContractResolver = contractResolver,
            Formatting = Formatting.None
        };
        var room = JsonConvert.DeserializeObject<Room>(json, options);
        CopyFrom(room!);
        
        var length = SideView?[0] ?? 0;
        if(SideView?.Length != length)
        {
            throw new Exception($"Room length header {length} does not match actual length for sideview: {SideView.Length}");
        }
    }

    private void CopyFrom(Room room)
    {
        Map = room.Map;
        Connections = room.Connections.ToArray();
        Enemies = room.Enemies.ToArray();
        NewEnemies = new byte[Enemies.Length];
        SideView = room.SideView.ToArray();
        ItemGetBits = room.ItemGetBits.ToArray();
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
        PalaceGroup = room.PalaceGroup;
        PalaceNumber = room.PalaceNumber;

        LeftByte = room.Connections[0];
        downByte = room.Connections[1];
        upByte = room.Connections[2];
        RightByte = room.Connections[3];
    }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public void WriteSideViewPtr(AsmModule a, string label)
    {
        if(PalaceGroup is <= 0 or > 3)
        {
            throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup);
        }
        int addr;
        switch (PalaceGroup)
        {
            case 1:
                a.Segment("PRG4");
                addr = sideview1;
                break;
            case 2:
                a.Segment("PRG4");
                addr = sideview2;
                break;
            default:
                a.Segment("PRG5", "PRG7");
                addr = sideview3;
                break;
        }
        a.RomOrg(addr + Map * 2);
        a.Word(a.Symbol(label));
    }

    public void UpdateEnemies(int enemyAddr, ROM romData)
    {
        //If we're using vanilla enemies, just clone them to newenemies so the logic can be the same for shuffled vs not
        if (NewEnemies[0] == 0)
        {
            NewEnemies = Enemies;
        }
        //#76: If the item room is a boss item room, and it's in palace group 1, move the boss up 1 tile.
        //For some reason a bunch of the boss item rooms are fucked up in a bunch of different ways, so i'm keeping digshake's catch-all
        //though repositioned into the place it belongs.
        if (PalaceGroup == 1 && HasItem && HasBoss)
        {
            NewEnemies[1] = 0x6C;
        }
        var enemyPtr = PalaceGroup switch
        {
            1 => Core.Enemies.Palace125EnemyPtr,
            2 => Core.Enemies.Palace346EnemyPtr,
            3 => Core.Enemies.GPEnemyPtr,
            _ => throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup)
        };

        var baseEnemyAddr = PalaceGroup switch
        {
            1 => Core.Enemies.NormalPalaceEnemyAddr,
            2 => Core.Enemies.NormalPalaceEnemyAddr,
            3 => Core.Enemies.GPEnemyAddr,
            _ => throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup)
        };

        if (NewEnemies.Length > 1 && PalaceGroup == 2)
        {
            for (var i = 2; i < NewEnemies.Length; i += 2)
            {
                if ((NewEnemies[i] & 0x3F) == 0x0A && !HasBoss && !HasItem)
                {
                    NewEnemies[i] = (byte)(0x0F + (NewEnemies[i] & 0xC0));
                }
            }
        }

        //Reconstructed palaces require us to rewrite the enemy pointers table.
        var memAddr = enemyAddr;
        switch (PalaceGroup)
        {
            //Write the updated pointers
            case 1:
                memAddr -= 0x98b0;
                romData.Put(Core.Enemies.Palace125EnemyPtr + Map * 2, (byte)(memAddr & 0x00FF));
                romData.Put(Core.Enemies.Palace125EnemyPtr + Map * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
                break;
            case 2:
                memAddr -= 0x98b0;
                romData.Put(Core.Enemies.Palace346EnemyPtr + Map * 2, (byte)(memAddr & 0x00FF));
                romData.Put(Core.Enemies.Palace346EnemyPtr + Map * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
                break;
            default:
                memAddr -= 0xd8b0;
                romData.Put(Core.Enemies.GPEnemyPtr + Map * 2, (byte)(memAddr & 0x00FF));
                romData.Put(Core.Enemies.GPEnemyPtr + Map * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
                break;
        }


        romData.Put(enemyAddr, NewEnemies);
    }

    public void UpdateItemGetBits(ROM romData)
    {
        if (PalaceGroup is <= 0 or > 3)
        {
            throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup);
        }

        var ptr = PalaceGroup switch
        {
            2 => Group2ItemGetStartAddress,
            3 => Group3ItemGetStartAddress,
            _ => Group1ItemGetStartAddress
        };
        if(Map % 2 == 0)
        {
            var old = romData.GetByte(ptr + Map / 2);
            old = (byte)(old & 0x0F);
            old = (byte)((ItemGetBits[0] << 4) | old);
            romData.Put(ptr + Map / 2, old);
        }
        else
        {
            var old = romData.GetByte(ptr + Map / 2);
            old = (byte)(old & 0xF0);
            old = (byte)((ItemGetBits[0]) | old);
            romData.Put(ptr + Map / 2, old);
        }
    }

    public void UpdateItem(Collectable i, ROM romData)
    {
        if (PalaceGroup is <= 0 or > 3)
        {
            throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup);
        }
        var sideViewPtr = (romData.GetByte(sideview1 + Map * 2) + (romData.GetByte(sideview1 + 1 + Map * 2) << 8));

        if (PalaceGroup == 2)
        {
            sideViewPtr = (romData.GetByte(sideview2 + Map * 2) + (romData.GetByte(sideview2 + 1 + Map * 2) << 8));
        }
        // If the address is is >= 0xc000 then its in the fixed bank so we want to add 0x1c010 to get the fixed bank
        // otherwise we want to use the bank offset for PRG4 (0x10000)
        sideViewPtr -= 0x8000;
        sideViewPtr += sideViewPtr >= 0x4000 ? (0x1c000 - 0x4000) : 0x10000;
        sideViewPtr += 0x10; // Add the offset for the iNES header
        var ptr = sideViewPtr + 4;
        var data = romData.GetByte(ptr);
        data = (byte)(data & 0xF0);
        data = (byte)(data >> 4);
        var data2 = romData.GetByte(ptr+1);
        while (data >= 13 || data2 != 0x0F)
        {
            ptr+=2;
            data = romData.GetByte(ptr);
            data = (byte)(data & 0xF0);
            data = (byte)(data >> 4);
            data2 = romData.GetByte(ptr + 1);
        }
        romData.Put(ptr + 2, (byte)i);

    }

    public void UpdateConnectionBytes()
    {
        PageCount = ((SideView[1] & 0b01100000) >> 5) + 1;
        Connections[0] = LeftByte;
        Connections[1] = downByte;
        Connections[2] = upByte;
        Connections[3] = RightByte;
    }

    public void UpdateConnectionStartAddress()
    {
        ConnectionStartAddress = PalaceGroup switch
        {
            1 => connectors1 + Map * 4,
            2 => connectors2 + Map * 4,
            3 => connectors3 + Map * 4,
            _ => throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup)
        };
    }

    public bool HasUpExit()
    {
        if (isUpDownReversed)
        {
            return (downByte < 0xFC && downByte > 0x03);
        }
        else
        {
            return (upByte < 0xFC);// || (Map == 4 && upByte == 0x02) || (Map == 1 && upByte == 0x02) || (Map == 2 && upByte == 0x03);
        }
    }

    public bool HasDownExit()
    {
        if (isUpDownReversed)
        {
            return (upByte < 0xFC);
        }
        else
        {
            return (downByte < 0xFC);
        }
    }

    public bool HasLeftExit()
    {

        return (LeftByte < 0xFC);

    }

    public bool HasRightExit()
    {

        return (RightByte < 0xFC);

    }

    public int CountOpenExits()
    {
        var exits = 0;
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
        var enemiesLength = (int)Enemies[0] - 1;
        NewEnemies[0] = Enemies[0];
        for (int i = 1; i <= enemiesLength; i += 2)
        {
            NewEnemies[i] = Enemies[i];
            var enemyNumber = Enemies[i + 1] & 0x3F;
            var enemyPage = Enemies[i + 1] & 0xC0;
            if (mixEnemies)
            {
                //If this is a shufflable enemy (As opposed to something like a crystal marker)
                if (shufflableEnemies.Contains(enemyNumber))
                {
                    var swapEnemy = allEnemies[RNG.Next(0, allEnemies.Length)];
                    var ypos = Enemies[i] & 0xF0;
                    var xpos = Enemies[i] & 0x0F;
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
                    var swap = RNG.Next(0, largeEnemies.Length);
                    var ypos = Enemies[i] & 0xF0;
                    var xpos = Enemies[i] & 0x0F;
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
                    NewEnemies[i + 1] = (byte)(smallEnemies[swap] + enemyPage);
                    continue;
                }
            }


            if (shufflableFlyingEnemies.Contains(enemyNumber))
            {
                var swap = RNG.Next(0, flyingEnemies.Length);
                while (enemyNumber == 0x07 && (flyingEnemies[swap] == 0x06 || flyingEnemies[swap] == 0x0E))
                {
                    swap = RNG.Next(0, flyingEnemies.Length);
                }
                NewEnemies[i + 1] = (byte)(flyingEnemies[swap] + enemyPage);
                continue;
            }

            if (shufflableGenerators.Contains(enemyNumber))
            {
                var swap = RNG.Next(0, generators.Length);
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
        sb.Append("Map: " + Map + " Name: " + Name + " Sideview: ");
        sb.Append(BitConverter.ToString(SideView).Replace("-", ""));
        sb.Append(" Enemies: ");
        sb.Append(BitConverter.ToString(NewEnemies[0] == 0 ? Enemies : NewEnemies).Replace("-", ""));
        return sb.ToString();
    }

    public string PrintUnsatisfiedExits()
    {
        StringBuilder sb = new();
        if (HasLeftExit() && Left == null)
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

    public string GetDebuggerDisplay()
    {
        StringBuilder sb = new();
        sb.Append(Map + " ");
        sb.Append(Name + " ");
        sb.Append("[" + BitConverter.ToString(SideView).Replace("-", "") + "] ");
        sb.Append("[" + BitConverter.ToString(Enemies).Replace("-", "") + "]");
        return sb.ToString();
    }


    private const int CONFLICT = -99;
    public int FitsWithDown(Room? down)
    {
        if (down == null)
        {
            return 0;
        }
        //No down
        if (!HasDownExit() && !HasDrop)
        {
            return CONFLICT;
        }
        //Elevator down
        if (HasDownExit() && ElevatorScreen >= 0)
        {
            return down.HasUpExit() ? 1 : CONFLICT;
        }
        //Drop down
        if (HasDrop)
        {
            return down.IsDropZone ? 1 : CONFLICT;
        }
        throw new ImpossibleException("Room " + Name + " is marked as a down exit with no elevator or drop.");
    }

    public int FitsWithUp(Room? up)
    {
        if (up == null)
        {
            return 0;
        }
        //No up
        if (!HasUpExit() && !IsDropZone)
        {
            return CONFLICT;
        }
        //Elevator up
        if (HasUpExit() && ElevatorScreen >= 0)
        {
            return up.HasDownExit() ? 1 : CONFLICT;
        }
        //Drop into
        if (IsDropZone)
        {
            return up.HasDrop ? 1 : CONFLICT;
        }
        throw new ImpossibleException("Room " + Name + " is marked as a up exit with no elevator or drop.");
    }

    public int FitsWithLeft(Room? left)
    {
        if (left == null)
        {
            return 0;
        }
        //No left
        if (!HasLeftExit())
        {
            return CONFLICT;
        }
        return left.HasRightExit() ? 1 : CONFLICT;
    }
    public int FitsWithRight(Room? right)
    {
        if (right == null)
        {
            return 0;
        }
        //No left
        if (!HasRightExit())
        {
            return CONFLICT;
        }
        return right.HasLeftExit() ? 1 : CONFLICT;
    }

    public List<(int, int)> GetOpenExitCoords()
    {
        List<(int, int)> exitCoords = [];
        if (coords == (0, 0) && !IsRoot)
        {
            throw new Exception("Uninitialized coordinates referenced in coordinate palace generation");
        }
        if (HasLeftExit() && Left == null)
        {
            exitCoords.Add((coords.Item1 - 1, coords.Item2));
        }
        if (HasRightExit() && Right == null)
        {
            exitCoords.Add((coords.Item1 + 1, coords.Item2));
        }
        if (HasUpExit() && Up == null)
        {
            exitCoords.Add((coords.Item1, coords.Item2 + 1));
        }
        if (HasDownExit() && Down == null)
        {
            exitCoords.Add((coords.Item1, coords.Item2 - 1));
        }

        return exitCoords;
    }
}

public class HexStringConverter : Newtonsoft.Json.JsonConverter<byte[]>
{
    public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
    {
        writer.WriteValue(Convert.ToHexString(value?.ToArray() ?? []));
    }

    public override byte[] ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var val = reader.Value as string ?? "";
        return Convert.FromHexString(val);
    }
}