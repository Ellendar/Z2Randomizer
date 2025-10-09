using System;
using System.Diagnostics;

namespace Z2Randomizer.RandomizerCore;

public class WeightedRandom<T>
{
    private readonly T[] _values;
    private readonly int[] _cumulativeWeights;
    private readonly int _totalWeight;

    public WeightedRandom((T value, int weight)[] entries)
    {
        if (entries == null || entries.Length == 0) { throw new ArgumentException("Entries cannot be null or empty.", nameof(entries)); }

        _values = new T[entries.Length];
        _cumulativeWeights = new int[entries.Length];

        int total = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            T value = entries[i].value;
            int weight = entries[i].weight;

            if (weight <= 0) { throw new ArgumentException($"Weight must be positive (entry {i})."); }

            total += weight;
            _values[i] = value;
            _cumulativeWeights[i] = total;
        }

        _totalWeight = total;
    }

    public T Next(Random rng)
    {
        Debug.Assert(rng != null);

        int roll = rng.Next(0, _totalWeight);

        for (int i = 0; i < _cumulativeWeights.Length; ++i)
        {
            if (roll < _cumulativeWeights[i])
            {
                return _values[i];
            }
        }

        throw new ImpossibleException("Failed to select a value.");
    }
}
