import base64 from './deps/deno.land/x/b64@1.1.27/src/base64.js';
import { Cpu } from './cpu.js';
import * as Exprs from './expr.js';
import * as mod from './module.js';
import * as Tokens from './token.js';
import { Tokenizer } from './tokenizer.js';
import { IntervalSet, assertNever } from './util.js';
export class Symbol {
    constructor() {
        /**
         * Index into the global symbol array.  Only applies to immutable
         * symbols that need to be accessible at link time.  Mutable symbols
         * and symbols with known values at use time are not added to the
         * global list and are therefore have no id.  Mutability is tracked
         * by storing a -1 here.
         */
        Object.defineProperty(this, "id", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        /** Whether the symbol has been explicitly scoped. */
        Object.defineProperty(this, "scoped", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        /**
         * The expression for the symbol.  Must be a statically-evaluatable constant
         * for mutable symbols.  Undefined for forward-referenced symbols.
         */
        Object.defineProperty(this, "expr", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        /** Name this symbol is exported as. */
        Object.defineProperty(this, "export", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        /** Token where this symbol was ref'd. */
        Object.defineProperty(this, "ref", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        }); // TODO - plumb this through
    }
}
class BaseScope {
    constructor() {
        //closed = false;
        Object.defineProperty(this, "symbols", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Map()
        });
    }
    pickScope(name) {
        return [name, this];
    }
    resolve(name, opts = {}) {
        const { allowForwardRef = false, ref } = opts;
        const [tail, scope] = this.pickScope(name);
        const sym = scope.symbols.get(tail);
        //console.log('resolve:',name,'sym=',sym,'fwd?',allowForwardRef);
        if (sym) {
            if (tail !== name)
                sym.scoped = true;
            return sym;
        }
        if (!allowForwardRef)
            return undefined;
        // if (scope.closed) throw new Error(`Could not resolve symbol: ${name}`);
        // make a new symbol - but only in an open scope
        //const symbol = {id: this.symbolArray.length};
        //console.log('created:',symbol);
        //this.symbolArray.push(symbol);
        const symbol = { ref };
        scope.symbols.set(tail, symbol);
        if (tail !== name)
            symbol.scoped = true;
        return symbol;
    }
}
class Scope extends BaseScope {
    constructor(parent, kind) {
        super();
        Object.defineProperty(this, "parent", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: parent
        });
        Object.defineProperty(this, "kind", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: kind
        });
        Object.defineProperty(this, "global", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "children", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Map()
        });
        Object.defineProperty(this, "anonymousChildren", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        this.global = parent ? parent.global : this;
    }
    pickScope(name) {
        // TODO - plumb the source information through here?
        // deno-lint-ignore no-this-alias
        let scope = this;
        const split = name.split(/::/g);
        const tail = split.pop();
        for (let i = 0; i < split.length; i++) {
            if (!i && !split[i]) { // global
                scope = scope.global;
                continue;
            }
            let child = scope.children.get(split[i]);
            while (!i && scope.parent && !child) {
                child = (scope = scope.parent).children.get(split[i]);
            }
            // If the name has an explicit scope, this is an error?
            if (!child) {
                const scopeName = split.slice(0, i + 1).join('::');
                throw new Error(`Could not resolve scope ${scopeName}`);
            }
            scope = child;
        }
        return [tail, scope];
    }
}
class CheapScope extends BaseScope {
    /** Clear everything out, making sure everything was defined. */
    clear() {
        for (const [name, sym] of this.symbols) {
            if (!sym.expr) {
                const at = sym.ref ? Tokens.at(sym.ref) : '';
                throw new Error(`Cheap local label never defined: ${name}${at}`);
            }
        }
        this.symbols.clear();
    }
}
export class Assembler {
    constructor(cpu = Cpu.P02, opts = {}) {
        Object.defineProperty(this, "cpu", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: cpu
        });
        Object.defineProperty(this, "opts", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: opts
        });
        /** The currently-open segment(s). */
        Object.defineProperty(this, "segments", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** Data on all the segments. */
        Object.defineProperty(this, "segmentData", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Map()
        });
        /** Stack of segments for .pushseg/.popseg. */
        Object.defineProperty(this, "segmentStack", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** All symbols in this object. */
        Object.defineProperty(this, "symbols", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** Global symbols. */
        // NOTE: we could add 'force-import', 'detect', or others...
        Object.defineProperty(this, "globals", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Map()
        });
        /** The current scope. */
        Object.defineProperty(this, "currentScope", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new Scope()
        });
        /** A scope for cheap local labels. */
        Object.defineProperty(this, "cheapLocals", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new CheapScope()
        });
        /** List of global symbol indices used by forward refs to anonymous labels. */
        Object.defineProperty(this, "anonymousForward", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** List of chunk/offset positions of previous anonymous labels. */
        Object.defineProperty(this, "anonymousReverse", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** Map of global symbol incides used by forward refs to relative labels. */
        Object.defineProperty(this, "relativeForward", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** Map of chunk/offset positions of back-referable relative labels. */
        Object.defineProperty(this, "relativeReverse", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** List of global symbol indices used by forward refs to rts statements. */
        Object.defineProperty(this, "rtsRefsForward", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** List of chunk/offset positions of back-referable rts statements. */
        Object.defineProperty(this, "rtsRefsReverse", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** All the chunks so far. */
        Object.defineProperty(this, "chunks", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        /** Set of offsets definitely written/freed so far. */
        Object.defineProperty(this, "written", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: new IntervalSet()
        });
        /** Currently active chunk */
        Object.defineProperty(this, "_chunk", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: undefined
        });
        /** Name of the next chunk */
        Object.defineProperty(this, "_name", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: undefined
        });
        /** Origin of the currnet chunk, if fixed. */
        Object.defineProperty(this, "_org", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: undefined
        });
        /** Prefix to prepend to all segment names. */
        Object.defineProperty(this, "_segmentPrefix", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: ''
        });
        /** Current source location, for error messages. */
        Object.defineProperty(this, "_source", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        /** Token for reporting errors. */
        Object.defineProperty(this, "errorToken", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        /** Supports refExtractor. */
        Object.defineProperty(this, "_exprMap", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: undefined
        });
        /**
         * When defining segments, this tracks the current offset in the output file
         * That way users don't have to define segment offsets if they are sequential
         */
        Object.defineProperty(this, "_segmentOffset", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: 0
        });
    }
    get chunk() {
        // make chunk only when needed
        this.ensureChunk();
        return this._chunk;
    }
    get exprMap() {
        return this._exprMap || (this._exprMap = new WeakMap());
    }
    get overwriteMode() {
        return this.opts.overwriteMode || 'allow';
    }
    ensureChunk() {
        if (!this._chunk) {
            // NOTE: multiple segments OK if disjoint memory...
            // if (this._org != null && this.segments.length !== 1) {
            //   this.fail(`.org chunks must be single-segment`);
            // }
            this._chunk = { segments: this.segments, data: [] };
            if (this._org != null)
                this._chunk.org = this._org;
            if (this._name)
                this._chunk.name = this._name;
            this.chunks.push(this._chunk);
            this._chunk.overwrite = this.overwriteMode;
        }
    }
    definedSymbol(sym) {
        // In this case, it's okay to traverse up the scope chain since if we
        // were to reference the symbol, it's guaranteed to be defined somehow.
        if (this.globals.get(sym) === 'import')
            return true;
        let scope = this.currentScope;
        const unscoped = !sym.includes('::');
        do {
            const s = scope.resolve(sym, { allowForwardRef: false });
            if (s)
                return Boolean(s.expr);
        } while (unscoped && (scope = scope.parent));
        return false;
    }
    constantSymbol(sym) {
        // If there's a symbol in a different scope, it's not actually constant.
        const s = this.currentScope.resolve(sym, { allowForwardRef: false });
        return Boolean(s && s.expr && !(s.id < 0));
    }
    referencedSymbol(sym) {
        // If not referenced in this scope, we don't know which it is...
        // NOTE: this is different from ca65.
        const s = this.currentScope.resolve(sym, { allowForwardRef: false });
        return s != null; // NOTE: this counts definitions.
    }
    evaluate(expr) {
        expr = this.resolve(expr);
        if (expr.op === 'num' && !expr.meta?.rel)
            return expr.num;
        return undefined;
    }
    // private get pc(): number|undefined {
    //   if (this._org == null) return undefined;
    //   return this._org + this.offset;
    // }
    pc() {
        const num = this.chunk.data.length; // NOTE: before counting chunks
        const meta = { rel: true, chunk: this.chunks.length - 1 };
        if (this._chunk?.org != null)
            meta.org = this._chunk.org;
        return Exprs.evaluate({ op: 'num', num, meta });
    }
    // Returns an expr resolving to a symbol name (e.g. a label)
    symbol(name) {
        return Exprs.evaluate(Exprs.parseOnly([{ token: 'ident', str: name }], 0, this.currentScope.symbols));
    }
    where() {
        if (!this._chunk)
            return '';
        if (this.chunk.org == null)
            return '';
        return `${this.chunk.segments.join(',')}:$${(this.chunk.org + this.chunk.data.length).toString(16)}`;
    }
    resolve(expr) {
        const out = Exprs.traverse(expr, (e, rec) => {
            while (e.op === 'sym' && e.sym) {
                e = this.resolveSymbol(e);
            }
            return Exprs.evaluate(rec(e));
        });
        if (this.opts.refExtractor?.ref && out !== expr) {
            const orig = this.exprMap.get(expr) || expr;
            this.exprMap.set(out, orig);
        }
        return out;
    }
    resolveSymbol(symbol) {
        const name = symbol.sym;
        const parsed = parseSymbol(name);
        if (parsed.type === 'pc') {
            return this.pc();
        }
        else if (parsed.type === 'anon' && parsed.num > 0) {
            // anonymous forward ref
            const i = parsed.num - 1;
            let num = this.anonymousForward[i];
            if (num != null)
                return { op: 'sym', num };
            this.anonymousForward[i] = num = this.symbols.length;
            this.symbols.push({ id: num });
            return { op: 'sym', num };
        }
        else if (parsed.type === 'rts' && parsed.num > 0) {
            // rts forward ref
            const i = parsed.num - 1;
            let num = this.rtsRefsForward[i];
            if (num != null)
                return { op: 'sym', num };
            this.rtsRefsForward[i] = num = this.symbols.length;
            this.symbols.push({ id: num });
            return { op: 'sym', num };
        }
        else if (parsed.type === 'rel' && parsed.num > 0) {
            // relative forward ref
            let num = this.relativeForward[parsed.num - 1];
            if (num != null)
                return { op: 'sym', num };
            this.relativeForward[name.length - 1] = num = this.symbols.length;
            this.symbols.push({ id: num });
            return { op: 'sym', num };
        }
        else if (parsed.type === 'anon' && parsed.num < 0) {
            // anonymous back ref
            const i = this.anonymousReverse.length + parsed.num;
            if (i < 0)
                this.fail(`Bad anonymous backref: ${name}`);
            return this.anonymousReverse[i];
        }
        else if (parsed.type === 'rts' && parsed.num < 0) {
            // rts back ref
            const i = this.rtsRefsReverse.length + parsed.num;
            if (i < 0)
                this.fail(`Bad rts backref: ${name}`);
            return this.rtsRefsReverse[i];
        }
        else if (parsed.type === 'rel' && parsed.num < 0) {
            // relative back ref
            const expr = this.relativeReverse[name.length - 1];
            if (expr == null)
                this.fail(`Bad relative backref: ${name}`);
            return expr;
        }
        const scope = name.startsWith('@') ? this.cheapLocals : this.currentScope;
        const sym = scope.resolve(name, { allowForwardRef: true, ref: symbol });
        if (sym.expr)
            return sym.expr;
        // if the expression is not yet known then refer to the symbol table,
        // adding it if necessary.
        if (sym.id == null) {
            sym.id = this.symbols.length;
            this.symbols.push(sym);
        }
        return { op: 'sym', num: sym.id };
    }
    // No banks are resolved yet.
    chunkData(chunk) {
        // TODO - handle zp segments?
        return { org: this.chunks[chunk].org };
    }
    closeScopes() {
        this.cheapLocals.clear();
        // Need to find any undeclared symbols in nested scopes and link
        // them to a parent scope symbol if possible.
        function close(scope) {
            for (const child of scope.children.values()) {
                close(child);
            }
            for (const child of scope.anonymousChildren) {
                close(child);
            }
            for (const [name, sym] of scope.symbols) {
                if (sym.expr || sym.id == null)
                    continue;
                if (scope.parent) {
                    // TODO - record where it was referenced?
                    if (sym.scoped)
                        throw new Error(`Symbol '${name}' undefined: ${JSON.stringify(sym)}`);
                    const parentSym = scope.parent.symbols.get(name);
                    if (!parentSym) {
                        // just alias it directly in the parent scope
                        scope.parent.symbols.set(name, sym);
                    }
                    else if (parentSym.id != null) {
                        sym.expr = { op: 'sym', num: parentSym.id };
                    }
                    else if (parentSym.expr) {
                        sym.expr = parentSym.expr;
                    }
                    else {
                        // must have either id or expr...?
                        throw new Error(`Impossible: ${name}`);
                    }
                }
                // handle global scope separately...
            }
        }
        // test case: ref a name in two child scopes, define it in grandparent
        if (this.currentScope.parent) {
            // TODO - record where it was opened?
            throw new Error(`Scope never closed`);
        }
        close(this.currentScope);
        for (const [name, global] of this.globals) {
            const sym = this.currentScope.symbols.get(name);
            if (global === 'export') {
                if (!sym?.expr)
                    throw new Error(`Symbol '${name}' undefined`);
                if (sym.id == null) {
                    sym.id = this.symbols.length;
                    this.symbols.push(sym);
                }
                sym.export = name;
            }
            else if (global === 'import') {
                if (!sym)
                    continue; // okay to import but not use.
                // TODO - record both positions?
                if (sym.expr)
                    throw new Error(`Already defined: ${name}`);
                sym.expr = { op: 'im', sym: name };
            }
            else {
                assertNever(global);
            }
        }
        for (const [name, sym] of this.currentScope.symbols) {
            if (!sym.expr)
                throw new Error(`Symbol '${name}' undefined: ${JSON.stringify(sym)}`);
        }
    }
    module() {
        this.closeScopes();
        // TODO - handle imports and exports out of the scope
        // TODO - add .scope and .endscope and forward scope vars at end to parent
        // Process and write the data
        const chunks = [];
        for (const chunk of this.chunks) {
            chunks.push({ ...chunk, data: Uint8Array.from(chunk.data) });
        }
        const symbols = [];
        for (const symbol of this.symbols) {
            if (symbol.expr == null)
                throw new Error(`Symbol undefined`);
            const out = { expr: symbol.expr };
            if (symbol.export != null)
                out.export = symbol.export;
            symbols.push(out);
        }
        const segments = [...this.segmentData.values()];
        return { chunks, symbols, segments };
    }
    // Assemble from a list of tokens
    async line(tokens) {
        this._source = tokens[0].source;
        if (tokens.length < 3 && Tokens.eq(tokens[tokens.length - 1], Tokens.COLON)) {
            this.label(tokens[0]);
        }
        else if (Tokens.eq(tokens[1], Tokens.ASSIGN)) {
            this.assign(Tokens.str(tokens[0]), this.parseExpr(tokens, 2));
        }
        else if (Tokens.eq(tokens[1], Tokens.SET)) {
            this.set(Tokens.str(tokens[0]), this.parseExpr(tokens, 2));
        }
        else if (tokens[0].token === 'cs') {
            this.directive(tokens);
        }
        else {
            await this.instruction(tokens);
        }
    }
    // Assemble from a token source
    async tokens(source) {
        let line;
        while ((line = await source.next())) {
            // console.log(`running line:`);
            // console.log(line);
            await this.line(line);
            // console.log(`checking output:`);
            // console.log(this.currentScope.global.symbols);
            // console.log(`\n\n`);
        }
    }
    // Assemble from an async token source
    // async tokensAsync(source: Tokens.Async): Promise<void> {
    //   let line;
    //   while ((line = await source.nextAsync())) {
    //     this.line(line);
    //   }
    // }
    directive(tokens) {
        // TODO - record line information, rewrap error messages?
        this.errorToken = tokens[0];
        try {
            switch (Tokens.str(tokens[0])) {
                case '.org': return this.org(this.parseConst(tokens, 1));
                case '.reloc': return this.parseNoArgs(tokens, 1), this.reloc();
                case '.assert': return this.assert(...this.parseAssert(tokens));
                case '.segment': return this.segment(...this.parseSegmentList(tokens, 1, false));
                case '.byt':
                case '.byte': return this.byte(...this.parseDataList(tokens, true));
                case '.bytestr': return this.byteInternal(this.parseByteStr(tokens));
                case '.res': return this.res(...this.parseResArgs(tokens));
                case '.word': return this.word(...this.parseDataList(tokens));
                case '.free': return this.free(this.parseConst(tokens, 1));
                case '.segmentprefix': return this.segmentPrefix(this.parseStr(tokens, 1));
                case '.import': return this.import(...this.parseIdentifierList(tokens));
                case '.export': return this.export(...this.parseIdentifierList(tokens));
                case '.scope': return this.scope(this.parseOptionalIdentifier(tokens));
                case '.endscope': return this.parseNoArgs(tokens, 1), this.endScope();
                case '.proc': return this.proc(this.parseRequiredIdentifier(tokens));
                case '.endproc': return this.parseNoArgs(tokens, 1), this.endProc();
                case '.pushseg': return this.pushSeg(...this.parseSegmentList(tokens, 1, true));
                case '.popseg': return this.parseNoArgs(tokens, 1), this.popSeg();
                case '.move': return this.move(...this.parseMoveArgs(tokens));
                case '.out': return this.log('info', tokens);
                case '.warning': return this.log('warn', tokens);
                case '.error': return this.log('error', tokens);
            }
            this.fail(`Unknown directive: ${Tokens.nameAt(tokens[0])}`);
        }
        finally {
            this.errorToken = undefined;
        }
    }
    label(label) {
        let ident;
        let token;
        const expr = this.pc();
        if (typeof label === 'string') {
            ident = label;
        }
        else {
            ident = Tokens.str(token = label);
            if (label.source)
                expr.source = label.source;
        }
        if (ident === ':') {
            // anonymous label - shift any forward refs off, and push onto the backs.
            this.anonymousReverse.push(expr);
            const sym = this.anonymousForward.shift();
            if (sym != null)
                this.symbols[sym].expr = expr;
            return;
        }
        else if (/^\++$/.test(ident)) {
            // relative forward ref - fill in global symbol we made earlier
            const sym = this.relativeForward[ident.length - 1];
            delete this.relativeForward[ident.length - 1];
            if (sym != null)
                this.symbols[sym].expr = expr;
            return;
        }
        else if (/^-+$/.test(ident)) {
            // relative backref - store the expr for later
            this.relativeReverse[ident.length - 1] = expr;
            return;
        }
        if (!ident.startsWith('@')) {
            this.cheapLocals.clear();
            if (!this.chunk.name && !this.chunk.data.length)
                this.chunk.name = ident;
            if (this.opts.refExtractor?.label && this.chunk.org != null) {
                this.opts.refExtractor.label(ident, this.chunk.org + this.chunk.data.length, this.chunk.segments);
            }
        }
        this.assignSymbol(ident, false, expr, token);
        // const symbol = this.scope.resolve(str, true);
        // if (symbol.expr) throw new Error(`Already defined: ${label}`);
        // if (!this.chunk) throw new Error(`Impossible?`);
        // const chunkId = this.chunks.length - 1; // must be AFTER this.chunk
        // symbol.expr = {op: 'off', num: this.offset, chunk: chunkId};
        // if (source) symbol.expr.source = source;
        // // Add the label to the current chunk...?
        // // Record the definition, etc...?
    }
    assign(ident, expr) {
        if (ident.startsWith('@')) {
            this.fail(`Cheap locals may only be labels: ${ident}`);
        }
        // Now make the assignment.
        if (typeof expr !== 'number')
            expr = this.resolve(expr);
        this.assignSymbol(ident, false, expr);
        // TODO - no longer needed?
        if (this.opts.refExtractor?.assign && typeof expr === 'number') {
            this.opts.refExtractor.assign(ident, expr);
        }
    }
    set(ident, expr) {
        if (ident.startsWith('@')) {
            this.fail(`Cheap locals may only be labels: ${ident}`);
        }
        // Now make the assignment.
        if (typeof expr !== 'number')
            expr = this.resolve(expr);
        this.assignSymbol(ident, true, expr);
    }
    assignSymbol(ident, mut, expr, token) {
        // NOTE: * _will_ get current chunk!
        if (typeof expr === 'number')
            expr = { op: 'num', num: expr };
        const scope = ident.startsWith('@') ? this.cheapLocals : this.currentScope;
        // NOTE: This is incorrect - it will look up the scope chain when it
        // shouldn't.  Mutables may or may not want this, immutables must not.
        // Whether this is tied to allowFwdRef or not is unclear.  It's also
        // unclear whether we want to allow defining symbols in outside scopes:
        //   ::foo = 43
        // FWIW, ca65 _does_ allow this, as well as foo::bar = 42 after the scope.
        let sym = scope.resolve(ident, { allowForwardRef: !mut, ref: token });
        if (sym && (mut !== (sym.id < 0))) {
            this.fail(`Cannot change mutability of ${ident}`, token);
        }
        else if (mut && expr.op != 'num') {
            this.fail(`Mutable set requires constant`, token);
        }
        else if (!sym) {
            if (!mut)
                throw new Error(`impossible`);
            scope.symbols.set(ident, sym = { id: -1 });
        }
        else if (!mut && sym.expr) {
            const orig = sym.expr.source ? `\nOriginally defined${Tokens.at(sym.expr)}` : '';
            const name = token ? Tokens.nameAt(token) :
                ident + (this._source ? Tokens.at({ source: this._source }) : '');
            throw new Error(`Redefining symbol ${name}${orig}`);
        }
        sym.expr = expr;
    }
    async instruction(...args) {
        let mnemonic;
        let arg;
        if (args.length === 1 && Array.isArray(args[0])) {
            // handle the line...
            const tokens = args[0];
            mnemonic = Tokens.expectIdentifier(tokens[0]).toLowerCase();
            arg = this.parseArg(tokens, 1);
        }
        else if (typeof args[1] === 'string') {
            // parse the tokens first
            mnemonic = args[0];
            const tokenizer = new Tokenizer(args[1]);
            arg = this.parseArg((await tokenizer.next()), 0);
        }
        else {
            [mnemonic, arg] = args;
            if (!arg)
                arg = ['imp'];
            mnemonic = mnemonic.toLowerCase();
        }
        if (mnemonic === 'rts') {
            // NOTE: we special-case this in both the tokenizer and here so that
            // `rts:+` and `rts:-` work for pointing to an rts instruction.
            const expr = this.pc();
            this.rtsRefsReverse.push(expr);
            const sym = this.rtsRefsForward.shift();
            if (sym != null)
                this.symbols[sym].expr = expr;
        }
        // may need to size the arg, depending.
        // cpu will take 'add', 'a,x', and 'a,y' and indicate which it actually is.
        const ops = this.cpu.op(mnemonic); // will throw if mnemonic unknown
        const m = arg[0];
        if (m === 'add' || m === 'a,x' || m === 'a,y') {
            // Special case for address mnemonics
            const expr = arg[1];
            const s = expr.meta?.size || 2;
            if (m === 'add' && s === 1 && 'zpg' in ops) {
                return this.opcode(ops.zpg, 1, expr);
            }
            else if (m === 'add' && 'abs' in ops) {
                return this.opcode(ops.abs, 2, expr);
            }
            else if (m === 'add' && 'rel' in ops) {
                return this.relative(ops.rel, 1, expr);
            }
            else if (m === 'a,x' && s === 1 && 'zpx' in ops) {
                return this.opcode(ops.zpx, 1, expr);
            }
            else if (m === 'a,x' && 'abx' in ops) {
                return this.opcode(ops.abx, 2, expr);
            }
            else if (m === 'a,y' && s === 1 && 'zpy' in ops) {
                return this.opcode(ops.zpy, 1, expr);
            }
            else if (m === 'a,y' && 'aby' in ops) {
                return this.opcode(ops.aby, 2, expr);
            }
            this.fail(`Bad address mode ${m} for ${mnemonic}`);
        }
        // All other mnemonics
        if (m in ops) {
            const argLen = this.cpu.argLen(m);
            if (m === 'rel')
                return this.relative(ops[m], argLen, arg[1]);
            return this.opcode(ops[m], argLen, arg[1]);
        }
        this.fail(`Bad address mode ${m} for ${mnemonic}`);
    }
    parseArg(tokens, start) {
        // Look for parens/brackets and/or a comma
        if (tokens.length === start)
            return ['imp'];
        const front = tokens[start];
        const next = tokens[start + 1];
        if (tokens.length === start + 1) {
            if (Tokens.isRegister(front, 'a'))
                return ['acc'];
        }
        else if (Tokens.eq(front, Tokens.IMMEDIATE)) {
            return ['imm', this.parseExpr(tokens, start + 1)];
        }
        // Look for relative or anonymous labels, which are not valid on their own
        if (Tokens.eq(front, Tokens.COLON) && tokens.length === start + 2 &&
            next.token === 'op' && /^[-+]+$/.test(next.str)) {
            // anonymous label
            return ['add', { op: 'sym', sym: ':' + next.str }];
        }
        else if (tokens.length === start + 1 && front.token === 'op' &&
            /^[-+]+$/.test(front.str)) {
            // relative label
            return ['add', { op: 'sym', sym: front.str }];
        }
        // check to see if there is a zp,abs,far operator forcing a new addressing mode type
        if (front.token == 'ident' && (front.str == 'a' || front.str == 'z') && Tokens.eq(next, Tokens.COLON)) {
            // Get the rest of the expression and force the addressing mode to the required one
            const [mode, out] = this.parseArg(tokens, start + 2);
            if (mode == 'acc' || mode == 'imm') {
                this.fail(`Cannot force direct or absolute addressing on acc or imm arguments`, front);
            }
            const lookup = (front.str == 'z') ? ForceDirectAddressingMap : ForceAbsoluteAddressingMap;
            const adr = lookup.get(mode);
            return [adr ? adr : mode, out];
        }
        // it must be an address of some sort - is it indirect?
        if (Tokens.eq(front, Tokens.LP) ||
            (this.opts.allowBrackets && Tokens.eq(front, Tokens.LB))) {
            const close = Tokens.findBalanced(tokens, start);
            if (close < 0)
                this.fail(`Unbalanced ${Tokens.name(front)}`, front);
            const args = Tokens.parseArgList(tokens, start + 1, close);
            if (!args.length)
                this.fail(`Bad argument`, front);
            const expr = this.parseExpr(args[0], 0);
            if (args.length === 1) {
                // either IND or INY
                if (Tokens.eq(tokens[close + 1], Tokens.COMMA) &&
                    Tokens.isRegister(tokens[close + 2], 'y')) {
                    Tokens.expectEol(tokens[close + 3]);
                    return ['iny', expr];
                }
                Tokens.expectEol(tokens[close + 1]);
                return ['ind', expr];
            }
            else if (args.length === 2 && args[1].length === 1) {
                // INX
                if (Tokens.isRegister(args[1][0], 'x'))
                    return ['inx', expr];
            }
            this.fail(`Bad argument`, front);
        }
        const args = Tokens.parseArgList(tokens, start);
        if (!args.length)
            this.fail(`Bad arg`, front);
        const expr = this.parseExpr(args[0], 0);
        if (args.length === 1)
            return ['add', expr];
        if (args.length === 2 && args[1].length === 1) {
            if (Tokens.isRegister(args[1][0], 'x'))
                return ['a,x', expr];
            if (Tokens.isRegister(args[1][0], 'y'))
                return ['a,y', expr];
        }
        this.fail(`Bad arg`, front);
    }
    relative(op, arglen, expr) {
        // Can arglen ever be 2? (yes - brl on 65816)
        // Basic plan here is that we actually want a relative expr.
        // TODO - clean this up to be more efficient.
        // TODO - handle local/anonymous labels separately?
        // TODO - check the range somehow?
        const num = this.chunk.data.length + arglen + 1;
        const meta = { rel: true, chunk: this.chunks.length - 1 };
        if (this._chunk?.org)
            meta.org = this._chunk.org;
        const nextPc = { op: 'num', num, meta };
        const rel = { op: '-', args: [expr, nextPc] };
        if (expr.source)
            rel.source = expr.source;
        this.opcode(op, arglen, rel);
    }
    opcode(op, arglen, expr) {
        // Emit some bytes.
        if (arglen)
            expr = this.resolve(expr); // BEFORE opcode (in case of *)
        const { chunk } = this;
        this.markWritten(1 + arglen);
        chunk.data.push(op);
        if (arglen) {
            this.append(expr, arglen);
        }
        if (!chunk.name)
            chunk.name = `Code`;
        // TODO - for relative, if we're in the same chunk, just compare
        // the offset...
    }
    markWritten(size) {
        if (this._chunk?.org == null)
            return;
        // NOTE: it's possible the chunk has spilled over into the next segment.
        // We just ignore this by asking for the offset of the _start_ of the
        // chunk, rather than the current position.  This is consistent with how
        // the linker works, but can lead to issues with free'd parts, etc.
        // Fortunately, the risk is relatively small because it's only relevant
        // for statically-placed chunks, and (one would hope) we know what we're
        // doing there.
        const offset = this.orgToOffset(this._chunk.org);
        if (offset != null) {
            this.written.add(offset + this._chunk.data.length, offset + this._chunk.data.length + size);
        }
    }
    append(expr, size) {
        const { chunk } = this;
        // Save the ref, as long as it's actually interesting.
        if (this.opts.refExtractor?.ref && chunk.org != null) {
            const orig = this._exprMap?.get(expr) || expr;
            if (Exprs.symbols(orig).length > 0) {
                this.opts.refExtractor.ref(orig, size, chunk.org + chunk.data.length, chunk.segments);
            }
        }
        // Append the number or placeholder
        expr = this.resolve(expr);
        const val = expr.num;
        if (expr.op !== 'num' || expr.meta?.rel) {
            // use a placeholder and add a substitution
            const offset = chunk.data.length;
            (chunk.subs || (chunk.subs = [])).push({ offset, size, expr });
            this.writeNumber(chunk.data, size); // write goes after subs
        }
        else {
            this.writeNumber(chunk.data, size, val);
        }
    }
    ////////////////////////////////////////////////////////////////
    // Directive handlers
    org(addr, name) {
        if (this._org != null && this._chunk != null &&
            this._org + this._chunk.data.length === addr) {
            return; // nothing to do?
        }
        this._org = addr;
        this._chunk = undefined;
        this._name = name;
    }
    reloc(name) {
        this._org = undefined;
        this._chunk = undefined;
        this._name = name;
    }
    segment(...segments) {
        // Usage: .segment "1a", "1b", ...
        this.segments = segments.map(s => typeof s === 'string' ? s : s.name);
        for (const s of segments) {
            if (typeof s === 'object') {
                const data = this.segmentData.get(s.name) || { name: s.name };
                this.segmentData.set(s.name, mod.Segment.merge(data, s));
            }
        }
        this._chunk = undefined;
        this._name = undefined;
    }
    assert(expr, _level, message) {
        expr = this.resolve(expr);
        const val = this.evaluate(expr);
        if (val != null) {
            if (!val) {
                let pc = '';
                const chunk = this.chunk;
                if (chunk.org != null) {
                    pc = ` (PC=$${(chunk.org + chunk.data.length).toString(16)})`;
                }
                this.fail(`${message}\nAssertion failed${pc}`, expr);
            }
        }
        else {
            const { chunk } = this;
            (chunk.asserts || (chunk.asserts = [])).push(expr);
        }
    }
    byte(...args) {
        this.byteInternal(args);
    }
    byteInternal(args) {
        const { chunk } = this;
        this.markWritten(args.length);
        for (const arg of args) {
            // TODO - if we ran off the end of the segment, make a new chunk???
            // For now, we're avoiding needing to worry about it because orgToOffset
            // and markWritten are based on the start of the chunk, rather than where
            // it ends; but this is still a potential source of bugs!
            if (typeof arg === 'number') {
                this.writeNumber(chunk.data, 1, arg);
            }
            else if (typeof arg === 'string') {
                writeString(chunk.data, arg);
            }
            else {
                this.append(arg, 1);
            }
        }
    }
    res(count, value) {
        if (!count)
            return;
        this.byte(...new Array(count).fill(value ?? 0));
    }
    word(...args) {
        const { chunk } = this;
        this.markWritten(2 * args.length);
        for (const arg of args) {
            if (typeof arg === 'number') {
                this.writeNumber(chunk.data, 2, arg);
            }
            else {
                this.append(arg, 2);
            }
        }
    }
    free(size) {
        // Must be in .org for a single segment.
        if (this._org == null)
            this.fail(`.free in .reloc mode`);
        this.markWritten(size);
        const segments = this.segments.length > 1 ? this.segments.filter(s => {
            const data = this.segmentData.get(s);
            if (!data || data.memory == null || data.size == null)
                return false;
            if (data.memory > this._org)
                return false;
            if (data.memory + data.size <= this._org)
                return false;
            return true;
        }) : this.segments;
        if (segments.length !== 1) {
            this.fail(`.free with non-unique segment: ${this.segments}`);
        }
        else if (size < 0) {
            this.fail(`.free with negative size: ${size}`);
        }
        // If we've got an open chunk, end it.
        if (this._chunk) {
            this._org += this._chunk.data.length;
        }
        this._chunk = undefined;
        // Ensure a segment object exists.
        const name = segments[0];
        let s = this.segmentData.get(name);
        if (!s)
            this.segmentData.set(name, s = { name });
        (s.free || (s.free = [])).push([this._org, this._org + size]);
        // Advance past the free space.
        this._org += size;
    }
    segmentPrefix(prefix) {
        // TODO - make more of a todo about changing this?
        this._segmentPrefix = prefix;
    }
    import(...idents) {
        for (const ident of idents) {
            this.globals.set(ident, 'import');
        }
    }
    export(...idents) {
        for (const ident of idents) {
            this.globals.set(ident, 'export');
        }
    }
    scope(name) {
        this.enterScope(name, 'scope');
    }
    proc(name) {
        this.label(name);
        this.enterScope(name, 'proc');
    }
    enterScope(name, kind) {
        const existing = name ? this.currentScope.children.get(name) : undefined;
        if (existing) {
            if (this.opts.reentrantScopes) {
                this.currentScope = existing;
                return;
            }
            this.fail(`Cannot re-enter scope ${name}`);
        }
        const child = new Scope(this.currentScope, kind);
        if (name) {
            this.currentScope.children.set(name, child);
        }
        else {
            this.currentScope.anonymousChildren.push(child);
        }
        this.currentScope = child;
    }
    endScope() { this.exitScope('scope'); }
    endProc() { this.exitScope('proc'); }
    exitScope(kind) {
        if (this.currentScope.kind !== kind || !this.currentScope.parent) {
            this.fail(`.end${kind} without .${kind}`);
        }
        this.currentScope = this.currentScope.parent;
    }
    pushSeg(...segments) {
        this.segmentStack.push([this.segments, this._chunk]);
        // If pushseg was called without any segments, just keep the current segment
        if (segments) {
            this.segment(...segments);
        }
    }
    popSeg() {
        if (!this.segmentStack.length)
            this.fail(`.popseg without .pushseg`);
        [this.segments, this._chunk] = this.segmentStack.pop();
        this._org = this._chunk?.org;
    }
    move(size, source) {
        this.append({ op: '.move', args: [source], meta: { size } }, size);
    }
    log(level, line) {
        // TODO properly handle logging
        const str = Tokens.expectString(line[1], line[0]);
        Tokens.expectEol(line[2], 'a single string');
        if (level === 'error')
            throw new Error(str);
        if (level === 'info')
            console.log(str);
        else
            console.warn(str);
    }
    // Utility methods for processing arguments
    parseConst(tokens, start) {
        const val = this.evaluate(this.parseExpr(tokens, start));
        if (val != null)
            return val;
        this.fail(`Expression is not constant`, tokens[1]);
    }
    parseNoArgs(tokens, _start) {
        Tokens.expectEol(tokens[1]);
    }
    parseExpr(tokens, start) {
        return Exprs.parseOnly(tokens, start, this.currentScope.symbols);
    }
    // parseStringList(tokens: Token[], start = 1): string[] {
    //   return Tokens.parseArgList(tokens, 1).map(ts => {
    //     const str = Tokens.expectString(ts[0]);
    //     Tokens.expectEol(ts[1], "a single string");
    //     return str;
    //   });
    // }
    parseStr(tokens, start) {
        const str = Tokens.expectString(tokens[start]);
        Tokens.expectEol(tokens[start + 1], "a single string");
        return str;
    }
    parseSegmentList(tokens, start, allowEmptySegmentList) {
        if (tokens.length < start + 1) {
            if (allowEmptySegmentList) {
                return [];
            }
            this.fail(`Expected a segment list`, tokens[start - 1]);
        }
        return Tokens.parseArgList(tokens, 1).map(ts => {
            const str = this._segmentPrefix + Tokens.expectString(ts[0]);
            if (ts.length === 1)
                return str;
            if (!Tokens.eq(ts[1], Tokens.COLON)) {
                this.fail(`Expected comma or colon: ${Tokens.name(ts[1])}`, ts[1]);
            }
            const seg = { name: str };
            // TODO - parse expressions...
            const attrs = Tokens.parseAttrList(ts, 1); // : ident [...]
            for (const [key, val] of attrs) {
                switch (key) {
                    case 'bank':
                        seg.bank = this.parseConst(val, 0);
                        break;
                    case 'size':
                        seg.size = this.parseConst(val, 0);
                        break;
                    case 'off':
                        seg.offset = this.parseConst(val, 0);
                        break;
                    case 'mem':
                        seg.memory = this.parseConst(val, 0);
                        break;
                    case 'fill':
                        seg.fill = this.parseConst(val, 0);
                        break;
                    case 'out':
                        seg.out = true;
                        break;
                    case 'overlay':
                        seg.overlay = this.parseStr(val, 0);
                        break;
                    // TODO allow setting free space
                    // case 'free': seg.free = this.parseConst(val, 0); break;
                    case 'zp':
                        seg.addressing = 1;
                        break;
                    default: this.fail(`Unknown segment attr: ${key}`);
                }
            }
            if (seg.offset === undefined && seg.size !== undefined) {
                seg.offset = this._segmentOffset;
                this._segmentOffset += seg.size;
            }
            if (seg.fill !== undefined && seg.size !== undefined) {
                seg.free = [[seg.memory ?? 0, (seg.memory ?? 0) + seg.size]];
            }
            return seg;
        });
    }
    parseResArgs(tokens) {
        const data = this.parseDataList(tokens);
        if (data.length > 2)
            this.fail(`Expected at most 2 args`, data[2]);
        if (!data.length)
            this.fail(`Expected at least 1 arg`);
        const count = this.evaluate(data[0]);
        if (count == null)
            this.fail(`Expected constant count`);
        const val = data[1] && this.evaluate(data[1]);
        if (data[1] && val == null)
            this.fail(`Expected constant value`);
        return [count, val];
    }
    parseDataList(tokens, allowString = false) {
        if (tokens.length < 2) {
            this.fail(`Expected a data list`, tokens[0]);
        }
        const out = [];
        for (const term of Tokens.parseArgList(tokens, 1)) {
            if (allowString && term.length === 1 && term[0].token === 'str') {
                out.push(term[0].str);
            }
            else if (term.length < 1) {
                this.fail(`Missing term`);
            }
            else {
                out.push(this.resolve(this.parseExpr(term, 0)));
            }
        }
        return out;
    }
    parseIdentifierList(tokens) {
        if (tokens.length < 2) {
            this.fail(`Expected identifier(s)`, tokens[0]);
        }
        const out = [];
        for (const term of Tokens.parseArgList(tokens, 1)) {
            if (term.length !== 1 || term[0].token !== 'ident') {
                this.fail(`Expected identifier: ${Tokens.name(term[0])}`, term[0]);
            }
            out.push(Tokens.str(term[0]));
        }
        return out;
    }
    parseOptionalIdentifier(tokens) {
        const tok = tokens[1];
        if (!tok)
            return undefined;
        const ident = Tokens.expectIdentifier(tok);
        Tokens.expectEol(tokens[2]);
        return ident;
    }
    parseRequiredIdentifier(tokens) {
        const ident = Tokens.expectIdentifier(tokens[1]);
        Tokens.expectEol(tokens[2]);
        return ident;
    }
    parseMoveArgs(tokens) {
        // .move 10, ident        ; must be an offset
        // .move 10, $1234, "seg" ; maybe support this?
        const args = Tokens.parseArgList(tokens, 1);
        if (args.length !== 2 /* && args.length !== 3 */) {
            this.fail(`Expected constant number, then identifier`);
        }
        const num = this.evaluate(this.parseExpr(args[0], 0));
        if (num == null)
            this.fail(`Expected a constant number`);
        // let segName = this.segments.length === 1 ? this.segments[0] : undefined;
        // if (args.length === 3) {
        //   if (args[2].length !== 1 || args[2][0].token !== 'str') {
        //     this.fail(`Expected a single segment name`, this.args[2][0]);
        //   }
        //   segName = args[2][0].str;
        // }
        // const seg = segName ? this.segmentData.get(segName) : undefined;
        const offset = this.resolve(this.parseExpr(args[1], 0));
        if (offset.op === 'num' && offset.meta?.chunk != null) {
            return [num, offset];
        }
        else {
            this.fail(`Expected a constant offset`, args[1][0]);
        }
    }
    parseByteStr(tokens) {
        const bytestr = Tokens.expectString(tokens[1]);
        Tokens.expectEol(tokens[2]);
        const buf = base64.toArrayBuffer(bytestr);
        return Array.from(new Uint8Array(buf));
    }
    parseAssert(tokens) {
        const args = Tokens.parseArgList(tokens, 1);
        if (!args[0]) {
            this.fail(`No assertion expression provided`);
        }
        const expr = this.parseExpr(args[0], 0);
        const level = Tokens.optionalIdentifier(args.at(1)?.at(0)) ?? 'error';
        const message = Tokens.optionalString(args.at(2)?.at(0)) ?? "Assertion failed";
        return [expr, level, message];
    }
    // Diagnostics
    fail(msg, at) {
        if (!at && this.errorToken)
            at = this.errorToken;
        if (at?.source)
            throw new Error(msg + Tokens.at(at));
        if (!this._source && this._chunk?.name) {
            throw new Error(msg + `\n  in ${this._chunk.name}`);
        }
        throw new Error(msg + Tokens.at({ source: this._source }));
    }
    writeNumber(data, size, val) {
        // TODO - if val is a signed/unsigned 32-bit number, it's not clear
        // whether we need to treat it one way or the other...?  but maybe
        // it doesn't matter since we're only looking at 32 bits anyway.
        const s = (size) << 3;
        if (val != null && (val < (-1 << s) || val >= (1 << s))) {
            const name = ['byte', 'word', 'farword', 'dword'][size - 1];
            this.fail(`Not a ${name}: $${val.toString(16)}`);
        }
        for (let i = 0; i < size; i++) {
            data.push(val != null ? val & 0xff : 0xff);
            if (val != null)
                val >>= 8;
        }
    }
    orgToOffset(org) {
        const segment = this.segmentData.get(this.segments.find(s => {
            const data = this.segmentData.get(s);
            return data && mod.Segment.includesOrg(data, org);
        }));
        return segment?.offset != null ?
            segment.offset + (org - segment.memory) : undefined;
    }
    isWritten(offset) {
        return this.written.has(offset);
    }
}
function writeString(data, str) {
    // TODO - support character maps (pass as third arg?)
    for (let i = 0; i < str.length; i++) {
        data.push(str.charCodeAt(i));
    }
}
function parseSymbol(name) {
    if (name === '*')
        return { type: 'pc' };
    if (/^:\++$/.test(name))
        return { type: 'anon', num: name.length - 1 };
    if (/^:\+\d+$/.test(name))
        return { type: 'anon', num: parseInt(name.substring(2)) };
    if (/^:-+$/.test(name))
        return { type: 'anon', num: 1 - name.length };
    if (/^:-\d+$/.test(name))
        return { type: 'anon', num: -parseInt(name.substring(2)) };
    if (/^:>*rts$/.test(name))
        return { type: 'rts', num: Math.max(name.length - 4, 1) };
    if (/^:<+rts$/.test(name))
        return { type: 'rts', num: 4 - name.length };
    if (/^\++$/.test(name))
        return { type: 'rel', num: name.length };
    if (/^-+$/.test(name))
        return { type: 'rel', num: -name.length };
    return { type: 'none' };
}
const ForceDirectAddressingMap = new Map([
    ['add', 'zpg'],
    ['a,x', 'zpx'],
    ['a,y', 'zpy'],
    ['abs', 'zpg'],
    ['abx', 'zpx'],
    ['aby', 'zpy'],
]);
const ForceAbsoluteAddressingMap = new Map([
    ['add', 'abs'],
    ['a,x', 'abx'],
    ['a,y', 'aby'],
    ['zpg', 'abs'],
    ['zpx', 'abx'],
    ['zpy', 'aby'],
]);
