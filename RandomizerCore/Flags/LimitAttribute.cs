using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.Core.Flags;

internal class LimitAttribute : Attribute
{
    public int Limit { get; }
    public LimitAttribute(int limit) 
    {
        Limit = limit;
    }
}
