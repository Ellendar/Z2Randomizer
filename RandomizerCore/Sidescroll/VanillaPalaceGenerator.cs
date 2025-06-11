using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class VanillaPalaceGenerator() : PalaceGenerator
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    internal override async Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber)
    {
        VanillaRoomPool roomPool = (VanillaRoomPool)rooms;
        if(roomPool.BossRooms.Count != 1
            || (palaceNumber == 7 && roomPool.TbirdRooms.Count != 1))
        {
            throw new Exception("Invalid vanilla palace room pool");
        }
        Palace palace = new(palaceNumber);

        // var palaceGroup = Util.AsPalaceGrouping(palaceNumber);

        palace.Entrance = new(roomPool.Entrances.First());
        // palace.Entrance.PalaceGroup = palaceGroup;
        palace.BossRoom = new(roomPool.BossRooms.First());
        // palace.BossRoom.PalaceGroup = palaceGroup;
        palace.AllRooms.Add(palace.Entrance);
        palace.ItemRooms = [];

        if (palaceNumber != 7)
        {
            Room itemRoom = new(roomPool.ItemRoom!);
            palace.ItemRooms.Add(itemRoom);
            palace.AllRooms.Add(itemRoom);
        }

        palace.AllRooms.Add(palace.BossRoom);
        if (palaceNumber == 7)
        {
            Room bird = new(roomPool.TbirdRooms.First());
            // bird.PalaceGroup = palaceGroup;
            palace.AllRooms.Add(bird);
            palace.TbirdRoom = bird;

        }
        foreach (Room v in roomPool.NormalRooms)
        {
            await Task.Yield();
            Room room = new(v);
            // room.PalaceGroup = palaceGroup;
            palace.AllRooms.Add(room);

            if (room.LinkedRoomName != null)
            {
                Room linkedRoom = new(roomPool.LinkedRooms[room.LinkedRoomName]);
                // linkedRoom.PalaceGroup = palaceGroup;
                linkedRoom.LinkedRoom = room;
                room.LinkedRoom = linkedRoom;
                palace.AllRooms.Add(linkedRoom);
            }
        }
        bool removeTbird = (palaceNumber == 7 && props.RemoveTbird);
        palace.CreateTree(removeTbird);

        if(palaceNumber != 7 && props.PalaceItemRoomCount == 0)
        {
            //Replace item room with an appropriately shaped stub
            RoomExitType itemRoomExitType = roomPool.ItemRoom!.CategorizeExits();
            Room itemRoomStub = new(roomPool.DefaultStubsByDirection[itemRoomExitType]);
            palace.ReplaceRoom(palace.ItemRooms[0], itemRoomStub);
            palace.ItemRooms.Clear();
        }

        if(palaceNumber != 7 && props.PalaceItemRoomCount > 1)
        {
            //Find all left/right dead ends that aren't special
            List<Room> normalDeadEnds = palace.AllRooms.Where(i =>
                !i.IsBossRoom
                && !i.IsEntrance
                && !i.HasItem
                //Replacing a linked room removes half of it which theoretically could work but currently breaks stuff
                && i.LinkedRoomName == null
                && (i.CategorizeExits() == RoomExitType.DEADEND_EXIT_LEFT || i.CategorizeExits() == RoomExitType.DEADEND_EXIT_RIGHT)).ToList();
            //pick N-1 of them 
            IEnumerable<Room> roomsToItemRoomify = normalDeadEnds.Sample(r, props.PalaceItemRoomCount - 1);
            //replace them with randomly selected vanilla item rooms of the same shape
            foreach(Room room in roomsToItemRoomify)
            {
                Room itemRoom = new(roomPool.ItemRoomsByExitType[room.CategorizeExits()].Sample(r)
                    ?? throw new Exception("There are no available item rooms for vanilla item room replacement"));
                palace.ReplaceRoom(room, itemRoom);
                palace.ItemRooms.Add(itemRoom);
            }
        }

        if(!palace.AllReachable() || (palaceNumber == 7 && props.RequireTbird && !palace.RequiresThunderbird()) || palace.HasDeadEnd())
        {
            throw new Exception("Vanilla palace (" + palaceNumber + ") was not all reachable. This should be impossible.");
        }

        if(props.ShortenNormalPalaces && palaceNumber < 7 || props.ShortenGP && palaceNumber == 7)
        {
            palace.Shorten(r);
        }

        palace.IsValid = true;
        return palace;
    }

    protected new bool AllowDuplicatePrevention(RandomizerProperties props, int palaceNumber)
    {
        return false;
    }

}
