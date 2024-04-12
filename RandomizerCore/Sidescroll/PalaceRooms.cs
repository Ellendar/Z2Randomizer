using Newtonsoft.Json;
using RandomizerCore.Sidescroll;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Z2Randomizer.Core.Sidescroll;

//We want this to be statically initialized for performance in bulk seed generation, but C# doesn't allow static overrides
//I looked at other options and this was the least hacky. I would be shocked if there weren't a better way.
public class PalaceRooms
{
    private static readonly Dictionary<RoomGroup, List<Room>> roomsByGroup = new();
    private static readonly Dictionary<RoomGroup, List<Room>> customRoomsByGroup = new();

    private static readonly Dictionary<string, Room> roomsByName = new();
    private static readonly Dictionary<string, Room> customRoomsByName = new();



    public static readonly string roomsMD5 = "bALtB+T2aJuye3op9TW3sg==";

    static PalaceRooms()
    {
        if (Util.FileExists("PalaceRooms.json"))
        {
            string roomsJson = Util.ReadAllTextFromFile("PalaceRooms.json");
            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(Regex.Replace(roomsJson, @"[\n\r\f]", "")));
            if (roomsMD5 != Convert.ToBase64String(hash))
            {
                throw new Exception("Invalid PalaceRooms.json");
            }
            dynamic rooms = JsonConvert.DeserializeObject(roomsJson);
            foreach (var obj in rooms)
            {
                Room room = new(obj.ToString());
                if (room.Enabled)
                {
                    if (!roomsByGroup.ContainsKey(room.Group))
                    {
                        roomsByGroup.Add(room.Group, new List<Room>());
                    }
                    roomsByGroup[room.Group].Add(room);
                }
                roomsByName[room.Name] = room;
            }
        }
        else
        {
            throw new Exception("Unable to find PalaceRooms.json. Consider reinstalling or contact Ellendar.");
        }

        if (Util.FileExists("CustomRooms.json"))
        {
            string roomsJson = Util.ReadAllTextFromFile("CustomRooms.json");
            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(Regex.Replace(roomsJson, @"[\n\r\f]", "")));

            dynamic rooms = JsonConvert.DeserializeObject(roomsJson);
            foreach (var obj in rooms)
            {
                Room room = new(obj.ToString());
                if (room.Enabled)
                {
                    if (!customRoomsByGroup.ContainsKey(room.Group))
                    {
                        customRoomsByGroup.Add(room.Group, new List<Room>());
                    }
                    customRoomsByGroup[room.Group].Add(room);
                }
                customRoomsByName[room.Name] = room;
            }
        }
    }

    public static IEnumerable<Room> VanillaPalaceRoomsByPalaceNumber(int palaceNum, bool customRooms)
    {
        int mapMin, mapMax, palaceGroup;
        switch(palaceNum)
        {
            case 1:
                mapMin = 0;
                mapMax = 13;
                palaceGroup = 1;
                break;
            case 2:
                mapMin = 14;
                mapMax = 34;
                palaceGroup = 1;
                break;
            case 3:
                mapMin = 0;
                mapMax = 14;
                palaceGroup = 2;
                break;
            case 4:
                mapMin = 15;
                mapMax = 35;
                palaceGroup = 2;
                break;
            case 5:
                mapMin = 35;
                mapMax = 62;
                palaceGroup = 1;
                break;
            case 6:
                mapMin = 36;
                mapMax = 62;
                palaceGroup = 2;
                break;
            case 7:
                mapMin = 0;
                mapMax = 54;
                palaceGroup = 3;
                break;
            default:
            throw new ArgumentException("Invalid palace number: " + palaceNum);
        }
        

        if(customRooms)
        {
            return customRoomsByGroup[RoomGroup.VANILLA].Where(
                i => i.PalaceGroup == palaceGroup 
                && i.Map >= mapMin
                && i.Map <= mapMax
                && !i.IsEntrance 
                && !i.IsBossRoom
                && !i.HasItem
                && !i.IsThunderBirdRoom
            );
        }

        return roomsByGroup[RoomGroup.VANILLA].Where(
            i => i.PalaceGroup == palaceGroup
            && i.Map >= mapMin
            && i.Map <= mapMax
            && !i.IsEntrance
            && !i.IsBossRoom
            && !i.HasItem
            && !i.IsThunderBirdRoom
        );
    }

    public static IEnumerable<Room> TBirdRooms(RoomGroup group, bool useCustomRooms = false)
    {
        if (useCustomRooms)
        {
            return customRoomsByGroup[group].Where(i => i.IsThunderBirdRoom);
        }
        return roomsByGroup[group].Where(i => i.IsThunderBirdRoom);
    }

    public static Room VanillaBossRoom(int palaceNum)
    {
        int map;
        switch(palaceNum)
        {
            case 1:
                map = 13;
                break;
            case 2:
                map = 34;
                break;
            case 3:
                map = 14;
                break;
            case 4:
                map = 28;
                break;
            case 5:
                map = 41;
                break;
            case 6:
                map = 58;
                break;
            case 7:
                map = 54;
                break;
            default:
                throw new ArgumentException("Invalid palace number: " + palaceNum);
        }
        return roomsByGroup[RoomGroup.VANILLA].Where(i => i.IsBossRoom && map == i.Map).First();
    }

    public static Room VanillaItemRoom(int palaceNum)
    {
        int map;
        switch (palaceNum)
        {
            case 1:
                map = 8;
                break;
            case 2:
                map = 20;
                break;
            case 3:
                map = 11;
                break;
            case 4:
                map = 31;
                break;
            case 5:
                map = 61;
                break;
            case 6:
                map = 44;
                break;
            case 7:
                throw new ArgumentException("GP Cannot have an item!");
            default:
                throw new ArgumentException("Invalid palace number: " + palaceNum);
        }
        return roomsByGroup[RoomGroup.VANILLA].Where(i => i.HasItem && map == i.Map).First();
    }

    public static IEnumerable<Room> ItemRoomsByDirection(RoomGroup group, Direction direction, bool useCustomRooms = false)
    {
        if(direction == Direction.NONE)
        {
            throw new ArgumentException("Invalid Direction.NONE in ItemRoomsByDirection");
        }

        Dictionary<RoomGroup, List<Room>> rooms = useCustomRooms ? customRoomsByGroup : roomsByGroup;
        switch(direction)
        {
            case Direction.HORIZONTAL_PASSTHROUGH:
                return rooms[group].Where(i => i.HasItem && i.HasLeftExit() && i.HasRightExit());
            case Direction.VERTICAL_PASSTHROUGH:
                return rooms[group].Where(i => i.HasItem && i.HasUpExit() && i.HasDownExit());
            case Direction.NORTH:
                return rooms[group].Where(i => i.HasItem && i.HasUpExit());
            case Direction.SOUTH:
                return rooms[group].Where(i => i.HasItem && i.HasDownExit());
            case Direction.WEST:
                return rooms[group].Where(i => i.HasItem && i.HasLeftExit());
            case Direction.EAST:
                return rooms[group].Where(i => i.HasItem && i.HasRightExit());
        }

        throw new ImpossibleException("Invalid direction in ItemRoomsByDirection");
    }

    public static IEnumerable<Room> NormalPalaceRoomsByGroup(RoomGroup group, bool useCustomRooms = false)
    {
        if (useCustomRooms)
        {
            return customRoomsByGroup[group].Where(i => (i.PalaceNumber ?? 1) != 7 
                && !i.IsThunderBirdRoom && !i.HasItem && !i.IsBossRoom && !i.IsEntrance);
        }
        return roomsByGroup[group].Where(i => (i.PalaceNumber ?? 1) != 7 
            && !i.IsThunderBirdRoom && !i.HasItem && !i.IsBossRoom && !i.IsEntrance);
    }

    public static IEnumerable<Room> GPRoomsByGroup(RoomGroup group, bool useCustomRooms = false)
    {
        if (useCustomRooms)
        {
            return customRoomsByGroup[group].Where(i => (i.PalaceNumber ?? 1) == 7
                && !i.IsThunderBirdRoom && !i.HasItem && !i.IsBossRoom && !i.IsEntrance);
        }
        return roomsByGroup[group].Where(i => (i.PalaceNumber ?? 1) == 7
            && !i.IsThunderBirdRoom && !i.HasItem && !i.IsBossRoom && !i.IsEntrance);
    }

    public static IEnumerable<Room> Entrances(RoomGroup group, bool useCustomRooms = false)
    {
        if (useCustomRooms)
        {
            return customRoomsByGroup[group].Where(i => i.IsEntrance);
        }
        return roomsByGroup[group].Where(i => i.IsEntrance);
    }

    public static IEnumerable<Room> BossRooms(RoomGroup group, bool useCustomRooms = false, int? palaceNum = null)
    {
        if (useCustomRooms)
        {
            return customRoomsByGroup[group].Where(i => i.IsBossRoom && (palaceNum == null || palaceNum == i.PalaceNumber));
        }
        return roomsByGroup[group].Where(i => i.IsBossRoom && (palaceNum == null || palaceNum == i.PalaceNumber));
    }
    public static Room GetRoomByName(string name, bool useCustomRooms = false)
    {
        if (useCustomRooms)
        {
            return customRoomsByName[name];
        }
        return roomsByName[name];
    }
}
