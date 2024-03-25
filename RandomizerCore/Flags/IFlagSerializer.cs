using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z2Randomizer.Core.Overworld;

namespace RandomizerCore.Flags;

internal interface IFlagSerializer
{
    public int GetLimit();
    public int Serialize(object obj);
    public object Deserialize(int option);
}
