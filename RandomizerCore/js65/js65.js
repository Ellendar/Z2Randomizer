import { Assembler } from "./assembler.js"
import { Cpu } from "./cpu.js"
import { Linker } from "./linker.js"
import { Preprocessor } from "./preprocessor.js"
import { Tokenizer } from "./tokenizer.js"
import { TokenStream } from "./tokenstream.js"

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
export async function compile(modules,romdata) {
    // debugger;
    let mods = modules;
    if (typeof romdata === 'string') {
        mods = JSON.parse(modules);
    }
    // Assemble all of the modules
    const assembled = [];
    for (const module of mods) {
        let a = new Assembler(Cpu.P02);
        for (const action of module) {
            await processAction(a, action);
        }
        assembled.push(a);
    }
    
    let rombytes = romdata;
    if (typeof romdata === 'string') {
        rombytes = new Uint8Array(window.base64ToArrayBuffer(romdata));
    }
    // And now link them together
    const linker = new Linker();
    linker.base(rombytes, 0);
    for (const m of assembled) {
        linker.read(m.module());
    }
    const out = linker.link();
    out.apply(rombytes);
    if (typeof romdata === 'string') {
        rombytes = window.arrayBufferToBase64(rombytes);
    }
    return rombytes;
}
