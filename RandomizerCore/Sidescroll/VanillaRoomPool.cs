using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

internal class VanillaRoomPool : RoomPool
{
    public Room? ItemRoom { get; private set; }
    public Dictionary<RoomExitType, List<Room>> ItemRoomsByExitType { get; private set; }
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

        ItemRoomsByExitType = new();
        ItemRoomsByExitType[RoomExitType.DEADEND_EXIT_RIGHT] 
            = [palaceRooms.VanillaItemRoom(1), palaceRooms.VanillaItemRoom(2), palaceRooms.VanillaItemRoom(5)];
        ItemRoomsByExitType[RoomExitType.DEADEND_EXIT_LEFT]
            = [palaceRooms.VanillaItemRoom(3), palaceRooms.VanillaItemRoom(4), palaceRooms.VanillaItemRoom(6)];

        //We also need horizontal stubs for item room replacements in vanilla palaces with 0 item rooms
        DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_RIGHT, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).Where(i => i.HasRightExit).First());
        DefaultStubsByDirection.Add(RoomExitType.DEADEND_EXIT_LEFT, palaceRooms.NormalPalaceRoomsByGroup(RoomGroup.STUBS).Where(i => i.HasLeftExit).First());

        VanillaBossRoom = palaceRooms.VanillaBossRoom(palaceNumber);
    }
}

