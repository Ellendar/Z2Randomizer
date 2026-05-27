// Portions of this file are derived from the .NET runtime implementation of System.Random.
// https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Random.cs
// https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Random.ImplBase.cs
// https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Random.Xoshiro256StarStarImpl.cs
// Copyright (c) .NET Foundation and Contributors.
// Licensed under the MIT License.
//
// Additional algorithms:
// - xoshiro256** (Public Domain) http://prng.di.unimi.it/xoshiro256starstar.c
// - SplitMix64 (Public Domain) https://xorshift.di.unimi.it/splitmix64.c

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Z2Randomizer.RandomizerCore;

public class Random
{
    private ulong _s0, _s1, _s2, _s3;

    public Random(long seed)
    {
        var splitMix = new SplitMix64((ulong)seed);
        do
        {
            _s0 = splitMix.Next();
            _s1 = splitMix.Next();
            _s2 = splitMix.Next();
            _s3 = splitMix.Next();
        } while ((_s0 | _s1 | _s2 | _s3) == 0); // at least one value must be non-zero
    }

    public System.String GetState()
    {
        System.Span<byte> bytes = stackalloc byte[32];
        MemoryMarshal.Write(bytes, in _s0);
        MemoryMarshal.Write(bytes.Slice(8), in _s1);
        MemoryMarshal.Write(bytes.Slice(16), in _s2);
        MemoryMarshal.Write(bytes.Slice(24), in _s3);
        return System.Convert.ToBase64String(bytes);
    }

    public void SetState(System.String state)
    {
        byte[] bytes = System.Convert.FromBase64String(state);

        if (bytes.Length != 32)
        {
            throw new System.ArgumentException("Invalid RNG state.", nameof(state));
        }

        System.ReadOnlySpan<byte> span = bytes;

        _s0 = MemoryMarshal.Read<ulong>(span);
        _s1 = MemoryMarshal.Read<ulong>(span[8..]);
        _s2 = MemoryMarshal.Read<ulong>(span[16..]);
        _s3 = MemoryMarshal.Read<ulong>(span[24..]);

        if ((_s0 | _s1 | _s2 | _s3) == 0)
        {
            throw new System.ArgumentException("Invalid RNG state (all zero).", nameof(state));
        }
    }

    /// <summary>Produces a value in the range [0, ulong.MaxValue].</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ulong NextUInt64()
    {
        ulong s0 = _s0, s1 = _s1, s2 = _s2, s3 = _s3;

        ulong result = BitOperations.RotateLeft(s1 * 5, 7) * 9;

        ulong t = s1 << 17;

        s2 ^= s0;
        s3 ^= s1;
        s1 ^= s2;
        s0 ^= s3;

        s2 ^= t;
        s3 = BitOperations.RotateLeft(s3, 45);

        _s0 = s0;
        _s1 = s1;
        _s2 = s2;
        _s3 = s3;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ulong NextUInt64(ulong maxValue)
    {
        ulong randomProduct = System.Math.BigMul(maxValue, NextUInt64(), out ulong lowPart);

        if (lowPart < maxValue)
        {
            ulong remainder = (0ul - maxValue) % maxValue;

            while (lowPart < remainder)
            {
                randomProduct = System.Math.BigMul(maxValue, NextUInt64(), out lowPart);
            }
        }

        return randomProduct;
    }

    /// <summary>Produces a value in the range [0, uint.MaxValue].</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // small-ish hot path used by very few call sites
    internal uint NextUInt32()
    {
        return (uint)(NextUInt64() >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal uint NextUInt32(uint maxValue)
    {
        ulong randomProduct = (ulong)maxValue * NextUInt32();
        uint lowPart = (uint)randomProduct;

        if (lowPart < maxValue)
        {
            uint remainder = (0u - maxValue) % maxValue;

            while (lowPart < remainder)
            {
                randomProduct = (ulong)maxValue * NextUInt32();
                lowPart = (uint)randomProduct;
            }
        }

        return (uint)(randomProduct >> 32);
    }

    public int Next()
    {
        while (true)
        {
            // Get top 31 bits to get a value in the range [0, int.MaxValue], but try again
            // if the value is actually int.MaxValue, as the method is defined to return a value
            // in the range [0, int.MaxValue).
            ulong result = NextUInt64() >> 33;
            if (result != int.MaxValue)
            {
                return (int)result;
            }
        }
    }

    public int Next(int maxValue)
    {
        Debug.Assert(maxValue >= 0);

        return (int)NextUInt32((uint)maxValue);
    }

    public int Next(int minValue, int maxValue)
    {
        Debug.Assert(minValue <= maxValue);

        return (int)NextUInt32((uint)(maxValue - minValue)) + minValue;
    }

    public long NextInt64()
    {
        while (true)
        {
            // Get top 63 bits to get a value in the range [0, long.MaxValue], but try again
            // if the value is actually long.MaxValue, as the method is defined to return a value
            // in the range [0, long.MaxValue).
            ulong result = NextUInt64() >> 1;
            if (result != long.MaxValue)
            {
                return (long)result;
            }
        }
    }

    public long NextInt64(long maxValue)
    {
        Debug.Assert(maxValue >= 0);

        return (long)NextUInt64((ulong)maxValue);
    }

    public long NextInt64(long minValue, long maxValue)
    {
        Debug.Assert(minValue <= maxValue);

        return (long)NextUInt64((ulong)(maxValue - minValue)) + minValue;
    }

    public void NextBytes(System.Span<byte> buffer)
    {
        while (buffer.Length >= sizeof(ulong))
        {
            ulong value = NextUInt64();
            MemoryMarshal.Write(buffer, in value);
            buffer = buffer.Slice(sizeof(ulong));
        }

        if (!buffer.IsEmpty)
        {
            ulong value = NextUInt64();
            System.Span<byte> buf = stackalloc byte[sizeof(ulong)];
            MemoryMarshal.Write(buf, in value);
            buf[..buffer.Length].CopyTo(buffer);
        }
    }

    public double NextDouble()
    {
        // As described in http://prng.di.unimi.it/:
        // "A standard double (64-bit) floating-point number in IEEE floating point format has 52 bits of significand,
        //  plus an implicit bit at the left of the significand. Thus, the representation can actually store numbers with
        //  53 significant binary digits. Because of this fact, in C99 a 64-bit unsigned integer x should be converted to
        //  a 64-bit double using the expression
        //  (x >> 11) * 0x1.0p-53"
        return (NextUInt64() >> 11) * (1.0 / (1ul << 53));
    }

    /// <summary>
    ///   Fills the elements of a specified span with items chosen at random from the provided set of choices.
    /// </summary>
    /// <param name="choices">The items to use to populate the span.</param>
    /// <param name="destination">The span to be filled with items.</param>
    /// <typeparam name="T">The type of span.</typeparam>
    /// <exception cref="ArgumentException"><paramref name="choices" /> is empty.</exception>
    /// <remarks>
    ///   The method uses <see cref="Next(int)" /> to select items randomly from <paramref name="choices" />
    ///   by index and populate <paramref name="destination" />.
    /// </remarks>
    public void GetItems<T>(System.ReadOnlySpan<T> choices, System.Span<T> destination)
    {
        if (choices.IsEmpty)
        {
            throw new System.ArgumentException();
        }
        // Simple fallback: get each item individually, generating a new random Int32 for each
        // item. This is slower than the above, but it works for all types and sizes of choices.
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = choices[Next(choices.Length)];
        }
    }

    /// <summary>
    ///   Creates an array populated with items chosen at random from the provided set of choices.
    /// </summary>
    /// <param name="choices">The items to use to populate the array.</param>
    /// <param name="length">The length of array to return.</param>
    /// <typeparam name="T">The type of array.</typeparam>
    /// <returns>An array populated with random items.</returns>
    /// <exception cref="ArgumentException"><paramref name="choices" /> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="length" /> is not zero or a positive number.
    /// </exception>
    /// <remarks>
    ///   The method uses <see cref="Next(int)" /> to select items randomly from <paramref name="choices" />
    ///   by index. This is used to populate a newly-created array.
    /// </remarks>
    public T[] GetItems<T>(System.ReadOnlySpan<T> choices, int length)
    {
        System.ArgumentOutOfRangeException.ThrowIfNegative(length);
        T[] items = new T[length];
        GetItems(choices, new System.Span<T>(items));
        return items;
    }

    /// <summary>
    ///   Performs an in-place shuffle of a span.
    /// </summary>
    /// <param name="values">The span to shuffle.</param>
    /// <typeparam name="T">The type of span.</typeparam>
    /// <remarks>
    ///   This method uses <see cref="Next(int, int)" /> to choose values for shuffling.
    ///   This method is an O(n) operation.
    /// </remarks>
    public void Shuffle<T>(System.Span<T> values)
    {
        for (int i = 0; i < values.Length - 1; i++)
        {
            int j = Next(i, values.Length);

            // The i != j check is excluded intentionally.
            // Microbenchmarks show that the mispredicted branches cost more than the redundant read/write for small value types.
            // Since large value types are uncommon in shuffle scenarios, the trade-off favors removing the branch.
            T temp = values[i];
            values[i] = values[j];
            values[j] = temp;
        }
    }

    private class SplitMix64(ulong seed)
    {
        public ulong Next()
        {
            ulong z = (seed += 0x9e3779b97f4a7c15);
            z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
            z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
            return z ^ (z >> 31);
        }
    }
}
