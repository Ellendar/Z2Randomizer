using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RandomizerCore.Sidescroll;

public class CartesianPalaceGenerator(CancellationToken ct) : PalaceGenerator
{
    private static int debug = 0;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly IEqualityComparer<byte[]> byteArrayEqualityComparer = new Util.StandardByteArrayEqualityComparer();
    private const int STALL_LIMIT = 1000;
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

        int stallCount = 0;
        int openJunctionsCount = 0;
        while (palace.AllRooms.Count + openCoords.Count < roomCount || openJunctionsCount > 0)
        {
            //Stalled out, try again from the start
            if(openCoords.Count == 0 || stallCount++ > STALL_LIMIT)
            {
                palace.IsValid = false;
                return palace;
            }
            Room? newRoom = roomPool.NormalRooms.Sample(r);
            if (newRoom == null)
            {
                palace.IsValid = false;
                return palace;
            }
            else
            {
                newRoom = new(newRoom);
            }

            openCoords.FisherYatesShuffle(r);
            (int, int) bestFit = (0,0);
            int bestFitExitCount = 0;
            foreach((int, int) openCoord in openCoords) 
            {
                newRoom.coords = openCoord;
                int fitExitCount = newRoom.FitsWithLeft(palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1 - 1, openCoord.Item2)))
                    + newRoom.FitsWithDown(palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1, openCoord.Item2 - 1)))
                    + newRoom.FitsWithUp(palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1, openCoord.Item2 + 1)))
                    + newRoom.FitsWithRight(palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1 + 1, openCoord.Item2)));

                if(fitExitCount > bestFitExitCount)
                {
                    bestFitExitCount = fitExitCount;
                    bestFit = openCoord;
                }
            }

            if(bestFitExitCount > 0 && bestFit != (0, 0))
            {
                newRoom.coords = bestFit;
                List<(int, int)> newOpenCoords = newRoom.GetOpenExitCoords();
                foreach ((int, int) coord in newOpenCoords.ToList())
                {
                    if (openCoords.Contains(coord) || palace.AllRooms.Any(i => i.coords == coord))
                    {
                        newOpenCoords.Remove(coord);
                    }
                }
                //If adding this room would cause the number of open coordinates to be too large,
                //keep searching for a room that more precisely fits the available space.
                if (newOpenCoords.Count + openCoords.Count + palace.AllRooms.Count > roomCount)
                {
                    continue;
                }
                if (props.NoDuplicateRoomsBySideview)
                {
                    if (palace.AllRooms.Any(i => byteArrayEqualityComparer.Equals(i.SideView, newRoom.SideView)))
                    {
                        continue;
                    }
                }
                if (props.NoDuplicateRooms)
                {
                    roomPool.NormalRooms.Remove(newRoom);
                }
                Room left = palace.AllRooms.FirstOrDefault(i => i.coords == (bestFit.Item1 - 1, bestFit.Item2))!;
                Room down = palace.AllRooms.FirstOrDefault(i => i.coords == (bestFit.Item1, bestFit.Item2 - 1))!;
                Room up = palace.AllRooms.FirstOrDefault(i => i.coords == (bestFit.Item1, bestFit.Item2 + 1))!;
                Room right = palace.AllRooms.FirstOrDefault(i => i.coords == (bestFit.Item1 + 1, bestFit.Item2))!;
                if(newRoom.FitsWithLeft(left) > 0)
                {
                    newRoom.Left = left;
                    left.Right = newRoom;
                }
                if (newRoom.FitsWithDown(down) > 0)
                {
                    newRoom.Down = down;
                    down.Up = newRoom;
                }
                if (newRoom.FitsWithUp(up) > 0)
                {
                    newRoom.Up = up;
                    up.Down = newRoom;
                }
                if (newRoom.FitsWithRight(right) > 0)
                {
                    newRoom.Right = right;
                    right.Left = newRoom;
                }
                openCoords.AddRange(newOpenCoords);
                openCoords.Remove(bestFit);
                palace.AllRooms.Add(newRoom);
                stallCount = 0;

                //Count the number of open coordinates that are junction coordinates, i.e. they have
                //more than 1 room they need to connect to. We want to fill all of these in before capping paths
                //to prevent the capping logic from getting dumb.
                //I considered just categorizing all the rooms by type and doing logic to determine the appropriate cap,
                //but that logic tree to find what shape the hole is to fill with the appropriate peg got messy.
                openJunctionsCount = 0;
                foreach((int, int) coord in openCoords)
                {
                    //debug++;
                    Room? coordLeft = palace.AllRooms.FirstOrDefault(i => i.coords == (coord.Item1 - 1, coord.Item2));
                    Room? coordRight = palace.AllRooms.FirstOrDefault(i => i.coords == (coord.Item1 + 1, coord.Item2));
                    Room? coordUp = palace.AllRooms.FirstOrDefault(i => i.coords == (coord.Item1, coord.Item2 + 1));
                    Room? coordDown = palace.AllRooms.FirstOrDefault(i => i.coords == (coord.Item1, coord.Item2 - 1));

                    if((coordLeft != null && coordLeft.HasRightExit ? 1 : 0)
                        + (coordRight != null && coordRight.HasLeftExit ? 1 : 0)
                        + (coordUp != null && (coordUp.HasDownExit || coordUp.HasDrop) ? 1 : 0)
                        + (coordDown != null && coordDown.HasUpExit ? 1 : 0) >= 2)
                    {
                        openJunctionsCount++;
                    }
                }

                //Debug.WriteLine("Added Room at (" + newRoom.coords.Item1 + ", " + newRoom.coords.Item2 + ")");
            }
        }
        if(openCoords.Count > 0)
        {
            roomsByExitType = roomPool.CategorizeNormalRoomExits();

            foreach ((int, int) openCoord in openCoords.ToList())
            {
                Room? left = palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1 - 1, openCoord.Item2));
                Room? right = palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1 + 1, openCoord.Item2));
                Room? up = palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1, openCoord.Item2 + 1));
                Room? down = palace.AllRooms.FirstOrDefault(i => i.coords == (openCoord.Item1, openCoord.Item2 - 1));
                if ((left != null && left.HasRightExit ? 1 : 0)
                    + (right != null && right.HasLeftExit ? 1 : 0)
                    + (up != null && (up.HasDownExit || up.HasDrop) ? 1 : 0)
                    + (down != null && down.HasUpExit ? 1 : 0) >= 2)
                {
                    throw new Exception("Junction remains in stub closing that should have been cleaned up");
                }

                bool placed = false;
                do
                {
                    RoomExitType exitType;
                    if (left != null && left.HasRightExit)
                    {
                        exitType = RoomExitType.DEADEND_LEFT;
                    }
                    else if (right != null && right.HasLeftExit)
                    {
                        exitType = RoomExitType.DEADEND_RIGHT;
                    }
                    else if (up != null && up.HasDownExit)
                    {
                        if (up.HasDrop)
                        {
                            exitType = RoomExitType.DROP_DEADEND_UP;
                            logger.Debug("Drop stubs are currently unsupported. Ask discord how we feel about these");
                            palace.IsValid = false;
                            return palace;
                        }
                        else
                        {
                            exitType = RoomExitType.DEADEND_UP;
                        }
                    }
                    else if (down != null && down.HasUpExit)
                    {
                        exitType = RoomExitType.DEADEND_DOWN;
                    }
                    else
                    {
                        throw new ImpossibleException("Open coordinate has no adjacent exits");
                    }
                    roomsByExitType.TryGetValue(exitType, out var possibleStubs);
                    Room? newRoom = possibleStubs?.Sample(r);
                    if (newRoom == null)
                    {
                        roomPool.StubsByDirection.TryGetValue(exitType, out newRoom);
                    }
                    //The most likely cause of this is roomPool exhaustion in no duplicate rooms seeds.
                    //There could be some compromise to try and save it, but might as well regen
                    if (newRoom == null)
                    {
                        palace.IsValid = false;
                        return palace;
                    }
                    else
                    {
                        newRoom = new(newRoom);
                        //If the stub is a drop zone, pretend it isn't, otherwise junctions can appear
                        //as a result of adding the stub.
                        if (newRoom.IsDropZone)
                        {
                            newRoom.IsDropZone = false;
                        }
                    }
                    newRoom.coords = openCoord;
                    palace.AllRooms.Add(newRoom);
                    openCoords.Remove(openCoord);
                    placed = true;

                    if (left != null)
                    {
                        newRoom.Left = left;
                        left.Right = newRoom;
                    }
                    if (down != null)
                    {
                        newRoom.Down = down;
                        down.Up = newRoom;
                    }
                    if (up != null)
                    {
                        newRoom.Up = up;
                        up.Down = newRoom;
                    }
                    if (right != null)
                    {
                        newRoom.Right = right;
                        right.Left = newRoom;
                    }

                    if (props.NoDuplicateRooms && newRoom.Group != RoomGroup.STUBS)
                    {
                        roomsByExitType[exitType].Remove(newRoom);
                    }
                    if (props.NoDuplicateRoomsBySideview)
                    {
                        if (palace.AllRooms.Any(i => byteArrayEqualityComparer.Equals(i.SideView, newRoom.SideView))
                            && newRoom.Group != RoomGroup.STUBS)
                        {
                            roomsByExitType[exitType].Remove(newRoom);
                            placed = false;
                        }
                    }
                } while (placed == false);
            }
        }

        if (palace.AllRooms.Count > roomCount)
        {
            throw new ImpossibleException("Palace Room count exceeds maximum room count.");
        }
        if(openCoords.Count != 0)
        {
            throw new ImpossibleException("Stray open coordinate after palace is generated");
        }

        //Recategorize the remaining rooms after stubbing out.
        roomsByExitType = roomPool.CategorizeNormalRoomExits();


        //ItemRoom
        if(palace.Number < 7)
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
                    palace.AllRooms.Where(i => i.CategorizeExits() == itemRoomCandidate.CategorizeExits()).ToList();
                Room? itemRoomReplacementRoom = itemRoomReplacementCandidates.Sample(r);
                if (itemRoomReplacementRoom != null)
                {
                    palace.AllRooms.Remove(itemRoomReplacementRoom);
                    palace.ItemRoom = new(itemRoomCandidate);
                    palace.AllRooms.Add(palace.ItemRoom);
                    palace.ItemRoom.coords = itemRoomReplacementRoom.coords;
                    palace.ItemRoom.PalaceGroup = palaceGroup;
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
                    palace.AllRooms.Where(i => i.CategorizeExits() == tbirdRoomCandidate.CategorizeExits()).ToList();
                Room? tbirdRoomReplacementRoom = tbirdRoomReplacementCandidates.Sample(r);
                if (tbirdRoomReplacementRoom != null)
                {
                    palace.AllRooms.Remove(tbirdRoomReplacementRoom);
                    palace.Tbird = new(tbirdRoomCandidate);
                    palace.AllRooms.Add(palace.Tbird);
                    palace.Tbird.coords = tbirdRoomReplacementRoom.coords;
                    palace.Tbird.PalaceGroup = palaceGroup;
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
            List<Room> bossRoomReplacementCandidates =
                palace.AllRooms.Where(i => i.CategorizeExits() == bossRoomCandidate.CategorizeExits()).ToList();
            Room? bossRoomReplacementRoom = bossRoomReplacementCandidates.Sample(r);
            if (bossRoomReplacementRoom != null)
            {
                palace.AllRooms.Remove(bossRoomReplacementRoom);
                palace.BossRoom = new(bossRoomCandidate);
                palace.AllRooms.Add(palace.BossRoom);
                palace.BossRoom.coords = bossRoomReplacementRoom.coords;
                palace.BossRoom.PalaceGroup = palaceGroup;
                break;
            }
        }

        if(palace.BossRoom == null
            || palace.Entrance == null
            || (palaceNumber == 7 && palace.Tbird == null)
            || (palaceNumber < 7 && palace.ItemRoom == null))
        {
            logger.Debug("Failed to place critical room in palace");
            palace.IsValid = false;
            return palace;
        }

        if(palace.AllRooms.Count != roomCount)
        {
            throw new Exception("Generated palace has the incorrect number of rooms");
        }
        //TODO: tbird required logic
        //TODO: passthru boss rooms (directly on the room pool)

        palace.IsValid = true;
        return palace;
    }
}
