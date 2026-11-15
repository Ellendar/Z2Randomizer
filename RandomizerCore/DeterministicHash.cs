using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore;

/// <summary>
/// A content hash that is stable across processes and runs. Implement this
/// (instead of relying on <see cref="object.GetHashCode"/>) for any type that
/// feeds a persisted or displayed hash; <see cref="DeterministicHash.Combine{T}(int, IEnumerable{T}?)"/>
/// requires it so a type without a real implementation cannot slip in.
/// <para>
/// The hash must depend only on content, never on process state, addresses, or
/// <see cref="System.HashCode"/> / <c>string.GetHashCode()</c>.
/// </para>
/// </summary>
public interface IDeterministicHash
{
    int GetDeterministicHash();
}

/// <summary>
/// Deterministic hash mixing for values that must be consistent between runs.
/// <para>
/// <see cref="System.HashCode"/>, <c>string.GetHashCode()</c> and the identity
/// hash of a collection are all seeded or address-derived per process, so they
/// must not feed these hashes.
/// </para>
/// <para>
/// Enumeration order is part of the input: the same items in a different order
/// hash differently. Null collections hash differently from empty ones.
/// </para>
/// </summary>
public static class DeterministicHash
{
    public const int Seed = 17;
    public const int Multiplier = 31;

    public static int Combine(int hash, int value)
    {
        unchecked
        {
            return hash * Multiplier + value;
        }
    }

    public static int Combine(int hash, bool value)
    {
        return Combine(hash, value ? 1 : 0);
    }

    public static int Combine(int hash, int? value)
    {
        // Fold presence separately so null cannot collide with any real value.
        hash = Combine(hash, value.HasValue ? 1 : 0);
        return value.HasValue ? Combine(hash, value.GetValueOrDefault()) : hash;
    }

    public static int Combine(int hash, string? value)
    {
        // Not value.GetHashCode(): .NET randomizes string hashes per process.
        if (value is null)
        {
            return Combine(hash, -1);
        }

        unchecked
        {
            hash = Combine(hash, value.Length);
            foreach (var c in value)
            {
                hash = Combine(hash, c);
            }
        }
        return hash;
    }

    public static int Combine(int hash, IEnumerable<byte>? values)
    {
        return Combine(hash, values, value => value);
    }

    public static int Combine(int hash, IEnumerable<int>? values)
    {
        return Combine(hash, values, value => value);
    }

    public static int Combine(int hash, IEnumerable<string?>? values)
    {
        return Combine(hash, values, value => Combine(0, value));
    }

    public static int Combine(int hash, IDeterministicHash? value)
    {
        return Combine(hash, value is null ? 0 : value.GetDeterministicHash());
    }

    public static int Combine<T>(int hash, IEnumerable<T>? values) where T : IDeterministicHash
    {
        return Combine(hash, values, value => value is null ? 0 : value.GetDeterministicHash());
    }

    public static int Combine<T>(int hash, IEnumerable<T>? values, Func<T, int> selector)
    {
        if (values is null)
        {
            return Combine(hash, -1);
        }

        foreach (var value in values)
        {
            hash = Combine(hash, selector(value));
        }
        return hash;
    }
}
