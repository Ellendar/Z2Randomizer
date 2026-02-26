using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Z2Randomizer.RandomizerCore.Flags;

/// <summary>
/// Sequential reader for decoding flag strings produced by <see cref="FlagBuilder"/>.
/// </summary>
/// <remarks>
/// The reader expands the encoded flag string into a linear bit stream and
/// consumes bits in order as typed values. Consumers are responsible for
/// reading values in the same order and with the same extents used during encoding.
/// </remarks>
public class FlagReader
{
    private static readonly byte[] Base64DecodeTable = CreateDecodeTable();

    private static byte[] CreateDecodeTable()
    {
        var table = new byte[128];

        var alphabet = FlagBuilder.ENCODING_TABLE;
        for (int i = 0; i < alphabet.Length; i++)
        {
            table[alphabet[i]] = (byte)i;
        }

        return table;
    }

    /// <summary>
    /// List of decoded bits as bools.
    /// </summary>
    private readonly List<bool> bits = [];

    /// <summary>
    /// Current read position within <see cref="bits"/>.
    /// </summary>
    private int index = 0;

    /// <summary>
    /// Initializes a new <see cref="FlagReader"/> from an encoded flag string.
    /// </summary>
    /// <param name="flags">
    /// The encoded flag string produced by <see cref="FlagBuilder.ToString"/>.
    /// </param>
    /// <remarks>
    /// Each character in the string is decoded into six bits using the same
    /// encoding table as <see cref="FlagBuilder"/>.
    ///
    /// The character '$' is treated as equivalent to '+' for command-line compatibility.
    /// </remarks>
    public FlagReader(string flags)
    {
        // NOTE: The '$' was replaced with '+' to help with command line usage
        flags = flags.Replace('$', '+');
        foreach (byte decode in flags.Select(character => Base64DecodeTable[character]))
        {
            bits.Add((decode & 0b100000) != 0);
            bits.Add((decode & 0b010000) != 0);
            bits.Add((decode & 0b001000) != 0);
            bits.Add((decode & 0b000100) != 0);
            bits.Add((decode & 0b000010) != 0);
            bits.Add((decode & 0b000001) != 0);
        }
    }

    /// <summary>
    /// Reads a fixed number of bits from the stream and returns them as an integer.
    /// </summary>
    /// <param name="count">
    /// The number of bits to read (1–8).
    /// </param>
    /// <returns>
    /// The decoded integer value represented by the bits.
    /// </returns>
    /// <remarks>
    /// Bits are interpreted most-significant first, matching the encoding behavior
    /// of <see cref="FlagBuilder"/>.
    /// </remarks>
    private int Take(int count)
    {
        switch (count)
        {
            case > 8:
                throw new ArgumentOutOfRangeException(nameof(count), "Flag fields are limited to 8 bits per flag");
            case 0:
                throw new ArgumentOutOfRangeException(nameof(count), "Unable to take zero or fewer bits");
        }

        if (count + index > bits.Count)
        {
            throw new IndexOutOfRangeException("Insufficient flag bits left to read.");
        }

        var end = index + count;
        var bitRange = bits[index..end];
        int ret = 0;
        for (int i = 0; i < count; i++) {
            ret |= (bitRange[i] ? 1 : 0) << (count - i - 1);
        }
        index += count;
        return ret;
    }

    /// <summary>
    /// Reads a single bit and returns it as a boolean value.
    /// </summary>
    public bool ReadBool()
    {
        int result = Take(1);
        return result != 0;
    }

    /// <summary>
    /// Reads a nullable boolean value encoded in two bits.
    /// </summary>
    public bool? ReadNullableBool()
    {
        int take = Take(2);
        return take switch
        {
            0 => false,
            1 => true,
            2 => null,
            _ => throw new InvalidDataException("Invalid nullable bool value")
        };
    }

    /// <summary>
    /// Reads an integer value using a fixed number of bits.
    /// 
    /// The number of bits read will be the minimum number of bits needed
    /// to represent values in the range <c>[minimum, minimum + extent - 1]</c>.
    /// </summary>
    /// <param name="extent">
    /// The number of distinct values that may be represented.
    /// </param>
    /// <param name="minimum">
    /// Optional minimum value added to the decoded result.
    /// </param>
    /// <returns>
    /// The decoded integer value.
    /// </returns>
    /// <remarks>
    /// <example>
    /// Examples (with <c>minimum = 0</c>):
    /// <list type="bullet">
    /// <item><description>
    /// <c>extent = 4</c> → values 0–3, uses 2 bits
    /// </description></item>
    /// <item><description>
    /// <c>extent = 8</c> → values 0–7, uses 3 bits
    /// </description></item>
    /// </list>
    /// </example>
    ///
    /// The same <paramref name="extent"/> and <paramref name="minimum"/> values
    /// must be used during encoding to ensure correct decoding.
    /// </remarks>
    public int ReadInt(int extent, int? minimum = null)
    {
        var min = minimum ?? 0;
        return Take(BitOperations.Log2((uint)extent - 1) + 1) + min;
    }

    /// <summary>
    /// Reads a nullable integer value using a fixed number of bits.
    /// 
    /// The number of bits read will be the minimum number of bits needed
    /// to represent values in the range
    /// <c>[minimum, minimum + extent - 1]</c>, where the value
    /// <c>minimum + extent</c> is reserved as a sentinel representing <c>null</c>.
    /// </summary>
    /// <param name="extent">
    /// The number of distinct non-null values that may be represented.
    /// </param>
    /// <param name="minimum">
    /// Optional minimum value added to non-null decoded results.
    /// </param>
    /// <returns>
    /// The decoded integer value, or <c>null</c>.
    /// </returns>
    /// <remarks>
    /// <example>
    /// Examples (with <c>minimum = 0</c>):
    /// <list type="bullet">
    /// <item><description>
    /// <c>extent = 3</c> → values 0–2 or null, uses 2 bits
    /// </description></item>
    /// <item><description>
    /// <c>extent = 7</c> → values 0–6 or null, uses 3 bits
    /// </description></item>
    /// </list>
    /// </example>
    ///
    /// The same <paramref name="extent"/> and <paramref name="minimum"/> values
    /// must be used during encoding to ensure correct decoding.
    /// </remarks>
    public int? ReadNullableInt(int extent, int? minimum = null)
    {
        var min = minimum ?? 0;
        int result = (byte)Take(BitOperations.Log2((uint)extent) + 1);
        if (result == extent)
        {
            return null;
        }
        return result + min;
    }
}
