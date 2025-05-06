using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class RandomWalkCoordinatePalaceGenerator() : CoordinatePalaceGenerator()
{
    public static int debug = 0;
    private const float DROP_CHANCE = .10f;
    internal override async Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        debug++;
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
        openCoords.AddRange(entrance.GetOpenExitCoords());
        palace.AllRooms.Add(entrance);
        palace.Entrance = entrance;

        Dictionary<Coord, RoomExitType> walkGraph = [];
        walkGraph[Coord.Uninitialized] = entrance.CategorizeExits();

        var currentCoord = Coord.Uninitialized;

        //Create graph
        while (walkGraph.Count < roomCount)
        {
            await Task.Yield();
            int direction = r.Next(4);
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
        roomsByExitType = roomPool.CategorizeNormalRoomExits();
        foreach (KeyValuePair<Coord, RoomExitType> item in walkGraph.OrderByDescending(i => i.Key.X).ThenBy(i => i.Key.Y))
        {
            await Task.Yield();
            var (x, y) = item.Key;
            RoomExitType exitType = item.Value;
            if (item.Key == Coord.Uninitialized)
            {
                continue;
            }

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
            Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == new Coord(x, y + 1));
            //if we dropped into this room
            if (upRoom != null && upRoom.HasDrop)
            {
                //There are no drop -> elevator conversion rooms, so if we have to keep going down, it needs to be a drop.
                if(exitType.ContainsDown())
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
                walkGraph[item.Key] = item.Value.ConvertToDrop();
                walkGraph[new Coord(x, y - 1)] = walkGraph[new Coord(x, y - 1)].RemoveUp();
            }
        }

        //drop stubs can't / shouldn't exist, so convert them to regular down stubs
        foreach (KeyValuePair<Coord, RoomExitType> item in walkGraph.Where(i => i.Value == RoomExitType.DROP_STUB))
        {
            walkGraph[item.Key] = RoomExitType.DEADEND_EXIT_UP;
            walkGraph[item.Key with { Y = item.Key.Y - 1 }] = item.Value.ConvertFromDropToDown();
        }

        //Add rooms
        roomsByExitType = roomPool.CategorizeNormalRoomExits(true);
        foreach (KeyValuePair<Coord, RoomExitType> item in walkGraph.OrderBy(i => i.Key.X).ThenByDescending(i => i.Key.Y))
        {
            if (item.Key == Coord.Uninitialized)
            {
                continue;
            }
            var (x, y) = item.Key;
            RoomExitType roomExitType = item.Value;
            roomsByExitType.TryGetValue(roomExitType, out var roomCandidates);
            roomCandidates ??= [];
            roomCandidates.FisherYatesShuffle(r);
            Room? newRoom = null;
            Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == new Coord(x, y + 1));
            foreach (Room roomCandidate in roomCandidates)
            {
                if((upRoom == null || (upRoom.HasDrop == roomCandidate.IsDropZone))
                    && roomCandidate.IsNormalRoom())
                {
                    newRoom = roomCandidate;
                    break;
                }
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
                roomCount++;
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


        if (palace.AllRooms.Count != roomCount)
        {
            throw new Exception("Generated palace has the incorrect number of rooms");
        }

        palace.AllRooms.ForEach(i => i.PalaceNumber = palaceNumber);

        palace.IsValid = true;
        return palace;
    }
}
