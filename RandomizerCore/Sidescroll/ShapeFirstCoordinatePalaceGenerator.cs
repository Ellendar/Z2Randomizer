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

    internal override async Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber, int attempt)
    {
        bool duplicateProtection = (props.NoDuplicateRooms || props.NoDuplicateRoomsBySideview) && AllowDuplicatePrevention(props, palaceNumber);
        Palace palace = new(palaceNumber);
        RoomPool roomPool = new(rooms);
        var itemRoomSelector = GetItemRoomSelectionStrategy();
        // var palaceGroup = Util.AsPalaceGrouping(palaceNumber);

        Dictionary<Coord, RoomExitType> palaceShape;
        palaceShape = await GetPalaceShape(props, palace, roomPool, r, roomCount);
        if(palaceShape.Count == 0)
        {
            palace.IsValid = false;
            return palace;
        }

        if (palace.BossRoom == null)
        {
            if (!PlaceBossRoomInShape(palace, roomPool, r, props, palaceShape))
            {
                palace.IsValid = false;
                return palace;
            }
        }

        List<Coord> prepopulatedCoordinates = [];
        prepopulatedCoordinates.Add(palace.AllRooms.FirstOrDefault(i => i.IsEntrance)?.coords ?? Coord.Uninitialized);
        prepopulatedCoordinates.Add(palace.AllRooms.FirstOrDefault(i => i.IsBossRoom)?.coords ?? Coord.Uninitialized);

        int itemRoomCount = palace.Number < 7 ? props.PalaceItemRoomCounts[palace.Number - 1] : 0;
        // place item rooms at the shape stage if strategy allows
        if (palace.ItemRooms.Count < itemRoomCount && itemRoomSelector is IItemRoomInShapeSelectionStrategy shapeSelector)
        {
            var itemRoomShapes = GetItemRoomShapes(roomPool, palace);
            Room[] itemRooms = shapeSelector.SelectItemRoomsInShape(roomPool, itemRoomCount, duplicateProtection, r, palaceShape, itemRoomShapes, palace.Entrance!.coords, prepopulatedCoordinates);
            if (itemRooms.Length < itemRoomCount)
            {
                palace.IsValid = false;
                return palace;
            }
            palace.ItemRooms = itemRooms.ToList();
            palace.AllRooms.AddRange(palace.ItemRooms);

            prepopulatedCoordinates.AddRange(palace.ItemRooms.Select(room => room.coords));
        }

        //We aren't currently prepopulating thunderbird, but this should probably have some safety.
        //Too lazy for now
        //prepopulatedCoordinates.Add(palace.AllRooms.First(i => i.IsThunderBirdRoom).coords);

        if (!ValidateShape(palace, palaceShape))
        {
            //Debug.WriteLine("ValidateShape failed:\n" + GetLayoutDebug(shape, false, prepopulatedCoordinates));
            palace.IsValid = false;
            return palace;
        }

        //Add rooms
        bool success = false;
        if (await FillShape(props, palace, rooms, roomPool, r, palaceShape, prepopulatedCoordinates))
        {
            if (await FinalizePalace(props, palace, roomPool, roomCount, r, itemRoomSelector))
            {
                success = true;
            }
        }
        if (!success)
        {
            palace.IsValid = false;
            return palace;
        }

        palace.AllRooms.ForEach(i => i.PalaceNumber = palaceNumber);

        palace.IsValid = true;
        return palace;
    }

    protected virtual async Task<bool> FillShape(RandomizerProperties props, Palace palace, RoomPool rooms, RoomPool roomPool, Random r, Dictionary<Coord, RoomExitType> palaceShape, List<Coord> prepopulatedCoordinates)
    {
        var roomsByExitTypeUnmodified = roomPool.CategorizeNormalRoomExits();

        foreach (KeyValuePair<Coord, RoomExitType> pair in palaceShape.OrderBy(i => i.Key.X).ThenByDescending(i => i.Key.Y))
        {
            Coord roomCoords = pair.Key;
            if (prepopulatedCoordinates.Contains(roomCoords))
            {
                continue;
            }
            RoomExitType roomExitType = pair.Value;

            Coord coordAbove = new(roomCoords.X, roomCoords.Y + 1);
            Room? upRoom = palace.AllRooms.FirstOrDefault(i => i.coords == coordAbove);
            bool dropZone = upRoom != null && upRoom.HasDrop;

            Room? newRoom = PickNonStubRoom(props, palace, rooms, roomPool, r, palaceShape, roomCoords, roomExitType, dropZone);

            if (newRoom == null)
            {
                roomPool.DefaultStubsByDirection.TryGetValue(roomExitType, out newRoom);
                if (newRoom != null && dropZone && !newRoom.IsDropZone)
                {
                    //We need to use a drop zone stub but one does not (and cannot) exist so this graph is doomed.
                    //Debug.WriteLine(GetLayoutDebug(walkGraph, false));
                    return false;
                }
            }

            if (newRoom == null)
            {
                return false;
            }
            else
            {
                newRoom = new(newRoom);
            }

            newRoom.coords = roomCoords;
            if (newRoom.LinkedRoomName == null)
            {
                palace.AllRooms.Add(newRoom);
            }
            else
            {
                Room linkedRoom = new(roomPool.LinkedRooms[newRoom.LinkedRoomName]);
                newRoom.LinkedRoom = linkedRoom;
                linkedRoom.LinkedRoom = newRoom;
                linkedRoom.coords = roomCoords;
                Room mergedRoom = newRoom.Merge(linkedRoom);
                palace.AllRooms.Add(mergedRoom);
            }
        }

        return true;
    }

    protected virtual Room? PickNonStubRoom(RandomizerProperties props, Palace palace, RoomPool rooms, RoomPool roomPool, Random r, Dictionary<Coord, RoomExitType> palaceShape, Coord roomCoords, RoomExitType roomExitType, bool dropZone)
    {
        bool duplicateProtection = (props.NoDuplicateRooms || props.NoDuplicateRoomsBySideview) && AllowDuplicatePrevention(props, palace.Number);
        List<Room> roomCandidates = GetNormalRoomsForExitType(roomPool, roomCoords, roomExitType);

        Room? newRoom = null;

        if (roomCandidates.Count > 0)
        {
            bool refillAllowed = duplicateProtection;
            while (true)
            {
                if (roomCandidates.Count == 0)
                {
                    if (!refillAllowed)
                    {
                        break;
                    }

                    logger.Debug($"Shape-first palace ran out of rooms of exit type: {roomExitType} in palace {palace.Number}. Starting to use duplicate rooms.");
                    roomPool.RefillNormalRoomsForExitType(rooms, roomExitType);
                    roomCandidates = roomPool.GetNormalRoomsForExitType(roomExitType, true);
                    Debug.Assert(roomCandidates.Count() > 0);
                    refillAllowed = false;
                }

                roomCandidates.FisherYatesShuffle(r);

                newRoom = SelectRoomForCoord(palace, rooms, roomPool, palaceShape, roomCoords, dropZone, roomCandidates);
                if (newRoom != null)
                {
                    break;
                }

                roomCandidates.Clear(); // clear after failure to force a refill and try again (once)
            }
            if (newRoom != null && duplicateProtection) { roomPool.RemoveDuplicates(props, newRoom); }
        }

        return newRoom;
    }

    protected virtual List<Room> GetNormalRoomsForExitType(RoomPool roomPool, Coord roomCoords, RoomExitType roomExitType)
    {
        return roomPool.GetNormalRoomsForExitType(roomExitType);
    }

    protected virtual Room? SelectRoomForCoord(Palace palace, RoomPool rooms, RoomPool roomPool, Dictionary<Coord, RoomExitType> palaceShape, Coord roomCoords, bool dropZone, List<Room> shuffledRoomCandidates)
    {
        foreach (Room roomCandidate in shuffledRoomCandidates)
        {
            Debug.Assert(roomCandidate.IsNormalRoom());
            if (dropZone && !roomCandidate.IsDropZone)
            {
                continue;
            }
            if (!string.IsNullOrWhiteSpace(roomCandidate.LinkedRoomName))
            {
                if (!LinkedRoomFitInShape(roomPool, palaceShape, palace.Entrance!.coords, roomCoords, roomCandidate))
                {
                    continue;
                }
            }
            return roomCandidate;
        }

        return null;
    }

    protected virtual async Task<bool> FinalizePalace(RandomizerProperties props, Palace palace, RoomPool roomPool, int roomCount, Random r, ItemRoomSelectionStrategy itemRoomSelector)
    {
        if (palace.AllRooms.Count(i => i.Enabled) > roomCount)
        {
            throw new Exception("Generated palace has the incorrect number of rooms");
        }

        await ConnectRooms(palace);

        ConnectNonEuclideanPaths(palace);

        //Some percentage of the time, dropifying some rooms causes part of the palace to become
        //unreachable because up was the only way to get there.
        if (!palace.AllReachable())
        {
            return false;
        }

        if (!AddSpecialRoomsByReplacement(palace, roomPool, r, props, itemRoomSelector))
        {
            return false;
        }

        if (palace.HasDisallowedDrop(props.BossRoomsExitToPalace[palace.Number - 1], props.PalaceDropStyle, r))
        {
            return false;
        }

        return true;
    }

    private static async Task ConnectRooms(Palace palace)
    {
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
    }

    public static bool LinkedRoomFitInShape(RoomPool roomPool, Dictionary<Coord, RoomExitType> palaceShape, Coord entrance, Coord coord, Room room)
    {
        Debug.Assert(room.LinkedRoomName != null);
        Room linkedRoom = new(roomPool.LinkedRooms[room.LinkedRoomName]);
        Debug.Assert(linkedRoom != null);

        // get unmerged shapes
        var shape1 = room.CategorizeExits();
        var shape2 = linkedRoom.CategorizeExits();

        // exits exclusive to each shape to verify reachability
        RoomExitType shape1Only = (RoomExitType)((int)shape1 & ~(int)shape2);
        RoomExitType shape2Only = (RoomExitType)((int)shape2 & ~(int)shape1);

        bool foundShape1 = false, foundShape2 = false;
        HashSet<Coord> visited = [];
        Queue<(IntVector2 dir, Coord target)> queue = [];

        queue.Enqueue((IntVector2.EAST, entrance));

        while (queue.Count > 0)
        {
            var (travelDirection, currentCoord) = queue.Dequeue();
            if (currentCoord == coord)
            {
                if (shape1Only.Contains(-travelDirection))
                {
                    foundShape1 = true;
                }
                if (shape2Only.Contains(-travelDirection))
                {
                    foundShape2 = true;
                }
                if (foundShape1 && foundShape2)
                {
                    return true;
                }
                // coord *not* added to visited
            }
            else if (!visited.Contains(currentCoord))
            {

                foreach (Coord neighbor in GetNeighborsOutgoing(palaceShape[currentCoord], currentCoord))
                {
                    if (!palaceShape.ContainsKey(neighbor))
                    {
                        return true; // shape is not closed, so just allow the linked room
                    }
                    if (!visited.Contains(neighbor))
                    {
                        var dir = neighbor.ToIntVector2() - currentCoord.ToIntVector2();
                        queue.Enqueue((dir, neighbor));
                    }
                }
                visited.Add(currentCoord);
            }
        }

        //Debug.WriteLine("Linked room failed:\n" + GetLayoutDebug(palaceShape, false, []));

        return false;
    }

    /// used to place a boss room when we are still working with shapes
    /// (instead of later replacing an existing room with a boss room)
    protected bool PlaceBossRoomInShape(Palace palace, RoomPool roomPool, Random r, RandomizerProperties props, Dictionary<Coord, RoomExitType> shape)
    {
        if (roomPool.BossRooms.Count == 0) { throw new Exception("No boss rooms in room pool"); }

        List<Room> bossRoomCandidates = roomPool.BossRooms.ToList();
        bossRoomCandidates.FisherYatesShuffle(r);

        var shapeOrdered = shape.OrderBy(i => i.Key.X).ThenByDescending(i => i.Key.Y).ToList();
        bool palaceContinues = palace.Number < 7 && props.BossRoomsExitToPalace[palace.Number - 1];
        int minDistance = GetBossMinDistance(props, palace.Number);

        foreach (Room bossRoomCandidate in bossRoomCandidates)
        {
            RoomExitType bossRoomExitType = bossRoomCandidate.CategorizeExits();
            if (palaceContinues)
            {
                bossRoomExitType = bossRoomExitType.AddRight();
            }

            var bossCoordCandidates = shapeOrdered.Where(pair => pair.Value == bossRoomExitType).ToList();
            bossCoordCandidates.FisherYatesShuffle(r);

            foreach (var pair in bossCoordCandidates)
            {
                var coord = pair.Key;
                if (coord == Coord.Origin) { continue; }
                var upCoord = coord with { Y = coord.Y + 1 };
                if (shape.TryGetValue(upCoord, out var exit) && exit.ContainsDrop()) { continue; }

                if (minDistance > 0 && !Palace.BossRoomMinDistanceShape(shape, coord, minDistance)) { continue; }

                Room bossRoom = new(bossRoomCandidate);
                bossRoom.coords = coord;
                bossRoom.Enemies = (byte[])roomPool.VanillaBossRoom.Enemies.Clone();
                if (palaceContinues)
                {
                    bossRoom.HasRightExit = true;
                    bossRoom.AdjustContinuingBossRoom();
                }
                palace.AllRooms.Add(bossRoom);
                palace.BossRoom = bossRoom;
                return true;
            }
        }
        return false;
    }

    protected virtual int GetBossMinDistance(RandomizerProperties props, int palaceNumber)
    {
        return palaceNumber == 7 ? props.DarkLinkMinDistance : 0;
    }

    protected virtual IEnumerable<RoomExitType> GetItemRoomShapes(RoomPool roomPool, Palace palace)
    {
        return roomPool.GetItemRoomShapes();
    }

    protected virtual bool ValidateShape(Palace palace, Dictionary<Coord, RoomExitType> palaceShape)
    {
        return true;
    }

    public static string GetLayoutDebug(Dictionary<Coord, RoomExitType> walkGraph, bool includeCoordinateGrid = true, List<Coord>? prepopulatedCoordinates = null)
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
                var coord = new Coord(x, y);
                if (!walkGraph.TryGetValue(coord, out RoomExitType room))
                {
                    sb.Append("   ");
                }
                else
                {
                    sb.Append(room.ContainsLeft() ? '-' : ' ');

                    if (prepopulatedCoordinates?.Contains(coord) ?? false)
                    {
                        sb.Append('P');
                    }
                    else
                    {
                        sb.Append('X');
                    }

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

    /// iterator over all neighboring coords from `coord` according to `exitType`
    public static IEnumerable<Coord> GetNeighborsOutgoing(RoomExitType exitType, Coord coord)
    {
        if (exitType.ContainsLeft()) { yield return coord with { X = coord.X - 1 }; }
        if (exitType.ContainsRight()) { yield return coord with { X = coord.X + 1 }; }
        if (exitType.ContainsUp()) { yield return coord with { Y = coord.Y + 1 }; }
        if (exitType.ContainsDown()) { yield return coord with { Y = coord.Y - 1 }; }
    }

    /// iterator over all neighbors, with drop rooms being included both ways
    public static IEnumerable<Coord> GetNeighborsAnyDirection(Dictionary<Coord, RoomExitType> shape, RoomExitType exitType, Coord coord)
    {
        if (exitType.ContainsLeft()) { yield return coord with { X = coord.X - 1 }; }
        if (exitType.ContainsRight()) { yield return coord with { X = coord.X + 1 }; }

        var upCoord = coord with { Y = coord.Y + 1 };
        if (exitType.ContainsUp())
        {
            yield return upCoord;
        }
        else if (shape.TryGetValue(upCoord, out var upShape) && upShape.ContainsDown())
        {
            yield return upCoord;
        }
        if (exitType.ContainsDown()) { yield return coord with { Y = coord.Y - 1 }; }
    }

    private static readonly Regex BlankLine = new(@"^[ \t\f\r\n]+$");
}
