using System;
using System.Diagnostics;

namespace Z2Randomizer.RandomizerCore;

public readonly struct IntVector2 : IEquatable<IntVector2>
{
    public static readonly IntVector2 ZERO = new(0, 0);
    public static readonly IntVector2 ORIGIN = new(0, 0);
    public static readonly IntVector2 NORTH = new(0, -1);
    public static readonly IntVector2 EAST = new(1, 0);
    public static readonly IntVector2 SOUTH = new(0, 1);
    public static readonly IntVector2 WEST = new(-1, 0);
    public static readonly IntVector2 NORTH_EAST = new(1, -1);
    public static readonly IntVector2 SOUTH_EAST = new(1, 1);
    public static readonly IntVector2 SOUTH_WEST = new(-1, 1);
    public static readonly IntVector2 NORTH_WEST = new(-1, -1);

    public static readonly IntVector2[] CARDINALS = [NORTH, EAST, SOUTH, WEST];
    public static readonly IntVector2[] ORDINALS = [NORTH_EAST, SOUTH_EAST, SOUTH_WEST, NORTH_WEST];
    public static readonly IntVector2[] DIRECTIONS = [NORTH, NORTH_EAST, EAST, SOUTH_EAST, SOUTH, SOUTH_WEST, WEST, NORTH_WEST];

    public readonly int X;
    public readonly int Y;

    public IntVector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(IntVector2 a, IntVector2 b)
        => a.Equals(b);

    public static bool operator !=(IntVector2 a, IntVector2 b)
        => !a.Equals(b);

    public static IntVector2 operator +(IntVector2 a, IntVector2 b)
        => new(a.X + b.X, a.Y + b.Y);

    public static IntVector2 operator -(IntVector2 a, IntVector2 b)
        => new(a.X - b.X, a.Y - b.Y);

    public static IntVector2 operator -(IntVector2 v)
        => new(-v.X, -v.Y);

    public static IntVector2 operator *(int k, IntVector2 v)
        => new(k * v.X, k * v.Y);

    public static IntVector2 operator /(IntVector2 v, int d)
        => new(v.X / d, v.Y / d);

    public static int Dot(IntVector2 a, IntVector2 b)
        => a.X * b.X + a.Y * b.Y;

    public static int Cross(IntVector2 a, IntVector2 b)
        => a.X * b.Y - a.Y * b.X;

    public IntVector2 ComponentMultiply(IntVector2 other)
        => new(X * other.X, Y * other.Y);

    public IntVector2 Normalize()
    {
        Debug.Assert(X == 0 || Y == 0, "Normalized IntVector2 can't be diagonal");
        if (X != 0)
        {
            return X > 0 ? EAST : WEST;
        }
        else if (Y != 0)
        {
            return Y > 0 ? SOUTH : NORTH;
        }
        else
        {
            return ZERO;
        }
    }

    public bool IsZero
        => X == 0 && Y == 0;

    public IntVector2 Abs()
        => new(Math.Abs(X), Math.Abs(Y));

    public int MinComponent()
        => Math.Min(X, Y);

    public int MaxComponent()
        => Math.Max(X, Y);

    public static IntVector2 Min(IntVector2 a, IntVector2 b)
        => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));

    public static IntVector2 Max(IntVector2 a, IntVector2 b)
        => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

    public int LengthSquared
        => X * X + Y * Y;

    public double Length
        => Math.Sqrt(LengthSquared);

    public int ManhattanLength
        => Math.Abs(X) + Math.Abs(Y);

    public int DistanceSquared(IntVector2 other)
        => (this - other).LengthSquared;

    public double Distance(IntVector2 other)
        => Math.Sqrt(DistanceSquared(other));

    public int ManhattanDistance(IntVector2 other)
        => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    public static IntVector2 Random(Random r, int maxX, int maxY)
    {
        int x = r.Next(maxX);
        int y = r.Next(maxY);
        return new(x, y);
    }

    public static IntVector2 Random(Random r, int minX, int maxX, int minY, int maxY)
    {
        int x = r.Next(minX, maxX);
        int y = r.Next(minY, maxY);
        return new(x, y);
    }

    public bool Equals(IntVector2 o)
    {
        return X == o.X && Y == o.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntVector2 o && Equals(o);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"{X},{Y}";
    }
}

public static class IntVector2Ext
{
    public static IntVector2 Perpendicular(this IntVector2 v)
    {
        return new(-v.Y, v.X);
    }

    public static IntVector2 PerpendicularCounterClockwise(this IntVector2 v)
    {
        return new(v.Y, -v.X);
    }

    /// <summary>
    /// Determines whether <paramref name="self"/> lies behind
    /// <paramref name="other"/> along the specified direction.
    /// </summary>
    /// <param name="self">The point being tested.</param>
    /// <param name="other">The reference point to compare against.</param>
    /// <param name="direction">The forward direction from the reference point.</param>
    /// <returns><c>true</c> if <paramref name="self"/> is behind <paramref name="other"/>; otherwise, <c>false</c>.</returns>
    public static bool IsBehind(this IntVector2 self, IntVector2 other, IntVector2 direction)
    {
        // A negative dot product means the direction from 'other' to 'self' faces away from the 'direction' vector.
        return IntVector2.Dot(self - other, direction) < 0;
    }
}
