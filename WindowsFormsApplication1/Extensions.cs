using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    static class Extensions
    {
        public static void Shuffle<T>(this List<T> list, Random random)
        {
            T temp;
            for (int i = list.Count() - 1; i > 0; i--)
            {
                int n = random.Next(i + 1);
                temp = list[i];
                list[i] = list[n];
                list[n] = temp;
            }
        }
    }
}
