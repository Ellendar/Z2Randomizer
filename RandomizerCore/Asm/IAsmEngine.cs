using System.Threading.Tasks;

namespace RandomizerCore.Asm;

public interface IAsmEngine
{
    public Task<byte[]?> Apply(byte[] rom, Assembler asmModule);
}
