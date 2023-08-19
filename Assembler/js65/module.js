import { z } from './deps/deno.land/x/zod@v3.21.4/mod.js';
import { ExprZ } from './expr.js';
import { base64 } from './deps/deno.land/x/b64@1.1.27/src/base64.js';
// export interface Substitution {
//   offset: number;
//   size: number;
//   expr: Expr;
// }
const SubstitutionZ = z.object({
    /** Offset into the chunk to substitute the expression into. */
    offset: z.number(),
    /** Number of bytes to substitute. */
    size: z.number(),
    /** Expression to substitute. */
    expr: ExprZ,
});
// export type Module = z.infer<typeof Module>;
// Default is "allow"
const OverwriteModeZ = z.enum(['forbid', 'allow', 'require']);
// export interface Chunk<T extends number[]|Uint8Array|string> {
//   name?: string;
//   segments: readonly string[];
//   org?: number;
//   data: T;
//   subs?: Substitution[];
//   asserts?: Expr[];
//   overwrite?: OverwriteMode;
// }
const BaseChunk = z.object({
    /** Human-readable identifier. */
    name: z.string().optional(),
    /** Which segments this chunk may be located in. */
    segments: z.array(z.string()),
    /** Absolute address of the start of the chunk, if not relocatable. */
    org: z.number().optional(),
    /** Substitutions to insert into the data. */
    subs: z.optional(z.array(SubstitutionZ)),
    /** Assertions within this chunk. Each expression must be nonzero. */
    asserts: z.optional(z.array(ExprZ)),
    /** How overwriting previously-written fixed-position data is handled. */
    overwrite: z.optional(OverwriteModeZ), // NOTE: only set programmatically?
});
const ChunkNumZ = BaseChunk.extend({
    /**
     * Data for the chunk, either a Uint8Array or a Base64-encoded string.
     * NOTE: While building this is a number array.  When serialized to disk, it
     * is a base64-encoded string.  When linking, it's a Uint8Array.
     */
    data: z.array(z.number()),
});
const ChunkZ = BaseChunk.extend({
    data: z.string().transform((s) => new Uint8Array(base64.toArrayBuffer(s)))
});
// export interface Symbol {
//   export?: string;
//   expr?: Expr;
// }
const SymbolZ = z.object({
    /** Name to export this symbol as, for importing into other objects. */
    export: z.string().optional(),
    // /** Index of the chunk this symbol is defined in. */
    // chunk?: number; // TODO - is this actually necessary?
    // /** Byte offset into the chunk for the definition. */
    // offset?: number;
    /** Value of the symbol. */
    expr: ExprZ.optional(),
});
// export interface Segment {
//   name: string;
//   bank?: number;
//   size?: number;
//   offset?: number;
//   memory?: number;
//   addressing?: number;
//   default?: boolean;
//   free?: Array<readonly [number, number]>;
// }
const SegmentZ = z.object({
    /** Name of the segment, as used in .segment directives. */
    name: z.string(),
    /** Bank for the segment. */
    bank: z.number().optional(),
    /** Segment size in bytes. */
    size: z.number().optional(),
    /** Offset of the segment in the rom image. */
    offset: z.number().optional(),
    /** Memory location of the segment in the CPU. */
    memory: z.number().optional(),
    /** Address size. */
    addressing: z.number().optional(),
    /** Address size. */
    fill: z.number().optional(),
    /** True if the segment should be written to the output file. */
    out: z.boolean().optional(),
    /** Name of the segment that this should be placed inside. */
    overlay: z.string().optional(),
    /** True if this segment is the "default" segment to use if no segment is defined */
    default: z.boolean().optional(),
    /** Unallocated ranges (org), half-open [a, b). */
    free: z.array(z.array(z.number())).optional(),
});
// deno-lint-ignore no-namespace
export var Segment;
(function (Segment) {
    function merge(a, b) {
        const seg = { ...a, ...b };
        const free = [...(a.free || []), ...(b.free || [])];
        if (free.length)
            seg.free = free;
        return seg;
    }
    Segment.merge = merge;
    function includesOrg(s, addr) {
        if (s.memory == null || s.size == null)
            return false;
        return addr >= s.memory && addr < (s.memory + s.size);
    }
    Segment.includesOrg = includesOrg;
})(Segment || (Segment = {}));
// export interface Module {
//   chunks?: Chunk<Uint8Array>[],
//   symbols?: Symbol[],
//   segments?: Segment[],
// }
export const ModuleZ = z.object({
    /** Filename if loaded from a file, otherwise a user provided name */
    name: z.string().optional(),
    /** All chunks, in a determinstic (indexable) order. */
    chunks: z.optional(z.array(ChunkZ)),
    /** All symbols, in a deterministic (indexable) order. */
    symbols: z.optional(z.array(SymbolZ)),
    /** All segments.  Indexed by name, but we don't use a map. */
    segments: z.optional(z.array(SegmentZ)),
});
