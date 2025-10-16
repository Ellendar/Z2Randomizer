using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using js65;
using NLog;
using Z2Randomizer.RandomizerCore.Overworld;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore;

/*
Classes Needed:

* Location - Represents entry from overworld
    * Terrain type
    * Requires Jump
    * Requires Fairy
    * Requires Hammer
    * X Position
    * Y Position
    * Exit Location?
    * Contains Item
    * Enter from right
    * Map Number

* World - Represent a single map
    * Location Tree
        * Root of tree is starting position, children are areas directly accessible from position
    * Lists of locations broken down by terrain
    * Entry / Exit Points

* Hyrule
    * Does the randomization
    * Checks for Sanity
    * Contains links to all World objects

* Room - Palace only?

* Palace
    * Pallette
    * Enemies?
    * Rooms
*/
public class ROM
{
    public const int RomHdrSize = 0x10;
    public const int PrgRomOffs = RomHdrSize;
    public const int PrgRomSize = 0x40000;
    public const int ChrRomOffset = RomHdrSize + PrgRomSize;
    public const int ChrRomSize = 0x20000;
    public const int RomSize = ChrRomOffset + ChrRomSize;

    public const int VanillaPrgRomSize = 0x20000;
    public const int VanillaChrRomOffs = RomHdrSize + VanillaPrgRomSize;
    public const int VanillaRomSize = VanillaChrRomOffs + ChrRomSize;

    public static readonly IReadOnlyList<int> FreeRomBanks = Enumerable.Range(0x10, 0xc).ToList();

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const int textPointerTableStart = 0xEFCE;
    private const int textAddrEndinROM = 0xF090;
    private const int textAddrOffset = 0x4010;

    //addresses for palace bricks and curtains
    private readonly int[] outBricks = { 0x10485, 0x10495, 0x104A5, 0x104B5, 0x104C5, 0x104D5, 0x14023 };
    private readonly int[] inBricks = { 0x13F15, 0x13F25, 0x13F35, 0x13F45, 0x13F55, 0x13F65, 0x14033 };
    private readonly int[] inWindows = { 0x13F19, 0x13F29, 0x13F39, 0x13F49, 0x13F59, 0x13F69, 0x14027 };
    private readonly int[] inCurtains = { 0x13F1D, 0x13F2D, 0x13F3D, 0x13F4D, 0x13F5D, 0x13F6D, 0x1402B };

    private readonly int[] brickSprites = {
        ChrRomOffset + 0x9640, 
        ChrRomOffset + 0xB640, 
        ChrRomOffset + 0xD640, 
        ChrRomOffset + 0x13640, 
        ChrRomOffset + 0x15640, 
        ChrRomOffset + 0x17640, 
        ChrRomOffset + 0x19640
    };
    private readonly int[] inBrickSprites = { 
        ChrRomOffset + 0x9680, 
        ChrRomOffset + 0xB680, 
        ChrRomOffset + 0xD680, 
        ChrRomOffset + 0x13680, 
        ChrRomOffset + 0x15680, 
        ChrRomOffset + 0x17680,
        ChrRomOffset + 0x19680
    };

    private const int STRING_TERMINATOR = 0xFF;
    private const int creditsLineOneAddr = 0x15377;
    private const int creditsLineTwoAddr = 0x15384;

    //sprite addresses for reading sprites from a ROM:
    private const int titleSpriteStartAddr = ChrRomOffset + 0xD00;
    private const int titleSpriteEndAddr = ChrRomOffset + 0xD20;
    private const int beamSpriteStartAddr = ChrRomOffset + 0x840;
    private const int beamSpriteEndAddr = ChrRomOffset + 0x860;
    private const int raftSpriteStartAddr = ChrRomOffset + 0x11440;
    private const int raftSpriteEndAddr = ChrRomOffset + 0x11480;
    private const int OWSpriteStartAddr = ChrRomOffset + 0x11740;
    private const int OWSpriteEndAddr = ChrRomOffset + 0x117c0;
    private const int sleeperSpriteStartAddr = ChrRomOffset + 0x1000;
    private const int sleeperSpriteEndAddr = ChrRomOffset + 0x1060;
    private const int oneUpSpriteStartAddr = ChrRomOffset + 0xa80;
    private const int oneUpSpriteEndAddr = ChrRomOffset + 0xaa0;
    private const int endSprite1StartAddr = ChrRomOffset + 0xed80;
    private const int endSprite1EndAddr = ChrRomOffset + 0xee80;
    private const int endSprite2StartAddr = ChrRomOffset + 0xf000;
    private const int endSprite2EndAddr = ChrRomOffset + 0xf0e0;
    private const int endSprite3StartAddr = ChrRomOffset + 0xd000;
    private const int endSprite3EndAddr = ChrRomOffset + 0xd040;
    private const int headSpriteStartAddr = ChrRomOffset + 0x1960;
    private const int headSpriteEndAddr = ChrRomOffset + 0x1970;
    private const int playerSpriteStartAddr = ChrRomOffset + 0x2000;
    private const int playerSpriteEndAddr = ChrRomOffset + 0x3000;

    //kasuto jars
    private const int kasutoJarTextAddr = 0xEEC9;
    private const int kasutoJarAddr = 0x1E7E8;

    private readonly int[] fastCastMagicAddr = { 0xE15, 0xE16, 0xE17 };

    private readonly int[] palPalettes = { 0, 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60 };
    private readonly int[] palGraphics = { 0, 0x04, 0x05, 0x09, 0x0A, 0x0B, 0x0C, 0x06 };

    //On second thought, all text processing should be moved to customTexts.
    /*
    private readonly Dictionary<Town, int> spellTextPointers = new()
    {
        {Town.RAURU, 0xEFEC },
        {Town.RUTO, 0xEFFE },
        {Town.SARIA_NORTH, 0xF014 },
        {Town.MIDO_WEST, 0xF02A },
        {Town.NABOORU_WIZARD, 0xF05A },
        {Town.DARUNIA_WEST, 0xf070 },
        {Town.NEW_KASUTO, 0xf088 },
        {Town.OLD_KASUTO, 0xf08e },
        {Town.MIDO_CHURCH, textPointerTableStart + 47 * 2 },
        {Town.DARUNIA_ROOF, textPointerTableStart + 82 * 2 },
        {Town.BAGU, textPointerTableStart + 48 * 2 },
        {Town.SARIA_TABLE, textPointerTableStart + 21 * 2 },
        {Town.NABOORU_FOUNTAIN, textPointerTableStart + 63 * 2 },
    };*/

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

    public static readonly int[] LinkOutlinePaletteAddr = {         0x285a, 0x2a0a, 0x40af, 0x40bf, 0x40cf, 0x40df, 0x80af, 0x80bf, 0x80cf, 0x80df, 0xc0af, 0xc0bf, 0xc0cf, 0xc0df, 0xc0ef, 0x100af, 0x100bf, 0x100cf, 0x100df, 0x140af, 0x140bf, 0x140cf, 0x140df, 0x17c19, 0x1c464, 0x1c47c };
    public static readonly int[] LinkFacePaletteAddr =    {         0x285b, 0x2a10, 0x40b0, 0x40c0, 0x40d0, 0x40e0, 0x80b0, 0x80c0, 0x80d0, 0x80e0, 0xc0b0, 0xc0c0, 0xc0d0, 0xc0e0, 0xc0f0, 0x100b0, 0x100c0, 0x100d0, 0x100e0, 0x140b0, 0x140c0, 0x140d0, 0x140e0, 0x17c1a, 0x1c465, 0x1c47d };
    public static readonly int[] LinkTunicPaletteAddr =   { 0x10ea, 0x285c, 0x2a16, 0x40b1, 0x40c1, 0x40d1, 0x40e1, 0x80b1, 0x80c1, 0x80d1, 0x80e1, 0xc0b1, 0xc0c1, 0xc0d1, 0xc0e1, 0xc0f1, 0x100b1, 0x100c1, 0x100d1, 0x100e1, 0x140b1, 0x140c1, 0x140d1, 0x140e1, 0x17c1b, 0x1c466, 0x1c47e };
    public const int LinkShieldPaletteAddr = 0xe9e;
    public static readonly int[] ZeldaOutlinePaletteAddr = { 0x4025, 0x8025, 0x14049, 0x140c7 };
    public static readonly int[] ZeldaFacePaletteAddr =    { 0x4023, 0x8023, 0x14047, 0x140c9 };
    public static readonly int[] ZeldaDressPaletteAddr =   { 0x4024, 0x8024, 0x14048, 0x140c8 };


    public byte[] rawdata { get; }

    public ROM(string filename, bool expandRom = false)
    {
        try
        {
            FileStream fs;

            fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fs, new ASCIIEncoding());
            rawdata = ConvertData(br.ReadBytes(VanillaRomSize), expandRom);

        }
        catch (Exception err)
        {
            throw new Exception("Cannot find or read file to dump.", err);
        }
    }

    public ROM(ROM clone, bool expandRom = false)
    {
        rawdata = ConvertData(clone.rawdata.ToArray(), expandRom);
    }

    public ROM(byte[] data, bool expandRom = false)
    {
        rawdata = ConvertData(data, expandRom);
    }

    private static byte[] ConvertData(byte[] data, bool expandRom)
    {
        if (!expandRom)
            return data;

        Debug.Assert(data.Length == VanillaRomSize);

        // Expand the ROM from 128 KB PRG-ROM / 128 KB CHR-ROM to 256/128
        var newdata = new byte[RomSize];

        Array.Copy(data, newdata, VanillaChrRomOffs);
        Array.Copy(data, VanillaChrRomOffs, newdata, ChrRomOffset, ChrRomSize);

        return newdata;
    }

    public byte GetByte(int index)
    {
        return rawdata[index];
    }

    
    //This used to be [start, end), but that's both awkward and not idiomatic, so I replaced it
    //with a more conventional start + lengh implementation
    public byte[] GetBytes(int start, int length)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = GetByte(i + start);
        }
        return bytes;
    }

    public int GetShort(int indexMsb, int indexLsb)
    {
        return (GetByte(indexMsb) << 8) + GetByte(indexLsb);
    }

    public byte[] GetTerminatedString(int index)
    {
        List<byte> bytes = new();
        byte b;
        do
        {
            b = GetByte(index++);
            bytes.Add(b);
        }
        while (b != STRING_TERMINATOR);
        return bytes.ToArray();
    }

    public void Put(int index, byte data)
    {
        rawdata[index] = data;
    }

    public void Put(int index, string data)
    {
        Put(index, Convert.FromHexString(data));
    }

    public void Put(int index, params byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            rawdata[index + i] = data[i];
        }
    }

    public void PutShort(int indexMsb, int indexLsb, int data)
    {
        Put(indexMsb, (byte)(data / 256));
        Put(indexLsb, (byte)(data % 256));
    }

    public void Dump(string filename)
    {
        File.WriteAllBytes(filename, rawdata);
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
        if      (minusHeader < 0x1c000)
        {
            return 0x8000 + (minusHeader & 0x3fff);
        }
        else if (minusHeader < 0x20000)
        {
            return 0xc000 + (minusHeader & 0x3fff);
        }
        else if (minusHeader < 0x3a000)
        {
            return 0x8000 + (minusHeader & 0x1fff);
        }
        else if (minusHeader < 0x3c000)
        {
            return 0xa000 + (minusHeader & 0x1fff);
        }
        else if (minusHeader < 0x3e000)
        {
            return 0xc000 + (minusHeader & 0x1fff);
        }
        else if (minusHeader < 0x40000)
        {
            return 0xe000 + (minusHeader & 0x1fff);
        }
        else
        {
            throw new ArgumentException("This is not a PRG address");
        }
    }

    /// Read pointer at `nesPtr`. Then read the data it points to.
    /// Relocate that data to a new address using js65. Write the
    /// new pointer to `nesPtr`. This will be done at link time.
    /// The rom data is not modified directly, only through js65.
    public void RelocateData(Assembler asm, int bank, int nesPtr)
    {
        var romPtr = ConvertNesPtrToPrgRomAddr(bank, nesPtr);
        var nesAddr = GetShort(romPtr + 1, romPtr);
        var romAddr = ConvertNesPtrToPrgRomAddr(bank, nesAddr);
        var length = GetByte(romAddr);
        var bytes = GetBytes(romAddr, length);
        const string label = "RelocateBytes";
        var a = asm.Module();
        a.Segment($"PRG{bank}");
        a.Reloc();
        a.Label(label);
        a.Byt(bytes);
        a.Org((ushort)nesPtr);
        a.Word(a.Symbol(label));
    }

    public byte[] ReadSprite(int spriteAddr, int tilesWide, int tilesHigh, byte[] palette)
    {
        // Each byte specifies 1 color bit for an 8 pixel column
        const int pixelsPerByte = 8;
        // These bytes are arranged in groups of 8
        const int bytesPerTile = 8;

        // We only have one palette to read from, but should there be multipalette sprites someday
        // this could come in handy
        const int paletteIdx = 0;
        int width = tilesWide * 8;
        int height = tilesHigh * 8;
        const int colorDepth = 4;
        int tileCount = tilesWide * tilesHigh;

        var buffer = new byte[colorDepth * width * height];
        for (var tile = 0; tile < tileCount; ++tile)
        {
            var offset = tile * 16;
            int tilex, tiley;
            if (tilesHigh % 2 == 1)
            {
                // If a sprite is only 1 tile (8 pixels) high, it is read horizontally
                tilex = (tile % tilesWide) * 8;
                tiley = (tile / tilesWide) * 8;
            }
            else
            {
                // In all other cases, 2 tiles (16 pixels) is read vertically at a time
                int halfTile = tile >> 1;
                tilex = (halfTile % tilesWide) * 8;
                tiley = (2 * (halfTile / tilesWide) + (tile & 1)) * 8;
            }
            for (var j = 0; j < bytesPerTile; ++j)
            {
                // The color bits for a single pixel is defined 8 bytes apart
                var colorByte0 = GetByte(spriteAddr + offset + j);
                var colorByte1 = GetByte(spriteAddr + offset + j + 8);
                for (var i = 0; i < pixelsPerByte; ++i)
                {
                    var pixelShift = 7 - i;
                    var bit0 = (colorByte0 >> pixelShift) & 1;
                    var bit1 = ((colorByte1 >> pixelShift) & 1) << 1;
                    var color = (bit0 | bit1) + (paletteIdx * 4);
                    var appliedColor = NesColors[palette[color]];

                    var x = tilex + i;
                    var y = tiley + j;
                    buffer[0 + 4 * (x + width * y)] = appliedColor.R;
                    buffer[1 + 4 * (x + width * y)] = appliedColor.G;
                    buffer[2 + 4 * (x + width * y)] = appliedColor.B;
                    buffer[3 + 4 * (x + width * y)] = (byte)((color == 0) ? 0 : 255);
                }
            }
        }
        return buffer;
    }

    public List<Text> GetGameText()
    {
        List<Text> texts = [];
        for (int i = textPointerTableStart; i <= textAddrEndinROM; i += 2)
        {
            int addr = GetShort(i + 1, i);
            addr += textAddrOffset;
            var t = GetTerminatedString(addr);
            texts.Add(new Text(t));
        }
        return texts;
    }

/*
    public void WriteHints(List<Text> texts)
    {
        int textptr = 0xE390;
        int ptr = 0xE390 - 0x4010;
        int ptrptr = 0xEFCE;

        for (int i = 0; i < texts.Count; i++)
        {
            int high = (ptr & 0xff00) >> 8;
            int low = (ptr & 0xff);
            Put(ptrptr, (byte)low);
            Put(ptrptr + 1, (byte)high);
            ptrptr = ptrptr + 2;
            for (int j = 0; j < texts[i].EncodedText.Count; j++)
            {
                Put(textptr, (byte)texts[i].EncodedText[j]);
                textptr++;
                ptr++;
            }
        }
    }
*/

    public void WritePalacePalettes(List<int[]> bricks, List<int[]> curtains, List<int> bRows, List<int> binRows)
    {
        int[,] bSprites = new int[7, 32];
        int[,] binSprites = new int[7, 32];
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                bSprites[i, j] = (int)GetByte(brickSprites[i] + j);
                binSprites[i, j] = (int)GetByte(inBrickSprites[i] + j);
            }
        }
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Put(outBricks[i] + j, (byte)bricks[i][j]);
                Put(inBricks[i] + j, (byte)bricks[i][j]);
                Put(inCurtains[i] + j, (byte)curtains[i][j]);
                if (j == 0)
                {
                    Put(inWindows[i] + j, (byte)bricks[i][j]);
                }
            }

            for (int j = 0; j < 32; j++)
            {
                Put(brickSprites[i] + j, (byte)bSprites[bRows[i], j]);
                Put(inBrickSprites[i] + j, (byte)binSprites[binRows[i], j]);
            }
        }
    }

    public void AddCredits()
    {
        var randoby = Util.ToGameText("RANDO BY  ", false);
        Put(creditsLineOneAddr, randoby);

        var digshake = Util.ToGameText("DIGSHAKE ", true);
        Put(creditsLineTwoAddr, digshake);
    }

    public void AddRandomizerToTitle(Assembler asm)
    {
        // This is just updating the macro commands used to draw the title screen tiles
        // The actual tile data will be imported into the rom data later
        asm.Module().Code("""
.macpack common
.segment "PRG5"

VERTICAL = $80
REPEAT = $40

; When looping the title screen, the game will draw this part in 7 different chunks
; so we need to update the hardcoded sizes of the chunks here and fit in one more line

; Free the locations of the vanilla table and relocate them to fit the extra line
FREE "PRG5" [$AEE7, $AEEE)
FREE "PRG5" [$AB5F, $AB6D)

; Update the vanilla table pointers
.org $AE76
    lda TitleLineTable,x
    sta $00
    lda TitleLineTable+1,x

.org $AEF0
    ldy TitleLineLenTable,x

.org $AE9F
    lda $34
    cmp #$08

.reloc
TitleLineLenTable:
.byte Line1 - Line0
.byte Line2 - Line1
.byte Line3 - Line2
.byte Line4 - Line3
.byte Line5 - Line4
.byte Line6 - Line5
.byte Line7 - Line6
.byte TitleEnd - Line7

.reloc
TitleLineTable:
.word (Line0)
.word (Line1)
.word (Line2)
.word (Line3)
.word (Line4)
.word (Line5)
.word (Line6)
.word (Line7)

.org $af69

Line0:
; ZELDA II
.byte $22, $4c, 8 ; Write 8 bytes to $224c
.byte $00, $01, $02, $03, $04, $05, $06, $07
; setup the attributes for the right hand size
.byte $23,$E0,REPEAT | 16,$00
.byte $23,$E7,1,$cc
; .byte $23,$E8,REPEAT | 7,$00
.byte $23,$EF,1,$cc
.byte $23,$f0,REPEAT | 16,$00
.byte $23,$f7,1,$cc
;.byte $23,$f8,REPEAT | 7,$00
.byte $23,$ff,1,$cc


Line1:
; RANDOMIZER
.byte $22, $6a, 12 ; Write 12 bytes to $226a
; skipping over $0e since its the copyright symbol used at the end
.byte $08, $09, $0a, $0b, $0c, $0d, $0f, $10, $11, $12, $13, $14

Line2:
; THE ADVENTURE OF TOP
.byte $22, $88, 17 ; Write 17 bytes to $2288
.byte $15, $16, $17, $18, $19, $1a, $1b, $1c, $1d, $1e, $1f, $20, $21, $22, $23, $24, $25 ; Top


Line3:
; THE ADVENTURE OF BOT
.byte $22, $a7, 18 ; Write 18 bytes to $22a7
.byte $26, $27, $28, $29, $2a, $2b, $2c, $2d, $2e, $2f, $30, $31, $32, $33, $34, $35, $36, $37 ; Bot

Line4:
; LINK(TM)
.byte $22, $ca, 14 ; Write 14 bytes to $22a7
.byte $38, $39, $3a, $f4, $3b, $3c, $39, $3d, $39, $3e, $3f, $40, $41, $42

Line5:
.byte $22, $ea, 13 ; Write 13 bytes to $22ea
.byte $43, $44, $45, $f4, $46, $47, $48, $49, $4a, $4b, $4c, $4d, $4e
Line6:
.byte $23, $09, 15 ; Write 15 bytes to $2309
.byte $4f, $50, $51, $52, $53, $54, $55, $56, $57, $54, $58, $59, $5a, $5b, $5c
Line7:
.byte $23, $29, 15 ; Write 15 bytes to $2329
.byte $5d, $5e, $5e, $5e, $5f, $5e, $60, $5e, $61, $62, $63, $64, $65, $66, $67
TitleEnd:

; Add some filler commands to take up the rest of the space
.byte $23, $40, $b02e - TitleEnd-3-4
.repeat $b02e - TitleEnd-3-4
    .byte $f4
.endrepeat

.byte $20,$1F,VERTICAL | REPEAT | $1f,$FD

.assert * = $b02e
""", "randomizer_title_text.s");
    }

    public void ApplyIps(byte[] patch, bool expandRom = false)
    {
        IpsPatcher.Patch(rawdata, patch, expandRom);
    }

    private readonly int[] fireLocs = { 
        ChrRomOffset + 0x00840, 
        ChrRomOffset + 0x02840, 
        ChrRomOffset + 0x04840, 
        ChrRomOffset + 0x06840, 
        ChrRomOffset + 0x08840, 
        ChrRomOffset + 0x0a840, 
        ChrRomOffset + 0x0c840, 
        ChrRomOffset + 0x0e840, 
        ChrRomOffset + 0x16840, 
        ChrRomOffset + 0x12840, 
        ChrRomOffset + 0x14840, 
        ChrRomOffset + 0x18840 
    };

    public void UpdateSprite(CharacterSprite charSprite, bool sanitize, bool changeItems)
    {
        /*
         * Dear future digshake,
         * 
         * This method is garbage.
         * 
         * The first entry in sprite info contains non-sprite data.
         * My lazy ass just skips it.
         * 
         * If you update the sprite structures in Graphics.cs, you could refactor this:
         * 
         * Make lists of all the ROM locations for sprites
         * Iterate through each sprite array, putting the data at the ROM locations
         * 
         * -Past digshake (who is still too lazy to fix this method)
         */
        /*
         * Dear future Ellendar,
         * This method is still garbage, but better.
         * Eventually we need to create a data structure that represents the individual properies of things like sprites
         * that we can save in external files so they can be imported from a simple format people can use to customize all
         * the cosmetics.
         */

        if (charSprite.Patch != null) {
            if (sanitize)
            {
                SpritePatcher.PatchSpriteSanitized(rawdata, charSprite.Patch, true, changeItems);
            }
            else
            {
                IpsPatcher.Patch(rawdata, charSprite.Patch, true);
            }
        }
    }

    public void UpdateSpritePalette(NesColor tunicColor, NesColor skinTone, NesColor outlineColor, NesColor shieldColor, BeamSprites beamSprite)
    {
        int? tunicColorInt = null;
        int? skinToneInt = null;
        int? outlineColorInt = null;
        int? shieldColorInt = null;

        // We won't write the default color to the ROM, but we set it
        // to avoid another random color rolling the same value.
        if (tunicColor != NesColor.Random)
        {
            tunicColorInt = tunicColor != NesColor.Default ? (int)tunicColor : GetByte(LinkTunicPaletteAddr[0]);
        }
        if (skinTone != NesColor.Random)
        {
            skinToneInt = skinTone != NesColor.Default ? (int)skinTone : GetByte(LinkFacePaletteAddr[0]);
        }
        if (outlineColor != NesColor.Random)
        {
            outlineColorInt = outlineColor != NesColor.Default ? (int)outlineColor : GetByte(LinkOutlinePaletteAddr[0]);
        }
        if (shieldColor != NesColor.Random)
        {
            shieldColorInt = shieldColor != NesColor.Default ? (int)shieldColor : GetByte(LinkShieldPaletteAddr);
        }

        static int RandomUniqueColor(params int?[] forbiddenColors)
        {
            Random random = new Random();
            int color;
            do
            {
                int firstHex = random.Next(4);
                int secondHex = random.Next(0, 14);
                color = firstHex * 16 + secondHex;
                if (color == 0x0d) { continue; }
            }
            while (forbiddenColors.Contains(color));
            return color;
        }
        if (tunicColor == NesColor.Random)
        {
            tunicColorInt = RandomUniqueColor([0x0d, tunicColorInt, skinToneInt, outlineColorInt, shieldColorInt]);
        }
        if (skinTone == NesColor.Random)
        {
            skinToneInt = RandomUniqueColor([0x0d, tunicColorInt, skinToneInt, outlineColorInt, shieldColorInt]);
        }
        if (outlineColor == NesColor.Random)
        {
            outlineColorInt = RandomUniqueColor([0x0d, tunicColorInt, skinToneInt, outlineColorInt, shieldColorInt]);
        }
        if(shieldColor == NesColor.Random)
        {
            shieldColorInt = RandomUniqueColor([0x0d, tunicColorInt, skinToneInt, outlineColorInt, shieldColorInt]);
        }

        if(tunicColor != NesColor.Default && tunicColorInt != null)
        { 
            foreach(int l in LinkTunicPaletteAddr)
            {
                Put(l, (byte)tunicColorInt);
            }
        }
        if (skinTone != NesColor.Default && skinToneInt != null)
        {
            foreach (int l in LinkFacePaletteAddr)
            {
                Put(l, (byte)skinToneInt);
            }
        }
        if (outlineColor != NesColor.Default && outlineColorInt != null)
        {
            foreach(int l in LinkOutlinePaletteAddr)
            {
                Put(l, (byte)outlineColorInt);
            }
        }
        if(shieldColor != NesColor.Default && shieldColorInt != null)
        {
            Put(LinkShieldPaletteAddr, (byte)shieldColorInt);
        }

        if(beamSprite == BeamSprites.RANDOM)
        {
            Random r2 = new Random();
            beamSprite = (BeamSprites)r2.Next(Enum.GetNames(typeof(BeamSprites)).Length - 1);
        }
       
        byte[] newSprite = new byte[32];

        if (beamSprite == BeamSprites.FIRE || beamSprite == BeamSprites.AXE || beamSprite == BeamSprites.HAMMER)
        {
            Put(0x18f5, [0xa9, 0x00, 0xea]);
        }
        else if (beamSprite != BeamSprites.DEFAULT)
        {
            Put(0X18FB, 0x84);
        }

        int beamSpriteOffset = beamSprite switch
        {
            BeamSprites.FIRE => 0,
            BeamSprites.BUBBLE => 0xaa0,
            BeamSprites.ROCK => 0x2ae0,
            BeamSprites.AXE => 0x2fa0,
            BeamSprites.HAMMER => 0x12ee0,
            BeamSprites.WIZZROBE_BEAM => 0x14dc0,
            BeamSprites.DEFAULT => 0,
            _ => throw new Exception("Invalid beam sprite")
        };
        if(beamSpriteOffset != 0)
        {
            for (int i = 0; i < 32; i++)
            {
                byte next = GetByte(ChrRomOffset + beamSpriteOffset + i);
                newSprite[i] = next;
            }
            foreach (int loc in fireLocs)
            {
                for (int i = 0; i < 32; i++)
                {
                    Put(loc + i, newSprite[i]);
                }
            }
        }
    }

    public void DoHackyFixes()
    {
        //Hacky fix for palace connections
        Put(0x1074A, 0xFC);
        Put(0x1477D, 0xFC);

        //Hacky fix for new kasuto

        Put(0x8660, 0x51);
        Put(0x924D, 0x00);
        
        //Hack fix for palace 6
        Put(0x8664, 0xE6);
        //put(0x935E, 0x02);
       
        //Fix for extra battle scene
        Put(0x8645, 0x00);

        //Make text go fast
        Put(0xF75E, 0x00);
        Put(0xF625, 0x00);
        Put(0xF667, 0x00);

        //300 point XP reward is actually 300 and not 301
        Put(0x1DDDC, 0x2C);
		
        // Horsehead mini-boss despawn bug fix.
        // For some reason in vanilla, killing horsehead also eliminates *ALL* loaded objects in the enemy slots
        // This includes custom rando rooms with items like pbags that are loaded into enemy slots.
        // The patch simply changes a hardcoded `ldx #5` which is used to loop through all object slots to `tax nop`
        // as `a` holds the current object ID (horsehead) and we want to just run the death code from horsehead to 0
        // keeping the item (pbag etc) in the room alive since it spawns in a later slot (#5)
        Put(0x13ec1, [0xaa, 0xea]); // tax nop

        // In vanilla Darunia there is written text on a wall inside one
        // of the houses. With the changes for FullItemShuffle and changes
        // to town signs, and there already being an invisible dialog hint at
        // the Upstab closed door, there aren't any bytes left to put a
        // text ID for this. Lets switch it to a person to talk to.
        Put(0xC9b7, [0x7d, 0x91]); // Replace sign with purple kid at (45,9)
        Put(0xE309, 0x19); // Put the old readable wall hint ID here
    }

    public void WriteKasutoJarAmount(int kasutoJars)
    {
        Put(kasutoJarTextAddr, (byte)(0xD0 + kasutoJars));
        Put(kasutoJarAddr, (byte)kasutoJars);
    }

    public void WriteFastCastMagic()
    {

        foreach (int addr in fastCastMagicAddr)
        {
            Put(addr, 0xEA);
        }
    }

    public void DisableMusic()
    {
        /*
         * This method needs some more refactoring to eliminate those magic numbers
         * But I just don't feel like it right now.
         */
        for (int i = 0; i < 4; i++)
        {
            Put(0x1a010 + i, 08);
            Put(0x1a3da + i, 08);
            Put(0x1a63f + i, 08);
        }

        Put(0x1a946, 08);
        Put(0x1a947, 08);
        Put(0x1a94c, 08);

        Put(0x1a02f, 0);
        Put(0x1a030, 0x44);
        Put(0x1a031, 0xA3);
        Put(0x1a032, 0);
        Put(0x1a033, 0);
        Put(0x1a034, 0);

        Put(0x1a3f4, 0);
        Put(0x1a3f5, 0x44);
        Put(0x1a3f6, 0xA3);
        Put(0x1a3f7, 0);
        Put(0x1a3f8, 0);
        Put(0x1a3f9, 0);

        Put(0x1a66e, 0);
        Put(0x1a66f, 0x44);
        Put(0x1a670, 0xA3);
        Put(0x1a671, 0);
        Put(0x1a672, 0);
        Put(0x1a673, 0);

        Put(0x1a970, 0);
        Put(0x1a971, 0x44);
        Put(0x1a972, 0xA3);
        Put(0x1a973, 0);
        Put(0x1a974, 0);
        Put(0x1a975, 0);
    }

    /// The vanilla game sets up all 3 save slots from PRG in the
    /// title screen.  If there is already valid data in a save slot,
    /// it will keep it.  Note: this includes keeping save slots
    /// without a name, and when you register your name it only
    /// updates the name. It does not re-read the starting data from
    /// PRG. This means having old data in SRAM, with no names
    /// registered can lead to unexpected behavior. Emulators,
    /// Everdrives etc. may remember some save data from another
    /// randomized ROM, with different starting values, but you, the
    /// player will see no indication of this, since there are no
    /// names registered. This could lead to the player not being able
    /// to beat the game, since they are missing items they were
    /// expected to start with.
    ///
    /// This fixes the problem by always reloading the save slot data in the
    /// title screen for save slots that do not have a name set.
    public void FixStaleSaveSlotData(Assembler asm)
    {
        var a = asm.Module();
        a.Code("""
.include "z2r.inc"

CreateSaveSlotFromPrg = $B976
CreateSaveSlotFromPrgWithYSet = $B978
KeepExistingSaveSlot = $B99D

.segment "PRG5"
.org $B96a
    cmp #$69                           ; recovering from reset mid-save (maybe?)
    beq $B9A7                          ; keeping the vanilla branch for this
    cmp #$A5                           ; save slot status byte can be both A5 or 5A depending on when the player reset
    jmp ExtendedSaveCheck
FREE_UNTIL $B976

.reloc
ExtendedSaveCheck:
    beq HasExistingDataCheckName
    cmp #$5A
    beq HasExistingDataCheckName
    jmp CreateSaveSlotFromPrg          ; unusable save slot status byte (probably fresh SRAM)

HasExistingDataCheckName:
    ldy #$31
@loop:
        lda ($00),y
        cmp #$F4
        bne NameNotEmpty
        dey
        cpy #$29
        bne @loop
    jmp CreateSaveSlotFromPrgWithYSet  ; name is empty (small optimization to not re-write the empty name)

NameNotEmpty:
    jmp KeepExistingSaveSlot
""");
    }

    public void SetLevelCap(Assembler asm, int atkMax, int magicMax, int lifeMax)
    {
        var a = asm.Module();
        a.Code("""
.segment "PRG0"
.org $9F7A
    jmp LoadNewLevelCap
    nop
    nop
LoadNewLevelCapReturn:        ; $9F7F

.org $A89E
LoadNewLevelCap:
    lda $0777,X               ; the instruction we overwrote with jmp
    cmp LevelCaps,X
    jmp LoadNewLevelCapReturn
""");
        a.Org(0xa8a7);
        a.Label("LevelCaps");
        a.Byt((byte)atkMax);
        a.Byt((byte)magicMax);
        a.Byt((byte)lifeMax);
    }

    /// <summary>
    /// If the level cap for a stat is less than 8, we do not want to allow
    /// the player to cancel out of the level up screen to continue chasing
    /// the experience for that stat once it is maxed. To stay true to vanilla,
    /// this is still allowed if the level cap is 8.
    /// 
    /// The point of this is to be able to have lower max stat levels, but
    /// not have this be abusable to reach high amounts of exp quickly using
    /// crystals.
    /// </summary>
    public void ChangeLevelUpCancelling(Assembler asm)
    {
        var a = asm.Module();
        a.Code("""
; This is called when the game checks if it should skip the cancel option
; in the level up menu or not. It runs once for each stat (X).
.segment "PRG0"
.org $A0D4
    jmp CheckIfStatMaxed
CheckStatNormally:           ;  $A0D7

.org $A0ED
SkipNormalStatCheck:

.segment "PRG0"
.reloc
CheckIfStatMaxed:
    lda LevelCaps,X
    cmp #$08
    bcs ReturnNormally       ; if level cap >= 8
    cmp $0777,X              ; current level for stat X
    bne ReturnNormally       ; if current level != level cap
    jmp SkipNormalStatCheck
ReturnNormally:
    txa                      ; things we overwrote to do jmp
    asl a
    asl a
    jmp CheckStatNormally
""");
        a.Org(0xa8a7);
        a.Label("LevelCaps");
    }

    public void UseExtendedBanksForPalaceRooms(Assembler a)
    {
        a.Module().Code("""
.include "z2r.inc"

.import SwapPRG, SwapToSavedPRG, PalaceMappingTable

; move the pointers for the data for the sideviews to load from RAM instead.
.segment "PRG7"
.org $C50C ; patch where the pointer for sideview data is loaded
    jsr CopySideviewIntoRAMAndLoadPointer
    jmp $C53A
FREE_UNTIL $C53A

.reloc
CopySideviewIntoRAMAndLoadPointer:
    ; y is the offset for the type to load from.
    ; 0 = encounter
    ; 1 = west hyrule town
    ; 2 = easy hyrule town
    ; 3 = palace group 1,2,5
    ; 4 = palace group 3,4,6 AND death mountain/maze island encounters
    ; 5 = palace GP
    ; So if Y >= 3 we need to change banks when loading the sideview data

    ; Save the sideview type into X for use later
    tya
    tax

    ; And setup the read pointer address to read the sideview from ROM
    ldy $C4BD, x
    lda $C4C3, y ; Area pointer table lo word
    sta $00
    lda $C4C3 + 1, y ; Area pointer table hi word
    sta $01
    ; And the enemy pointer address
    lda $C4C3 + 4, y ; Enemy pointer table lo word
    sta $02
    lda $C4C3 + 4 + 1, y ; Enemy pointer table hi word
    sta $03
    
    ; Now we have the pointer to the sideview address, load that so we can read the data
    ; and also load the enemy pointer into $d6 as well
    lda $0561
    asl
    tay
    lda ($00),y
    sta $d4
    lda ($02),y
    sta $d6
    iny
    lda ($00),y
    sta $d5
    lda ($02),y
    sta $d7

    ; if its not a palace area, skip switching banks
    lda WorldNumber
    cmp #3
    bcc @skipswap
        lda RegionNumber
        asl
        asl
        adc PalaceNumber
        tay
        lda PalaceMappingTable,y
        cmp #$ff
        beq @skipswap
        ; Prevent bank swapping during the end game cutscene
        lda $076c ; Game mode
        cmp #3 ; 03=wake up zelda, 04=roll credits, 06=show the lives then restart the scene
        bcs @skipswap
            ; Loading a palace sideview so use the data from extended banks instead
            lda #$0e
            jsr SwapPRG
@skipswap:

    ; Read from wherever the vanilla table to the sideview data takes us.
    ; (if its a palace, its using the extended banks now)
    ldy #0
    lda ($d4),y
    sta SideViewBuffer
    tay ; Read the sideview length byte
    ; And start reading all of the data into the buffer
-
        lda ($d4),y
        sta SideViewBuffer,y ; don't need to use indirect addressing since its a fixed buffer
        dey
        bne -

    ; If we switched banks to load the sideview, switch it back
    ; the conditional is kinda heavy, so just always switch
    jsr SwapToSavedPRG

    ; Now reload the pointers that the game expects
    ; Load the Sideview RAM buffer pointer
    lda #.lobyte(SideViewBuffer)
    sta $d4
    lda #.hibyte(SideViewBuffer)
    sta $d5
    rts
""");
    }

    public void ExtendMapSize(Assembler a)
    {
        //Implements CF's map size hack:
        //https://github.com/cfrantz/z2doc/wiki/bigger-overworlds
        a.Module().Code("""
.include "z2r.inc"

.segment "PRG0"
; Update the pointer to the overworld data
.org $87f7
    lda #$7a

.segment "PRG7"
.org $cd98
    jsr LoadMapData
    jmp $cdf5

FREE_UNTIL $cdf5

.reloc
LoadDataThroughPointers:
    ldy #$00
    --  lda ($02),y
        sta ($20),y
        iny
        bpl --
        dex
        beq +
    -   lda ($02),y
        sta ($20),y
        iny
        bne -
        inc $03
        inc $21
        dex
        bne --
+   rts

.reloc
.import RaftWorldMappingTable
LoadMapData:
    ldx $0706
    lda RaftWorldMappingTable,x
    tax
    lda $8508,x
    sta $02
    lda $8508+1,x
    sta $03
    lda #$00
    sta $20
    lda #$7a
    sta $21
    ldx #$0b
    jsr LoadDataThroughPointers
    lda #$a0
    sta $02
    lda #$88
    sta $03
    lda #$70
    sta $21
    ldx #$08
    jmp LoadDataThroughPointers

""", "extend_map_size.s");
    }

    public void DisableTurningPalacesToStone()
    {
        Put(0x87b3, new byte[] { 0xea, 0xea, 0xea });
        Put(0x47ba, new byte[] { 0xea, 0xea, 0xea });
        Put(0x1e02e, new byte[] { 0xea, 0xea, 0xea });
    }

    public void UpdateMapPointers()
    {
        Put(0x4518, new byte[] { 0x70, 0xb4 }); //west
        Put(0x451a, new byte[] { 0xf0, 0xb9 }); //dm
        Put(0x8518, new byte[] { 0x70, 0xb4 }); //east
        Put(0x851a, new byte[] { 0xf0, 0xb9 }); //maze island
    }

    public void UpAController1(Assembler a)
    {
        a.Module().Code("""
.segment "PRG0"
.org $a19f
CheckController1ForUpAUnknown:
  lda $f7
  cmp #$28

.org $a1dd
CheckController1ForUpAMagic:
  lda $f7
  cmp #$28
""", "UpAController1.s");
        //Put(0x21B0, 0xF7);
        //Put(0x21B2, 0x28);
        //Put(0x21EE, 0xF7);
        //Put(0x21F0, 0x28);
    }

    public void DisableFlashing()
    {
        Put(0x2A01, 0x12);
        Put(0x2A02, 0x12);
        Put(0x2A03, 0x12);
        Put(0x1C9FA, 0x16);
        Put(0x1C9FC, 0x16);
    }

    public async Task<byte[]?> ApplyAsm(Assembler engine)
    {
        return await engine.Apply(rawdata);
    }
    public void RandomizeKnockback(Assembler asm, Random RNG)
    {
        var a = asm.Module();
        // 0x23 is the max enemy id and we have 3 tables for each randomized value
        byte enemy_count = 0x24;
        byte[] x_arr = new byte[enemy_count];
        byte[] y_lo_arr = new byte[enemy_count];
        byte[] y_hi_arr = new byte[enemy_count];
        ushort recoilTableXAddr = (ushort) (0xbfff - enemy_count * 3);
        a.Set("RecoilTableX", recoilTableXAddr);
        a.Set("RecoilTableYLo", recoilTableXAddr + enemy_count);
        a.Set("RecoilTableYHi", recoilTableXAddr + enemy_count * 2);
        for (var i = 0; i < 7; i++)
        {
            // this is heavily biased towards higher than default knockback... muhahahaha
            a.Segment($"PRG{i}");
            a.Org(recoilTableXAddr);
            for (int j = 0; j < enemy_count; j++)
            {
                x_arr[j] = (byte)RNG.Next(0x01, 0x40);
                ushort y_val = (ushort)RNG.Next(0x50, 0x700);
                y_val = (ushort)(~y_val + 1); // convert to a negative number
                y_lo_arr[j] = (byte)(y_val & 0xff);
                y_hi_arr[j] = (byte)((y_val >> 8) & 0xff);
            }
            a.Byt(x_arr);
            a.Byt(y_lo_arr);
            a.Byt(y_hi_arr);
        }
        a.Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.Recoil.s"), "recoil.s");
    }

    public void AllowForChangingDoorYPosition(Assembler a)
    {
        a.Module().Code("""
ObjectYPositionData = $0730
KeyDoorYPosition = $075e ; just a piece of unused ram afaict

.segment "PRG4"
.org $813D
.word KeyDoorCustomPositionPRG4
.word KeyDoorCustomPositionPRG4 ; its in the item table twice for reasons i don't know

.segment "PRG4", "PRG7"
.reloc
KeyDoorCustomPositionPRG4:
    lda ObjectYPositionData
    and #$f0
    clc
    adc #$20
    sta KeyDoorYPosition
    jmp $8245 ; finish with original door code

.segment "PRG5"
.org $813D
.word KeyDoorCustomPositionPRG5

.segment "PRG5", "PRG7"
.reloc
KeyDoorCustomPositionPRG5:
    lda ObjectYPositionData
    and #$f0
    clc
    adc #$20
    sta KeyDoorYPosition
    jmp $8256 ; finish with original doorcode

.segment "PRG0"
.org $9118
    jsr LoadDoorYPos
    nop

.reloc
LoadDoorYPos:
    lda KeyDoorYPosition
    sta $2b
    rts

; Fix up the key door sparkle "poof" animation to not use a fixed position
.segment "PRG7"
.org $D9AA
    jsr LoadDoorYPosition
    nop

.reloc
LoadDoorYPosition:
    lda $2a,x
    clc
    adc #$0c
    sta $30
    rts
""", "change_door_position.s");
    }

    public void AllowForChangingElevatorYPosition(Assembler a)
    {
        a.Module().Code("""
.include "z2r.inc"

.segment "PRG1"
.org $82BA ; death mountain
    jsr StoreElevatorParamDM

.segment "PRG1", "PRG7"
.reloc
StoreElevatorParamDM:
    lda $0731 ; load the current elevator command
    and #$0f ; the last 4 bits are the param
    sta ElevatorYStart
    lda $010A ; re-do the command JSR overwrote
    rts

.segment "PRG4"
.org $823E ; palace 1-6
    jsr StoreElevatorParam

.segment "PRG4", "PRG7"
.reloc
StoreElevatorParam:
    lda $0731 ; load the current elevator command
    and #$0f ; the last 4 bits are the param
    sta ElevatorYStart
    lda $010A ; re-do the command JSR overwrote
    rts

.segment "PRG5"
.org $824F ; palace 7
    jsr StoreElevatorParamGP

.segment "PRG5", "PRG7"
.reloc
StoreElevatorParamGP:
    lda $0731 ; load the current elevator command
    and #$0f ; the last 4 bits are the param
    sta ElevatorYStart
    lda $010A ; re-do the command JSR overwrote
    rts

.segment "PRG0", "PRG7"
.org $9169
    lda ElevatorYStart ; load param here to utilize the bytes
    jsr SetElevatorYPosition

.segment "PRG7"
.reloc
SetElevatorYPosition:
    ; we will do: A = 0x98 - (ElevatorParam * 8)
    asl
    asl
    asl
    ; optimized way to do A = 0x98 - A
    ; carry is 0 from asl, which makes sbc subtract 1 more
    sbc #$98 ; A = A - (0x98 + 1)
    eor #$FF ; invert bits (two's complement negation)
    sta $2A ; store the elevator y position. was originally always 0x98
    lda #$98
    sta $BC ; keeping this at 0x98 - changing it lead to weird bugs!
    rts
""", "change_elevator_y_position.s");
    }

    public void FixElevatorPositionInFallRooms(Assembler a)
    {
        // When falling into a fallroom, it will also run the elevator enemy setup code
        // which causes the elevator to try and position at link's height. But in a fall
        // room when you fall into it, then we want the elevator to spawn at the default
        // position. We pull this off by setting $0705 to `2` instead of `1` when falling
        // and checking for exactly 1 when updating the elevator position
        a.Module().Code("""
.segment "PRG0", "PRG7"

.org $C68D
    ; This is the entry point for the "falling down" game mode
    jmp SetFlagForFalling

.reloc
SetFlagForFalling:
    ; The next code block will also inc this value, so this sets it to two.
    inc $0705
    ; instead of returning, jump to where the original code was planning on going
    jmp $C67C

.org $916F
; Patch the check to just check for the elevator falling
jsr CheckIfElevatorEntrance
bne $9188

.reloc
CheckIfElevatorEntrance:
    lda $0705
    cmp #1
    rts
""", "fix_elevator_position_in_fall_rooms.s");
    }

    public void DontCountExpDuringTalking(Assembler a)
    {
        a.Module().Code("""
.segment "PRG7"
; Patch the count up code and check to see if we are in a dialog
.org $d433
    jsr CheckForDialog
.reloc
CheckForDialog:
    lda $74c ; current dialog type
    beq OriginalPatchedCode
        ; We are in a dialog or something, so don't tick down
        ; by skipping ahead to after the counting code
        pla
        pla
        jmp $d477
OriginalPatchedCode: 
    lda $756 ; Low byte of EXP to add
    rts
""", "dont_count_during_talking.s");
    }

    public void Global5050Jar(Assembler a)
    {
        a.Module().Code("""
.include "z2r.inc"

.segment "PRG4"
; Patch the count up code and check to see if we are in a dialog
.org $AF78
    jsr IncreaseGlobal5050JarDropPRG4
    beq *+8
.reloc
IncreaseGlobal5050JarDropPRG4:
    inc Global5050JarDrop
    lda Global5050JarDrop
    and #1
    rts

.segment "PRG5"
; Patch the count up code and check to see if we are in a dialog
.org $A536
    jsr IncreaseGlobal5050JarDropPRG5
    beq *+8
.reloc
IncreaseGlobal5050JarDropPRG5:
    inc Global5050JarDrop
    lda Global5050JarDrop
    and #1
    rts
""", "global5050jar.s");
    }

    public void ReduceDripperVariance(Assembler a)
    {
        a.Module().Code("""
.include "z2r.inc"

.segment "PRG4"
.org $B9F0
jmp NextDripColor

.reloc
NextDripColor:
    beq DripReturn            ; regular RNG hits (A == 0 here)
    lda DripperRedCounter
    clc
    adc #$01
    cmp #$08
    bcc DripReturn            ; less than 8 red drips in a row (A != 0 here)
    lda #$00                  ; force 8th red drip to be blue
DripReturn:
    sta DripperRedCounter
    sta $044C,y
    rts
""", "reduce_dripper_variance.s");
    }

    public void InstantText(Assembler a)
    {
        a.Module().Code("""
.segment "PRG3"

DialogActionLoadTextPtr = $b480
DialogActionDrawBox = $b0d2
DrawTwoRowsOfTiles = $b2f2

DialogPtr = $569

; Swap the dialog action pointers for loading the text pointer and the dialog draw box
.org $b0b9 ; actions 4 and 5
    .word DialogActionLoadTextPtr
    .word DialogActionDrawBox

; Patch the count up code and check to see if we are in a dialog
.org $b10b
    jsr InstantDialog
.reloc
InstantDialog:
    ; Run the original code to draw two rows of tiles
    jsr DrawTwoRowsOfTiles
    ; save the Draw Macro current write offset since the later code uses y
    sty $362

    ; Save the values in $00 which is used later for calculating the attribute offset
    ; We could use any other unused zp temp (i originally used $09-0a) but this is just
    ; extremely safe and it doesn't add lag to save/restore here since its in a menu load
    lda $00
    pha
    lda $01
    pha

    ; Check the current line, no text on lines 0-1 so skip it
    lda $525
    beq Exit

    lda #$60 ; 60 = typewriter sound
    sta $ec  ; Sound Effects Type 1
    ; If the current offset isn't on a $f4 tile, then move ahead until we get to it
    ; This will happen if the text box is split across multiple nametables
    ldx #0
FindNextF4:
    lda $368,x
    cmp #$f4
    beq StartReadingLetters
        inx
        bne FindNextF4
StartReadingLetters:
    ldy #0
    ; Load the current pointer for the text and see if we reached the end.
    lda DialogPtr+0
    sta $00
    lda DialogPtr+1
    sta $01
    lda ($00),y
ReadLetterLoop:
    ; if its FF then its end of string so just exit
    cmp #$ff
    beq ExitUpdatePtr
    ; Read each letter until the end of the line
    cmp #$fc
    bcs NextLine
    ; Draw the current letter over the blank space that we are rendering
    sta $368,x
    ; Move to the next spot in ram and check that its a blank space before writing.
    inx
FindNextF4Again:
    lda $368,x
    cmp #$f4
    beq F4Found
        inx
        bne FindNextF4Again
F4Found:
    iny
    lda ($00),y
    jmp ReadLetterLoop
NextLine:
    ; Letters $fc-$fe indicate go to next line
    iny
ExitUpdatePtr:
    ; Store current dialog pointer back into the text pointer
    tya
    clc
    adc DialogPtr+0
    sta DialogPtr+0
    bcc Exit
        inc DialogPtr+1
Exit:
    ; restore the saved values in $00 and $01
    pla
    sta $01
    pla
    sta $00
    ldy $362
    rts
""", "instant_text.s");
    }

    public void ChangeLavaKillPosition(Assembler asm)
    {
        // Don't grab the player at the top pixels of the lava block.
        // This way we can have "floor level" lava without being sucked into it when we are close.
        var a = asm.Module();
        a.Code("""
.include "z2r.inc"
.segment "PRG7"

closest_rts = $E0E5

.org $E0A4
HandleLavaTileCollision:
    lda $29                          ; load Link's y pos
    and #$0f                         ; mask the last 4 bits, to get the Link's pixel position inside the tile
    cmp #$06                         ; check that we are deep into the lava tile
    bcc closest_rts                  ; Link's position is not low enough, return
    jmp ActualLavaDeath
FREE_UNTIL $E0B0

.reloc
ActualLavaDeath:                     ; original code that we replaced
    lda #$01
    sta $e9                          ; $e9 = 01
    lda #$10
    sta $050c                        ; timer for Link being in injured state = 10
    inc $b5                          ; increase Link's state to 2, meaning Link will die
    rts
""");
    }

    /**
     * The function in bank 4 $9C45 (file offset 0x11c55) and bank 5 $A4E9 (file offset 0x164f9)
     * are divide functions that are used to display the HP bar for bosses and split it into 8 segments.
     * Inputs - A = divisor; X = enemy slot
     *
     * This function updates all the call sites to these two functions to match the HP for the boss.
     */
    private static readonly List<int> bossHpAddresses = [
        0x11451, // Horsehead
        0x13C86, // Helmethead
        0x12951, // Rebonack
        0x13041, // Unhorsed Rebonack
        0x12953, // Carock
        0x13C87, // Gooma
        0x12952, // Barba
        // These are bank 5 enemies so we should make a separate table for them
        // but we can deal with these when we start randomizing their hp
        // 0x15453, // Thunderbird
        // 0x15454, // Dark Link
    ];
    private static readonly List<(int, int)> bossMap = [
        (bossHpAddresses[0], 0x13b80), // Horsehead
        (bossHpAddresses[1], 0x13ae2), // Helmethead
        (bossHpAddresses[2], 0x12fd2), // Rebonack
        (bossHpAddresses[3], 0x1325c), // Unhorsed Rebonack
        (bossHpAddresses[4], 0x12e92), // Carock
        (bossHpAddresses[5], 0x134cf), // Gooma
        (bossHpAddresses[6], 0x13136), // Barba
        // 0x13ae9 - unknown; who is this? I'm guessing its a rebonack mini boss thing?
        // (0x15453, 0x16406), // Thunderbird
        // (0x15454, 0x158aa), // Dark Link
    ];
    private void UpdateAllBossHpDivisor(AsmModule a)
    {
        a.Code(/* lang=s */$"""
.include "z2r.inc"
.segment "PRG4"
.reloc
Bank4BossHpDivisorLo:
    .byte BOSS_0_HP_DIVISOR_LO, BOSS_1_HP_DIVISOR_LO, BOSS_2_HP_DIVISOR_LO
    .byte BOSS_3_HP_DIVISOR_LO, BOSS_4_HP_DIVISOR_LO, BOSS_5_HP_DIVISOR_LO
    .byte BOSS_6_HP_DIVISOR_LO

.reloc
Bank4BossHpDivisorHi:
    .byte BOSS_0_HP_DIVISOR_HI, BOSS_1_HP_DIVISOR_HI, BOSS_2_HP_DIVISOR_HI
    .byte BOSS_3_HP_DIVISOR_HI, BOSS_4_HP_DIVISOR_HI, BOSS_5_HP_DIVISOR_HI
    .byte BOSS_6_HP_DIVISOR_HI

.define BossHpLo $00
.define BossHpHi $01
.define DivisorLo $02
.define DivisorHi $03

.org $9C45
    tay
    lda $C2,x ; Boss HP
    sta BossHpHi
    lda Bank4BossHpDivisorLo,y
    jsr DoDivisionByRepeatedSubtraction
    nop
.assert * = $9C51
.org $9C7A
    jmp HandleOverHP 
.reloc
HandleOverHP:
    dey ; 1 or below means that the boss is 100% or less HP, so no over health
    bmi @Exit
        ; Change the tile ID to represent over health on a boss.
        ldx #$1c
        lda #$c5
@overhp:
        sta $02c1, x
        dex
        dex
        dex
        dex
        dey
        bpl @overhp  
@Exit:
    ; Do the original code
    ldx $10
    rts

.reloc
DoDivisionByRepeatedSubtraction:
    sta DivisorLo
    lda Bank4BossHpDivisorHi,y
    sta DivisorHi
    ldy #0
    sty BossHpLo
    sec
    @loop:
        lda BossHpLo
        sbc DivisorLo
        sta BossHpLo
        lda BossHpHi
        sbc DivisorHi
        sta BossHpHi
        iny
        bcs @loop
    rts

""");
        foreach (var (hpaddr, divisoraddr) in bossMap)
        {
            int hp = GetByte(hpaddr);
            Put(divisoraddr, (byte)(hp / 8));
        }
    }

    public void RandomizeEnemyStats(Assembler asm, Random RNG)
    {
        var a = asm.Module();
        // bank1_Enemy_Hit_Points at 0x5431 + Enemy ID (Overworld West)
        RandomizeHP(a, RNG, 0x5434, 0x5453);
        // bank2_Enemy_Hit_Points at 0x9431 + Enemy ID (Overworld East)
        RandomizeHP(a, RNG, 0x9434, 0x944E);
        // bank4_Enemy_Hit_Points0 at 0x11431 + Enemy ID (Palace 125)
        RandomizeHP(a, RNG, 0x11434, 0x11435); // Myu, Bot
        RandomizeHP(a, RNG, 0x11437, 0x11454); // Remaining palace enemies
        // bank4_Enemy_Hit_Points1 at 0x12931 + Enemy ID (Palace 346)
        RandomizeHP(a, RNG, 0x12934, 0x12935); // Myu, Bot
        RandomizeHP(a, RNG, 0x12937, 0x12954); // Remaining palace enemies
        // bank4_Table_for_Helmethead_Gooma
        RandomizeHP(a, RNG, 0x13C86, 0x13C87); // Helmethead, Gooma
        // bank5_Enemy_Hit_Points at 0x15431 + Enemy ID (Great Palace)
        RandomizeHP(a, RNG, 0x15434, 0x15435); // Myu, Bot
        RandomizeHP(a, RNG, 0x15437, 0x15438); // Moa, Ache
        RandomizeHP(a, RNG, 0x1543B, 0x1543B); // Acheman
        RandomizeHP(a, RNG, 0x15440, 0x15443); // Bago Bagos, Ropes
        RandomizeHP(a, RNG, 0x15445, 0x1544B); // Bubbles, Dragon Head, Fokkas
        RandomizeHP(a, RNG, 0x1544E, 0x1544E); // Fokkeru
        RandomizeHP(a, RNG, 0x13041,  0x13041); // Unhorsed Rebo

        // Add the new HP divisors for the bosses
        UpdateAllBossHpDivisor(a);
    }

    public void UseOHKOMode(Assembler asm)
    {
        var a = asm.Module();
        a.Segment("PRG7");
        var WriteHPValue = (int romorg, byte byt) =>
        {
            var segment = (romorg - 0x10) / 0x4000;
            a.Segment($"PRG{segment}");
            a.RomOrg(romorg);
            a.Byt(byt);
        };
        for (var i = 0; i < 8; i++)
        {
            WriteHPValue(0x1E67D + i, 192);
        }
        WriteHPValue(0x05432, 193);
        WriteHPValue(0x09432, 193);
        WriteHPValue(0x11436, 193);
        WriteHPValue(0x12936, 193);
        WriteHPValue(0x15532, 193);
        WriteHPValue(0x11437, 192);
        WriteHPValue(0x1143F, 192);
        WriteHPValue(0x12937, 192);
        WriteHPValue(0x1293F, 192);
        WriteHPValue(0x15445, 192);
        WriteHPValue(0x15446, 192);
        WriteHPValue(0x15448, 192);
        WriteHPValue(0x15453, 193);
        WriteHPValue(0x12951, 227); // Rebonack HP
    }

    private void RandomizeHP(AsmModule a, Random RNG, int start, int end)
    {
        var segment = (start - 0x10) / 0x4000;
        a.Segment($"PRG{segment}");
        a.RomOrg(start);
        for (var i = start; i <= end; i++)
        {
            int val = GetByte(i);

            var newVal = Math.Min(RNG.Next((int)(val * 0.5), (int)(val * 1.5)), 255);

            a.Byt((byte)newVal);
            var idx = bossHpAddresses.IndexOf(i);
            // If this isn't a boss skip adding it to the boss HP table
            if (idx <= -1) continue;
            var (_, addr) = bossMap[idx];
            var originalDivisor = GetByte(addr);
            a.RomOrg(addr);
            a.Byt((byte)idx); // Write the index of the boss to the old HP spot
            // we keep the original divisor, but add a remainder value.
            // This way the boss can accurately represent over and under HP values
            a.Assign($"BOSS_{idx}_HP_DIVISOR_HI", originalDivisor);
            // Take the remainder, and convert it into a fractional value out of 256 values
            // The +1 works around an issue when the boss HP is exactly a multiple of the divisor
            a.Assign($"BOSS_{idx}_HP_DIVISOR_LO", (newVal % originalDivisor) * (256 / originalDivisor) + 1);
            // restore the previous ORG for writing the next byte in the list
            a.RomOrg(i+1);
        }
    }

    public void FixItemPickup(Assembler asm)
    {
        // In Z2R, Link never holds items above his head. So,
        // we don't set the hold item over head timer ($0x49c), and
        // we don't set the hold item over head ID ($0x49d).
        // (If we wanted we could remove all code using these)
        //
        // Instead, we clear out $a8,x to fix the item pickup phantom damage,
        // caused by generator code that interprets it as collision data.
        //
        // Also, since 1-ups can drop anywhere, if we're picking up a 1-up
        // we don't reset the velocity.
        var a = asm.Module();
        a.Code(/* lang=s */"""
.include "z2r.inc"
.segment "PRG7"
.org $e53b
SetPostItemPickupVars:
    lda $af,x                          ; this byte has the item ID we picked up
    and #$7f                           ; keep bits .xxx xxxx
    cmp #$12                           ; check if item is 1-up
    beq SetPostItemPickupKeepVelocity  ; skip resetting velocity if 1-up
    lda #$00
    sta $70                            ; set Link's X velocity to zero
    sta $57d                           ; set Link's Y velocity to zero
SetPostItemPickupKeepVelocity:
    lda #$00
    sta $a8,x                          ; clear item/enemy collision byte to prevent phantom damage
    rts

FREE_UNTIL $e54f

""");
    }

    public void FixMinibossGlitchyAppearance(Assembler asm)
    {
        var a = asm.Module();
        a.Code(/* lang=s */"""
.include "z2r.inc"
.import SwapCHR

EnemyFacingDirection = $DC91
CurrentPRGBank = $0769
CurrentCHRBank = $076E

.segment "PRG7"

; Patch the start of the sideview initialization to check if the enemy is loaded in the first screen
; This is after switching the CHR banks for the sideview
.org $C638
    jmp CheckToOverwriteChrBank

.reloc
CheckToOverwriteChrBank:
; If A = $20 then we are horsehead/rebo
    ldx #6
@loop:
        lda $a1 - 1,x
        cmp #$20
        bne @NotHorsehead
            jsr OverwriteSpriteCHRBank
@NotHorsehead:
        dex
        bne @loop
    jmp SideViewInit

.reloc
OverwriteSpriteCHRBank:
    ; We are loading that enemy, so switch the sprite banks based on which palace we are in
    lda WorldNumber ; 3 = palace group 1,2,5 ; 4 = palace group 3,4,6
    cmp #$03 ; we are fighting a Horsehead since this is palace set 4
    beq @LoadHorsehead
        cmp #4
        bne @Exit
        lda #$18 * 4 + 4 ; CHR bank for rebo as mini boss
        bne @WriteCHRBanks ; unconditional (we can't use BIT $abs here safely)
@LoadHorsehead:
    lda #$0a * 4 + 4 ; CHR bank for horsehead as mini boss
@WriteCHRBanks:
    ; switch two banks which is enough for both mini bosses
    sta SpChrBank4Reg
    clc
    adc #1
    sta SpChrBank5Reg
    ; due to an MMC5 issue, we need to write a bg bank as well.
    lda CurrentCHRBank
    asl
    asl
    ; clc ; carry is clear here
    adc #4
    sta BgChrBank0Reg
@Exit:
    rts

; Patch the enemy loading routine to check if the enemy is horsehead/rebo
.org $D68B
    jsr CheckIfHorseheadReboshark
.reloc
CheckIfHorseheadReboshark:
    ; If A = $20 then we are horsehead/rebo
    cmp #$20
    bne @Exit
        jsr OverwriteSpriteCHRBank
@Exit:
    jmp EnemyFacingDirection

""");
    }

    public void FixBossKillPaletteGlitch(Assembler asm)
    {
        // Restore red palette color that is set to black for Link's shadow during boss explosions
        var a = asm.Module();
        a.Code(/* lang=s */"""
.segment "PRG7"
.org $DE1A
HookIntoSpawnBossItem:
    jmp RestorePaletteAfterBossKill

.reloc
RestorePaletteAfterBossKill:
    sta $af,x ; command overwritten by jmp
    ldx #$00
    ldy $362
@CopyLoop:
    lda ResetRedPalettePayload,x
    sta $0363,y
    inx
    iny
    cpx #$08
    bne @CopyLoop
    lda #$02
    sta $0725 ; setting PPU macro 2
    dey
    sty $362
    ldx $10
    rts

.reloc
ResetRedPalettePayload:
    ; 8 byte palette payload for PPU macro
    .byte $3f, $18, $04, $0f, $06, $16, $30, $ff

""");
    }

    public void BuffCarrock(Assembler a)
    {
        a.Module().Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.BuffCarock.s"), "buff_carock.s");
    }

    public void DashSpell(Assembler asm)
    {
        var a = asm.Module();
        a.Code(Util.ReadResource("Z2Randomizer.RandomizerCore.Asm.DashSpell.s"), "dash_spell.s");

        byte[] dash = Util.ToGameText("DASH", false).Select(x => (byte)x).ToArray();
        a.Org(0x9c62);
        a.Byt(dash);
    }

    public void CombineFireSpell(Assembler asm, List<Collectable>? customSpellOrder, Random RNG)
    {
        // These are the bit flags for each spell.  The old implementation
        // changed this table directly. However as this table is also used in
        // the BIT comparison operation when determining if a spell should be
        // cast, it lead to inconsistent behavior where some spells would be
        // two-way linked and some would not.
        byte[] spellBytes = GetBytes(0xdcb, 8);

        int fireSpellIndex = 4;
        int r = RNG.Next(7);
        int linkedSpellIndex = r > 3 ? r + 1 : r;

        byte combinedSpellBits = (byte)(spellBytes[linkedSpellIndex] | spellBytes[fireSpellIndex]);
        spellBytes[fireSpellIndex] = combinedSpellBits;
        spellBytes[linkedSpellIndex] = combinedSpellBits;

        byte[] finalSpellBytes = customSpellOrder != null
            ? customSpellOrder.Select(s => spellBytes[s.VanillaSpellOrder()]).ToArray()
            : spellBytes;

        var a = asm.Module();
        a.Segment("PRG0");
        a.Reloc();
        a.Label("CombinedSpellTable");
        a.Byt(finalSpellBytes);
        // When a spell is cast, load from our new table instead
        a.Org(0x8dfb);
        a.Code("lda CombinedSpellTable,Y");
    }

    public void MoveAfterGem()
    {
        Put(0x11b15, new byte[] { 0xea, 0xea });

        Put(0x11af5, new byte[] { 0x47, 0x9b, 0x56, 0x9b, 0x35, 0x9b });
    }

    public void ElevatorBossFix(bool bossItem)
    {
        /*
         * Notes:
         * 
         * Screen lock set at bank 4 BE99 (0x13ea9)
         * Screen lock tbird set at bank 5 A363 (0x16373) 
         * Screen lock released at bank 7 E7A9 (0x1e7b9)
         * 
         * Jump to subroutine at all three locations above:
         * 
         * 20 40 F3
         * 
         * Subroutine at 1F350:
         * 
         * Load accumulator with 1 (A9 01)
         * EOR 728 (4D 28 07)
         * store 728 (8D 28 07)
         * load accumulator with 13 (A9 13)
         * compare a2 with 0x13 (C5 A2)
         * branch if not equal to the end (D0 0A)
         * Load accumulator with 1 (a9 01)
         * EOR b6 (45 b6)
         * store b6 (85 b6)
         * load accumulator with a0 (a9 a0)
         * Set 2a to 0xa0 (85 2a)
         * return (60)
         */

        // jsr $f340
        var jsrF340 = new byte[] { 0x20, 0x40, 0xF3 };
        Put(0x13ea9, jsrF340);
        Put(0x16373, jsrF340);
        Put(0x13230, jsrF340);

        if (!bossItem)
        {
            Put(0x1e7b9, jsrF340);
        }
        else
        {
            Put(0x1e7b1, jsrF340);
        }

        /*
         * Patched function at $f340
         * lda #1
         * eor $0728   ; _728_FreezeScrolling un freeze scrolling if frozen, otherwise freeze it.
         * sta $0728   ; _728_FreezeScrolling
         * lda #$13     ; check if the enemy id is 0x13
         * cmp $a1     ; enemy slot "6" (index 0) current ID
         * bne + ; * + 10
         * lda #1      ; if it is equal, then flip $b6 (holds the enemy status?)
         * eor $b6
         * sta $b6
         * lda #$a0    ; and set the enemy y position to 0xa0
         * sta $2a
         * +
         * rts
         */
        Put(0x1F350, new byte[] { 0xa9, 0x01, 0x4d, 0x28, 0x07, 0x8d, 0x28, 0x07, 0xa9, 0x13, 0xc5, 0xa1, 0xd0, 0x0a, 0xa9, 0x01, 0x45, 0xb6, 0x85, 0xb6, 0xa9, 0xa0, 0x85, 0x2a, 0x60 });
    }

    public void AdjustGpProjectileDamage()
    {
        // We are using some enemies that are not present in vanilla
        // Great Palace, some of which use bytes that are not set
        // properly in bank 5 (the GP bank) as they never got used.

        // ACHEMAN projectile
        // West damage class: 1 (0x81)
        // GP vanilla damage class: 6 (0x86)
        //
        // The damage class byte in bank 5 is likely from copying
        // bank 4 where it is used for Barba's projectile damage.
        // Lets lower it a bit, but not all the way to 1.
        Put(0x1542b, 0x83);

        // BUBBLE_GENERATOR
        // West damage class: 0 (0x80)
        // East damage class: 0 (0x80)
        // Vanilla GP damage class: 3 (0x03)
        //
        // The damage class byte in bank 5 is likely from copying
        // bank 4 where it is used for Helmethead's main projectile.
        // The 0x80 bit determines if Reflect is necessary to block.
        // Since the GP sprite is an Energy Ball it makes sense to not
        // require Reflect, so setting it to 0.
        Put(0x15429, 0x00);

        // ROCK_GENERATOR
        // West damage class: 0 (0x00)
        // Vanilla GP damage class: 0 (0x00)
        //Put(0x15428, 0x00); // already at 0
    }

    /// When Rebonack's HP is set to exactly 2 * your damage, it will
    /// trigger a bug where you kill Rebo's horse while de-horsing him.
    /// This causes an additional key to drop, as well as softlocking
    /// the player if they die before killing Rebo. It seems to also
    /// trigger if you have exactly damage == Rebo HP (very high damage).
    /// 
    /// This has to be called after RandomizeEnemyStats and
    /// RandomizeAttackEffectiveness.
    ///
    /// (In Vanilla Zelda 2, your sword damage is never this high.)
    public void FixRebonackHorseKillBug()
    {
        byte[] attackValues = GetBytes(0x1E67D, 8);
        byte reboHp = GetByte(0x12951);
        while (attackValues.Any(v => v * 2 == reboHp || v == reboHp))
        {
            reboHp++;
            Put(0x12951, reboHp);
        }
    }

    /// Rewrite the graphic tiles for walkthrough walls to be something else
    public void RevealWalkthroughWalls()
    {
        // 0x02 is the background tiles in a different color
        // another option could be: 0x00 for just fully black
        // or curtains: 0xCF 0xD0 (regular palaces) and 0xC0 0xC1 (GP)
        Put(0x1019d, [0x02, 0x02]); // regular palaces
        Put(0x141b3, [0x02, 0x02]); // GP
    }

    public static string Z2BytesToString(byte[] data)
    {
        return new string(data.Select(letter => {
            return ReverseCharMap.TryGetValue(letter, out var chr) ? chr : ' ';
        }).ToArray());
    }

    public static byte[] StringToZ2Bytes(string text)
    {
        return text.Select(letter => {
            return CharMap.TryGetValue(letter, out var byt) ? byt : (byte)0xfc;
        }).ToArray();
    }

    private static readonly IDictionary<char, byte> CharMap = new Dictionary<char, byte>()
    {
        { '$', 0xc9 }, // sword
        { '#', 0xca }, // filled box
        { '=', 0xcb }, // horizontal border
        { '|', 0xcc }, // vertical border
        { '+', 0xcd }, // gem
        { '/', 0xce },
        { '.', 0xcf },
        { '0', 0xd0 },
        { '1', 0xd1 },
        { '2', 0xd2 },
        { '3', 0xd3 },
        { '4', 0xd4 },
        { '5', 0xd5 },
        { '6', 0xd6 },
        { '7', 0xd7 },
        { '8', 0xd8 },
        { '9', 0xd9 },
        { 'A', 0xda },
        { 'B', 0xdb },
        { 'C', 0xdc },
        { 'D', 0xdd },
        { 'E', 0xde },
        { 'F', 0xdf },
        { 'G', 0xe0 },
        { 'H', 0xe1 },
        { 'I', 0xe2 },
        { 'J', 0xe3 },
        { 'K', 0xe4 },
        { 'L', 0xe5 },
        { 'M', 0xe6 },
        { 'N', 0xe7 },
        { 'O', 0xe8 },
        { 'P', 0xe9 },
        { 'Q', 0xea },
        { 'R', 0xeb },
        { 'S', 0xec },
        { 'T', 0xed },
        { 'U', 0xee },
        { 'V', 0xef },
        { 'W', 0xf0 },
        { 'X', 0xf1 },
        { 'Y', 0xf2 },
        { 'Z', 0xf3 },
        { ' ', 0xf4 },
        { '-', 0xf6 },
    };

    private static readonly IDictionary<byte, char> ReverseCharMap = CharMap.ToDictionary(x => x.Value, x => x.Key);

    public Terrain[,] ReadVanillaMap(ROM romData, int mapAddr, int mapRows, int mapCols)
    {
        int addr = mapAddr;
        int i = 0;
        int j = 0;
        Terrain[,] map = new Terrain[mapRows, mapCols];
        while (i < mapRows)
        {
            j = 0;
            while (j < mapCols)
            {
                byte data = romData.GetByte(addr);
                int count = (data & 0xF0) >> 4;
                count++;
                Terrain t = (Terrain)(data & 0x0F);
                for (int k = 0; k < count; k++)
                {
                    map[i, j + k] = t;
                }
                j += count;
                addr++;
            }
            i++;
        }

        return map;
    }

    public List<Location> LoadLocations(int startAddr, int locNum, SortedDictionary<int, Terrain> Terrains, Continent continent)
    {
        List<Location> locations = [];
        for (int i = 0; i < locNum; i++)
        {
            int addr = startAddr + i;
            Terrain terrain = Terrains[addr];
            var location = LoadLocation(addr, terrain, continent);
            locations.Add(location);
        }
        return locations;
    }

    public Location LoadLocation(int addr, Terrain terrain, Continent continent)
    {
        byte yByte = GetByte(addr);
        byte xByte = GetByte(addr + RomMap.overworldXOffset);
        byte mapByte = GetByte(addr + RomMap.overworldMapOffset);
        byte worldByte = GetByte(addr + RomMap.overworldWorldOffset);
        int yPos = yByte & 0x7f;
        int xPos = xByte & 0x3f;
        int map = mapByte & 0x3f;
        return new Location(yPos, xPos, addr, map, continent)
        {
            ExternalWorld = yByte & 0x80,
            appear2loweruponexit = xByte & 0x80,
            Secondpartofcave = xByte & 0x40,
            MapPage = mapByte & 0xC0,
            FallInHole = worldByte & 0x80,
            PassThrough = worldByte & 0x40,
            ForceEnterRight = worldByte & 0x20,
            TerrainType = terrain,
            AppearsOnMap = true
        };
    }

    public void RemoveUnusedConnectors(World world)
    {
        if (world.raft == null)
        {
            Put(world.baseAddr + 41, 0x00);
        }

        if (world.bridge == null)
        {
            Put(world.baseAddr + 40, 0x00);
        }

        if (world.cave1 == null)
        {
            Put(world.baseAddr + 42, 0x00);
        }

        if (world.cave2 == null)
        {
            Put(world.baseAddr + 43, 0x00);
        }
    }

    //This was refactored out of EastHyrule. The signature/timing/structure needs work.
    public void UpdateHiddenPalaceSpot(Biome biome, (int, int) hiddenPalaceCoords, Location hiddenPalaceLocation, 
        Location townAtNewKasuto, Location spellTower, bool vanillaShuffleUsesActualTerrain)
    {
        if (biome != Biome.VANILLA && biome != Biome.VANILLA_SHUFFLE)
        {
            Put(0x8382, (byte)hiddenPalaceCoords.Item1);
            Put(0x8388, (byte)hiddenPalaceCoords.Item2);
        }
        int pos = hiddenPalaceLocation.Ypos;

        Put(0x1df78, (byte)(pos + hiddenPalaceLocation.ExternalWorld));
        Put(0x1df84, 0xff);
        Put(0x1ccc0, (byte)pos);
        int connection = hiddenPalaceLocation.MemAddress - 0x862F;
        Put(0x1df76, (byte)connection);
        hiddenPalaceLocation.NeedRecorder = true;
        if (hiddenPalaceLocation == townAtNewKasuto || hiddenPalaceLocation == spellTower)
        {
            townAtNewKasuto.NeedRecorder = true;
            spellTower.NeedRecorder = true;
        }
        if (vanillaShuffleUsesActualTerrain || biome != Biome.VANILLA_SHUFFLE)
        {
            Put(0x1df74, (byte)hiddenPalaceLocation.TerrainType);
            if (hiddenPalaceLocation.TerrainType == Terrain.PALACE)
            {
                Put(0x1df7d, 0x60);
                Put(0x1df82, 0x61);

                Put(0x1df7e, 0x62);

                Put(0x1df83, 0x63);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.SWAMP)
            {
                Put(0x1df7d, 0x6F);
                Put(0x1df82, 0x6F);

                Put(0x1df7e, 0x6F);

                Put(0x1df83, 0x6F);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.LAVA || hiddenPalaceLocation.TerrainType == Terrain.WALKABLEWATER)
            {
                Put(0x1df7d, 0x6E);
                Put(0x1df82, 0x6E);

                Put(0x1df7e, 0x6E);

                Put(0x1df83, 0x6E);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.FOREST)
            {
                Put(0x1df7d, 0x68);
                Put(0x1df82, 0x69);

                Put(0x1df7e, 0x6A);

                Put(0x1df83, 0x6B);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.GRAVE)
            {
                Put(0x1df7d, 0x70);
                Put(0x1df82, 0x71);

                Put(0x1df7e, 0x7F);

                Put(0x1df83, 0x7F);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.ROAD)
            {
                Put(0x1df7d, 0xFE);
                Put(0x1df82, 0xFE);

                Put(0x1df7e, 0xFE);

                Put(0x1df83, 0xFE);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.BRIDGE)
            {
                Put(0x1df7d, 0x5A);
                Put(0x1df82, 0x5B);

                Put(0x1df7e, 0x5A);

                Put(0x1df83, 0x5B);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.CAVE)
            {
                Put(0x1df7d, 0x72);
                Put(0x1df82, 0x73);

                Put(0x1df7e, 0x72);

                Put(0x1df83, 0x73);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.DESERT)
            {
                Put(0x1df7d, 0x6C);
                Put(0x1df82, 0x6C);

                Put(0x1df7e, 0x6C);

                Put(0x1df83, 0x6C);
            }
            else if (hiddenPalaceLocation.TerrainType == Terrain.TOWN)
            {
                Put(0x1df7d, 0x5C);
                Put(0x1df82, 0x5D);

                Put(0x1df7e, 0x5E);

                Put(0x1df83, 0x5F);
            }
        }

        int ppu_addr1 = 0x2000 + 2 * (32 * (hiddenPalaceLocation.Ypos % 15) + (hiddenPalaceLocation.Xpos % 16)) + 2048 * (hiddenPalaceLocation.Ypos % 30 / 15);
        int ppu_addr2 = ppu_addr1 + 32;
        int ppu1low = ppu_addr1 & 0x00ff;
        int ppu1high = (ppu_addr1 >> 8) & 0xff;
        int ppu2low = ppu_addr2 & 0x00ff;
        int ppu2high = (ppu_addr2 >> 8) & 0xff;
        Put(0x1df7a, (byte)ppu1high);
        Put(0x1df7b, (byte)ppu1low);
        Put(0x1df7f, (byte)ppu2high);
        Put(0x1df80, (byte)ppu2low);

    }

    public void UpdateKasuto(Location hiddenKasutoLocation, Location townAtNewKasuto, Location spellTower, Biome biome,
        int baseAddr, Terrain hiddenKasutoTerrain, bool vanillaShuffleUsesActualTerrain)
    {
        Put(0x1df79, (byte)(hiddenKasutoLocation.Ypos + hiddenKasutoLocation.ExternalWorld));
        Put(0x1dfac, (byte)(hiddenKasutoLocation.Ypos - 30));
        Put(0x1dfb2, (byte)(hiddenKasutoLocation.Xpos + 1));
        Put(0x1ccd4, (byte)(hiddenKasutoLocation.Xpos + hiddenKasutoLocation.Secondpartofcave));
        Put(0x1ccdb, (byte)(hiddenKasutoLocation.Ypos));
        int connection = hiddenKasutoLocation.MemAddress - baseAddr;
        Put(0x1df77, (byte)connection);
        hiddenKasutoLocation.NeedHammer = true;
        if (hiddenKasutoLocation == townAtNewKasuto || hiddenKasutoLocation == spellTower)
        {
            townAtNewKasuto.NeedHammer = true;
            spellTower.NeedHammer = true;
        }
        if (vanillaShuffleUsesActualTerrain || biome != Biome.VANILLA_SHUFFLE)
        {
            //Terrain t = terrains[hiddenKasutoLocation.MemAddress];
            Put(0x1df75, (byte)hiddenKasutoTerrain);
            if (hiddenKasutoTerrain == Terrain.PALACE)
            {
                Put(0x1dfb6, 0x60);
                Put(0x1dfbb, 0x61);

                Put(0x1dfc0, 0x62);

                Put(0x1dfc5, 0x63);
            }
            else if (hiddenKasutoTerrain == Terrain.SWAMP)
            {
                Put(0x1dfb6, 0x6F);
                Put(0x1dfbb, 0x6F);

                Put(0x1dfc0, 0x6F);

                Put(0x1dfc5, 0x6F);
            }
            else if (hiddenKasutoTerrain == Terrain.LAVA || hiddenKasutoTerrain == Terrain.WALKABLEWATER)
            {
                Put(0x1dfb6, 0x6E);
                Put(0x1dfbb, 0x6E);

                Put(0x1dfc0, 0x6E);

                Put(0x1dfc5, 0x6E);
            }
            else if (hiddenKasutoTerrain == Terrain.FOREST)
            {
                Put(0x1dfb6, 0x68);
                Put(0x1dfbb, 0x69);

                Put(0x1dfc0, 0x6A);

                Put(0x1dfc5, 0x6B);
            }
            else if (hiddenKasutoTerrain == Terrain.GRAVE)
            {
                Put(0x1dfb6, 0x70);
                Put(0x1dfbb, 0x71);

                Put(0x1dfc0, 0x7F);

                Put(0x1dfc5, 0x7F);
            }
            else if (hiddenKasutoTerrain == Terrain.ROAD)
            {
                Put(0x1dfb6, 0xFE);
                Put(0x1dfbb, 0xFE);

                Put(0x1dfc0, 0xFE);

                Put(0x1dfc5, 0xFE);
            }
            else if (hiddenKasutoTerrain == Terrain.BRIDGE)
            {
                Put(0x1dfb6, 0x5A);
                Put(0x1dfbb, 0x5B);

                Put(0x1dfc0, 0x5A);

                Put(0x1dfc5, 0x5B);
            }
            else if (hiddenKasutoTerrain == Terrain.CAVE)
            {
                Put(0x1dfb6, 0x72);
                Put(0x1dfbb, 0x73);

                Put(0x1dfc0, 0x72);

                Put(0x1dfc5, 0x73);
            }
            else if (hiddenKasutoTerrain == Terrain.DESERT)
            {
                Put(0x1dfb6, 0x6C);
                Put(0x1dfbb, 0x6C);

                Put(0x1dfc0, 0x6C);

                Put(0x1dfc5, 0x6C);
            }
            else if (hiddenKasutoTerrain == Terrain.TOWN)
            {
                Put(0x1dfb6, 0x5C);
                Put(0x1dfbb, 0x5D);

                Put(0x1dfc0, 0x5E);

                Put(0x1dfc5, 0x5F);
            }
        }
    }

    public void UpdateItem(Collectable item, Room room)
    {
        int sideviewPtrAddr = room.GetSideviewPtrRomAddr();
        int sideviewNesPtr = GetShort(sideviewPtrAddr + 1, sideviewPtrAddr);
        // Sideview data is moved to the expanded banks at $1c/$1d
        // Currently no palace rooms are added to bank 7 but keeping this anyway.
        // Using $1c for both works because $1d address range is directly followed by $1c.
        int sideviewBank = sideviewNesPtr >= 0xC000 ? 0x7 : 0x1c;
        int sideviewRomPtr = ConvertNesPtrToPrgRomAddr(sideviewBank, sideviewNesPtr);

        byte sideviewLength = GetByte(sideviewRomPtr);
        int offset = 4;

        do
        {
            int yPos = GetByte(sideviewRomPtr + offset++);
            yPos = (byte)(yPos & 0xF0);
            yPos = (byte)(yPos >> 4);
            int byte2 = GetByte(sideviewRomPtr + offset++);

            if (yPos >= 13 || byte2 != 0x0F) continue;
            int byte3 = GetByte(sideviewRomPtr + offset++);

            if (((Collectable)byte3).IsMinorItem()) continue;
            Put(sideviewRomPtr + offset - 1, (byte)item);
            return;
        } while (offset < sideviewLength);
        logger.Warn($"Could not write Collectable {item} to Item room {room.GetDebuggerDisplay()} in palace {room.PalaceNumber}");
        //throw new Exception("Could not write Collectable to Item room in palace " + PalaceNumber);
    }
}
