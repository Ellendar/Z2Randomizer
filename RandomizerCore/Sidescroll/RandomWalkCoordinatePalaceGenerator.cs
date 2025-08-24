using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class RandomWalkCoordinatePalaceGenerator() : CoordinatePalaceGenerator()
{
    public static int debug = 0;
    private const float DROP_CHANCE = .15f;
    internal override async Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        debug++;
        bool duplicateProtection = (props.NoDuplicateRooms || props.NoDuplicateRoomsBySideview) && AllowDuplicatePrevention(props, palaceNumber);
        Palace palace = new(palaceNumber);
        List<Coord> openCoords = new();
        Dictionary<RoomExitType, List<Room>> roomsByExitType;
        RoomPool roomPool = new(rooms);
        // var palaceGroup = Util.AsPalaceGrouping(palaceNumber);
        Room entrance = new(roomPool.Entrances[r.Next(roomPool.Entrances.Count)])
        {
            IsRoot = true,
            // PalaceGroup = palaceGroup
        };
        if (props.UsePalaceItemRoomCountIndicator && palaceNumber != 7) {
            entrance.AdjustEntrance(props.PalaceItemRoomCounts[palaceNumber - 1], r);
        }
        openCoords.AddRange(entrance.GetOpenExitCoords());
        palace.AllRooms.Add(entrance);
        palace.Entrance = entrance;

        Dictionary<Coord, RoomExitType> walkGraph = [];
        walkGraph[Coord.Uninitialized] = entrance.CategorizeExits();

        var currentCoord = Coord.Uninitialized;

        //Back to even weight for now.
        WeightedRandom<int> weightedRandomDirection = new([
            (0, 35),  // left
            (1, 35),  // down
            (2, 35),  // up
            (3, 35),  // right
        ]);

        //Create graph
        while (walkGraph.Count < roomCount)
        {
            await Task.Yield();
            int direction = weightedRandomDirection.Next(r);
            Coord nextCoord = direction switch
            {
                0 => currentCoord with { X = currentCoord.X - 1 }, //left
                1 => currentCoord with { Y = currentCoord.Y - 1 }, //down
                2 => currentCoord with { Y = currentCoord.Y + 1 }, //up
                3 => currentCoord with { X = currentCoord.X + 1 }, //right
                _ => throw new ImpossibleException()
            };
            if (nextCoord == Coord.Uninitialized
                || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(-1, 0)) //can't ever go left from an entrance.
                || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(1, 0) && !entrance.HasRightExit)
                || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(0, 1) && !entrance.HasUpExit)
                || (currentCoord == Coord.Uninitialized && nextCoord == new Coord(0, -1) && !entrance.HasDownExit)
            )
            {
                continue;
            }

            walkGraph.TryAdd(nextCoord, RoomExitType.NO_ESCAPE);

            switch (direction)
            {
                case 0: //Left
                    walkGraph[nextCoord] = walkGraph[nextCoord].AddRight();
                    walkGraph[currentCoord] = walkGraph[currentCoord].AddLeft();
                    break;
                case 1: //Down
                    walkGraph[nextCoord] = walkGraph[nextCoord].AddUp();
                    walkGraph[currentCoord] = walkGraph[currentCoord].AddDown();
                    break;
                case 2: //Up
                    walkGraph[nextCoord] = walkGraph[nextCoord].AddDown();
                    walkGraph[currentCoord] = walkGraph[currentCoord].AddUp();
                    break;
                case 3: //Right
                    walkGraph[nextCoord] = walkGraph[nextCoord].AddLeft();
                    walkGraph[currentCoord] = walkGraph[currentCoord].AddRight();
                    break;
            }
            currentCoord = nextCoord;
        }

        //Dropify graph
        foreach (Coord coord in walkGraph.Keys.OrderByDescending(i => i.Y).ThenBy(i => i.X))
        {
            await Task.Yield();
            if(!walkGraph.TryGetValue(coord, out RoomExitType exitType))
            {
                throw new ImpossibleException("Walk graph coordinate was explicitly missing");
            }
            int x = coord.X;
            int y = coord.Y;

            RoomExitType? downExitType = null;
            if (walkGraph.ContainsKey(new Coord(x, y - 1)))
            {
                downExitType = walkGraph[new Coord(x, y - 1)];
            }
            else
            {
                continue;
            }
            double dropChance = DROP_CHANCE;
            //if we dropped into this room
            if (walkGraph.TryGetValue(new Coord(x, y + 1), out RoomExitType upRoomType) && upRoomType.ContainsDrop())
            {
                //If There are no drop -> elevator conversion rooms, so if we have to keep going down, it needs to be a drop.
                if(exitType.ContainsDown() && roomPool.GetNormalRoomsForExitType(RoomExitType.DROP_STUB).Any(i => i.IsDropZone))
                {
                    dropChance = 1f;
                } 
            }
            //if the path doesn't go down, or the room below doesn't exist, or the room below only goes up
            if (!exitType.ContainsDown() || downExitType == null || downExitType == RoomExitType.DEADEND_EXIT_UP)
            {
                dropChance = 0f;
            }

            if (r.NextDouble() < dropChance)
            {
                walkGraph[coord] = exitType.ConvertToDrop();
                walkGraph[new Coord(x, y - 1)] = walkGraph[new Coord(x, y - 1)].RemoveUp();
            }
        }

        //Debug.WriteLine(GetLayoutDebug(walkGraph, false));

        //If dropification created a room with no entrance, change it
        foreach (KeyValuePair<Coord, RoomExitType> item in walkGraph.Where(i => i.Value == RoomExitType.DROP_STUB))
        {
            if(!walkGraph.ContainsKey(new Coord(item.Key.X, item.Key.Y + 1)))
            {
                walkGraph[item.Key] = RoomExitType.DEADEND_EXIT_DOWN;
                RoomExitType downRoomType = walkGraph[new Coord(item.Key.X, item.Key.Y - 1)];
                walkGraph[new Coord(item.Key.X, item.Key.Y - 1)] = downRoomType.AddUp();
            }
        }

        //If dropification created a pit, convert it to an elevator.
        //This should never happen, but it's a good safety
        foreach (KeyValuePair<Coord, RoomExitType> item in walkGraph.Where(i => i.Value == RoomExitType.NO_ESCAPE))
        {
            walkGraph[item.Key] = RoomExitType.DEADEND_EXIT_UP;
            RoomExitType upRoomType = walkGraph[new Coord(item.Key.X, item.Key.Y + 1)];
            walkGraph[new Coord(item.Key.X, item.Key.Y + 1)] = upRoomType.ConvertFromDropToDown();
        }

        //Debug.WriteLine(GetLayoutDebug(walkGraph, false));

        //Add rooms
        roomsByExitType = roomPool.CategorizeNormalRoomExits(true);
        Dictionary<RoomExitType, bool> stubOnlyExitTypes = new();
        foreach (KeyValuePair<Coord, RoomExitType> item in walkGraph.OrderBy(i => i.Key.X).ThenByDescending(i => i.Key.Y))
        {
            if (item.Key == Coord.Uninitialized)
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
                    logger.Info($"RandomWalk ran out of rooms of exit type: {roomExitType} in palace {palaceNumber}. Starting to use duplicate rooms.");
                }
                roomCandidates!.FisherYatesShuffle(r);
                Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == new Coord(x, y + 1));
                foreach (Room roomCandidate in roomCandidates!)
                {
                    if ((upRoom == null || (upRoom.HasDrop == roomCandidate.IsDropZone)))
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
                roomPool.DefaultStubsByDirection.TryGetValue(roomExitType, out newRoom);
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

            palace.AllRooms.Add(newRoom);
            if(newRoom.LinkedRoomName != null)
            {
                Room linkedRoom = new(roomPool.LinkedRooms[newRoom.LinkedRoomName]);
                newRoom.LinkedRoom = linkedRoom;
                linkedRoom.LinkedRoom = newRoom;
                linkedRoom.coords = item.Key;
                palace.AllRooms.Add(linkedRoom);
            }
            newRoom.coords = item.Key;
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

            foreach (Room down in downRooms)
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
            foreach (Room up in upRooms)
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
            foreach (Room right in rightRooms)
            {
                if (right != null && room.FitsWithRight(right) > 0)
                {
                    room.Right = right;
                    right.Left = room;
                }
            }
        }

        //Some percentage of the time, dropifying some rooms causes part of the palace to become
        //unreachable because up was the only way to get there.
        if (!palace.AllReachable())
        {
            palace.IsValid = false;
            return palace;
        }

        if(!AddSpecialRoomsByReplacement(palace, roomPool, r, props))
        {
            palace.IsValid = false;
            return palace;
        }

        if (palace.AllRooms.Count(i => i.Enabled) != roomCount)
        {
            throw new Exception("Generated palace has the incorrect number of rooms");
        }

        palace.AllRooms.ForEach(i => i.PalaceNumber = palaceNumber);

        palace.IsValid = true;
        return palace;
    }

    public string GetLayoutDebug(Dictionary<Coord, RoomExitType> walkGraph, bool includeCoordinateGrid = true)
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
                    if(room.ContainsDown())
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

    private static readonly Regex BlankLine = new(@"^[ \t\f\r\n]+$");
}
