using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z2Randomizer;
using Z2Randomizer.Sidescroll;

namespace Z2Randomizer
{
    public class Shuffler
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const int maxTextLength = 3134;
        private const int numberOfTextEntries = 98;
        private const int baguTextIndex = 48;
        private const int bridgeTextIndex = 37;
        private const int downstabTextIndex = 47;
        private const int upstabTextIndex = 82;
        private const int trophyIndex = 13;
        private const int medIndex = 43;
        private const int kidIndex = 79;
        private const int numberOfHints = 4;
        private const int baguText = 50;
        private readonly int[] wizardindex = { 15, 24, 35, 46, 70, 81, 93, 96 };
        private static readonly List<int> rauruHints = new List<int> { 32, 12, 30 }; //Three houses, first screen
        private static readonly List<int> rutoHints = new List<int> { 18, 33, 25, 26 }; //error is 25 and 26, two houses, outside left
        private static readonly List<int> sariaHints = new List<int> { 50, 28 };//moving middle screen, sleeping thing, stationary right
        private static readonly List<int> kingsTomb = new List<int> { 51 };
        private static readonly List<int> midoHints = new List<int> { 45 };//moving old lady left, moving kid middle, inside house right
        private static readonly List<int> nabooruHints = new List<int> { 67, 64, 97 };//inside house right, moving bagu middle, stationary left, moving left, persistent left
        private static readonly List<int> daruniaHints = new List<int> { 77, 73 }; //wall first screen, outside last screen
        private static readonly List<int> newkasutoHints = new List<int> { 83, 68, 92 }; //outside first screen, wall first screen
        private static readonly List<int> oldkasutoHint = new List<int> { 74 };
        private const int rauruSign = 11;
        private const int rutoSign = 20;
        private const int sariaSign = 29;
        private const int midoSign = 41;
        private const int nabooruSign = 62;
        private const int daruniaSign = 76;
        private const int newKasutoSign = 86;
        private const int oldKasutoSign = 94;
        private static readonly List<int> bossRooms = new List<int> { 13, 34, 41 }; //break this up by palace group
        private static readonly List<int> bossRooms2 = new List<int> { 14, 28, 58 }; //break this up by palace group
        private static readonly List<int> bossRooms3 = new List<int> { 53, 54 };

        private readonly List<List<int>> hintIndexes = new List<List<int>> { rauruHints, rutoHints, sariaHints, kingsTomb, midoHints, nabooruHints, daruniaHints, newkasutoHints, oldkasutoHint };

        private readonly int[] rauruMoving = { 9, 10, };
        private readonly int[] rutoMoving = { 19, 17 };
        private readonly int[] sariaMoving = { 27 };
        private readonly int[] movingMido = { 40, 39 };
        private readonly int[] movingNabooru = { 61, 60 };
        private readonly int[] daruniaMoving = { 72, 75, };
        private readonly int[] newkasutoMoving = { 88, 89 };
        private const int errorTextIndex1 = 25;
        private const int errorTextIndex2 = 26;

        private readonly int[] drops = { 0x8a, 0x8b, 0x8c, 0x8d, 0x90, 0x91, 0x92, 0x88 };//items that can be dropped

        private readonly SortedDictionary<int, int> palaceConnectionLocs = new SortedDictionary<int, int>
        {
            {1, 0x1072B},
            {2, 0x1072B},
            {3, 0x12208},
            {4, 0x12208},
            {5, 0x1072B},
            {6, 0x12208},
            {7, 0x1472B},
        };

        private readonly Dictionary<int, int> palaceAddr = new Dictionary<int, int>
        {
            {1, 0x4663 },
            {2, 0x4664 },
            {3, 0x4665 },
            {4, 0xA140 },
            {5, 0x8663 },
            {6, 0x8664 },
            {7, 0x8665 }
        };

        //instance variables
        private RandomizerProperties props;
        private ROM ROMData;
        //private Character link;
        private Random R1;
        public Random R { get => R1; set => R1 = value; }
        public RandomizerProperties Props { get => props; set => props = value; }

        public Shuffler(RandomizerProperties props, ROM ROMData, Random R)
        {
            this.R1 = R;

            this.props = props;
            this.ROMData = ROMData;
        }

        public void GenerateHints(List<Location> itemLocs, Boolean startsWithTrophy, Boolean startsWithMedicine, Boolean startsWithKid, Dictionary<Spell, Spell> spellMap, Location bagu)
        {
            List<Hint> hints = ROMData.GetGameText();
            if (props.dashSpell)
            {
                hints[70] = new Hint(Util.ToGameText("USE THIS$TO GO$FAST", true), this);
            }
            if (props.useCommunityHints)
            {
                GenerateCommunityHints(hints);
            }

            if (props.spellItemHints)
            {
                GenerateSpellHints(itemLocs, hints, startsWithTrophy, startsWithMedicine, startsWithKid);
            }

            List<int> placedIndex = new List<int>();
            if (props.bagusWoods)
            {
                hints[baguText] = GenerateBaguHint(bagu);
                sariaHints.Remove(baguText);

            }
            if (props.helpfulHints)
            {
                placedIndex = GenerateHelpfulHints(hints, itemLocs);
            }

            if (props.spellItemHints || props.helpfulHints)
            {
                GenerateKnowNothings(hints, placedIndex);
            }

            if (props.townNameHints)
            {
                GenerateTownNameHints(hints, spellMap);
            }

            ROMData.TextToRom(hints);
        }

        private Hint GenerateBaguHint(Location bagu)
        {
            int baguy = bagu.Ypos - 30;
            int bagux = bagu.Xpos;
            String hint = "BAGU IN$";
            if (baguy < 25)
            {
                if (bagux < 21)
                {
                    hint += "NORTHWEST$";
                }
                else if (bagux < 42)
                {
                    hint += "NORTH$";
                }
                else
                {
                    hint += "NORTHEAST$";
                }
            }
            else if (baguy < 50)
            {
                if (bagux < 21)
                {
                    hint += "WEST$";
                }
                else if (bagux < 42)
                {
                    hint += "CENTER";
                }
                else
                {
                    hint += "EAST$";
                }
            }
            else
            {
                if (bagux < 21)
                {
                    hint += "SOUTHWEST$";
                }
                else if (bagux < 42)
                {
                    hint += "SOUTH$";
                }
                else
                {
                    hint += "SOUTHEAST$";
                }
            }
            hint += "WOODS";
            Hint baguH = new Hint(Util.ToGameText(hint, true), this);
            return baguH;
        }

        private void GenerateTownNameHints(List<Hint> hints, Dictionary<Spell, Spell> spellMap)
        {
            Hint h = new Hint(this);
            h.GenerateTownHint(spellMap[Spell.SHIELD]);
            hints[rauruSign] = h;

            h = new Hint(this);
            h.GenerateTownHint(spellMap[Spell.JUMP]);
            hints[rutoSign] = h;

            h = new Hint(this);
            h.GenerateTownHint(spellMap[Spell.LIFE]);
            hints[sariaSign] = h;

            h = new Hint(this);
            h.GenerateTownHint(spellMap[Spell.FAIRY]);
            hints[midoSign] = h;

            h = new Hint(this);
            h.GenerateTownHint(spellMap[Spell.FIRE]);
            hints[nabooruSign] = h;

            h = new Hint(this);
            h.GenerateTownHint(spellMap[Spell.REFLECT]);
            hints[daruniaSign] = h;

            h = new Hint(this);
            h.GenerateTownHint(spellMap[Spell.SPELL]);
            hints[newKasutoSign] = h;

            h = new Hint(this);
            h.GenerateTownHint(spellMap[Spell.THUNDER]);
            hints[oldKasutoSign] = h;
        }

        private void GenerateKnowNothings(List<Hint> hints, List<int> placedIndex)
        {
            List<int> stationary = new List<int>();
            stationary.AddRange(rauruHints.ToList());
            stationary.AddRange(rutoHints.ToList());
            stationary.AddRange(sariaHints.ToList());
            stationary.AddRange(midoHints.ToList());
            stationary.AddRange(nabooruHints.ToList());
            stationary.AddRange(daruniaHints.ToList());
            stationary.AddRange(newkasutoHints.ToList());
            stationary.AddRange(kingsTomb);
            stationary.AddRange(oldkasutoHint);

            List<int> moving = new List<int>();
            moving.AddRange(rauruMoving.ToList());
            moving.AddRange(rutoMoving.ToList());
            moving.AddRange(sariaMoving.ToList());
            moving.AddRange(movingMido.ToList());
            moving.AddRange(movingNabooru.ToList());
            moving.AddRange(daruniaMoving.ToList());
            moving.AddRange(newkasutoMoving.ToList());

            Hint knowNothing = new Hint(this);
            for (int i = 0; i < stationary.Count(); i++)
            {
                if (!placedIndex.Contains(stationary[i]))
                {
                    hints[stationary[i]] = knowNothing;
                }
            }

            for (int i = 0; i < moving.Count(); i++)
            {
                hints[moving[i]] = knowNothing;
            }
        }

        private List<int> GenerateHelpfulHints(List<Hint> hints, List<Location> itemLocs)
        {
            List<int> placedIndex = new List<int>();

            List<Item> placedItems = new List<Item>();
            bool placedSmall = false;
            List<Item> smallItems = new List<Item> { Item.BLUE_JAR, Item.XL_BAG, Item.KEY, Item.MEDIUM_BAG, Item.MAGIC_CONTAINER, Item.HEART_CONTAINER, Item.ONEUP, Item.RED_JAR, Item.SMALL_BAG, Item.LARGE_BAG };
            List<int> placedTowns = new List<int>();

            List<Item> it = new List<Item>();
            for (int i = 0; i < itemLocs.Count(); i++)
            {
                it.Add(itemLocs[i].item);
            }

            if (props.spellItemHints)
            {
                it.Remove(Item.TROPHY);
                it.Remove(Item.CHILD);
                it.Remove(Item.MEDICINE);
            }

            for (int i = 0; i < numberOfHints; i++)
            {
                Item doThis = it[R.Next(it.Count())];
                int tries = 0;
                while (((placedSmall && smallItems.Contains(doThis)) || placedItems.Contains(doThis)) && tries < 1000)
                {
                    doThis = it[R.Next(it.Count())];
                    tries++;
                }
                int j = 0;
                while (itemLocs[j].item != doThis)
                {
                    j++;
                }
                Hint hint = new Hint(this);
                hint.GenerateHelpfulHint(itemLocs[j]);
                int town = R.Next(9);
                while (placedTowns.Contains(town))
                {
                    town = R.Next(9);
                }
                int index = hintIndexes[town][R.Next(hintIndexes[town].Count())];
                if (index == errorTextIndex1 || index == errorTextIndex2)
                {
                    hints[errorTextIndex1] = hint;
                    hints[errorTextIndex2] = hint;
                    placedIndex.Add(errorTextIndex1);
                    placedIndex.Add(errorTextIndex2);
                }
                else
                {
                    hints[index] = hint;
                    placedIndex.Add(index);
                }

                placedTowns.Add(town);
                placedItems.Add(doThis);
                if (smallItems.Contains(doThis))
                {
                    placedSmall = true;
                }

            }
            return placedIndex;
        }

        private void GenerateSpellHints(List<Location> itemLocs, List<Hint> hints, Boolean startsWithTrophy, Boolean startsWithMedicine, Boolean startsWithKid)
        {
           
            foreach(Location itemLocation in itemLocs)
            {
                if (itemLocation.item == Item.TROPHY && !startsWithTrophy)
                {
                    Hint trophyHint = new Hint(this);
                    trophyHint.GenerateHelpfulHint(itemLocation);
                    hints[trophyIndex] = trophyHint;
                }
                else if (itemLocation.item == Item.MEDICINE && !startsWithMedicine)
                {
                    Hint medHint = new Hint(this);
                    medHint.GenerateHelpfulHint(itemLocation);
                    hints[medIndex] = medHint;
                }
                else if (itemLocation.item == Item.CHILD && !startsWithKid)
                {
                    Hint kidHint = new Hint(this);
                    kidHint.GenerateHelpfulHint(itemLocation);
                    hints[kidIndex] = kidHint;
                }
            }
        }

        private void GenerateCommunityHints(List<Hint> hints)
        {
            Hint.Reset();
            do
            {
                for (int i = 0; i < 8; i++)
                {
                    Hint wizardHint = new Hint(this);
                    wizardHint.GenerateCommunityHint("wizard");
                    hints.RemoveAt(wizardindex[i]);
                    hints.Insert(wizardindex[i], wizardHint);

                }

                Hint baguHint = new Hint(this);
                baguHint.GenerateCommunityHint("bagu");
                hints[baguTextIndex] = baguHint;

                Hint bridgeHint = new Hint(this);
                bridgeHint.GenerateCommunityHint("bridge");
                hints[bridgeTextIndex] = bridgeHint;

                Hint downstabHint = new Hint(this);
                downstabHint.GenerateCommunityHint("downstab");
                hints[downstabTextIndex] = downstabHint;

                Hint upstabHint = new Hint(this);
                upstabHint.GenerateCommunityHint("upstab");
                hints[upstabTextIndex] = upstabHint;

            } while (TextLength(hints) > maxTextLength);
        }

        private static int TextLength(List<Hint> texts)
        {
            int sum = 0;
            for (int i = 0; i < texts.Count(); i++)
            {
                sum += texts[i].Text.Count;
            }
            return sum;
        }

        public void ShufflePalacePalettes()
        {
            List<int[]> brickList = new List<int[]>();
            List<int[]> curtainList = new List<int[]>();
            List<int> bRows = new List<int>();
            List<int> binRows = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                int group = R.Next(3);
                int brickRow = R.Next(Graphics.brickgroup[group].GetLength(0));
                int curtainRow = R.Next(Graphics.curtaingroup[group].GetLength(0));

                int[] bricks = new int[3];
                int[] curtains = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    bricks[j] = Graphics.brickgroup[group][brickRow, j];
                    curtains[j] = Graphics.curtaingroup[group][curtainRow, j];
                }

                brickList.Add(bricks);
                curtainList.Add(curtains);

                bRows.Add(R.Next(7));
                binRows.Add(R.Next(7));
            }

            ROMData.WritePalacePalettes(brickList, curtainList, bRows, binRows);
        }

        public void ShuffleDrops()
        {
            List<int> small = new List<int>();
            List<int> large = new List<int>();
            if (props.randoDrops)
            {

                while (small.Count() == 0)
                {
                    for (int i = 0; i < drops.Length; i++)
                    {
                        if (R.NextDouble() > .5)
                        {
                            small.Add(drops[i]);
                        }
                    }
                }

                while (large.Count == 0)
                {
                    for (int i = 0; i < drops.Length; i++)
                    {
                        if (R.NextDouble() > .5)
                        {
                            large.Add(drops[i]);
                        }
                    }
                }
            }

            if (props.ShuffleEnemyDrops)
            {
                //private readonly int[] drops = { 0x8a, 0x8b, 0x8c, 0x8d, 0x90, 0x91, 0x92, 0x88 };

                if (props.smallbluejar)
                {
                    small.Add(0x90);
                }
                if (props.smallredjar)
                {
                    small.Add(0x91);
                }
                if (props.small50)
                {
                    small.Add(0x8a);
                }
                if (props.small100)
                {
                    small.Add(0x8b);
                }
                if (props.small200)
                {
                    small.Add(0x8c);
                }
                if (props.small500)
                {
                    small.Add(0x8d);
                }
                if (props.small1up)
                {
                    small.Add(0x92);
                }
                if (props.smallkey)
                {
                    small.Add(0x88);
                }
                if (props.largebluejar)
                {
                    large.Add(0x90);
                }
                if (props.largeredjar)
                {
                    large.Add(0x91);
                }
                if (props.large50)
                {
                    large.Add(0x8a);
                }
                if (props.large100)
                {
                    large.Add(0x8b);
                }
                if (props.large200)
                {
                    large.Add(0x8c);
                }
                if (props.large500)
                {
                    large.Add(0x8d);
                }
                if (props.large1up)
                {
                    large.Add(0x92);
                }
                if (props.largekey)
                {
                    large.Add(0x88);
                }
            }

            if (props.randoDrops || props.ShuffleEnemyDrops)
            {
                for (int i = 0; i < small.Count(); i++)
                {
                    int swap = R.Next(small.Count());
                    int temp = small[i];
                    small[i] = small[swap];
                    small[swap] = temp;
                }

                for (int i = 0; i < large.Count(); i++)
                {
                    int swap = R.Next(large.Count());
                    int temp = large[i];
                    large[i] = large[swap];
                    large[swap] = temp;
                }
                for (int i = 0; i < 8; i++)
                {
                    if (i < small.Count())
                    {
                        ROMData.Put(0x1E880 + i, (byte)small[i]);
                    }
                    else
                    {
                        ROMData.Put(0x1E880 + i, (byte)small[R.Next(small.Count())]);
                    }
                    if (i < large.Count())
                    {
                        ROMData.Put(0x1E888 + i, (byte)large[i]);
                    }
                    else
                    {
                        ROMData.Put(0x1E888 + i, (byte)large[R.Next(large.Count())]);
                    }
                }
            }
        }

        public void ShufflePbagAmounts()
        {
            if (props.shufflePbagXp)
            {
                ROMData.Put(0x1e800, (byte)R.Next(5, 10));
                ROMData.Put(0x1e801, (byte)R.Next(7, 12));
                ROMData.Put(0x1e802, (byte)R.Next(9, 14));
                ROMData.Put(0x1e803, (byte)R.Next(11, 16));
            }
        }

        public int ShuffleKasutoJars()
        {
            int kasutoJars = 7;
            if (props.kasutoJars)
            {
                kasutoJars = R.Next(5, 8);
                ROMData.WriteKasutoJarAmount(kasutoJars);
            }
            return kasutoJars;
        }

        public void ShuffleBossDrop()
        {
            int drop = drops[R.Next(drops.Count())];
            ROMData.Put(0x1de29, (byte)(drop - 0x80));

            /*
             * LE79A                                                                          ;
                lda      #$08                          ; 0x1e7aa $E79A A9 08                   ; A = 08
                sta      $EF                           ; 0x1e7ac $E79C 85 EF                   ; Sound Effects Type 4
                cpy      #$08                          ; 0x1e7ae $E79E C0 08                   ;
                bne      LE7BB                         ; 0x1e7b0 $E7A0 D0 19                   ;
                lda      $0728                         ; 0x1e7b2 $E7A2 AD 28 07                ; Related to boss key state
                beq      LE7B5                         ; 0x1e7b5 $E7A5 F0 0E                   ;
                lda      #$00                          ; 0x1e7b7 $E7A7 A9 00                   ; A = 00
                sta      $0728                         ; 0x1e7b9 $E7A9 8D 28 07                ;;_728_FreezeScrolling		= $728	;1=freeze screen, prevent from exiting left/right
                lda      $07FB                         ; 0x1e7bc $E7AC AD FB 07                ;
                bne      LE7B5                         ; 0x1e7bf $E7AF D0 04                   ;
                ;                                                                              ;Restart Music after taking a key that falls after beating a boss
                lda      #$02                          ; 0x1e7c1 $E7B1 A9 02                   ; A = 02 (04 = quiet version of Palace theme)
                sta      $EB                           ; 0x1e7c3 $E7B3 85 EB                   ; Music
                LE7B5                                                                          ;
                inc      $0793                         ; 0x1e7c5 $E7B5 EE 93 07                ; Number of Keys
                jmp      LE797                         ; 0x1e7c8 $E7B8 4C 97 E7    
            */
            ROMData.Put(0x1e7aa, new byte[] { 0xAD, 0x28, 0x07, 0xF0, 0x0E, 0xA9, 0x00, 0x8D, 0x28, 0x07, 0xAD, 0xFB, 0x07, 0xD0, 0x04, 0xa9, 0x02, 0x85, 0xeb, 0xa9, 0x08, 0x85, 0xef, 0xc0, 0x08, 0xd0, 0x06, 0xee, 0x93, 0x07, 0x4c, 0x97, 0xe7 });

            //jump to 1f33a
            ROMData.Put(0x1e81c, new byte[] { 0x20, 0x2a, 0xf3, 0xea });
            ROMData.Put(0x1e85b, new byte[] { 0x20, 0x35, 0xf3, 0xea });

            //1f33a

            //if $EB == 2
            //A5 eb
            //c9 02
            //f0 04
            //else $EB = 10
            //A9 10
            //85 eb
            //60
            ROMData.Put(0x1f33a, new byte[] { 0xA5, 0xEB, 0xC9, 0x02, 0xf0, 0x04, 0xa9, 0x10, 0x85, 0xeb, 0x60 });

            //1f345
            ROMData.Put(0x1f345, new byte[] { 0xA5, 0xEB, 0xC9, 0x02, 0xf0, 0x04, 0xa9, 0x00, 0x85, 0xeb, 0x60 });
        }

        public List<Palace> CreatePalaces(BackgroundWorker worker)
        {
            List<Palace> palaces = new List<Palace>();
            Dictionary<Byte[], List<Room>> sideviews = new Dictionary<Byte[], List<Room>>(new Util.MyEqualityComparer());
            Dictionary<Byte[], List<Room>> sideviewsgp = new Dictionary<Byte[], List<Room>>(new Util.MyEqualityComparer());
            int enemyBytes = 0;
            int enemyBytesgp = 0;
            int mapNo = 0;
            int mapNoGp = 0;
            if (props.createPalaces)
            {
                PalaceRooms.roomPool.Clear();
                PalaceRooms.roomPool.AddRange(PalaceRooms.palace1vanilla);
                PalaceRooms.roomPool.AddRange(PalaceRooms.palace2vanilla);
                PalaceRooms.roomPool.AddRange(PalaceRooms.palace3vanilla);
                PalaceRooms.roomPool.AddRange(PalaceRooms.palace4vanilla);
                PalaceRooms.roomPool.AddRange(PalaceRooms.palace5vanilla);
                PalaceRooms.roomPool.AddRange(PalaceRooms.palace6vanilla);
                if(props.customRooms)
                {
                    PalaceRooms.roomPool.AddRange(PalaceRooms.roomJamGTM);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.dmInPalaces);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.winterSolstice);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.maxRoomJam);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.dusterRoomJam);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.aaronRoomJam);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.knightCrawlerRoomJam);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.triforceOfCourage);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.benthicKing);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.easternShadow);
                    PalaceRooms.roomPool.AddRange(PalaceRooms.eunosRooms);
                }
                int[] sizes = new int[7];

                sizes[0] = R.Next(10, 17);
                sizes[1] = R.Next(16, 25);
                sizes[2] = R.Next(11, 18);
                sizes[3] = R.Next(16, 25);
                sizes[4] = R.Next(23, 63 - sizes[0] - sizes[1]);
                sizes[5] = R.Next(22, 63 - sizes[2] - sizes[3]);

                if (props.shortenGP)
                {
                    sizes[6] = R.Next(27, 41);
                }
                else
                {
                    sizes[6] = R.Next(54, 60);
                }
                
                for (int i = 1; i < 8; i++) //everything but gp
                {
                    
                    Palace p = new Palace(i, palaceAddr[i], palaceConnectionLocs[i], this.ROMData);
                    int tries = 0;
                    
                    do
                    {
                        if (worker != null && worker.CancellationPending)
                        {
                            return null;
                        }

                        tries = 0;
                        bool done = false;
                        do
                        {
                            if (i == 1)
                            {
                                mapNo = 0;
                            }
                            if (i == 2)
                            {
                                mapNo = palaces[0].AllRooms.Count;
                            }
                            if (i == 3)
                            {
                                mapNo = 0;
                            }
                            if (i == 4)
                            {
                                mapNo = palaces[2].AllRooms.Count;

                            }
                            if (i == 5)
                            {
                                mapNo = palaces[0].AllRooms.Count + palaces[1].AllRooms.Count;

                            }
                            if (i == 6)
                            {
                                mapNo = palaces[2].AllRooms.Count + palaces[3].AllRooms.Count;

                            }


                            if (i == 7)
                            {
                                mapNoGp = 0;
                            }

                            p = new Palace(i, palaceAddr[i], palaceConnectionLocs[i], this.ROMData);
                            p.Root = PalaceRooms.entrances[i - 1].deepCopy();

                            p.BossRoom = SelectBossRoom(i);

                            p.AllRooms.Add(p.Root);

                            p.AllRooms.Add(p.BossRoom);
                            if (i < 7)
                            {
                                p.ItemRoom = SelectItemRoom();
                                if((i == 1 || i == 2 || i == 5) && p.ItemRoom.HasBoss)
                                {
                                    p.ItemRoom.Enemies[1] = 0x6C;
                                }

                                p.AllRooms.Add(p.ItemRoom);

                                p.Root.Newmap = mapNo;
                                IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                p.BossRoom.Newmap = mapNo;
                                if (props.bossRoomConnect)
                                {
                                    p.BossRoom.RightByte = 0x69;
                                }
                                IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                p.ItemRoom.Newmap = mapNo;
                                p.ItemRoom.setItem((Item)i);
                                IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                if (p.ItemRoom.Map == 69)
                                {
                                    Room extra = PalaceRooms.maxBonusItemRoom.deepCopy();
                                    extra.Newmap = p.ItemRoom.Newmap;
                                    extra.setItem((Item)i);
                                    p.AllRooms.Add(extra);
                                    p.SortRoom(extra);
                                    p.SetOpenRoom(extra);
                                }
                                p.SortRoom(p.Root);
                                p.SortRoom(p.BossRoom);
                                p.SortRoom(p.ItemRoom);
                                p.SetOpenRoom(p.Root);
                            }
                            else
                            {
                                p.Root.Newmap = mapNoGp;
                                IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                p.BossRoom.Newmap = mapNoGp;
                                IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                p.SortRoom(p.Root);
                                p.SortRoom(p.BossRoom);
                                //thunderbird?
                                if (!props.removeTbird)
                                {
                                    p.Tbird = PalaceRooms.tbirdRooms[R.Next(PalaceRooms.tbirdRooms.Count)].deepCopy();
                                    p.Tbird.Newmap = mapNoGp;
                                    IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                    p.SortRoom(p.Tbird);
                                    p.AllRooms.Add(p.Tbird);
                                }
                                p.SetOpenRoom(p.Root);

                            }

                            
                            p.MaxRooms = sizes[i-1];
                            //add rooms
                            if (i == 7)
                            {
                                PalaceRooms.roomPool.Clear();
                                PalaceRooms.roomPool.AddRange(PalaceRooms.palace7vanilla);
                                if(props.customRooms)
                                {
                                    PalaceRooms.roomPool.AddRange(PalaceRooms.link7777RoomJam);
                                    PalaceRooms.roomPool.AddRange(PalaceRooms.gtmNewgpRooms);
                                    PalaceRooms.roomPool.AddRange(PalaceRooms.gtmOldgpRooms);
                                    PalaceRooms.roomPool.AddRange(PalaceRooms.winterSolsticeGP);
                                    PalaceRooms.roomPool.AddRange(PalaceRooms.eonRoomjam);
                                    PalaceRooms.roomPool.AddRange(PalaceRooms.triforceOfCourageGP);
                                    PalaceRooms.roomPool.AddRange(PalaceRooms.eunosGpRooms);
                                    PalaceRooms.roomPool.AddRange(PalaceRooms.flippedGp);

                                }
                            }
                            bool dropped = false;
                            while (p.AllRooms.Count < p.MaxRooms)
                            {
                                Room addThis = PalaceRooms.roomPool[R.Next(PalaceRooms.roomPool.Count)].deepCopy();
                                if (i < 7)
                                {
                                    addThis.Newmap = mapNo;
                                }
                                else
                                {
                                    addThis.Newmap = mapNoGp;
                                }
                                bool added = p.AddRoom(addThis, props.blockersAnywhere);
                                if (added)
                                {
                                    IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                    if(addThis.HasDrop && !dropped)
                                    {
                                        int numDrops = R.Next(Math.Min(3, p.MaxRooms - p.AllRooms.Count), Math.Min(6, p.MaxRooms - p.AllRooms.Count));
                                        bool lastDrop = true;
                                        int j = 0;
                                        while(j < numDrops && lastDrop)
                                        {
                                            Room r = PalaceRooms.roomPool[R.Next(PalaceRooms.roomPool.Count)].deepCopy();
                                            while(!r.DropZone)
                                            {
                                                r = PalaceRooms.roomPool[R.Next(PalaceRooms.roomPool.Count)].deepCopy();
                                            }
                                            if (i < 7)
                                            {
                                                r.Newmap = mapNo;
                                            }
                                            else
                                            {
                                                r.Newmap = mapNoGp;
                                            }
                                            bool added2 = p.AddRoom(r, props.blockersAnywhere);
                                            if(added2)
                                            {
                                                IncrementMapNo(ref mapNo, ref mapNoGp, i);
                                                lastDrop = r.HasDrop;
                                                j++;
                                            }
                                        }
                                    }
                                }

                                if (p.GetOpenRooms() >= p.MaxRooms - p.AllRooms.Count) //consolidate
                                {
                                    p.Consolidate();
                                }

                            }
                            done = true;
                            foreach (Room r in p.AllRooms)
                            {
                                if (r.getOpenExits() > 0)
                                {
                                    done = false;
                                }
                            }

                        } while (!done);

                        p.ShuffleRooms(R);
                        bool reachable = p.AllReachable();
                        bool keepgoing = reachable;
                        while ((!reachable || (i == 7 && (props.requireTbird && !p.RequiresThunderbird())) || p.HasDeadEnd()) && (tries < 10000))
                        {
                            p.ResetRooms();
                            p.ShuffleRooms(R);
                            reachable = p.AllReachable();
                            if(reachable)
                            {
                                keepgoing = true;
                            }
                            tries++;
                            logger.Trace(tries);
                        }
                    } while (tries >= 10000);
                    palaces.Add(p);
                    foreach (Room r in p.AllRooms)
                    {
                        if (i != 7)
                        {

                            enemyBytes += r.Enemies.Length;
                            if (sideviews.ContainsKey(r.SideView))
                            {
                                sideviews[r.SideView].Add(r);
                            }
                            else
                            {
                                List<Room> l = new List<Room> { r };
                                sideviews.Add(r.SideView, l);
                            }
                        }

                        else
                        {
                            enemyBytesgp += r.Enemies.Length;
                            if (sideviewsgp.ContainsKey(r.SideView))
                            {
                                sideviewsgp[r.SideView].Add(r);
                            }
                            else
                            {
                                List<Room> l = new List<Room> { r };
                                sideviewsgp.Add(r.SideView, l);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 1; i < 8; i++)
                {
                    int check2 = R.Next(10);

                    Palace p = new Palace(i, palaceAddr[i], palaceConnectionLocs[i], this.ROMData);
                    //p.dumpMaps();

                    p.Root = PalaceRooms.entrances[i - 1].deepCopy();
                    p.BossRoom = PalaceRooms.bossRooms[i - 1].deepCopy();
                    p.AllRooms.Add(p.Root);
                    if (i != 7)
                    {
                        p.ItemRoom = PalaceRooms.itemRooms[i - 1].deepCopy();
                        p.AllRooms.Add(p.ItemRoom);
                    }
                    p.AllRooms.Add(p.BossRoom);
                    if (i == 7)
                    {
                        Room bird = PalaceRooms.thunderBird.deepCopy();
                        p.AllRooms.Add(bird);
                        p.Tbird = bird;
                        
                    }
                    foreach (Room v in PalaceRooms.palaces[i - 1])
                    {
                        p.AllRooms.Add(v.deepCopy());
                    }
                    Boolean removeTbird = (i == 7 && props.removeTbird);
                    p.CreateTree(removeTbird);

                    if (i == 7 && props.shortenGP)
                    {
                        p.Shorten(R);
                    }
                    if (props.shufflePalaceRooms)
                    {
                        p.ShuffleRooms(R);
                    }
                    while (!p.AllReachable() || (i == 7 && (props.requireTbird && !p.RequiresThunderbird())) || p.HasDeadEnd())
                    {
                        p.ResetRooms();
                        if (props.shufflePalaceRooms)
                        {
                            p.ShuffleRooms(R);
                        }
                    }
                    palaces.Add(p);
                }
            } 
            int check = R.Next(10);

            for (int i = 0; i < 6; i++)
            {
                palaces[i].UpdateBlocks();
            }

            if (palaces[1].NeedGlove && !props.shufflePalaceItems && (props.shufflePalaceRooms || props.createPalaces))
            {
                return new List<Palace>();
            }

            if(!(props.shufflePalaceRooms || props.createPalaces))
            {
                palaces[1].NeedGlove = false;
            }
            //update pointers
            if (props.createPalaces)
            {
                Dictionary<int, int> freeSpace = SetupFreeSpace(true, 0);
                if (enemyBytes > 0x400 || enemyBytesgp > 681)
                {
                    return new List<Palace>();
                }
                int enemyPtr = 0x108b0;
                int enemyPtrgp = 0x148B0;
                foreach (byte[] sv in sideviews.Keys)
                {
                    int addr = FindFreeSpace(freeSpace, sv);
                    if (addr == -1) //not enough space
                    {
                        return new List<Palace>();
                    }
                    ROMData.Put(addr, sv);
                    if(ROMData.GetByte(addr + sv.Length) >= 0xD0)
                    {
                        ROMData.Put(addr + sv.Length, 0x00);
                    }
                    List<Room> rooms = sideviews[sv];
                    foreach (Room r in rooms)
                    {
                        if(r.Newmap == 45)
                        {
                            Console.Write("here");
                        }
                        int palSet = 1;
                        if (palaces[2].AllRooms.Contains(r) || palaces[3].AllRooms.Contains(r) || palaces[5].AllRooms.Contains(r))
                        {
                            palSet = 2;
                        }
                        r.writeSideViewPtr(addr, palSet, ROMData);
                        r.updateEnemies(enemyPtr, palSet, ROMData);
                        enemyPtr += r.Enemies.Length;
                        r.updateBitmask(palSet, ROMData);
                        bool entrance = false;
                        foreach(Palace p in palaces)
                        {
                            if(p.Root == r || p.BossRoom == r)
                            {
                                entrance = true;
                            }
                        }
                        r.updateConnectors(palSet, ROMData, entrance);
                    }

                }
                freeSpace = SetupFreeSpace(false, enemyBytesgp);
                foreach (byte[] sv in sideviewsgp.Keys)
                {
                    int addr = FindFreeSpace(freeSpace, sv);
                    if (addr == -1) //not enough space
                    {
                        return new List<Palace>();
                    }
                    ROMData.Put(addr, sv);
                    if (ROMData.GetByte(addr + sv.Length) >= 0xD0)
                    {
                        ROMData.Put(addr + sv.Length, 0x00);
                    }
                    List<Room> rooms = sideviewsgp[sv];
                    foreach (Room r in rooms)
                    {


                        r.writeSideViewPtr(addr, 3, ROMData);
                        r.updateEnemies(enemyPtrgp, 3, ROMData);
                        enemyPtrgp += r.Enemies.Length;
                        r.updateBitmask(3, ROMData);
                        r.updateConnectors(3, ROMData, r == palaces[6].Root);
                    }

                }

            }
            else
            {
                foreach (Palace p in palaces)
                {
                    p.UpdateRom();
                }
            }

            if (props.shuffleSmallItems || props.extraKeys)
            {
                palaces[0].ShuffleSmallItems(4, true, R, props.shuffleSmallItems, props.extraKeys, props.createPalaces);
                palaces[1].ShuffleSmallItems(4, true, R, props.shuffleSmallItems, props.extraKeys, props.createPalaces);
                palaces[2].ShuffleSmallItems(4, false, R, props.shuffleSmallItems, props.extraKeys, props.createPalaces);
                palaces[3].ShuffleSmallItems(4, false, R, props.shuffleSmallItems, props.extraKeys, props.createPalaces);
                palaces[4].ShuffleSmallItems(4, true, R, props.shuffleSmallItems, props.extraKeys, props.createPalaces);
                palaces[5].ShuffleSmallItems(4, false, R, props.shuffleSmallItems, props.extraKeys, props.createPalaces);
                palaces[6].ShuffleSmallItems(5, true, R, props.shuffleSmallItems, props.extraKeys, props.createPalaces);
            }
            return palaces;
        }

        private Room SelectBossRoom(int pal)
        {
            if(pal == 7)
            {
                return PalaceRooms.darkLinkRooms[R.Next(PalaceRooms.darkLinkRooms.Count)].deepCopy();
            }
            if(pal == 6)
            {
                return PalaceRooms.newp6BossRooms[R.Next(PalaceRooms.newp6BossRooms.Count)].deepCopy();
            }
            Room r = PalaceRooms.newBossRooms[R.Next(PalaceRooms.newBossRooms.Count)].deepCopy();
            r.Enemies = PalaceRooms.bossRooms[pal - 1].Enemies;
            return r;
        }

        private static void IncrementMapNo(ref int mapNo, ref int mapNoGp, int i)
        {
            if (i < 7)
            {
                mapNo++;
                //if (bossRooms.Contains(mapNo) && (i == 1 || i == 2 || i == 5))
                //{
                //    mapNo++;
                //}
                //else if(bossRooms2.Contains(mapNo) && (i == 3 || i == 4 || i == 6))
                //{
                //    mapNo++;
                //}
            }
            else
            {
                mapNoGp++;
                //while (bossRooms3.Contains(mapNoGp))
                //{
                //    mapNoGp++;
                //}
            }
        }

        private Dictionary<int, int> SetupFreeSpace(bool bank4, int enemyData)
        {
            Dictionary<int, int> freeSpace = new Dictionary<int, int>();
            if (bank4)
            {
                freeSpace.Add(0x103EC, 148);
                freeSpace.Add(0x10649, 226);
                freeSpace.Add(0x10827, 89);
                freeSpace.Add(0x10cb0, 1888);
                freeSpace.Add(0x11ef0, 288);
                freeSpace.Add(0x12124, 79);
                freeSpace.Add(0x1218b, 125);
                freeSpace.Add(0x12304, 1548);
            }
            else
            {
                freeSpace.Add(0x1435e, 385);
                freeSpace.Add(0x1462f, 251);
                freeSpace.Add(0x14827, 137);
                //freeSpace.Add(0x148b0 + enemyData, 681 - enemyData);
                freeSpace.Add(0x153be, 82);
                freeSpace.Add(0x1655f, 177);
                //freeSpace.Add(0x17db1, 447);
                freeSpace.Add(0x1f369, 1869);
            }
            return freeSpace;
        }
        public Room SelectItemRoom()
        {
            int dir = R.Next(5);
            if(dir == 0)
            {
                return PalaceRooms.leftOpenItemRooms[R.Next(PalaceRooms.leftOpenItemRooms.Count)].deepCopy();
            }
            else if(dir == 1)
            {
                return PalaceRooms.rightOpenItemRooms[R.Next(PalaceRooms.rightOpenItemRooms.Count)].deepCopy();

            }
            else if(dir == 2)
            {
                return PalaceRooms.upOpenItemRooms[R.Next(PalaceRooms.upOpenItemRooms.Count)].deepCopy();

            }
            else if(dir == 3)
            {
                return PalaceRooms.downOpenItemRooms[R.Next(PalaceRooms.downOpenItemRooms.Count)].deepCopy();

            }
            else
            {
                return PalaceRooms.throughItemRooms[R.Next(PalaceRooms.throughItemRooms.Count)].deepCopy();

            }
        }
        private int FindFreeSpace(Dictionary<int, int> freeSpace, byte[] sv)
        {
            foreach (int addr in freeSpace.Keys)
            {
                if (freeSpace[addr] > sv.Length)
                {
                    int oldSize = freeSpace[addr];
                    freeSpace.Remove(addr);
                    freeSpace.Add(addr + sv.Length, oldSize - sv.Length);
                    return addr;
                }
            }
            return -1;
        }
    }

}
