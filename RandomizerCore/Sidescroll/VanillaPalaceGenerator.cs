using System;
using System.Linq;
using System.Threading;
using NLog;

namespace RandomizerCore.Sidescroll;

public class VanillaPalaceGenerator(CancellationToken ct) : PalaceGenerator
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    internal override Palace GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        VanillaRoomPool roomPool = (VanillaRoomPool)rooms;
        if(roomPool.BossRooms.Count != 1
            || (palaceNumber == 7 && roomPool.TbirdRooms.Count != 1))
        {
            throw new Exception("Invalid vanilla palace room pool");
        }
        Palace palace = new(palaceNumber);

        int palaceGroup = palaceNumber switch
        {
            1 => 1,
            2 => 1,
            3 => 2,
            4 => 2,
            5 => 1,
            6 => 2,
            7 => 3,
            _ => throw new ImpossibleException("Invalid palace number: " + palaceNumber)
        };

        palace.Entrance = new(roomPool.Entrances.First());
        palace.Entrance.PalaceGroup = palaceGroup;
        palace.BossRoom = new(roomPool.BossRooms.First());
        palace.BossRoom.PalaceGroup = palaceGroup;
        palace.AllRooms.Add(palace.Entrance);
        if (palaceNumber != 7)
        {
            Room itemRoom = new(roomPool.ItemRoom);
            palace.ItemRoom = itemRoom;
            palace.ItemRoom.PalaceGroup = palaceGroup;
            palace.AllRooms.Add(palace.ItemRoom);
        }
        palace.AllRooms.Add(palace.BossRoom);
        if (palaceNumber == 7)
        {
            Room bird = new(roomPool.TbirdRooms.First());
            bird.PalaceGroup = palaceGroup;
            palace.AllRooms.Add(bird);
            palace.TbirdRoom = bird;

        }
        foreach (Room v in roomPool.NormalRooms)
        {
            Room room = new(v);
            room.PalaceGroup = palaceGroup;
            palace.AllRooms.Add(room);

            if (room.LinkedRoomName != null)
            {
                Room linkedRoom = new(roomPool.LinkedRooms[room.LinkedRoomName]);
                linkedRoom.PalaceGroup = palaceGroup;
                linkedRoom.LinkedRoom = room;
                room.LinkedRoom = linkedRoom;
                palace.AllRooms.Add(linkedRoom);
            }
        }
        bool removeTbird = (palaceNumber == 7 && props.RemoveTbird);
        palace.CreateTree(removeTbird);

        if(!palace.AllReachable() || (palaceNumber == 7 && props.RequireTbird && !palace.RequiresThunderbird()) || palace.HasDeadEnd())
        {
            throw new Exception("Vanilla palace (" + palaceNumber + ") was not all reachable. This should be impossible.");
        }

        if(props.ShortenNormalPalaces && palaceGroup < 7 || props.ShortenGP && palaceGroup == 7)
        {
            palace.Shorten(r);
        }

        palace.IsValid = true;
        return palace;
    }
}
