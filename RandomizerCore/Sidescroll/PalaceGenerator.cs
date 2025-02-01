using System;
using System.Collections.Generic;

namespace RandomizerCore.Sidescroll;

public abstract class PalaceGenerator
{
    protected const int ROOM_SHUFFLE_ATTEMPT_LIMIT = 100;
    protected const int DROP_PLACEMENT_FAILURE_LIMIT = 100;
    protected const int ROOM_PLACEMENT_FAILURE_LIMIT = 100;
    //protected const int DUPLICATE_PREVENTION_CUTOFF_THRESHOLD = 5000;

    protected static readonly IEqualityComparer<byte[]> byteArrayEqualityComparer = new Util.StandardByteArrayEqualityComparer();

    internal abstract Palace GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber);

    protected bool AllowDuplicatePrevention(RandomizerProperties props, int palaceNumber)
    {
        if (palaceNumber < 7)
        {
            //Short normal palace should always be safe regardless of which palace set is being used
            if (props.ShortenNormalPalaces)
            {
                return true;
            }
            //Vanilla only is too small for full palaces probably (and maybe some vanilla only groups)
            if (!props.AllowV4Rooms && !props.AllowV4_4Rooms)
            {
                return false;
            }
            return true;
        }
        else
        {
            //Short GP is ok with any 2 groups
            if (props.ShortenGP)
            {
                if(props.AllowV4Rooms && props.AllowV4_4Rooms
                    || props.AllowV4_4Rooms && props.AllowVanillaRooms
                    || props.AllowV4Rooms && props.AllowVanillaRooms)
                {
                    return true;
                }
                return false;
            }
            //Right now same logic with long GP, but this is likely wrong
            else
            {
                if (props.AllowV4Rooms && props.AllowV4_4Rooms
                    || props.AllowV4_4Rooms && props.AllowVanillaRooms
                    || props.AllowV4Rooms && props.AllowVanillaRooms)
                {
                    return true;
                }
            }
        }
        return true;
    }

}
