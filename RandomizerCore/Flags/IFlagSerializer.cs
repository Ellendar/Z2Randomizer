using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerCore.Flags;

internal interface IFlagSerializer
{
    public int GetLimit();
    public int Serialize(object obj);
    public object Deserialize(int option);
}
