namespace Assembler;


using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

public class Assembler
{
    private readonly V8ScriptEngine _engine;

    public List<PropertyBag> Actions { get; } = new();

    public Assembler(V8ScriptEngine engine)
    {
        _engine = engine;
        _engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
    }

    public void code(string asm, string name = "")
    {
        Actions.Add(new () {
            { "action", "code" },
            { "code", asm },
            { "name", name },
        });
    }

    public void label(string lb)
    {
        Actions.Add(new () {
            { "action", "label" },
            { "label", lb },
        });
    }

    public void byt(params byte[] bytes)
    {
        Actions.Add(new() {
            { "action", "byte" },
            { "bytes", bytes },
        });
    }
    public void byt(params PropertyBag[] bytes)
    {
        Actions.Add(new() {
            { "action", "byte" },
            { "bytes", bytes },
        });
    }

    public void word(params ushort[] words)
    {
        Actions.Add(new() {
            { "action", "word" },
            { "words", words },
        });
    }
    public void word(params PropertyBag[] words)
    {
        Actions.Add(new() {
            { "action", "word" },
            { "words", words },
        });
    }

    public void org(ushort addr, string name = "")
    {
        Actions.Add(new() {
            { "action", "org" },
            { "addr", addr },
            { "name", name },
        });
    }

    // Converts from file address space to CPU address space, just a helper function
    // since all the current addresses in the randomizer are in file address space
    public void romorg(int addr, string name = "")
    {
        // adjustment for the ines header
        int romaddr = addr - 0x10;
        byte segment = (byte) (romaddr / 0x4000);
        ushort cpuoffset = (ushort) (segment == 7 ? 0xc000 : 0x8000);
        ushort cpuaddr = (ushort) ((romaddr % 0x4000) + cpuoffset);

        Actions.Add(new() {
            { "action", "org" },
            { "addr", cpuaddr },
            { "name", name },
        });
    }
    public void segment(params string[] name)
    {
        Actions.Add(new () {
            { "action", "segment" },
            { "name", name },
        });
    }

    public void reloc(string name = "")
    {
        Actions.Add(new()
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
        Actions.Add(new()
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
}

public class Engine
{
    private V8ScriptEngine _engine;
    public List<List<PropertyBag>> Modules { get; } = new();
    private List<PropertyBag> InitModule { get; } = new();

    public Engine() {
        // If you need to debug the javascript, add these flags and connect to the debugger through vscode.
        // follow this tutorial for how https://microsoft.github.io/ClearScript/Details/Build.html#_Debugging_with_ClearScript_2
        //_engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart);
        _engine = new V8ScriptEngine();

        // Setup the initial segments for the randomizer
        var a = Asm();
        a.code("""
;;; Initialization. This must come before all other modules.

;;; Tag for labels that we expect to override vanilla
.define OVERRIDE

;;; Nicer syntax for declaring free sections
.define FREE {seg [start, end)} \
    .pushseg seg .eol \
    .org start .eol \
    .free end - start .eol \
    .popseg
.define FREE {seg [start, end]} .noexpand FREE seg [start, end + 1)


;;; Relocate a block of code and update refs
;;; Usage:
;;;   RELOCATE segments [start, end) refs...
;;; Where |segments| is an optional comma-separated list of segment
;;; names, and |refs| is a space-separated list of addresses whose
;;; contents point to |start| and that need to be updated to point to
;;; whereever it eventually ended up.  If no segments are specified
;;; then the relocation will stay within the current segment.
.define RELOCATE {seg [start, end) refs .eol} \
.org start .eol \
: FREE_UNTIL end .eol \
.ifnblank seg .eol \
.pushseg seg .eol \
.endif .eol \
.reloc .eol \
: .move (end-start), :-- .eol \
.ifnblank seg .eol \
.popseg .eol \
.endif .eol \
UPDATE_REFS :- @ refs

;;; Update a handful of refs to point to the given address.
;;; Usage:
;;;   UPDATE_REFS target @ refs...
;;; Where |refs| is a space-separated list of addresses, and
;;; |target| is an address or label to insert into each ref.
.define UPDATE_REFS {target @ ref refs .eol} \
.org ref .eol \
  .word (target) .eol \
UPDATE_REFS target @ refs
.define UPDATE_REFS {target @ .eol}


.macro FREE_UNTIL end
  .assert * <= end
  .free end - *
.endmacro


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

; Mark unused areas in the ROM so the linker can place stuff here

FREE "PRG0" [$AA40, $c000)

FREE "PRG4" [$83DC, $8470)
FREE "PRG4" [$84f0, $8500)
FREE "PRG4" [$8508, $850C)
FREE "PRG4" [$870E, $871B)
FREE "PRG4" [$8817, $88A0)
FREE "PRG4" [$8EC3, $9400)
FREE "PRG4" [$9EE0, $a000)
FREE "PRG4" [$A1E3, $A1F8)
FREE "PRG4" [$A3FB, $A440)
FREE "PRG4" [$A539, $A640)
FREE "PRG4" [$A765, $A900)
FREE "PRG4" [$BEFD, $BF00)
FREE "PRG4" [$bf60, $c000)

FREE "PRG5" [$834e, $84d0)
FREE "PRG5" [$861f, $871b)
FREE "PRG5" [$8817, $88a0)
FREE "PRG5" [$93ae, $9400)
FREE "PRG5" [$a54f, $a600)
FREE "PRG5" [$bda1, $c000)

; DPCM data, will affect dpcm sfx but not gameplay so its fine to use this as a last ditch
; free space for patches
FREE "PRG7" [$f369, $fcfb);


""", "__init.s");
        InitModule = a.Actions;
    }

    public Assembler Asm()
    {
        return new Assembler(_engine);
    }

    public void Apply(byte[] rom)
    {
        var data = (ITypedArray<byte>) _engine.Evaluate($"new Uint8Array({rom.Length});");
        data.WriteBytes(rom, 0, data.Length, 0);
        _engine.Script.romdata = data;
        _engine.Script.modules = Modules;
        _engine.Script.initmodule = InitModule;
        _engine.Execute(new DocumentInfo { Category = ModuleCategory.Standard }, /* language=javascript */ """

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
    }
}

// This anon function runs the assembler and linker
(async function() {
    debugger;
    // Assemble all of the modules
    const assembled = [];
    // Setup the original code as overwriteMode require to make it so the code is
    // only overwritten if its freed first
    let initmod = new Assembler(Cpu.P02, {overwriteMode: 'require'});
    for (const action of initmodule) {
        await processAction(initmod, action);
    }
    assembled.push(initmod);
    for (const module of modules) {
        // Set all custom modules to overwrite mode forbid to prevent patches from overwriting each other
        let a = new Assembler(Cpu.P02, {overwriteMode: 'forbid'});
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
}

