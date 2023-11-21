using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerCore;

internal static class ListExtensions
{
    public static void FisherYatesShuffle<T>(this List<T> list, Random RNG)
    {
        for (int iteratedIndex = list.Count - 1; iteratedIndex > 0; --iteratedIndex)
        {
            int shuffleIndex = RNG.Next(0, list.Count + 1);
            (list[iteratedIndex], list[shuffleIndex]) = (list[shuffleIndex], list[iteratedIndex]);
        }
    }
}
