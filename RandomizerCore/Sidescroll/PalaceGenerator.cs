using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            if (props.PalaceLengths[palaceNumber - 1] < 22) // this may need to be adjusted
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
            if (props.PalaceLengths[6] < 35) // this may need to be adjusted
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

    /// <summary>
    /// If full duplicate protection by sideview is enabled, this will return a lookup map of DuplicateGroupId -> collection of rooms.
    /// Otherwise it will return null. The lookup being null can safely be passed to DetermineRoomVariants and nothing will be done.
    /// </summary>
    protected static ILookup<string, Room>? CreateRoomVariantsLookupOrNull(RandomizerProperties props, int palaceNumber, RoomPool roomPool)
    {
        if (props.NoDuplicateRoomsBySideview && AllowDuplicatePrevention(props, palaceNumber))
        {
            return roomPool.NormalRooms.Where(r => r.DuplicateGroup != null && r.DuplicateGroup != "").ToLookup(r => r.DuplicateGroup);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// With full duplicate protection enabled (meaning duplicateRoomLookup is non-null), 
    /// this will remove all but one rooms of each duplicate group, at random.
    /// </summary>
    protected static void DetermineRoomVariants(Random r, ILookup<string, Room>? duplicateRoomLookup, List<Room> rooms)
    {
        if (duplicateRoomLookup == null) { return; }
        HashSet<Room> toRemove = new();
        foreach (IGrouping<string, Room> group in duplicateRoomLookup)
        {
            // randomly pick one room from the duplicate group to keep
            int keepIndex = r.Next(group.Count());
            Room keep = group.ElementAt(keepIndex);
            foreach (var room in group)
            {
                if (!ReferenceEquals(room, keep)) { toRemove.Add(room); }
            }
        }
        rooms.RemoveAll(toRemove.Contains);
    }

    protected static void RemoveDuplicatesFromPool(ICollection<Room> rooms, Room roomThatWasUsed)
    {
        if (rooms is List<Room> list)
        {
            var removed = list.RemoveAll(r => r.Name == roomThatWasUsed.Name);
            Debug.Assert(removed == 1);
        }
        else if (rooms is HashSet<Room> set)
        {
            var removed = set.RemoveWhere(r => r.Name == roomThatWasUsed.Name);
            Debug.Assert(removed == 1);
        }
        else { throw new NotImplementedException(); }
    }

    [Conditional("DEBUG")]
    public static void DebugCheckDuplicates(RandomizerProperties props, Palace palace)
    {
        switch(props.PalaceStyles[palace.Number - 1])
        {
            case PalaceStyle.VANILLA:
            case PalaceStyle.SHUFFLED:
            case PalaceStyle.RANDOM_WALK: // not implemented
            case PalaceStyle.VANILLA_WEIGHTED: // based on Random Walk
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
