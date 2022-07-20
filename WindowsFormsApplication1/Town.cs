using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    /// <summary>
    /// Town indexes.
    /// Because this is a replacement for Digshake's magic ints, this is 1-indexed for compatability.
    /// </summary>
    public enum Town
    {
        //TODO: Probably these should all just be singleton locations, the whole enum indexing thing is unneccessary.
        RAURU = 1,
        RUTO = 2,
        SARIA_NORTH = 3,
        SARIA_SOUTH = 4,
        MIDO = 5,
        NABOORU = 6,
        DARUNIA = 7,
        NEW_KASUTO = 8,
        NEW_KASUTO_2 = 9,
        OLD_KASUTO = 10
    }
}
