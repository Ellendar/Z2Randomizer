using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using Z2Randomizer.Core;
using Z2Randomizer.Core.Sidescroll;

namespace Z2Randomizer.Tests;

[TestClass]
public class RomDump
{
    [TestMethod]
    public void DumpRooms()
    {
        ROM ROMData = new ROM("C:\\emu\\NES\\roms\\z2r-item_room_fixes\\Z2_755165440_g0$e6W$3ZAXqioTbDhg$tcVqv@sukyJAWh-P6-39.nes");
        int[] connAddr = new int[] { 0x1072B, 0x12208, 0x1472B };
        int[] side = new int[] { 0x10533, 0x12010, 0x14533 };
        int[] enemy = new int[] { 0x105b1, 0x1208E, 0x145b1 };
        int[] bit = new int[] { 0x17ba5, 0x17bc5, 0x17be5 };
        StringBuilder sb = new StringBuilder("[");
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 63; i++)
            {
                int addr = connAddr[j] + i * 4;
                Byte[] connectBytes = new Byte[4];
                for (int k = 0; k < 4; k++)
                {
                    connectBytes[k] = ROMData.GetByte(addr + k);

                }
                Room r;
                int sideViewPtr = (ROMData.GetByte(side[j] + i * 2) + (ROMData.GetByte(side[j] + 1 + i * 2) << 8)) + 0x8010;
                if (j == 2)
                {
                    sideViewPtr = (ROMData.GetByte(side[j] + i * 2) + (ROMData.GetByte(side[j] + 1 + i * 2) << 8)) + 0xC010;
                }
                int sideViewLength = ROMData.GetByte(sideViewPtr);
                Byte[] sideView = ROMData.GetBytes(sideViewPtr, sideViewPtr + sideViewLength);

                int enemyPtr = ROMData.GetByte(enemy[j] + i * 2) + (ROMData.GetByte(enemy[j] + 1 + i * 2) << 8) + 0x98b0;
                if (j == 2)
                {
                    enemyPtr = ROMData.GetByte(enemy[j] + i * 2) + (ROMData.GetByte(enemy[j] + 1 + i * 2) << 8) + 0xd8b0;
                }

                int enemyLength = ROMData.GetByte(enemyPtr);
                Byte[] enemies = ROMData.GetBytes(enemyPtr, enemyPtr + enemyLength);

                Byte bitmask = ROMData.GetByte(bit[j] + i / 2);

                if (i % 2 == 0)
                {
                    bitmask = (byte)(bitmask & 0xF0);
                    bitmask = (byte)(bitmask >> 4);
                }
                else
                {
                    bitmask = (byte)(bitmask & 0x0F);
                }


                r = new Room(i, connectBytes, enemies, sideView, bitmask, false, false, false, false, false, false, false, false, -1, addr, false, false);
                r.Group = "Bank# " + (j + 1);
                sb.Append(r.Serialize() + ",");

            }
        }
        sb[sb.Length - 1] = ']';
        File.WriteAllText("Rooms.dump", sb.ToString());
    }

}
