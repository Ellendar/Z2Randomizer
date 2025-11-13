using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.RandomizerCore;

public class NES
{
    /// Basic look up table to convert from the original NES palette value to RGB
    public static readonly Color[] NesColors = [
        Color.FromArgb(101, 101, 101), Color.FromArgb(0, 18, 125),    Color.FromArgb(24, 0, 142),    Color.FromArgb(54, 0, 130),
        Color.FromArgb(86, 0, 93),     Color.FromArgb(90, 0, 24),     Color.FromArgb(79, 5, 0),      Color.FromArgb(56, 25, 0),
        Color.FromArgb(29, 49, 0),     Color.FromArgb(0, 61, 0),      Color.FromArgb(0, 65, 0),      Color.FromArgb(0, 59, 23),
        Color.FromArgb(0, 46, 85),     Color.FromArgb(0, 0, 0),       Color.FromArgb(0, 0, 0),       Color.FromArgb(0, 0, 0),

        Color.FromArgb(175, 175, 175), Color.FromArgb(25, 78, 200),   Color.FromArgb(71, 47, 227),   Color.FromArgb(107, 31, 215),
        Color.FromArgb(147, 27, 174),  Color.FromArgb(158, 26, 94),   Color.FromArgb(153, 50, 0),    Color.FromArgb(123, 75, 0),
        Color.FromArgb(91, 103, 0),    Color.FromArgb(38, 122, 0),    Color.FromArgb(0, 130, 0),     Color.FromArgb(0, 122, 62),
        Color.FromArgb(0, 110, 138),   Color.FromArgb(0, 0, 0),       Color.FromArgb(0, 0, 0),       Color.FromArgb(0, 0, 0),

        Color.FromArgb(255, 255, 255), Color.FromArgb(100, 169, 255), Color.FromArgb(142, 137, 255), Color.FromArgb(182, 118, 255),
        Color.FromArgb(224, 111, 255), Color.FromArgb(239, 108, 196), Color.FromArgb(240, 128, 106), Color.FromArgb(216, 152, 44),
        Color.FromArgb(185, 180, 10),  Color.FromArgb(131, 203, 12),  Color.FromArgb(91, 214, 63),   Color.FromArgb(74, 209, 126),
        Color.FromArgb(77, 199, 203),  Color.FromArgb(76, 76, 76),    Color.FromArgb(0, 0, 0),       Color.FromArgb(0, 0, 0),

        Color.FromArgb(255, 255, 255), Color.FromArgb(199, 229, 255), Color.FromArgb(217, 217, 255), Color.FromArgb(233, 209, 255),
        Color.FromArgb(249, 206, 255), Color.FromArgb(255, 204, 241), Color.FromArgb(255, 212, 203), Color.FromArgb(248, 223, 177),
        Color.FromArgb(237, 234, 164), Color.FromArgb(214, 244, 164), Color.FromArgb(197, 248, 184), Color.FromArgb(190, 246, 211),
        Color.FromArgb(191, 241, 241), Color.FromArgb(185, 185, 185), Color.FromArgb(0, 0, 0),       Color.FromArgb(0, 0, 0),
    ];
}

/// <summary>
/// Represents a pointer within an NES ROM (PRG-ROM section),
/// using MMC5 mapper.
/// </summary>
public class NesPointer
{
    private const int RomHdrSize = ROM.RomHdrSize;

    public int Bank { get; private set; }
    public ushort PrgAddr { get; private set; }
    public int RomAddr { get; private set; }

    /// <summary>
    /// Construct from a raw ROM address (including 0x10-byte header).
    /// </summary>
    public NesPointer(int romAddress)
    {
        Bank = GetPrgRomAddrSegment(romAddress);
        PrgAddr = (ushort)ConvertPrgRomAddrToAsmAddr(romAddress);
        RomAddr = romAddress;
    }

    /// <summary>
    /// Construct from a PRG bank and address (e.g., $8000–$FFFF).
    /// </summary>
    public NesPointer(int bank, int prgAddr)
    {
        Bank = bank;
        PrgAddr = (ushort)prgAddr;
        RomAddr = ConvertNesPtrToPrgRomAddr(bank, prgAddr);
    }

    public override string ToString()
    {
        return $"Bank=${Bank:X2}, Addr=${PrgAddr:X4} (ROM=${RomAddr:X6})";
    }

    public static int ConvertNesPtrToPrgRomAddr(int bank, int nesPtr)
    {
        Debug.Assert(nesPtr >= 0x8000, "Non-PRG pointers (like SRAM) are not supported here");
        switch (bank)
        {
            case < 0x07:
                return nesPtr - 0x8000 + bank * 0x4000 + RomHdrSize;
            case 0x07:
                return nesPtr - 0xC000 + bank * 0x4000 + RomHdrSize;
            case < 0x10:
                throw new NotImplementedException();
            case < 0x1d:
                return nesPtr - 0x8000 + bank * 0x2000 + RomHdrSize;
            case 0x1d:
                return nesPtr - 0xA000 + bank * 0x2000 + RomHdrSize;
            case 0x1e:
                return nesPtr - 0xC000 + bank * 0x2000 + RomHdrSize;
            case 0x1f:
                return nesPtr - 0xE000 + bank * 0x2000 + RomHdrSize;
            default:
                throw new NotImplementedException();
        }
    }

    public static int ConvertPrgRomAddrToAsmAddr(int romAddr)
    {
        int minusHeader = romAddr - RomHdrSize;
        // refer to Asm/Init.s for these values
        if (minusHeader < 0x1c000) // PRG0 to PRG6
        {
            return 0x8000 + (minusHeader & 0x3fff);
        }
        else if (minusHeader < 0x20000) // PRG7
        {
            return 0xc000 + (minusHeader & 0x3fff);
        }
        else if (minusHeader < 0x3a000) // PRG10 to PRG1C
        {
            return 0x8000 + (minusHeader & 0x1fff);
        }
        else if (minusHeader < 0x3c000) // PRG1D
        {
            return 0xa000 + (minusHeader & 0x1fff);
        }
        else if (minusHeader < 0x3e000) // PRG1E
        {
            return 0xc000 + (minusHeader & 0x1fff);
        }
        else if (minusHeader < 0x40000) // PRG1F
        {
            return 0xe000 + (minusHeader & 0x1fff);
        }
        else
        {
            throw new ArgumentException("This is not a PRG address");
        }
    }

    public static int GetPrgRomAddrSegment(int romAddr)
    {
        int minusHeader = romAddr - RomHdrSize;
        // refer to Asm/Init.s for these values
        if (minusHeader < 0x20000) // PRG1 to PRG7
        {
            return minusHeader / 0x4000;
        }
        else if (minusHeader < 0x40000) // PRG10 to PRG1F
        {
            return minusHeader / 0x2000;
        }
        else
        {
            throw new ArgumentException("This is not a PRG address");
        }
    }
}
