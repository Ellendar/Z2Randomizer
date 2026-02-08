using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Z2Randomizer.RandomizerCore;

public interface IWeightedSampler<T>
{
    T Next(Random r);
    IEnumerable<T> Keys();
    IWeightedSampler<T> Remove(T t);
    int Weight(T t);
    IWeightedSampler<T> Clone();
}


/// Best option for small weight sums
/// Construction time proportional to total weight sum.
/// Memory usage proportional to total weight sum (bad if weights are large).
/// Sampling is O(1).
public class TableWeightedRandom<T> : IWeightedSampler<T>
    where T : notnull
{
    private readonly T[] _table;
    private readonly int _totalWeight;
    private readonly FrozenDictionary<T, int> _weights;

    public TableWeightedRandom(IEnumerable<(T value, int weight)> entries)
    {
        int size;
        if (entries == null || (size = entries.Count()) == 0) { throw new ArgumentException("Entries cannot be null or empty.", nameof(entries)); }

        _weights = entries.ToFrozenDictionary(e => e.value, e => e.weight);
        _totalWeight = _weights.Values.Sum();
        _table = new T[_totalWeight];
        int index = 0;

        foreach (var (value, weight) in _weights)
        {
            for (int i = 0; i < weight; i++)
            {
                _table[index] = value;
                index++;
            }
        }
    }

    public TableWeightedRandom(IEnumerable<KeyValuePair<T, int>> dict)
    : this(dict.Select(kvp => (kvp.Key, kvp.Value)))
    {
    }

    private TableWeightedRandom(T[] table, int totalWeight, FrozenDictionary<T, int> weights)
    {
        _table = table;
        _totalWeight = totalWeight;
        _weights = weights;
    }

    public IWeightedSampler<T> Clone()
    {
        return new TableWeightedRandom<T>(_table, _totalWeight, _weights);
    }

    public T Next([NotNull] Random r)
    {
        int roll = r.Next(0, _totalWeight);
        return _table[roll];
    }

    public IEnumerable<T> Keys()
    {
        return _weights.Keys;
    }

    /// Note: This returns a new copy of the class. This is pretty slow,
    /// but also needing this should be avoided in the first place.
    public IWeightedSampler<T> Remove(T t)
    {
        if (!_weights.ContainsKey(t))
        {
            return this;
        }

        var newWeights = new Dictionary<T, int>(_weights);
        newWeights.Remove(t);

        return new TableWeightedRandom<T>(
            newWeights.Select(kvp => (kvp.Key, kvp.Value))
        );
    }

    public int Weight(T t)
    {
        return _weights[t];
    }
}

/// Good for modest n (hundreds or less) where total weight is too big for TableWeightedRandom
/// Construction time O(n).
/// Memory usage O(n), independent of weight magnitudes.
/// Sampling is O(n) (linear scan).
public class LinearWeightedRandom<T> : IWeightedSampler<T>
{
    private readonly T[] _values;
    private readonly int[] _cumulativeWeights;
    private readonly int _totalWeight;

    public LinearWeightedRandom(IEnumerable<(T, int)> entries)
    {
        int size;
        if (entries == null || (size = entries.Count()) == 0) { throw new ArgumentException("Entries cannot be null or empty.", nameof(entries)); }

        _values = new T[size];
        _cumulativeWeights = new int[size];

        int i = 0;
        int total = 0;
        foreach (var (value, weight) in entries)
        {
            if (weight < 0) { throw new ArgumentException($"Weight must be positive (entry {i})."); }

            total += weight;
            _values[i] = value;
            _cumulativeWeights[i] = total;
            i++;
        }

        _totalWeight = total;
    }

    public LinearWeightedRandom(IEnumerable<KeyValuePair<T, int>> dict)
    : this(dict.Select(kvp => (kvp.Key, kvp.Value)))
    {
    }

    private LinearWeightedRandom(T[] values, int[] cumulativeWeights, int totalWeight)
    {
        _values = values;
        _cumulativeWeights = cumulativeWeights;
        _totalWeight = totalWeight;
    }

    public IWeightedSampler<T> Clone()
    {
        return new LinearWeightedRandom<T>(_values, _cumulativeWeights, _totalWeight);
    }

    public T Next([NotNull] Random r)
    {
        int roll = r.Next(0, _totalWeight);

        for (int i = 0; i < _cumulativeWeights.Length; ++i)
        {
            if (roll < _cumulativeWeights[i])
            {
                return _values[i];
            }
        }

        throw new ImpossibleException("Failed to select a value.");
    }

    public IEnumerable<T> Keys()
    {
        return _values;
    }

    public IWeightedSampler<T> Remove(T t)
    {
        int i = 0;
        int length = _values.Length;
        List<T> values = new(length);
        List<int> cumulativeWeights = new(length);
        int lastTotal = 0;
        int removedWeight = 0;

        for (; i < length; i++)
        {
            T v = _values[i]!;
            if (t!.Equals(v))
            {
                removedWeight = _cumulativeWeights[i] - lastTotal;
                break;
            }
            lastTotal = _cumulativeWeights[i];
            values.Add(v);
            cumulativeWeights.Add(lastTotal);
        }
        for (i++; i < length; i++) // append subtracted cumulatives for values that followed the removed value
        {
            values.Add(_values[i]);
            cumulativeWeights.Add(_cumulativeWeights[i] - removedWeight);
        }

        return new LinearWeightedRandom<T>(values.ToArray(), cumulativeWeights.ToArray(), _totalWeight - removedWeight); ;
    }

    public int Weight(T t)
    {
        throw new NotImplementedException();
    }
}

public class WeightedShuffler<T>
    where T : notnull
{
    private readonly FrozenDictionary<T, int> _weights;

    public WeightedShuffler(FrozenDictionary<T, int> weights)
    {
        _weights = weights;
    }

    public WeightedShuffler(IEnumerable<(T value, int weight)> entries)
    {
        int size;
        if (entries == null || (size = entries.Count()) == 0) { throw new ArgumentException("Entries cannot be null or empty.", nameof(entries)); }

        _weights = entries.ToFrozenDictionary(e => e.value, e => e.weight);
    }

    public WeightedShuffler(IEnumerable<KeyValuePair<T, int>> dict)
    : this(dict.Select(kvp => (kvp.Key, kvp.Value)))
    {
    }

    /// <summary>
    /// Returns all values shuffled using weights to prioritize sorting
    /// values earlier in the list.
    /// </summary>
    public T[] Shuffle([NotNull] Random r)
    {
        int length = _weights.Count;
        if (length == 0) { return Array.Empty<T>(); }

        // will contain random rolls 0..1 to the exponent 1/weight
        var tuple = new (double key, T value)[length];
        int i = 0;
        foreach (var (value, weight) in _weights)
        {
            double d = r.NextDouble();
            double exp = Math.Pow(d, 1.0 / weight);
            tuple[i] = (exp, value);
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
