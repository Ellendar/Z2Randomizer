using System;
using System.Collections.Generic;

namespace RandomizerCore;

internal static class ListExtensions
{
    //TODO: .NET8 now has a native Random.Shuffle we should use for this.
    public static void FisherYatesShuffle<T>(this List<T> list, Random RNG)
    {
        for (int iteratedIndex = list.Count - 1; iteratedIndex > 0; --iteratedIndex)
        {
            int shuffleIndex = RNG.Next(0, iteratedIndex + 1);
            (list[iteratedIndex], list[shuffleIndex]) = (list[shuffleIndex], list[iteratedIndex]);
        }
    }

    public static T Sample<T>(this List<T> list, Random RNG)
    {
        return list[RNG.Next(list.Count)];
    }

    public static T Sample<T>(this List<T> list, Random RNG, int count)
    {
        //TODO: Implement this with Random.Shared.GetItems(array, count);
        throw new NotImplementedException();
    }
}
