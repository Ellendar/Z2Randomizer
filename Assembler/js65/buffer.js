class State {
    constructor(line, column, prefix, remainder, match) {
        Object.defineProperty(this, "line", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: line
        });
        Object.defineProperty(this, "column", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: column
        });
        Object.defineProperty(this, "prefix", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: prefix
        });
        Object.defineProperty(this, "remainder", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: remainder
        });
        Object.defineProperty(this, "match", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: match
        });
    }
}
export class Buffer {
    constructor(content, line = 1, column = 0) {
        Object.defineProperty(this, "content", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: content
        });
        Object.defineProperty(this, "line", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: line
        });
        Object.defineProperty(this, "column", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: column
        });
        Object.defineProperty(this, "prefix", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: ''
        });
        Object.defineProperty(this, "remainder", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        Object.defineProperty(this, "lastMatch", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        this.remainder = content;
    }
    advance(s) {
        const s1 = this.remainder.substring(0, s.length);
        if (s !== s1)
            throw new Error(`Non-rooted token: '${s}' vs '${s1}'`);
        this.prefix += s;
        this.remainder = this.remainder.substring(s.length);
        s = s.replace('\n', s.includes('\r') ? '' : '\r');
        const lines = s.split(/\r/g);
        if (lines.length > 1) {
            this.line += lines.length - 1;
            this.column = 0;
        }
        this.column += lines[lines.length - 1].length;
    }
    saveState() {
        return new State(this.line, this.column, this.prefix, this.remainder, this.lastMatch);
    }
    restoreState(state) {
        this.line = state.line;
        this.column = state.column;
        this.prefix = state.prefix;
        this.remainder = state.remainder;
        this.lastMatch = state.match;
    }
    skip(re) {
        const match = re.exec(this.remainder);
        if (!match)
            return false;
        this.advance(match[0]);
        return true;
    }
    space() { return this.skip(/^[ \t]+/); }
    newline() { return this.skip(/^(\r\n|\n|\r)/); }
    lookingAt(re) {
        if (typeof re === 'string')
            return this.remainder.startsWith(re);
        return re.test(this.remainder);
    }
    // NOTE: re should always be rooted with /^/ at the start.
    token(re) {
        let match;
        if (typeof re === 'string') {
            if (!this.remainder.startsWith(re))
                return false;
            match = [re];
        }
        else {
            match = re.exec(this.remainder);
        }
        if (!match)
            return false;
        match.line = this.line;
        match.column = this.column;
        this.lastMatch = match;
        this.advance(match[0]);
        //    console.log(`TOKEN: ${re} "${match[0]}"`);
        //try{throw Error();}catch(e){console.log(e);}
        return true;
    }
    lookBehind(re) {
        if (typeof re === 'string')
            return this.prefix.endsWith(re);
        const match = re.exec(this.prefix);
        if (!match)
            return false;
        match.line = this.line;
        match.column = this.line;
        this.lastMatch = match;
        return true;
    }
    match() {
        return this.lastMatch;
    }
    group(index = 0) {
        return this.lastMatch?.[index];
    }
    eof() {
        return !this.remainder;
    }
}
