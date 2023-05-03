using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.Core.Flags;

internal class MinimumAttribute : Attribute
{
    public int Minimum { get; }
    public MinimumAttribute(int minimum) 
    {
        Minimum = minimum;
    }
}
