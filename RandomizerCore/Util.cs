using System;
using System.Collections.Generic;
using System.Linq;
using Z2Randomizer.Core.Overworld;

namespace Z2Randomizer.Core;

public class Util
{
    private const int textEndByte = 0xFF;
    public static byte ReverseByte(byte b)
    {
        return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
    }

    public static List<char> ToGameText(string s2, bool endByte)
    {
        s2 = s2.ToUpper();
        List<char> s = s2.ToCharArray().ToList();
        for (int i = 0; i < s.Count; i++)
        {
            if (s[i] >= '0' && s[i] <= '9')
                s[i] += (char)(0xd0 - '0');
            else if (s[i] >= 'A' && s[i] <= 'Z')
                s[i] += (char)(0xda - 'A');
            else if (s[i] == '.')
                s[i] = (char)0xcf;
            else if (s[i] == '/')
                s[i] = (char)0xce;
            else if (s[i] == ',')
                s[i] = (char)0x9c;
            else if (s[i] == '!')
                s[i] = (char)0x36;
            else if (s[i] == '?')
                s[i] = (char)0x34;
            else if (s[i] == '*')
                s[i] = (char)0x32;
            else if (s[i] == ' ')
                s[i] = (char)0xf4;
            else if (s[i] == '\n')
                s[i] = (char)0xfd;
            else if (s[i] == '$')
                s[i] = (char)0xfd;
        }
        if (endByte)
        {
            s.Add((char)textEndByte);
        }

        return s;
    }
    public static void Swap(Location p1, Location p2)
    {
        (p2.World, p1.World) = (p1.World, p2.World);
        (p2.Map, p1.Map) = (p1.Map, p2.Map);
        (p2.PalaceNumber, p1.PalaceNumber) = (p1.PalaceNumber, p2.PalaceNumber);

        (p2.TownNum, p1.TownNum) = (p1.TownNum, p2.TownNum);
        (p2.Item, p1.Item) = (p1.Item, p2.Item);
        (p2.Name, p1.Name) = (p1.Name, p2.Name);
    }
    public static string ByteArrayToHexString(byte[] bytes)
    {
        string hex = BitConverter.ToString(bytes);
        return hex.Replace("-", "");
    }

    /// <summary>
    /// Comparer where two byte arrays are equal iff their length is the same, and at each index, the arrays contain the same value
    /// </summary>
    public class StandardByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(byte[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }

    public static IEqualityComparer<byte[]> byteArrayEqualityComparer = new StandardByteArrayEqualityComparer();
} 
