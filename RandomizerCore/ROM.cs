using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using js65;
using NLog;
using RandomizerCore.Overworld;

namespace RandomizerCore;

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

    public static readonly IReadOnlyList<int> FreeRomBanks = new List<int>(Enumerable.Range(0x10, 0xf));

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

    public void Dump(string filename)
    {
        File.WriteAllBytes(filename, rawdata);
    }

    public List<Text> GetGameText()
    {
        List<Text> texts = [];
        for (int i = textPointerTableStart; i <= textAddrEndinROM; i += 2)
        {
            List<char> t = [];
            int addr = GetByte(i);
            addr += (GetByte(i + 1) << 8);
            addr += textAddrOffset;
            int c = GetByte(addr);
            while (c != STRING_TERMINATOR)
            {
                addr++;
                t.Add((char)c);
                c = GetByte(addr);
            }
            t.Add((char)0xFF);
            texts.Add(new Text(t));

        }
        return texts;
    }

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
        List<char> randoby = Util.ToGameText("RANDO BY  ", false);
        for (int i = 0; i < randoby.Count; i++)
        {
            Put(creditsLineOneAddr + i, (byte)randoby[i]);
        }

        List<char> digshake = Util.ToGameText("DIGSHAKE ", true);
        for (int i = 0; i < digshake.Count; i++)
        {
            Put(creditsLineTwoAddr + i, (byte)digshake[i]);
        }
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

    public void UpdateSprites(CharacterSprite charSprite, CharacterColor tunicColor, CharacterColor outlineColor, CharacterColor shieldColor, BeamSprites beamSprite)
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
            IpsPatcher.Patch(rawdata, charSprite.Patch, true);
        }

        var colorMap = new Dictionary<CharacterColor, int>()
        {
            { CharacterColor.Green, 0x2A },
            { CharacterColor.DarkGreen, 0x0A },
            { CharacterColor.Aqua, 0x3C },
            { CharacterColor.LightBlue, 0x11 },
            { CharacterColor.DarkBlue, 0x02 },
            { CharacterColor.Purple, 0x04 },
            { CharacterColor.Pink, 0x24 },
            { CharacterColor.Red, 0x16 },
            { CharacterColor.Orange, 0x27 },
            { CharacterColor.Turd, 0x18 },
            { CharacterColor.White, 0x30 },
            { CharacterColor.LightGray, 0x10 },
            { CharacterColor.DarkGray, 0x2d },
            { CharacterColor.Black, 0x0d },
        };

        int? tunicColorInt = null;
        int? outlineColorInt = null;
        int? shieldColorInt = null;
        // We won't write the default color to the ROM, but we set it
        // to avoid another random color rolling the same value.
        if (tunicColor != CharacterColor.Random)
        {
            tunicColorInt = colorMap[tunicColor != CharacterColor.Default ? tunicColor : CharacterColor.Green];
        }
        if (outlineColor != CharacterColor.Random)
        {
            outlineColorInt = colorMap[outlineColor != CharacterColor.Default ? outlineColor : CharacterColor.Turd];
        }
        if (shieldColor != CharacterColor.Random)
        {
            shieldColorInt = colorMap[shieldColor != CharacterColor.Default ? shieldColor : CharacterColor.Red];
        }
        if (tunicColor == CharacterColor.Random)
        {
            Random r2 = new Random();
            int c2p1 = r2.Next(3);
            int c2p2 = r2.Next(1, 13);
            tunicColorInt = c2p1 * 16 + c2p2;

            while (tunicColorInt == outlineColorInt || tunicColorInt == shieldColorInt)
            {
                c2p1 = r2.Next(3);
                c2p2 = r2.Next(1, 13);
                tunicColorInt = c2p1 * 16 + c2p2;
            }
        }
        if (outlineColor == CharacterColor.Random)
        {
            Random r2 = new Random();
            int c2p1 = r2.Next(3);
            int c2p2 = r2.Next(1, 13);
            outlineColorInt = c2p1 * 16 + c2p2;

            while (outlineColorInt == tunicColorInt || outlineColorInt == shieldColorInt)
            {
                c2p1 = r2.Next(3);
                c2p2 = r2.Next(1, 13);
                outlineColorInt = c2p1 * 16 + c2p2;
            }
        }
        if(shieldColor == CharacterColor.Random)
        {
            Random r2 = new Random();
            int c2p1 = r2.Next(3);
            int c2p2 = r2.Next(1, 13);
            shieldColorInt = c2p1 * 16 + c2p2;

            while(shieldColorInt == tunicColorInt || shieldColorInt == outlineColorInt)
            {
                c2p1 = r2.Next(3);
                c2p2 = r2.Next(1, 13);
                shieldColorInt = c2p1 * 16 + c2p2;
            }
        }

        int[] tunicLocs = { 0x10ea, 0x285C, 0x40b1, 0x40c1, 0x40d1, 0x80e1, 0x80b1, 0x80c1, 0x80d1, 0x80e1, 0xc0b1, 0xc0c1, 0xc0d1, 0xc0e1, 0x100b1, 0x100c1, 0x100d1, 0x100e1, 0x140b1, 0x140c1, 0x140d1, 0x140e1, 0x17c1b, 0x1c466, 0x1c47e };
        int[] outlineLocs = { 0x285a, 0x2a0a, 0x40af, 0x40bf, 0x40cf, 0x40df, 0x80af, 0x80bf, 0x80cf, 0x80df, 0xc0af, 0xc0bf, 0xc0cf, 0xc0df, 0xc0ef, 0x100af, 0x100bf, 0x100cf, 0x100df, 0x140af, 0x140bf, 0x140cf, 0x140df, 0x17c19, 0x1c464, 0x1c47c };
        int shieldLoc = 0xe9e;

        if(tunicColor != CharacterColor.Default && tunicColorInt != null)
        { 
            foreach(int l in tunicLocs)
            {
                Put(l, (byte)tunicColorInt);
            }
        }
        if(outlineColor != CharacterColor.Default && outlineColorInt != null)
        {
            foreach(int l in outlineLocs)
            {
                Put(l, (byte)outlineColorInt);
            }
        }
        if(shieldColor != CharacterColor.Default && shieldColorInt != null)
        {
            Put(shieldLoc, (byte)shieldColorInt);
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

        //Disable hold over head animation
        Put(0x1E54C, 0);

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

    public void SetLevelCap(int atkMax, int magicMax, int lifeMax)
    {

        //jump to check which attribute is levelling up
        Put(0x1f8a, 0x4C);
        Put(0x1f8b, 0x9e);
        Put(0x1f8c, 0xa8);

        ////x = 2 life, x = 1 magic, x = 0 attack
        //load current level for (attack, magic, life)
        //compare to address of cap
        //go back to $9f7f

        //BD 77 07 (load level)
        //DD A7 A8
        //4C 7F 9F

        Put(0x28AE, 0xBD);
        Put(0x28AF, 0x77);
        Put(0x28B0, 0x07);
        Put(0x28B1, 0xDD);
        Put(0x28B2, 0xA7);
        Put(0x28B3, 0xA8);
        Put(0x28B4, 0x4C);
        Put(0x28B5, 0x7F);
        Put(0x28B6, 0x9F);

        //these are the actual caps
        Put(0x28B7, (byte)atkMax);
        Put(0x28B8, (byte)magicMax);
        Put(0x28B9, (byte)lifeMax);


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

    public void ChangeMapperToMMC5(Assembler a)
    {
        a.Module().Code(Util.ReadResource("RandomizerCore.Asm.MMC5.s"), "mmc5_conversion.s");
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
        a.Code(Util.ReadResource("RandomizerCore.Asm.Recoil.s"), "recoil.s");
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
; TODO: Add RAM segment support to js65 for allocating variables
;.segment "SHORTRAM"
;Global5050JarDrop: .res 1
Global5050JarDrop = $123 ; $0123

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

    public void BuffCarrock(Assembler a)
    {
        a.Module().Code(Util.ReadResource("RandomizerCore.Asm.BuffCarock.s"), "buff_carock.s");
    }

    public void DashSpell(Assembler asm)
    {
        var a = asm.Module();
        a.Code(Util.ReadResource("RandomizerCore.Asm.DashSpell.s"), "dash_spell.s");

        byte[] dash = Util.ToGameText("DASH", false).Select(x => (byte)x).ToArray();
        a.Org(0x9c62);
        a.Byt(dash);
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

    public string Z2BytesToString(byte[] data)
    {
        return new string(data.Select(letter => {
            return ReverseCharMap.TryGetValue(letter, out var chr) ? chr : ' ';
        }).ToArray());
    }

    public byte[] StringToZ2Bytes(string text)
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
        List<Location> locations = new List<Location>();
        for (int i = 0; i < locNum; i++)
        {
            byte[] bytes = [
                GetByte(startAddr + i),
                GetByte(startAddr + RomMap.overworldXOffset + i),
                GetByte(startAddr + RomMap.overworldMapOffset + i),
                GetByte(startAddr + RomMap.overworldWorldOffset + i)
            ];
            Location location = new Location(bytes[0] & 127, bytes[1] & 63, startAddr + i, bytes[2] & 63, continent)
            {
                ExternalWorld = bytes[0] & 128,
                appear2loweruponexit = bytes[1] & 128,
                Secondpartofcave = bytes[1] & 64,
                MapPage = bytes[2] & 192,
                FallInHole = bytes[3] & 128,
                PassThrough = bytes[3] & 64,
                ForceEnterRight = bytes[3] & 32,
                TerrainType = Terrains[startAddr + i],
                AppearsOnMap = true
            };
            locations.Add(location);
        }
        return locations;
    }

    //Why does this and LoadLocations not share any code. Eliminate one of them.
    public Location LoadLocation(int addr, Terrain t, Continent c)
    {
        byte[] bytes = [
            GetByte(addr),
            GetByte(addr + RomMap.overworldXOffset),
            GetByte(addr + RomMap.overworldMapOffset),
            GetByte(addr + RomMap.overworldWorldOffset)
        ];
        return new Location(bytes[0] & 127, bytes[1] & 63, addr, bytes[2] & 63, c)
        {
            ExternalWorld = bytes[0] & 128,
            appear2loweruponexit = bytes[1] & 128,
            Secondpartofcave = bytes[1] & 64,
            MapPage = bytes[2] & 192,
            FallInHole = bytes[3] & 128,
            PassThrough = bytes[3] & 64,
            ForceEnterRight = bytes[3] & 32,
            TerrainType = t,
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
}
