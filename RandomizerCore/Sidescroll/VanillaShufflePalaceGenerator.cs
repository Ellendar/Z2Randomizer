using System;
using System.Threading;

namespace RandomizerCore.Sidescroll;

public class VanillaShufflePalaceGenerator(CancellationToken ct) : VanillaPalaceGenerator(ct)
{
    internal override Palace GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber) 
    {
        Palace palace = base.GeneratePalace(props, rooms, r, roomCount, palaceNumber);

        palace.ResetRooms();
        palace.ShuffleRooms(r);

        int tries = 0;
        while (
            !palace.AllReachable() 
            || (palaceNumber == 7 && props.RequireTbird && !palace.RequiresThunderbird()) 
            || palace.HasDeadEnd()
        )
        {
            palace.ResetRooms();
            palace.ShuffleRooms(r);
            if(++tries > ROOM_SHUFFLE_ATTEMPT_LIMIT)
            {
                palace.IsValid = false;
                return palace;
            }
        }
        return palace;
    }
}
