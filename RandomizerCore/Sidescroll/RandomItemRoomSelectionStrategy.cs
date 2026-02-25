using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core.Tokens;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class RandomItemRoomSelectionStrategy : ItemRoomSelectionStrategy
{
    private const int MAX_ATTEMPTS = 200;
    public override Room[] SelectItemRooms(Palace palace, RoomPool roomPool, int itemRoomCount, bool avoidDuplicates, Random r)
    {
        int itemRoomNumber = 0, attemptNumber = 0;
        List<Room> itemRoomCandidates = roomPool.ItemRooms.ToList();
        List<Room> itemRooms = [];
        List<Coord> replacedCoords = [];
        itemRoomCandidates.FisherYatesShuffle(r);
        if(itemRoomCandidates.Count == 0)
        {
            throw new Exception($"No item room candidates for palace {palace.Number} in RandomItemRoomSelectionStrategy");
        }

        while(itemRooms.Count < itemRoomCount && attemptNumber++ < MAX_ATTEMPTS)
        {
            Room? itemRoomCandidate = itemRoomCandidates.Sample(r);
            if(itemRoomCandidate == null)
            {
                return [];
            }

            RoomExitType itemRoomExitType = itemRoomCandidate.CategorizeExits();
            
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

                    if(avoidDuplicates)
                    {
                        itemRoomCandidates.Remove(itemRoomCandidate);
                    }
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
