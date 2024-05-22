using System.Reflection;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using NLog;
using RandomizerCore.Asm;

namespace Desktop.Common;

public class DesktopJsEngine : IAsmEngine
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly V8ScriptEngine scriptEngine;

    public DesktopJsEngine()
    {
        scriptEngine = new();
        // If you need to debug the javascript, add these flags and connect to the debugger through vscode.
        // follow this tutorial for how https://microsoft.github.io/ClearScript/Details/Build.html#_Debugging_with_ClearScript_2
        // scriptEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableRemoteDebugging | V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart);

        scriptEngine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
    }
    
    public async Task<byte[]?> Apply(byte[] rom, Assembler asm)
    {
        return await Task.Run(() =>
        {
            var data = (ITypedArray<byte>) scriptEngine.Evaluate($"new Uint8Array({rom.Length});");
            data.WriteBytes(rom, 0, data.Length, 0);
            var initmodule = new AsmModule();
            var assembly = Assembly.Load("RandomizerCore");
            initmodule.Code(assembly.ReadResource("RandomizerCore.Asm.Init.s"), "__init.s");
            asm.Modules.Insert(0, initmodule);
            scriptEngine.Script.romdata = data;
            scriptEngine.Script.modules = asm.AsExpando();

            scriptEngine.Execute(new DocumentInfo { Category = ModuleCategory.Standard },  /* language=javascript */ """
import { compile } from "js65/js65.js"
compile(modules,romdata);
""");
            byte[] outdata = new byte[rom.Length];
            data.ReadBytes(0, (ulong)outdata.Length, outdata, 0);
            return outdata;
        });
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
