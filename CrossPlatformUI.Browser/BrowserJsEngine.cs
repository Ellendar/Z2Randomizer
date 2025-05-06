using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json.Serialization;
namespace CrossPlatformUI.Browser;

[JsonSourceGenerationOptions(WriteIndented = false)]
// [JsonSerializable(typeof(Assembler))]
// [JsonSerializable(typeof(AsmModule[]))]
// [JsonSerializable(typeof(AsmModule))]
[JsonSerializable(typeof(List<List<ExpandoObject>>))]
[JsonSerializable(typeof(List<object>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(int))]
// [JsonSerializable(typeof(List<AsmModule>))]
[JsonSerializable(typeof(Dictionary<string, object>[]))]
// [JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class AssmeblerContext : JsonSerializerContext;

//XXX: We broke the web version. We'll fix it later
/*
public partial class BrowserJsEngine : IAsmEngine
{
    [JSImport("compile", "js65.js65.js")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    private static partial Task<string> Compile(string asm, string rom);

    private readonly Task<JSObject> module;

    private readonly AsmModule initmodule;

    public BrowserJsEngine()
    {
        module = JSHost.ImportAsync("js65.js65.js", "../js65/js65.js");
        
        var assembly = Assembly.Load("RandomizerCore");
        initmodule = new AsmModule();
        initmodule.Code(assembly.ReadResource("Z2Randomizer.RandomizerCore.Asm.Init.s"), "__init.s");
    }
    
    public async Task<byte[]?> Apply(byte[] rom, Assembler asmModule)
    {
        _ = await module;
        asmModule.Modules.Insert(0, initmodule);
        var expando = asmModule.AsExpando();
        var json = JsonSerializer.Serialize(expando, AssmeblerContext.Default.ListListExpandoObject);
        var b64Bytes = Convert.ToBase64String(rom);
        var output = await Compile(json, b64Bytes);
        return Convert.FromBase64String(output);
    }
}

internal static class AssemblyExtensions
{
    public static string ReadResource(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    public static async Task<string> ReadResourceAsync(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        await using var stream = assembly.GetManifestResourceStream(name)!;
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }
    public static byte[] ReadBinaryResource(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new BinaryReader(stream);
        return reader.ReadBytes((int)stream.Length);
    }
}
*/