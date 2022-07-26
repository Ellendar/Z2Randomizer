using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    class Util
    {
        private const int textEndByte = 0xFF;
        public static byte ReverseByte(byte b)
        {
            return (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
        }

        public static List<char> ToGameText(string s2, Boolean endByte)
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
            int tempw = p1.World;
            p1.World = p2.World;
            p2.World = tempw;

            tempw = p1.Map;
            p1.Map = p2.Map;
            p2.Map = tempw;

            tempw = p1.PalNum;
            p1.PalNum = p2.PalNum;
            p2.PalNum = tempw;

            Town tempTown = p1.TownNum;
            p1.TownNum = p2.TownNum;
            p2.TownNum = tempTown;

            Item i = p1.item;
            p1.item = p2.item;
            p2.item = i;


        }

        public class MyEqualityComparer : IEqualityComparer<byte[]>
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
    }

    
}
