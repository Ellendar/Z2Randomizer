using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public abstract class ItemRoomSelectionStrategy
{
    public abstract Room[] SelectItemRooms(Palace palace, RoomPool roomPool, int itemRoomCount, bool avoidDuplicates, Random r);
}

public interface IItemRoomInShapeSelectionStrategy
{
    Room[] SelectItemRoomsInShape(RoomPool roomPool, int itemRoomCount, bool duplicateProtection, Random r, Dictionary<Coord, RoomExitType> shape, IEnumerable<RoomExitType> itemRoomShapes, List<Coord> prepopulatedCoordinates);
}
