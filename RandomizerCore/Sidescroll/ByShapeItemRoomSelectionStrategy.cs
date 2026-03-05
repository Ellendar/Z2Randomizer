using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

internal class ByShapeItemRoomSelectionStrategy : ItemRoomSelectionStrategy
{
    private static readonly RoomExitType[] PRIORITY_ROOM_SHAPES = [];// [RoomExitType.DROP_STUB, RoomExitType.DROP_T];
    private const int MAX_ATTEMPTS = 200;

    public override Room[] SelectItemRooms(Palace palace, RoomPool roomPool, int itemRoomCount, bool avoidDuplicates, Random r)
    {
        if (palace.ItemRooms.Count == itemRoomCount) {
            return palace.ItemRooms.ToArray();
        }
        List<Room> allRooms = palace.AllRooms;
        return SelectItemRooms(allRooms, roomPool, itemRoomCount, avoidDuplicates, r);
    }

    public static Room[] SelectItemRooms(List<Room> allRooms, RoomPool roomPool, int itemRoomCount, bool avoidDuplicates, Random r)
    {
        var shapesInPool = roomPool.GetItemRoomShapes();
        List<RoomExitType> possibleItemRoomExitTypes = ShuffleItemRoomShapes(shapesInPool, r);
        List<Room> itemRooms = [], originalItemRooms = [];
        List<Coord> replacedCoords = [];
        int itemRoomNumber = 0, attemptNumber = 0;

        while (itemRoomNumber < itemRoomCount && attemptNumber++ < MAX_ATTEMPTS)
        {
            RoomExitType itemRoomExitType = possibleItemRoomExitTypes[0];
            List<Room> itemRoomCandidates = roomPool.GetItemRoomsForShape(itemRoomExitType).ToList();
            itemRoomCandidates.FisherYatesShuffle(r);

            bool itemRoomPlaced = false;

            foreach (Room itemRoomCandidate in itemRoomCandidates)
            {
                if (avoidDuplicates && originalItemRooms.Contains(itemRoomCandidate)) { continue; }
                List<Room> itemRoomReplacementCandidates =
                    allRooms.Where(i => i.IsNormalRoom() && i.CategorizeExits() == itemRoomExitType && !replacedCoords.Contains(i.coords)).ToList();
                itemRoomReplacementCandidates.FisherYatesShuffle(r);

                foreach (Room itemRoomReplacementRoom in itemRoomReplacementCandidates)
                {
                    var coord = itemRoomReplacementRoom.coords;
                    Room? upRoom = allRooms.FirstOrDefault(i => i.coords == coord with { Y = coord.Y + 1 });
                    bool mustBeDropZone = upRoom != null && upRoom.HasDownExit && upRoom.HasDrop;
                    if (mustBeDropZone && !itemRoomCandidate.IsDropZone) { continue; }

                    Room itemRoom = CreateItemRoom(itemRoomCandidate, coord, roomPool);
                    replacedCoords.Add(coord);
                    itemRoomNumber++;
                    itemRoomPlaced = true;
                    originalItemRooms.Add(itemRoomCandidate);
                    itemRooms.Add(itemRoom);
                    break;
                }
                if (itemRoomPlaced) { break; }
            }
            if (!itemRoomPlaced)
            {
                possibleItemRoomExitTypes.Remove(itemRoomExitType);
                if (possibleItemRoomExitTypes.Count == 0)
                {
                    break;
                }
            }
            else
            {
                possibleItemRoomExitTypes.FisherYatesShuffle(r);
            }
        }
        if (itemRoomCount == itemRooms.Count)
        {
            return itemRooms.ToArray();
        }
        return [];
    }

    public Room[] SelectItemRoomsInShape(RoomPool roomPool, int itemRoomCount, bool avoidDuplicates, Random r, Dictionary<Coord, RoomExitType> shape, IEnumerable<RoomExitType> itemRoomShapes, List<Coord> preplacedCoords)
    {
        List<RoomExitType> possibleItemRoomExitTypes = ShuffleItemRoomShapes(itemRoomShapes, r);
        List<Room> itemRooms = [], originalItemRooms = [];
        List<Coord> replacedCoords = [.. preplacedCoords];
        int itemRoomNumber = 0, attemptNumber = 0;

        while (itemRoomNumber < itemRoomCount && attemptNumber++ < MAX_ATTEMPTS)
        {
            RoomExitType itemRoomExitType = possibleItemRoomExitTypes[0];
            List<Room> itemRoomCandidates = roomPool.GetItemRoomsForShape(itemRoomExitType).ToList();
            itemRoomCandidates.FisherYatesShuffle(r);

            bool itemRoomPlaced = false;

            foreach (Room itemRoomCandidate in itemRoomCandidates)
            {
                if (avoidDuplicates && originalItemRooms.Contains(itemRoomCandidate)) { continue; }
                var itemRoomCoordCandidates = shape.Where(pair => pair.Value == itemRoomExitType && !replacedCoords.Contains(pair.Key)).ToList();
                itemRoomCoordCandidates.FisherYatesShuffle(r);

                foreach (var pair in itemRoomCoordCandidates)
                {
                    var coord = pair.Key;
                    var upCoord = coord with { Y = coord.Y + 1 };
                    RoomExitType candidate = pair.Value;
                    bool mustBeDropZone = shape.TryGetValue(upCoord, out var exit) && exit.ContainsDrop();
                    if (mustBeDropZone && !itemRoomCandidate.IsDropZone) { continue; }

                    Room itemRoom = CreateItemRoom(itemRoomCandidate, coord, roomPool);
                    replacedCoords.Add(coord);
                    itemRoomNumber++;
                    itemRoomPlaced = true;
                    originalItemRooms.Add(itemRoomCandidate);
                    itemRooms.Add(itemRoom);
                    break;
                }
                if (itemRoomPlaced) { break; }
            }
            if (!itemRoomPlaced)
            {
                possibleItemRoomExitTypes.Remove(itemRoomExitType);
                if (possibleItemRoomExitTypes.Count == 0)
                {
                    break;
                }
            }
            else
            {
                possibleItemRoomExitTypes.FisherYatesShuffle(r);
            }
        }
        if (itemRoomCount == itemRooms.Count)
        {
            return itemRooms.ToArray();
        }
        return [];
    }

    private static Room CreateItemRoom(Room candidate, Coord coord, RoomPool pool)
    {
        Room itemRoom = new(candidate);
        itemRoom.coords = coord;
        if (candidate.LinkedRoomName != null)
        {
            Room linkedRoom = pool.LinkedRooms[candidate.LinkedRoomName];
            itemRoom = itemRoom.Merge(linkedRoom);
        }
        return itemRoom;
    }

    private static List<RoomExitType> ShuffleItemRoomShapes(IEnumerable<RoomExitType> possibleItemRoomExitTypes, Random r)
    {
        List<RoomExitType> priorityShapes = [.. possibleItemRoomExitTypes.Where(i => PRIORITY_ROOM_SHAPES.Contains(i))];
        List<RoomExitType> nonPriorityShapes = [.. possibleItemRoomExitTypes.Where(i => !PRIORITY_ROOM_SHAPES.Contains(i))];
        priorityShapes.FisherYatesShuffle(r);
        nonPriorityShapes.FisherYatesShuffle(r);
        return [.. priorityShapes, .. nonPriorityShapes];
    }
}
