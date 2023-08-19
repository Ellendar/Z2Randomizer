namespace Assembler;


using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;


public class Assembler
{
    private readonly V8ScriptEngine _engine;

    private List<Dictionary<string, object>> _actions = new();

    public Assembler(V8ScriptEngine engine)
    {
        _engine = engine;
    }

    public void code(string asm)
    {
        _actions.Append(new Dictionary<string, object>() {
            { "action", "code" },
            { "code", asm },
        });
    }

    public void label(string lb)
    {
        _actions.Append(new Dictionary<string, object>() {
            { "action", "label" },
            { "label", lb },
        });
    }

    public void byt(params byte[] bytes)
    {
        _actions.Append(new Dictionary<string, object>() {
            { "action", "byte" },
            { "bytes", bytes },
        });
    }


    public string module()
    {
        _engine.AddHostObject("actions", _actions);
        _engine.Execute("""
    import { Assembler } from "js65/assembler.js"
    import { Cpu } from "js65/cpu.js"

    (async function() {
        let a = new Assembler(Cpu.P02);
        for (const action of actions) {
            switch (action["action"]) {
                case "code":
                    
                case "label":
                    a.label(action["label"]);
                    a.export(action["label"]);
                    break;
                case "byte":
                    a.byte(...action["bytes"]);
                    break;
            }
        }
        let module = JSON.stringify(a.module());
    })();
""");
        return (string)_engine.Script.module;
    }
}

public class Engine
{
    private V8ScriptEngine _engine;
    public Engine() {
        _engine = new V8ScriptEngine();
        _engine.ExecuteDocument("assembler.js");
        _engine.ExecuteDocument("linker.js");
    }

    public Assembler Asm()
    {
        return new Assembler(_engine);
    }

    public void Apply(byte[] rom, List<string> modules)
    {
        _engine.AddHostObject("romdata", rom);
        _engine.AddHostObject("modules", modules);
        _engine.Execute(@"
    const linker = new Linker();
    linker.base(romdata, 0);
    for (const m of modules) {
      linker.read(JSON.parse(m));
    }
    const out = linker.link();
    out.apply(romdata);
");
    }
}

