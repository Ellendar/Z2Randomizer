using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

internal class ByEntranceDirectionItemRoomSelectionStrategy : ItemRoomSelectionStrategy
{
    private const int MAX_ATTEMPTS = 200;

    private const bool BREAK_EARLY_ON_FIRST_FAILED_DIRECTION = false;
    public override Room[] SelectItemRooms(Palace palace, RoomPool roomPool, int itemRoomCount, bool avoidDuplicates, Random r)
    {
        List<Room> itemRooms = [], originalItemRooms = [];
        int attemptNumber = 0;
        Direction itemRoomDirection;
        List<Direction> remainingDirections = new(DirectionExtensions.ITEM_ROOM_ORIENTATIONS);
        while (itemRooms.Count < itemRoomCount && attemptNumber++ < MAX_ATTEMPTS && remainingDirections.Count > 0)
        {
            itemRoomDirection = remainingDirections.Sample(r);
            if (!roomPool.ItemRoomsByDirection.TryGetValue(itemRoomDirection, out var value))
            {
                remainingDirections.Remove(itemRoomDirection);
                if (BREAK_EARLY_ON_FIRST_FAILED_DIRECTION)
                {
                    return [];
                }
                continue;
            }
            Room itemRoom = value.Next(r);

            if (!avoidDuplicates || !originalItemRooms.Contains(itemRoom))
            {
                originalItemRooms.Add(itemRoom);
                itemRooms.Add(new(itemRoom));
            }

        }
        

        return itemRooms.ToArray();
    }
}
