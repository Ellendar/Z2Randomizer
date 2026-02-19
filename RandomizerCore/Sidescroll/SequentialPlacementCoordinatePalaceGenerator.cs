using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class SequentialPlacementCoordinatePalaceGenerator : CoordinatePalaceGenerator
{
    public static int[] dropFailureCounts = [0, 0, 0, 0, 0, 0, 0];
    public static int[] stallFailureCounts = [0, 0, 0, 0, 0, 0, 0];
    public static int[] specialRoomFailureCounts = [0, 0, 0, 0, 0, 0, 0];

    private static readonly ItemRoomSelectionStrategy itemRoomSelectionStrategy = new RandomItemRoomSelectionStrategy();

    private const int STALL_LIMIT = 5000;
    static int debug = 0;
    
    private HashSet<Coord> openCoords = new();
    private Dictionary<Coord, Room> roomsByCoordinate = new();
    private int stallCount;
    private int openJunctionsCount;
    private int roomCount;
    private int palaceNumber;
    private RoomPool? roomPool;
    
    internal override async Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNum)
    {
        this.roomCount = roomCount;
        this.palaceNumber = palaceNum;

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
        
        roomsByCoordinate= new() { { Coord.Uninitialized, entrance } };
        palace.Entrance = entrance;
        palace.AllRooms.Add(entrance);

        stallCount = 0;
        openJunctionsCount = 0;
        while (roomsByCoordinate.Count + openCoords.Count < this.roomCount || openJunctionsCount > 0)
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
            Room? baseRoom = roomPool.NormalRooms.Sample(r);
            List<Room> newRooms = [];
            if (baseRoom == null)
            {
                palace.IsValid = false;
                return palace;
            }

            baseRoom = new(baseRoom);
            newRooms.Add(baseRoom);
            Room mergedRoom = baseRoom;
            if (baseRoom.LinkedRoomName != null)
            {
                Room linkedRoom = new(rooms.LinkedRooms[baseRoom.LinkedRoomName]);
                newRooms.Add(linkedRoom);
                mergedRoom = baseRoom.Merge(linkedRoom);
            }

            var openCoordsIterator = openCoords.ToArray();
            r.Shuffle(openCoordsIterator);
            Coord bestFitCoordinate = Coord.Uninitialized;
            int bestFitExitCount = 0;
            foreach(Coord openCoord in openCoordsIterator) 
            {
                var (x, y) = openCoord;
                int fitExitCount = mergedRoom.FitsWithLeft(roomsByCoordinate.GetValueOrDefault(new Coord(x - 1, y)))
                    + mergedRoom.FitsWithDown(roomsByCoordinate.GetValueOrDefault(new Coord(x, y - 1)))
                    + mergedRoom.FitsWithUp(roomsByCoordinate.GetValueOrDefault(new Coord(x, y + 1)))
                    + mergedRoom.FitsWithRight(roomsByCoordinate.GetValueOrDefault(new Coord(x + 1, y)));

                if(fitExitCount > bestFitExitCount)
                {
                    bestFitExitCount = fitExitCount;
                    bestFitCoordinate = openCoord;
                }
            }

            if (bestFitExitCount <= 0 || bestFitCoordinate == Coord.Uninitialized)
            {
                continue;
            }
            mergedRoom.coords = bestFitCoordinate;
            UpdateRoom(newRooms, mergedRoom);
            palace.AllRooms.AddRange(newRooms);
            //Debug.WriteLine(palace.GetLayoutDebug(PalaceStyle.SEQUENTIAL, false));
            if (duplicateProtection)
            {
                roomPool.RemoveDuplicates(props, baseRoom);
            }
        }
        //Debug.WriteLine(palace.GetLayoutDebug(PalaceStyle.SEQUENTIAL, false));

        //close stubs
        if (openCoords.Count > 0)
        {
            roomsByExitType = roomPool.CategorizeNormalRoomExits();

            var openCoordsIter = openCoords.ToArray();
            foreach (var openCoord in openCoordsIter)
            {
                var (x, y) = openCoord;
                Room? left = roomsByCoordinate.GetValueOrDefault(new Coord(x - 1, y));
                Room? right = roomsByCoordinate.GetValueOrDefault(new Coord(x + 1, y));
                Room? up = roomsByCoordinate.GetValueOrDefault(new Coord(x, y + 1));
                Room? down = roomsByCoordinate.GetValueOrDefault(new Coord(x, y - 1));
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
                    roomsByCoordinate.Add(newRoom.coords, newRoom);
                    palace.AllRooms.Add(newRoom);
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
                        if (duplicateProtection) { roomPool.RemoveDuplicates(props, newRoom); }
                    }
                } while (placed == false);
            }
        }
        //Debug.WriteLine(palace.GetLayoutDebug(PalaceStyle.SEQUENTIAL, false));
        if (roomsByCoordinate.Count > this.roomCount)
        {
            throw new ImpossibleException("Palace Room count exceeds maximum room count.");
        }
        if(openCoords.Count != 0)
        {
            throw new ImpossibleException("Stray open coordinate after palace is generated");
        }

        //Recategorize the remaining rooms after stubbing out.
        roomsByExitType = roomPool.CategorizeNormalRoomExits();

        if (palace.HasDisallowedDrop(props.BossRoomsExitToPalace[palace.Number - 1], props.PalaceDropStyle, r))
        {
            palace.IsValid = false;
            return palace;
        }

        if (!AddSpecialRoomsByReplacement(palace, roomPool, r, props, new RandomItemRoomSelectionStrategy()))
        {
            specialRoomFailureCounts[palaceNumber - 1]++;
            palace.IsValid = false;
            return palace;
        }

        if (roomsByCoordinate.Count - roomsByCoordinate.Count(i => i.Value.LinkedRoomName != null && !i.Value.Enabled) != this.roomCount)
        {
            throw new Exception("Generated palace has the incorrect number of rooms");
        }

        palace.AllRooms.ForEach(i => i.PalaceNumber = palaceNumber);
        palace.IsValid = true;
        return palace;
    }

    private void UpdateRoom(List<Room> newRooms, Room mergedRoom)
    {
        Coord bestFit = mergedRoom.coords;
        
        HashSet<Coord> newOpenCoords = mergedRoom.GetOpenExitCoords();
        foreach (Coord coord in newOpenCoords)
        {
            if (openCoords.Contains(coord) || roomsByCoordinate.Keys.Any(c => c == coord))
            {
                newOpenCoords.Remove(coord);
            }
        }
        //If adding this room would cause the number of open coordinates to be too large,
        //keep searching for a room that more precisely fits the available space.
        if (newOpenCoords.Count + openCoords.Count + roomsByCoordinate.Count > roomCount)
        {
            return;
        }
        var (x, y) = bestFit;
        Room left = roomsByCoordinate.GetValueOrDefault(new Coord(x - 1, y))!;
        Room down = roomsByCoordinate.GetValueOrDefault(new Coord(x, y - 1))!;
        Room up = roomsByCoordinate.GetValueOrDefault(new Coord(x, y + 1))!;
        Room right = roomsByCoordinate.GetValueOrDefault(new Coord(x + 1, y))!;
        foreach(Room newRoom in newRooms)
        {
            newRoom.coords = mergedRoom.coords;
            if (newRoom.FitsWithLeft(left) > 0)
            {
                newRoom.Left = left;
                left.Right = newRoom;
            }
            if (newRoom.FitsWithDown(down) > 0)
            {
                newRoom.Down = down;
                if (!newRoom.HasDrop)
                {
                    down.Up = newRoom;
                }
            }
            if (newRoom.FitsWithUp(up) > 0)
            {
                if (!up.HasDrop)
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
        }
       
        openCoords.UnionWith(newOpenCoords);
        openCoords.Remove(bestFit);

        roomsByCoordinate.Add(mergedRoom.coords, mergedRoom);
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
            Room? coordLeft = roomsByCoordinate.GetValueOrDefault(new Coord(x - 1, y));
            Room? coordRight = roomsByCoordinate.GetValueOrDefault(new Coord(x + 1, y));
            Room? coordUp = roomsByCoordinate.GetValueOrDefault(new Coord(x, y + 1));
            Room? coordDown = roomsByCoordinate.GetValueOrDefault(new Coord(x, y - 1));

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

    protected override ItemRoomSelectionStrategy GetItemRoomSelectionStrategy()
    {
        return itemRoomSelectionStrategy;
    }
}
