using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Z2Randomizer
{

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
    class ROM
    {
        private const int textAddrStartinROM = 0xEFCE;
        private const int textAddrEndinROM = 0xF090;
        private const int textAddrOffset = 0x4010;

        //addresses for palace bricks and curtains
        private readonly int[] outBricks = { 0x10485, 0x10495, 0x104A5, 0x104B5, 0x104C5, 0x104D5, 0x14023 };
        private readonly int[] inBricks = { 0x13F15, 0x13F25, 0x13F35, 0x13F45, 0x13F55, 0x13F65, 0x14033 };
        private readonly int[] inWindows = { 0x13F19, 0x13F29, 0x13F39, 0x13F49, 0x13F59, 0x13F69, 0x14027 };
        private readonly int[] inCurtains = { 0x13F1D, 0x13F2D, 0x13F3D, 0x13F4D, 0x13F5D, 0x13F6D, 0x1402B };

        private readonly int[] brickSprites = { 0x29650, 0x2B650, 0x2D650, 0x33650, 0x35650, 0x37650, 0x39650 };
        private readonly int[] inBrickSprites = { 0x29690, 0x2B690, 0x2D690, 0x33690, 0x35690, 0x37690, 0x39690 };

        private const int textEndByte = 0xFF;
        private const int creditsLineOneAddr = 0x15377;
        private const int creditsLineTwoAddr = 0x15384;

        //sprite addresses for reading sprites from a ROM:
        private const int titleSpriteStartAddr = 0x20D10;
        private const int titleSpriteEndAddr = 0x20D30;
        private const int beamSpriteStartAddr = 0x20850;
        private const int beamSpriteEndAddr = 0x20870;
        private const int raftSpriteStartAddr = 0x31450;
        private const int raftSpriteEndAddr = 0x31490;
        private const int OWSpriteStartAddr = 0x31750;
        private const int OWSpriteEndAddr = 0x317d0;
        private const int sleeperSpriteStartAddr = 0x21010;
        private const int sleeperSpriteEndAddr = 0x21070;
        private const int oneUpSpriteStartAddr = 0x20a90;
        private const int oneUpSpriteEndAddr = 0x20ab0;
        private const int endSprite1StartAddr = 0x2ed90;
        private const int endSprite1EndAddr = 0x2ee90;
        private const int endSprite2StartAddr = 0x2f010;
        private const int endSprite2EndAddr = 0x2f0f0;
        private const int endSprite3StartAddr = 0x2d010;
        private const int endSprite3EndAddr = 0x2d050;
        private const int headSpriteStartAddr = 0x21970;
        private const int headSpriteEndAddr = 0x21980;
        private const int playerSpriteStartAddr = 0x22010;
        private const int playerSpriteEndAddr = 0x23010;

        //kasuto jars
        private const int kasutoJarTextAddr = 0xEEC9;
        private const int kasutoJarAddr = 0x1E7E8;

        private readonly int[] fastCastMagicAddr = { 0xE15, 0xE16, 0xE17 };

        private readonly int[] palPalettes = { 0, 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60 };
        private readonly int[] palGraphics = { 0, 0x04, 0x05, 0x09, 0x0A, 0x0B, 0x0C, 0x06 };

        private readonly int[] spellTextPointers = { 0xEFEC, 0xEFFE, 0xF014, 0xF02A, 0xF05A, 0xf070, 0xf088, 0xf08e };

        private byte[] ROMData;

        public ROM(String filename)
        {
            try
            {
                FileStream fs;

                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

                BinaryReader br = new BinaryReader(fs, new ASCIIEncoding());
                ROMData = br.ReadBytes(257 * 1024);

            }
            catch
            {
                MessageBox.Show("Cannot find or read file to dump.");
            }
        }

        public Byte getByte(int index)
        {
            return ROMData[index];
        }

        public Byte[] getBytes(int start, int end)
        {
            Byte[] bytes = new byte[end - start];
            for(int i = start; i < end; i++)
            {
                bytes[i - start] = getByte(i);
            }
            return bytes;
        }

        public void put(int index, Byte data)
        {
            ROMData[index] = data;
        }

        public void put(int index, Byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                ROMData[index + i] = data[i];
            }
        }

        public void dump(String filename)
        {
            File.WriteAllBytes(filename, ROMData);
        }

        public List<Hint> getGameText()
        {
            List<Hint> texts = new List<Hint>();
            for (int i = textAddrStartinROM; i <= textAddrEndinROM; i += 2)
            {
                List<char> t = new List<char>();
                int addr = getByte(i);
                addr += (getByte(i + 1) << 8);
                addr += textAddrOffset;
                int c = getByte(addr);
                while (c != textEndByte)
                {
                    addr++;
                    t.Add((char)c);
                    c = getByte(addr);
                }
                t.Add((char)0xFF);
                texts.Add(new Hint(t, null));

            }
            return texts;
        }

        public void textToRom(List<Hint> texts)
        {
            int textptr = 0xE390;
            int ptr = 0xE390 - 0x4010;
            int ptrptr = 0xEFCE;

            for (int i = 0; i < texts.Count; i++)
            {
                int high = (ptr & 0xff00) >> 8;
                int low = (ptr & 0xff);
                put(ptrptr, (byte)low);
                put(ptrptr + 1, (byte)high);
                ptrptr = ptrptr + 2;
                for (int j = 0; j < texts[i].Text.Count; j++)
                {
                    put(textptr, (Byte)texts[i].Text[j]);
                    textptr++;
                    ptr++;
                }
            }
        }

        public void writePalacePalettes(List<int[]> bricks, List<int[]> curtains, List<int> bRows, List<int> binRows)
        {
            int[,] bSprites = new int[7, 32];
            int[,] binSprites = new int[7, 32];
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    bSprites[i, j] = (int)getByte(brickSprites[i] + j);
                    binSprites[i, j] = (int)getByte(inBrickSprites[i] + j);
                }
            }
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    put(outBricks[i] + j, (Byte)bricks[i][j]);
                    put(inBricks[i] + j, (Byte)bricks[i][j]);
                    put(inCurtains[i] + j, (Byte)curtains[i][j]);
                    if (j == 0)
                    {
                        put(inWindows[i] + j, (Byte)bricks[i][j]);
                    }
                }

                for (int j = 0; j < 32; j++)
                {
                    put(brickSprites[i] + j, (Byte)bSprites[bRows[i], j]);
                    put(inBrickSprites[i] + j, (Byte)binSprites[binRows[i], j]);
                }
            }
        }

        public void addCredits()
        {
            List<char> randoby = Util.toGameText("RANDO BY  ", false);
            for (int i = 0; i < randoby.Count; i++)
            {
                put(creditsLineOneAddr + i, (Byte)randoby[i]);
            }

            List<char> digshake = Util.toGameText("DIGSHAKE ", true);
            for (int i = 0; i < digshake.Count; i++)
            {
                put(creditsLineTwoAddr + i, (Byte)digshake[i]);
            }
        }

        public void updateSprites(String charSprite)
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
            if(charSprite.Equals("Random"))
            {
                Random r = new Random();
                int sp = r.Next(18);

                if(sp == 0)
                {
                    charSprite = "Link";
                }
                else if(sp == 1)
                {
                    charSprite = "Zelda";
                }
                else if (sp == 2)
                {
                    charSprite = "Iron Knuckle";
                }
                else if (sp == 3)
                {
                    charSprite = "Error";
                }
                else if (sp == 4)
                {
                    charSprite = "Samus";
                }
                else if (sp == 5)
                {
                    charSprite = "Simon";
                }
                else if (sp == 6)
                {
                    charSprite = "Stalfos";
                }
                else if (sp == 7)
                {
                    charSprite = "Vase Lady";
                }
                else if (sp == 8)
                {
                    charSprite = "Ruto";
                }
                else if (sp == 9)
                {
                    charSprite = "Yoshi";
                }
                else if (sp == 10)
                {
                    charSprite = "Dragonlord";
                }
                else if (sp == 11)
                {
                    charSprite = "Miria";
                }
                else if (sp == 12)
                {
                    charSprite = "Crystalis";
                }
                else if (sp == 13)
                {
                    charSprite = "Taco";
                }
                else if (sp == 14)
                {
                    charSprite = "Pyramid";
                }
                else if (sp == 15)
                {
                    charSprite = "Lady Link";
                }
                else if (sp == 16)
                {
                    charSprite = "Hoodie Link";
                }
                else if (sp == 17)
                {
                    charSprite = "GliitchWiitch";
                }
            }
            if (!charSprite.Equals("Link"))
            {
                List<int[]> spriteInfo = Graphics.sprites[charSprite];

                int[] sprite = spriteInfo[0];
                int[] s1up = spriteInfo[1];
                int[] sOW = spriteInfo[2];
                int[] sleeper = spriteInfo[3];
                int[] sTitle = spriteInfo[4];
                int[] end1 = spriteInfo[5];
                int[] end2 = spriteInfo[6];
                int[] end3 = spriteInfo[7];
                int[] head = spriteInfo[8];
                int[] raft = spriteInfo[9];

                for (int i = 0; i < sprite.Count() * 3 / 8; i++)
                {
                    put(0x20010 + i, (byte)sprite[i]);
                    put(0x22010 + i, (byte)sprite[i]);
                    put(0x24010 + i, (byte)sprite[i]);
                    put(0x26010 + i, (byte)sprite[i]);
                    put(0x28010 + i, (byte)sprite[i]);
                    put(0x2a010 + i, (byte)sprite[i]);
                    put(0x2c010 + i, (byte)sprite[i]);
                    put(0x2e010 + i, (byte)sprite[i]);
                    put(0x30010 + i, (byte)sprite[i]);
                    put(0x32010 + i, (byte)sprite[i]);
                    put(0x34010 + i, (byte)sprite[i]);
                    if (i < 0x4E0 || i >= 0x520)
                    {
                        put(0x36010 + i, (byte)sprite[i]);
                    }
                    put(0x38010 + i, (byte)sprite[i]);

                }

                for (int i = 0; i < 0x20; i++)
                {
                    put(0x206d0 + i, (byte)sprite[0x6c0 + i]);
                    put(0x2e6d0 + i, (byte)sprite[0x6c0 + i]);
                    put(0x306d0 + i, (byte)sprite[0x6c0 + i]);
                }

                for (int i = 0; i < s1up.Count(); i++)
                {
                    put(0x20a90 + i, (byte)s1up[i]);
                    put(0x22a90 + i, (byte)s1up[i]);
                    put(0x24a90 + i, (byte)s1up[i]);
                    put(0x26a90 + i, (byte)s1up[i]);
                    put(0x28a90 + i, (byte)s1up[i]);
                    put(0x2aa90 + i, (byte)s1up[i]);
                    put(0x2ca90 + i, (byte)s1up[i]);
                    put(0x2ea90 + i, (byte)s1up[i]);
                    put(0x30a90 + i, (byte)s1up[i]);
                    put(0x32a90 + i, (byte)s1up[i]);
                    put(0x34a90 + i, (byte)s1up[i]);
                    put(0x36a90 + i, (byte)s1up[i]);
                    put(0x38a90 + i, (byte)s1up[i]);


                }

                for (int i = 0; i < sOW.Count(); i++)
                {
                    put(0x31750 + i, (byte)sOW[i]);
                }

                for (int i = 0; i < sTitle.Count(); i++)
                {
                    put(0x20D10 + i, (byte)sTitle[i]);
                    put(0x2ED10 + i, (byte)sTitle[i]);

                }

                for (int i = 0; i < sleeper.Count(); i++)
                {
                    put(0x21010 + i, (byte)sleeper[i]);
                    if (i > 31)
                    {
                        put(0x23270 + i, (byte)sleeper[i]);
                    }
                }

                for (int i = 0; i < end1.Count(); i++)
                {
                    put(0x2ed90 + i, (byte)end1[i]);
                }

                for (int i = 0; i < end2.Count(); i++)
                {
                    put(0x2f010 + i, (byte)end2[i]);
                }

                for (int i = 0; i < end3.Count(); i++)
                {
                    put(0x2d010 + i, (byte)end3[i]);
                }

                for (int i = 0; i < head.Count(); i++)
                {
                    put(0x21970 + i, (byte)head[i]);
                    put(0x23970 + i, (byte)head[i]);
                    put(0x25970 + i, (byte)head[i]);
                    put(0x27970 + i, (byte)head[i]);
                    put(0x29970 + i, (byte)head[i]);
                    put(0x2B970 + i, (byte)head[i]);
                    put(0x2D970 + i, (byte)head[i]);
                    put(0x2F970 + i, (byte)head[i]);
                    put(0x31970 + i, (byte)head[i]);
                    put(0x33970 + i, (byte)head[i]);
                    put(0x35970 + i, (byte)head[i]);
                    put(0x37970 + i, (byte)head[i]);
                    put(0x39970 + i, (byte)head[i]);
                }

                for (int i = 0; i < raft.Count(); i++)
                {
                    put(0x31450 + i, (byte)raft[i]);
                }
            }

            if(charSprite.Equals("Samus"))
            {
                for(int i = 0; i < Graphics.samusEnd.Count; i++)
                {
                    put(0x20010, (byte)Graphics.samusEnd[i]);
                }
            }

        }

        public void updateSpellText(Dictionary<spells, spells> spellMap)
        {
            int[,] textPointers = new int[8, 2];
            for (int i = 0; i < spellTextPointers.Length; i++)
            {
                textPointers[i, 0] = getByte(spellTextPointers[i]);
                textPointers[i, 1] = getByte(spellTextPointers[i] + 1);
            }

            for (int i = 0; i < spellTextPointers.Length; i++)
            {
                put(spellTextPointers[i], (byte)textPointers[(int)spellMap[(spells)i], 0]);
                put(spellTextPointers[i] + 1, (byte)textPointers[(int)spellMap[(spells)i], 1]);
            }
        }

        public void doHackyFixes()
        {
            //Hacky fix for palace connections
            put(0x1074A, 0xFC);
            put(0x1477D, 0xFC);

            //Hacky fix for new kasuto

            put(0x8660, 0x51);
            put(0x924D, 0x00);
            
            //Hack fix for palace 6
            put(0x8664, 0xE6);
            //put(0x935E, 0x02);
           
            //Fix for extra battle scene
            put(0x8645, 0x00);

            //Disable hold over head animation
            put(0x1E54C, (Byte)0);

            //Make text go fast
            put(0xF75E, 0x00);
            put(0xF625, 0x00);
            put(0xF667, 0x00);
        }

        public void writeKasutoJarAmount(int kasutoJars)
        {
            put(kasutoJarTextAddr, (Byte)(0xD0 + kasutoJars));
            put(kasutoJarAddr, (Byte)kasutoJars);
        }

        public void writeFastCastMagic()
        {
            foreach (int addr in fastCastMagicAddr)
            {
                put(addr, 0xEA);
            }
        }

        public void disableMusic()
        {
            /*
             * This method needs some more refactoring to eliminate those magic numbers
             * But I just don't feel like it right now.
             */
            for (int i = 0; i < 4; i++)
            {
                put(0x1a010 + i, 08);
                put(0x1a3da + i, 08);
                put(0x1a63f + i, 08);
            }

            put(0x1a946, 08);
            put(0x1a947, 08);
            put(0x1a94c, 08);

            put(0x1a02f, 0);
            put(0x1a030, 0x44);
            put(0x1a031, 0xA3);
            put(0x1a032, 0);
            put(0x1a033, 0);
            put(0x1a034, 0);

            put(0x1a3f4, 0);
            put(0x1a3f5, 0x44);
            put(0x1a3f6, 0xA3);
            put(0x1a3f7, 0);
            put(0x1a3f8, 0);
            put(0x1a3f9, 0);

            put(0x1a66e, 0);
            put(0x1a66f, 0x44);
            put(0x1a670, 0xA3);
            put(0x1a671, 0);
            put(0x1a672, 0);
            put(0x1a673, 0);

            put(0x1a970, 0);
            put(0x1a971, 0x44);
            put(0x1a972, 0xA3);
            put(0x1a973, 0);
            put(0x1a974, 0);
            put(0x1a975, 0);
        }

        public void fixSoftLock()
        {
            put(0x1E19A, (Byte)0x20);
            put(0x1E19B, (Byte)0xAA);
            put(0x1E19C, (Byte)0xFE);

            put(0x1FEBA, (Byte)0xEE);
            put(0x1FEBB, (Byte)0x26);
            put(0x1FEBC, (Byte)0x07);
            put(0x1FEBD, (Byte)0xAD);
            put(0x1FEBE, (Byte)0x4C);
            put(0x1FEBF, (Byte)0x07);
            put(0x1FEC0, (Byte)0xC9);
            put(0x1FEC1, (Byte)0x02);
            put(0x1FEC2, (Byte)0xF0);
            put(0x1FEC3, (Byte)0x05);
            put(0x1FEC4, (Byte)0xA2);
            put(0x1FEC5, (Byte)0x00);
            put(0x1FEC6, (Byte)0x8E);
            put(0x1FEC7, (Byte)0x4C);
            put(0x1FEC8, (Byte)0x07);
            put(0x1FEC9, (Byte)0x60);
        }

        public void setLevelCap(int atkMax, int magicMax, int lifeMax)
        {

            //jump to check which attribute is levelling up
            put(0x1f8a, 0x4C);
            put(0x1f8b, 0x9e);
            put(0x1f8c, 0xa8);

            ////x = 2 life, x = 1 magic, x = 0 attack
            //load current level for (attack, magic, life)
            //compare to address of cap
            //go back to $9f7f

            //BD 77 07 (load level)
            //DD A7 A8
            //4C 7F 9F

            put(0x28AE, 0xBD);
            put(0x28AF, 0x77);
            put(0x28B0, 0x07);
            put(0x28B1, 0xDD);
            put(0x28B2, 0xA7);
            put(0x28B3, 0xA8);
            put(0x28B4, 0x4C);
            put(0x28B5, 0x7F);
            put(0x28B6, 0x9F);

            //these are the actual caps
            put(0x28B7, (byte)atkMax);
            put(0x28B8, (byte)magicMax);
            put(0x28B9, (byte)lifeMax);


        }

        public void extendMapSize()
        {
            //Implements CF's map size hack:
            //https://github.com/cfrantz/z2doc/wiki/bigger-overworlds

            /*
             * cd9b: a000          LDY #$00              # Initialize start index
            cd9d: b102          LDA ($02,Y)           # load from source
            cd9f: 9120          STA ($20,Y)           # store to dest
            cda1: c8            INY
            cda2: 10f9          BPL $f9 (target=cd9d) # do it 128 times
            cda4: ca            DEX                   # decrement counter
            cda5: f00e          BEQ $0e (target=cdb5) # done yet?
            cda7: b102          LDA ($02,Y)           # load from source
            cda9: 9120          STA ($20,Y)           # store to dest
            cdab: c8            INY
            cdac: d0f9          BNE $f9 (target=cda7) # 128 more times
            cdae: e603          INC $03               # increment source pointer
            cdb0: e621          INC $21               # increment dest pointer
            cdb2: ca            DEX                   # decrement counter
            cdb3: d0e8          BNE $e8 (target=cd9d) # not done? do it again.
            cdb5: 60            RTS                   # return to caller
            */

            put(0x1cda8, new Byte[] { 0x4c, 0xc6, 0xcd, 0xa0, 0x00, 0xb1, 0x02, 0x91, 0x20, 0xc8, 0x10, 0xf9, 0xca, 0xf0, 0x0e, 0xb1, 0x02, 0x91, 0x20, 0xc8, 0xd0, 0xf9, 0xe6, 0x03, 0xe6, 0x21, 0xca, 0xd0, 0xe8, 0x60 });

            //# Fill with NOPs all the way to $cdc6
            for (int i = 0x1cdc6; i < 0x1cdd6; i++)
            {
                put(i, 0xea);
            }

            /*
             * cdc6: ae0607        LDX $0706       # load overworld number into X
                cdc9: bd27cd        LDA $cd27,X     # overworld to map pointer offset
                cdcc: aa            TAX             # into index X
                cdcd: bd0885        LDA $8508,X     # put ROM pointer into $02-$03
                cdd0: 8502          STA $02
                cdd2: bd0985        LDA $8509,X
                cdd5: 8503          STA $03
            */
            put(0x1cdd6, new byte[] { 0xae, 0x06, 0x07, 0xbd, 0xf1, 0xff, 0xaa, 0xbd, 0x08, 0x85, 0x85, 0x02, 0xbd, 0x09, 0x85, 0x85, 0x03 });

            /*
             * cdd7: a900          LDA #$00        # put destination $7c00 into $20-$21
                cdd9: 8520          STA $20
                cddb: a97c          LDA #$7a
                cddd: 8521          STA $21
                cddf: a207          LDX #$07        # 7 half-pages == 896 bytes
                cde1: 209bcd        JSR $cd9b       # copy
            */

            put(0x1cde7, new byte[] { 0xa9, 0x00, 0x85, 0x20, 0xa9, 0x7a, 0x85, 0x21, 0xa2, 0x0b, 0x20, 0x9b, 0xcd });

            /*
             * cde4: a9a0          LDA #$a0        # load source $88a0 into $02-$03
                cde6: 8502          STA $02
                cde8: a988          LDA #$88
                cdea: 8503          STA $03
                cdec: a970          LDA #$70        # load dest $7000 into $20-$21 (address
                cdee: 8521          STA $21         #   $20 should still be 0)
                cdf0: a208          LDX #$08        # 8 half-pages == 1024 bytes
                cdf2: 209bcd        JSR $cd9b       # copy
            */

            put(0x1cdf4, new byte[] { 0xa9, 0xa0, 0x85, 0x02, 0xa9, 0x88, 0x85, 0x03, 0xa9, 0x70, 0x85, 0x21, 0xa2, 0x08, 0x20, 0x9b, 0xcd });

            put(0x808, 0x7a);
        }

        public void disableTurningPalacesToStone()
        {
            put(0x87b3, new byte[] { 0xea, 0xea, 0xea });
            put(0x47ba, new byte[] { 0xea, 0xea, 0xea });
            put(0x1e02e, new byte[] { 0xea, 0xea, 0xea });
        }

        public void updateMapPointers()
        {
            put(0x4518, new byte[] { 0x70, 0xb4 }); //west
            put(0x451a, new byte[] { 0xf0, 0xb9 }); //dm
            put(0x8518, new byte[] { 0x70, 0xb4 }); //east
            put(0x851a, new byte[] { 0xf0, 0xb9 }); //maze island
        }

        public void fixContinentTransitions()
        {
            //https://github.com/cfrantz/z2doc/wiki/add-an-extra-overworld
            put(0x1FFF0, new byte[] { 0x01, 0x01, 0x02, 0x02, 0x00, 0x10, 0x20, 0x20, 0x30, 0x30, 0x30, 0x30, 0x40, 0x50, 0x60, 0x60, 0x30 });

            /*
             * cd4a: ad0607        LDA $0706     # Get overworld number
                cd4d: 0a            ASL A         # times 4
                cd4e: 0a            ASL A
                cd4f: 0d0a07        ORA $070a     # plus previous overworld number
                cd52: a8            TAY           # into index Y
                cd53: b9e0ff        LDA $ffe0,Y   # load bank from lookup table
                cd56: 8d6907        STA $0769     # store in bank-to-switch-to
                cd59: 8d6907        STA $0769
                cd5c: 20ccff        JSR $ffcc     # load bank
                cd5f: ade0bf        LDA $bfe0     # load pseudo-bank
            */
            put(0x1cd5a, new byte[] { 0xad, 0x06, 0x07, 0xea, 0xea, 0xea, 0xea, 0xea, 0xa8, 0xb9, 0xe0, 0xff, 0x8d, 0x69, 0x07, 0x8d, 0x69, 0x07, 0x020, 0xcc, 0xff });

            put(0x1cd94, new byte[] { 0xa8, 0xb9, 0xf1, 0xff, 0x0a, 0xa8 });
            put(0x1c516, new byte[] { 0xa8, 0xb9, 0xf1, 0xff, 0x0a, 0xa8 });
            put(0x20001, new byte[] { 0x00, 0x02, 0x00, 0x02 });
            put(0x1ce43, new byte[] { 0xe4, 0xff });
            //put(0x1cdd2, new byte[] { 0xf0, 0xff });

            //update item memory locations:
            put(0x1f310, getBytes(0x1c275, 0x1c295));
            put(0x1f330, new byte[] { 0x60, 0x06, 0x60, 0x06, 0x80, 0x06, 0xa0, 0x06, 0xc0, 0x06 });
            put(0x1c2c9, new byte[] { 0x00, 0xF3 });
            put(0x1c2ce, new byte[] { 0x01, 0xF3 });

            //fix raft check
            put(0x5b2, new byte[] { 0xea, 0xea });
        }

        public void upAController1()
        {
            put(0x21B0, 0xF7);
            put(0x21B2, 0x28);
            put(0x21EE, 0xF7);
            put(0x21F0, 0x28);
        }

        public void disableFlashing()
        {
            put(0x2A01, 0x12);
            put(0x2A02, 0x12);
            put(0x2A03, 0x12);
            put(0x1C9FA, 0x16);
            put(0x1C9FC, 0x16);
        }

        public void dashSpell()
        {
            /*
             * push accumulator to stack (48)
             * load 769 (AD 6F 07)
             * check 4th bit (29 10)
             * branch if equal to fire table (F0 X)
             * pop stack to accumulator (68)
             * compare to normal table D9 B3 93
             * return (60)
             * pop stack to accumulator (68)
             * compare to dash table (D9 X X)
             * return (60)
             * two bytes for dash table
             */
            put(0x2a50, new Byte[] { 0x48, 0xad, 0x6f, 0x07, 0x29, 0x10, 0xd0, 0x05, 0x68, 0xd9, 0xb3, 0x93, 0x60, 0x68, 0xd9, 0x52, 0xaa, 0x60, 0x30, 0xd0}); //, 0x20, 0xFD, 0x93, 0xa9, 0x18, 0x8d, 0xb3, 0x93, 0xa9, 0xE8, 0x8d, 0xb4, 0x93, 0x60 });

            //Jump to 97f1 
            put(0x140f, new Byte[] { 0x20, 0x40, 0xAA });

            //put values back
            put(0xe60, new Byte[] { 0x14, 0x98 });
            List<char> dash = Util.toGameText("DASH", false);

            for(int i = 0; i < dash.Count; i++)
            {
                put(0x1c72 + i, (byte)dash[i]);
            }
        }

        public void moveAfterGem()
        {
            put(0x11b15, new Byte[] { 0xea, 0xea });

            put(0x11af5, new Byte[] { 0x47, 0x9b, 0x56, 0x9b, 0x35, 0x9b });
        }

        public void elevatorBossFix(bool bossItem)
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

            put(0x13ea9, new byte[] { 0x20, 0x40, 0xF3 });
            put(0x16373, new byte[] { 0x20, 0x40, 0xF3 });
            put(0x13230, new byte[] { 0x20, 0x40, 0xF3 });

            if (!bossItem)
            {
                put(0x1e7b9, new byte[] { 0x20, 0x40, 0xF3 });
            }
            else
            {
                put(0x1e7b1, new byte[] { 0x20, 0x40, 0xF3 });
            }

            put(0x1F350, new byte[] { 0xa9, 0x01, 0x4d, 0x28, 0x07, 0x8d, 0x28, 0x07, 0xa9, 0x13, 0xc5, 0xa1, 0xd0, 0x0a, 0xa9, 0x01, 0x45, 0xb6, 0x85, 0xb6, 0xa9, 0xa0, 0x85, 0x2a, 0x60 });
        }

        public void dumpAll(String name)
        {
            dumpSprite(name);
            dump1up(name);
            dumpOW(name);
            dumpSleeper(name);
            dumpTitle(name);
            ending1(name);
            ending2(name);
            ending3(name);
            dumpHead(name);
            dumpRaft(name);
            dumpBeam(name);
            dumpColors(name);
        }

        public void dumpTitle(String name)
        {
            Console.Write("private static readonly int[] " + name + "Title = {");
            for (int i = titleSpriteStartAddr; i < titleSpriteEndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void dumpBeam(String name)
        {
            Console.Write("private static readonly int[] " + name + "Beam = {");
            for (int i = beamSpriteStartAddr; i < beamSpriteEndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void dumpRaft(String name)
        {
            Console.Write("private static readonly int[] " + name + "Raft = {");
            for (int i = raftSpriteStartAddr; i < raftSpriteEndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void dumpOW(String name)
        {
            Console.Write("private static readonly int[] " + name + "OW = {");
            for (int i = OWSpriteStartAddr; i < OWSpriteEndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void dumpSleeper(String name)
        {
            Console.Write("private static readonly int[] " + name + "Sleeper = {");
            for (int i = sleeperSpriteStartAddr; i < sleeperSpriteEndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void dump1up(String name)
        {
            Console.Write("private static readonly int[] " + name + "1up = {");
            for (int i = oneUpSpriteStartAddr; i < oneUpSpriteEndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void ending1(String name)
        {
            Console.Write("private static readonly int[] " + name + "End1 = {");
            for (int i = endSprite1StartAddr; i < endSprite1EndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void ending2(String name)
        {
            Console.Write("private static readonly int[] " + name + "End2 = {");
            for (int i = endSprite2StartAddr; i < endSprite2EndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void ending3(String name)
        {
            Console.Write("private static readonly int[] " + name + "End3 = {");
            for (int i = endSprite3StartAddr; i < endSprite3EndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void dumpHead(String name)
        {
            Console.Write("private static readonly int[] " + name + "Head = {");
            for (int i = headSpriteStartAddr; i < headSpriteEndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void dumpSprite(String name)
        {
            Console.Write("private static readonly int[] " + name + "Sprite = {");
            for (int i = playerSpriteStartAddr; i < playerSpriteEndAddr; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }

        public void dumpColors(String name)
        {
            Console.WriteLine("private static readonly List<int[]> " + name + " = new List<int[]> { " + name + "Sprite, " + name + "1up, " + name + "OW, " + name + "Sleeper, " + name + "Title, " + name + "End1, " + name + "End2, " + name + "End3, " + name + "Head, " + name + "Raft, " + name + "Beam };");
            Console.WriteLine("0x2a0a: " + getByte(0x2a0a));
            Console.WriteLine("0x2a10: " + getByte(0x2a10));
            Console.WriteLine("Tunic: " + getByte(0x285c));
            Console.WriteLine("Tunic2: " + getByte(0x285b));
            Console.WriteLine("Tunic3: " + getByte(0x285a));
            Console.WriteLine("Shield: " + getByte(0xe9e));
            
        }

        public void dumpSamus()
        {
            Console.Write("private static readonly List<int> samusEnd = new List<int[]> { ");
            for(int i = 0x20010; i < 0x21010; i++)
            {
                Console.Write(getByte(i) + ", ");
            }
            Console.WriteLine("};");
        }
    }
}
