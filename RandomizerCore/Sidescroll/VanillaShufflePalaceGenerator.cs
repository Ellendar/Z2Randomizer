using System;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public class VanillaShufflePalaceGenerator() : VanillaPalaceGenerator()
{
    internal override async Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber) 
    {
        Palace palace = await base.GeneratePalace(props, rooms, r, roomCount, palaceNumber);

        palace.ResetRooms();
        palace.ShuffleRooms(r);

        int tries = 0;
        while (
            !palace.AllReachable() 
            || (palaceNumber == 7 && props.RequireTbird && !palace.RequiresThunderbird())
            || (palaceNumber == 7 && !palace.BossRoomMinDistance(props.DarkLinkMinDistance))
            || palace.HasDisallowedDrop(props.BossRoomsExitToPalace[palace.Number - 1], props.PalaceDropStyle, r)
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
