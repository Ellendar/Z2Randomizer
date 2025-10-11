using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore.Sidescroll;

public abstract class PalaceGenerator
{
    protected const int ROOM_SHUFFLE_ATTEMPT_LIMIT = 100;
    protected const int DROP_PLACEMENT_FAILURE_LIMIT = 100;
    protected const int ROOM_PLACEMENT_FAILURE_LIMIT = 100;
    //protected const int DUPLICATE_PREVENTION_CUTOFF_THRESHOLD = 5000;

    protected static readonly IEqualityComparer<byte[]> byteArrayEqualityComparer = new Util.StandardByteArrayEqualityComparer();

    internal abstract Task<Palace> GeneratePalace(RandomizerProperties props, RoomPool rooms, Random r, int roomCount, int palaceNumber);

    protected static bool AllowDuplicatePrevention(RandomizerProperties props, int palaceNumber)
    {
        if (palaceNumber < 7)
        {
            //Short normal palace should always be safe regardless of which palace set is being used
            if (props.ShortenNormalPalaces)
            {
                return true;
            }
            //Vanilla only is too small for full palaces probably (and maybe some vanilla only groups)
            if (!props.AllowV4Rooms && !props.AllowV5_0Rooms)
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
                if(props.AllowV4Rooms && props.AllowV5_0Rooms
                    || props.AllowV5_0Rooms && props.AllowVanillaRooms
                    || props.AllowV4Rooms && props.AllowVanillaRooms)
                {
                    return true;
                }
                return false;
            }
            //Right now same logic with long GP, but this is likely wrong
            else
            {
                if (props.AllowV4Rooms && props.AllowV5_0Rooms
                    || props.AllowV5_0Rooms && props.AllowVanillaRooms
                    || props.AllowV4Rooms && props.AllowVanillaRooms)
                {
                    return true;
                }
            }
        }
        return true;
    }

    protected static void RemoveDuplicatesFromPool(RandomizerProperties props, ICollection<Room> rooms, Room roomThatWasUsed)
    {
        if (props.NoDuplicateRoomsBySideview)
        {
            var sideviewBytes = roomThatWasUsed.SideView;
            if (rooms is List<Room> list)
            {
                list.RemoveAll(r => byteArrayEqualityComparer.Equals(r.SideView, sideviewBytes));
            }
            else if (rooms is HashSet<Room> set)
            {
                set.RemoveWhere(r => byteArrayEqualityComparer.Equals(r.SideView, sideviewBytes));
            }
            else { throw new NotImplementedException(); }
        }
        else if (props.NoDuplicateRooms)
        {
            rooms.Remove(roomThatWasUsed);
        }
    }

    [Conditional("DEBUG")]
    public static void DebugCheckDuplicates(RandomizerProperties props, Palace palace)
    {
        switch(props.PalaceStyles[palace.Number - 1])
        {
            case PalaceStyle.VANILLA:
            case PalaceStyle.SHUFFLED:
            case PalaceStyle.RANDOM_WALK: // not implemented
                return;
        }
        if (!AllowDuplicatePrevention(props, palace.Number)) { return; }
        if (props.NoDuplicateRoomsBySideview)
        {
            HashSet<byte[]> usedRoomVariants = new(byteArrayEqualityComparer);
            for (int i = 0; i < palace.AllRooms.Count; i++)
            {
                var room = palace.AllRooms[i];
                if (room.Group == RoomGroup.STUBS) { continue; }
                if (room.HasItem) { continue; }
                if (room.LinkedRoom != null) { continue; }
                var sideviewBytes = room.SideView;
                Debug.Assert(!usedRoomVariants.Contains(sideviewBytes));
                usedRoomVariants.Add(sideviewBytes);
            }
        }
    }
}
