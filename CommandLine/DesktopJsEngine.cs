using System.Collections;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using NLog;
using RandomizerCore.Asm;

namespace CommandLine;

using System.Reflection;

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
            var modules = new List<List<PropertyBag>>();
            foreach (var module in asm.Modules)
            {
                var outmodule = new List<PropertyBag>();
                foreach (var dict in module.Actions)
                {
                    outmodule.Add(dict.ToPropertyBag());
                }
                modules.Add(outmodule);
            }
            scriptEngine.Script.romdata = data;
            scriptEngine.Script.modules = modules;

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

internal static class DictionaryExtensions
{
    public static PropertyBag ToPropertyBag(this IDictionary<string, object> dictionary)
    {
        var bag = new PropertyBag();
        foreach (var kvp in dictionary)
        {
            switch (kvp.Value)
            {
                case IDictionary<string, object> objects:
                {
                    var inner = objects.ToPropertyBag();
                    bag.Add(kvp.Key, inner);
                    break;
                }
                case ICollection list:
                {
                    var itemList = new List<object>();
                    foreach (var item in list)
                    {
                        if (item is IDictionary<string, object> objs)
                        {
                            var bagitem = objs.ToPropertyBag();
                            itemList.Add(bagitem);
                        }
                        else
                        {
                            itemList.Add(item);
                        }
                    }

                    bag.Add(kvp.Key, itemList);
                    break;
                }
                default:
                    bag.Add(kvp.Key, kvp.Value);
                    break;
            }
        }

        return bag;
    }
}