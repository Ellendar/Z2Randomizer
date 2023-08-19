namespace Assembler;


using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using System.Numerics;

public class Assembler
{
    private readonly V8ScriptEngine _engine;

    private List<PropertyBag> _actions = new();

    public Assembler(V8ScriptEngine engine)
    {
        _engine = engine;
        _engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
    }

    public void code(string asm, string name = "")
    {
        _actions.Add(new () {
            { "action", "code" },
            { "code", asm },
            { "name", name },
        });
    }

    public void label(string lb)
    {
        _actions.Add(new () {
            { "action", "label" },
            { "label", lb },
        });
    }

    public void byt(params byte[] bytes)
    {
        _actions.Add(new() {
            { "action", "byte" },
            { "bytes", bytes },
        });
    }
    public void byt(params PropertyBag[] bytes)
    {
        _actions.Add(new() {
            { "action", "byte" },
            { "bytes", bytes },
        });
    }

    public void word(params ushort[] words)
    {
        _actions.Add(new() {
            { "action", "word" },
            { "words", words },
        });
    }
    public void word(params PropertyBag[] words)
    {
        _actions.Add(new() {
            { "action", "word" },
            { "words", words },
        });
    }

    public void org(uint addr, string name = "")
    {
        _actions.Add(new () {
            { "action", "org" },
            { "addr", addr },
            { "name", name },
        });
    }
    public void segment(params string[] name)
    {
        _actions.Add(new () {
            { "action", "segment" },
            { "name", name },
        });
    }

    public void reloc(string name)
    {
        _actions.Add(new()
        {
            { "action", "reloc" },
            { "name", name },
        });
    }

    public PropertyBag symbol(string name)
    {
        // kinda jank, but instead of eating the overhead for creating this token,
        // just hardcode the symbol token
        return new PropertyBag()
        {
            { "op", "sym" },
            { "sym", name },
        };
    }

    public void export(string name)
    {
        _actions.Add(new()
        {
            { "action", "reloc" },
            { "name", name },
        });
    }

    public void relocExportLabel(string name, params string[] segments)
    {
        if (segments.Length > 0)
        {
            segment(segments);
        }
        reloc(name);
        label(name);
        export(name);
    }

    public string module()
    {
        _engine.AddHostObject("actions", _actions);
        return (string) _engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, """
import { Assembler } from "js65/assembler.js"
import { base64 } from 'js65/deps/deno.land/x/b64@1.1.27/src/base64.js';
import { Cpu } from "js65/cpu.js"
import { Preprocessor } from "js65/preprocessor.js"
import { Tokenizer } from "js65/tokenizer.js"
import { TokenStream } from "js65/tokenstream.js"

(async function() {
    let a = new Assembler(Cpu.P02);
    for (const action of actions) {
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
                a.word(...action["bytes"]);
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
        }
    }
    return JSON.stringify(a.module(), (k, v) => {
        if (k === "data" && typeof v === "object") {
        // v == Uint8Array
        return base64.fromArrayBuffer(v);
        }
        return v;
    }, "  ");
})();
""");
        //return (string)_engine.Script.module;
    }
}

public class Engine
{
    private V8ScriptEngine _engine;
    private string _init_module;
    public Engine() {
        // If you need to debug the javascript, add these flags and connect to the debugger through vscode.
        // follow this tutorial for how https://microsoft.github.io/ClearScript/Details/Build.html#_Debugging_with_ClearScript_2
        //_engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart);
        _engine = new V8ScriptEngine();

        // Setup the initial segments for the randomizer
        var a = Asm();
        a.code("""
.segment "HEADER" :size $10
.segment "PRG0"   :bank $00 :size $4000 :mem $8000 :off $00010
.segment "PRG1"   :bank $01 :size $4000 :mem $8000 :off $04010
.segment "PRG2"   :bank $02 :size $4000 :mem $8000 :off $08010
.segment "PRG3"   :bank $03 :size $4000 :mem $8000 :off $0c010
.segment "PRG4"   :bank $04 :size $4000 :mem $8000 :off $10010
.segment "PRG5"   :bank $05 :size $4000 :mem $8000 :off $14010
.segment "PRG6"   :bank $06 :size $4000 :mem $8000 :off $18010
.segment "PRG7"   :bank $07 :size $4000 :mem $c000 :off $1c010
.segment "CHR"    :size $20000 :off $20010 :out

""", "__init.s");
        _init_module = a.module();
    }

    public Assembler Asm()
    {
        return new Assembler(_engine);
    }

    public void Apply(byte[] rom, List<string> modules)
    {
        var data = (ITypedArray<byte>) _engine.Evaluate($"new Uint8Array({rom.Length});");
        data.WriteBytes(rom, 0, data.Length, 0);
        _engine.Script.init_module = _init_module;
        _engine.Script.romdata = data;
        _engine.Script.modules = modules;
        _engine.Execute(new DocumentInfo { Category = ModuleCategory.Standard }, """
import { Linker } from "js65/linker.js"
import { ModuleZ } from "js65/module.js";

(async function() {
    //debugger;
    const linker = new Linker();
    linker.base(romdata, 0);
    linker.read(JSON.parse(init_module));
    for (const m of modules) {
        const parsedModule = await ModuleZ.safeParseAsync(JSON.parse(m));
        // Just assume its successful for now...
        linker.read(parsedModule.data);
    }
    const out = linker.link();
    out.apply(romdata);
})();
""");
        // Copy the patched bytes back into the final rom.
        data.ReadBytes(0, (ulong)rom.Length, rom, 0);
    }
}

