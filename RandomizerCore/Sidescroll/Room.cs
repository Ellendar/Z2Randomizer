using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using js65;
using NLog;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public record struct Coord(int X, int Y)
{
    public Coord((int, int) coords) : this(coords.Item1, coords.Item2) {}
    public static Coord Uninitialized = new(0, 0);
}

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
[JsonSerializable(typeof(Coord))]
public partial class RoomSerializationContext : JsonSerializerContext { }

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Room : IJsonOnDeserialized
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    internal Coord coords;

    private const int sideview1 = 0x10533;
    private const int sideview2 = 0x12010;
    private const int sideview3 = 0x14533;
    public const int Group1ItemGetStartAddress = 0x17ba5;
    public const int Group2ItemGetStartAddress = 0x17bc5;
    public const int Group3ItemGetStartAddress = 0x17be5;
    private const int connectors1 = 0x1072b;
    private const int connectors2 = 0x12208;
    private const int connectors3 = 0x1472b;

    [JsonPropertyName("bitmask")]
    [JsonConverter(typeof(HexStringConverter))]
    public byte[] ItemGetBits { get; set; }

    public byte Map { get; set; }

    [JsonConverter(typeof(JsonNumberEnumConverter<PalaceGrouping>))]
    public PalaceGrouping? PalaceGroup => Util.AsPalaceGrouping(PalaceNumber);
    [JsonIgnore]
    public bool IsRoot { get; set; }
    
    [JsonIgnore]
    public Room? LinkedRoom { get; set; }
    [JsonIgnore]
    public Room? Left { get; set; }
    [JsonIgnore]
    public Room? Right { get; set; }
    [JsonIgnore]
    public Room? Up { get; set; }
    [JsonIgnore]
    public Room? Down { get; set; }
    [JsonIgnore]
    public bool IsReachable { get; set; }
    [JsonPropertyName("memoryAddress")]
    public int ConnectionStartAddress { get; set; }

    [JsonIgnore]
    public bool IsDeadEnd => (HasLeftExit ? 1 : 0) + (HasRightExit ? 1 : 0) + (HasUpExit ? 1 : 0) + (HasDownExit ? 1 : 0) == 1;
    //public bool IsPlaced { get; set; }
	
	//This still exists just to facilitate serialization because I didn't want to mess with it.
    //It's also used for the vanilla rooms to build the vanilla room tree, which should probably just be
    //handled elsewhere.
    //Please do not reference it outside of that
    [JsonConverter(typeof(HexStringConverter))]
    public byte[] Connections { get; set; }
    [JsonIgnore]
    public bool IsBeforeTbird { get; set; }

    public bool HasDrop { get; set; }
    public int ElevatorScreen { get; set; }
    [JsonConverter(typeof(RequirementsJsonConverter))]
    public Requirements Requirements { get; set; }
    public bool IsDropZone { get; set; }
    public bool HasItem { get; set; }
    [JsonConverter(typeof(HexStringConverter))]
    public byte[] Enemies { get; set; }
    [JsonIgnore]
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
    public string? LinkedRoomName { get; set; }
    //public int PageCount { get; private set; }
    [JsonIgnore]
    public bool HasLeftExit { get; private set; }
    [JsonIgnore]
    public bool HasRightExit { get; set; }
    [JsonIgnore]
    public bool HasUpExit { get; private set; }
    [JsonIgnore]
    public bool HasDownExit { get; private set; }
    public bool IsUpDownReversed { get; set; }

    [JsonIgnore]
    public Room MergedPrimary { get; set; }
    [JsonIgnore]
    public Room MergedSecondary { get; set; }
    public List<string> SuppressWarning { get; set; } = [];

    //The json loads the fields the analyzer says aren't loaded. Source: trust me bro
    //But seriously eventually put some validation here.
#pragma warning disable CS8618
    public Room() {}
    
    public Room(Room room)
    {
        CopyFrom(room);
    }


    public Room(string json)
#pragma warning restore CS8618 
    {
        var room = JsonSerializer.Deserialize(json, RoomSerializationContext.Default.Room);
        CopyFrom(room!);
        
        var length = SideView?[0] ?? 0;
        if(SideView?.Length != length)
        {
            throw new Exception($"Room length header {length} does not match actual length for sideview: {SideView!.Length}");
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
        // PalaceGroup = room.PalaceGroup;
        PalaceNumber = room.PalaceNumber;

        Connections = room.Connections;
        HasLeftExit = room.Connections[0] < 0xFC;
        HasDownExit = IsUpDownReversed ? room.Connections[2] < 0xFC : room.Connections[1] < 0xFC;
        HasUpExit = IsUpDownReversed ? room.Connections[1] < 0xFC : room.Connections[2] < 0xFC;
        HasRightExit = room.Connections[3] < 0xFC;
    }

    public void OnDeserialized()
    {
        HasLeftExit = Connections[0] < 0xFC;
        HasDownExit = IsUpDownReversed ? Connections[2] < 0xFC : Connections[1] < 0xFC;
        HasUpExit = IsUpDownReversed ? Connections[1] < 0xFC : Connections[2] < 0xFC;
        HasRightExit = Connections[3] < 0xFC;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, RoomSerializationContext.Default.Room);
    }

    public void WriteSideViewPtr(AsmModule a, string label)
    {
        // var tableAddr = (ushort)(0x8000 + (byte)PalaceGroup * 0x40 * 2 + Map * 2);
        var tableAddr = PalaceGroup switch
        {
            PalaceGrouping.Palace125 => sideview1,
            PalaceGrouping.Palace346 => sideview2,
            PalaceGrouping.PalaceGp => sideview3,
            _ => throw new NotImplementedException(),
        };
        tableAddr += Map * 2;
        // Console.WriteLine($"whatever {label} here: ${tableAddr:X6}");
        a.Segment(PalaceGroup == PalaceGrouping.PalaceGp ? "PRG5" : "PRG4");
        a.RomOrg(tableAddr);
        a.Word(a.Symbol(label));
    }

    public int UpdateEnemies(AsmModule a, int enemyAddr)
    {
        //If we're using vanilla enemies, just clone them to newenemies so the logic can be the same for shuffled vs not
        if (NewEnemies[0] == 0)
        {
            NewEnemies = Enemies;
        }
        //#76: If the item room is a boss item room, and it's in palace group 1, move the boss up 1 tile.
        //For some reason a bunch of the boss item rooms are fucked up in a bunch of different ways, so i'm keeping digshake's catch-all
        //though repositioned into the place it belongs.
        if (PalaceGroup == PalaceGrouping.Palace125 && HasItem && HasBoss)
        {
            NewEnemies[1] = 0x6C;
        }

        if (NewEnemies.Length > 1 && PalaceGroup == PalaceGrouping.Palace346)
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
        var enemyDataAddr = enemyAddr;
        var tableAddr = 0;
        switch (PalaceGroup)
        {
            //Write the updated pointers
            case PalaceGrouping.Palace125:
                enemyDataAddr -= 0x98b0;
                tableAddr = RandomizerCore.Enemies.Palace125EnemyPtr + Map * 2;
                break;
            case PalaceGrouping.Palace346:
                enemyDataAddr -= 0x98b0;
                tableAddr = RandomizerCore.Enemies.Palace346EnemyPtr + Map * 2;
                break;
            default:
                enemyDataAddr -= 0xd8b0;
                tableAddr = RandomizerCore.Enemies.GPEnemyPtr + Map * 2;
                break;
        }
        
        a.RomOrg(tableAddr);
        a.Word((ushort)enemyDataAddr);

        a.RomOrg(enemyAddr);
        a.Byt(NewEnemies);
        return NewEnemies.Length;
    }

    public void UpdateItemGetBits(Dictionary<PalaceGrouping, byte[]> palaceItemBits)
    {
        PalaceGrouping palaceGroup = PalaceGroup ?? throw new Exception("Unable to assign ItemGetBits on a room with no palace group");

        var old = palaceItemBits[palaceGroup][Map / 2];
        if(Map % 2 == 0)
        {
            old = (byte)(old & 0x0F);
            old = (byte)((ItemGetBits[0] << 4) | old);
        }
        else
        {
            old = (byte)(old & 0xF0);
            old = (byte)((ItemGetBits[0]) | old);
        }
        palaceItemBits[palaceGroup][Map / 2] = old;
    }

    public void UpdateRomItem(Collectable item, ROM romData)
    {
        // Sideview data is moved to the expanded banks at $1c/$1d
        var baseAddr = sideview1;
        if (PalaceGroup == PalaceGrouping.Palace346)
        {
            baseAddr = sideview2;
        }

        int sideViewPtr = romData.GetByte(baseAddr + Map * 2) | (romData.GetByte(baseAddr + 1 + Map * 2) << 8);

        // Start of the segment memory address is $8000, so offset for that
        sideViewPtr -= 0x8000;
        // If the address is is >= 0xc000 then its in the fixed bank so we want to add 0x1c010 to get the fixed bank
        // otherwise we want to use the bank offset for PRG4 (0x10000)
        // sideViewPtr += sideViewPtr >= 0x4000 ? (0x1c000 - 0x4000) : 0x10000;
        sideViewPtr += sideViewPtr >= 0x4000 ? (0x1c000 - 0x4000) : 0x38000;
        sideViewPtr += 0x10; // Add the offset for the iNES header
        byte sideviewLength = romData.GetByte(sideViewPtr);
        int offset = 4;

        do
        {
            int yPos = romData.GetByte(sideViewPtr + offset++);
            yPos = (byte)(yPos & 0xF0);
            yPos = (byte)(yPos >> 4);
            int byte2 = romData.GetByte(sideViewPtr + offset++);

            if (yPos >= 13 || byte2 != 0x0F) continue;
            int byte3 = romData.GetByte(sideViewPtr + offset++);
            
            if (((Collectable)byte3).IsMinorItem()) continue;
            romData.Put(sideViewPtr + offset - 1, (byte)item);
            return;
        } while (offset < sideviewLength);
        logger.Warn("Could not write Collectable to Item room in palace " + PalaceNumber);
        //throw new Exception("Could not write Collectable to Item room in palace " + PalaceNumber);
    }

    public void UpdateConnectionStartAddress()
    {
        ConnectionStartAddress = PalaceGroup switch
        {
            PalaceGrouping.Palace125 => connectors1 + Map * 4,
            PalaceGrouping.Palace346 => connectors2 + Map * 4,
            PalaceGrouping.PalaceGp => connectors3 + Map * 4,
            _ => throw new ImpossibleException("INVALID PALACE GROUP: " + PalaceGroup)
        };
    }

    public int CountExits()
    {
        return (HasLeftExit ? 1 : 0) + (HasDownExit ? 1 : 0) + (HasUpExit ? 1 : 0) + (HasRightExit ? 1 : 0);
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

    public void RandomizeEnemies(bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG)
    {
        //Because a 125 room could be shuffled into 346 or vice versa, we have to check if the enemy is that type in either
        //palace group, and if so, shuffle that enemy into a new enemy specifically appropriate to that palace
        switch (PalaceGroup)
        {
            case PalaceGrouping.Palace125:
                {
                    var groundEnemies = RandomizerCore.Enemies.Palace125GroundEnemies;
                    var smallEnemies = RandomizerCore.Enemies.Palace125SmallEnemies;
                    var largeEnemies = RandomizerCore.Enemies.Palace125LargeEnemies;
                    var flyingEnemies = RandomizerCore.Enemies.Palace125FlyingEnemies;
                    var generators = RandomizerCore.Enemies.Palace125Generators;
                    var ee = new EnemiesEditable<EnemiesPalace125>(Enemies);
                    RandomizeEnemiesInner(ee, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG, groundEnemies, smallEnemies, largeEnemies, flyingEnemies, generators);
                    break;
                }
            case PalaceGrouping.Palace346:
                {
                    var groundEnemies = RandomizerCore.Enemies.Palace346GroundEnemies;
                    var smallEnemies = RandomizerCore.Enemies.Palace346SmallEnemies;
                    var largeEnemies = RandomizerCore.Enemies.Palace346LargeEnemies;
                    var flyingEnemies = RandomizerCore.Enemies.Palace346FlyingEnemies;
                    var generators = RandomizerCore.Enemies.Palace346Generators;
                    var ee = new EnemiesEditable<EnemiesPalace346>(Enemies);
                    RandomizeEnemiesInner(ee, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG, groundEnemies, smallEnemies, largeEnemies, flyingEnemies, generators);
                    break;
                }
            case PalaceGrouping.PalaceGp:
                {
                    var groundEnemies = RandomizerCore.Enemies.GPGroundEnemies;
                    var smallEnemies = RandomizerCore.Enemies.GPSmallEnemies;
                    var largeEnemies = RandomizerCore.Enemies.GPLargeEnemies;
                    var flyingEnemies = RandomizerCore.Enemies.GPFlyingEnemies;
                    var generators = RandomizerCore.Enemies.GPGenerators;
                    var ee = new EnemiesEditable<EnemiesGreatPalace>(Enemies);
                    RandomizeEnemiesInner(ee, mixLargeAndSmallEnemies, generatorsAlwaysMatch, RNG, groundEnemies, smallEnemies, largeEnemies, flyingEnemies, generators);
                    break;
                }
            default:
                throw new ImpossibleException("Invalid Palace Group");
        }
    }

    private void RandomizeEnemiesInner<T>(EnemiesEditable<T> ee, bool mixLargeAndSmallEnemies, bool generatorsAlwaysMatch, Random RNG, T[] groundEnemies, T[] smallEnemies, T[] largeEnemies, T[] flyingEnemies, T[] generators) where T : Enum
    {
        bool[,]? solidGridLazy = null; // lazily instanced if needed
        bool[,] GetSolidGrid<P>() where P : Enum
        {
            if (solidGridLazy == null)
            {
                var sv = new SideviewEditable<P>(SideView);
                solidGridLazy = sv.CreateSolidGrid();
            }
            return solidGridLazy;
        }
        bool AreaIsOpen<P>(ref bool? cachedResult, int x, int y, int w, int h) where P : Enum
        {
            if (cachedResult == null)
            {
                var solidGrid = GetSolidGrid<P>();
                cachedResult = SolidGridHelper.AreaIsOpen(solidGrid, x, y, w, h);
            }
            return cachedResult.Value;
        }
        bool PositionKingBot(ref bool? cachedResult, Enemy<T> enemy)
        {
            // Vanilla King Bot positions for reference:
            // GP Map 37 at y == 1 where floor is at y == 8
            // GP Map 48 at y == 3 where floor is at y == 11

            // Do our best to position King Bots high up, so they are unlikely to
            // spawn inside the player's position.  y==3 is the preferred spot unless
            // the floor is high, as there is also the risk of having the King Bot
            // spawn in your position when you are Fairying over a room.
            // (Enemies in Zelda II cannot be positioned at y == 0 or y == 2.)

            // Unless we find a 3x6 empty area here we will not spawn a King Bot.
            if (cachedResult == null)
            {
                var solidGrid = GetSolidGrid<GreatPalaceObject>();

                int newY = 0;
                foreach (int j in new int[] { 3, 4, 1, 5 })
                {
                    if (SolidGridHelper.AreaIsOpen(solidGrid, enemy.X, j, 3, 6))
                    {
                        newY = j;
                        break;
                    }
                }
                if (newY != 0)
                {
                    enemy.Y = newY;
                    cachedResult = true;
                }
                else
                {
                    cachedResult = false;
                }
            }
            return cachedResult.Value;
        }

        T RerollLargeEnemyIfNeeded(Enemy<T> enemy, T swapToId)
        {
            if (PalaceGroup != PalaceGrouping.PalaceGp)
            {
                bool? roomForStalfos = null;
                while (true)
                {
                    bool reroll = false;
                    switch (swapToId)
                    {
                        // Re-roll Magos and Wizards unless their y pos is 9.
                        case EnemiesPalace125.MAGO:
                        case EnemiesPalace346.WIZARD:
                            reroll = enemy.Y != 0x09;
                            break;

                        // Re-roll Stalfos if they don't have room to dive from the ceiling to their position
                        case EnemiesPalace125.RED_STALFOS:
                        case EnemiesPalace125.BLUE_STALFOS:
                        case EnemiesPalace346.RED_STALFOS:
                        case EnemiesPalace346.BLUE_STALFOS:
                            // Stalfos will get fully stuck if the ceiling is solid until y=4. If it's solid only until y=3 or y=2, it will slowly slide down.
                            reroll = !AreaIsOpen<PalaceObject>(ref roomForStalfos, enemy.X, 4, 1, Math.Max(enemy.Y - 4, 2));
                            break;
                    }

                    if (reroll)
                    {
                        swapToId = largeEnemies[RNG.Next(0, largeEnemies.Length)];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return swapToId;
        }

        T RerollFlyingEnemyIfNeeded(Enemy<T> enemy, T swapToId)
        {
            if (PalaceGroup != PalaceGrouping.PalaceGp)
            {
                // The rando does not re-roll Moas in regular palaces (I just simplified the legacy behavior)
                if (enemy.IdByte == EnemiesRegularPalaceShared.ORANGE_MOA)
                {
                    swapToId = enemy.Id; // swap it back to Moa
                }
            }
            else // GP
            {
                bool? roomForBubble = null;
                bool? roomForBigBubble = null;
                bool? roomForKingBot = null;
                while (true)
                {
                    // Re-roll enemies that do not fit (get stuck in walls)
                    bool reroll = false;
                    switch (swapToId)
                    {
                        case EnemiesGreatPalace.SLOW_BUBBLE:
                        case EnemiesGreatPalace.FAST_BUBBLE:
                            reroll = !AreaIsOpen<GreatPalaceObject>(ref roomForBubble, enemy.X, enemy.Y, 1, 1);
                            break;

                        case EnemiesGreatPalace.BIG_BUBBLE:
                            reroll = !AreaIsOpen<GreatPalaceObject>(ref roomForBigBubble, enemy.X, enemy.Y, 2, 2);
                            break;

                        case EnemiesGreatPalace.KING_BOT:
                            reroll = !PositionKingBot(ref roomForKingBot, enemy); // updates enemy position on success
                            break;
                    }

                    if (reroll)
                    {
                        swapToId = flyingEnemies[RNG.Next(0, flyingEnemies.Length)];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return swapToId;
        }

        int? firstGenerator = null;
        for (int i = 0; i < ee.Enemies.Count; i++)
        {
            Enemy<T> enemy = ee.Enemies[i];

            if (mixLargeAndSmallEnemies)
            {
                if (enemy.IsShufflableSmallOrLarge())
                {
                    T swapToId = groundEnemies[RNG.Next(0, groundEnemies.Length)];
                    if (enemy.IsShufflableSmall() && largeEnemies.Contains(swapToId))
                    {
                        enemy.Y--; // subtract Y by 1 when switching a small enemy to a large enemy
                        swapToId = RerollLargeEnemyIfNeeded(enemy, swapToId);
                    }
                    else
                    {
                        swapToId = RerollLargeEnemyIfNeeded(enemy, swapToId);
                    }
                    enemy.Id = swapToId;
                    continue;
                }
            }
            else
            {
                if (enemy.IsShufflableLarge())
                {
                    T swapToId = largeEnemies[RNG.Next(0, largeEnemies.Length)];
                    swapToId = RerollLargeEnemyIfNeeded(enemy, swapToId);
                    enemy.Id = swapToId;
                    continue;
                }
                else if (enemy.IsShufflableSmall())
                {
                    T swapEnemy = smallEnemies[RNG.Next(0, smallEnemies.Length)];
                    enemy.Id = swapEnemy;
                    continue;
                }
            }

            if (enemy.IsShufflableFlying())
            {
                T swapToId = flyingEnemies[RNG.Next(0, flyingEnemies.Length)];
                swapToId = RerollFlyingEnemyIfNeeded(enemy, swapToId);
                enemy.Id = swapToId;
                continue;
            }

            if (enemy.IsShufflableGenerator())
            {
                T swapToId = generators[RNG.Next(0, generators.Length)];
                firstGenerator ??= (int)(object)swapToId;
                if (generatorsAlwaysMatch)
                {
                    enemy.Id = (T)(object)firstGenerator;
                }
                else
                {
                    enemy.Id = swapToId;
                }
                continue;
            }
        }
        NewEnemies = ee.Finalize();
    }

    public void AdjustContinuingBossRoom()
    {
        const PalaceObject statueId = PalaceObject.IRON_KNUCKLE_STATUE;
        const byte statueXpos = 62;
        const byte statueYpos = 9;

        var edit = new SideviewEditable<PalaceObject>(SideView);
        var statue = edit.Find(o => o.Id == statueId && o.AbsX == statueXpos && o.Y == statueYpos);
        if (statue != null)
        {
            edit.Remove(statue);
            SideView = edit.Finalize();
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
        sb.Append(coords + " ");
        sb.Append(CategorizeExits() + " ");
        sb.Append("[" + BitConverter.ToString(SideView).Replace("-", "") + "] ");
        sb.Append("[" + BitConverter.ToString(Enemies).Replace("-", "") + "]");
        return sb.ToString();
    }


    private const int CONFLICT = -100;
    public int FitsWithDown(Room? down)
    {
        if (down == null)
        {
            return 0;
        }
        //This room doesn't go down but the room below goes up.
        if (!HasDownExit && !HasDrop && down.HasUpExit)
        {
            return CONFLICT;
        }
        //Down
        if (HasDownExit)
        {
            //Drop down
            if (HasDrop && down.IsDropZone)
            {
                return 1;
            }
            if (ElevatorScreen >= 0 && down.ElevatorScreen >= 0 && down.HasUpExit && !HasDrop)
            {
                return 1;
            }
            //types don't match
            return CONFLICT;
        }
        if (!HasDownExit && !HasDrop && !down.HasUpExit)
        {
            return 0;
        }
        throw new ImpossibleException("Room " + Name + Util.ByteArrayToHexString(SideView) +
            " Failed in FitsWithDown");
    }

    public int FitsWithUp(Room? up)
    {
        if (up == null)
        {
            return 0;
        }
        //There is a room above, but the room above goes down in a way that doesn't support this room
        //Or, this room goes up, but the room above can't do down
        if ((up.HasDrop && !IsDropZone)
            || (up.HasDrop && !IsDropZone)
            || (HasUpExit && !up.HasDownExit))
        {
            return CONFLICT;
        }
        //Up
        if (HasUpExit)
        {
            if (ElevatorScreen >= 0 && up.ElevatorScreen >= 0 && up.HasDownExit)
            {
                return 1;
            }
            //types don't match
            return CONFLICT;
        }
        else
        {
            if(up.HasDownExit)
            {
                return up.HasDrop && IsDropZone ? 1 : CONFLICT;
            }
        }
        //There is a room Above, but they don't interact at all
        //Theoretically we could just default true but this is a good safety
        if(!HasUpExit && !up.HasDownExit && !up.HasDrop)
        {
            return 0;
        }
        throw new ImpossibleException("Room " + Name + Util.ByteArrayToHexString(SideView) +
            " failed in FitsWithUp");
    }

    public int FitsWithLeft(Room? left)
    {
        //There is no left room or this and the room on the left don't interact
        if (left == null || (!left.HasRightExit && !HasLeftExit))
        {
            return 0;
        }
        //This room doesn't go left, but the room to the left goes right
        if (!HasLeftExit && left.HasRightExit)
        {
            return CONFLICT;
        }
        return left.HasRightExit ? 1 : CONFLICT;
    }
    public int FitsWithRight(Room? right)
    {
        //There is no right room or this and the room on the right don't interact
        if (right == null || (!right.HasLeftExit && !HasRightExit))
        {
            return 0;
        }
        //This room doesn't go right, but the room to the left goes left
        if (!HasRightExit && right.HasLeftExit)
        {
            return CONFLICT;
        }
        return right.HasLeftExit ? 1 : CONFLICT;
    }

    public HashSet<Coord> GetOpenExitCoords()
    {
        HashSet<Coord> exitCoords = [];
        var (x, y) = coords;
        if (coords == Coord.Uninitialized && !IsRoot)
        {
            throw new Exception("Uninitialized coordinates referenced in coordinate palace generation");
        }
        if (HasLeftExit && Left == null)
        {
            exitCoords.Add(new Coord(x - 1, y));
        }
        if (HasRightExit && Right == null)
        {
            exitCoords.Add(new Coord(x + 1, y));
        }
        if (HasUpExit && Up == null)
        {
            exitCoords.Add(new Coord(x, y + 1));
        }
        if (HasDownExit && Down == null)
        {
            exitCoords.Add(new Coord(x, y - 1));
        }

        return exitCoords;
    }

    public RoomExitType CategorizeExits()
    {
        /*
        if(ElevatorScreen >= 0 && (HasDrop || IsDropZone))
        {
            throw new Exception("Room has a mixed drop type. Add categories for this.");
        }
        */
        int flags =
            (HasLeftExit ? RoomExitTypeExtensions.LEFT : 0)
            + (HasRightExit ? RoomExitTypeExtensions.RIGHT : 0)
            + (HasUpExit ? RoomExitTypeExtensions.UP : 0)
            + (HasDownExit && HasDrop ? RoomExitTypeExtensions.DROP : 0)
            + (HasDownExit && !HasDrop ? RoomExitTypeExtensions.DOWN : 0);

        return (RoomExitType)flags;
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
        if (HasDownExit && !HasDrop && Down != null && Down.Up != this)
        {
            return false;
        }
        return true;
    }

    public bool ConnectRandomly(Room otherRoom, Random r)
    {
        Direction[] directons = [Direction.NORTH, Direction.SOUTH, Direction.WEST, Direction.EAST];
        r.Shuffle(directons);

        foreach (Direction direction in directons)
        {
            switch(direction)
            {
                case Direction.NORTH:
                    if(FitsWithUp(otherRoom) == 1 && ElevatorScreen > -1 && otherRoom.ElevatorScreen > -1)
                    {
                        Up = otherRoom;
                        return true;
                    }
                    break;
                case Direction.SOUTH:
                    if (FitsWithDown(otherRoom) == 1)
                    {
                        Down = otherRoom;
                        return true;
                    }
                    break;
                case Direction.WEST:
                    if (FitsWithLeft(otherRoom) == 1)
                    {
                        Left = otherRoom;
                        return true;
                    }
                    break;
                case Direction.EAST:
                    if (FitsWithRight(otherRoom) == 1)
                    {
                        Right = otherRoom;
                        return true;
                    }
                    break;
            }
        }
        return false;
    }

    public Room Merge(Room otherRoom)
    {
        Room mergedRoom = new Room(this);
        mergedRoom.HasLeftExit = HasLeftExit | otherRoom.HasLeftExit;
        mergedRoom.HasRightExit = HasRightExit | otherRoom.HasRightExit;
        mergedRoom.HasUpExit = HasUpExit | otherRoom.HasUpExit;
        mergedRoom.HasDownExit = HasDownExit | otherRoom.HasDownExit;
        mergedRoom.IsDropZone = IsDropZone | otherRoom.IsDropZone;
        mergedRoom.HasDrop = HasDrop | otherRoom.HasDrop;
        mergedRoom.MergedPrimary = this;
        mergedRoom.MergedSecondary = otherRoom;

        return mergedRoom;
    }

    public bool IsOrphaned()
    {
        return (HasLeftExit && Left != null)
            || (HasUpExit && Up != null)
            || (HasDownExit && Down != null)
            || (HasRightExit && Right != null);
    }

    public bool IsNormalRoom()
    {
        return !(IsBossRoom || IsThunderBirdRoom || HasItem || IsEntrance);
    }

    public bool IsOpen()
    {
        return HasLeftExit && Left == null
            || HasRightExit && Right == null
            || HasUpExit && Up == null
            || HasDownExit && Down == null;
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
