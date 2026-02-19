using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class RandomItemRoomSelectionStrategy : ItemRoomSelectionStrategy
{
    public override Room[] SelectItemRooms(Palace palace, RoomPool roomPool, int itemRoomCount, Random r)
    {
        int itemRoomNumber = 0;
        List<Room> itemRoomCandidates = roomPool.ItemRooms.ToList();
        List<Room> itemRooms = [];
        List<Coord> replacedCoords = [];
        itemRoomCandidates.FisherYatesShuffle(r);
        if(itemRoomCandidates.Count == 0)
        {
            throw new Exception($"No item room candidates for palace {palace.Number} in RandomItemRoomSelectionStrategy");
        }

        foreach (Room itemRoomCandidate in itemRoomCandidates)
        {
            RoomExitType itemRoomExitType = itemRoomCandidate.CategorizeExits();
            if (itemRoomCount == itemRooms.Count)
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
}
