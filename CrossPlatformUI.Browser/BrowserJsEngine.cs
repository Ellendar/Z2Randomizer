using js65;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CrossPlatformUI.Browser;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(List<List<ExpandoObject>>))]
[JsonSerializable(typeof(List<object>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(Dictionary<string, object>[]))]
[JsonSerializable(typeof(Js65Options))]
[JsonSerializable(typeof(Js65Callbacks))]
internal partial class AssemblerContext : JsonSerializerContext;

[SupportedOSPlatform("browser")]
public partial class BrowserJsEngine : Assembler
{
    public BrowserJsEngine(Js65Options? options) : base(options, null)
    {
        Callbacks = new();
        Callbacks.OnFileReadText = LoadTextFileCallback;
        Callbacks.OnFileReadBinary = LoadBinaryFileCallback;
    }

    [JSImport("compile", "js65.libassembler.js")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    private static partial Task<string> Compile(string asm, string rom, string options,
        [JSMarshalAs<JSType.Function<JSType.String,JSType.String,JSType.String>>]
        Func<string, string, string> textCallback,
        [JSMarshalAs<JSType.Function<JSType.String,JSType.String,JSType.String>>]
        Func<string, string, string> binaryCallback
    );

    private readonly Task<JSObject> _module = JSHost.ImportAsync("js65.libassembler.js", "js65/libassembler.js");
    private readonly Assembly _assembly = Assembly.Load("RandomizerCore");

    public override async Task<byte[]?> Apply(byte[] rom)
    {
        // Import the module and wait for it to finish
        _ = await _module;
        var expando = IntoExpandoObject();
        var modulesJson = JsonSerializer.Serialize(expando, AssemblerContext.Default.ListListExpandoObject);
        var optsJson = JsonSerializer.Serialize(Options, AssemblerContext.Default.Js65Options);
        var b64Bytes = Convert.ToBase64String(rom);
        var output = await Compile(modulesJson, b64Bytes, optsJson,
            (basePath, filePath) => Callbacks?.OnFileReadText?.Invoke(basePath, filePath) ?? "",
            (basePath, filePath) => Convert.ToBase64String(Callbacks?.OnFileReadBinary?.Invoke(basePath, filePath) ?? []));
        return Convert.FromBase64String(output);
    }

    private string LoadTextFileCallback(string basePath, string relPath)
    {
        return _assembly.ReadResource(relPath);
    }
    private byte[] LoadBinaryFileCallback(string basePath, string relPath)
    {
        return _assembly.ReadBinaryResource(relPath);
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
