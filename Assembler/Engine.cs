namespace Assembler;

using System.Reflection;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

public class Engine
{
    internal V8ScriptEngine scriptEngine;
    public List<List<PropertyBag>> Modules { get; set; } = new();
    private Actions InitModule { get; } = new();

    public Engine()
    {
        scriptEngine = new();
        scriptEngine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

        // If you need to debug the javascript, add these flags and connect to the debugger through vscode.
        // follow this tutorial for how https://microsoft.github.io/ClearScript/Details/Build.html#_Debugging_with_ClearScript_2
        //this.scriptEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableRemoteDebugging | V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart);
        
        // Setup the initial segments for the randomizer
        Assembler assembler = new();
        assembler.Code(Assembly.GetExecutingAssembly().ReadResource("Assembler.Init.s"), "__init.s");
        InitModule = assembler.Actions;
    }

    public void Apply(byte[] rom)
    {
        var data = (ITypedArray<byte>) scriptEngine.Evaluate($"new Uint8Array({rom.Length});");
        data.WriteBytes(rom, 0, data.Length, 0);
        scriptEngine.Script.romdata = data;
        scriptEngine.Script.modules = Modules;
        scriptEngine.Script.initmodule = InitModule;
        try
        {
            scriptEngine.Execute(new DocumentInfo { Category = ModuleCategory.Standard }, /* language=javascript */ """

import { Assembler } from "js65/assembler.js"
import { base64 } from 'js65/deps/deno.land/x/b64@1.1.27/src/base64.js';
import { Cpu } from "js65/cpu.js"
import { Linker } from "js65/linker.js"
import { ModuleZ } from "js65/module.js";
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
    }
}

// This anon function runs the assembler and linker
(async function() {
    debugger;
    // Assemble all of the modules
    const assembled = [];
    let initmod = new Assembler(Cpu.P02);
    for (const action of initmodule) {
        await processAction(initmod, action);
    }
    assembled.push(initmod);
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
            // Copy the patched bytes back into the final rom.
            data.ReadBytes(0, (ulong)rom.Length, rom, 0);
        }
        catch(Exception)
        {
            throw;
        }
    }
}

internal static class AssemblyExtensions
{
    public static string ReadResource(this Assembly assembly, string name)
    {
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        using var stream = assembly.GetManifestResourceStream(name);
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
}
