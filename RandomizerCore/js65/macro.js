import * as Tokens from './token.js';
export class Macro {
    constructor(params, production) {
        Object.defineProperty(this, "params", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: params
        });
        Object.defineProperty(this, "production", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: production
        });
    }
    static async from(line, source) {
        // First line must start with .macro <name> [args]
        // Last line is the line BEFORE the .endmacro
        // Nested macro definitions are not allowed!
        if (!Tokens.eq(line[0], Tokens.MACRO))
            throw new Error(`invalid`);
        if (line[1]?.token !== 'ident')
            throw new Error(`invalid`);
        const params = Tokens.identsFromCList(line.slice(2));
        const lines = [];
        let next;
        while ((next = await source.next())) {
            if (Tokens.eq(next[0], Tokens.ENDMACRO))
                return new Macro(params, lines);
            if (Tokens.eq(next[0], Tokens.ENDMAC))
                return new Macro(params, lines);
            lines.push(next);
        }
        throw new Error(`EOF looking for .endmacro: ${Tokens.nameAt(line[1])}`);
    }
    expand(tokens, idGen) {
        // Find the parameters.
        // This is a little more principled than Define, but we do need to be
        // a little careful.
        let i = 1; // start looking _after_ macro ident
        const replacements = new Map();
        const lines = [];
        // Find a comma, skipping balanced curlies.  Parens are not special.
        for (const param of this.params) {
            const comma = Tokens.findComma(tokens, i);
            let slice = tokens.slice(i, comma);
            i = comma + 1;
            if (slice.length === 1 && slice[0].token === 'grp') {
                // unwrap one layer
                slice = slice[0].inner;
            }
            replacements.set(param, slice);
        }
        if (i < tokens.length) {
            throw new Error(`Too many macro parameters: ${Tokens.nameAt(tokens[i])}`);
        }
        // All params filled in, make replacement
        const locals = new Map();
        for (const line of this.production) {
            if (Tokens.eq(line[0], Tokens.LOCAL)) {
                const locallist = Tokens.identsFromCList(line.slice(1));
                for (const local of locallist) {
                    // pick a name that is impossible to type due to the '@' in the middle
                    locals.set(local, `${local}@${idGen.next()}`);
                }
            }
            // TODO - check for .local here and rename?  move into assemlber
            // or preprocessing...?  probably want to keep track elsewhere.
            const map = (toks) => {
                const mapped = [];
                for (const tok of toks) {
                    // skip over the line declaring the local variables
                    if (Tokens.eq(tok, Tokens.LOCAL))
                        return mapped;
                    if (tok.token === 'ident') {
                        const param = replacements.get(tok.str);
                        if (param) {
                            // this is actually a parameter
                            mapped.push(...param); // TODO - copy w/ child sourceinfo?
                            continue;
                        }
                        const local = locals.get(tok.str);
                        if (local) {
                            mapped.push({ token: 'ident', str: local });
                            continue;
                        }
                    }
                    else if (tok.token === 'grp') {
                        mapped.push({ token: 'grp', inner: map(tok.inner) });
                        continue;
                    }
                    const source = tok.source && tokens[0].source ?
                        { ...tok.source, parent: tokens[0].source } :
                        tok.source || tokens[0].source;
                    mapped.push(source ? { ...tok, source } : tok);
                }
                return mapped;
            };
            lines.push(map(line));
        }
        return lines.filter(m => m.length != 0);
    }
}
