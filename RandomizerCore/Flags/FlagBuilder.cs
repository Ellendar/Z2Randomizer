using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Z2Randomizer.RandomizerCore.Flags;

/// <summary>
/// Extremely crude builder pattern implementation for flags backed by a bitarray. If I cared that much about efficency this would 
/// be reimplemented as a linked list based implementation similar to how .NET's StringBuilder works.
/// Thankfully, I don't.
/// </summary>
public class FlagBuilder
{
    public List<bool> bits;
    public static readonly string ENCODING_TABLE = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz1234567890!@#+";
    public FlagBuilder()
    {
        bits = new List<bool>();
    }

    public FlagBuilder Append(bool value)
    {
        bits.Add(value);
        return this;
    }
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
    public FlagBuilder Append(byte value, int extent)
    {
        BitArray argBits = new BitArray(value);
        //Switch the endianness so we only have to care about the 
        for (int i = 7; i > 8 - extent; i++)
        {
            bits.Add(argBits[i]);
        }
        return this;
    }
    public FlagBuilder Append(byte? value, int extent)
    {
        return Append(value ?? extent, extent + 1);
    }
    public FlagBuilder Append(int value, int extent)
    {
        if(value >= extent)
        {
            throw new ArgumentException("Value is greater than extent in FlagBuilder.Append(int, int)");
        }
        BitArray argBits = new BitArray(new int[] {value});
        for (int i = (int)Math.Log(extent - 1, 2); i >= 0; i--)
        {
            bits.Add(argBits[i]);
        }
        return this;
    }
    public FlagBuilder Append(int? value, int extent)
    {
        return Append(value ?? extent, extent + 1);
    }
    //There is 100% a more elegant way to do this, but I gave up and hit it with a hammer.
    public override string ToString()
    {
        StringBuilder flags = new StringBuilder();
        int bitCount = 0;
        while (bitCount < bits.Count)
        {
            int index = 0;
            if (bitCount < bits.Count && bits[bitCount])
            {
                index += 0b00100000;
            }
            if (++bitCount < bits.Count && bits[bitCount])
            {
                index += 0b00010000;
            }
            if (++bitCount < bits.Count && bits[bitCount])
            {
                index += 0b00001000;
            }
            if (++bitCount < bits.Count && bits[bitCount])
            {
                index += 0b00000100;
            }
            if (++bitCount < bits.Count && bits[bitCount])
            {
                index += 0b00000010;
            }
            if (++bitCount < bits.Count && bits[bitCount])
            {
                index += 0b00000001;
            }
            bitCount++;
            flags.Append(ENCODING_TABLE[index]);
        }
        return flags.ToString();
    }

    public int BitCount()
    {
        return bits.Count;
    }
}
