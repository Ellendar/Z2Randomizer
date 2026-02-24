using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

internal class ByShapeItemRoomSelectionStrategy : ItemRoomSelectionStrategy
{
    private static readonly RoomExitType[] PRIORITY_ROOM_SHAPES = [];// [RoomExitType.DROP_STUB, RoomExitType.DROP_T];
    private const int MAX_ATTEMPTS = 200;

    public override Room[] SelectItemRooms(Palace palace, RoomPool roomPool, int itemRoomCount, bool avoidDuplicates, Random r)
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
                if (itemRoomPlaced)
                {
                    break;
                }
                List<Room> itemRoomReplacementCandidates =
                    palace.AllRooms.Where(i => i.IsNormalRoom() && i.CategorizeExits() == itemRoomExitType && !replacedCoords.Contains(i.coords)).ToList();

                itemRoomReplacementCandidates.FisherYatesShuffle(r);
                foreach (Room itemRoomReplacementRoom in itemRoomReplacementCandidates)
                {
                    Room? upRoom = palace.AllRooms.FirstOrDefault(
                        i => i.coords == itemRoomReplacementRoom.coords with { Y = itemRoomReplacementRoom.coords.Y + 1 });
                    if (itemRoomReplacementRoom != null &&
                        (upRoom == null || !upRoom.HasDownExit || upRoom.HasDrop == itemRoomCandidate.IsDropZone))
                    {
                        Room itemRoom = new(itemRoomCandidate);
                        itemRoom.coords = itemRoomReplacementRoom.coords;
                        itemRooms.Add(itemRoom);
                        if (itemRoomCandidate.LinkedRoomName != null)
                        {
                            Room linkedRoom = roomPool.LinkedRooms[itemRoomCandidate.LinkedRoomName];
                            itemRoom = itemRoom.Merge(linkedRoom);
                        }
                        replacedCoords.Add(itemRoomReplacementRoom.coords);
                        itemRoomNumber++;
                        itemRoomPlaced = true;

                        if (!avoidDuplicates || !originalItemRooms.Contains(itemRoom))
                        {
                            originalItemRooms.Add(itemRoom);
                            itemRooms.Add(itemRoom);
                        }
                        break;
                    }
                }
            }
            if (!itemRoomPlaced)
            {
                possibleItemRoomExitTypes.Remove(itemRoomExitType);
                if(possibleItemRoomExitTypes.Count == 0)
                {
                    break;
                }
            }

        }
        if (itemRoomCount == itemRooms.Count)
        {
            return itemRooms.ToArray();
        }
        return [];
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
