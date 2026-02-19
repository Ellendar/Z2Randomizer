using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.RandomizerCore;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

//There is a split in strategies about how linked rooms are handled. What's right/consistent
//Should this actually just be responsible for adding the item to the palace itself (probably not)
//how do we handle multiple item rooms where placement is dependent (see previous line for followup on that)
internal class ByEntranceDirectionItemRoomSelectionStrategy : ItemRoomSelectionStrategy
{
    private const bool BREAK_EARLY_ON_FIRST_FAILED_DIRECTION = false;
    public override Room[] SelectItemRooms(Palace palace, RoomPool roomPool, int itemRoomCount, Random r)
    {
        List<Room> itemRooms = [];
        List<Direction> remainingDirections = new(DirectionExtensions.ITEM_ROOM_ORIENTATIONS);
        Direction itemRoomDirection = Direction.NONE;
        while (itemRooms.Count < itemRoomCount && remainingDirections.Count > 0 )
        {
            itemRoomDirection = remainingDirections.Sample(r);
            if (!roomPool.ItemRoomsByDirection.TryGetValue(itemRoomDirection, out var value))
            {
                remainingDirections.Remove(itemRoomDirection);
                if(BREAK_EARLY_ON_FIRST_FAILED_DIRECTION)
                {
                    return [];
                }
                continue;
            }

            itemRooms.Add(value.Next(r));
        }

        return itemRooms.ToArray();
    }
}
