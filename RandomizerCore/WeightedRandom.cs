using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Z2Randomizer.RandomizerCore;

public interface IWeightedSampler<T> where T : notnull
{
    T Next(Random r);
    IReadOnlyList<T> Keys();
    int Weight(T t);
    /// Note: This returns a new copy of the class. This is pretty slow,
    /// but also needing this should be avoided in the first place.
    IWeightedSampler<T> Subtract(T keyToRemove);
    /// This should not really be necessary since all members should be read-only. Might remove later.
    IWeightedSampler<T> Clone();
}


/// Best option for small weight sums
/// Construction time proportional to total weight sum.
/// Memory usage proportional to total weight sum (bad if weights are large).
/// Sampling is O(1).
public class TableWeightedRandom<T> : IWeightedSampler<T> where T : notnull
{
    /// contains each key value once
    private readonly T[] _keys;
    /// contains each key value n times, where n is the weight of the key
    private readonly T[] _table;
    private readonly int _totalWeight;
    /// the key order for _weights is nondeterministic
    private readonly FrozenDictionary<T, int> _weights;

    public TableWeightedRandom(IReadOnlyList<(T value, int weight)> entries)
    {
        int size;
        if (entries == null || (size = entries.Count()) == 0) { throw new ArgumentException("Entries cannot be null or empty.", nameof(entries)); }

        _keys = entries.Select(entry => entry.value).ToArray();
        _totalWeight = entries.Select(entry => entry.weight).Sum();

        _table = new T[_totalWeight];
        int tableIndex = 0;
        foreach (var (value, weight) in entries)
        {
            for (int i = 0; i < weight; i++)
            {
                _table[tableIndex] = value;
                tableIndex++;
            }
        }

        _weights = entries.ToFrozenDictionary(e => e.value, e => e.weight);
    }

    private TableWeightedRandom(T[] keys, T[] table, int totalWeight, FrozenDictionary<T, int> weights)
    {
        _keys = keys;
        _table = table;
        _totalWeight = totalWeight;
        _weights = weights;
    }

    public IWeightedSampler<T> Clone()
    {
        return new TableWeightedRandom<T>(_keys, _table, _totalWeight, _weights);
    }

    public IWeightedSampler<T> Subtract(T keyToRemove)
    {
        var keys = Keys();
        if (!keys.Contains(keyToRemove))
        {
            return this;
        }

        var newEntries = keys.Where(k => !k.Equals(keyToRemove)).Select(k => (k, Weight(k))).ToList();
        return new TableWeightedRandom<T>(newEntries);
    }

    public T Next([NotNull] Random r)
    {
        int roll = r.Next(0, _totalWeight);
        return _table[roll];
    }

    public IReadOnlyList<T> Keys()
    {
        return _keys;
    }

    public int Weight(T t)
    {
        return _weights[t];
    }
}

/// Good for a modest number of keys (hundreds or less) where total weight is too big for TableWeightedRandom
/// Construction time O(n).
/// Memory usage O(n), independent of weight magnitudes.
/// Sampling is O(n) (linear scan).
public class LinearWeightedRandom<T> : IWeightedSampler<T> where T : notnull
{
    private readonly T[] _keys;
    private readonly int[] _cumulativeWeights;
    private readonly int _totalWeight;
    /// the key order for _weights is nondeterministic
    private readonly FrozenDictionary<T, int> _weights;

    public LinearWeightedRandom(IReadOnlyList<(T value, int weight)> entries)
    {
        int size;
        if (entries == null || (size = entries.Count()) == 0) { throw new ArgumentException("Entries cannot be null or empty.", nameof(entries)); }

        _keys = new T[size];
        _cumulativeWeights = new int[size];

        int i = 0;
        int total = 0;
        foreach (var (value, weight) in entries)
        {
            if (weight < 0) { throw new ArgumentException($"Weight must be positive (entry {i})."); }

            total += weight;
            _keys[i] = value;
            _cumulativeWeights[i] = total;
            i++;
        }

        _totalWeight = total;

        _weights = entries.ToFrozenDictionary(e => e.value, e => e.weight);
    }

    private LinearWeightedRandom(T[] values, int[] cumulativeWeights, int totalWeight, FrozenDictionary<T, int> weights)
    {
        _keys = values;
        _cumulativeWeights = cumulativeWeights;
        _totalWeight = totalWeight;
        _weights = weights;
    }

    public IWeightedSampler<T> Clone()
    {
        return new LinearWeightedRandom<T>(_keys, _cumulativeWeights, _totalWeight, _weights);
    }

    public IWeightedSampler<T> Subtract(T keyToRemove)
    {
        var keys = Keys();
        if (!keys.Contains(keyToRemove))
        {
            return this;
        }

        var newEntries = keys.Where(k => !k.Equals(keyToRemove)).Select(k => (k, Weight(k))).ToList();
        return new LinearWeightedRandom<T>(newEntries);
    }

    public T Next([NotNull] Random r)
    {
        int roll = r.Next(0, _totalWeight);

        for (int i = 0; i < _cumulativeWeights.Length; ++i)
        {
            if (roll < _cumulativeWeights[i])
            {
                return _keys[i];
            }
        }

        throw new ImpossibleException("Failed to select a value.");
    }

    public IReadOnlyList<T> Keys()
    {
        return _keys;
    }

    public int Weight(T t)
    {
        // this would just require keeping another dictionary
        // _weights could be copied
        throw new NotImplementedException();
    }
}

public class WeightedShuffler<T> where T : notnull
{
    /// contains each key value once
    private readonly T[] _keys;
    /// the key order for _weights is nondeterministic
    private readonly FrozenDictionary<T, int> _weights;

    public WeightedShuffler(IReadOnlyList<(T value, int weight)> entries)
    {
        int size;
        if (entries == null || (size = entries.Count()) == 0) { throw new ArgumentException("Entries cannot be null or empty.", nameof(entries)); }

        _keys = entries.Select(entry => entry.value).ToArray();
        _weights = entries.ToFrozenDictionary(e => e.value, e => e.weight);
    }

    /// <summary>
    /// Returns all values shuffled using weights to prioritize sorting
    /// values earlier in the list.
    /// </summary>
    public T[] Shuffle([NotNull] Random r)
    {
        int length = _keys.Length;
        if (length == 0) { return Array.Empty<T>(); }

        // will contain random rolls 0..1 to the exponent 1/weight
        var tuple = new (double key, T value)[length];
        int i = 0;
        foreach (var k in _keys)
        {
            var weight = _weights[k];
            double d = r.NextDouble();
            double exp = Math.Pow(d, 1.0 / weight);
            tuple[i] = (exp, k);
            i++;
        }

        // higher keys first
        Array.Sort(tuple, (a, b) => b.key.CompareTo(a.key));

        var result = new T[length];
        for (i = 0; i < length; i++)
        {
            result[i] = tuple[i].value;
        }
        return result;
    }
}
