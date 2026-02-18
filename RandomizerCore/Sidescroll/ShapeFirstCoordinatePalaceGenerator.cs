using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public abstract class ShapeFirstCoordinatePalaceGenerator() : CoordinatePalaceGenerator()
{

    internal override async Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        bool duplicateProtection = (props.NoDuplicateRooms || props.NoDuplicateRoomsBySideview) && AllowDuplicatePrevention(props, palaceNumber);
        Palace palace = new(palaceNumber);
        Dictionary<RoomExitType, List<Room>> roomsByExitType;
        RoomPool roomPool = new(rooms);
        // var palaceGroup = Util.AsPalaceGrouping(palaceNumber);

        Dictionary<Coord, RoomExitType> shape;
        shape = GetPalaceShape(props, palace, roomPool, r, roomCount).Result;
        if(shape.Count == 0)
        {
            palace.IsValid = false;
            return palace;
        }


        List < Coord > prepopulatedCoordinates = [];
        prepopulatedCoordinates.Add(palace.AllRooms.FirstOrDefault(i => i.IsEntrance)?.coords ?? Coord.Uninitialized);
        prepopulatedCoordinates.Add(palace.AllRooms.FirstOrDefault(i => i.IsBossRoom)?.coords ?? Coord.Uninitialized);
        prepopulatedCoordinates.Add(palace.AllRooms.FirstOrDefault(i => i.IsThunderBirdRoom)?.coords ?? Coord.Uninitialized);

        //Add rooms
        roomsByExitType = roomPool.CategorizeNormalRoomExits(true);
        Dictionary<RoomExitType, bool> stubOnlyExitTypes = new();
        foreach (KeyValuePair<Coord, RoomExitType> item in shape.OrderBy(i => i.Key.X).ThenByDescending(i => i.Key.Y))
        {
            if (prepopulatedCoordinates.Contains(item.Key))
            {
                continue;
            }
            var (x, y) = item.Key;
            RoomExitType roomExitType = item.Value;

            bool stubOnly;
            List<Room>? roomCandidates;
            Room? newRoom = null;
            if (!stubOnlyExitTypes.TryGetValue(roomExitType, out stubOnly))
            {
                roomsByExitType.TryGetValue(roomExitType, out roomCandidates);
                stubOnly = roomCandidates == null || roomCandidates.Count == 0;
                stubOnlyExitTypes[roomExitType] = stubOnly;
            }
            else
            {
                roomCandidates = stubOnly ? null : roomsByExitType.GetValueOrDefault(roomExitType);
            }
            if (!stubOnly)
            {
                Debug.Assert(roomCandidates != null);
                if (duplicateProtection && roomCandidates!.Count == 0)
                {
                    roomCandidates = roomPool.GetNormalRoomsForExitType(roomExitType, true);
                    Debug.Assert(roomCandidates.Count() > 0);
                    roomsByExitType[roomExitType] = roomCandidates;
                    logger.Debug($"RandomWalk ran out of rooms of exit type: {roomExitType} in palace {palaceNumber}. Starting to use duplicate rooms.");
                }
                roomCandidates!.FisherYatesShuffle(r);
                Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == new Coord(x, y + 1));
                foreach (Room roomCandidate in roomCandidates!)
                {
                    if (upRoom == null || !upRoom.HasDrop || roomCandidate.IsDropZone)
                    {
                        Debug.Assert(roomCandidate.IsNormalRoom());
                        newRoom = roomCandidate;
                        break;
                    }
                }
                if (newRoom != null && duplicateProtection) { RemoveDuplicatesFromPool(props, roomCandidates!, newRoom); }
            }

            if (newRoom == null)
            {
                Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == new Coord(x, y + 1));
                roomPool.DefaultStubsByDirection.TryGetValue(roomExitType, out newRoom);
                if (newRoom != null && upRoom != null && upRoom.HasDrop && !newRoom.IsDropZone)
                {
                    //We need to use a drop zone stub but one does not (and cannot) exist so this graph is doomed.
                    //Debug.WriteLine(GetLayoutDebug(walkGraph, false));
                    palace.IsValid = false;
                    return palace;
                }
            }
            if (newRoom == null)
            {
                palace.IsValid = false;
                return palace;
            }
            else
            {
                newRoom = new(newRoom);
            }

            newRoom.coords = item.Key;
            if (newRoom.LinkedRoomName == null)
            {
                palace.AllRooms.Add(newRoom);
            }
            else
            {
                Room linkedRoom = new(roomPool.LinkedRooms[newRoom.LinkedRoomName]);
                newRoom.LinkedRoom = linkedRoom;
                linkedRoom.LinkedRoom = newRoom;
                linkedRoom.coords = item.Key;
                Room mergedRoom = newRoom.Merge(linkedRoom);
                palace.AllRooms.Add(mergedRoom);
            }
        }

        //Connect adjacent rooms if they exist
        foreach (Room room in palace.AllRooms)
        {
            await Task.Yield();
            Room[] leftRooms = palace.AllRooms.Where(i => i.coords == room.coords with { X = room.coords.X - 1 }).ToArray();
            Room[] downRooms = palace.AllRooms.Where(i => i.coords == room.coords with { Y = room.coords.Y - 1 }).ToArray();
            Room[] upRooms = palace.AllRooms.Where(i => i.coords == room.coords with { Y = room.coords.Y + 1 }).ToArray();
            Room[] rightRooms = palace.AllRooms.Where(i => i.coords == room.coords with { X = room.coords.X + 1 }).ToArray();

            foreach(Room left in leftRooms)
            {
                if (left != null && room.FitsWithLeft(left) > 0)
                {
                    room.Left = left;
                    left.Right = room;
                }
            }

            foreach(Room down in downRooms)
            {
                if (down != null && room.FitsWithDown(down) > 0)
                {
                    room.Down = down;
                    if (!room.HasDrop)
                    {
                        down.Up = room;
                    }
                }
            }
            foreach(Room up in upRooms)
            {
                if (up != null && room.FitsWithUp(up) > 0)
                {
                    if (!up.HasDrop)
                    {
                        room.Up = up;
                    }
                    up.Down = room;
                }
            }
            foreach(Room right in rightRooms)
            {
                if (right != null && room.FitsWithRight(right) > 0)
                {
                    room.Right = right;
                    right.Left = room;
                }
            }
        }

        ConnectNonEuclideanPaths(palace);

        //Some percentage of the time, dropifying some rooms causes part of the palace to become
        //unreachable because up was the only way to get there.
        if (!palace.AllReachable())
        {
            palace.IsValid = false;
            return palace;
        }


        if (!AddSpecialRoomsByReplacement(palace, roomPool, r, props))
        {
            palace.IsValid = false;
            return palace;
        }

        if (palace.AllRooms.Count(i => i.Enabled) != roomCount)
        {
            throw new Exception("Generated palace has the incorrect number of rooms");
        }

        if (palace.HasDisallowedDrop(props.BossRoomsExitToPalace[palace.Number - 1], props.PalaceDropStyle, r))
        {
            palace.IsValid = false;
            return palace;
        }

        palace.AllRooms.ForEach(i => i.PalaceNumber = palaceNumber);

        palace.IsValid = true;
        return palace;
    }

    public static string GetLayoutDebug(Dictionary<Coord, RoomExitType> walkGraph, bool includeCoordinateGrid = true)
    {
        StringBuilder sb = new();
        if (includeCoordinateGrid)
        {
            sb.Append("   ");
            for (int headerX = -20; headerX <= 20; headerX++)
            {
                sb.Append(headerX.ToString().PadLeft(3, ' '));
            }
            sb.Append('\n');
        }
        for (int y = 20; y >= -20; y--)
        {
            sb.Append("   ");
            for (int x = -20; x <= 20; x++)
            {
                if (!walkGraph.TryGetValue(new Coord(x, y), out RoomExitType room))
                {
                    sb.Append("   ");
                }
                else
                {
                    sb.Append(" " + (room.ContainsUp() ? "|" : " ") + " ");
                }
            }
            sb.Append('\n');
            sb.Append(includeCoordinateGrid ? y.ToString().PadLeft(3, ' ') : "   ");
            for (int x = -20; x <= 20; x++)
            {
                if (!walkGraph.TryGetValue(new Coord(x, y), out RoomExitType room))
                {
                    sb.Append("   ");
                }
                else
                {
                    sb.Append(room.ContainsLeft() ? '-' : ' ');
                    sb.Append('X');
                    sb.Append(room.ContainsRight() ? '-' : ' ');
                }
            }
            sb.Append('\n');
            sb.Append("   ");
            for (int x = -20; x <= 20; x++)
            {
                if (!walkGraph.TryGetValue(new Coord(x, y), out RoomExitType room))
                {
                    sb.Append("   ");
                }
                else
                {
                    if (room.ContainsDown())
                    {
                        sb.Append(" | ");
                    }
                    else if (room.ContainsDrop())
                    {
                        sb.Append(" v ");
                    }
                    else
                    {
                        sb.Append("   ");
                    }
                }
            }
            sb.Append('\n');
        }

        if (!includeCoordinateGrid)
        {
            StringBuilder condensed = new();
            foreach (string line in sb.ToString().Split('\n'))
            {
                if (!BlankLine.IsMatch(line))
                {
                    condensed.AppendLine(line);
                }
            }
            return condensed.ToString();
        }
        return sb.ToString();
    }

    protected abstract Task<Dictionary<Coord, RoomExitType>> GetPalaceShape(RandomizerProperties props, Palace palace, RoomPool roomPool, Random r, int roomCount);

    /// <summary>
    /// Eucildean palaces require no additional connections, but override this to add additional
    /// style-specific connection logic
    /// </summary>
    /// <param name="palace"></param>
    protected virtual void ConnectNonEuclideanPaths(Palace palace)
    {

    }

    private static readonly Regex BlankLine = new(@"^[ \t\f\r\n]+$");
}
