using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Z2Randomizer.RandomizerCore.Flags;

/// <summary>
/// Crude builder-pattern implementation for constructing flag strings
/// backed by a bit sequence. Bits are appended sequentially and later encoded
/// into a compact, character-based representation.
/// </summary>
/// <remarks>
/// This implementation prioritizes simplicity over performance. Internally,
/// bits are stored in a <see cref="List{Boolean}"/> and encoded in 6-bit chunks
/// using a custom character table.
/// </remarks>
public class FlagBuilder
{
    /// <summary>
    /// The underlying bit storage for the flag being constructed.
    /// Bits are appended in logical order and later encoded into characters.
    /// </summary>
    public List<bool> bits;

    public static readonly string ENCODING_TABLE = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz1234567890!@#+";

    public FlagBuilder()
    {
        bits = new List<bool>(400);
    }

    /// <summary>
    /// Appends a single boolean value as one bit.
    /// </summary>
    /// <param name="value">
    /// The value to append (<c>false</c> = 0, <c>true</c> = 1).
    /// </param>
    /// <returns>
    /// The current <see cref="FlagBuilder"/> instance for fluent chaining.
    /// </returns>
    public FlagBuilder Append(bool value)
    {
        bits.Add(value);
        return this;
    }

    /// <summary>
    /// Appends a nullable boolean value encoded as two bits.
    /// </summary>
    /// <param name="value">
    /// The nullable boolean value to append.
    /// </param>
    /// <returns>
    /// The current <see cref="FlagBuilder"/> instance for fluent chaining.
    /// </returns>
    public FlagBuilder Append(bool? value)
    {
        int result = value switch
        {
            false => 0,
            true => 1,
            null => 2
        };
        Append(result, 3);
        return this;
    }

    /// <summary>
    /// Appends an integer value using a fixed number of bits.
    /// 
    /// The number of bits written will be the minimum number of bits needed
    /// to represent values in the range <c>[minimum, minimum + extent - 1]</c>.
    /// </summary>
    /// <param name="val">
    /// The value to encode.
    /// </param>
    /// <param name="extent">
    /// The number of distinct values that may be represented.
    /// </param>
    /// <param name="minimum">
    /// Optional minimum value used to rebase the encoded value.
    /// </param>
    /// <returns>
    /// The current <see cref="FlagBuilder"/> instance for fluent chaining.
    /// </returns>
    /// <remarks>
    /// The encoded value is rebased by subtracting <paramref name="minimum"/>
    /// before writing bits. The same <paramref name="extent"/> and
    /// <paramref name="minimum"/> values must be used during decoding.
    ///
    /// <example>
    /// Examples (with <c>minimum = 0</c>):
    /// <list type="bullet">
    /// <item><description>
    /// <c>extent = 4</c> → values 0–3 possible, uses 2 bits
    /// </description></item>
    /// <item><description>
    /// <c>extent = 8</c> → values 0–7 possible, uses 3 bits
    /// </description></item>
    /// </list>
    /// </example>
    /// </remarks>
    public FlagBuilder Append(int val, int extent, int? minimum = null)
    {
        var min = minimum ?? 0;
        // Subtract value - min to rebase the value to zero.
        // We add min when extracting the bit when converting back to the value
        int value = val - min;
        if (value >= extent)
        {
            throw new ArgumentException("Value is greater than extent in FlagBuilder.Append(int, int)");
        }

        BitArray argBits = new BitArray([value]);
        for (int i = BitOperations.Log2((uint)extent - 1); i >= 0; i--)
        {
            bits.Add(argBits[i]);
        }
        return this;
    }

    /// <summary>
    /// Appends a nullable integer value using a fixed number of bits.
    /// 
    /// The number of bits written will be the minimum number of bits needed
    /// to represent values in the range
    /// <c>[minimum, minimum + extent - 1]</c>, with one additional reserved
    /// encoding used to represent <c>null</c>.
    /// </summary>
    /// <param name="value">
    /// The nullable integer value to encode.
    /// </param>
    /// <param name="extent">
    /// The number of distinct non-null values that may be represented.
    /// </param>
    /// <param name="minimum">
    /// Optional minimum value used to rebase non-null values.
    /// </param>
    /// <returns>
    /// The current <see cref="FlagBuilder"/> instance for fluent chaining.
    /// </returns>
    /// <remarks>
    /// Internally, values are rebased by subtracting <paramref name="minimum"/>.
    /// Non-null values are encoded in the range <c>[0, extent - 1]</c>.
    /// The value <c>extent</c> is reserved as a sentinel representing <c>null</c>.
    ///
    /// <example>
    /// Examples (with <c>minimum = 0</c>):
    /// <list type="bullet">
    /// <item><description>
    /// <c>extent = 3</c> → values 0–2 or null possible, uses 2 bits
    /// </description></item>
    /// <item><description>
    /// <c>extent = 7</c> → values 0–6 or null possible, uses 3 bits
    /// </description></item>
    /// </list>
    /// </example>
    ///
    /// The same <paramref name="extent"/> and <paramref name="minimum"/> values
    /// must be used during decoding to ensure correct decoding.
    /// </remarks>
    public FlagBuilder Append(int? value, int extent, int? minimum = null)
    {
        return Append((value - minimum) ?? extent, extent + 1);
    }

    /// <summary>
    /// Converts the accumulated bits into a compact encoded flag string.
    /// </summary>
    /// <returns>
    /// A string where each character represents a 6-bit chunk of the bit stream.
    /// </returns>
    /// <remarks>
    /// Bits are consumed in groups of six, with earlier bits becoming
    /// more significant within each character.
    /// The final character may represent fewer than six bits if the total
    /// bit count is not divisible by six.
    /// </remarks>
    public override string ToString()
    {
        StringBuilder flags = new StringBuilder();
        int bitCount = 0;
        while (bitCount < bits.Count)
        {
            int index = 0;
            for (int i = 5; i >= 0 && bitCount < bits.Count; i--, bitCount++)
            {
                if (bits[bitCount])
                {
                    index |= 1 << i;
                }
            }
            flags.Append(ENCODING_TABLE[index]);
        }
        return flags.ToString();
    }

    /// <summary>
    /// Returns the total number of bits currently stored in the builder.
    /// </summary>
    /// <returns>
    /// The count of appended bits.
    /// </returns>
    public int BitCount()
    {
        return bits.Count;
    }
}
