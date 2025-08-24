using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class SequentialPlacementCoordinatePalaceGenerator() : CoordinatePalaceGenerator()
{
    public static int[] dropFailureCounts = [0, 0, 0, 0, 0, 0, 0];
    public static int[] stallFailureCounts = [0, 0, 0, 0, 0, 0, 0];
    public static int[] specialRoomFailureCounts = [0, 0, 0, 0, 0, 0, 0];

    private const int STALL_LIMIT = 5000;
    static int debug = 0;
    
    private Room? newRoom;
    private HashSet<Coord> openCoords;
    private Dictionary<Coord, Room> allRooms;
    private RandomizerProperties props;
    private int stallCount;
    private int openJunctionsCount;
    private int roomCount;
    private int palaceNumber;
    private RoomPool roomPool;
    
    internal override async Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        this.props = props;
        this.roomCount = roomCount;
        this.palaceNumber = palaceNumber;

        debug++;
        bool duplicateProtection = (props.NoDuplicateRooms || props.NoDuplicateRoomsBySideview) && AllowDuplicatePrevention(props, palaceNumber);
        Palace palace = new(palaceNumber);
        openCoords = new();
        Dictionary<RoomExitType, List<Room>> roomsByExitType;
        roomPool = new(rooms);
        // var palaceGroup = Util.AsPalaceGrouping(palaceNumber);
        Room entrance = new(roomPool.Entrances[r.Next(roomPool.Entrances.Count)])
        {
            IsRoot = true,
            // PalaceGroup = palaceGroup
        };
        openCoords.UnionWith(entrance.GetOpenExitCoords());
        
        allRooms= new() { { Coord.Uninitialized, entrance } };
        if (props.UsePalaceItemRoomCountIndicator && palaceNumber != 7) {
            entrance.AdjustEntrance(props.PalaceItemRoomCounts[palaceNumber - 1], r);
        }
        palace.Entrance = entrance;

        stallCount = 0;
        openJunctionsCount = 0;
        while (allRooms.Count + openCoords.Count < roomCount || openJunctionsCount > 0)
        {
            //Stalled out, try again from the start
            if(openCoords.Count == 0 || stallCount++ >= STALL_LIMIT)
            {
                await Task.Yield();
                palace.IsValid = false;
                if(stallCount++ >= STALL_LIMIT)
                {
                    //Debug.WriteLine(palace.GetLayoutDebug());
                    stallFailureCounts[palaceNumber - 1]++;
                }
                return palace;
            }
            newRoom = roomPool.NormalRooms.Sample(r);
            if (newRoom == null)
            {
                palace.IsValid = false;
                return palace;
            }

            newRoom = new(newRoom);
            if (newRoom.LinkedRoomName != null)
            {
                Room linkedRoom = rooms.LinkedRooms[newRoom.LinkedRoomName];
                newRoom = newRoom.Merge(new(linkedRoom));
            }

            var openCoordsIter = openCoords.ToArray();
            r.Shuffle(openCoordsIter);
            Coord bestFit = Coord.Uninitialized;
            int bestFitExitCount = 0;
            foreach(Coord openCoord in openCoordsIter) 
            {
                newRoom.coords = openCoord;
                var (x, y) = openCoord;
                int fitExitCount = newRoom.FitsWithLeft(allRooms.GetValueOrDefault(new Coord(x - 1, y)))
                    + newRoom.FitsWithDown(allRooms.GetValueOrDefault(new Coord(x, y - 1)))
                    + newRoom.FitsWithUp(allRooms.GetValueOrDefault(new Coord(x, y + 1)))
                    + newRoom.FitsWithRight(allRooms.GetValueOrDefault(new Coord(x + 1, y)));

                if(fitExitCount > bestFitExitCount)
                {
                    bestFitExitCount = fitExitCount;
                    bestFit = openCoord;
                }
            }

            if (bestFitExitCount <= 0 || bestFit == Coord.Uninitialized) continue;
            UpdateRoom(bestFit);
            if (duplicateProtection) { RemoveDuplicatesFromPool(props, roomPool.NormalRooms, newRoom); }
        }
        //close stubs
        if (openCoords.Count > 0)
        {
            roomsByExitType = roomPool.CategorizeNormalRoomExits();

            var openCoordsIter = openCoords.ToArray();
            foreach (var openCoord in openCoordsIter)
            {
                var (x, y) = openCoord;
                Room? left = allRooms.GetValueOrDefault(new Coord(x - 1, y));
                Room? right = allRooms.GetValueOrDefault(new Coord(x + 1, y));
                Room? up = allRooms.GetValueOrDefault(new Coord(x, y + 1));
                Room? down = allRooms.GetValueOrDefault(new Coord(x, y - 1));
                if ((left is { HasRightExit: true } ? 1 : 0)
                    + (right is { HasLeftExit: true } ? 1 : 0)
                    + (up != null && (up.HasDownExit || up.HasDrop) ? 1 : 0)
                    + (down is { HasUpExit: true } ? 1 : 0) >= 2)
                {
                    throw new Exception("Junction remains in stub closing that should have been cleaned up");
                }

                RoomExitType exitType;
                if (left is { HasRightExit: true })
                {
                    exitType = RoomExitType.DEADEND_EXIT_LEFT;
                }
                else if (right is { HasLeftExit: true })
                {
                    exitType = RoomExitType.DEADEND_EXIT_RIGHT;
                }
                else if (up is { HasDownExit: true })
                {
                    if (up.HasDrop)
                    {
                        exitType = RoomExitType.NO_ESCAPE;
                        dropFailureCounts[palaceNumber - 1]++;
                        //logger.Debug("Drop stubs are currently unsupported. Ask discord how we feel about these");
                        palace.IsValid = false;
                        return palace;
                    }
                    else
                    {
                        exitType = RoomExitType.DEADEND_EXIT_UP;
                    }
                }
                else if (down is { HasUpExit: true })
                {
                    exitType = RoomExitType.DEADEND_EXIT_DOWN;
                }
                else
                {
                    throw new ImpossibleException("Open coordinate has no adjacent exits");
                }
                roomsByExitType.TryGetValue(exitType, out var possibleStubs);

                bool placed = false;
                do //while (placed == false)
                {
                    Room? newRoom = possibleStubs?.Sample(r);
                    if (newRoom == null)
                    {
                        roomPool.DefaultStubsByDirection.TryGetValue(exitType, out newRoom);
                    }
                    //This should no longer be possible since default stubs aren't removable
                    if (newRoom == null)
                    {
                        palace.IsValid = false;
                        return palace;
                    }

                    newRoom = new(newRoom);
                    //If the stub is a drop zone, pretend it isn't, otherwise junctions can appear
                    //as a result of adding the stub.
                    if (newRoom.IsDropZone)
                    {
                        newRoom.IsDropZone = false;
                    }
                    newRoom.coords = openCoord;
                    allRooms.Add(newRoom.coords, newRoom);
                    openCoords.Remove(openCoord);
                    placed = true;

                    if (left != null && newRoom.HasLeftExit)
                    {
                        newRoom.Left = left;
                        left.Right = newRoom;
                    }
                    if (down != null && newRoom.HasDownExit)
                    {
                        newRoom.Down = down;
                        down.Up = newRoom;
                    }
                    if (up != null && newRoom.HasUpExit)
                    {
                        newRoom.Up = up;
                        up.Down = newRoom;
                    }
                    if (right != null && newRoom.HasRightExit)
                    {
                        newRoom.Right = right;
                        right.Left = newRoom;
                    }

                    if (newRoom.Group != RoomGroup.STUBS)
                    {
                        if (duplicateProtection) { RemoveDuplicatesFromPool(props, roomsByExitType[exitType], newRoom); }
                    }
                } while (placed == false);
            }
        }

        if (allRooms.Count > roomCount)
        {
            throw new ImpossibleException("Palace Room count exceeds maximum room count.");
        }
        if(openCoords.Count != 0)
        {
            throw new ImpossibleException("Stray open coordinate after palace is generated");
        }

        //Recategorize the remaining rooms after stubbing out.
        roomsByExitType = roomPool.CategorizeNormalRoomExits();
        
        // AddSpecialRoomsByReplacement below relies on palace.AllRooms, so fill it in now
        palace.AllRooms.AddRange(allRooms.Values);

        if (palace.HasDeadEnd())
        {
            palace.IsValid = false;
            return palace;
        }

        if (!AddSpecialRoomsByReplacement(palace, roomPool, r, props))
        {
            specialRoomFailureCounts[palaceNumber - 1]++;
            palace.IsValid = false;
            return palace;
        }

        if (allRooms.Count - allRooms.Count(i => i.Value.LinkedRoomName != null && i.Value.Enabled) != roomCount)
        {
            throw new Exception("Generated palace has the incorrect number of rooms");
        }

        palace.AllRooms.ForEach(i => i.PalaceNumber = palaceNumber);
        palace.IsValid = true;
        return palace;
    }

    private void UpdateRoom(Coord bestFit)
    {
        newRoom!.coords = bestFit;
        
        HashSet<Coord> newOpenCoords = newRoom.GetOpenExitCoords();
        foreach (Coord coord in newOpenCoords)
        {
            if (openCoords.Contains(coord) || allRooms.Keys.Any(c => c == coord))
            {
                newOpenCoords.Remove(coord);
            }
        }
        //If adding this room would cause the number of open coordinates to be too large,
        //keep searching for a room that more precisely fits the available space.
        if (newOpenCoords.Count + openCoords.Count + allRooms.Count > roomCount)
        {
            return;
        }
        var (x, y) = bestFit;
        Room left = allRooms.GetValueOrDefault(new Coord(x - 1, y))!;
        Room down = allRooms.GetValueOrDefault(new Coord(x, y - 1))!;
        Room up = allRooms.GetValueOrDefault(new Coord(x, y + 1))!;
        Room right = allRooms.GetValueOrDefault(new Coord(x + 1, y))!;
        if(newRoom.FitsWithLeft(left) > 0)
        {
            newRoom.Left = left;
            left.Right = newRoom;
        }
        if (newRoom.FitsWithDown(down) > 0)
        {
            newRoom.Down = down;
            if(!newRoom.HasDrop)
            {
                down.Up = newRoom;
            }
        }
        if (newRoom.FitsWithUp(up) > 0)
        {
            if(!up.HasDrop)
            {
                newRoom.Up = up;
            }
            up.Down = newRoom;
        }
        if (newRoom.FitsWithRight(right) > 0)
        {
            newRoom.Right = right;
            right.Left = newRoom;
        }
        openCoords.UnionWith(newOpenCoords);
        openCoords.Remove(bestFit);
        if(newRoom.Name.Contains("Central Complex drop fourway"))
        {
            //debug++;
        }
        allRooms.Add(newRoom.coords, newRoom);
        stallCount = 0;

        //Count the number of open coordinates that are junction coordinates, i.e. they have
        //more than 1 room they need to connect to. We want to fill all of these in before capping paths
        //to prevent the capping logic from getting dumb.
        //I considered just categorizing all the rooms by type and doing logic to determine the appropriate cap,
        //but that logic tree to find what shape the hole is to fill with the appropriate peg got messy.
        openJunctionsCount = 0;
        foreach(var coord in openCoords)
        {
            //debug++;
            (x, y) = coord;
            Room? coordLeft = allRooms.GetValueOrDefault(new Coord(x - 1, y));
            Room? coordRight = allRooms.GetValueOrDefault(new Coord(x + 1, y));
            Room? coordUp = allRooms.GetValueOrDefault(new Coord(x, y + 1));
            Room? coordDown = allRooms.GetValueOrDefault(new Coord(x, y - 1));

            if((coordLeft is { HasRightExit: true } ? 1 : 0)
               + (coordRight is { HasLeftExit: true } ? 1 : 0)
               + (coordUp != null && (coordUp.HasDownExit || coordUp.HasDrop) ? 1 : 0)
               + (coordDown is { HasUpExit: true } ? 1 : 0) >= 2)
            {
                openJunctionsCount++;
            }
        }
        //Debug.WriteLine("Added Room at (" + newRoom.coords.Item1 + ", " + newRoom.coords.Item2 + ")");
    }
}
