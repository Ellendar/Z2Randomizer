
using System.Collections.Generic;

namespace RandomizerCore.Asm;

public class Assembler
{
    public List<AsmModule> Modules { get; } = new();

    public void Add(AsmModule asmModule)
    {
        Modules.Add(asmModule);
    }

    public AsmModule Module()
    {
        var mod = new AsmModule();
        Add(mod);
        return mod;
    }
}
