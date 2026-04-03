using System;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore;

internal static class ListExtensions
{
    //.NET8 now has a native Random.Shuffle we should use for this.
    //...Except it only works for arrays and spans but not lists, so this is still the most performant option.
    public static void FisherYatesShuffle<T>(this List<T> list, Random RNG)
    {
        for (int iteratedIndex = list.Count - 1; iteratedIndex > 0; --iteratedIndex)
        {
            int shuffleIndex = RNG.Next(0, iteratedIndex + 1);
            (list[iteratedIndex], list[shuffleIndex]) = (list[shuffleIndex], list[iteratedIndex]);
        }
    }
    public static T? Sample<T>(this T[] list, Random RNG)
    {
        return list.Length == 0 ? default : list[RNG.Next(list.Length)];
    }
    public static T[] Sample<T>(this T[] list, Random RNG, int count)
    {
        return RNG.GetItems(list, count);
    }

    public static T? Sample<T>(this List<T> list, Random RNG)
    {
        return list.Count == 0 ? default : list[RNG.Next(list.Count)];
    }

    public static List<T> Sample<T>(this List<T> list, Random RNG, int count)
    {
        return RNG.GetItems(list.ToArray(), count).ToList();
    }
}
