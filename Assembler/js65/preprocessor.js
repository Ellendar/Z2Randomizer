import { Define } from './define.js';
import * as Exprs from './expr.js';
import { Macro } from './macro.js';
import * as Tokens from './token.js';
// TODO - figure out how to actually keep track of stack depth?
//  - might need to insert a special token at the end of an expansion
//    to know when to release the frame?
const MAX_STACK_DEPTH = 100;
// interface TokenSource {
//   next(): Token[];
//   include(file: string): Promise<void>;
//   unshift(...lines: Token[][]): void;
//   enter(): void;
//   exit(): void;
//   //options(): Tokenizer.Options;
// }
// Since the Env is most closely tied to the Assembler, we tie the
// unique ID generation to it as well, without adding additional
// constraints on the Assembler API.
const ID_MAP = new WeakMap();
function idGen(env) {
    let id = ID_MAP.get(env);
    if (!id)
        ID_MAP.set(env, id = (num => ({ next: () => num++ }))(0));
    return id;
}
// export abstract class Abstract implements Source {
//   // TODO - move pump() into here, refactor Preprocessor as a TokenSource
//   // TODO - rename Processor into Assembler, fix up the clunky methods
//   //      - add line(Token[]), tokens(TokenSource) and asyncTokens(ATS)
//   //        the latter returns Promise<void> and must be awaited.
//   // Delegate the 
//   abstract pump(): Generator<Token[]|undefined>;
// }
export class Preprocessor {
    // NOTE: there is no scope here... - not for macros
    //  - only symbols have scope
    // TODO - evaluate constants...
    constructor(stream, env, parent) {
        Object.defineProperty(this, "stream", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: stream
        });
        Object.defineProperty(this, "env", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: env
        });
        Object.defineProperty(this, "macros", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "sink", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        // builds up repeating tokens...
        Object.defineProperty(this, "repeats", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        Object.defineProperty(this, "runDirectives", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: {
                '.define': (line) => this.parseDefine(line),
                '.undefine': (line) => this.parseUndefine(line),
                '.else': ([cs]) => badClose('.if', cs),
                '.elseif': ([cs]) => badClose('.if', cs),
                '.endif': ([cs]) => badClose('.if', cs),
                '.endmac': ([cs]) => badClose('.macro', cs),
                '.endmacro': ([cs]) => badClose('.macro', cs),
                '.endrep': (line) => this.parseEndRepeat(line),
                '.endrepeat': (line) => this.parseEndRepeat(line),
                '.exitmacro': async ([, a]) => {
                    noGarbage(a);
                    this.stream.exit();
                    return await Promise.resolve();
                },
                '.if': ([cs, ...args]) => this.parseIf(!!this.evaluateConst(parseOneExpr(args, cs))),
                '.ifdef': ([cs, ...args]) => this.parseIf(this.parseIfDef(args, cs)),
                '.ifndef': ([cs, ...args]) => this.parseIf(!this.parseIfDef(args, cs)),
                '.ifblank': ([, ...args]) => this.parseIf(!args.length),
                '.ifnblank': ([, ...args]) => this.parseIf(!!args.length),
                '.ifref': ([cs, ...args]) => this.parseIf(this.env.referencedSymbol(parseOneIdent(args, cs))),
                '.ifnref': ([cs, ...args]) => this.parseIf(!this.env.referencedSymbol(parseOneIdent(args, cs))),
                '.ifsym': ([cs, ...args]) => this.parseIf(this.env.definedSymbol(parseOneIdent(args, cs))),
                '.ifnsym': ([cs, ...args]) => this.parseIf(!this.env.definedSymbol(parseOneIdent(args, cs))),
                '.ifconst': ([cs, ...args]) => this.parseIf(this.env.constantSymbol(parseOneIdent(args, cs))),
                '.ifnconst': ([cs, ...args]) => this.parseIf(!this.env.constantSymbol(parseOneIdent(args, cs))),
                '.macro': (line) => this.parseMacro(line),
                '.repeat': (line) => this.parseRepeat(line),
            }
        });
        this.macros = parent ? parent.macros : new Map();
    }
    async tokens() {
        const tokens = [];
        let tok;
        while ((tok = await this.next())) {
            tokens.push(tok);
        }
        return tokens;
    }
    async next() {
        while (true) {
            if (!this.sink)
                this.sink = await this.pump();
            const { value, done } = await this.sink.next();
            if (!done)
                return value;
            this.sink = undefined;
        }
    }
    // For use as a token source in the next stage.
    async *pump() {
        const line = await this.readLine();
        if (line == null)
            return void (yield line); // EOF
        while (line.length) {
            const front = line[0];
            switch (front.token) {
                case 'ident':
                    // Possibilities: (1) label, (2) instruction/assign, (3) macro
                    // Labels get split out.  We don't distinguish assigns yet.
                    if (Tokens.eq(line[1], Tokens.COLON)) {
                        yield line.splice(0, 2);
                        break;
                    }
                    if (!this.tryExpandMacro(line))
                        yield line;
                    return;
                case 'cs':
                    if (!(await this.tryRunDirective(line)))
                        yield line;
                    return;
                case 'op':
                    // Probably an anonymous label...
                    if (/^[-+]+$/.test(front.str)) {
                        const label = [front];
                        const second = line[1];
                        if (second && Tokens.eq(second, Tokens.COLON)) {
                            label.push(second);
                            line.splice(0, 2);
                        }
                        else {
                            label.push({ token: 'op', str: ':' });
                            line.splice(0, 1);
                        }
                        yield label;
                        break;
                    }
                    else if (front.str === ':') {
                        yield line.splice(0, 1);
                        break;
                    }
                /* fallthrough */
                default:
                    throw new Error(`Unexpected: ${Tokens.nameAt(line[0])}`);
            }
        }
    }
    // Expand a single line of tokens from the front of toks.
    async readLine() {
        // Apply .define expansions as necessary.
        const line = await this.stream.next();
        if (line == null)
            return line;
        return this.expandLine(line);
    }
    ////////////////////////////////////////////////////////////////
    // EXPANSION
    expandLine(line, pos = 0) {
        const front = line[0];
        let depth = 0;
        let maxPos = 0;
        while (pos < line.length) {
            if (pos > maxPos) {
                maxPos = pos;
                depth = 0;
            }
            else if (depth++ > MAX_STACK_DEPTH) {
                throw new Error(`Maximum expansion depth reached: ${line.map(Tokens.name).join(' ')}${Tokens.at(front)}`);
            }
            pos = this.expandToken(line, pos);
        }
        return line;
    }
    /** Returns the next position to expand. */
    expandToken(line, pos) {
        const front = line[pos];
        if (front.token === 'ident') {
            const define = this.macros.get(front.str);
            if (define instanceof Define) {
                const overflow = define.expand(line, pos);
                //console.log('post-expand', line);
                if (overflow) {
                    if (overflow.length)
                        this.stream.unshift(...overflow);
                    return pos;
                }
            }
        }
        else if (front.token === 'cs') {
            return this.expandDirective(front.str, line, pos);
        }
        return pos + 1;
    }
    tryExpandMacro(line) {
        const [first] = line;
        if (first.token !== 'ident')
            throw new Error(`impossible`);
        const macro = this.macros.get(first.str);
        if (!(macro instanceof Macro))
            return false;
        const expansion = macro.expand(line, idGen(this.env));
        this.stream.enter();
        this.stream.unshift(...expansion); // process them all over again...
        return true;
    }
    expandDirective(directive, line, i) {
        switch (directive) {
            case '.define':
            case '.ifdef':
            case '.ifndef':
            case '.undefine':
                return this.skipIdentifier(line, i);
            case '.skip': return this.skip(line, i);
            case '.noexpand': return this.noexpand(line, i);
            case '.tcount': return this.parseArgs(line, i, 1, this.tcount);
            case '.ident': return this.parseArgs(line, i, 1, this.ident);
            case '.string': return this.parseArgs(line, i, 1, this.string);
            case '.concat': return this.parseArgs(line, i, 0, this.concat);
            case '.sprintf': return this.parseArgs(line, i, 0, this.sprintf);
            case '.cond': return this.parseArgs(line, i, 0, this.cond);
            case '.def':
            case '.defined':
                return this.parseArgs(line, i, 1, this.defined);
            case '.definedsymbol':
                return this.parseArgs(line, i, 1, this.definedSymbol);
            case '.constantsymbol':
                return this.parseArgs(line, i, 1, this.constantSymbol);
            case '.referencedsymbol':
                return this.parseArgs(line, i, 1, this.referencedSymbol);
        }
        return i + 1;
    }
    // QUESTION - does skip descend into groups?
    //          - seems like it should...
    skip(line, i) {
        // expand i + 1, then splice self out
        line.splice(i, 1);
        const skipped = line[i];
        if (skipped?.token === 'grp') {
            this.expandToken(skipped.inner, 0);
        }
        else {
            this.expandToken(line, i + 1);
        }
        return i;
    }
    noexpand(line, i) {
        const skip = line[i + 1];
        if (skip.token === 'grp') {
            line.splice(i, 2, ...skip.inner);
            i += skip.inner.length - 1;
        }
        else {
            line.splice(i, 1);
        }
        return i + 1;
    }
    parseArgs(line, i, argCount, fn) {
        const cs = line[i];
        Tokens.expect(Tokens.LP, line[i + 1], cs);
        const end = Tokens.findBalanced(line, i + 1);
        const args = Tokens.parseArgList(line, i + 2, end).map(ts => {
            if (ts.length === 1 && ts[0].token === 'grp')
                ts = ts[0].inner;
            return this.expandLine(ts);
        });
        if (argCount && args.length !== argCount) {
            throw new Error(`Expected ${argCount} parameters: ${Tokens.nameAt(cs)}`);
        }
        const expansion = fn.call(this, cs, ...args);
        line.splice(i, end + 1 - i, ...expansion);
        return i; // continue expansion from same spot
    }
    tcount(cs, arg) {
        return [{ token: 'num', num: Tokens.count(arg), source: cs.source }];
    }
    ident(cs, arg) {
        const str = Tokens.expectString(arg[0], cs);
        Tokens.expectEol(arg[1], 'a single token');
        return [{ token: 'ident', str, source: arg[0].source }];
    }
    string(cs, arg) {
        const str = Tokens.expectIdentifier(arg[0], cs);
        Tokens.expectEol(arg[1], 'a single token');
        return [{ token: 'str', str, source: arg[0].source }];
    }
    concat(cs, ...args) {
        const strs = args.map(ts => {
            const str = Tokens.expectString(ts[0]);
            Tokens.expectEol(ts[1], 'a single string');
            return str;
        });
        return [{ token: 'str', str: strs.join(''), source: cs.source }];
    }
    sprintf(cs, fmtToks, ..._args) {
        const fmt = Tokens.expectString(fmtToks[0], cs);
        Tokens.expectEol(fmtToks[1], 'a single format string');
        // figure out what placeholders...
        // TODO - evaluate numeric args as exprs, strings left as is
        const [_] = [fmt];
        throw new Error('unimplemented');
    }
    cond(_cs, ..._args) {
        throw new Error('unimplemented');
    }
    defined(cs, arg) {
        const ident = Tokens.expectIdentifier(arg[0], cs);
        Tokens.expectEol(arg[1], 'a single identifier');
        return [{ token: 'num', num: this.macros.has(ident) ? 1 : 0 }];
    }
    definedSymbol(cs, arg) {
        const ident = Tokens.expectIdentifier(arg[0], cs);
        Tokens.expectEol(arg[1], 'a single identifier');
        return [{ token: 'num', num: this.env.definedSymbol(ident) ? 1 : 0 }];
    }
    constantSymbol(cs, arg) {
        const ident = Tokens.expectIdentifier(arg[0], cs);
        Tokens.expectEol(arg[1], 'a single identifier');
        return [{ token: 'num', num: this.env.constantSymbol(ident) ? 1 : 0 }];
    }
    referencedSymbol(cs, arg) {
        const ident = Tokens.expectIdentifier(arg[0], cs);
        Tokens.expectEol(arg[1], 'a single identifier');
        return [{ token: 'num', num: this.env.referencedSymbol(ident) ? 1 : 0 }];
    }
    // TODO - does .byte expand its strings into bytes here?
    //   -- maybe not...
    //   -- do we need to handle string exprs at all?
    //   -- maybe not - maybe just tokens?
    /**
     * If the following is an identifier, skip it.  This is used when
     * expanding .define, .undefine, .defined, .ifdef, and .ifndef.
     * Does not skip scoped identifiers, since macros can't be scoped.
     */
    skipIdentifier(line, i) {
        return line[i + 1]?.token === 'ident' ? i + 2 : i + 1;
    }
    ////////////////////////////////////////////////////////////////
    // RUN DIRECTIVES
    async tryRunDirective(line) {
        const first = line[0];
        if (first.token !== 'cs')
            throw new Error(`impossible`);
        const handler = this.runDirectives[first.str];
        if (!handler)
            return false;
        await handler(line);
        return true;
    }
    evaluateConst(expr) {
        // Attempt to look up a symbol and see if its a constant value
        const evalWrapper = (ex) => {
            if (ex.op === 'sym' && this.env.definedSymbol(ex.sym)) {
                // HACK? If its defined but not set, default it to zero?
                const num = this.env.evaluate(ex);
                if (num === undefined)
                    throw new Error(`Symbol ${ex.sym} is undefined`);
                return Exprs.evaluate({ op: 'num', num, meta: Exprs.size(num, undefined) });
            }
            return Exprs.evaluate(ex);
        };
        expr = Exprs.traversePost(expr, evalWrapper);
        if (expr.op === 'num' && !expr.meta?.rel)
            return expr.num;
        const at = Tokens.at(expr);
        throw new Error(`Expected a constant${at}`);
    }
    async parseDefine(line) {
        const name = Tokens.expectIdentifier(line[1], line[0]);
        const define = Define.from(line);
        const prev = this.macros.get(name);
        if (prev instanceof Define) {
            prev.append(define);
        }
        else if (prev) {
            throw new Error(`Already defined: ${name}`);
        }
        else {
            this.macros.set(name, define);
        }
        return await Promise.resolve();
    }
    async parseUndefine(line) {
        const [cs, ident, eol] = line;
        const name = Tokens.expectIdentifier(ident, cs);
        Tokens.expectEol(eol);
        if (!this.macros.has(name)) {
            throw new Error(`Not defined: ${Tokens.nameAt(ident)}`);
        }
        this.macros.delete(name);
        return await Promise.resolve();
    }
    async parseMacro(line) {
        const name = Tokens.expectIdentifier(line[1], line[0]);
        const macro = await Macro.from(line, this.stream);
        const prev = this.macros.get(name);
        if (prev)
            throw new Error(`Already defined: ${name}`);
        this.macros.set(name, macro);
    }
    async parseRepeat(line) {
        const [expr, end] = Exprs.parse(line, 1);
        const at = line[1] || line[0];
        if (!expr)
            throw new Error(`Expected expression: ${Tokens.nameAt(at)}`);
        const times = this.evaluateConst(expr);
        if (times == null)
            throw new Error(`Expected a constant${Tokens.at(expr)}`);
        let ident;
        if (end < line.length) {
            if (!Tokens.eq(line[end], Tokens.COMMA)) {
                throw new Error(`Expected comma: ${Tokens.nameAt(line[end])}`);
            }
            ident = Tokens.expectIdentifier(line[end + 1]);
            Tokens.expectEol(line[end + 2]);
        }
        const lines = [];
        let depth = 1;
        while (depth > 0) {
            line = await this.stream.next() ?? fail(`.repeat with no .endrep`);
            if (Tokens.eq(line[0], Tokens.REPEAT))
                depth++;
            if (Tokens.eq(line[0], Tokens.ENDREPEAT))
                depth--;
            if (Tokens.eq(line[0], Tokens.ENDREP))
                depth--;
            lines.push(line);
        }
        this.repeats.push([lines, times, -1, ident]);
        this.parseEndRepeat(line);
    }
    async parseEndRepeat(line) {
        Tokens.expectEol(line[1]);
        const top = this.repeats.pop();
        if (!top)
            throw new Error(`.endrep with no .repeat${Tokens.at(line[0])}`);
        if (++top[2] >= top[1])
            return await Promise.resolve();
        this.repeats.push(top);
        this.stream.unshift(...top[0].map(line => line.map(token => {
            if (token.token !== 'ident' || token.str !== top[3])
                return token;
            const t = { token: 'num', num: top[2] };
            if (token.source)
                t.source = token.source;
            return t;
        })));
        return await Promise.resolve();
    }
    async parseIf(cond) {
        let depth = 1;
        let done = false;
        const result = [];
        while (depth > 0) {
            const line = await this.stream.next();
            if (!line)
                throw new Error(`EOF looking for .endif`); // TODO: start?
            const front = line[0];
            if (Tokens.eq(front, Tokens.ENDIF)) {
                depth--;
                if (!depth)
                    break;
            }
            else if (front.token === 'cs' && front.str.startsWith('.if')) {
                depth++;
            }
            else if (depth === 1 && !done) {
                if (cond && (Tokens.eq(front, Tokens.ELSE) ||
                    Tokens.eq(front, Tokens.ELSEIF))) {
                    // if true ... else .....
                    cond = false;
                    done = true;
                    continue;
                }
                else if (Tokens.eq(front, Tokens.ELSEIF)) {
                    // if false ... else if .....
                    cond = !!this.evaluateConst(parseOneExpr(line.slice(1), front));
                    continue;
                }
                else if (Tokens.eq(front, Tokens.ELSE)) {
                    // if false ... else .....
                    cond = true;
                    continue;
                }
            }
            // anything else on the line
            if (cond)
                result.push(line);
        }
        // result has the expansion: unshift it
        this.stream.unshift(...result);
    }
    parseIfDef(args, cs) {
        return this.macros.has(parseOneIdent(args, cs)) ||
            this.env.definedSymbol(parseOneIdent(args, cs));
    }
}
// Handles scoped names, too.
function parseOneIdent(ts, prev) {
    const e = parseOneExpr(ts, prev);
    return Exprs.identifier(e);
}
function parseOneExpr(ts, prev) {
    if (!ts.length) {
        if (!prev)
            throw new Error(`Expected expression`);
        throw new Error(`Expected expression: ${Tokens.nameAt(prev)}`);
    }
    return Exprs.parseOnly(ts);
}
function noGarbage(token) {
    if (token)
        throw new Error(`garbage at end of line: ${Tokens.nameAt(token)}`);
}
// function fail(t: Token, msg: string): never {
//   const s = t.stream;
//   if (s) {
//     msg += `\n  at ${s.file}:${s.line}:${s.column}: Tokens.name(t)`;
//     // TODO - expanded from?
//   }
//   throw new Error(msg);
// }
function badClose(open, tok) {
    throw new Error(`${Tokens.name(tok)} with no ${open}${Tokens.at(tok)}`);
}
function fail(msg) {
    throw new Error(msg);
}
