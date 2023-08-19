import { Buffer } from './buffer.js';
import * as Tokens from './token.js';
export class Tokenizer {
    constructor(str, file = 'input.s', opts = {}) {
        Object.defineProperty(this, "file", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: file
        });
        Object.defineProperty(this, "opts", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: opts
        });
        Object.defineProperty(this, "buffer", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        this.buffer = new Buffer(str);
    }
    async next() {
        return await new Promise((resolve) => {
            let tok = this.token();
            while (Tokens.eq(tok, Tokens.EOL)) {
                // Skip EOLs at beginning of line.
                tok = this.token();
            }
            // Group curly brace groups into a single effective Tokens.
            const stack = [[]];
            let depth = 0;
            while (!Tokens.eq(tok, Tokens.EOL) && !Tokens.eq(tok, Tokens.EOF)) {
                if (Tokens.eq(tok, Tokens.LC)) {
                    stack[depth++].push(tok);
                    stack.push([]);
                }
                else if (Tokens.eq(tok, Tokens.RC)) {
                    if (!depth)
                        throw new Error(`Missing open curly: ${Tokens.nameAt(tok)}`);
                    const inner = stack.pop();
                    const source = stack[--depth].pop().source;
                    const token = { token: 'grp', inner };
                    if (source)
                        token.source = source;
                    stack[depth].push(token);
                }
                else {
                    stack[depth].push(tok);
                }
                tok = this.token();
            }
            if (depth) {
                const open = stack[depth - 1].pop();
                throw new Error(`Missing close curly: ${Tokens.nameAt(open)}`);
            }
            resolve(stack[0].length ? stack[0] : undefined);
        });
    }
    token() {
        // skip whitespace
        while (this.buffer.space() ||
            this.buffer.token(/^;.*/) ||
            (this.opts.lineContinuations && this.buffer.token(/^\\(\r\n|\n|\r)/))) {
            // intentionally empty
        }
        if (this.buffer.eof())
            return Tokens.EOF;
        // remember position of non-whitespace
        const source = {
            file: this.file,
            line: this.buffer.line,
            column: this.buffer.column,
        };
        try {
            const tok = this.tokenInternal();
            if (!this.opts.skipSourceAnnotations)
                tok.source = source;
            return tok;
        }
        catch (err) {
            const { file, line, column } = source;
            let last = this.buffer.group();
            last = last ? ` near '${last}'` : '';
            err.message += `\n  at ${file}:${line}:${column}${last}`;
            throw err;
        }
    }
    tokenInternal() {
        if (this.buffer.newline())
            return { token: 'eol' };
        if (this.buffer.token(/^@+[a-z0-9_]*/i) ||
            this.buffer.token(/^((::)?[a-z_][a-z0-9_]*)+/i)) {
            return this.strTok('ident');
        }
        if (this.buffer.token(/^\.[a-z]+/i))
            return this.strTok('cs');
        if (this.buffer.token(/^:([+-]\d+|[-+]+|<+rts|>*rts)/))
            return this.strTok('ident');
        if (this.buffer.token(/^(:|\++|-+|&&?|\|\|?|[#*/,=~!^]|<[<>=]?|>[>=]?)/)) {
            return this.strTok('op');
        }
        if (this.buffer.token('['))
            return { token: 'lb' };
        if (this.buffer.token('{'))
            return { token: 'lc' };
        if (this.buffer.token('('))
            return { token: 'lp' };
        if (this.buffer.token(']'))
            return { token: 'rb' };
        if (this.buffer.token('}'))
            return { token: 'rc' };
        if (this.buffer.token(')'))
            return { token: 'rp' };
        if (this.buffer.token(/^["']/))
            return this.tokenizeStr();
        if (this.buffer.token(/^[$%]?[0-9a-z_]+/i))
            return this.tokenizeNum();
        throw new Error(`Syntax error`);
    }
    tokenizeStr() {
        const b = this.buffer;
        const m = b.match();
        const end = m[0];
        let str = '';
        while (!b.lookingAt(end)) {
            if (b.eof())
                throw new Error(`EOF while looking for ${end}`);
            if (b.token(/^\\u([0-9a-f]{4})/i)) {
                str += String.fromCodePoint(parseInt(b.group(1), 16));
            }
            else if (b.token(/^\\x([0-9a-f]{2})/i)) {
                str += String.fromCharCode(parseInt(b.group(1), 16));
            }
            else if (b.token(/^\\(.)/)) {
                str += b.group(1);
            }
            else {
                b.token(/^./);
                str += b.group(0);
            }
        }
        b.token(end);
        return { token: 'str', str };
    }
    strTok(token) {
        return { token, str: this.buffer.group() };
    }
    tokenizeNum(str = this.buffer.group()) {
        if (this.opts.numberSeparators)
            str = str.replace(/_/g, '');
        if (str[0] === '$')
            return parseHex(str.substring(1));
        if (str[0] === '%')
            return parseBin(str.substring(1));
        if (str[0] === '0')
            return parseOct(str);
        return parseDec(str);
    }
}
function parseHex(str) {
    if (!/^[0-9a-f]+$/i.test(str))
        throw new Error(`Bad hex number: $${str}`);
    return { token: 'num', num: Number.parseInt(str, 16), width: Math.ceil(str.length / 2) };
}
function parseDec(str) {
    if (!/^[0-9]+$/.test(str))
        throw new Error(`Bad decimal number: ${str}`);
    return { token: 'num', num: Number.parseInt(str, 10) };
}
function parseOct(str) {
    if (!/^[0-7]+$/.test(str))
        throw new Error(`Bad octal number: ${str}`);
    return { token: 'num', num: Number.parseInt(str, 8) };
}
function parseBin(str) {
    if (!/^[01]+$/.test(str))
        throw new Error(`Bad binary number: %${str}`);
    return { token: 'num', num: Number.parseInt(str, 2), width: Math.ceil(str.length / 8) };
}
