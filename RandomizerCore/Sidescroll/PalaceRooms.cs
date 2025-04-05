using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RandomizerCore.Sidescroll;

public partial class PalaceRooms
{
    private readonly Dictionary<RoomGroup, List<Room>> roomsByGroup = new();

    private readonly Dictionary<string, Room> roomsByName = new();


    private const string RoomsMd5 = "dKNxFT6dZjJevgj9khD11Q==";

    public PalaceRooms(string palaceJson, bool doValidation)
    {
        var hash = MD5Hash.ComputeHash(Encoding.UTF8.GetBytes(RemoveNewLines().Replace(palaceJson, "")));
        if (doValidation && RoomsMd5 != Convert.ToBase64String(hash))
        {
            throw new Exception("Invalid PalaceRooms.json");
        }

        var rooms = JsonSerializer.Deserialize(palaceJson, RoomSerializationContext.Default.ListRoom)!;
        foreach (var room in rooms)
        {
            if (room.Enabled)
            {
                if (!roomsByGroup.TryGetValue(room.Group, out var value))
                {
                    value = [];
                    roomsByGroup.Add(room.Group, value);
                }
            
                value.Add(room);
            }
            roomsByName[room.Name] = room;
        }
    }

    public IEnumerable<Room> VanillaPalaceRoomsByPalaceNumber(int palaceNum)
    {
        int mapMin, mapMax;
        PalaceGrouping palaceGroup = Util.AsPalaceGrouping(palaceNum) ?? throw new Exception("Invalid vanilla palace room without PalaceGroup set");
        switch (palaceNum)
        {
            case 1:
                mapMin = 0;
                mapMax = 13;
                break;
            case 2:
                mapMin = 14;
                mapMax = 34;
                break;
            case 3:
                mapMin = 0;
                mapMax = 14;
                break;
            case 4:
                mapMin = 15;
                mapMax = 35;
                break;
            case 5:
                mapMin = 35;
                mapMax = 62;
                break;
            case 6:
                mapMin = 36;
                mapMax = 62;
                break;
            case 7:
                mapMin = 0;
                mapMax = 54;
                break;
            default:
            throw new ArgumentException("Invalid palace number: " + palaceNum);
        }
        

        var roomgroup = roomsByGroup[RoomGroup.VANILLA];

        return roomgroup.Where(
            i => //i.Group == RoomGroup.VANILLA &&
                 Util.GetPalaceGroupingByMemoryAddress(i.ConnectionStartAddress) == palaceGroup
                 && i.Map >= mapMin
                 && i.Map <= mapMax
                 && i is { IsEntrance: false, IsBossRoom: false, HasItem: false, IsThunderBirdRoom: false }
        );
    }

    public IEnumerable<Room> ThunderBirdRooms(RoomGroup group)
    {
        var roomgroup = roomsByGroup[group];
        return roomgroup.Where(i => i.IsThunderBirdRoom);
    }

    public Room VanillaBossRoom(int palaceNum)
    {
        var map = palaceNum switch
        {
            1 => 13,
            2 => 34,
            3 => 14,
            4 => 28,
            5 => 41,
            6 => 58,
            7 => 54,
            _ => throw new ArgumentException("Invalid palace number: " + palaceNum)
        };
        return roomsByGroup[RoomGroup.VANILLA].First(i => i.IsBossRoom && map == i.Map);
    }

    public Room VanillaItemRoom(int palaceNum)
    {
        var map = palaceNum switch
        {
            1 => 8,
            2 => 20,
            3 => 11,
            4 => 31,
            5 => 61,
            6 => 44,
            7 => throw new ArgumentException("GP Cannot have an item!"),
            _ => throw new ArgumentException("Invalid palace number: " + palaceNum)
        };
        return roomsByGroup[RoomGroup.VANILLA].First(i => i.HasItem && map == i.Map);
    }

    public IEnumerable<Room> ItemRoomsByDirection(RoomGroup group, Direction direction)
    {
        if(direction == Direction.NONE)
        {
            throw new ArgumentException("Invalid Direction.NONE in ItemRoomsByDirection");
        }

        var rooms = roomsByGroup;
        return direction switch
        {
            //case Direction.HORIZONTAL_PASSTHROUGH:
            //    return rooms[group].Where(i => i.HasItem && i.HasLeftExit() && i.HasRightExit());
            //case Direction.VERTICAL_PASSTHROUGH:
            //    return rooms[group].Where(i => i.HasItem && i.HasUpExit() && i.HasDownExit());
            Direction.NORTH => rooms[group].Where(i => i.HasItem && i.HasUpExit),
            Direction.SOUTH => rooms[group].Where(i => i.HasItem && i.HasDownExit),
            Direction.WEST => rooms[group].Where(i => i.HasItem && i.HasLeftExit),
            Direction.EAST => rooms[group].Where(i => i.HasItem && i.HasRightExit),
            _ => throw new ImpossibleException("Invalid direction in ItemRoomsByDirection")
        };
    }

    public IEnumerable<Room> NormalPalaceRoomsByGroup(RoomGroup group)
    {
        var roomgroup = roomsByGroup[group];
        return roomgroup.Where(i => (i.PalaceNumber ?? 1) != 7 
            && i is { IsThunderBirdRoom: false, HasItem: false, IsBossRoom: false, IsEntrance: false });
    }

    public IEnumerable<Room> GpRoomsByGroup(RoomGroup group)
    {
        var roomgroup = roomsByGroup[group];
        return roomgroup.Where(i => (i.PalaceNumber ?? 1) == 7
            && i is { IsThunderBirdRoom: false, HasItem: false, IsBossRoom: false, IsEntrance: false });
    }

    public IEnumerable<Room> Entrances(RoomGroup group)
    {
        var roomgroup = roomsByGroup[group];
        return roomgroup.Where(i => i.IsEntrance);
    }

    public IEnumerable<Room> BossRooms(RoomGroup group, int? palaceNum = null)
    {
        var roomgroup = roomsByGroup[group];
        return roomgroup.Where(i => i.IsBossRoom && (palaceNum == null || palaceNum == i.PalaceNumber));
    }
    public Room GetRoomByName(string name)
    {
        return roomsByName[name];
    }

    public Dictionary<string, Room> LinkedRooms(RoomGroup group, int? palaceNum = null)
    {
        Dictionary<string, Room> linkedRooms = [];
        foreach (Room room in roomsByGroup[group])
        {
            if(room.Enabled && room.LinkedRoomName != null)
            {
                linkedRooms.Add(room.LinkedRoomName, GetRoomByName(room.LinkedRoomName));
            }
        }
        return linkedRooms;
    }

    [GeneratedRegex(@"[\n\r\f]")]
    private static partial Regex RemoveNewLines();
}
