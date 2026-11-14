using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Z2Randomizer.RandomizerCore.Sidescroll.Palace;

/// <summary>
/// Orders <see cref="Room"/> instances by object reference, which is a stable total
/// ordering for the lifetime of the rooms within a single process run.
/// </summary>
public sealed class RoomReferenceComparer : IComparer<Room>
{
    public int Compare(Room? x, Room? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }
        if (x is null)
        {
            return -1;
        }
        if (y is null)
        {
            return 1;
        }
        return RuntimeHelpers.GetHashCode(x).CompareTo(RuntimeHelpers.GetHashCode(y));
    }
}

/// <summary>
/// Provides equality comparison for a <see cref="ValueTuple{T, Room}"/> where
/// the first element is compared using <see cref="EqualityComparer{T}.Default"/>,
/// while the <see cref="Room"/> element is compared by object reference.
/// </summary>
public sealed class RoomTupleComparer<T> : IEqualityComparer<(T Value, Room Room)>
{
    public bool Equals((T Value, Room Room) a, (T Value, Room Room) b)
    {
        return EqualityComparer<T>.Default.Equals(a.Value, b.Value)
            && ReferenceEquals(a.Room, b.Room);
    }

    public int GetHashCode((T Value, Room Room) obj)
    {
        return HashCode.Combine(
            EqualityComparer<T>.Default.GetHashCode(obj.Value!),
            RuntimeHelpers.GetHashCode(obj.Room));
    }
}
