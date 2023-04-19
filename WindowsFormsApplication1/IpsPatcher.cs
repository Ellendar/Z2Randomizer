using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer;

/// <summary>
/// Applies IPS Patches
/// 
/// Ported from code provided by Noobish Noobsicle on the SMWCentral forums
/// https://www.smwcentral.net/?p=viewthread&t=18445&page=1&pid=274388#p274388
/// </summary>
internal class IpsPatcher
{
    public void Patch(byte[] romData, string patchName)
    {
        //FileStream romstream = new FileStream(romname, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        MemoryStream romstream = new MemoryStream(romData);
        FileStream ipsstream = new FileStream(patchName, FileMode.Open, FileAccess.Read);
        int lint = (int)ipsstream.Length;
        byte[] ipsbyte = new byte[ipsstream.Length];
        //byte[] romData = new byte[romstream.Length];
        IAsyncResult romresult;
        IAsyncResult ipsresult = ipsstream.BeginRead(ipsbyte, 0, lint, null, null);
        ipsstream.EndRead(ipsresult);
        int ipson = 5;
        int totalrepeats = 0;
        int offset = 0;
        bool keepgoing = true;
        //////////////////End Init code
        //////////////////Start main code
        while (keepgoing == true)
        {
            offset = ipsbyte[ipson] * 0x10000 + ipsbyte[ipson + 1] * 0x100 + ipsbyte[ipson + 2];
            ipson++;
            ipson++;
            ipson++;
            /////////////split between repeating byte mode and standard mode
            if (ipsbyte[ipson] * 256 + ipsbyte[ipson + 1] == 0)
            {
                ////////////repeating byte mode
                ipson++;
                ipson++;
                totalrepeats = ipsbyte[ipson] * 256 + ipsbyte[ipson + 1];
                ipson++;
                ipson++;
                byte[] repeatbyte = new byte[totalrepeats];
                for (int ontime = 0; ontime < totalrepeats; ontime++)
                    repeatbyte[ontime] = ipsbyte[ipson];
                romstream.Seek(offset, SeekOrigin.Begin);
                romresult = romstream.BeginWrite(repeatbyte, 0, totalrepeats, null, null);
                romstream.EndWrite(romresult);
                ipson++;
            }
            else
            {
                ////////////standard mode
                totalrepeats = ipsbyte[ipson] * 256 + ipsbyte[ipson + 1];
                ipson++;
                ipson++;
                romstream.Seek(offset, SeekOrigin.Begin);
                romresult = romstream.BeginWrite(ipsbyte, ipson, totalrepeats, null, null);
                romstream.EndWrite(romresult);
                ipson = ipson + totalrepeats;
            }
            /////////////Test For "EOF"
            if (ipsbyte[ipson] == 69 && ipsbyte[ipson + 1] == 79 && ipsbyte[ipson + 2] == 70)
                keepgoing = false;
        }
        romstream.Close();
        ipsstream.Close();
    }
}
