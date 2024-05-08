using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using RandomizerCore.Asm;

namespace CrossPlatformUI.Browser;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(Assembler))]
[JsonSerializable(typeof(List<AsmModule>))]
[JsonSerializable(typeof(List<Dictionary<string, string>>))]
internal partial class AssmeblerContext : JsonSerializerContext
{
}
public partial class BrowserJsEngine : IAsmEngine
{
    [JSImport("compile", "js65/js65.js")]
    [return: JSMarshalAs<JSType.MemoryView>]
    internal static partial Span<byte> Compile(string asm,
        [JSMarshalAs<JSType.MemoryView>] Span<byte> rom);
    
    public async Task<byte[]?> Apply(byte[] rom, Assembler asmModule)
    {
        var output = await Task.Run(() =>
        {
            var json = JsonSerializer.Serialize(asmModule, AssmeblerContext.Default.Assembler);
            Span<byte> bytes = rom;
            return Compile(json, bytes).ToArray();
        });
        return output;
    }
}