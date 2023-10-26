class AbstractCpu {
    constructor(table) {
        Object.defineProperty(this, "table", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: table
        });
        Object.defineProperty(this, "reverse", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        const reverse = [];
        for (const [mnemonic, ops] of Object.entries(table)) {
            for (const [mode, op] of Object.entries(ops)) {
                reverse[op] = [mnemonic, mode];
            }
        }
        this.reverse = reverse;
    }
    op(mnemonic) {
        const ops = this.table[mnemonic];
        if (!ops)
            throw new Error(`Bad mnemonic: ${mnemonic}`);
        return ops;
    }
    disasm(byte) {
        const result = this.reverse[byte];
        return result && [...result];
    }
    // TODO - may need to abstract this, too...
    argLen(mode) {
        switch (mode) {
            case 'acc':
            case 'imp':
                return 0;
            case 'imm':
            case 'rel':
            case 'zpg':
            case 'zpx':
            case 'zpy':
            case 'iny':
            case 'inx':
                return 1;
            case 'abs':
            case 'abx':
            case 'aby':
            case 'ind':
                return 2;
        }
    }
    format(mode, arg) {
        if (mode === 'acc' || mode === 'imp')
            return '';
        if (typeof arg === 'number') {
            if (mode === 'rel') {
                const displacement = (arg > 127 ? arg - 256 : arg) + 2;
                if (displacement < 0) {
                    arg = `*-${-displacement}`;
                }
                else if (displacement > 0) {
                    arg = `*+${displacement}`;
                }
                else {
                    arg = '*';
                }
            }
            else if (mode.startsWith('zp') || mode === 'iny' || mode === 'imm') {
                arg = `$${(arg & 0xff).toString(16).padStart(2, '0')}`;
            }
            else {
                arg = `$${(arg & 0xffff).toString(16).padStart(4, '0')}`;
            }
        }
        switch (mode) {
            case 'imm': return `#${arg}`;
            case 'rel': return arg;
            case 'zpg': return arg;
            case 'abs': return arg;
            case 'zpx': return `${arg},x`;
            case 'abx': return `${arg},x`;
            case 'zpy': return `${arg},y`;
            case 'aby': return `${arg},y`;
            case 'iny': return `(${arg}),y`;
            case 'ind': return `(${arg})`;
            case 'inx': return `(${arg},x)`;
        }
    }
}
// deno-lint-ignore no-namespace
export var Cpu;
(function (Cpu) {
    Cpu.P02 = new AbstractCpu({
        adc: { abs: 0x6d, abx: 0x7d, aby: 0x79, imm: 0x69,
            iny: 0x71, inx: 0x61, zpg: 0x65, zpx: 0x75 },
        and: { abs: 0x2d, abx: 0x3d, aby: 0x39, imm: 0x29,
            iny: 0x31, inx: 0x21, zpg: 0x25, zpx: 0x35 },
        asl: { abs: 0x0e, abx: 0x1e, acc: 0x0a, imp: 0x0a, zpg: 0x06, zpx: 0x16 },
        bcc: { rel: 0x90 },
        bcs: { rel: 0xb0 },
        beq: { rel: 0xf0 },
        bit: { abs: 0x2c, zpg: 0x24 },
        bmi: { rel: 0x30 },
        bne: { rel: 0xd0 },
        bpl: { rel: 0x10 },
        brk: { imp: 0x00 },
        bvc: { rel: 0x50 },
        bvs: { rel: 0x70 },
        clc: { imp: 0x18 },
        cld: { imp: 0xd8 },
        cli: { imp: 0x58 },
        clv: { imp: 0xb8 },
        cmp: { abs: 0xcd, abx: 0xdd, aby: 0xd9, imm: 0xc9,
            iny: 0xd1, inx: 0xc1, zpg: 0xc5, zpx: 0xd5 },
        cpx: { abs: 0xec, imm: 0xe0, zpg: 0xe4 },
        cpy: { abs: 0xcc, imm: 0xc0, zpg: 0xc4 },
        dec: { abs: 0xce, abx: 0xde, zpg: 0xc6, zpx: 0xd6 },
        dex: { imp: 0xca },
        dey: { imp: 0x88 },
        eor: { abs: 0x4d, abx: 0x5d, aby: 0x59, imm: 0x49,
            iny: 0x51, inx: 0x41, zpg: 0x45, zpx: 0x55 },
        inc: { abs: 0xee, abx: 0xfe, zpg: 0xe6, zpx: 0xf6 },
        inx: { imp: 0xe8 },
        iny: { imp: 0xc8 },
        jmp: { abs: 0x4c, ind: 0x6c },
        jsr: { abs: 0x20 },
        lda: { abs: 0xad, abx: 0xbd, aby: 0xb9, imm: 0xa9,
            iny: 0xb1, inx: 0xa1, zpg: 0xa5, zpx: 0xb5 },
        ldx: { abs: 0xae, aby: 0xbe, imm: 0xa2, zpg: 0xa6, zpy: 0xb6 },
        ldy: { abs: 0xac, abx: 0xbc, imm: 0xa0, zpg: 0xa4, zpx: 0xb4 },
        lsr: { abs: 0x4e, abx: 0x5e, acc: 0x4a, imp: 0x4a, zpg: 0x46, zpx: 0x56 },
        nop: { imp: 0xea },
        ora: { abs: 0x0d, abx: 0x1d, aby: 0x19, imm: 0x09,
            iny: 0x11, inx: 0x01, zpg: 0x05, zpx: 0x15 },
        pha: { imp: 0x48 },
        php: { imp: 0x08 },
        pla: { imp: 0x68 },
        plp: { imp: 0x28 },
        rol: { abs: 0x2e, abx: 0x3e, acc: 0x2a, imp: 0x2a, zpg: 0x26, zpx: 0x36 },
        ror: { abs: 0x6e, abx: 0x7e, acc: 0x6a, imp: 0x6a, zpg: 0x66, zpx: 0x76 },
        rti: { imp: 0x40 },
        rts: { imp: 0x60 },
        sbc: { abs: 0xed, abx: 0xfd, aby: 0xf9, imm: 0xe9,
            iny: 0xf1, inx: 0xe1, zpg: 0xe5, zpx: 0xf5 },
        sec: { imp: 0x38 },
        sed: { imp: 0xf8 },
        sei: { imp: 0x78 },
        sta: { abs: 0x8d, abx: 0x9d, aby: 0x99,
            iny: 0x91, inx: 0x81, zpg: 0x85, zpx: 0x95 },
        stx: { abs: 0x8e, zpg: 0x86, zpy: 0x96 },
        sty: { abs: 0x8c, zpg: 0x84, zpx: 0x94 },
        tax: { imp: 0xaa },
        tay: { imp: 0xa8 },
        tsx: { imp: 0xba },
        txa: { imp: 0x8a },
        txs: { imp: 0x9a },
        tya: { imp: 0x98 },
        // extended instructions
        slo: { abs: 0x0f, abx: 0x1f, aby: 0x1b,
            zpg: 0x07, zpx: 0x17, inx: 0x03, iny: 0x13 },
        rla: { abs: 0x2f, abx: 0x3f, aby: 0x3b,
            zpg: 0x27, zpx: 0x37, inx: 0x23, iny: 0x33 },
        sre: { abs: 0x4f, abx: 0x5f, aby: 0x5b,
            zpg: 0x47, zpx: 0x57, inx: 0x43, iny: 0x53 },
        rra: { abs: 0x6f, abx: 0x7f, aby: 0x7b,
            zpg: 0x67, zpx: 0x77, inx: 0x63, iny: 0x73 },
        sax: { abs: 0x8f, zpg: 0x87, zpy: 0x97, inx: 0x83 },
        lax: { abs: 0xaf, aby: 0xbf, zpg: 0xa7,
            zpy: 0xb7, inx: 0xa3, iny: 0xb3 },
        dcp: { abs: 0xcf, abx: 0xdf, aby: 0xdb,
            zpg: 0xc7, zpx: 0xd7, inx: 0xc3, iny: 0xd3 },
        isc: { abs: 0xef, abx: 0xff, aby: 0xfb,
            zpg: 0xe7, zpx: 0xf7, inx: 0xe3, iny: 0xf3 },
        alr: { imm: 0x4b },
        arr: { imm: 0x6b },
        axs: { imm: 0xcb },
        tas: { aby: 0x9b },
        shy: { abx: 0x9c },
        shx: { aby: 0x9e },
        ahx: { aby: 0x9f, iny: 0x93 },
        anc: { imm: 0x2b },
        las: { aby: 0xbb },
        // there's also a handful of extra nop's, skb's, and skw's:
        //   nop: 1a, 3a, 5a, 7a, da, fa
        //   skb: 80 82 89 c2 e2 04 14 34 44 54 64 d4 f4
        //   skw: 0c 1c 3c 5c 7c dc fc
        // and an extra immediate sbc at eb
        // plus some hlt/kil instructions that we don't bother with.
    });
})(Cpu || (Cpu = {}));
