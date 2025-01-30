using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RandomizerCore.Sidescroll;

public class RandomWalkCoordinatePalaceGenerator() : CoordinatePalaceGenerator()
{
    internal override Palace GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        Palace palace = new(palaceNumber);
        List<(int, int)> openCoords = new();
        Dictionary<RoomExitType, List<Room>> roomsByExitType;
        RoomPool roomPool = new(rooms);
        int palaceGroup = palaceNumber switch
        {
            1 => 1,
            2 => 1,
            3 => 2,
            4 => 2,
            5 => 1,
            6 => 2,
            7 => 3,
            _ => throw new ImpossibleException("Invalid palace number: " + palaceNumber)
        };
        Room entrance = new(roomPool.Entrances[r.Next(roomPool.Entrances.Count)])
        {
            IsRoot = true,
            PalaceGroup = palaceGroup
        };
        openCoords.AddRange(entrance.GetOpenExitCoords());
        palace.AllRooms.Add(entrance);
        palace.Entrance = entrance;

        Dictionary<(int, int), RoomExitType> walkGraph = [];
        walkGraph[(0, 0)] = entrance.CategorizeExits();

        (int, int) currentCoord = (0, 0);

        //Create graph
        while (walkGraph.Count < roomCount)
        {
            int direction = r.Next(4);
            (int, int) nextCoord = direction switch
            {
                0 => (currentCoord.Item1 - 1, currentCoord.Item2), //left
                1 => (currentCoord.Item1, currentCoord.Item2 - 1), //down
                2 => (currentCoord.Item1, currentCoord.Item2 + 1), //up
                3 => (currentCoord.Item1 + 1, currentCoord.Item2), //right
                _ => throw new ImpossibleException()
            };
            if (nextCoord == (0, 0)
                || (currentCoord == (0, 0) && nextCoord == (-1, 0)) //can't ever go left from an entrance.
                || (currentCoord == (0, 0) && nextCoord == (1, 0) && !entrance.HasRightExit)
                || (currentCoord == (0, 0) && nextCoord == (0, 1) && !entrance.HasUpExit)
                || (currentCoord == (0, 0) && nextCoord == (0, -1) && !entrance.HasDownExit)
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
        foreach (KeyValuePair<(int, int), RoomExitType> item in walkGraph.OrderByDescending(i => i.Key.Item2).ThenBy(i => i.Key.Item1))
        {
            int x = item.Key.Item1;
            int y = item.Key.Item2;
            RoomExitType exitType = item.Value;
            if (item.Key == (0, 0))
            {
                continue;
            }

            RoomExitType? downExitType = null;
            if (walkGraph.ContainsKey((x, y - 1)))
            {
                downExitType = walkGraph[(x, y - 1)];
            }
            else
            {
                continue;
            }
            double dropChance = .15d;
            Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == (x, y + 1));
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
                walkGraph[(x, y - 1)] = walkGraph[(x, y - 1)].RemoveUp();
            }
        }

        //Add rooms
        roomsByExitType = roomPool.CategorizeNormalRoomExits(true);
        foreach (KeyValuePair<(int, int), RoomExitType> item in walkGraph.OrderByDescending(i => i.Key.Item2).ThenBy(i => i.Key.Item1))
        {
            if (item.Key == (0, 0))
            {
                continue;
            }
            int x = item.Key.Item1;
            int y = item.Key.Item2;
            RoomExitType roomExitType = item.Value;
            roomsByExitType.TryGetValue(roomExitType, out var roomCandidates);
            roomCandidates ??= [];
            roomCandidates.FisherYatesShuffle(r);
            Room? newRoom = null;
            Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == (x, y + 1));
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
                roomPool.StubsByDirection.TryGetValue(roomExitType, out newRoom);
            }
            if (newRoom == null)
            {
                if(roomExitType == RoomExitType.DROP_STUB)
                {
                    if(upRoom?.IsDropZone ?? false)
                    {
                        upRoom.Down = upRoom;
                        continue;
                    }
                }
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
                palace.AllRooms.Add(new(roomPool.LinkedRooms[newRoom.LinkedRoomName]));
                roomCount++;
            }
            newRoom.coords = item.Key;
        }

        //Connect adjacent rooms if they exist
        foreach (Room room in palace.AllRooms)
        {
            Room[] leftRooms = palace.AllRooms.Where(i => i.coords == (room.coords.Item1 - 1, room.coords.Item2)).ToArray();
            Room[] downRooms = palace.AllRooms.Where(i => i.coords == (room.coords.Item1, room.coords.Item2 - 1)).ToArray();
            Room[] upRooms = palace.AllRooms.Where(i => i.coords == (room.coords.Item1, room.coords.Item2 + 1)).ToArray();
            Room[] rightRooms = palace.AllRooms.Where(i => i.coords == (room.coords.Item1 + 1, room.coords.Item2)).ToArray();

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
