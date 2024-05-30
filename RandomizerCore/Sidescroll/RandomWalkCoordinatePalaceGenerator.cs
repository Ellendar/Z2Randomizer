using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RandomizerCore.Sidescroll;

public class RandomWalkCoordinatePalaceGenerator(CancellationToken ct) : PalaceGenerator
{
    private static int debug = 0;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly IEqualityComparer<byte[]> byteArrayEqualityComparer = new Util.StandardByteArrayEqualityComparer();
    private const int STALL_LIMIT = 1000;
    internal override Palace GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        debug++;
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

        roomsByExitType = roomPool.CategorizeNormalRoomExits();

        //for each node
        //if the node contains down
        //if the node below is a deadend, drop chance 0%
        //if the node above drops in, drop chance 100% (we don't have drop -> elevator conversion rooms)
        //otherwise drop chance is 30%

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
            double dropChance = .3d;
            Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == (x, y + 1));
            if (upRoom != null && upRoom.HasDrop)
            {
                dropChance = 1f;
            }
            if (downExitType == null || downExitType == RoomExitType.DEADEND_EXIT_UP)
            {
                dropChance = 0f;
            }

            if (r.NextDouble() < dropChance)
            {
                walkGraph[item.Key] = item.Value.ConvertToDrop();
                walkGraph[(x, y - 1)] = walkGraph[(x, y - 1)].RemoveUp();
            }
        }

        roomsByExitType = roomPool.CategorizeNormalRoomExits();
        foreach (KeyValuePair<(int, int), RoomExitType> item in walkGraph)
        {
            if (item.Key == (0, 0))
            {
                continue;
            }
            int x = item.Key.Item1;
            int y = item.Key.Item2;
            roomsByExitType.TryGetValue(item.Value, out var roomCandidates);
            roomCandidates ??= [];
            roomCandidates.FisherYatesShuffle(r);
            Room newRoom = null;
            Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == (x, y + 1));
            foreach (Room roomCandidate in roomCandidates)
            {
                if(upRoom == null || (upRoom.HasDrop == roomCandidate.IsDropZone))
                {
                    newRoom = roomCandidate;
                    break;
                }
            }
            if (newRoom == null)
            {
                roomPool.StubsByDirection.TryGetValue(item.Value, out newRoom);
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

            //Connect adjacent rooms if they exist
            Room? left = palace.AllRooms.FirstOrDefault(i => i.coords == (item.Key.Item1 - 1, item.Key.Item2));
            Room? down = palace.AllRooms.FirstOrDefault(i => i.coords == (item.Key.Item1, item.Key.Item2 - 1));
            Room? up = palace.AllRooms.FirstOrDefault(i => i.coords == (item.Key.Item1, item.Key.Item2 + 1));
            Room? right = palace.AllRooms.FirstOrDefault(i => i.coords == (item.Key.Item1 - 1, item.Key.Item2));
            if (left != null && newRoom.FitsWithLeft(left) > 0)
            {
                newRoom.Left = left;
                left.Right = newRoom;
            }
            if (down != null && newRoom.FitsWithDown(down) > 0)
            {
                newRoom.Down = down;
                if (!newRoom.HasDrop)
                {
                    down.Up = newRoom;
                }
            }
            if (up != null && newRoom.FitsWithUp(up) > 0)
            {
                if (!up.HasDrop)
                {
                    newRoom.Up = up;
                }
                up.Down = newRoom;
            }
            if (right != null && newRoom.FitsWithRight(right) > 0)
            {
                newRoom.Right = right;
                right.Left = newRoom;
            }

            palace.AllRooms.Add(newRoom);
            newRoom.coords = item.Key;
        }

        //ItemRoom
        if (palace.Number < 7)
        {
            if (roomPool.ItemRoomsByDirection.Values.Sum(i => i.Count) == 0)
            {
                throw new Exception("No item rooms for generated palace");
            }
            Direction itemRoomDirection;
            Room itemRoom = null;
            do
            {
                itemRoomDirection = DirectionExtensions.RandomItemRoomOrientation(r);
            } while (!roomPool.ItemRoomsByDirection.ContainsKey(itemRoomDirection));
            List<Room> itemRoomCandidates = roomPool.ItemRoomsByDirection[itemRoomDirection].ToList();
            itemRoomCandidates.FisherYatesShuffle(r);

            foreach (Room itemRoomCandidate in itemRoomCandidates)
            {
                List<Room> itemRoomReplacementCandidates =
                    palace.AllRooms.Where(i => i.CategorizeExits() == itemRoomCandidate.CategorizeExits() && i.IsNormalRoom()).ToList();
                Room? itemRoomReplacementRoom = itemRoomReplacementCandidates.Sample(r);
                if (itemRoomReplacementRoom != null)
                {
                    palace.ItemRoom = new(itemRoomCandidate);
                    palace.ReplaceRoom(itemRoomReplacementRoom, palace.ItemRoom);
                    break;
                }
            }
        }
        //Tbird Room
        else
        {
            if (roomPool.TbirdRooms.Count == 0)
            {
                throw new Exception("No tbird rooms for generated palace");
            }
            List<Room> tbirdRoomCandidates = roomPool.TbirdRooms.ToList();
            tbirdRoomCandidates.FisherYatesShuffle(r);

            foreach (Room tbirdRoomCandidate in tbirdRoomCandidates)
            {
                List<Room> tbirdRoomReplacementCandidates =
                    palace.AllRooms.Where(i => i.CategorizeExits() == tbirdRoomCandidate.CategorizeExits() && i.IsNormalRoom()).ToList();
                Room? tbirdRoomReplacementRoom = tbirdRoomReplacementCandidates.Sample(r);
                if (tbirdRoomReplacementRoom != null)
                {
                    palace.Tbird = new(tbirdRoomCandidate);
                    palace.ReplaceRoom(tbirdRoomReplacementRoom, palace.Tbird);
                    break;
                }
            }
        }

        //BossRoom
        if (roomPool.BossRooms.Count == 0)
        {
            throw new Exception("No boss rooms for generated palace");
        }
        List<Room> bossRoomCandidates = roomPool.BossRooms.ToList();
        bossRoomCandidates.FisherYatesShuffle(r);

        foreach (Room bossRoomCandidate in bossRoomCandidates)
        {
            RoomExitType bossRoomExitType = bossRoomCandidate.CategorizeExits();
            if (props.BossRoomConnect && palaceNumber < 7)
            {
                bossRoomExitType = (RoomExitType)((int)bossRoomExitType | RoomExitTypeExtensions.RIGHT);
            }
            List<Room> bossRoomReplacementCandidates =
                palace.AllRooms.Where(i => i.CategorizeExits() == bossRoomExitType && i.IsNormalRoom()).ToList();

            Room? bossRoomReplacementRoom = bossRoomReplacementCandidates.Sample(r);
            if (bossRoomReplacementRoom != null)
            {
                palace.BossRoom = new(bossRoomCandidate);
                if (props.BossRoomConnect && palaceNumber < 7)
                {
                    palace.BossRoom.HasRightExit = true;
                }
                palace.ReplaceRoom(bossRoomReplacementRoom, palace.BossRoom);
                break;
            }
        }

        if (palace.BossRoom == null
            || palace.Entrance == null
            || (palaceNumber == 7 && palace.Tbird == null)
            || (palaceNumber < 7 && palace.ItemRoom == null))
        {
            logger.Debug("Failed to place critical room in palace");
            palace.IsValid = false;
            return palace;
        }

        //TODO: This is REALLY late to abandon ship on the whole palace, but 
        //refactoring the boss placement to do the check properly without a million stupid room swaps
        //was not a thing I felt like figuring out.
        //Maybe we use ShuffleRooms()?
        //So for now we suffer lesser performance (but still way better than Reconstructed so do we care?)
        if (palaceNumber == 7 && props.RequireTbird && !palace.RequiresThunderbird())
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
