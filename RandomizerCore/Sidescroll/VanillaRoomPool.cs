using System.Collections.Generic;
using System.Linq;

namespace RandomizerCore.Sidescroll;

internal class VanillaRoomPool : RoomPool
{
    public Room? ItemRoom { get; private set; }
    public VanillaRoomPool(PalaceRooms palaceRooms, int palaceNumber, RandomizerProperties props)
    {
        Entrances.AddRange(palaceRooms.Entrances(RoomGroup.VANILLA)
            .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
        BossRooms.AddRange(palaceRooms.BossRooms(RoomGroup.VANILLA)
            .Where(i => (i.PalaceNumber == null && palaceNumber < 6) || i.PalaceNumber == palaceNumber).ToList());
        TbirdRooms.AddRange(palaceRooms.ThunderBirdRooms(RoomGroup.VANILLA)
            .Where(i => i.PalaceNumber == null || i.PalaceNumber == palaceNumber).ToList());
        foreach (KeyValuePair<string, Room> room in palaceRooms.LinkedRooms(RoomGroup.VANILLA))
        {
            LinkedRooms.Add(room.Key, room.Value);
        }
        if (palaceNumber < 7)
        {
            ItemRoom = palaceRooms.VanillaItemRoom(palaceNumber);
        }
        NormalRooms.AddRange(palaceRooms.VanillaPalaceRoomsByPalaceNumber(palaceNumber));
    }
}

