using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore;

internal sealed class ShuffleBag<T>
{
    private readonly List<T> source;
    private readonly Queue<T> queue = new();
    private readonly Random random;

    public ShuffleBag(IEnumerable<T> items, Random random)
    {
        source = items.ToList();
        if (source.Count == 0)
        {
            throw new ArgumentException("Shuffle bag requires at least one item.", nameof(items));
        }

        this.random = random;
    }

    public T Draw()
    {
        if (queue.Count == 0)
        {
            Refill();
        }

        return queue.Dequeue();
    }

    private void Refill()
    {
        List<T> shuffledItems = [.. source];
        shuffledItems.FisherYatesShuffle(random);

        foreach (T item in shuffledItems)
        {
            queue.Enqueue(item);
        }
    }
}
