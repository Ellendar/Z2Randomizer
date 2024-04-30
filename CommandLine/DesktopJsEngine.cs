
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
import { Assembler } from "js65/assembler.js"
import { Cpu } from "js65/cpu.js"
import { Linker } from "js65/linker.js"
import { Preprocessor } from "js65/preprocessor.js"
import { Tokenizer } from "js65/tokenizer.js"
import { TokenStream } from "js65/tokenstream.js"

async function processAction(a, action) {
    switch (action["action"]) {
        case "code": {
            const opts = {lineContinuations: true};
            const toks = new TokenStream(undefined, undefined, opts);
            const tokenizer = new Tokenizer(action["code"], action["name"], opts);
            toks.enter(tokenizer);
            const pre = new Preprocessor(toks, a);
            await a.tokens(pre);
            break;
        }
        case "label": {
            a.label(action["label"]);
            a.export(action["label"]);
            break;
        }
        case "byte": {
            a.byte(...action["bytes"]);
            break;
        }
        case "word": {
            a.word(...action["words"]);
            break;
        }
        case "org": {
            a.org(action["addr"], action["name"]);
            break;
        }
        case "reloc": {
            a.reloc(action["name"]);
            break;
        }
        case "export": {
            a.export(action["name"]);
            break;
        }
        case "segment": {
            a.segment(...action["name"]);
            break;
        }
        case "assign": {
            a.assign(action["name"], action["value"]);
            break;
        }
        case "set": {
            a.set(action["name"], action["value"]);
            break;
        }
        case "free": {
            a.free(action["size"]);
        }
    }
}
// This anon function runs the assembler and linker
(async function() {
    debugger;
    // Assemble all of the modules
    const assembled = [];
    for (const module of modules) {
        let a = new Assembler(Cpu.P02);
        for (const action of module) {
            await processAction(a, action);
        }
        assembled.push(a);
    }

    // And now link them together
    const linker = new Linker();
    linker.base(romdata, 0);
    for (const m of assembled) {
        linker.read(m.module());
    }
    const out = linker.link();
    out.apply(romdata);
})();
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