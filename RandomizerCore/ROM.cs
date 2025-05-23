﻿using Assembler;
using NLog;
using RandomizerCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Z2Randomizer.Core.Overworld;

namespace Z2Randomizer.Core;

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
    public const int ChrRomOffs = RomHdrSize + PrgRomSize;
    public const int ChrRomSize = 0x20000;
    public const int RomSize = ChrRomOffs + ChrRomSize;

    public const int VanillaPrgRomSize = 0x20000;
    public const int VanillaChrRomOffs = RomHdrSize + VanillaPrgRomSize;
    public const int VanillaRomSize = VanillaChrRomOffs + ChrRomSize;

    public static readonly IReadOnlyList<int> FreeRomBanks = new List<int>(Enumerable.Range(0x10, 0xf));

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private const int textAddrStartinROM = 0xEFCE;
    private const int textAddrEndinROM = 0xF090;
    private const int textAddrOffset = 0x4010;

    //addresses for palace bricks and curtains
    private readonly int[] outBricks = { 0x10485, 0x10495, 0x104A5, 0x104B5, 0x104C5, 0x104D5, 0x14023 };
    private readonly int[] inBricks = { 0x13F15, 0x13F25, 0x13F35, 0x13F45, 0x13F55, 0x13F65, 0x14033 };
    private readonly int[] inWindows = { 0x13F19, 0x13F29, 0x13F39, 0x13F49, 0x13F59, 0x13F69, 0x14027 };
    private readonly int[] inCurtains = { 0x13F1D, 0x13F2D, 0x13F3D, 0x13F4D, 0x13F5D, 0x13F6D, 0x1402B };

    private readonly int[] brickSprites = {
        ChrRomOffs + 0x9640, 
        ChrRomOffs + 0xB640, 
        ChrRomOffs + 0xD640, 
        ChrRomOffs + 0x13640, 
        ChrRomOffs + 0x15640, 
        ChrRomOffs + 0x17640, 
        ChrRomOffs + 0x19640
    };
    private readonly int[] inBrickSprites = { 
        ChrRomOffs + 0x9680, 
        ChrRomOffs + 0xB680, 
        ChrRomOffs + 0xD680, 
        ChrRomOffs + 0x13680, 
        ChrRomOffs + 0x15680, 
        ChrRomOffs + 0x17680,
        ChrRomOffs + 0x19680
    };

    private const int textEndByte = 0xFF;
    private const int creditsLineOneAddr = 0x15377;
    private const int creditsLineTwoAddr = 0x15384;

    //sprite addresses for reading sprites from a ROM:
    private const int titleSpriteStartAddr = ChrRomOffs + 0xD00;
    private const int titleSpriteEndAddr = ChrRomOffs + 0xD20;
    private const int beamSpriteStartAddr = ChrRomOffs + 0x840;
    private const int beamSpriteEndAddr = ChrRomOffs + 0x860;
    private const int raftSpriteStartAddr = ChrRomOffs + 0x11440;
    private const int raftSpriteEndAddr = ChrRomOffs + 0x11480;
    private const int OWSpriteStartAddr = ChrRomOffs + 0x11740;
    private const int OWSpriteEndAddr = ChrRomOffs + 0x117c0;
    private const int sleeperSpriteStartAddr = ChrRomOffs + 0x1000;
    private const int sleeperSpriteEndAddr = ChrRomOffs + 0x1060;
    private const int oneUpSpriteStartAddr = ChrRomOffs + 0xa80;
    private const int oneUpSpriteEndAddr = ChrRomOffs + 0xaa0;
    private const int endSprite1StartAddr = ChrRomOffs + 0xed80;
    private const int endSprite1EndAddr = ChrRomOffs + 0xee80;
    private const int endSprite2StartAddr = ChrRomOffs + 0xf000;
    private const int endSprite2EndAddr = ChrRomOffs + 0xf0e0;
    private const int endSprite3StartAddr = ChrRomOffs + 0xd000;
    private const int endSprite3EndAddr = ChrRomOffs + 0xd040;
    private const int headSpriteStartAddr = ChrRomOffs + 0x1960;
    private const int headSpriteEndAddr = ChrRomOffs + 0x1970;
    private const int playerSpriteStartAddr = ChrRomOffs + 0x2000;
    private const int playerSpriteEndAddr = ChrRomOffs + 0x3000;

    //kasuto jars
    private const int kasutoJarTextAddr = 0xEEC9;
    private const int kasutoJarAddr = 0x1E7E8;

    private readonly int[] fastCastMagicAddr = { 0xE15, 0xE16, 0xE17 };

    private readonly int[] palPalettes = { 0, 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60 };
    private readonly int[] palGraphics = { 0, 0x04, 0x05, 0x09, 0x0A, 0x0B, 0x0C, 0x06 };

    private byte[] ROMData;

    public ROM(string filename, bool expandRom = false)
    {
        try
        {
            FileStream fs;

            fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fs, new ASCIIEncoding());
            ROMData = ConvertData(br.ReadBytes(VanillaRomSize), expandRom);

        }
        catch (Exception err)
        {
            throw new Exception("Cannot find or read file to dump.", err);
        }
    }

    public ROM(ROM clone, bool expandRom = false)
    {
        ROMData = ConvertData(clone.ROMData.ToArray(), expandRom);
    }

    public ROM(byte[] data, bool expandRom = false)
    {
        ROMData = ConvertData(data, expandRom);
    }

    private static byte[] ConvertData(byte[] data, bool expandRom)
    {
        if (!expandRom)
            return data;

        Debug.Assert(data.Length == VanillaRomSize);

        // Expand the ROM from 128 KB PRG-ROM / 128 KB CHR-ROM to 256/128
        var newdata = new byte[RomSize];

        Array.Copy(data, newdata, VanillaChrRomOffs);
        Array.Copy(data, VanillaChrRomOffs, newdata, ChrRomOffs, ChrRomSize);

        return newdata;
    }

    public byte GetByte(int index)
    {
        return ROMData[index];
    }

    public byte[] GetBytes(int start, int length)
    {
        byte[] bytes = new byte[length];
        for(int i = start; i < start + length; i++)
        {
            bytes[i - start] = GetByte(i);
        }
        return bytes;
    }

    public void Put(int index, byte data)
    {
        ROMData[index] = data;
    }

    public void Put(int index, string data)
    {
        Put(index, Convert.FromHexString(data));
    }

    public void Put(int index, params byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            ROMData[index + i] = data[i];
        }
    }

    public void Dump(string filename)
    {
        File.WriteAllBytes(filename, ROMData);
    }

    public List<Text> GetGameText()
    {
        List<Text> texts = new List<Text>();
        for (int i = textAddrStartinROM; i <= textAddrEndinROM; i += 2)
        {
            List<byte> encodedText = new List<byte>();
            int addr = GetByte(i);
            addr += (GetByte(i + 1) << 8);
            addr += textAddrOffset;
            byte c = GetByte(addr);
            while (c != textEndByte)
            {
                addr++;
                encodedText.Add(c);
                c = GetByte(addr);
            }
            encodedText.Add(0xFF);
            texts.Add(new Text(encodedText.ToArray()));

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
            for (int j = 0; j < texts[i].EncodedText.Length; j++)
            {
                Put(textptr, texts[i].EncodedText[j]);
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
        byte[] randoby = Util.ToGameText("RANDO BY  ", false);
        for (int i = 0; i < randoby.Length; i++)
        {
            Put(creditsLineOneAddr + i, randoby[i]);
        }

        byte[] digshake = Util.ToGameText("DIGSHAKE ", true);
        for (int i = 0; i < digshake.Length; i++)
        {
            Put(creditsLineTwoAddr + i, digshake[i]);
        }
    }

    public void ApplyIps(byte[] patch, bool expandRom = false)
    {
        new IpsPatcher().Patch(ROMData, patch, expandRom);
    }

    private readonly int[] fireLocs = { 
        ChrRomOffs + 0x00840, 
        ChrRomOffs + 0x02840, 
        ChrRomOffs + 0x04840, 
        ChrRomOffs + 0x06840, 
        ChrRomOffs + 0x08840, 
        ChrRomOffs + 0x0a840, 
        ChrRomOffs + 0x0c840, 
        ChrRomOffs + 0x0e840, 
        ChrRomOffs + 0x16840, 
        ChrRomOffs + 0x12840, 
        ChrRomOffs + 0x14840, 
        ChrRomOffs + 0x18840 
    };

    public void UpdateSprites(CharacterSprite charSprite, string tunicColor, string shieldColor, string beamSprite)
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

        if (charSprite.Equals(CharacterSprite.LINK))
        {
            //Do nothing now.
        }
        else
        {
            new IpsPatcher().Patch(ROMData, charSprite.Path, true);
        }

        Dictionary<string, int> colorMap = new Dictionary<string, int> { { "Green", 0x2A }, { "Dark Green", 0x0A }, { "Aqua", 0x3C }, { "Dark Blue", 0x02 }, { "Purple", 0x04 }, { "Pink", 0x24 }, { "Red", 0x16 }, { "Orange", 0x27 }, { "Turd", 0x18 } };

        /*colors to include
            Green (2A)
            Dark Green (0A)
            Aqua (3C)
            Dark Blue (02)
            Purple (04)
            Pink (24)
            Red (16)
            Orange (27)
            Turd (08)
        */
        int? c2 = null;
        int? c1 = null;
        if (tunicColor == "Random")
        {
            Random r2 = new Random();

            int c2p1 = r2.Next(3);
            int c2p2 = r2.Next(1, 13);
            c2 = c2p1 * 16 + c2p2;

            while (c1 == c2)
            {
                c2p1 = r2.Next(3);
                c2p2 = r2.Next(1, 13);
                c2 = c2p1 * 16 + c2p2;
            }
        } else if (tunicColor != "Default")
        {
            c2 = colorMap[tunicColor];
        }

        if (shieldColor == "Random")
        {
            Random r2 = new Random();

            int c1p1 = r2.Next(3);
            int c1p2 = r2.Next(1, 13);

            c1 = c1p1 * 16 + c1p2;

            while (c1 == c2)
            {
                c1p1 = r2.Next(3);
                c1p2 = r2.Next(1, 13);
                c1 = c1p1 * 16 + c1p2;
            }
        } else if (shieldColor != "Default")
        {
            c1 = colorMap[shieldColor];
        }

        int[] tunicLocs = { 0x285C, 0x40b1, 0x40c1, 0x40d1, 0x80e1, 0x80b1, 0x80c1, 0x80d1, 0x80e1, 0xc0b1, 0xc0c1, 0xc0d1, 0xc0e1, 0x100b1, 0x100c1, 0x100d1, 0x100e1, 0x140b1, 0x140c1, 0x140d1, 0x140e1, 0x17c1b, 0x1c466, 0x1c47e };

        foreach (int l in tunicLocs)
        {
            if (c2 != null)
            {
                Put(0x10ea, (byte)c2);
                if ((charSprite == CharacterSprite.LINK || !charSprite.IsLegacy))
                {
                    if (tunicColor != "Default")
                    {
                        Put(0x10ea, (byte)c2);
                        Put(l, (byte)c2);
                    }
                    //Don't overwrite for null 
                }
                else
                {
                    Put(0x10ea, (byte)c2);
                    Put(l, (byte)c2);
                }
            }
        }

        if ((charSprite == CharacterSprite.LINK || !charSprite.IsLegacy) && shieldColor == "Default")
        {
            //Don't overwrite default shield. For custom sprite IPS base
        }
        else
        {
            if(c1 != null)
            {
                Put(0xe9e, (byte)c1);
            }
        }

        int beamType = -1;
        if (beamSprite.Equals("Random"))
        {

            Random r2 = new Random();
            beamType = r2.Next(6);
        }
        else if (beamSprite.Equals("Fire"))
        {
            beamType = 0;
        }
        else if (beamSprite.Equals("Bubble"))
        {
            beamType = 1;
        }
        else if (beamSprite.Equals("Rock"))
        {
            beamType = 2;
        }
        else if (beamSprite.Equals("Axe"))
        {
            beamType = 3;
        }
        else if (beamSprite.Equals("Hammer"))
        {
            beamType = 4;
        }
        else if (beamSprite.Equals("Wizzrobe Beam"))
        {
            beamType = 5;
        }
        byte[] newSprite = new byte[32];

        if (beamType == 0 || beamType == 3 || beamType == 4)
        {
            Put(0x18f5, 0xa9);
            Put(0x18f6, 0x00);
            Put(0x18f7, 0xea);
        }
        else if (beamType != -1)
        {
            Put(0X18FB, 0x84);
        }

        if (beamType == 1)//bubbles
        {
            for (int i = 0; i < 32; i++)
            {
                byte next = GetByte(ChrRomOffs + 0xaa0 + i);
                newSprite[i] = next;
            }
        }

        if (beamType == 2)//rocks
        {
            for (int i = 0; i < 32; i++)
            {
                byte next = GetByte(ChrRomOffs + 0x2ae0 + i);
                newSprite[i] = next;
            }
        }

        if (beamType == 3)//axes
        {
            for (int i = 0; i < 32; i++)
            {
                byte next = GetByte(ChrRomOffs + 0x2fa0 + i);
                newSprite[i] = next;
            }
        }

        if (beamType == 4)//hammers
        {
            for (int i = 0; i < 32; i++)
            {
                byte next = GetByte(ChrRomOffs + 0x12ee0 + i);
                newSprite[i] = next;
            }
        }

        if (beamType == 5)//wizzrobe beam
        {
            for (int i = 0; i < 32; i++)
            {
                byte next = GetByte(ChrRomOffs + 0x14dc0 + i);
                newSprite[i] = next;
            }
        }


        if (beamType != 0 && beamType != -1)
        {
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
        Put(0x13ec1, new byte[2] { 0xaa, 0xea, }); // tax nop
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

    public void ExtendMapSize(Engine engine)
    {
        //Implements CF's map size hack:
        //https://github.com/cfrantz/z2doc/wiki/bigger-overworlds
        Assembler.Assembler a = new();
        a.Code("""
.macpack common

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
        engine.Modules.Add(a.Actions);
    }

    public void DisableTurningPalacesToStone()
    {
        Put(0x87b3, [0xea, 0xea, 0xea]);
        Put(0x47ba, [0xea, 0xea, 0xea]);
        Put(0x1e02e, [0xea, 0xea, 0xea]);
    }

    public void UpdateMapPointers()
    {
        Put(0x4518, [0x70, 0xb4]); //west
        Put(0x451a, [0xf0, 0xb9]); //dm
        Put(0x8518, [0x70, 0xb4]); //east
        Put(0x851a, [0xf0, 0xb9]); //maze island
    }

    public void UpAController1(Engine engine)
    {
        Assembler.Assembler assembler = new();
        assembler.Code("""
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
        engine.Modules.Add(assembler.Actions);
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

    public void ChangeMapperToMMC5(Engine engine, bool enableZ2ft)
    {
        Assembler.Assembler assembler = new();
        assembler.Assign("ENABLE_Z2FT", enableZ2ft ? 1 : 0);
        assembler.Code(Assembly.GetExecutingAssembly().ReadResource("RandomizerCore.Asm.MMC5.s"), "mmc5_conversion.s");

        engine.Modules.Add(assembler.Actions);
    }

    public void ApplyAsm(Engine engine)
    {
        engine.Apply(ROMData);
    }
    public void RandomizeKnockback(Engine engine, Random RNG)
    {
        Assembler.Assembler a = new();

        // 0x23 is the max enemy id and we have 3 tables for each randomized value
        byte enemy_count = 0x24;
        byte[] x_arr = new byte[enemy_count];
        byte[] y_lo_arr = new byte[enemy_count];
        byte[] y_hi_arr = new byte[enemy_count];
        ushort recoilTableXAddr = (ushort) (0xbfff - enemy_count * 3);
        a.Set("RecoilTableX", recoilTableXAddr);
        a.Set("RecoilTableYLo", recoilTableXAddr + enemy_count);
        a.Set("RecoilTableYHi", recoilTableXAddr + enemy_count * 2);
        for (int i = 0; i < 7; i++)
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
        a.Code(Assembly.GetExecutingAssembly().ReadResource("RandomizerCore.Asm.Recoil.s"), "recoil.s");
        engine.Modules.Add(a.Actions);
    }


    public void DontCountExpDuringTalking(Engine engine)
    {
        Assembler.Assembler a = new();

        a.Code("""
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
        engine.Modules.Add(a.Actions);
    }

    public void InstantText(Engine engine)
    {
        Assembler.Assembler a = new();

        a.Code("""
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
        engine.Modules.Add(a.Actions);
    }

    public void PreventLR(Engine engine)
    {
        Assembler.Assembler a = new();
        a.Code("""
.macpack common

PollPadInput = $d367

.define buttons $f5
.define last_frame_buttons $f7

.segment "PRG5"
; Patch the title screen call to the read routine
.org $A68A
  jsr ControllerReading


.segment "PRG7"
; Clear the space for the vanilla controller reading code
.org $d346
FREE_UNTIL PollPadInput

; Patch the main places that users the controller read routine
.org $c137
  jsr ControllerReading
.org $cd9e
  jsr ControllerReading

; Make a new controller read routine that prevents L+R inputs
.reloc
ControllerReading:
    jsr PollPadInput
    lda $f5
    sta $00
    jsr PollPadInput
    lda $f5
    cmp $00
    bne ControllerReading
    ; for both player 1 and player 2 controller inputs
    ldx #$01
controller_process_loop:
        ; Prevent L/R inputs
        lda buttons, x
        and #%00001010    ; Compare Up and Left...
        lsr a
        and buttons, x    ; to Down and Right
        beq not_opposing
            ; Use previous frame's directions
            lda buttons, x
            eor last_frame_buttons, x
            and #%11110000
            eor last_frame_buttons, x
            sta buttons, x
not_opposing:
        ; and Calculate which buttons were pressed this frame
        lda buttons,x
        tay
        eor last_frame_buttons,x
        and buttons,x
        sta buttons,x
        sty last_frame_buttons,x
        dex
        bpl controller_process_loop
    rts
""", "prevent_lr.s");
        engine.Modules.Add(a.Actions);
    }

    public void BuffCarrock(Engine engine)
    {
        Assembler.Assembler assembler = new();
        assembler.Code(Assembly.GetExecutingAssembly().ReadResource("RandomizerCore.Asm.BuffCarock.s"), "buff_carock.s");
        engine.Modules.Add(assembler.Actions);
    }

    public void DashSpell(Engine engine)
    {
        Assembler.Assembler assembler = new();
        assembler.Code(Assembly.GetExecutingAssembly().ReadResource("RandomizerCore.Asm.DashSpell.s"), "dash_spell.s");

        byte[] dash = Util.ToGameText("DASH", false).Select(x => (byte)x).ToArray();
        assembler.Org(0x9c62);
        assembler.Byt(dash);
        engine.Modules.Add(assembler.Actions);
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
