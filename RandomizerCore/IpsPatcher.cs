namespace RandomizerCore;

/// <summary>
/// Applies IPS Patches
/// 
/// Ported from code provided by Noobish Noobsicle on the SMWCentral forums
/// https://www.smwcentral.net/?p=viewthread&t=18445&page=1&pid=274388#p274388
/// </summary>
internal class IpsPatcher
{
    public static void Patch(byte[] romData, byte[] patch)
    {
        var ipsbyte = patch;
        int ipson = 5;
        int totalrepeats = 0;
        int offset = 0;
        bool keepgoing = true;
        //////////////////End Init code
        //////////////////Start main code
        while (keepgoing)
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
                repeatbyte.CopyTo(romData, offset);
                ipson++;
            }
            else
            {
                ////////////standard mode
                totalrepeats = ipsbyte[ipson] * 256 + ipsbyte[ipson + 1];
                ipson++;
                ipson++;
                var end = ipson + totalrepeats;
                ipsbyte[ipson..end].CopyTo(romData, offset);
                ipson = end;
            }
            /////////////Test For "EOF"
            if (ipsbyte[ipson] == 69 && ipsbyte[ipson + 1] == 79 && ipsbyte[ipson + 2] == 70)
                keepgoing = false;
        }
    }
}
