using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RandomizerCore;

/// <summary>
/// Applies IPS Patches
/// </summary>
internal class IpsPatcher
{
    static readonly IReadOnlyList<byte> PatchSig = Encoding.ASCII.GetBytes("PATCH");
    static readonly IReadOnlyList<byte> EofSig = Encoding.ASCII.GetBytes("EOF");

    public static void Patch(byte[] romData, byte[] ipsData, bool expandRom = false)
    {
        Debug.Assert(PatchSig.SequenceEqual(new ArraySegment<byte>(ipsData, 0, PatchSig.Count)));

        int ipsOffs = PatchSig.Count;
        while (!EofSig.SequenceEqual(new ArraySegment<byte>(ipsData, ipsOffs, EofSig.Count)))
        {
            int tgtOffs = ((int)ipsData[ipsOffs] << 16) 
                | ((int)ipsData[ipsOffs + 1] << 8) 
                | ipsData[ipsOffs + 2];
            ipsOffs += 3;

            int size = ((int)ipsData[ipsOffs] << 8) | ipsData[ipsOffs + 1];
            ipsOffs += 2;

            byte? fillValue = null;
            if (size == 0)
            {
                size = ((int)ipsData[ipsOffs] << 8) | ipsData[ipsOffs + 1];
                ipsOffs += 2;

                fillValue = ipsData[ipsOffs++];
            }

            if (expandRom && tgtOffs + size > ROM.VanillaChrRomOffs)
            {
                if (tgtOffs < ROM.VanillaChrRomOffs)
                {
                    int segSize = ROM.VanillaChrRomOffs - tgtOffs;
                    if (fillValue is not null)
                        Array.Fill<byte>(romData, (byte)fillValue, tgtOffs, segSize);
                    else
                    {
                        Array.Copy(ipsData, ipsOffs, romData, tgtOffs, segSize);
                        ipsOffs += segSize;
                    }

                    tgtOffs += segSize;
                    size -= segSize;
                }

                tgtOffs += ROM.ChrRomOffs - ROM.VanillaChrRomOffs;
            }

            if (fillValue is not null)
                Array.Fill<byte>(romData, (byte)fillValue, tgtOffs, size);
            else
            {
                Array.Copy(ipsData, ipsOffs, romData, tgtOffs, size);
                ipsOffs += size;
            }
        }
    }

    public static void Patch(byte[] romData, string patchName, bool expandRom = false)
    {
        byte[]? ipsData = null;
        using (var ipsStream = new FileStream(patchName, FileMode.Open, FileAccess.Read))
        {
            ipsData = new byte[checked((int)ipsStream.Length)];
            ipsStream.ReadAtLeast(new(ipsData), ipsData.Length);
        }

        Patch(romData, ipsData, expandRom);
    }
}
