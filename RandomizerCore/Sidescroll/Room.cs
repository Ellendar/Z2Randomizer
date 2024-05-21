using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;
using RandomizerCore.Asm;

namespace RandomizerCore.Sidescroll;

[JsonSourceGenerationOptions(
    UseStringEnumConverter = true,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    Converters = [typeof(RequirementsJsonConverter)]
)]
[JsonSerializable(typeof(List<Room>))]
[JsonSerializable(typeof(Room))]
[JsonSerializable(typeof(Requirements))]
public partial class RoomSerializationContext : JsonSerializerContext { }

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Room
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

    [JsonPropertyName("bitmask")]
    [JsonConverter(typeof(HexStringConverter))]
    public byte[] ItemGetBits { get; set; }

    public byte Map { get; set; }
    public int? PalaceGroup { get; set; }
    public bool IsRoot { get; set; }
    
    [JsonIgnore]
    public Room LinkedRoom { get; set; }
    [JsonIgnore]
    public Room? Left { get; set; }
    [JsonIgnore]
    public Room? Right { get; set; }
    public Room Up { get; set; }
    public Room Down { get; set; }
    
    public bool IsReachable { get; set; }
    public int ConnectionStartAddress { get; set; }


    public bool IsDeadEnd => (HasLeftExit ? 1 : 0) + (HasRightExit ? 1 : 0) + (HasUpExit ? 1 : 0) + (HasDownExit ? 1 : 0) == 1;
    public bool IsPlaced { get; set; }
	
	//This still exists just to facilitate serialization because I didn't want to mess with it.
    //It's also used for the vanilla rooms to build the vanilla room tree, which should probably just be
    //handled elsewhere.
    //Please do not reference it outside of that
    [JsonConverter(typeof(HexStringConverter))]
    public byte[] Connections { get; set; }

    public bool IsBeforeTbird { get; set; }

    public bool HasDrop { get; set; }
    public int ElevatorScreen { get; set; }
    [JsonConverter(typeof(RequirementsJsonConverter))]
    public Requirements Requirements { get; set; }
    public bool IsDropZone { get; set; }
    public bool HasItem { get; set; }
    [JsonConverter(typeof(HexStringConverter))]
    public byte[] Enemies { get; set; }
    public byte[] NewEnemies { get; set; }
    [JsonPropertyName("sideviewData")]
    [JsonConverter(typeof(HexStringConverter))]
    public byte[] SideView { get; set; }
    //public int? NewMap { get; set; }
    //Whether the room contains a boss, which is true of boss rooms, but also boss-containing passthrough/item rooms.
    public bool HasBoss { get; set; }
    //Specifically indicates this is a "boss room" i.e. boss then a gem statue, then an exit.
    public bool IsBossRoom { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public RoomGroup Group { get; set; }
    public bool IsThunderBirdRoom { get; set; }
    public bool Enabled { get; set; }
    public bool IsEntrance { get; set; }
    public int? PalaceNumber { get; set; }
    public string LinkedRoomName { get; set; }
    //public int PageCount { get; private set; }
    public bool HasLeftExit { get; private set; }
    public bool HasRightExit { get; set; }
    public bool HasUpExit { get; private set; }
    public bool HasDownExit { get; private set; }
    public bool IsUpDownReversed { get; set; }


    public Room() {}
    
    public Room(Room room)
    {
        CopyFrom(room);
    }

    public Room(string json)
    {
        var room = JsonSerializer.Deserialize(json, RoomSerializationContext.Default.Room);
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
        HasLeftExit = room.HasLeftExit;
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
        IsUpDownReversed = room.IsUpDownReversed;
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

        Connections = room.Connections;
        HasLeftExit = room.Connections[0] < 0xFC;
        HasDownExit = IsUpDownReversed ? room.Connections[2] < 0xFC : room.Connections[1] < 0xFC;
        HasUpExit = IsUpDownReversed ? room.Connections[1] < 0xFC : room.Connections[2] < 0xFC;
        HasRightExit = room.Connections[3] < 0xFC;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, RoomSerializationContext.Default.Room);
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
            1 => RandomizerCore.Enemies.Palace125EnemyPtr,
            2 => RandomizerCore.Enemies.Palace346EnemyPtr,
            3 => RandomizerCore.Enemies.GPEnemyPtr,
            _ => throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup)
        };

        var baseEnemyAddr = PalaceGroup switch
        {
            1 => RandomizerCore.Enemies.NormalPalaceEnemyAddr,
            2 => RandomizerCore.Enemies.NormalPalaceEnemyAddr,
            3 => RandomizerCore.Enemies.GPEnemyAddr,
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
                romData.Put(RandomizerCore.Enemies.Palace125EnemyPtr + Map * 2, (byte)(memAddr & 0x00FF));
                romData.Put(RandomizerCore.Enemies.Palace125EnemyPtr + Map * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
                break;
            case 2:
                memAddr -= 0x98b0;
                romData.Put(RandomizerCore.Enemies.Palace346EnemyPtr + Map * 2, (byte)(memAddr & 0x00FF));
                romData.Put(RandomizerCore.Enemies.Palace346EnemyPtr + Map * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
                break;
            default:
                memAddr -= 0xd8b0;
                romData.Put(RandomizerCore.Enemies.GPEnemyPtr + Map * 2, (byte)(memAddr & 0x00FF));
                romData.Put(RandomizerCore.Enemies.GPEnemyPtr + Map * 2 + 1, (byte)((memAddr >> 8) & 0xFF));
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

    public int CountOpenExits()
    {
        var exits = 0;
        if(HasRightExit && Right == null)
        {
            exits++;
        }

        if (HasLeftExit && Left == null)
        {
            exits++;
        }

        if (HasUpExit && Up == null)
        {
            exits++;
        }

        if (HasDownExit && Down == null)
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
                allEnemies = RandomizerCore.Enemies.Palace125Enemies;
                smallEnemies = RandomizerCore.Enemies.Palace125SmallEnemies;
                largeEnemies = RandomizerCore.Enemies.Palace125LargeEnemies;
                flyingEnemies = RandomizerCore.Enemies.Palace125FlyingEnemies;
                generators = RandomizerCore.Enemies.Palace125Generators;
                shufflableEnemies = RandomizerCore.Enemies.StandardPalaceEnemies;
                shufflableSmallEnemies = RandomizerCore.Enemies.StandardPalaceSmallEnemies;
                shufflableLargeEnemies = RandomizerCore.Enemies.StandardPalaceLargeEnemies;
                shufflableFlyingEnemies = RandomizerCore.Enemies.StandardPalaceFlyingEnemies;
                shufflableGenerators = RandomizerCore.Enemies.StandardPalaceGenerators;
                break;
            case 2:
                allEnemies = RandomizerCore.Enemies.Palace346Enemies;
                smallEnemies = RandomizerCore.Enemies.Palace346SmallEnemies;
                largeEnemies = RandomizerCore.Enemies.Palace346LargeEnemies;
                flyingEnemies = RandomizerCore.Enemies.Palace346FlyingEnemies;
                generators = RandomizerCore.Enemies.Palace346Generators;
                shufflableEnemies = RandomizerCore.Enemies.StandardPalaceEnemies;
                shufflableSmallEnemies = RandomizerCore.Enemies.StandardPalaceSmallEnemies;
                shufflableLargeEnemies = RandomizerCore.Enemies.StandardPalaceLargeEnemies;
                shufflableFlyingEnemies = RandomizerCore.Enemies.StandardPalaceFlyingEnemies;
                shufflableGenerators = RandomizerCore.Enemies.StandardPalaceGenerators;
                break;
            case 3:
                allEnemies = RandomizerCore.Enemies.GPEnemies;
                smallEnemies = RandomizerCore.Enemies.GPSmallEnemies;
                largeEnemies = RandomizerCore.Enemies.GPLargeEnemies;
                flyingEnemies = RandomizerCore.Enemies.GPFlyingEnemies;
                generators = RandomizerCore.Enemies.GPGenerators;
                shufflableEnemies = RandomizerCore.Enemies.GPEnemies;
                shufflableSmallEnemies = RandomizerCore.Enemies.GPSmallEnemies;
                shufflableLargeEnemies = RandomizerCore.Enemies.GPLargeEnemies;
                shufflableFlyingEnemies = RandomizerCore.Enemies.GPFlyingEnemies;
                shufflableGenerators = RandomizerCore.Enemies.GPGenerators;
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
        if (HasLeftExit && Left == null)
        {
            sb.Append("(Left) ");
        }
        if (HasRightExit && Right == null)
        {
            sb.Append("(Right) ");
        }
        if (HasUpExit && Up == null)
        {
            sb.Append("(Up) ");
        }
        if (HasDownExit && Down == null)
        {
            sb.Append("(Down) ");
        }
        return sb.ToString();
    }
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
        if (!HasDownExit && !HasDrop)
        {
            return CONFLICT;
        }
        //Elevator down
        if (HasDownExit && ElevatorScreen >= 0)
        {
            return down.HasUpExit ? 1 : CONFLICT;
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
        if (!HasUpExit && !IsDropZone)
        {
            return CONFLICT;
        }
        //Elevator up
        if (HasUpExit && ElevatorScreen >= 0)
        {
            return up.HasDownExit ? 1 : CONFLICT;
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
        if (!HasLeftExit)
        {
            return CONFLICT;
        }
        return left.HasRightExit ? 1 : CONFLICT;
    }
    public int FitsWithRight(Room? right)
    {
        if (right == null)
        {
            return 0;
        }
        //No left
        if (!HasRightExit)
        {
            return CONFLICT;
        }
        return right.HasLeftExit ? 1 : CONFLICT;
    }

    public List<(int, int)> GetOpenExitCoords()
    {
        List<(int, int)> exitCoords = [];
        if (coords == (0, 0) && !IsRoot)
        {
            throw new Exception("Uninitialized coordinates referenced in coordinate palace generation");
        }
        if (HasLeftExit && Left == null)
        {
            exitCoords.Add((coords.Item1 - 1, coords.Item2));
        }
        if (HasRightExit && Right == null)
        {
            exitCoords.Add((coords.Item1 + 1, coords.Item2));
        }
        if (HasUpExit && Up == null)
        {
            exitCoords.Add((coords.Item1, coords.Item2 + 1));
        }
        if (HasDownExit && Down == null)
        {
            exitCoords.Add((coords.Item1, coords.Item2 - 1));
        }

        return exitCoords;
    }

    internal RoomExitType CategorizeExits()
    {
        if(HasLeftExit)
        {
            if (HasRightExit)
            {
                if (HasUpExit)
                {
                    if (HasDownExit)
                    {
                        return RoomExitType.FOUR_WAY;
                    }
                    return RoomExitType.INVERSE_T;
                }
                else if (HasDownExit)
                {
                    return RoomExitType.T;
                }
                else
                {
                    return RoomExitType.HORIZONTAL_PASSTHROUGH;
                }
            }
            else if (HasUpExit)
            {
                if (HasDownExit)
                {
                    return RoomExitType.LEFT_T;
                }
                return RoomExitType.NW_L;
            }
            else if (HasDownExit)
            {
                return RoomExitType.SW_L;
            }
            else return RoomExitType.DEADEND_LEFT;
        }
        //Not Left
        if(HasRightExit) {
            if(HasUpExit)
            {
                if(HasDownExit)
                {
                    return RoomExitType.RIGHT_T;
                }
                return RoomExitType.NE_L;
            }
            return RoomExitType.DEADEND_RIGHT;
        }
        if (HasUpExit)
        {
            if (HasDownExit)
            {
                return RoomExitType.VERTICAL_PASSTHROUGH;
            }
            return RoomExitType.DEADEND_UP;
        }
        else if (HasDownExit)
        {
            return RoomExitType.DEADEND_DOWN;
        }
        throw new ImpossibleException("Room has no exits");
    }

    public bool ValidateExits()
    {
        if (!HasRightExit && Right != null) 
        {
            return false;
        }
        if (!HasLeftExit && Left != null)
        {
            return false;
        }
        if (!HasUpExit && Up != null)
        {
            return false;
        }
        if (!HasDownExit && Down != null)
        {
            return false;
        }
        if(HasRightExit && Right != null && Right.Left != this)
        {
            return false;
        }
        if (HasLeftExit && Left != null && Left.Right != this)
        {
            return false;
        }
        if (HasUpExit && Up != null && Up.Down != this)
        {
            return false;
        }
        if (HasDownExit && Down != null && Down.Up != this)
        {
            return false;
        }
        return true;
    }
}

public class HexStringConverter : JsonConverter<byte[]?>
{

    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var val = reader.GetString();
        return val == null ? null : Convert.FromHexString(val);
    }

    public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToHexString(value?.ToArray() ?? []));
    }
}
