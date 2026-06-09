using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class ReconstructedLoopyPalaceGenerator(CancellationToken ct) : ReconstructedPalaceGenerator(ct)
{
    internal override Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber, int attempt)
    {
        rooms.RemoveRooms(room => room.HasDrop);
        rooms.RemoveRooms(room => !room.IsEntrance && !room.IsBossRoom && !room.HasItem
                                  && RoomExitTypeExtensions.DEADENDS.Contains(room.CategorizeExits()));
        return base.GeneratePalace(props, rooms, r, roomCount, palaceNumber, attempt);
    }

    public override void Consolidate(List<Room> openRooms, RandomizerProperties props, int palaceNumber)
    {
        Room[] openCopy = new Room[openRooms.Count];
        openRooms.CopyTo(openCopy); // shallow copy 
        foreach (Room r2 in openCopy)
        {
            var furthestFirst = openRooms.OrderBy(room => -Palace.RoomDistance(r2, room));
            foreach (Room r3 in furthestFirst)
            {
                if (r2 != r3 && openRooms.Contains(r2) && openRooms.Contains(r3))
                {
                    if (AttachToOpen(r2, r3, openRooms))
                    {
                        break;
                    }
                }
            }
        }
    }

    public override bool AllReachable(Palace palace)
    {
        return palace.AllReachable(allowBossEnterLeft: palace.Number == 7);
    }
}
