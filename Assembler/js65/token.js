import { z } from './deps/deno.land/x/zod@v3.21.4/mod.js';
import { assertNever } from './util.js';
// TODO - consider moving into a namespace?
export function concat(...sources) {
    let source;
    return {
        next: async () => {
            while (true) {
                if (!source)
                    source = sources.shift();
                if (!source)
                    return undefined;
                const line = await source.next();
                if (line)
                    return line;
                source = undefined;
            }
        },
    };
}
const BaseSourceInfo = z.object({
    file: z.string(),
    line: z.number(),
    column: z.number(),
});
export const SourceInfoZ = BaseSourceInfo.extend({
    parent: z.lazy(() => SourceInfoZ).optional(),
});
// Grouping tokens
export const LB = { token: 'lb' };
export const LC = { token: 'lc' };
export const LP = { token: 'lp' };
export const RB = { token: 'rb' };
export const RC = { token: 'rc' };
export const RP = { token: 'rp' };
export const EOL = { token: 'eol' };
export const EOF = { token: 'eof' };
// Important macro expansion tokens
export const DEFINE = { token: 'cs', str: '.define' };
export const DOT_EOL = { token: 'cs', str: '.eol' };
export const ELSE = { token: 'cs', str: '.else' };
export const ELSEIF = { token: 'cs', str: '.elseif' };
export const ENDIF = { token: 'cs', str: '.endif' };
export const ENDMAC = { token: 'cs', str: '.endmac' };
export const ENDMACRO = { token: 'cs', str: '.endmacro' };
export const ENDREP = { token: 'cs', str: '.endrep' };
export const ENDREPEAT = { token: 'cs', str: '.endrepeat' };
export const ENDPROC = { token: 'cs', str: '.endproc' };
export const ENDSCOPE = { token: 'cs', str: '.endscope' };
export const LOCAL = { token: 'cs', str: '.local' };
export const MACRO = { token: 'cs', str: '.macro' };
export const REPEAT = { token: 'cs', str: '.repeat' };
export const SET = { token: 'cs', str: '.set' };
export const SKIP = { token: 'cs', str: '.skip' };
// Tokens we match
export const BYTE = { token: 'cs', str: '.byte' };
export const BYTESTR = { token: 'cs', str: '.bytestr' };
export const WORD = { token: 'cs', str: '.word' };
// Important operator tokens
export const COLON = { token: 'op', str: ':' };
//export const DCOLON: Token = {token: 'op', str: '::'};
export const COMMA = { token: 'op', str: ',' };
export const STAR = { token: 'op', str: '*' };
export const IMMEDIATE = { token: 'op', str: '#' };
export const ASSIGN = { token: 'op', str: '=' };
export function match(left, right) {
    if (left.token !== right.token)
        return false;
    if (left.token === 'num' || left.token === 'str')
        return true;
    if (left.str !== right.str)
        return false;
    // NOTE: don't compare num because 'num' already early-returned.
    return true;
}
export function eq(left, right) {
    if (!left || !right)
        return false;
    if (left.token !== right.token)
        return false;
    if (left.token === 'grp')
        return false; // don't check groups.
    if (left.str !== right.str)
        return false;
    if (left.num !== right.num)
        return false;
    return true;
}
export function name(arg) {
    switch (arg.token) {
        case 'num': return `NUM[$${arg.num.toString(16)}]`;
        case 'str': return `STR[$${arg.str}]`;
        case 'lb': return `[`;
        case 'rb': return `]`;
        case 'grp': return `{`;
        case 'lc': return `{`;
        case 'rc': return `}`;
        case 'lp': return `(`;
        case 'rp': return `)`;
        case 'eol': return `EOL`;
        case 'eof': return `EOF`;
        case 'ident':
            return arg.str;
        case 'cs':
        case 'op':
            return `${arg.str.toUpperCase()}`;
        default:
            assertNever(arg);
    }
}
export function at(arg) {
    const s = arg.source;
    if (!s)
        return '';
    const parent = s.parent ? at({ source: s.parent }) : '';
    return `\n  at ${s.file}:${s.line}:${s.column}${parent}`;
    // TODO - definition vs usage?
}
export function nameAt(arg) {
    if (!arg)
        return 'at unknown';
    const token = arg;
    return (token.token ? name(token) : '') + at(arg);
}
export function expectEol(token, name = 'end of line') {
    if (token)
        throw new Error(`Expected ${name}: ${nameAt(token)}`);
}
export function expect(want, token, prev) {
    if (!token) {
        if (!prev)
            throw new Error(`Expected ${name(want)}`);
        throw new Error(`Expected ${name(want)} after ${nameAt(token)}`);
    }
    if (!eq(want, token)) {
        throw new Error(`Expected ${name(want)}: ${nameAt(token)}`);
    }
}
export function expectIdentifier(token, prev) {
    return expectStringToken('ident', 'identifier', token, prev);
}
export function optionalIdentifier(token) {
    return optionalStringToken('ident', 'identifier', token);
}
export function expectString(token, prev) {
    return expectStringToken('str', 'constant string', token, prev);
}
export function optionalString(token) {
    return optionalStringToken('str', 'constant string', token);
}
function expectStringToken(want, name, token, prev) {
    if (!token) {
        if (!prev)
            throw new Error(`Expected ${name}`);
        throw new Error(`Expected ${name} after ${nameAt(prev)}`);
    }
    if (token.token !== want) {
        throw new Error(`Expected ${name}: ${nameAt(token)}`);
    }
    return token.str;
}
function optionalStringToken(want, name, token) {
    if (!token) {
        return undefined;
    }
    if (token.token !== want) {
        throw new Error(`Expected ${name}: ${nameAt(token)}`);
    }
    return token.str;
}
// export function fail(token: Token, msg: string): never {
//   if 
//   throw new Error(msg + 
// }
/**
 * Given a comma-separated list of identifiers, return the
 * identifiers as a list of strings.  Throws an error if
 * the input is not actually a comma-separated list.
 */
export function identsFromCList(list) {
    if (!list.length)
        return [];
    const out = [];
    for (let i = 0; i <= list.length; i += 2) {
        const ident = list[i];
        if (ident?.token !== 'ident') {
            if (ident)
                throw new Error(`Expected identifier: ${nameAt(ident)}`);
            const last = list[list.length - 1];
            throw new Error(`Expected identifier after ${nameAt(last)}`);
        }
        else if (i + 1 < list.length && !eq(list[i + 1], COMMA)) {
            const sep = list[i + 1];
            throw new Error(`Expected comma: ${nameAt(sep)}`);
        }
        out.push(ident.str);
    }
    return out;
}
/** Finds a balanced paren/bracket: returns its index, or -1. */
export function findBalanced(tokens, i) {
    const open = tokens[i++].token;
    if (open !== 'lp' && open !== 'lb')
        throw new Error(`non-grouping token`);
    const close = open === 'lp' ? 'rp' : 'rb';
    let depth = 1;
    for (; i < tokens.length; i++) {
        const tok = tokens[i].token;
        depth += Number(tok === open) - Number(tok === close);
        if (!depth)
            return i;
    }
    return -1;
}
/**
 * Splits on commas not enclosed in balanced parens.  Braces are
 * ignored/not allowed at this point.  This is intended for arithmetic.
 */
export function parseArgList(tokens, start = 0, end = tokens.length) {
    let arg = [];
    const args = [arg];
    let parens = 0;
    for (let i = start; i < end; i++) {
        const token = tokens[i];
        if (!parens && eq(token, COMMA)) {
            args.push(arg = []);
        }
        else {
            arg.push(token);
            if (eq(token, LP))
                parens++;
            if (eq(token, RP)) {
                if (--parens < 0)
                    throw new Error(`Unbalanced paren${at(token)}`);
            }
        }
    }
    return args;
}
export function parseAttrList(tokens, start) {
    // Expect a colon...
    // TODO - allow colon inside balanced parens? allow a single group?
    //   .segment "foo" :bar {foo:bar} :baz
    const out = new Map();
    let key;
    let val = [];
    if (start >= tokens.length)
        return out;
    if (!eq(tokens[start], COLON)) {
        throw new Error(`Unexpected: ${nameAt(tokens[start])}`);
    }
    for (let i = start + 1; i < tokens.length; i++) {
        const tok = tokens[i];
        if (eq(tok, COLON)) {
            if (key == null)
                throw new Error(`Missing key${at(tok)}`);
            out.set(key, val);
            key = undefined;
            val = [];
        }
        else if (key == null) {
            key = expectIdentifier(tok);
        }
        else {
            val.push(tok);
        }
    }
    if (key != null) {
        out.set(key, val);
    }
    else {
        expectIdentifier(undefined, tokens[tokens.length - 1]);
    }
    return out;
}
/** Finds a comma or EOL. */
export function findComma(tokens, start) {
    const index = find(tokens, COMMA, start);
    return index < 0 ? tokens.length : index;
}
/** Finds a token, or -1 if not found. */
export function find(tokens, want, start) {
    for (let i = start; i < tokens.length; i++) {
        if (eq(tokens[i], want))
            return i;
    }
    return -1;
}
export function count(ts) {
    let total = 0;
    for (const t of ts) {
        if (t.token === 'grp') {
            total += 2 + count(t.inner);
        }
        else {
            total++;
        }
    }
    return total;
}
export function isRegister(t, reg) {
    return t.token === 'ident' && t.str.toLowerCase() === reg;
}
export function str(t) {
    switch (t.token) {
        case 'cs':
        case 'ident':
        case 'str':
        case 'op':
            return t.str;
    }
    throw new Error(`Non-string token: ${nameAt(t)}`);
}
export function strip(t) {
    delete t.source;
    return t;
}
export function format(toks) {
    return toks.map(t => {
        switch (t.token) {
            case 'grp': return `{ ${format(t.inner)} }`;
            case 'lb': return '[';
            case 'lc': return '{';
            case 'lp': return '(';
            case 'rb': return ']';
            case 'rc': return '}';
            case 'rp': return ')';
            case 'eol': return '.eol';
            case 'eof': throw new Error(`Cannot format EOF`);
            case 'num': return '$' + t.num.toString(16).padStart(t.num < 256 ? 2 : 4, '0');
            case 'ident': return t.str;
            case 'op': return t.str;
            case 'cs': return t.str;
            case 'str': return `"${t.str.replace(/[\\"]/g, '\\$&')}"`;
            default: return checkExhaustive(t);
        }
    }).join(' ');
}
function checkExhaustive(arg) {
    throw new Error(`was supposed to be exhaustive but got ${arg}`);
}
// interface Expr {
//   // operator, function name, '()', '{}', 'num', 'str', 'ident'
//   op: string;
//   // one arg for a unary, two for binary, or N for comma or function
//   args: Expr[];
//   // if op === 'num'
//   num: number;
//   // if op === 'str' or 'ident'
//   str: string;
// }
export const TOKENFUNCS = new Set([
    '.blank',
    '.const',
    '.defined',
    '.left',
    '.match',
    '.mid',
    '.right',
    '.tcount',
    '.xmatch',
]);
export const DIRECTIVES = [
    '.define',
    '.else',
    '.elseif',
    '.endif',
    '.endmacro',
    '.endproc',
    '.endscope',
    '.ident',
    '.if',
    '.ifblank',
    '.ifdef',
    '.ifnblank',
    '.ifndef',
    '.ifnref',
    '.ifref',
    '.incbin',
    '.include',
    '.local',
    '.macpack',
    '.macro',
    '.proc',
    '.scope',
    '.skip',
];
