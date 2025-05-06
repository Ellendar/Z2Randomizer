using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Z2Randomizer.RandomizerCore.Flags;

public class FlagReader
{

    //This is a lazy backwards implementation Digshake's base64 encoding system.
    private static Dictionary<char, int> BASE64_DECODE = new Dictionary<char, int>(64)
    {
        {'A', 0},
        {'B', 1},
        {'C', 2},
        {'D', 3},
        {'E', 4},
        {'F', 5},
        {'G', 6},
        {'H', 7},
        {'J', 8},
        {'K', 9},
        {'L', 10},
        {'M', 11},
        {'N', 12},
        {'O', 13},
        {'P', 14},
        {'Q', 15},
        {'R', 16},
        {'S', 17},
        {'T', 18},
        {'U', 19},
        {'V', 20},
        {'W', 21},
        {'X', 22},
        {'Y', 23},
        {'Z', 24},

        {'a', 25},
        {'b', 26},
        {'c', 27},
        {'d', 28},
        {'e', 29},
        {'f', 30},
        {'g', 31},
        {'h', 32},
        {'i', 33},
        {'j', 34},
        {'k', 35},
        {'m', 36},
        {'n', 37},
        {'o', 38},
        {'p', 39},
        {'q', 40},
        {'r', 41},
        {'s', 42},
        {'t', 43},
        {'u', 44},
        {'v', 45},
        {'w', 46},
        {'x', 47},
        {'y', 48},
        {'z', 49},

        {'1', 50},
        {'2', 51},
        {'3', 52},
        {'4', 53},
        {'5', 54},
        {'6', 55},
        {'7', 56},
        {'8', 57},
        {'9', 58},
        {'0', 59},
        {'!', 60},
        {'@', 61},
        {'#', 62},
        {'+', 63},
    };

    private readonly List<bool> bits = [];
    public int index = 0;
    public FlagReader(string flags)
    {
        // NOTE: The '$' was replaced with '+' to help with command line usage
        flags = flags.Replace('$', '+');
        foreach (var decode in flags.Select(character => BASE64_DECODE[character]))
        {
            bits.Add((decode & 0b100000) > 0);
            bits.Add((decode & 0b010000) > 0);
            bits.Add((decode & 0b001000) > 0);
            bits.Add((decode & 0b000100) > 0);
            bits.Add((decode & 0b000010) > 0);
            bits.Add((decode & 0b000001) > 0);
        }
    }

    private int Take(int count)
    {
        if (count > 8)
        {
            throw new ArgumentOutOfRangeException("Flag fields are limited to 8 bits per flag");
        }
        if (count == 0)
        {
            throw new ArgumentOutOfRangeException("Unable to take zero or fewer bits");
        }
        if (count + index > bits.Count)
        {
            throw new IndexOutOfRangeException("Insufficient flag bits left to read.");
        }

        bool[] bitRange = bits.GetRange(index, count).ToArray();
        Array.Reverse(bitRange);
        bitRange.Reverse();
        BitArray bitArray = new BitArray(bitRange);
        int[] ints = new int[1];
        bitArray.CopyTo(ints, 0);
        index += count;
        int retval = ints[0];
        return retval;
    }

    public bool ReadBool()
    {
        int result = Take(1);
        return result != 0;
    }
    public bool? ReadNullableBool()
    {
        int take = Take(2);
        return take switch
        {
            0 => false,
            1 => true,
            2 => null,
            _ => throw new Exception("Invalid nullable bool value")
        };
    }
    public byte ReadByte(int extent)
    {
        return (byte)Take((int)Math.Log(extent - 1, 2) + 1);
    }
    public byte? ReadNullableByte(int extent)
    {
        int result = (byte)Take((int)Math.Log(extent, 2) + 1);
        if (result == extent)
        {
            return null;
        }
        return (byte?)result;
    }
    public int ReadInt(int extent)
    {
        return Take((int)Math.Log(extent - 1, 2) + 1);
    }
    public int? ReadNullableInt(int extent)
    {
        int result = (byte)Take((int)Math.Log(extent, 2) + 1);
        if (result == extent)
        {
            return null;
        }
        return (int?)result;
    }

    public T ReadEnum<T>() where T : struct, Enum
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("Invalid ReadEnum on non-enumeration type");
        }

        int limit = Enum.GetValues(typeof(T)).Length - 1;

        int take = (byte)Take((int)Math.Log(limit, 2) + 1);
        return (T)Enum.GetValues<T>().GetValue(take)!;
    }

    public T? ReadNullableEnum<T>() where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("Invalid ReadEnum on non-enumeration type");
        }

        int limit = Enum.GetValues(typeof(T)).Length;

        int take = (byte)Take((int)Math.Log(limit, 2) + 1);
        if(take == limit)
        {
            return null;
        }
        return (T)Enum.ToObject(typeof(T), take);
    }
}
