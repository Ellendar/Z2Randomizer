import { Assembler } from './assembler.js';
import { Cpu } from './cpu.js';
import * as Exprs from './expr.js';
import { Segment } from './module.js';
import { Targets } from "./preamble.js";
import { Preprocessor } from './preprocessor.js';
import * as Tokens from './token.js';
import { Tokenizer } from './tokenizer.js';
import { TokenStream } from './tokenstream.js';
import { IntervalSet, SparseByteArray, binaryInsert } from './util.js';
export class Linker {
    // TODO - accept a list of [filename, contents]?
    static assemble(contents) {
        const opts = { lineContinuations: true };
        const source = new Tokenizer(contents, 'contents.s', opts);
        const asm = new Assembler(Cpu.P02);
        const toks = new TokenStream(undefined, undefined, opts);
        toks.enter(source);
        const pre = new Preprocessor(toks, asm);
        asm.tokens(pre);
        const linker = new Linker();
        //linker.base(this.prg, 0);
        linker.read(asm.module());
        const out = linker.link();
        const data = new Uint8Array(out.length);
        out.apply(data);
        return data;
    }
    static link(...files) {
        const linker = new Linker();
        for (const file of files) {
            linker.read(file);
        }
        return linker.link();
    }
    constructor(opts = {}) {
        Object.defineProperty(this, "opts", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "_link", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Link()
        });
        Object.defineProperty(this, "_exports", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        this.opts = opts;
    }
    read(file) {
        this._link.readFile(file);
        return this;
    }
    base(data, offset = 0) {
        this._link.base(data, offset);
        return this;
    }
    link() {
        const target = Targets.get(this.opts.target?.toLowerCase());
        if (target) {
            target.segments.forEach(seg => this._link.addRawSegment(seg));
        }
        return this._link.link();
    }
    report(verbose = false) {
        console.log(this._link.report(verbose));
    }
    exports() {
        if (this._exports)
            return this._exports;
        return this._exports = this._link.buildExports();
    }
    watch(...offset) {
        this._link.watches.push(...offset);
    }
}
// TODO - link-time only function for getting either the original or the
//        patched byte.  Would allow e.g. copy($8000, $2000, "1e") to move
//        a bunch of code around without explicitly copy-pasting it in the
//        asm patch.
// Tracks an export.
// interface Export {
//   chunks: Set<number>;
//   symbol: number;
// }
function fail(msg) {
    throw new Error(msg);
}
class LinkSegment {
    constructor(segment) {
        Object.defineProperty(this, "name", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "bank", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "size", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "offset", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "memory", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "addressing", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "fill", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        const name = this.name = segment.name;
        this.bank = segment.bank ?? 0;
        this.addressing = segment.addressing ?? 2;
        this.size = segment.size ?? fail(`Size must be specified: ${name}`);
        this.offset = segment.offset ?? fail(`Offset must be specified: ${name}`);
        // this.memory = segment.memory ?? fail(`Memory must be specified: ${name}`);
        // Allow memory offset to be null for non-prg segments
        this.memory = segment.memory ?? 0;
        this.fill = segment.fill ?? 0;
    }
    // offset = org + delta
    get delta() { return this.offset - this.memory; }
}
class LinkChunk {
    constructor(linker, index, chunk, chunkOffset, symbolOffset) {
        Object.defineProperty(this, "linker", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: linker
        });
        Object.defineProperty(this, "index", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: index
        });
        Object.defineProperty(this, "name", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "size", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "segments", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "asserts", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "subs", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Set()
        });
        Object.defineProperty(this, "selfSubs", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Set()
        });
        /** Global IDs of chunks needed to locate before we can complete this one. */
        Object.defineProperty(this, "deps", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Set()
        });
        /** Symbols that are imported into this chunk (these are also deps). */
        Object.defineProperty(this, "imports", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Set()
        });
        // /** Symbols that are exported from this chunk. */
        // exports = new Set<string>();
        Object.defineProperty(this, "follow", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Map()
        });
        /**
         * Whether the chunk is placed overlapping with something else.
         * Overlaps aren't written to the patch.
         */
        Object.defineProperty(this, "overlaps", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: false
        });
        Object.defineProperty(this, "_data", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "_org", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "_offset", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "_segment", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "_overwrite", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        this.name = chunk.name;
        this.size = chunk.data.length;
        this.segments = chunk.segments;
        this._data = chunk.data;
        for (const sub of chunk.subs || []) {
            this.subs.add(translateSub(sub, chunkOffset, symbolOffset));
        }
        this.asserts = (chunk.asserts || [])
            .map(e => translateExpr(e, chunkOffset, symbolOffset));
        if (chunk.org)
            this._org = chunk.org;
        this._overwrite = chunk.overwrite || 'allow';
    }
    get org() { return this._org; }
    get offset() { return this._offset; }
    get segment() { return this._segment; }
    get data() { return this._data ?? fail('no data'); }
    initialPlacement() {
        // Invariant: exactly one of (data) or (org, _offset, _segment) is present.
        // If (org, ...) filled in then we use linker.data instead.
        // We don't call this in the ctor because it depends on all the segments
        // being loaded, but it's the first thing we do in link().
        if (this._org == null)
            return;
        const eligibleSegments = [];
        for (const name of this.segments) {
            const s = this.linker.segments.get(name);
            if (!s)
                throw new Error(`Unknown segment: ${name}`);
            if (this._org >= s.memory && this._org < s.memory + s.size) {
                eligibleSegments.push(s);
            }
        }
        if (eligibleSegments.length !== 1) {
            throw new Error(`Non-unique segment for ${this.name}:\n${''}Segments: ${this.segments.join(',')}, ${''}org: $${this.org?.toString(16)}, ${''}offset: $${this.offset?.toString(16)}\n${''}Eligible: [${eligibleSegments}]`);
        }
        const segment = eligibleSegments[0];
        if (this._org >= segment.memory + segment.size) {
            throw new Error(`Chunk does not fit in segment ${segment.name}`);
        }
        this.place(this._org, segment, this._overwrite);
    }
    // NOTE: overwrite is only passed for direct placements!
    place(org, segment, overwrite) {
        this._org = org;
        this._segment = segment;
        const offset = this._offset = org + segment.delta;
        for (const w of this.linker.watches) {
            if (w >= offset && w < offset + this.size)
                fail("Unable to place");
        }
        binaryInsert(this.linker.placed, x => x[0], [offset, this]);
        // Copy data, leaving out any holes
        const full = this.linker.data;
        const data = this._data ?? fail(`No data`);
        this._data = undefined;
        if (this.subs.size) {
            full.splice(offset, data.length);
            const sparse = new SparseByteArray();
            sparse.set(0, data);
            for (const sub of this.subs) {
                sparse.splice(sub.offset, sub.size);
            }
            for (const [start, chunk] of sparse.chunks()) {
                full.set(offset + start, ...chunk);
            }
        }
        else {
            full.set(offset, data);
        }
        if (overwrite && data.length) {
            // Regardless of the check mode, it's a direct write so record it
            let overwritten = false;
            const [next] = this.linker.written.tail(offset);
            if (next?.[0] <= offset && next[1] >= offset + data.length) {
                overwritten = true;
            }
            else if (next?.[0] < offset + data.length) {
                overwritten = null;
            }
            let error = '';
            if (overwrite === 'require' && overwritten !== true) {
                error = `required to overwrite ${data.length} bytes but did not.`;
            }
            else if (overwrite === 'forbid' && overwritten !== false) {
                error = `forbidden to overwrite ${data.length} but did anyway.`;
            }
            if (error) {
                error = `Chunk at ${segment.name}:$${org.toString(16).padStart(4, '0')} (offset $${offset.toString(16).padStart(5, '0')} was ${error}`;
                if (!NO_THROW)
                    throw new Error(error);
                if (!QUIET)
                    console.error(error);
            }
            this.linker.written.add(offset, offset + data.length);
        }
        // Retry the follow-ons
        for (const [sub, chunk] of this.follow) {
            chunk.resolveSub(sub, false);
        }
        this.linker.free.delete(this.offset, this.offset + this.size);
    }
    resolveSubs(initial = false) {
        // iterate over the subs, see what progres we can make?
        // result: list of dependent chunks.
        // NOTE: if we depend on ourself then we will return empty deps,
        //       and may be placed immediately, but will still have holes.
        //      - NO, it's responsibility of caller to check that
        for (const sub of this.selfSubs) {
            this.resolveSub(sub, initial);
        }
        // const deps = new Set();
        for (const sub of this.subs) {
            // const subDeps = 
            this.resolveSub(sub, initial);
            // if (!subDeps) continue;
            // for (const dep of subDeps) {
            //   let subs = deps.get(dep);
            //   if (!subs) deps.set(dep, subs = []);
            //   subs.push(sub);
            // }
        }
        // if (this.org != null) return new Set();
        // return deps;
    }
    addDep(sub, dep) {
        if (dep === this.index && this.subs.delete(sub))
            this.selfSubs.add(sub);
        this.linker.chunks[dep].follow.set(sub, this);
        this.deps.add(dep);
    }
    // Returns a list of dependent chunks, or undefined if successful.
    resolveSub(sub, initial) {
        // TODO - resolve(resolver) via chunkData to resolve banks!!
        // Do a full traverse of the expression - see what's blocking us.
        if (!this.subs.has(sub) && !this.selfSubs.has(sub))
            return;
        sub.expr = Exprs.traverse(sub.expr, (e, rec, p) => {
            // First handle most common bank byte case, since it triggers on a
            // different type of resolution.
            if (initial && p?.op === '^' && p.args.length === 1 && e.meta) {
                if (e.meta.bank == null) {
                    this.addDep(sub, e.meta.chunk);
                }
                return e; // skip recursion either way.
            }
            e = this.linker.resolveLink(Exprs.evaluate(rec(e)));
            if (initial && e.meta?.rel)
                this.addDep(sub, e.meta.chunk);
            return e;
        });
        // PROBLEM - off is relative to the chunk, but we want to be able to
        // specify an ABSOLUTE org within a segment...!
        // An absolute offset within the whole orig is no good, either
        // want to write it as .segment "foo"; Sym = $1234
        // Could also just do .move count, "seg", $1234 and store a special op
        // that uses both sym and num?
        // See if we can do it immediately.
        let del = false;
        if (sub.expr.op === 'num' && !sub.expr.meta?.rel) {
            this.writeValue(sub.offset, sub.expr.num, sub.size);
            del = true;
        }
        else if (sub.expr.op === '.move') {
            if (sub.expr.args.length !== 1)
                throw new Error(`bad .move`);
            const child = sub.expr.args[0];
            if (child.op === 'num' && child.meta?.offset != null) {
                const delta = child.meta.offset - (child.meta.rel ? 0 : child.meta.org);
                const start = child.num + delta;
                const slice = this.linker.orig.slice(start, start + sub.size);
                this.writeBytes(sub.offset, Uint8Array.from(slice));
                del = true;
            }
        }
        if (del) {
            this.subs.delete(sub) || this.selfSubs.delete(sub);
            if (!this.subs.size) { // NEW: ignores self-subs now
                // if (!this.subs.size || (deps.size === 1 && deps.has(this.index)))  {
                // add to resolved queue - ready to be placed!
                // Question: should we place it right away?  We place the fixed chunks
                // immediately in the ctor, but there's no choice to defer.  For reloc
                // chunks, it's better to wait until we've resolved as much as possible
                // before placing anything.  Fortunately, placing a chunk will
                // automatically resolve all deps now!
                if (this.linker.unresolvedChunks.delete(this)) {
                    this.linker.insertResolved(this);
                }
            }
        }
    }
    writeBytes(offset, bytes) {
        if (this._data) {
            this._data.subarray(offset, offset + bytes.length).set(bytes);
        }
        else if (this._offset != null) {
            this.linker.data.set(this._offset + offset, bytes);
        }
        else {
            throw new Error(`Impossible`);
        }
    }
    writeValue(offset, val, size) {
        // TODO - this is almost entirely copied from processor writeNumber
        const bits = (size) << 3;
        if (val != null && (val < (-1 << bits) || val >= (1 << bits))) {
            const name = ['byte', 'word', 'farword', 'dword'][size - 1];
            throw new Error(`Not a ${name}: $${val.toString(16)} at $${(this.org + offset).toString(16)}`);
        }
        const bytes = new Uint8Array(size);
        for (let i = 0; i < size; i++) {
            bytes[i] = val & 0xff;
            val >>= 8;
        }
        this.writeBytes(offset, bytes);
    }
}
function translateSub(s, dc, ds) {
    s = { ...s };
    s.expr = translateExpr(s.expr, dc, ds);
    return s;
}
function translateExpr(e, dc, ds) {
    e = { ...e };
    if (e.meta)
        e.meta = { ...e.meta };
    if (e.args)
        e.args = e.args.map(a => translateExpr(a, dc, ds));
    if (e.meta?.chunk != null)
        e.meta.chunk += dc;
    if (e.op === 'sym' && e.num != null)
        e.num += ds;
    return e;
}
function translateSymbol(s, dc, ds) {
    s = { ...s };
    if (s.expr)
        s.expr = translateExpr(s.expr, dc, ds);
    return s;
}
// This class is single-use.
class Link {
    constructor() {
        Object.defineProperty(this, "data", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new SparseByteArray()
        });
        Object.defineProperty(this, "orig", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new SparseByteArray()
        });
        // Maps symbol to symbol # // [symbol #, dependent chunks]
        Object.defineProperty(this, "exports", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Map()
        }); // readonly [number, Set<number>]>();
        Object.defineProperty(this, "chunks", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        Object.defineProperty(this, "symbols", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        Object.defineProperty(this, "written", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new IntervalSet()
        });
        Object.defineProperty(this, "free", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new IntervalSet()
        });
        Object.defineProperty(this, "rawSegments", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Map()
        });
        Object.defineProperty(this, "segments", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Map()
        });
        Object.defineProperty(this, "resolvedChunks", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        Object.defineProperty(this, "unresolvedChunks", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Set()
        });
        Object.defineProperty(this, "watches", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        }); // debugging aid: offsets to watch.
        Object.defineProperty(this, "placed", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        Object.defineProperty(this, "initialReport", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: ''
        });
    }
    // TODO - deferred - store some sort of dependency graph?
    insertResolved(chunk) {
        binaryInsert(this.resolvedChunks, c => c.size, chunk);
    }
    base(data, offset = 0) {
        this.data.set(offset, data);
        this.orig.set(offset, data);
    }
    readFile(file) {
        const dc = this.chunks.length;
        const ds = this.symbols.length;
        // segments come first, since LinkChunk constructor needs them
        for (const segment of file.segments || []) {
            this.addRawSegment(segment);
        }
        for (const chunk of file.chunks || []) {
            const lc = new LinkChunk(this, this.chunks.length, chunk, dc, ds);
            this.chunks.push(lc);
        }
        for (const symbol of file.symbols || []) {
            this.symbols.push(translateSymbol(symbol, dc, ds));
        }
        // TODO - what the heck do we do with segments?
        //      - in particular, who is responsible for defining them???
        // Basic idea:
        //  1. get all the chunks
        //  2. build up a dependency graph
        //  3. write all fixed chunks, memoizing absolute offsets of
        //     missing subs (these are not eligible for coalescing).
        //     -- probably same treatment for freed sections
        //  4. for reloc chunks, find the biggest chunk with no deps.
    }
    // resolveChunk(chunk: LinkChunk) {
    //   //if (chunk.resolving) return; // break any cycles
    // }
    resolveLink(expr) {
        if (expr.op === '.orig' && expr.args?.length === 1) {
            const child = expr.args[0];
            const offset = child.meta?.offset;
            if (offset != null) {
                const num = this.orig.get(offset + child.num);
                if (num != null)
                    return { op: 'num', num };
            }
        }
        else if (expr.op === 'num' && expr.meta?.chunk != null) {
            const meta = expr.meta;
            const chunk = this.chunks[meta.chunk];
            if (chunk.org !== meta.org ||
                chunk.segment?.bank !== meta.bank ||
                chunk.offset !== meta.offset) {
                const meta2 = {
                    org: chunk.org,
                    offset: chunk.offset,
                    bank: chunk.segment?.bank,
                };
                expr = Exprs.evaluate({ ...expr, meta: { ...meta, ...meta2 } });
            }
        }
        return expr;
    }
    // NOTE: so far this is only used for asserts?
    // It basically copy-pastes from resolveSubs... :-(
    resolveExpr(expr) {
        expr = Exprs.traverse(expr, (e, rec) => {
            return this.resolveLink(Exprs.evaluate(rec(e)));
        });
        if (expr.op === 'num' && !expr.meta?.rel)
            return expr.num;
        const at = Tokens.at(expr);
        throw new Error(`Unable to fully resolve expr${at}`);
    }
    link() {
        // Build up the LinkSegment objects
        for (const [name, segments] of this.rawSegments) {
            let s = segments[0];
            for (let i = 1; i < segments.length; i++) {
                s = Segment.merge(s, segments[i]);
            }
            this.segments.set(name, new LinkSegment(s));
        }
        // Add the free space
        for (const [name, segments] of this.rawSegments) {
            const s = this.segments.get(name);
            for (const segment of segments) {
                const free = segment.free;
                // Add the free space
                for (const [start, end] of free || []) {
                    this.free.add(start + s.delta, end + s.delta);
                    this.data.splice(start + s.delta, end - start);
                }
            }
        }
        // Set up all the initial placements.
        for (const chunk of this.chunks) {
            chunk.initialPlacement();
        }
        if (DEBUG) {
            this.initialReport = `Initial:\n${this.report(true)}`;
        }
        // Find all the exports.
        for (let i = 0; i < this.symbols.length; i++) {
            const symbol = this.symbols[i];
            // TODO - we'd really like to identify this earlier if at all possible!
            if (!symbol.expr)
                throw new Error(`Symbol ${i} never resolved`);
            // look for imports/exports
            if (symbol.export != null) {
                this.exports.set(symbol.export, i);
            }
        }
        // Resolve all the imports in all symbol and chunk.subs exprs.
        for (const symbol of this.symbols) {
            symbol.expr = this.resolveSymbols(symbol.expr);
        }
        for (const chunk of this.chunks) {
            for (const sub of [...chunk.subs, ...chunk.selfSubs]) {
                sub.expr = this.resolveSymbols(sub.expr);
            }
            for (let i = 0; i < chunk.asserts.length; i++) {
                chunk.asserts[i] = this.resolveSymbols(chunk.asserts[i]);
            }
        }
        // At this point, we don't care about this.symbols at all anymore.
        // Now figure out the full dependency tree: chunk #X requires chunk #Y
        for (const c of this.chunks) {
            c.resolveSubs(true);
        }
        // TODO - fill (un)resolvedChunks
        //   - gets 
        const chunks = [...this.chunks];
        chunks.sort((a, b) => b.size - a.size);
        for (const chunk of chunks) {
            chunk.resolveSubs();
            if (chunk.subs.size) {
                this.unresolvedChunks.add(chunk);
            }
            else {
                this.insertResolved(chunk);
            }
        }
        let count = this.resolvedChunks.length + 2 * this.unresolvedChunks.size;
        while (count) {
            const c = this.resolvedChunks.pop();
            if (c) {
                this.placeChunk(c);
            }
            else {
                // resolve all the first unresolved chunks' deps
                const [first] = this.unresolvedChunks;
                for (const dep of first.deps) {
                    const chunk = this.chunks[dep];
                    if (chunk.org == null)
                        this.placeChunk(chunk);
                }
            }
            const next = this.resolvedChunks.length + 2 * this.unresolvedChunks.size;
            if (next === count) {
                console.error(this.resolvedChunks, this.unresolvedChunks);
                throw new Error(`Not making progress`);
            }
            count = next;
        }
        // if (!chunk.org && !chunk.subs.length) this.placeChunk(chunk);
        // At this point the dep graph is built - now traverse it.
        // const place = (i: number) => {
        //   const chunk = this.chunks[i];
        //   if (chunk.org != null) return;
        //   // resolve first
        //   const remaining: Substitution[] = [];
        //   for (const sub of chunk.subs) {
        //     if (this.resolveSub(chunk, sub)) remaining.push(sub);
        //   }
        //   chunk.subs = remaining;
        //   // now place the chunk
        //   this.placeChunk(chunk); // TODO ...
        //   // update the graph; don't bother deleting form blocked.
        //   for (const revDep of revDeps[i]) {
        //     const fwd = fwdDeps[revDep];
        //     fwd.delete(i);
        //     if (!fwd.size) insert(unblocked, revDep);
        //   }
        // }
        // while (unblocked.length || blocked.length) {
        //   let next = unblocked.shift();
        //   if (next) {
        //     place(next);
        //     continue;
        //   }
        //   next = blocked[0];
        //   for (const rev of revDeps[next]) {
        //     if (this.chunks[rev].org != null) { // already placed
        //       blocked.shift();
        //       continue;
        //     }
        //     place(rev);
        //   }
        // }
        // At this point, everything should be placed, so do one last resolve.
        const patch = new SparseByteArray();
        // Before placing the data, add the fill bytes to segments with fill
        for (const [_name, seg] of this.segments) {
            if (seg.fill) {
                const buf = new Uint8Array(new ArrayBuffer(seg.size));
                buf.fill(seg.fill);
                patch.set(seg.offset, buf);
            }
        }
        for (const c of this.chunks) {
            for (const a of c.asserts) {
                const v = this.resolveExpr(a);
                if (v)
                    continue;
                const at = Tokens.at(a);
                throw new Error(`Assertion failed${at}`);
            }
            if (c.overlaps)
                continue;
            patch.set(c.offset, Uint8Array.from(this.data.slice(c.offset, c.offset + c.size)));
        }
        if (DEBUG)
            console.log(this.report(true));
        return patch;
    }
    placeChunk(chunk) {
        if (chunk.org != null)
            return; // don't re-place.
        // if this chunk doesn't have a predefined segment, and there is a default segment defined, then use that one
        if (chunk.segments.length == 0) {
            this.rawSegments.forEach((segments, name) => {
                for (const seg of segments) {
                    if (seg.default) {
                        chunk.segments = [name];
                        break;
                    }
                }
            });
        }
        const size = chunk.size;
        // Hueristic, don't search for duplicates for large chunk sizes
        if (chunk.size < 256 && !chunk.subs.size && !chunk.selfSubs.size) {
            // chunk is resolved: search for an existing copy of it first
            const pattern = this.data.pattern(chunk.data);
            for (const name of chunk.segments) {
                const segment = this.segments.get(name) ?? fail(`Segment not found with name: ${name}`);
                const start = segment.offset;
                const end = start + segment.size;
                const index = pattern.search(start, end);
                if (index < 0)
                    continue;
                chunk.place(index - segment.delta, segment);
                chunk.overlaps = true;
                return;
            }
        }
        // either unresolved, or didn't find a match; just allocate space.
        // look for the smallest possible free block.
        for (const name of chunk.segments) {
            const segment = this.segments.get(name) ?? fail(`Segment not found with name: ${name}`);
            const s0 = segment.offset;
            const s1 = s0 + segment.size;
            let found;
            let smallest = Infinity;
            for (const [f0, f1] of this.free.tail(s0)) {
                if (f0 >= s1)
                    break;
                const df = Math.min(f1, s1) - f0;
                if (df < size)
                    continue;
                if (df < smallest) {
                    found = f0;
                    smallest = df;
                }
            }
            if (found != null) {
                // found a region
                chunk.place(found - segment.delta, segment);
                // this.free.delete(f0, f0 + size);
                // TODO - factor out the subs-aware copy method!
                return;
            }
        }
        if (DEBUG)
            console.log(`Initial:\n${this.initialReport}`);
        console.log(`After filling:\n${this.report(true)}`);
        const name = chunk.name ? `${chunk.name} ` : '';
        console.log(this.segments.get(chunk.segments[0]));
        throw new Error(`Could not find space for ${size}-byte chunk ${name} in ${chunk.segments.join(', ')}`);
    }
    resolveSymbols(expr) {
        // pre-traverse so that transitive imports work
        return Exprs.traverse(expr, (e, rec) => {
            while (e.op === 'im' || e.op === 'sym') {
                if (e.op === 'im') {
                    const name = e.sym;
                    const imported = this.exports.get(name);
                    if (imported == null) {
                        const at = Tokens.at(expr);
                        throw new Error(`Symbol never exported ${name}${at}`);
                    }
                    e = this.symbols[imported].expr;
                }
                else {
                    if (e.num == null)
                        throw new Error(`Symbol not global`);
                    e = this.symbols[e.num].expr;
                }
            }
            return Exprs.evaluate(rec(e));
        });
    }
    // resolveBankBytes(expr: Expr): Expr {
    //   return Exprs.traverse(expr, (e: Expr) => {
    //     if (e.op !== '^' || e.args?.length !== 1) return e;
    //     const child = e.args[0];
    //     if (child.op !== 'off') return e;
    //     const chunk = this.chunks[child.num!];
    //     const banks = new Set<number>();
    //     for (const s of chunk.segments) {
    //       const segment = this.segments.get(s);
    //       if (segment?.bank != null) banks.add(segment.bank);
    //     }
    //     if (banks.size !== 1) return e;
    //     const [b] = banks;
    //     return {op: 'num', size: 1, num: b};
    //   });
    // }
    //     if (expr.op === 'import') {
    //       if (!expr.sym) throw new Error(`Import with no symbol.`);
    //       const sym = this.symbols[this.exports.get(expr.sym)];
    //       return this.resolveImports(sym.expr);
    //     }
    //     // TODO - this is nonsense...
    //     const args = [];
    //     let mut = false;
    //     for (let i = 0; i < expr.args; i++) {
    //       const child = expr.args[i];
    //       const resolved = this.resolveImports(child);
    //       args.push(resolved);
    //       if (child !== resolved) expr.args[i] = resolved;
    //       return 
    //     }
    //   }
    //   // TODO - add all the things
    //   return patch;
    // }
    addRawSegment(segment) {
        let list = this.rawSegments.get(segment.name);
        if (!list)
            this.rawSegments.set(segment.name, list = []);
        list.push(segment);
    }
    buildExports() {
        const map = new Map();
        for (const symbol of this.symbols) {
            if (!symbol.export)
                continue;
            const e = Exprs.traverse(symbol.expr, (e, rec) => {
                return this.resolveLink(Exprs.evaluate(rec(e)));
            });
            if (e.op !== 'num')
                throw new Error(`never resolved: ${symbol.export}`);
            const value = e.num;
            const out = { value };
            if (e.meta?.offset != null && e.meta.org != null) {
                out.offset = e.meta.offset + value - e.meta.org;
            }
            if (e.meta?.bank != null)
                out.bank = e.meta.bank;
            map.set(symbol.export, out);
        }
        return map;
    }
    report(verbose = false) {
        // TODO - accept a segment to filter?
        let out = '';
        for (const [s, e] of this.free) {
            out += `Free: ${s.toString(16)}..${e.toString(16)}: ${e - s} bytes\n`;
        }
        if (verbose) {
            for (const [s, c] of this.placed) {
                const name = c.name ?? `Chunk ${c.index}`;
                const end = c.offset + c.size;
                out += `${s.toString(16).padStart(5, '0')} .. ${end.toString(16).padStart(5, '0')}: ${name} (${end - s} bytes)\n`;
            }
        }
        return out;
    }
}
const DEBUG = false;
const NO_THROW = false; // for overwrite
const QUIET = false; // temporary for overwrite
