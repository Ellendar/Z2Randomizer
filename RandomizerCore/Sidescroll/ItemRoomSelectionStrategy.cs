using System;
using System.Collections.Generic;
using System.Text;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public abstract class ItemRoomSelectionStrategy
{
    public abstract Room[] SelectItemRooms(Palace palace, RoomPool roomPool, int itemRoomCount, bool avoidDuplicates, Random r);
}
