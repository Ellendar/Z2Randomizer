using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Z2Randomizer
{
    /*
     * 
    Change List:
        Removed items are no longer replaced with a pbag, they can be replaced with any small item
        Implemented a level cap, along with an option to scale xp requirements to the level cap
        Added an option for town signs to tell you what spell is in the town (thanks, Thirwolf!)
        Added an option to reduce the encounter spawn rate or turn them off entirely
        Introduced new exp shuffling levels
        Added an option to allow users to select their starting atk/mag/life level
        Added options to shuffle how continents are connected
        Added new overworld biomes (Islands, Canyon, Caldera, Volcano, Mountainous)
        Added an option to create new palaces
        Added the ability to use community based rooms in the palaces (thanks GTM, Scorpion__Max, eonHck, aaron2u2, TKnightCrawler, Duster, Link_7777)
        Reintroduced vanila maps
        Removed a few more extraneous rooms from spell houses
        Added an option to remap Up+A to controller 1
        Added an option to allow less important locations to be hidden on the overworld
        Added an option to restrict how connection caves are placed
        Added an option to allow connection caves to be boulder blocked
        Added an option to randomize which locations are hidden in New Kasuto/Hidden Palace spots
        Added an option to remove the flashing death screen
        Added an option to randomly select character sprite
        Added an option to randomize the item dropped by bosses
        Added an option to generate Bagu's Woods
        Fixed softlock when dying while talking (thanks eon!)
        Fixed a bug that robbed you of 1 attack power at attack 5 in rare situations
        Added the Yoshi, Dragonlord, Miria, Crystalis sprites (thanks TKnightCrawler!)
        Added the Pyramid sprite (thanks Plan!)
        Added the GliitchWiitch sprite (thanks RandomSaliance!)
        Added the Lady Link sprite (thanks eonHck!)
        Added the Hoodie Link sprite (thanks gtm!)
        Added the Taco sprite (thanks Warlock!)

        Double clicking on the box with the flags selects all of the text automatically
        Removed options to shuffle xp of bosses and enemies seperately
        Changed UI for level effectiveness
        Continued my never ending quest to get tooltips right
        UI now updates progress of seed generation


    Bug List:
        funky grass encounter in east hyrule
        death mountain encounters

    Todo List for version 4.0:
        Mess with drop rooms
        Add more rooms
        Update tooltips / Make sure UI is reasonable
        Update documentation

    Bugs in 4.0
       

    Feature List:

        Item shuffling
            More extreme item shuffling
            Duplicate items? (maybe a bad idea...)
        Move towns accross continents (tried this and it is very hard)
            Notes:
                Must swap map and world bytes for this to work
                Must update sanity checker
                How to deal with life town?
                How to deal with new kasuto?
        Palace columns tell you info about the palace
        New experience amounts *
        Shorten Towns
        More extreme enemy shuffle
            Slider? (easy, medium, crazy)
        Overworld Generation improvements
            Continent sizes (maybe just for maze island?)
        Randomize overworld caves
        Swap up/downstab
        Allow jars to restore health
        Allow fairies to restore magic
        New graphics/tile sets/etc.
        Spells as items?
        Tri-state checkboxes?
        Random%        
    */

    public enum spells { shield = 0, jump = 1, life = 2, fairy = 3, fire = 4, reflect = 5, spell = 6, thunder = 7, downstab = 8, upstab = 9 }

    class Hyrule
    { 
        private readonly int[] fireLocs = { 0x20850, 0x22850, 0x24850, 0x26850, 0x28850, 0x2a850, 0x2c850, 0x2e850, 0x36850, 0x32850, 0x34850, 0x38850 };

        private readonly int[] palPalettes = { 0, 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60 };
        private readonly int[] palGraphics = { 0, 0x04, 0x05, 0x09, 0x0A, 0x0B, 0x0C, 0x06 };

     
        private ROM romData;
        private int overworldXOff = 0x3F;
        private int overworldMapOff = 0x7E;
        private int overworldWorldOff = 0xBD;
        private Dictionary<int, int> spellEnters;
        private Dictionary<int, int> spellExits;
        public HashSet<String> reachableAreas;
        private Dictionary<spells, spells> spellMap; //key is location, value is spell (this is a bad implementation)
        private List<Location> itemLocs;
        private List<Location> pbagHearts;
        protected Dictionary<Location, List<Location>> connections;
        public SortedDictionary<String, List<Location>> areasByLocation;
        public Dictionary<Location, String> section;
        private int magContainers;
        private int heartContainers;
        private int startHearts;
        private int maxHearts;
        private int numHContainers;
        private int kasutoJars;
        private BackgroundWorker worker;
        

        private Character link;

        public Boolean[] itemGet;
        private Boolean[] spellGet;
        public Boolean hiddenPalace;
        public Boolean hiddenKasuto;
        private readonly int enemyAddr1 = 0x108B0;
        private readonly int enemyAddr2 = 0x148B0;
        private readonly int enemyPtr1 = 0x105B1;
        private readonly int enemyPtr2 = 0x1208E;
        private readonly int enemyPtr3 = 0x145B1;
        private readonly List<int> enemies1 = new List<int> { 3, 4, 12, 17, 18, 24, 25, 26, 29, 0x1E, 0x1F, 0x23 };
        private readonly List<int> flyingEnemies1 = new List<int> { 0x06, 0x07, 0x0E };
        private readonly List<int> generators1 = new List<int> { 0x0B, 0x0F, 0x1B, 0x0A };
        private readonly List<int> shorties1 = new List<int> { 0x03, 0x04, 0x11, 0x12 };
        private readonly List<int> tallGuys1 = new List<int> { 0x0C, 0x18, 0x19, 0x1A, 0x1D, 0x1E, 0x1F, 0x23 };
        private readonly List<int> enemies2 = new List<int> { 3, 4, 12, 17, 24, 25, 26, 29, 0x1F, 0x1E, 0x23 };
        private readonly List<int> flyingEnemies2 = new List<int> { 0x06, 0x07, 0x0E };
        private readonly List<int> generators2 = new List<int> { 0x0B, 0x1B, 0x0F };
        private readonly List<int> shorties2 = new List<int> { 0x03, 0x04, 0x11 };
        private readonly List<int> tallGuys2 = new List<int> { 0x0C, 0x18, 0x19, 0x1A, 0x1D, 0x1F, 0x1E, 0x23 };
        private readonly List<int> enemies3 = new List<int> { 3, 4, 17, 18, 24, 25, 26, 0x1D };
        private readonly List<int> flyingEnemies3 = new List<int> { 0x06, 0x14, 0x15, 0x17, 0x1E };
        private readonly List<int> generators3 = new List<int> { 0x0B, 0x0C, 0x0F, 0x16 };
        private readonly List<int> shorties3 = new List<int> { 0x03, 0x04, 0x11, 0x12 };
        private readonly List<int> tallGuys3 = new List<int> { 0x18, 0x19, 0x1A, 0x1D };
        private List<int> visitedEnemies;

        public WestHyrule westHyrule;
        public EastHyrule eastHyrule;
        private MazeIsland mazeIsland;
        private DeathMountain deathMountain;

        private Shuffler s;
        private Random r;
        private RandomizerProperties props;
        private List<World> worlds;
        private List<Palace> palaces;
        private bool startKid;
        private bool startTrophy;
        private bool startMed;

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

        public ROM ROMData
        {
            get
            {
                return romData;
            }

            set
            {
                romData = value;
            }
        }

        public Random R
        {
            get
            {
                return r;
            }

            set
            {
                r = value;
            }
        }

        public RandomizerProperties Props
        {
            get
            {
                return props;
            }

            set
            {
                props = value;
            }
        }

        public bool[] SpellGet { get => spellGet; set => spellGet = value; }

        public Hyrule(RandomizerProperties p, BackgroundWorker worker)
        {

            props = p;
            
            r = new Random(props.seed);
            ROMData = new ROM(props.filename);
            //ROMData.dumpAll("glitch");
            //ROMData.dumpSamus();
            Palace.dumpMaps(ROMData);
            this.worker = worker;


            link = new Character(props);
            s = new Shuffler(props, ROMData, link, r);

            palaces = new List<Palace>();
            itemGet = new Boolean[] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
            spellGet = new Boolean[] { false, false, false, false, false, false, false, false, false, false };
            reachableAreas = new HashSet<string>();
            areasByLocation = new SortedDictionary<string, List<Location>>();

            

            kasutoJars = s.shuffleKasutoJars();
            //ROMData.moveAfterGem();

            //Allows casting magic without requeueing a spell
            if (props.fastCast)
            {
                ROMData.writeFastCastMagic();
            }

            if (props.disableMusic)
            {
                ROMData.disableMusic();
            }

            
            

            if (props.hiddenPalace.Equals("Random"))
            {
                hiddenPalace = R.NextDouble() > .5;
            }
            else
            {
                hiddenPalace = props.hiddenPalace.Equals("On");
            }

            if (props.hiddenKasuto.Equals("Random"))
            {
                hiddenKasuto = R.NextDouble() > .5;
            }
            else
            {
                hiddenKasuto = props.hiddenKasuto.Equals("On");
            }

            ROMData.doHackyFixes();
            s.shuffleDrops();
            s.shufflePbagAmounts();

            ROMData.fixSoftLock();
            ROMData.extendMapSize();
            ROMData.disableTurningPalacesToStone();
            ROMData.updateMapPointers();
            ROMData.fixContinentTransitions();

            if (props.dashSpell)
            {
                ROMData.dashSpell();
            }

            /*
            Up + A:
        1cbba(cbaa): insert jump to d39a (1d3aa) (209ad3)

        1d3aa(d39a): store 707(8D0707) compare to 3(c903) less than 2 jump(3012) Load FB1 (ADb10f)compare with zero(c901) branch if zero(f00B) Load 561(AD6105) store accumulator into side memory(8Db00f) load accumulator with 1(a901) store to fb1(8db10f) return (60)

        d3bc(1d3cc): Load accumulator with fbo (adb00f)store to 561(8d6105) load 707(AD0707) return (60)

        feb3(1fec3): Store y into 707(8c0707) load 0(a900) stor into fb1(8db10f) return (60)

        CAD0(1CAE0): (20bcd3) c902 10

        CAE3(1CAF3): NOP NOP NOP(EAEAEA)

        CF92: (1CFA2): Jump to feb3(20b3fe)

            */

            if(props.upAC1)
            {
                ROMData.upAController1();
            }
            if (props.upaBox)
            {
                ROMData.put(0x1cbba, 0x20);
                ROMData.put(0x1cbbb, 0x9a);
                ROMData.put(0x1cbbc, 0xd3);

                ROMData.put(0x1d3aa, 0x8d);
                ROMData.put(0x1d3ab, 0x07);
                ROMData.put(0x1d3ac, 0x07);
                ROMData.put(0x1d3ad, 0xad);
                ROMData.put(0x1d3ae, 0x07);
                ROMData.put(0x1d3af, 0x07);
                ROMData.put(0x1d3b0, 0xc9);
                ROMData.put(0x1d3b1, 0x03);
                ROMData.put(0x1d3b2, 0x30);
                ROMData.put(0x1d3b3, 0x12);
                ROMData.put(0x1d3b4, 0xad);
                ROMData.put(0x1d3b5, 0xb0);
                ROMData.put(0x1d3b6, 0x0f);
                ROMData.put(0x1d3b7, 0xc9);
                ROMData.put(0x1d3b8, 0x01);
                ROMData.put(0x1d3b9, 0xf0);
                ROMData.put(0x1d3ba, 0x0b);
                ROMData.put(0x1d3bb, 0xad);
                ROMData.put(0x1d3bc, 0x61);
                ROMData.put(0x1d3bd, 0x05);
                ROMData.put(0x1d3be, 0x8d);
                ROMData.put(0x1d3bf, 0xb1);
                ROMData.put(0x1d3c0, 0x0f);
                ROMData.put(0x1d3c1, 0xa9);
                ROMData.put(0x1d3c2, 0x01);
                ROMData.put(0x1d3c3, 0x8D);
                ROMData.put(0x1d3c4, 0xB0);
                ROMData.put(0x1d3c5, 0x0F);
                ROMData.put(0x1d3c6, 0xad);
                ROMData.put(0x1d3c7, 0x07);
                ROMData.put(0x1d3c8, 0x07);
                ROMData.put(0x1d3c9, 0x29);
                ROMData.put(0x1d3ca, 0x07);
                ROMData.put(0x1d3cb, 0x60);
                ROMData.put(0x1d3cc, 0xad);
                ROMData.put(0x1d3cd, 0xb1);
                ROMData.put(0x1d3ce, 0x0f);
                ROMData.put(0x1d3cf, 0x8d);
                ROMData.put(0x1d3d0, 0x61);
                ROMData.put(0x1d3d1, 0x05);
                ROMData.put(0x1d3d2, 0x20);
                ROMData.put(0x1d3d3, 0x57);
                ROMData.put(0x1d3d4, 0xa0);
                ROMData.put(0x1d3d5, 0xad);
                ROMData.put(0x1d3d6, 0x07);
                ROMData.put(0x1d3d7, 0x07);
                ROMData.put(0x1d3d8, 0x60);

                //feb3(1fec3): Store y into 707(8c0707) load 0(a900) stor into fb1(8db10f) return (60)
                ROMData.put(0x1feca, 0x8c);
                ROMData.put(0x1fecb, 0x07);
                ROMData.put(0x1fecc, 0x07);
                ROMData.put(0x1fecd, 0xa9);
                ROMData.put(0x1fece, 0x00);
                ROMData.put(0x1fecf, 0x8d);
                ROMData.put(0x1fed0, 0xb0);
                ROMData.put(0x1fed1, 0x0f);
                ROMData.put(0x1fed2, 0x60);

                //CAD0(1CAE0): (20b7d3) c902 10
                ROMData.put(0x1cae0, 0x20);
                ROMData.put(0x1cae1, 0xbc);
                ROMData.put(0x1cae2, 0xd3);
                ROMData.put(0x1cae3, 0xc9);
                ROMData.put(0x1cae4, 0x03);
                ROMData.put(0x1cae5, 0x10);

                //CAE3(1CAF3): NOP NOP NOP(EAEAEA)
                ROMData.put(0x1caf3, 0xea);
                ROMData.put(0x1caf4, 0xea);
                ROMData.put(0x1caf5, 0xea);

                //CF92: (1CFA2): Jump to feba(20bafe)
                ROMData.put(0x1cfa2, 0x20);
                ROMData.put(0x1cfa3, 0xba);
                ROMData.put(0x1cfa4, 0xfe);
            }

            if (props.permanentBeam)
            {
                ROMData.put(0x186c, 0xEA);
                ROMData.put(0x186d, 0xEA);
            }

            if (props.standardizeDrops)
            {
                ROMData.put(0x1e8bd, 0x20);
                ROMData.put(0x1e8be, 0x4c);
                ROMData.put(0x1e8bf, 0xff);

                ROMData.put(0x1ff5c, 0xc0);
                ROMData.put(0x1ff5d, 0x02);
                ROMData.put(0x1ff5e, 0xd0);
                ROMData.put(0x1ff5f, 0x07);
                ROMData.put(0x1ff60, 0xad);
                ROMData.put(0x1ff61, 0xfe);
                ROMData.put(0x1ff62, 0x06);
                ROMData.put(0x1ff63, 0xee);
                ROMData.put(0x1ff64, 0xfe);
                ROMData.put(0x1ff65, 0x06);
                ROMData.put(0x1ff66, 0x60);
                ROMData.put(0x1ff67, 0xad);
                ROMData.put(0x1ff68, 0xff);
                ROMData.put(0x1ff69, 0x06);
                ROMData.put(0x1ff6a, 0xee);
                ROMData.put(0x1ff6b, 0xff);
                ROMData.put(0x1ff6c, 0x06);
                ROMData.put(0x1ff6d, 0x60);

            }

            //load 706 (3) (AD0607)
            //cmp 03 (2) (c903)
            //bne 2 (2) (d0 03)
            //store 01 in 706 (3) (
            //do the math (3)
            //push to stack (1)
            //load 70a (3)
            //store to 706 (3)
            //pop stack (1)
            //rts (1)

            spellEnters = new Dictionary<int, int>();
            spellEnters.Add(0, 0x90);
            spellEnters.Add(1, 0x94);
            spellEnters.Add(2, 0x98);
            spellEnters.Add(3, 0x9C);
            spellEnters.Add(4, 0xA0);
            spellEnters.Add(5, 0xA4);
            spellEnters.Add(6, 0x4D);
            spellEnters.Add(7, 0xAC);
            spellEnters.Add(8, 0xB0);
            spellEnters.Add(9, 0xB4);

            spellExits = new Dictionary<int, int>();
            spellExits.Add(0, 0xC1);
            spellExits.Add(1, 0xC5);
            spellExits.Add(2, 0xC9);
            spellExits.Add(3, 0xCD);
            spellExits.Add(4, 0xD1);
            spellExits.Add(5, 0xD5);
            spellExits.Add(6, 0x6A);
            spellExits.Add(7, 0xDD);
            spellExits.Add(8, 0xE1);
            spellExits.Add(9, 0xE5);

            shortenWizards();
            magContainers = 4;
            visitedEnemies = new List<int>();

            randomizeStartingValues();
            randomizeEnemies();


            palaces = s.createPalaces(worker);
            while(palaces == null || palaces.Count != 7)
            {
                if (palaces == null)
                {
                    return;
                }
                palaces = s.createPalaces(worker);
                
            }
            Console.WriteLine("Random: " + r.Next(10));
            if (props.shufflePalaceEnemies)
            {
                shuffleE(enemyPtr1, enemyAddr1, enemies1, generators1, shorties1, tallGuys1, flyingEnemies1, false);
                shuffleE(enemyPtr2, enemyAddr1, enemies2, generators2, shorties2, tallGuys2, flyingEnemies2, false);
                shuffleE(enemyPtr3, enemyAddr2, enemies3, generators3, shorties3, tallGuys3, flyingEnemies3, true);
            }

            processOverworld();
            bool f = updateProgress(8);
            if(!f)
            {
                return;
            }
            s.generateHints(itemLocs, startTrophy, startMed, startKid, spellMap, westHyrule.bagu);
            f = updateProgress(9);
            if (!f)
            {
                return;
            }
            updateRom();
            String newFileName = props.filename.Substring(0, props.filename.LastIndexOf("\\") + 1) + "Z2_" + props.seed + "_" + props.flags + ".nes";
            ROMData.dump(newFileName);




        }



        /*
            Text Notes:
            
            Community Text Changes
            ----------------------
            Shield Spell    15  43
            Cannot Help     16  35
            Jump Spell      24  34
            Life Spell      35  37
            You know..?     37  42
            Fairy           46  37
            Downstab        47  38
            Bagu            48  44
            Fire            70  43
            You know        71  34
            Reflect         81  37
            Upstab          82  32
            Spell           93  25
            Thunder         96  36 
        */
        

        private void shuffleAttackEffectiveness(bool ohko)
        {
            if (!ohko)
            {
                int[] atk = new int[8];
                for (int i = 0; i < 8; i++)
                {
                    atk[i] = ROMData.getByte(0x1E67D + i);
                }

                for (int i = 0; i < atk.Length; i++)
                {
                    int minAtk = (int)Math.Ceiling(atk[i] - atk[i] * .333);
                    int maxAtk = (int)(atk[i] + atk[i] * .5);
                    int next = atk[i];

                    if (props.shuffleAtkEff)
                    {
                        next = r.Next(minAtk, maxAtk);
                    }
                    else if (props.highAtk)
                    {
                        next = (int)(atk[i] + (atk[i] * .4));
                    }
                    else if (props.lowAtk)
                    {
                        next = (int)(atk[i] * .5);
                    }

                    if (props.shuffleAtkEff)
                    {
                        if (i == 0)
                        {
                            atk[i] = Math.Max(next, 2);
                        }
                        else
                        {
                            if (next < atk[i - 1])
                            {
                                atk[i] = atk[i - 1];
                            }
                            else
                            {
                                atk[i] = next;
                            }
                        }
                    }
                    else
                    {
                        atk[i] = next;
                    }
                }


                for (int i = 0; i < 8; i++)
                {
                    ROMData.put(0x1E67D + i, (Byte)atk[i]);
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    ROMData.put(0x1E67D + i, (Byte)192);
                }
            }
        }

        private void shuffleItems()
        {
            List<items> itemList = new List<items> { items.candle, items.glove, items.raft, items.boots, items.horn, items.cross, items.heartcontainer, items.heartcontainer, items.magiccontainer, items.medicine, items.trophy, items.heartcontainer, items.heartcontainer, items.magiccontainer, items.magickey, items.magiccontainer, items.hammer, items.kid, items.magiccontainer };
            List<items> replaceList = new List<items> { items.bluejar, items.redjar, items.smallbag, items.hundobag, items.twohundobag, items.fivehundobag, items.oneup, items.key };
            Location kidLoc = mazeIsland.kid;
            Location medicineLoc = westHyrule.medCave;
            Location trophyLoc = westHyrule.trophyCave;
            numHContainers = maxHearts - startHearts;
            for (int i = 0; i < itemGet.Count(); i++)
            {
                itemGet[i] = false;
            }
            foreach (Location l in itemLocs)
            {
                l.itemGet = false;
            }
            westHyrule.pbagCave.itemGet = false;
            eastHyrule.pbagCave1.itemGet = false;
            eastHyrule.pbagCave2.itemGet = false;
            if (props.shuffleItems)
            {
                for (int i = 0; i < 8; i++)
                {
                    bool hasItem = r.NextDouble() > .75;
                    ROMData.put(0x17B01 + i, hasItem ? (Byte)1 : (Byte)0);
                    itemGet[i] = hasItem;
                }
            }
            else
            {
                ROMData.put(0x17B01, props.startCandle ? (Byte)1 : (Byte)0);
                itemGet[(int)items.candle] = props.startCandle;
                ROMData.put(0x17B02, props.startGlove ? (Byte)1 : (Byte)0);
                itemGet[(int)items.glove] = props.startGlove;
                ROMData.put(0x17B03, props.startRaft ? (Byte)1 : (Byte)0);
                itemGet[(int)items.raft] = props.startRaft;
                ROMData.put(0x17B04, props.startBoots ? (Byte)1 : (Byte)0);
                itemGet[(int)items.boots] = props.startBoots;
                ROMData.put(0x17B05, props.startFlute ? (Byte)1 : (Byte)0);
                itemGet[(int)items.horn] = props.startFlute;
                ROMData.put(0x17B06, props.startCross ? (Byte)1 : (Byte)0);
                itemGet[(int)items.cross] = props.startCross;
                ROMData.put(0x17B07, props.startHammer ? (Byte)1 : (Byte)0);
                itemGet[(int)items.hammer] = props.startHammer;
                ROMData.put(0x17B08, props.startKey ? (Byte)1 : (Byte)0);
                itemGet[(int)items.magickey] = props.startKey;
            }
            itemList = new List<items> { items.candle, items.glove, items.raft, items.boots, items.horn, items.cross, items.heartcontainer, items.heartcontainer, items.magiccontainer, items.medicine, items.trophy, items.heartcontainer, items.heartcontainer, items.magiccontainer, items.magickey, items.magiccontainer, items.hammer, items.kid, items.magiccontainer };

            if (props.pbagItemShuffle)
            {
                westHyrule.pbagCave.item = (items)ROMData.getByte(0x4FE2);
                eastHyrule.pbagCave1.item = (items)ROMData.getByte(0x8ECC);
                eastHyrule.pbagCave2.item = (items)ROMData.getByte(0x8FB3);
                itemList.Add(westHyrule.pbagCave.item);
                itemList.Add(eastHyrule.pbagCave1.item);
                itemList.Add(eastHyrule.pbagCave2.item);

            }
            pbagHearts = new List<Location>();
            if (numHContainers < 4)
            {
                int x = 4 - numHContainers;
                while (x > 0)
                {
                    int remove = r.Next(itemList.Count);
                    if (itemList[remove] == items.heartcontainer)
                    {
                        itemList[remove] = replaceList[R.Next(replaceList.Count)];
                        x--;
                    }
                }
            }

            if (numHContainers > 4)
            {
                if (props.pbagItemShuffle)
                {
                    int x = numHContainers - 4;
                    while (x > 0)
                    {
                        itemList[22 - x] = items.heartcontainer;
                        x--;
                    }
                }
                else
                {
                    int x = numHContainers - 4;
                    while (x > 0)
                    {
                        int y = r.Next(3);
                        if (y == 0 && !pbagHearts.Contains(westHyrule.pbagCave))
                        {
                            pbagHearts.Add(westHyrule.pbagCave);
                            westHyrule.pbagCave.item = items.heartcontainer;
                            itemList.Add(items.heartcontainer);
                            itemLocs.Add(westHyrule.pbagCave);
                            x--;
                        }
                        if (y == 1 && !pbagHearts.Contains(eastHyrule.pbagCave1))
                        {
                            pbagHearts.Add(eastHyrule.pbagCave1);
                            eastHyrule.pbagCave1.item = items.heartcontainer;
                            itemList.Add(items.heartcontainer);
                            itemLocs.Add(eastHyrule.pbagCave1);
                            x--;
                        }
                        if (y == 2 && !pbagHearts.Contains(eastHyrule.pbagCave2))
                        {
                            pbagHearts.Add(eastHyrule.pbagCave2);
                            eastHyrule.pbagCave2.item = items.heartcontainer;
                            itemList.Add(items.heartcontainer);
                            itemLocs.Add(eastHyrule.pbagCave2);
                            x--;
                        }
                    }
                }
            }

            if(props.removeSpellItems)
            {
                itemList[9] = replaceList[R.Next(replaceList.Count)];
                itemList[10] = replaceList[R.Next(replaceList.Count)];
                itemList[17] = replaceList[R.Next(replaceList.Count)];
                itemGet[(int)items.trophy] = true;
                itemGet[(int)items.medicine] = true;
                itemGet[(int)items.kid] = true;

            }

            if (spellGet[(int)spellMap[spells.fairy]])
            {
                itemList[9] = replaceList[R.Next(replaceList.Count)];
                itemGet[(int)items.medicine] = true;
                startMed = true;
            }

            if(spellGet[(int)spellMap[spells.jump]])
            {
                itemList[10] = replaceList[R.Next(replaceList.Count)];
                itemGet[(int)items.trophy] = true;
                startTrophy = true;
            }

            if(spellGet[(int)spellMap[spells.reflect]])
            {
                itemList[17] = replaceList[R.Next(replaceList.Count)];
                itemGet[(int)items.kid] = true;
                startKid = true;
            }

            if (itemGet[0])
            {
                itemList[0] = replaceList[R.Next(replaceList.Count)];
            }

            if (itemGet[1])
            {
                itemList[1] = replaceList[R.Next(replaceList.Count)];
            }

            if (itemGet[2])
            {
                itemList[2] = replaceList[R.Next(replaceList.Count)];
            }

            if (itemGet[3])
            {
                itemList[3] = replaceList[R.Next(replaceList.Count)];
            }

            if (itemGet[4])
            {
                itemList[4] = replaceList[R.Next(replaceList.Count)];
            }

            if (itemGet[5])
            {
                itemList[5] = replaceList[R.Next(replaceList.Count)];
            }

            if (itemGet[7])
            {
                itemList[14] = replaceList[R.Next(replaceList.Count)];
            }

            if (itemGet[6])
            {
                itemList[16] = replaceList[R.Next(replaceList.Count)];
            }


            if (props.mixOverworldPalaceItems)
            {
                for (int i = 0; i < itemList.Count; i++)
                {

                    int s = R.Next(i, itemList.Count);
                    items sl = itemList[s];
                    itemList[s] = itemList[i];
                    itemList[i] = sl;
                }
            }
            else
            {
                if (props.shufflePalaceItems)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        int s = R.Next(i, 6);
                        items sl = itemList[s];
                        itemList[s] = itemList[i];
                        itemList[i] = sl;
                    }
                }
                else
                {

                }

                if (props.shuffleOverworldItems)
                {
                    for (int i = 6; i < itemList.Count; i++)
                    {
                        int s = R.Next(i, itemList.Count);
                        items sl = itemList[s];
                        itemList[s] = itemList[i];
                        itemList[i] = sl;
                    }
                }
            }
            for (int i = 0; i < itemList.Count; i++)
            {
                itemLocs[i].item = itemList[i];
            }
            foreach (Location l in itemLocs)
            {
                if (l.item == items.kid)
                {
                    kidLoc = l;
                }
                else if (l.item == items.trophy)
                {
                    trophyLoc = l;
                }
                else if (l.item == items.medicine)
                {
                    medicineLoc = l;
                }
            }

            for (int i = 0; i < 64; i++)
            {
                Byte heartByte = ROMData.getByte(0x27810 + i);
                ROMData.put(0x29810 + i, heartByte);
                ROMData.put(0x2B810 + i, heartByte);
                ROMData.put(0x2D810 + i, heartByte);
                ROMData.put(0x33810 + i, heartByte);
                ROMData.put(0x35810 + i, heartByte);
                ROMData.put(0x37810 + i, heartByte);
                ROMData.put(0x39810 + i, heartByte);
            }


        }

        private Boolean everythingReachable()
        {
            //return true;
            int dm = 0;
            int mi = 0;
            int wh = 0;
            int eh = 0;
            int count = 1;
            int prevCount = 0;
            magContainers = 4;
            heartContainers = startHearts;

            int total = westHyrule.AllLocations.Count + eastHyrule.AllLocations.Count + deathMountain.AllLocations.Count + mazeIsland.AllLocations.Count;
            Boolean f = false;
            Boolean g = false;
            while (prevCount != count || f || g)
            {
                prevCount = count;
                westHyrule.updateVisit();
                deathMountain.updateVisit();
                eastHyrule.updateVisit();
                mazeIsland.updateVisit();

                foreach(World w in worlds)
                {
                    if(w.raft != null && canGet(w.raft) && itemGet[(int)items.raft])
                    {
                        foreach(World w2 in worlds)
                        {
                            w2.visitRaft();
                        }
                    }

                    if (w.bridge != null && canGet(w.bridge))
                    {
                        foreach (World w2 in worlds)
                        {
                            w2.visitBridge();
                        }
                    }

                    if (w.cave1 != null && canGet(w.cave1))
                    {
                        foreach (World w2 in worlds)
                        {
                            w2.visitCave1();
                        }
                    }

                    if (w.cave2 != null && canGet(w.cave2))
                    {
                        foreach (World w2 in worlds)
                        {
                            w2.visitCave2();
                        }
                    }
                }
                westHyrule.updateVisit();
                deathMountain.updateVisit();
                eastHyrule.updateVisit();
                mazeIsland.updateVisit();

                f = updateItems();
                g = updateSpells();

                
               

                count = 0;
                dm = 0;
                mi = 0;
                wh = 0;
                eh = 0;

                foreach (Location l in westHyrule.AllLocations)
                {
                    if (l.Reachable)
                    {
                        count++;
                        wh++;
                    }
                }

                foreach (Location l in eastHyrule.AllLocations)
                {
                    if (l.Reachable)
                    {
                        count++;
                        eh++;
                    }
                }

                foreach (Location l in deathMountain.AllLocations)
                {
                    if (l.Reachable)
                    {
                        count++;
                        dm++;
                    }
                }

                foreach (Location l in mazeIsland.AllLocations)
                {
                    if (l.Reachable)
                    {
                        count++;
                        mi++;
                    }
                }
            }
            Console.WriteLine("Reached: " + count);
            Console.WriteLine("wh: " + wh + " / " + westHyrule.AllLocations.Count);
            Console.WriteLine("eh: " + eh + " / " + eastHyrule.AllLocations.Count);
            Console.WriteLine("dm: " + dm + " / " + deathMountain.AllLocations.Count);
            Console.WriteLine("mi: " + mi + " / " + mazeIsland.AllLocations.Count);
            for (int i = 0; i < 8; i++)
            {
                if (itemGet[i] == false)
                {
                    return false;
                }
            }

            for (int i = 19; i < 22; i++)
            {
                if (itemGet[i] == false)
                {
                    return false;
                }
            }
            if (magContainers != 8)
            {
                return false;
            }
            if (heartContainers != maxHearts)
            {
                return false;
            }
            for (int i = 0; i < spellGet.Count(); i++)
            {
                if (!spellGet[i])
                {
                    return false;
                }
            }

            return (canGet(westHyrule.Towns) && canGet(eastHyrule.Towns) && canGet(westHyrule.palace1) && canGet(westHyrule.palace2) && canGet(westHyrule.palace3) && canGet(mazeIsland.palace4) && canGet(eastHyrule.palace5) && canGet(eastHyrule.palace6) && canGet(eastHyrule.gp) && canGet(itemLocs) && canGet(westHyrule.bagu) && (!hiddenKasuto || (canGet(eastHyrule.hkLoc))) && (!hiddenPalace || (canGet(eastHyrule.hpLoc))));
        }

        private Boolean canGet(List<Location> l)
        {
            foreach (Location ls in l)
            {
                if (ls.Reachable == false)
                {
                    return false;
                }
            }
            return true;
        }
        private Boolean canGet(Location l)
        {

            return l.Reachable;
        }

        private void shortenWizards()
        {
            /*
            Spell swap notes:
            Shield exit: 0xC7BB, 0xC1; enter: 0xC7EC, 0x90 //change map 48 pointer to map 40 pointer
        Jump exit: 0xC7BF, 0xC5; enter: 0xC7F0, 0x94 //change map 49 pointer to map 41 pointer
        Life exit: 0xC7C3, 0xC9; enter 0xC7F4, 0x98 //change map 50 pointer to map 42 pointer
        Fairy exit: 0xC7C7, 0xCD; enter 0xC7F8, 0x9C //change map 51 pointer to map 43 pointer
        Fire exit: 0xC7Cb, 0xD1; enter 0xC7FC, 0xA0 //change map 52 pointer to map 44 pointer
        Reflect exit: 0xC7Cf, 0xD5; enter 0xC800, 0xA4 //change map 53 pointer to map 45 pointer
        Spell exit: 0xC7D3, 0x6A; enter 0xC795, 0xC796, 0x4D //new kasuto item?
        Thunder exit: 0xC7D7, 0xDD; enter 0xC808, 0xAC
        Downstab exit: 0xC7DB, 0xE1; enter 0xC80C, 0xB0
        Upstab exit: 0xC7DF, 0xE5; enter 0xC810, 0xB4
    */
            for (int i = 0; i < 16; i = i + 2)
            {
                ROMData.put(0xC611 + i, (Byte)0x75);
                ROMData.put(0xC611 + i + 1, (Byte)0x70);
                ROMData.put(0xC593 + i, (Byte)0x48);
                ROMData.put(0xC593 + i + 1, (Byte)0x9B);
            }
            ROMData.put(0xC7BB, (Byte)0x07);
            ROMData.put(0xC7BF, (Byte)0x13);
            ROMData.put(0xC7C3, (Byte)0x21);
            ROMData.put(0xC7C7, (Byte)0x27);
            ROMData.put(0xC7CB, (Byte)0x37);
            ROMData.put(0xC7CF, (Byte)0x3F);
            ROMData.put(0xC850, (Byte)0xB0);
            //ROMData.put(0xC7D3, (Byte)0x4D);
            ROMData.put(0xC7D7, (Byte)0x5E);
            ROMData.put(0xC7DF, (Byte)0x43);
            ROMData.put(0xC870, (Byte)0xB8);
            ROMData.put(0xC7E3, (Byte)0x49);
            ROMData.put(0xC874, (Byte)0xA8);
            ROMData.put(0xC7D3, (Byte)0x4D);
            ROMData.put(0xC7DB, (Byte)0x29);
            //ROMData.put(0xC7E3, (Byte)0x49);
            // ROMData.put(0xC874, (Byte)0xA8);
            //ROMData.put(0x8560, (Byte)0xBC);
        }

        private Boolean updateSpells()
        {
            Location[] t = new Location[11];
            t[westHyrule.shieldTown.townNum] = westHyrule.shieldTown;
            t[westHyrule.jump.townNum] = westHyrule.jump;
            t[westHyrule.lifeNorth.townNum] = westHyrule.lifeNorth;
            t[westHyrule.lifeSouth.townNum] = westHyrule.lifeSouth;
            t[westHyrule.fairy.townNum] = westHyrule.fairy;
            t[eastHyrule.fireTown.townNum] = eastHyrule.fireTown;
            t[eastHyrule.darunia.townNum] = eastHyrule.darunia;
            t[eastHyrule.newKasuto.townNum] = eastHyrule.newKasuto;
            t[eastHyrule.newKasuto2.townNum] = eastHyrule.newKasuto2;
            t[eastHyrule.oldKasuto.townNum] = eastHyrule.oldKasuto;
            Boolean changed = false;
            foreach (spells s in spellMap.Keys)
            {
                if (s == spells.fairy && (((itemGet[(int)items.medicine] || props.removeSpellItems) && westHyrule.fairy.townNum == 5) || (westHyrule.fairy.townNum == 10 && (magContainers >= 8 || props.disableMagicRecs))) && canGet(westHyrule.fairy))
                {
                    if(!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
                else if (s == spells.jump && (((itemGet[(int)items.trophy] || props.removeSpellItems) && westHyrule.jump.townNum == 2) || (westHyrule.jump.townNum == 7 && (magContainers >= 6 || props.disableMagicRecs) && (itemGet[(int)items.kid] || props.removeSpellItems))) && canGet(westHyrule.jump))
                {
                    if (!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
                else if (s == spells.downstab && (spellGet[(int)spells.jump] || spellGet[(int)spells.fairy]) && canGet(t[5]))
                {
                    if (!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
                else if (s == spells.upstab && (spellGet[(int)spells.jump]) && canGet(t[7]))
                {
                    if (!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
                else if (s == spells.life && (canGet(westHyrule.lifeNorth)) && (((magContainers >= 7 || props.disableMagicRecs) && westHyrule.lifeNorth.townNum == 8) || westHyrule.lifeNorth.townNum == 3))
                {
                    if (!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
                else if (s == spells.shield && (canGet(westHyrule.shieldTown)) && (((magContainers >= 5 || props.disableMagicRecs) && westHyrule.shieldTown.townNum == 6) || westHyrule.shieldTown.townNum == 1))
                {
                    if (!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
                else if (s == spells.reflect && ((eastHyrule.darunia.townNum == 2 && (itemGet[(int)items.trophy] || props.removeSpellItems)) || ((itemGet[(int)items.kid] || props.removeSpellItems) && eastHyrule.darunia.townNum == 7 && (magContainers >= 6 || props.disableMagicRecs))) && canGet(eastHyrule.darunia))
                {
                    if (!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
                else if (s == spells.fire && (canGet(eastHyrule.fireTown)) && (((magContainers >= 5 || props.disableMagicRecs) && eastHyrule.fireTown.townNum == 6) || eastHyrule.fireTown.townNum == 1))
                {
                    if (!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
                else if (s == spells.spell && (canGet(eastHyrule.newKasuto)) && (((magContainers >= 7 || props.disableMagicRecs) && eastHyrule.newKasuto.townNum == 8) || eastHyrule.newKasuto.townNum == 3))
                {
                    if (!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
                else if (s == spells.thunder && (canGet(eastHyrule.oldKasuto)) && (((magContainers >= 8 || props.disableMagicRecs) && eastHyrule.oldKasuto.townNum == 10) || (eastHyrule.oldKasuto.townNum == 5 && (itemGet[(int)items.medicine] || props.removeSpellItems))))
                {
                    if (!spellGet[(int)spellMap[s]])
                    {
                        changed = true;
                    }
                    spellGet[(int)spellMap[s]] = true;
                }
            }
            return changed;
        }

        private Boolean updateItems()
        {
            
            Boolean changed = false;
            foreach (Location l in itemLocs)
            {
                Boolean itemGotten = l.itemGet;
                if (l.PalNum > 0 && l.PalNum < 7)
                {

                        

                    Palace p = palaces[l.PalNum - 1];
                    if (l.PalNum == 4 && l.item == items.kid)
                    {
                        Console.WriteLine("here");
                    }
                    l.itemGet = itemGet[(int)l.item] = canGet(l) && (spellGet[(int)spells.fairy] || itemGet[(int)items.magickey]) && (!p.NeedDstab || (p.NeedDstab && spellGet[(int)spells.downstab])) && (!p.NeedFairy || (p.NeedFairy && spellGet[(int)spells.fairy])) && (!p.NeedGlove || (p.NeedGlove && itemGet[(int)items.glove])) && (!p.NeedJumpOrFairy || (p.NeedJumpOrFairy && (spellGet[(int)spells.jump]) || spellGet[(int)spells.fairy])) && (!p.NeedReflect || (p.NeedReflect && spellGet[(int)spells.reflect]));
                }
                else if (l.townNum == 8)
                {
                    l.itemGet = itemGet[(int)l.item] = canGet(l) && (magContainers >= kasutoJars) && (!l.Needhammer || itemGet[(int)items.hammer]);
                }
                else if (l.townNum == 9)
                {
                    l.itemGet = itemGet[(int)l.item] = (canGet(l) && spellGet[(int)spells.spell]) && (!l.Needhammer || itemGet[(int)items.hammer]);
                }
                else
                {
                    l.itemGet = itemGet[(int)l.item] = canGet(l) && (!l.Needhammer || itemGet[(int)items.hammer]) && (!l.NeedRecorder || itemGet[(int)items.horn]);
                }
                if (itemGotten != l.itemGet && l.item == items.magiccontainer)
                {
                    magContainers++;
                }
                if (itemGotten != l.itemGet && l.item == items.heartcontainer)
                {
                    heartContainers++;
                }
                if(!itemGotten && l.itemGet)
                {
                    changed = true;
                }
            }
            return changed;
        }
        private void shuffleLifeEffectiveness(bool isMag)
        {

            int numBanks = 7;
            int start = 0x1E2BF;
            if (isMag)
            {
                numBanks = 8;
                start = 0xD8B;
            }
            int[,] life = new int[numBanks, 8];
            for (int i = 0; i < numBanks; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int lifeVal = ROMData.getByte(start + (i * 8) + j);
                    int highPart = (lifeVal & 0xF0) >> 4;
                    int lowPart = lifeVal & 0x0F;
                    life[i, j] = highPart * 8 + lowPart / 2;
                }
            }

            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < numBanks; i++)
                {
                    int nextVal = life[i, j];
                    if ((props.shuffleLifeEff && !isMag) || (props.shuffleMagEff && isMag))
                    {
                        int max = (int)(life[i, j] + life[i, j] * .5);
                        int min = (int)(life[i, j] - life[i, j] * .5);
                        if(!isMag)
                        {
                            min = (int)(life[i, j] - life[i, j] * .25);
                        }
                        if (j == 0)
                        {
                            nextVal = r.Next(min, Math.Min(max, 120));
                        }
                        else
                        {
                            nextVal = r.Next(min, Math.Min(max, 120));
                            if (nextVal > life[i, j - 1])
                            {
                                nextVal = life[i, j - 1];
                            }
                        }
                    }
                    else if (props.highMag && isMag)
                    {
                        nextVal = (int)(life[i, j] + (life[i, j] * .5));
                    }
                    else if (props.highDef && !isMag || props.lowMag && isMag)
                    {
                        nextVal = (int)(life[i, j] * .5);
                    }

                    if (isMag && nextVal > 120)
                    {
                        nextVal = 120;
                    }
                    life[i, j] = nextVal;
                }
            }

            for (int i = 0; i < numBanks; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int highPart = (life[i, j] / 8) << 4;
                    int lowPart = (life[i, j] % 8);
                    ROMData.put(start + (i * 8) + j, (Byte)(highPart + (lowPart * 2)));
                }
            }

        }

        private void randomizeEnemies()
        {
            if (props.shuffleEnemyHP)
            {
                shuffleHP(0x5434, 0x5453);
                shuffleHP(0x9434, 0x944E);
                shuffleHP(0x11435, 0x11435);
                shuffleHP(0x11437, 0x11454);
                shuffleHP(0x13C86, 0x13C87);
                shuffleHP(0x15434, 0x15438);
                shuffleHP(0x15440, 0x15443);
                shuffleHP(0x15445, 0x1544B);
                shuffleHP(0x1544E, 0x1544E);
                shuffleHP(0x12935, 0x12935);
                shuffleHP(0x12937, 0x12954);
            }

            if (props.ohkoEnemies)
            {
                shuffleAttackEffectiveness(true);
                ROMData.put(0x005432, (Byte)193);
                ROMData.put(0x009432, (Byte)193);
                ROMData.put(0x11436, (Byte)193);
                ROMData.put(0x12936, (Byte)193);
                ROMData.put(0x15532, (Byte)193);
                ROMData.put(0x11437, (Byte)192);
                ROMData.put(0x1143F, (Byte)192);
                ROMData.put(0x12937, (Byte)192);
                ROMData.put(0x1293F, (Byte)192);
                ROMData.put(0x15445, (Byte)192);
                ROMData.put(0x15446, (Byte)192);
                ROMData.put(0x15448, (Byte)192);
                ROMData.put(0x15453, (Byte)193);
                ROMData.put(0x12951, (Byte)227);

            }
        }

        private void shuffleHP(int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                int newVal = 0;
                int val = (int)ROMData.getByte(i);

                newVal = r.Next((int)(val * 0.5), (int)(val * 1.5));
                if (newVal > 255)
                {
                    newVal = 255;
                }

                ROMData.put(i, (Byte)newVal);
            }
        }

        private void processOverworld()
        {
            if (props.shuffleSmallItems)
            {
                shuffleSmallItems(1, true);
                shuffleSmallItems(1, false);
                shuffleSmallItems(2, true);
                shuffleSmallItems(2, false);
                shuffleSmallItems(3, true);
                //shuffleSmallItems(4, true);
                //shuffleSmallItems(4, false);
            }
            do
            {
                worlds = new List<World>();
                westHyrule = new WestHyrule(this);
                deathMountain = new DeathMountain(this);
                eastHyrule = new EastHyrule(this);
                mazeIsland = new MazeIsland(this);
                worlds.Add(westHyrule);
                worlds.Add(deathMountain);
                worlds.Add(eastHyrule);
                worlds.Add(mazeIsland);
                shuffleTowns();


                //shuffle continent connections
                //westHyrule.loadRaft(2);
                //eastHyrule.loadRaft(0);
                //westHyrule.loadCave1(1);
                //deathMountain.loadCave1(0);
                //westHyrule.loadCave2(1);
                //deathMountain.loadCave2(0);
                //eastHyrule.loadBridge(3);
                //mazeIsland.loadBridge(2);

                do
                {

                    worlds = new List<World>();
                    westHyrule = new WestHyrule(this);
                    deathMountain = new DeathMountain(this);
                    eastHyrule = new EastHyrule(this);
                    mazeIsland = new MazeIsland(this);
                    worlds.Add(westHyrule);
                    worlds.Add(deathMountain);
                    worlds.Add(eastHyrule);
                    worlds.Add(mazeIsland);
                    shuffleTowns();

                    if (props.continentConnections.Equals("Normal") || props.continentConnections.Equals("R+B Border Shuffle"))
                    {
                        westHyrule.loadCave1(1);
                        westHyrule.loadCave2(1);
                        westHyrule.loadRaft(2);

                        deathMountain.loadCave1(0);
                        deathMountain.loadCave2(0);

                        eastHyrule.loadRaft(0);
                        eastHyrule.loadBridge(3);

                        mazeIsland.loadBridge(2);
                    }
                    else if (props.continentConnections.Equals("Transportation Shuffle"))
                    {
                        List<int> chosen = new List<int>();
                        int type = R.Next(4);
                        if (props.westBiome.Equals("Vanilla") || props.westBiome.Equals("Vanilla (shuffled)") || props.dmBiome.Equals("Vanilla") || props.dmBiome.Equals("Vanilla (shuffled)"))
                        {
                            type = 3;
                        }

                        setTransportation(0, 1, type);
                        chosen.Add(type);
                        if (props.westBiome.Equals("Vanilla") || props.westBiome.Equals("Vanilla (shuffled)") || props.dmBiome.Equals("Vanilla") || props.dmBiome.Equals("Vanilla (shuffled)"))
                        {
                            type = 0;
                        }
                        else
                        {
                            do
                            {
                                type = R.Next(4);
                            } while (chosen.Contains(type));
                        }
                        setTransportation(0, 1, type);
                        chosen.Add(type);
                        if (props.westBiome.Equals("Vanilla") || props.westBiome.Equals("Vanilla (shuffled)") || props.eastBiome.Equals("Vanilla") || props.eastBiome.Equals("Vanilla (shuffled)"))
                        {
                            type = 1;
                        }
                        else
                        {
                            do
                            {
                                type = R.Next(4);
                            } while (chosen.Contains(type));
                        }
                        setTransportation(0, 2, type);
                        chosen.Add(type);
                        if (props.eastBiome.Equals("Vanilla") || props.eastBiome.Equals("Vanilla (shuffled)") || props.mazeBiome.Equals("Vanilla") || props.mazeBiome.Equals("Vanilla (shuffled)"))
                        {
                            type = 2;
                        }
                        else
                        {
                            do
                            {
                                type = R.Next(4);
                            } while (chosen.Contains(type));
                        }
                        setTransportation(2, 3, type);
                    }
                    else
                    {
                        List<int> doNotPick = new List<int>();
                        if (props.westBiome.Equals("Vanilla") || props.westBiome.Equals("Vanilla (shuffled)"))
                        {
                            doNotPick.Add(0);
                        }
                        if (props.eastBiome.Equals("Vanilla") || props.eastBiome.Equals("Vanilla (shuffled)"))
                        {
                            doNotPick.Add(2);
                        }
                        if (props.dmBiome.Equals("Vanilla") || props.dmBiome.Equals("Vanilla (shuffled)"))
                        {
                            doNotPick.Add(1);
                        }
                        if (props.mazeBiome.Equals("Vanilla") || props.mazeBiome.Equals("Vanilla (shuffled)"))
                        {
                            doNotPick.Add(3);
                        }

                        int raftw1 = R.Next(worlds.Count);

                        if (props.westBiome.Equals("Vanilla") || props.westBiome.Equals("Vanilla (shuffled)"))
                        {
                            raftw1 = 0;
                        }
                        else
                        {
                            while (doNotPick.Contains(raftw1))
                            {
                                raftw1 = R.Next(worlds.Count);
                            }
                        }


                        int raftw2 = R.Next(worlds.Count);
                        if (props.eastBiome.Equals("Vanilla") || props.eastBiome.Equals("Vanilla (shuffled)"))
                        {
                            raftw2 = 2;
                        }
                        else
                        {
                            do
                            {
                                raftw2 = R.Next(worlds.Count);
                            } while (raftw1 == raftw2 || doNotPick.Contains(raftw2));
                        }

                        worlds[raftw1].loadRaft(raftw2);
                        worlds[raftw2].loadRaft(raftw1);

                        int bridgew1 = R.Next(worlds.Count);
                        if (props.eastBiome.Equals("Vanilla") || props.eastBiome.Equals("Vanilla (shuffled)"))
                        {
                            bridgew1 = 2;
                        }
                        else
                        {
                            while (doNotPick.Contains(bridgew1))
                            {
                                bridgew1 = R.Next(worlds.Count);
                            }
                        }
                        int bridgew2 = R.Next(worlds.Count);
                        if (props.mazeBiome.Equals("Vanilla") || props.mazeBiome.Equals("Vanilla (shuffled)"))
                        {
                            bridgew2 = 3;
                        }
                        else
                        {
                            do
                            {
                                bridgew2 = R.Next(worlds.Count);
                            } while (bridgew1 == bridgew2 || doNotPick.Contains(bridgew2));
                        }

                        worlds[bridgew1].loadBridge(bridgew2);
                        worlds[bridgew2].loadBridge(bridgew1);

                        int c1w1 = R.Next(worlds.Count);
                        if (props.westBiome.Equals("Vanilla") || props.westBiome.Equals("Vanilla (shuffled)"))
                        {
                            c1w1 = 0;
                        }
                        else
                        {
                            while (doNotPick.Contains(c1w1))
                            {
                                c1w1 = R.Next(worlds.Count);
                            }
                        }
                        int c1w2 = R.Next(worlds.Count);
                        if (props.dmBiome.Equals("Vanilla") || props.dmBiome.Equals("Vanilla (shuffled)"))
                        {
                            c1w2 = 1;
                        }
                        else
                        {
                            do
                            {
                                c1w2 = R.Next(worlds.Count);
                            } while (c1w1 == c1w2 || doNotPick.Contains(c1w2));
                        }

                        worlds[c1w1].loadCave1(c1w2);
                        worlds[c1w2].loadCave1(c1w1);

                        int c2w1 = R.Next(worlds.Count);
                        if (props.westBiome.Equals("Vanilla") || props.westBiome.Equals("Vanilla (shuffled)"))
                        {
                            c2w1 = 0;
                        }
                        else
                        {
                            while (doNotPick.Contains(c2w1))
                            {
                                c2w1 = R.Next(worlds.Count);
                            }
                        }
                        int c2w2 = R.Next(worlds.Count);
                        if (props.dmBiome.Equals("Vanilla") || props.dmBiome.Equals("Vanilla (shuffled)"))
                        {
                            c2w2 = 1;
                        }
                        else
                        {
                            do
                            {
                                c2w2 = R.Next(worlds.Count);
                            } while (c2w1 == c2w2 || doNotPick.Contains(c2w2));
                        }

                        worlds[c2w1].loadCave2(c2w2);
                        worlds[c2w2].loadCave2(c2w1);
                    }
                } while (!allContinentsHaveConnection(worlds));

                int wtries = 0;
                int x = 0;
                do
                {
                    bool g = updateProgress(2);
                    if (!g)
                    {
                        return;
                    }
                    wtries++;
                    if (!westHyrule.Allreached)
                    {
                        bool f = false;
                        do
                        {
                            f = westHyrule.terraform();
                        } while (!f);
                    }
                    
                    westHyrule.reset();


                    g = updateProgress(3);
                    if (!g)
                    {
                        return;
                    }
                    if (!deathMountain.Allreached)
                    {
                        bool f = false;
                        do
                        {
                            f = deathMountain.terraform();
                        } while (!f);
                    }

                        deathMountain.reset();

                    g = updateProgress(4);
                    if (!g)
                    {
                        return;
                    }
                    if (!eastHyrule.Allreached)
                    {
                        bool f = false;
                        do
                        {
                            f = eastHyrule.terraform();
                        } while (!f);
                    }

                        eastHyrule.reset();

                    g = updateProgress(5);
                    if (!g)
                    {
                        return;
                    }
                    if (!mazeIsland.Allreached)
                    {
                        bool f = false;
                        do
                        {
                            f = mazeIsland.terraform();
                        } while (!f);
                    }

                        mazeIsland.reset();
                    g = updateProgress(6);
                    if (!g)
                    {
                        return;
                    }
                    loadItemLocs();
                    shuffleSpells();
                    shuffleItems();
                    foreach (Location l in itemLocs)
                    {
                        if (l.PalNum == 4 && l.item == items.kid)
                        {
                            Console.WriteLine("here");
                        }
                    }
                    shufflePalaces();
                    loadItemLocs();
                    westHyrule.setStart();
                    g = updateProgress(7);
                    if (!g)
                    {
                        return;
                    }
                    x = 0;
                    while (!everythingReachable() && x < 10)
                    {
                        westHyrule.allReachable();
                        eastHyrule.allReachable();
                        mazeIsland.allReachable();
                        deathMountain.allReachable();
                        foreach (Location l in westHyrule.AllLocations)
                        {
                            l.Reachable = false;
                        }

                        foreach (Location l in eastHyrule.AllLocations)
                        {
                            l.Reachable = false;
                        }

                        foreach (Location l in mazeIsland.AllLocations)
                        {
                            l.Reachable = false;
                        }

                        foreach (Location l in deathMountain.AllLocations)
                        {
                            l.Reachable = false;
                        }
                        eastHyrule.newKasuto2.Reachable = false;
                        //eastHyrule.bridge.Reachable = false;
                        startMed = false;
                        startTrophy = false;
                        startKid = false;
                        westHyrule.reset();
                        eastHyrule.reset();
                        mazeIsland.reset();
                        shuffleSpells();
                        loadItemLocs();
                        deathMountain.reset();
                        westHyrule.setStart();
                        shuffleItems();
                        foreach (Location l in itemLocs)
                        {
                            if (l.PalNum == 4 && l.item == items.kid)
                            {
                                Console.WriteLine("here");
                            }
                        }
                        shufflePalaces();
                        loadItemLocs();

                        

                        x++;

                    }
                    int west = 0;
                    if (x != 10)
                    {
                        break;
                    }
                    foreach (Location l in westHyrule.AllLocations)
                    {
                        if (l.Reachable)
                        {
                            west++;
                        }
                    }

                    int east = 0;
                    foreach (Location l in eastHyrule.AllLocations)
                    {
                        if (l.Reachable)
                        {
                            east++;
                        }
                    }

                    int maze = 0;
                    foreach (Location l in mazeIsland.AllLocations)
                    {
                        if (l.Reachable)
                        {
                            maze++;
                        }
                    }

                    int dm = 0;
                    foreach (Location l in deathMountain.AllLocations)
                    {
                        if (l.Reachable)
                        {
                            dm++;
                        }
                    }

                    Console.WriteLine("wr: " + west + " / " + westHyrule.AllLocations.Count);
                    Console.WriteLine("er: " + east + " / " + eastHyrule.AllLocations.Count);
                    Console.WriteLine("dm: " + dm + " / " + deathMountain.AllLocations.Count);
                    Console.WriteLine("maze: " + maze + " / " + mazeIsland.AllLocations.Count);
                } while (wtries < 10 && !everythingReachable());
                if(x != 10 && wtries != 10)
                {
                    break;
                }
            } while (!everythingReachable()) ;

            if (props.shuffleOverworldEnemies)
            {
                foreach (World w in worlds)
                {
                    w.shuffleE();
                }
            }
        }

        private bool updateProgress(int v)
        {
            if (worker != null)
            {
                if (worker.CancellationPending)
                {
                    return false;
                }
                worker.ReportProgress(v);
            }
            return true;
        }

        private void setTransportation(int w1, int w2, int type)
        {
            if(type == 1)
            {
                worlds[w1].loadRaft(w2);
                worlds[w2].loadRaft(w1);
            }
            else if (type == 2)
            {
                worlds[w1].loadBridge(w2);
                worlds[w2].loadBridge(w1);
            }
            else if(type == 3)
            {
                worlds[w1].loadCave1(w2);
                worlds[w2].loadCave1(w1);

            }
            else
            {
                worlds[w1].loadCave2(w2);
                worlds[w2].loadCave2(w1);
            }
        }

        private bool allContinentsHaveConnection(List<World> worlds)
        {
            foreach (World w in worlds)
            {
                if (!w.hasConnections())
                {
                    return false;
                }
            }
            return true;
        }

        private void shuffleTowns()
        {
            westHyrule.shieldTown.townNum = 1;
            westHyrule.jump.townNum = 2;
            westHyrule.lifeNorth.townNum = 3;
            westHyrule.lifeSouth.townNum = 4;
            westHyrule.fairy.townNum = 5;
            eastHyrule.fireTown.townNum = 6;
            eastHyrule.darunia.townNum = 7;
            eastHyrule.newKasuto.townNum = 8;
            eastHyrule.newKasuto2.townNum = 9;
            eastHyrule.oldKasuto.townNum = 10;

            if(props.townSwap)
            {
                if(R.NextDouble() > .5)
                {
                    Util.swap(westHyrule.shieldTown, eastHyrule.fireTown);
                }
                if (R.NextDouble() > .5)
                {
                    Util.swap(westHyrule.jump, eastHyrule.darunia);
                }
                if (R.NextDouble() > .5)
                {
                    Util.swap(westHyrule.lifeNorth, eastHyrule.newKasuto);
                    Util.swap(westHyrule.lifeSouth, eastHyrule.newKasuto2);

                    eastHyrule.newKasuto.NeedBagu = true;
                    eastHyrule.newKasuto2.NeedBagu = true;

                    westHyrule.lifeNorth.NeedBagu = false;
                    westHyrule.lifeSouth.NeedBagu = false;

                    westHyrule.connections.Remove(westHyrule.lifeNorth);
                    westHyrule.connections.Remove(westHyrule.lifeSouth);

                    eastHyrule.connections.Add(eastHyrule.newKasuto, eastHyrule.newKasuto2);
                    eastHyrule.connections.Add(eastHyrule.newKasuto2, eastHyrule.newKasuto);

                    westHyrule.AllLocations.Remove(westHyrule.lifeSouth);

                    eastHyrule.AllLocations.Add(eastHyrule.newKasuto2);



                }
                if (R.NextDouble() > .5)
                {
                    Util.swap(westHyrule.fairy, eastHyrule.oldKasuto);
                }
            }
        }

        private void shufflePalaces()
        {

            if (props.swapPalaceCont)
            {

                List<Location> pals = new List<Location> { westHyrule.palace1, westHyrule.palace2, westHyrule.palace3, mazeIsland.palace4, eastHyrule.palace5, eastHyrule.palace6 };

                if (props.p7shuffle)
                {
                    pals.Add(eastHyrule.gp);
                }

                for (int i = 0; i < pals.Count; i++)
                {
                    int swapp = r.Next(i, pals.Count);
                    Util.swap(pals[i], pals[swapp]);
                }

                westHyrule.palace1.World = westHyrule.palace1.World & 0xFC;
                westHyrule.palace2.World = westHyrule.palace2.World & 0xFC;
                westHyrule.palace3.World = westHyrule.palace3.World & 0xFC;

                mazeIsland.palace4.World = mazeIsland.palace4.World & 0xFC;
                mazeIsland.palace4.World = mazeIsland.palace4.World | 0x03;

                eastHyrule.palace5.World = eastHyrule.palace5.World & 0xFC;
                eastHyrule.palace5.World = eastHyrule.palace5.World | 0x02;

                eastHyrule.palace6.World = eastHyrule.palace6.World & 0xFC;
                eastHyrule.palace6.World = eastHyrule.palace6.World | 0x02;

                if (props.p7shuffle)
                {
                    eastHyrule.gp.World = eastHyrule.gp.World & 0xFC;
                    eastHyrule.gp.World = eastHyrule.gp.World | 0x02;
                }

                /*
                subroutine start bf60(13f70)

                instruction: 20 60 bf

                subroutine:
                    load 22 into accumulator    A9 22
                    xor with $561               4D 61 05
                    return                      60


                Gooma / helmet head fix (CHECK THESE):
                    13c96 = d0--hitbox / exp / hp
                    13d88 = d0--sprite info
                    13ad6 = d0--behavior
                    11b2d = d0(don't need?)
                    */

                //write subroutine
                ROMData.put(0x13f70, 0xA9);
                byte helmetRoom = 0x22;
                if (props.createPalaces)
                {
                    helmetRoom = (byte)palaces[1].BossRoom.Newmap;
                }
                ROMData.put(0x13f71, helmetRoom);
                ROMData.put(0x13f72, 0x4D);
                ROMData.put(0x13f73, 0x61);
                ROMData.put(0x13f74, 0x05);
                ROMData.put(0x13f75, 0x60);

                //jump to subroutine
                ROMData.put(0x13c93, 0x20);
                ROMData.put(0x13c94, 0x60);
                ROMData.put(0x13c95, 0xBF);

                ROMData.put(0x13d85, 0x20);
                ROMData.put(0x13d86, 0x60);
                ROMData.put(0x13d87, 0xBF);

                ROMData.put(0x13ad3, 0x20);
                ROMData.put(0x13ad4, 0x60);
                ROMData.put(0x13ad5, 0xBF);

                //fix for key glitch
                ROMData.put(0x11b37, 0xea);
                ROMData.put(0x11b38, 0xea);
                ROMData.put(0x11b39, 0xea);
            }

        }

        private List<Location> loadItemLocs()
        {
            itemLocs = new List<Location>();
            if (westHyrule.palace1.PalNum != 7)
            {
                itemLocs.Add(westHyrule.palace1);
            }
            if (westHyrule.palace2.PalNum != 7)
            {
                itemLocs.Add(westHyrule.palace2);
            }
            if (westHyrule.palace3.PalNum != 7)
            {
                itemLocs.Add(westHyrule.palace3);
            }
            if (mazeIsland.palace4.PalNum != 7)
            {
                itemLocs.Add(mazeIsland.palace4);
            }
            if (eastHyrule.palace5.PalNum != 7)
            {
                itemLocs.Add(eastHyrule.palace5);
            }
            if (eastHyrule.palace6.PalNum != 7)
            {
                itemLocs.Add(eastHyrule.palace6);
            }
            if (eastHyrule.gp.PalNum != 7)
            {
                itemLocs.Add(eastHyrule.gp);
            }
            itemLocs.Add(westHyrule.heart1);
            itemLocs.Add(westHyrule.heart2);
            itemLocs.Add(westHyrule.jar);
            itemLocs.Add(westHyrule.medCave);
            itemLocs.Add(westHyrule.trophyCave);
            itemLocs.Add(eastHyrule.heart1);
            itemLocs.Add(eastHyrule.heart2);
            if (eastHyrule.newKasuto.townNum == 8)
            {
                itemLocs.Add(eastHyrule.newKasuto);
                itemLocs.Add(eastHyrule.newKasuto2);
            } 
            else
            {
                itemLocs.Add(westHyrule.lifeNorth);
                itemLocs.Add(westHyrule.lifeSouth);
            }
            itemLocs.Add(deathMountain.magicCave);
            itemLocs.Add(deathMountain.hammerCave);
            itemLocs.Add(mazeIsland.kid);
            itemLocs.Add(mazeIsland.magic);


            if (props.pbagItemShuffle)
            {
                itemLocs.Add(westHyrule.pbagCave);
                itemLocs.Add(eastHyrule.pbagCave1);
                itemLocs.Add(eastHyrule.pbagCave2);
            }

            return itemLocs;
        }

        private void shuffleSpells()
        {
            spellMap = new Dictionary<spells, spells>();
            List<int> shuffleThis = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
            for (int i = 0; i < spellGet.Count(); i++)
            {
                spellGet[i] = false;
            }
            if (props.shuffleSpellLocations)
            {
                for (int i = 0; i < shuffleThis.Count; i++)
                {

                    int s = R.Next(i, shuffleThis.Count);
                    int sl = shuffleThis[s];
                    shuffleThis[s] = shuffleThis[i];
                    shuffleThis[i] = sl;
                }
            }
            for (int i = 0; i < shuffleThis.Count; i++)
            {
                spellMap.Add((spells)i, (spells)shuffleThis[i]);
            }
            spellMap.Add(spells.upstab, spells.upstab);
            spellMap.Add(spells.downstab, spells.downstab);

            if (props.shuffleSpells)
            {
                for (int i = 0; i < 8; i++)
                {
                    bool hasSpell = r.NextDouble() > .75;
                    ROMData.put(0x17AF7 + i, hasSpell ? (Byte)1 : (Byte)0);
                    spellGet[(int)spellMap[(spells)i]] = hasSpell;
                }
            }
            else
            {
                ROMData.put(0x17AF7 + spellMap.Values.ToList().IndexOf(spells.shield), props.startShield ? (Byte)1 : (Byte)0);
                spellGet[(int)spells.shield] = props.startShield;
                ROMData.put(0x17AF7 + spellMap.Values.ToList().IndexOf(spells.jump), props.startJump ? (Byte)1 : (Byte)0);
                spellGet[(int)spells.jump] = props.startJump;
                ROMData.put(0x17AF7 + spellMap.Values.ToList().IndexOf(spells.life), props.startLife ? (Byte)1 : (Byte)0);
                spellGet[(int)spells.life] = props.startLife;
                ROMData.put(0x17AF7 + spellMap.Values.ToList().IndexOf(spells.fairy), props.startFairy ? (Byte)1 : (Byte)0);
                spellGet[(int)spells.fairy] = props.startFairy;
                ROMData.put(0x17AF7 + spellMap.Values.ToList().IndexOf(spells.fire), props.startFire ? (Byte)1 : (Byte)0);
                spellGet[(int)spells.fire] = props.startFire;
                ROMData.put(0x17AF7 + spellMap.Values.ToList().IndexOf(spells.reflect), props.startReflect ? (Byte)1 : (Byte)0);
                spellGet[(int)spells.reflect] = props.startReflect;
                ROMData.put(0x17AF7 + spellMap.Values.ToList().IndexOf(spells.spell), props.startSpell ? (Byte)1 : (Byte)0);
                spellGet[(int)spells.spell] = props.startSpell;
                ROMData.put(0x17AF7 + spellMap.Values.ToList().IndexOf(spells.thunder), props.startThunder ? (Byte)1 : (Byte)0);
                spellGet[(int)spells.thunder] = props.startThunder;
            }

            if (props.combineFire)
            {
                int newFire = R.Next(7);
                if (newFire > 3)
                {
                    newFire++;
                }
                Byte newnewFire = (Byte)(0x10 | ROMData.getByte(0xDCB + newFire));
                ROMData.put(0xDCF, newnewFire);
            }




        }

        private void shuffleExp(int start, int cap)
        {
            int[] exp = new int[8];

            for (int i = 0; i < exp.Length; i++)
            {
                exp[i] = ROMData.getByte(start + i) * 256;
                exp[i] = exp[i] + ROMData.getByte(start + 24 + i);
            }

            for (int i = 0; i < exp.Length; i++)
            {
                int nextMin = (int)(exp[i] - exp[i] * 0.25);
                int nextMax = (int)(exp[i] + exp[i] * 0.25);
                if (i == 0)
                {
                    exp[i] = r.Next(Math.Max(10, nextMin), nextMax);
                }
                else
                {
                    exp[i] = r.Next(Math.Max(exp[i - 1], nextMin), Math.Min(nextMax, 9990));
                }
            }

            for (int i = 0; i < exp.Length; i++)
            {
                exp[i] = exp[i] / 10 * 10; //wtf is this line of code? -digshake, 2020
            }

            int[] cappedExp = new int[8];
            if (props.scaleLevels)
            {
                for (int i = 0; i < exp.Length; i++)
                {
                    if (i >= cap)
                    {
                        cappedExp[i] = exp[i]; //shouldn't matter, just wanna put something here
                    }
                    else if (i == cap - 1)
                    {
                        cappedExp[i] = exp[7]; //exp to get a 1up
                    }
                    else
                    {
                        int index = (int)(8 * ((i + 1.0) / (cap - 1)));
                        cappedExp[i] = exp[(int)(6 * ((i + 1.0) / (cap - 1)))]; //cap = 3, level 4, 8, 
                    }
                }
            }
            else
            {
                cappedExp = exp;
            }    

            for (int i = 0; i < exp.Length; i++)
            {
                ROMData.put(start + i, (Byte)(cappedExp[i] / 256));
                ROMData.put(start + 24 + i, (Byte)(cappedExp[i] % 256));
            }

            for (int i = 0; i < exp.Length; i++)
            {

                ROMData.put(start + 2057 + i, intToText(cappedExp[i] / 1000));
                cappedExp[i] = cappedExp[i] - ((cappedExp[i] / 1000) * 1000);
                ROMData.put(start + 2033 + i, intToText(cappedExp[i] / 100));
                cappedExp[i] = cappedExp[i] - ((cappedExp[i] / 100) * 100);
                ROMData.put(start + 2009 + i, intToText(cappedExp[i] / 10));
            }
        }

        private void shuffleBits(List<int> addr, bool fire)
        {
            int mask = 0x10;
            int notMask = 0xEF;
            if (fire)
            {
                mask = 0x20;
                notMask = 0xDF;
            }

            double count = 0;
            foreach (int i in addr)
            {
                if ((ROMData.getByte(i) & mask) > 0)
                {
                    count++;
                }
            }

            double fraction = count / addr.Count;

            foreach (int i in addr)
            {
                int part1 = 0;
                int part2 = ROMData.getByte(i) & notMask;
                bool havethis = r.NextDouble() <= fraction;
                if (havethis)
                {
                    part1 = mask;
                }
                ROMData.put(i, (Byte)(part1 + part2));
            }
        }

        private void shuffleEnemyExp(List<int> addr)
        {
            foreach (int i in addr)
            {
                Byte exp = ROMData.getByte(i);
                int high = exp & 0xF0;
                int low = exp & 0x0F;

                if(props.expLevel.Equals("High"))
                {
                    low++;
                } else if(props.expLevel.Equals("Low")) {
                    low--;
                } else if(props.expLevel.Equals("None")) {
                    low = 0;
                }

                if (!props.expLevel.Equals("None"))
                {
                    low = r.Next(low - 2, low + 3);
                }
                if (low < 0)
                {
                    low = 0;
                }
                else if (low > 15)
                {
                    low = 15;
                }
                ROMData.put(i, (Byte)(high + low));
            }
        }

        private void shuffleEncounters(List<int> addr)
        {
            for (int i = 0; i < addr.Count; i++)
            {
                int swap = r.Next(i, addr.Count);
                Byte temp = ROMData.getByte(addr[i]);
                ROMData.put(addr[i], ROMData.getByte(addr[swap]));
                ROMData.put(addr[swap], temp);
            }
        }
        private void randomizeStartingValues()
        {

            ROMData.put(0x17AF3, (byte)props.startAtk);
            ROMData.put(0x17AF4, (byte)props.startMag);
            ROMData.put(0x17AF5, (byte)props.startLifeLvl);

            if(props.removeFlashing)
            {
                ROMData.disableFlashing();
            }

            if(props.spellEnemy)
            {
                List<int> enemies = new List<int> { 3, 4, 6, 7, 14, 16, 17, 18, 24, 25, 26 };
                ROMData.put(0x11ef, (byte)enemies[r.Next(enemies.Count())]);
            }
            int opts = 0;
            if (props.westBiome.Equals("Random (no Vanilla)"))
            {
                opts = 5;
            }
            else if (props.westBiome.Equals("Random (with Vanilla)"))
            {
                opts = 7;
            }
            if(props.bossItem)
            {
                s.shuffleBossDrop();
            }
            if(opts != 0) { 
                int wb = R.Next(opts);
                if(wb == 0)
                {
                    props.westBiome = "Vanilla-Like";
                }
                else if(wb == 1)
                {
                    props.westBiome = "Islands";
                }
                else if(wb == 2)
                {
                    props.westBiome = "Canyon";
                }
                else if (wb == 3)
                {
                    props.westBiome = "Caldera";
                }
                else if (wb == 4)
                {
                    props.westBiome = "Mountainous";
                }
                else if (wb == 5)
                {
                    props.westBiome = "Vanilla";
                }
                else if (wb == 6)
                {
                    props.westBiome = "Vanilla (shuffled)";
                }
            }

            opts = 0;
            if (props.eastBiome.Equals("Random (no Vanilla)"))
            {
                opts = 5;
            }
            else if (props.eastBiome.Equals("Random (with Vanilla)"))
            {
                opts = 7;
            }
            if (opts != 0)
            {
                int wb = R.Next(opts);

                if (wb == 0)
                {
                    props.eastBiome = "Vanilla-Like";
                }
                else if (wb == 1)
                {
                    props.eastBiome = "Islands";
                }
                else if (wb == 2)
                {
                    props.eastBiome = "Canyon";
                }
                else if (wb == 3)
                {
                    props.eastBiome = "Volcano";
                }
                else if (wb == 4)
                {
                    props.eastBiome = "Mountainous";
                }
                else if (wb == 5)
                {
                    props.eastBiome = "Vanilla";
                }
                else if (wb == 6)
                {
                    props.eastBiome = "Vanilla (shuffled)";
                }
            }

            opts = 0;
            if (props.dmBiome.Equals("Random (no Vanilla)"))
            {
                opts = 5;
            }
            else if (props.dmBiome.Equals("Random (with Vanilla)"))
            {
                opts = 7;
            }
            if (opts != 0)
            {
                int wb = R.Next(opts);
                if (wb == 0)
                {
                    props.dmBiome = "Vanilla-Like";
                }
                else if (wb == 1)
                {
                    props.dmBiome = "Islands";
                }
                else if (wb == 2)
                {
                    props.dmBiome = "Canyon";
                }
                else if (wb == 3)
                {
                    props.dmBiome = "Caldera";
                }
                else if (wb == 4)
                {
                    props.dmBiome = "Mountainous";
                }
                else if (wb == 5)
                {
                    props.dmBiome = "Vanilla";
                }
                else if (wb == 6)
                {
                    props.dmBiome = "Vanilla (shuffled)";
                }
            }

            if(props.mazeBiome.Equals("Random (with Vanilla)"))
            {
                int wb = r.Next(3);
                if(wb == 0)
                {
                    props.mazeBiome = "Vanilla";
                }
                else if(wb == 1)
                {
                    props.mazeBiome = "Vanilla (shuffled)";
                }
                else if(wb == 2)
                {
                    props.mazeBiome = "Vanilla-Like";
                }
            }

            if (props.westBiome.Equals("Canyon"))
            {
                if(R.NextDouble() > 0.5)
                {
                    props.westBiome = "CanyonD";
                }
            }

            if (props.eastBiome.Equals("Canyon"))
            {
                if (R.NextDouble() > 0.5)
                {
                    props.eastBiome = "CanyonD";
                }
            }

            if (props.dmBiome.Equals("Canyon"))
            {
                if (R.NextDouble() > 0.5)
                {
                    props.dmBiome = "CanyonD";
                }
            }

            if (props.removeSpellItems)
            {
                ROMData.put(0xF584, 0xA9);
                ROMData.put(0xF585, 0x01);
                ROMData.put(0xF586, 0xEA);
            }
            ROMData.updateSprites(props.charSprite);

            Dictionary<String, int> colorMap = new Dictionary<String, int> { { "Green", 0x2A }, { "Dark Green", 0x0A }, { "Aqua", 0x3C }, { "Dark Blue", 0x02 }, { "Purple", 0x04 }, { "Pink", 0x24 }, { "Red", 0x16 }, { "Orange", 0x27 }, { "Turd", 0x18 } };

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
            int c2 = 0;
            int c1 = 0;

            if(props.tunicColor.Equals("Default"))
            {
                if(props.charSprite.Equals("Link"))
                {
                    c2 = colorMap["Green"];
                }
                else if(props.charSprite.Equals("Iron Knuckle"))
                {
                    c2 = colorMap["Dark Blue"];
                }
                else if(props.charSprite.Equals("Error"))
                {
                    c2 = 0x13;
                }
                else if(props.charSprite.Equals("Samus"))
                {
                    c2 = 0x27;
                }
                else if(props.charSprite.Equals("Simon"))
                {
                    c2 = 0x27;
                }
                else if(props.charSprite.Equals("Stalfos"))
                {
                    c2 = colorMap["Red"];
                }
                else if(props.charSprite.Equals("Vase Lady"))
                {
                    c2 = 0x13;
                }
                else if(props.charSprite.Equals("Ruto"))
                {
                    c2 = 0x30;
                }
                else if(props.charSprite.Equals("Yoshi"))
                {
                    c2 = 0x2a;
                }
                else if(props.charSprite.Equals("Dragonlord"))
                {
                    c2 = 0x01;
                }
                else if(props.charSprite.Equals("Miria"))
                {
                    c2 = 0x16;
                }
                else if(props.charSprite.Equals("Crystalis"))
                {
                    c2 = 0x14;
                }
                else if(props.charSprite.Equals("Taco"))
                {
                    c2 = 0x2a;
                }
                else if(props.charSprite.Equals("Pyramid"))
                {
                    c2 = 0x32;
                }
                else if (props.charSprite.Equals("Faxanadu"))
                {
                    c2 = 0x2a;
                }
                else if (props.charSprite.Equals("Lady Link"))
                {
                    c2 = 0x2a;
                }
                else if (props.charSprite.Equals("Hoodie Link"))
                {
                    c2 = 0x2a;
                }
                else if(props.charSprite.Equals("GliitchWiitch"))
                {
                    c2 = 0x0c;
                }
            }
            else if (!props.tunicColor.Equals("Random"))
            {
                c2 = colorMap[props.tunicColor];
            }

            if(props.shieldColor.Equals("Default"))
            {
                if (props.charSprite.Equals("Link"))
                {
                    c1 = colorMap["Red"];
                }
                else if (props.charSprite.Equals("Iron Knuckle"))
                {
                    c1 = colorMap["Red"];
                }
                else if (props.charSprite.Equals("Error"))
                {
                    c1 = colorMap["Red"];
                }
                else if(props.charSprite.Equals("Samus"))
                {
                    c1 = 0x37;
                }
                else if(props.charSprite.Equals("Simon"))
                {
                    c1 = 0x16;
                }
                else if(props.charSprite.Equals("Stalfos"))
                {
                    c1 = colorMap["Dark Blue"];
                }
                else if(props.charSprite.Equals("Vase Lady"))
                {
                    c1 = colorMap["Red"];
                }
                else if(props.charSprite.Equals("Ruto"))
                {
                    c1 = 0x3c;
                }
                else if(props.charSprite.Equals("Yoshi"))
                {
                    c1 = 0x0F;
                }
                else if(props.charSprite.Equals("Dragonlord"))
                {
                    c1 = 0x03;
                }
                else if(props.charSprite.Equals("Miria"))
                {
                    c1 = 0x15;
                }
                else if (props.charSprite.Equals("Crystalis"))
                {
                    c1 = 0x1B;
                }
                else if (props.charSprite.Equals("Taco"))
                {
                    c1 = 0x16;
                }
                else if(props.charSprite.Equals("Pyramid"))
                {
                    c1 = 0x02;
                }
                else if (props.charSprite.Equals("Lady Link"))
                {
                    c1 = 0x16;
                }
                else if (props.charSprite.Equals("Hoodie Link"))
                {
                    c1 = 0x16;
                }
                else if(props.charSprite.Equals("GliitchWiitch"))
                {
                    c1 = 0x25;
                }

            }
            else if (!props.shieldColor.Equals("Random"))
            {
                c1 = colorMap[props.shieldColor];
            }
            if (props.tunicColor.Equals("Random"))
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
            }

            if (props.shieldColor.Equals("Random"))
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
            }

            if(props.encounterRate.Equals("None"))
            {
                ROMData.put(0x294, 0x60); //skips the whole routine
            }

            if(props.encounterRate.Equals("50%"))
            {
                //terrain timers
                ROMData.put(0x250, 0x40);
                ROMData.put(0x251, 0x30);
                ROMData.put(0x252, 0x30);
                ROMData.put(0x253, 0x40);
                ROMData.put(0x254, 0x12);
                ROMData.put(0x255, 0x06);

                //initial overworld timer
                ROMData.put(0x88A, 0x10);

                /*
                 * insert jump to a8aa at 2a3 (4c AA A8)
                 * 
                 * At a8aa
                 * Load $26 (A5 26)
                 * bne to end (2 bytes) (D0 0D)
                 * inc new step counter (where?) EE E0 06
                 * Load 1 to accumulator (A9 01)
                 * xor new step counter with 1 (2D E0 06)
                 * bne to end (D0 03)
                 * jump to encounter spawn 8298 (4C 98 82)
                 * jump to rts 829f (4C 93 82)
                 */
                ROMData.put(0x29f, new byte[] { 0x4C, 0xAA, 0xA8 });

                ROMData.put(0x28ba, new byte[] { 0xA5, 0x26, 0xD0, 0x0D, 0xEE, 0xE0, 0x06, 0xA9, 0x01, 0x2D, 0xE0, 0x06, 0xD0, 0x03, 0x4C, 0x98, 0x82, 0x4C, 0x93, 0x82 });
            }

            int[] tunicLocs = { 0x285C, 0x40b1, 0x40c1, 0x40d1, 0x80e1, 0x80b1, 0x80c1, 0x80d1, 0x80e1, 0xc0b1, 0xc0c1, 0xc0d1, 0xc0e1, 0x100b1, 0x100c1, 0x100d1, 0x100e1, 0x140b1, 0x140c1, 0x140d1, 0x140e1, 0x17c1b, 0x1c466, 0x1c47e };

            foreach (int l in tunicLocs)
            {
                ROMData.put(0x10ea, (byte)c2);
                if (props.charSprite.Equals("Iron Knuckle"))
                {
                    ROMData.put(0x10ea, (byte)0x30);
                    ROMData.put(0x2a0a, 0x0D);
                    ROMData.put(0x2a10, (byte)c2);
                    ROMData.put(l, 0x20);
                    ROMData.put(l - 1, (byte)c2);
                    ROMData.put(l - 2, 0x0D);
                }
                else if(props.charSprite.Equals("Samus"))
                {
                    ROMData.put(0x2a0a, 0x16);
                    ROMData.put(0x2a10, 0x1a);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x1a);
                    ROMData.put(l - 2, 0x16);
                }
                else if (props.charSprite.Equals("Error") || props.charSprite.Equals("Vase Lady"))
                {
                    ROMData.put(0x2a0a, 0x0F);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 2, 0x0F);
                }
                else if(props.charSprite.Equals("Simon"))
                {
                    ROMData.put(0x2a0a, 0x07);
                    ROMData.put(0x2a10, 0x37);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x37);
                    ROMData.put(l - 2, 0x07);
                }
                else if(props.charSprite.Equals("Stalfos"))
                {
                    ROMData.put(0x2a0a, 0x08);
                    ROMData.put(0x2a10, 0x20);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x20);
                    ROMData.put(l - 2, 0x08);
                }
                else if(props.charSprite.Equals("Ruto"))
                {
                    ROMData.put(0x2a0a, 0x0c);
                    ROMData.put(0x2a10, 0x1c);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x1c);
                    ROMData.put(l - 2, 0x0c);
                }
                else if(props.charSprite.Equals("Yoshi"))
                {
                    ROMData.put(0x2a0a, 0x16);
                    ROMData.put(0x2a10, 0x20);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x20);
                    ROMData.put(l - 2, 0x16);
                }
                else if(props.charSprite.Equals("Dragonlord"))
                {
                    ROMData.put(0x2a0a, 0x28);
                    ROMData.put(0x2a10, 0x11);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x11);
                    ROMData.put(l - 2, 0x28);
                }
                else if (props.charSprite.Equals("Miria"))
                {
                    ROMData.put(0x2a0a, 0x0D);
                    ROMData.put(0x2a10, 0x30);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x30);
                    ROMData.put(l - 2, 0x0D);
                    
                }
                else if (props.charSprite.Equals("Crystalis"))
                {
                    ROMData.put(0x2a0a, 0x0D);
                    ROMData.put(0x2a10, 0x36);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x36);
                    ROMData.put(l - 2, 0x0D);

                }
                else if (props.charSprite.Equals("Taco"))
                {
                    ROMData.put(0x2a0a, 0x18);
                    ROMData.put(0x2a10, 0x36);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x36);
                    ROMData.put(l - 2, 0x18);

                }
                else if (props.charSprite.Equals("Pyramid"))
                {
                    ROMData.put(0x2a0a, 0x12);
                    ROMData.put(0x2a10, 0x22);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x22);
                    ROMData.put(l - 2, 0x12);

                }
                else if (props.charSprite.Equals("Faxanadu"))
                {
                    ROMData.put(0x2a0a, 0x18);
                    ROMData.put(0x2a10, 0x36);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x36);
                    ROMData.put(l - 2, 0x18);

                }
                else if (props.charSprite.Equals("Lady Link"))
                {
                    ROMData.put(0x2a0a, 0x18);
                    ROMData.put(0x2a10, 0x36);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x36);
                    ROMData.put(l - 2, 0x18);

                }
                else if (props.charSprite.Equals("Hoodie Link"))
                {
                    ROMData.put(0x2a0a, 0x18);
                    ROMData.put(0x2a10, 0x36);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x36);
                    ROMData.put(l - 2, 0x18);

                }
                else if (props.charSprite.Equals("GliitchWiitch"))
                {
                    ROMData.put(0x2a0a, 0x08);
                    ROMData.put(0x2a10, 0x36);
                    ROMData.put(l, (byte)c2);
                    ROMData.put(l - 1, 0x36);
                    ROMData.put(l - 2, 0x08);

                }
                else
                {
                    ROMData.put(0x10ea, (byte)c2);
                    ROMData.put(l, (byte)c2);
                }
            }

            ROMData.put(0xe9e, (byte)c1);



            int beamType = -1;
            if (props.beamSprite.Equals("Random"))
            {

                Random r2 = new Random();
                beamType = r2.Next(6);
            }
            else if (props.beamSprite.Equals("Fire"))
            {
                beamType = 0;
            }
            else if (props.beamSprite.Equals("Bubble"))
            {
                beamType = 1;
            }
            else if (props.beamSprite.Equals("Rock"))
            {
                beamType = 2;
            }
            else if (props.beamSprite.Equals("Axe"))
            {
                beamType = 3;
            }
            else if (props.beamSprite.Equals("Hammer"))
            {
                beamType = 4;
            }
            else if (props.beamSprite.Equals("Wizzrobe Beam"))
            {
                beamType = 5;
            }
            Byte[] newSprite = new Byte[32];

            if (beamType == 0 || beamType == 3 || beamType == 4)
            {
                ROMData.put(0x18f5, 0xa9);
                ROMData.put(0x18f6, 0x00);
                ROMData.put(0x18f7, 0xea);
            }
            else if(beamType != -1)
            {
                ROMData.put(0X18FB, 0x84);
            }

            if (beamType == 1)//bubbles
            {
                for (int i = 0; i < 32; i++)
                {
                    Byte next = ROMData.getByte(0x20ab0 + i);
                    newSprite[i] = next;
                }
            }

            if (beamType == 2)//rocks
            {
                for (int i = 0; i < 32; i++)
                {
                    Byte next = ROMData.getByte(0x22af0 + i);
                    newSprite[i] = next;
                }
            }

            if (beamType == 3)//axes
            {
                for (int i = 0; i < 32; i++)
                {
                    Byte next = ROMData.getByte(0x22fb0 + i);
                    newSprite[i] = next;
                }
            }

            if (beamType == 4)//hammers
            {
                for (int i = 0; i < 32; i++)
                {
                    Byte next = ROMData.getByte(0x32ef0 + i);
                    newSprite[i] = next;
                }
            }

            if (beamType == 5)//wizzrobe beam
            {
                for (int i = 0; i < 32; i++)
                {
                    Byte next = ROMData.getByte(0x34dd0 + i);
                    newSprite[i] = next;
                }
            }


            if (beamType != 0 && beamType != -1)
            {
                foreach (int loc in fireLocs)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        ROMData.put(loc + i, newSprite[i]);
                    }
                }
            }


            if (props.disableBeep)
            {
                ROMData.put(0x1D4E4, (Byte)0xEA);
                ROMData.put(0x1D4E5, (Byte)0x38);
            }
            if (props.shuffleLifeRefill)
            {
                int lifeRefill = r.Next(1, 6);
                ROMData.put(0xE7A, (Byte)(lifeRefill * 16));
            }

            if (props.shuffleStealExpAmt)
            {
                int small = ROMData.getByte(0x1E30E);
                int big = ROMData.getByte(0x1E314);
                small = r.Next((int)(small - small * .5), (int)(small + small * .5) + 1);
                big = r.Next((int)(big - big * .5), (int)(big + big * .5) + 1);
                ROMData.put(0x1E30E, (Byte)small);
                ROMData.put(0x1E314, (Byte)big);
            }

            List<int> addr = new List<int>();
            for (int i = 0x54E8; i < 0x54ED; i++)
            {
                addr.Add(i);
            }
            for (int i = 0x54EF; i < 0x54F8; i++)
            {
                addr.Add(i);
            }
            for (int i = 0x54F9; i < 0x5508; i++)
            {
                addr.Add(i);
            }

            if (props.shuffleEnemyStealExp)
            {
                shuffleBits(addr, false);
            }

            if (props.shuffleSwordImmunity)
            {
                shuffleBits(addr, true);
            }

            if (!props.expLevel.Equals("Normal"))
            {
                shuffleEnemyExp(addr);
            }
            addr = new List<int>();
            for (int i = 0x94E8; i < 0x94ED; i++)
            {
                addr.Add(i);
            }
            for (int i = 0x94EF; i < 0x94F8; i++)
            {
                addr.Add(i);
            }
            for (int i = 0x94F9; i < 0x9502; i++)
            {
                addr.Add(i);
            }
            if (props.shuffleEnemyStealExp)
            {
                shuffleBits(addr, false);
            }

            if (props.shuffleSwordImmunity)
            {
                shuffleBits(addr, true);
            }
            if (!props.expLevel.Equals("Normal"))
            {
                shuffleEnemyExp(addr);
            }

            addr = new List<int>();
            for (int i = 0x114E8; i < 0x114EA; i++)
            {
                addr.Add(i);
            }
            for (int i = 0x114EB; i < 0x114ED; i++)
            {
                addr.Add(i);
            }
            for (int i = 0x114EF; i < 0x114F8; i++)
            {
                addr.Add(i);
            }
            for (int i = 0x114FD; i < 0x11505; i++)
            {
                addr.Add(i);
            }
            addr.Add(0x11508);

            if (props.shuffleEnemyStealExp)
            {
                shuffleBits(addr, false);
            }

            if (props.shuffleSwordImmunity)
            {
                shuffleBits(addr, true);
            }
            if (!props.expLevel.Equals("Normal"))
            {
                shuffleEnemyExp(addr);
            }

            addr = new List<int>();
            for (int i = 0x129E8; i < 0x129EA; i++)
            {
                addr.Add(i);
            }

            for (int i = 0x129EB; i < 0x129ED; i++)
            {
                addr.Add(i);
            }

            for (int i = 0x129EF; i < 0x129F4; i++)
            {
                addr.Add(i);
            }

            for (int i = 0x129F5; i < 0x129F7; i++)
            {
                addr.Add(i);
            }

            for (int i = 0x129FD; i < 0x12A05; i++)
            {
                addr.Add(i);
            }

            addr.Add(0x12A08);

            if (props.shuffleEnemyStealExp)
            {
                shuffleBits(addr, false);
            }

            if (props.shuffleSwordImmunity)
            {
                shuffleBits(addr, true);
            }
            if (!props.expLevel.Equals("Normal"))
            {
                shuffleEnemyExp(addr);
            }

            addr = new List<int>();
            for (int i = 0x154E9; i < 0x154ED; i++)
            {
                addr.Add(i);
            }

            for (int i = 0x154F2; i < 0x154F8; i++)
            {
                addr.Add(i);
            }

            for (int i = 0x154F9; i < 0x15500; i++)
            {
                addr.Add(i);
            }

            for (int i = 0x15502; i < 15504; i++)
            {
                addr.Add(i);
            }

            if (props.shuffleEnemyStealExp)
            {
                shuffleBits(addr, false);
            }

            if (props.shuffleSwordImmunity)
            {
                shuffleBits(addr, true);
            }
            if (!props.expLevel.Equals("Normal"))
            {
                shuffleEnemyExp(addr);
            }

            if (!props.expLevel.Equals("Normal"))
            {
                addr = new List<int>();
                addr.Add(0x11505);
                addr.Add(0x13C88);
                addr.Add(0x13C89);
                addr.Add(0x12A05);
                addr.Add(0x12A06);
                addr.Add(0x12A07);
                addr.Add(0x15507);
                shuffleEnemyExp(addr);
            }

            if (props.shuffleEncounters)
            {
                addr = new List<int>();
                addr.Add(0x441b);
                addr.Add(0x4419);
                addr.Add(0x441D);
                addr.Add(0x4420);
                addr.Add(0x441C);
                addr.Add(0x441A);
                addr.Add(0x4422);
                addr.Add(0x441E);

                if (props.allowPathEnemies)
                {
                    addr.Add(0x4424);
                    addr.Add(0x4423);
                }

                shuffleEncounters(addr);

                addr = new List<int>();
                addr.Add(0x841B);
                addr.Add(0x8419);
                addr.Add(0x841D);
                addr.Add(0x8422);
                addr.Add(0x8420);
                addr.Add(0x841A);
                addr.Add(0x841E);
                addr.Add(0x8426);

                if (props.allowPathEnemies)
                {
                    addr.Add(0x8423);
                    addr.Add(0x8424);
                }

                shuffleEncounters(addr);
            }

            if (props.jumpAlwaysOn)
            {
                ROMData.put(0x1482, ROMData.getByte(0x1480));
                ROMData.put(0x1483, ROMData.getByte(0x1481));
                ROMData.put(0x1486, ROMData.getByte(0x1484));
                ROMData.put(0x1487, ROMData.getByte(0x1485));

            }

            if (props.disableMagicRecs)
            {
                ROMData.put(0xF539, (Byte)0xC9);
                ROMData.put(0xF53A, (Byte)0);
            }

            if (props.shuffleAllExp)
            {
                shuffleExp(0x1669, props.attackCap);//atk
                shuffleExp(0x1671, props.magicCap);//mag
                shuffleExp(0x1679, props.lifeCap);//life
            }
            else
            {
                if (props.shuffleAtkExp)
                {
                    shuffleExp(0x1669, props.attackCap);
                }

                if (props.shuffleMagicExp)
                {
                    shuffleExp(0x1671, props.magicCap);
                }

                if (props.shuffleLifeExp)
                {
                    shuffleExp(0x1679, props.lifeCap);
                }
            }

            ROMData.setLevelCap(props.attackCap, props.magicCap, props.lifeCap);

            shuffleAttackEffectiveness(false);

            shuffleLifeEffectiveness(true);

            shuffleLifeEffectiveness(false);

            if (props.startGems.Equals("Random"))
            {
                ROMData.put(0x17B10, (Byte)r.Next(0, 7));
            }
            else
            {
                ROMData.put(0x17B10, (Byte)Int32.Parse(props.startGems));
            }

            if (props.startHearts.Equals("Random"))
            {
                startHearts = r.Next(1, 9);
                ROMData.put(0x17B00, (Byte)startHearts);
            }
            else
            {
                startHearts = Int32.Parse(props.startHearts);
                ROMData.put(0x17B00, (Byte)startHearts);
            }

            if (props.maxHearts.Equals("Random"))
            {
                maxHearts = r.Next(startHearts, 9);
            }
            else
            {
                maxHearts = Int32.Parse(props.maxHearts);
            }

            numHContainers = maxHearts - startHearts;

            if (props.shuffleLives)
            {
                ROMData.put(0x1C369, (Byte)r.Next(2, 6));
            }

            if (props.startTech.Equals("Random"))
            {
                int swap = r.Next(7);
                if (swap <= 3)
                {
                    ROMData.put(0x17B12, (Byte)0);
                }
                else if (swap == 4)
                {
                    ROMData.put(0x17B12, (Byte)0x10);
                }
                else if (swap == 5)
                {
                    ROMData.put(0x17B12, (Byte)0x04);
                }
                else
                {
                    ROMData.put(0x17B12, (Byte)0x14);
                }
            }
            else if (props.startTech.Equals("Downstab"))
            {
                ROMData.put(0x17B12, (Byte)0x10);
            }
            else if (props.startTech.Equals("Upstab"))
            {
                ROMData.put(0x17B12, (Byte)0x04);
            }
            else if (props.startTech.Equals("Both"))
            {
                ROMData.put(0x17B12, (Byte)0x14);
            }
            else
            {
                ROMData.put(0x17B12, (Byte)0x00);
            }

            if (props.tankMode)
            {
                for (int i = 0x1E2BF; i < 0x1E2BF + 56; i++)
                {
                    ROMData.put(i, 0);
                }
            }

            if (props.ohkoLink)
            {
                for (int i = 0x1E2BF; i < 0x1E2BF + 56; i++)
                {
                    ROMData.put(i, 0xFF);
                }
            }

            if (props.wizardMode)
            {
                for (int i = 0xD8B; i < 0xD8b + 64; i++)
                {
                    ROMData.put(i, 0);
                }
            }

            if (props.palacePalette)
            {

                s.shufflePalacePalettes();

            }

            if (props.pbagDrop)
            {
                int drop = r.Next(5) + 4;
                ROMData.put(0x1E8B0, (Byte)drop);
            }

        }

        private Byte intToText(int x)
        {
            switch (x)
            {
                case 0:
                    return (Byte)0xD0;
                case 1:
                    return (Byte)0xD1;
                case 2:
                    return (Byte)0xD2;
                case 3:
                    return (Byte)0xD3;
                case 4:
                    return (Byte)0xD4;
                case 5:
                    return (Byte)0xD5;
                case 6:
                    return (Byte)0xD6;
                case 7:
                    return (Byte)0xD7;
                case 8:
                    return (Byte)0xD8;
                default:
                    return (Byte)0xD9;
            }
        }

        



        private void updateRom()
        {
            foreach (World w in worlds)
            {
                List<Location> locs = w.AllLocations;
                foreach (Location l in locs)
                {
                    l.updateBytes();
                    ROMData.put(l.MemAddress, l.LocationBytes[0]);
                    ROMData.put(l.MemAddress + overworldXOff, l.LocationBytes[1]);
                    ROMData.put(l.MemAddress + overworldMapOff, l.LocationBytes[2]);
                    ROMData.put(l.MemAddress + overworldWorldOff, l.LocationBytes[3]);
                }
                w.removeUnusedConnectors();
            }


            Location medicineLoc = null;
            Location trophyLoc = null;
            Location kidLoc = null;
            foreach (Location l in itemLocs)
            {
                if (l.item == items.medicine)
                {
                    medicineLoc = l;
                }
                if (l.item == items.trophy)
                {
                    trophyLoc = l;
                }
                if (l.item == items.kid)
                {
                    kidLoc = l;
                }
            }

            Byte[] medSprite = new Byte[32];
            Byte[] trophySprite = new Byte[32];
            Byte[] kidSprite = new Byte[32];

            for (int i = 0; i < 32; i++)
            {
                medSprite[i] = ROMData.getByte(0x23310 + i);
                trophySprite[i] = ROMData.getByte(0x232f0 + i);
                kidSprite[i] = ROMData.getByte(0x25310 + i);
            }
            bool medEast = (eastHyrule.AllLocations.Contains(medicineLoc) || mazeIsland.AllLocations.Contains(medicineLoc));
            bool trophyEast = (eastHyrule.AllLocations.Contains(trophyLoc) || mazeIsland.AllLocations.Contains(trophyLoc));
            bool kidWest = (westHyrule.AllLocations.Contains(kidLoc) || deathMountain.AllLocations.Contains(kidLoc));
            Dictionary<int, int> palaceMems = new Dictionary<int, int>();
            palaceMems.Add(1, 0x29AD0);
            palaceMems.Add(2, 0x2BAD0);
            palaceMems.Add(3, 0x33AD0);
            palaceMems.Add(4, 0x35AD0);
            palaceMems.Add(5, 0x37AD0);
            palaceMems.Add(6, 0x39AD0);

            if (medEast && eastHyrule.palace5.item != items.medicine && eastHyrule.palace6.item != items.medicine && mazeIsland.palace4.item != items.medicine)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(0x25430 + i, medSprite[i]);
                }
                ROMData.put(0x1eeb9, 0x43);
                ROMData.put(0x1eeba, 0x43);
            }

            if (trophyEast)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(0x25410 + i, trophySprite[i]);
                }
                ROMData.put(0x1eeb7, 0x41);
                ROMData.put(0x1eeb8, 0x41);
            }

            if (kidWest && westHyrule.palace1.item != items.kid && westHyrule.palace2.item != items.kid && westHyrule.palace3.item != items.kid)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(0x23570 + i, kidSprite[i]);
                }
                ROMData.put(0x1eeb5, 0x57);
                ROMData.put(0x1eeb6, 0x57);
            }

            if (eastHyrule.newKasuto.item == items.trophy || eastHyrule.newKasuto2.item == items.trophy || westHyrule.lifeNorth.item == items.trophy || westHyrule.lifeSouth.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(0x27210 + i, trophySprite[i]);
                }
                ROMData.put(0x1eeb7, 0x21);
                ROMData.put(0x1eeb8, 0x21);
            }

            if (eastHyrule.newKasuto.item == items.medicine || eastHyrule.newKasuto2.item == items.medicine || westHyrule.lifeNorth.item == items.trophy || westHyrule.lifeSouth.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(0x27230 + i, medSprite[i]);
                }
                ROMData.put(0x1eeb9, 0x23);
                ROMData.put(0x1eeba, 0x23);
            }

            if (eastHyrule.newKasuto.item == items.kid || eastHyrule.newKasuto2.item == items.kid || westHyrule.lifeNorth.item == items.trophy || westHyrule.lifeSouth.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(0x27250 + i, kidSprite[i]);
                }
                ROMData.put(0x1eeb5, 0x25);
                ROMData.put(0x1eeb6, 0x25);
            }

            if (westHyrule.palace1.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[westHyrule.palace1.PalNum] + i, trophySprite[i]);
                }
                ROMData.put(0x1eeb7, 0xAD);
                ROMData.put(0x1eeb8, 0xAD);
            }
            if (westHyrule.palace2.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[westHyrule.palace2.PalNum] + i, trophySprite[i]);
                }
                ROMData.put(0x1eeb7, 0xAD);
                ROMData.put(0x1eeb8, 0xAD);
            }
            if (westHyrule.palace3.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[westHyrule.palace3.PalNum] + i, trophySprite[i]);
                }
                ROMData.put(0x1eeb7, 0xAD);
                ROMData.put(0x1eeb8, 0xAD);
            }
            if (mazeIsland.palace4.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[mazeIsland.palace4.PalNum] + i, trophySprite[i]);
                }
                ROMData.put(0x1eeb7, 0xAD);
                ROMData.put(0x1eeb8, 0xAD);
            }
            if (eastHyrule.palace5.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[eastHyrule.palace5.PalNum] + i, trophySprite[i]);
                }
                ROMData.put(0x1eeb7, 0xAD);
                ROMData.put(0x1eeb8, 0xAD);
            }
            if (eastHyrule.palace6.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[eastHyrule.palace6.PalNum] + i, trophySprite[i]);
                }
                ROMData.put(0x1eeb7, 0xAD);
                ROMData.put(0x1eeb8, 0xAD);
            }
            if (eastHyrule.gp.item == items.trophy)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[eastHyrule.gp.PalNum] + i, trophySprite[i]);
                }
                ROMData.put(0x1eeb7, 0xAD);
                ROMData.put(0x1eeb8, 0xAD);
            }

            if (westHyrule.palace1.item == items.medicine)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[westHyrule.palace1.PalNum] + i, medSprite[i]);
                }
                ROMData.put(0x1eeb9, 0xAD);
                ROMData.put(0x1eeba, 0xAD);
            }
            if (westHyrule.palace2.item == items.medicine)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[westHyrule.palace2.PalNum] + i, medSprite[i]);
                }
                ROMData.put(0x1eeb9, 0xAD);
                ROMData.put(0x1eeba, 0xAD);
            }
            if (westHyrule.palace3.item == items.medicine)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[westHyrule.palace3.PalNum] + i, medSprite[i]);
                }
                ROMData.put(0x1eeb9, 0xAD);
                ROMData.put(0x1eeba, 0xAD);
            }
            if (mazeIsland.palace4.item == items.medicine)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[mazeIsland.palace4.PalNum] + i, medSprite[i]);
                }
                ROMData.put(0x1eeb9, 0xAD);
                ROMData.put(0x1eeba, 0xAD);
            }
            if (eastHyrule.palace5.item == items.medicine)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[eastHyrule.palace5.PalNum] + i, medSprite[i]);
                }
                ROMData.put(0x1eeb9, 0xAD);
                ROMData.put(0x1eeba, 0xAD);
            }
            if (eastHyrule.palace6.item == items.medicine)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[eastHyrule.palace6.PalNum] + i, medSprite[i]);
                }
                ROMData.put(0x1eeb9, 0xAD);
                ROMData.put(0x1eeba, 0xAD);
            }
            if (eastHyrule.gp.item == items.medicine)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[eastHyrule.gp.PalNum] + i, medSprite[i]);
                }
                ROMData.put(0x1eeb9, 0xAD);
                ROMData.put(0x1eeba, 0xAD);
            }

            if (westHyrule.palace1.item == items.kid)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[westHyrule.palace1.PalNum] + i, kidSprite[i]);
                }
                ROMData.put(0x1eeb5, 0xAD);
                ROMData.put(0x1eeb6, 0xAD);
            }
            if (westHyrule.palace2.item == items.kid)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[westHyrule.palace2.PalNum] + i, kidSprite[i]);
                }
                ROMData.put(0x1eeb5, 0xAD);
                ROMData.put(0x1eeb6, 0xAD);
            }
            if (westHyrule.palace3.item == items.kid)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[westHyrule.palace3.PalNum] + i, kidSprite[i]);
                }
                ROMData.put(0x1eeb5, 0xAD);
                ROMData.put(0x1eeb6, 0xAD);
            }
            if (mazeIsland.palace4.item == items.kid)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[mazeIsland.palace4.PalNum] + i, kidSprite[i]);
                }
                ROMData.put(0x1eeb5, 0xAD);
                ROMData.put(0x1eeb6, 0xAD);
            }
            if (eastHyrule.palace5.item == items.kid)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[eastHyrule.palace5.PalNum] + i, kidSprite[i]);
                }
                ROMData.put(0x1eeb5, 0xAD);
                ROMData.put(0x1eeb6, 0xAD);
            }
            if (eastHyrule.palace6.item == items.kid)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[eastHyrule.palace6.PalNum] + i, kidSprite[i]);
                }
                ROMData.put(0x1eeb5, 0xAD);
                ROMData.put(0x1eeb6, 0xAD);
            }
            if (eastHyrule.gp.item == items.kid)
            {
                for (int i = 0; i < 32; i++)
                {
                    ROMData.put(palaceMems[eastHyrule.gp.PalNum] + i, kidSprite[i]);
                }
                ROMData.put(0x1eeb5, 0xAD);
                ROMData.put(0x1eeb6, 0xAD);
            }

            romData.addCredits();
            //fixes improper exit from p6/new kasuto
            //if (eastHyrule.palace6.PalNum != 7)
            //{
            //    ROMData.put(0x1ccd3, 0xea);
            //    ROMData.put(0x1ccd4, 0xea);
            //    ROMData.put(0x1ccd5, 0xea);
            //    ROMData.put(0x1ccd6, 0xc0);
            //    ROMData.put(0x1ccd7, 0x31);
            //}


            ROMData.put(0x1CD3A, (Byte)palGraphics[westHyrule.palace1.PalNum]);


            ROMData.put(0x1CD3B, (Byte)palGraphics[westHyrule.palace2.PalNum]);


            ROMData.put(0x1CD3C, (Byte)palGraphics[westHyrule.palace3.PalNum]);


            ROMData.put(0x1CD46, (Byte)palGraphics[mazeIsland.palace4.PalNum]);


            ROMData.put(0x1CD42, (Byte)palGraphics[eastHyrule.palace5.PalNum]);

            ROMData.put(0x1CD43, (Byte)palGraphics[eastHyrule.palace6.PalNum]);
            ROMData.put(0x1CD44, (Byte)palGraphics[eastHyrule.gp.PalNum]);

            //if (!props.palacePalette)
            //{

            ROMData.put(0x1FFF4, (Byte)palPalettes[westHyrule.palace1.PalNum]);

            ROMData.put(0x1FFF5, (Byte)palPalettes[westHyrule.palace2.PalNum]);

            ROMData.put(0x1FFF6, (Byte)palPalettes[westHyrule.palace3.PalNum]);

            ROMData.put(0x20000, (Byte)palPalettes[mazeIsland.palace4.PalNum]);

            ROMData.put(0x1FFFC, (Byte)palPalettes[eastHyrule.palace5.PalNum]);

            ROMData.put(0x1FFFD, (Byte)palPalettes[eastHyrule.palace6.PalNum]);

            ROMData.put(0x1FFFE, (Byte)palPalettes[eastHyrule.gp.PalNum]);

            //}

            if (props.shuffleDripper)
            {
                ROMData.put(0x11927, (Byte)enemies1[R.Next(enemies1.Count)]);
            }

            if (props.shuffleEnemyPalettes)
            {
                List<int> doubleLocs = new List<int> { 0x40b4, 0x80b4, 0x100b4, 0x100b8, 0x100bc, 0x140b4, 0x140b8, 0x140bc };
                List<int> singleLocs = new List<int> { 0x40b8, 0x40bc, 0x80b8, 0x80bc };

                foreach (int i in doubleLocs)
                {
                    int low = r.Next(12) + 1;
                    int high = (r.Next(2) + 1) * 16;
                    int color = high + low;
                    ROMData.put(i, (byte)color);
                    ROMData.put(i + 16, (byte)color);
                    ROMData.put(i - 1, (byte)(color - 15));
                    ROMData.put(i + 16 - 1, (byte)(color - 15));
                }
                foreach (int i in singleLocs)
                {
                    int low = r.Next(13);
                    int high = (r.Next(3)) * 16;
                    int color = high + low;
                    ROMData.put(i, (byte)color);
                    ROMData.put(i + 16, (byte)color);
                    ROMData.put(i + 16 - 1, (byte)(color - 15));
                }

                for (int i = 0x54e8; i < 0x5508; i++)
                {
                    if (i != 0x54f8)
                    {
                        int b = ROMData.getByte(i);
                        int p = b & 0x3F;
                        int n = r.Next(4);
                        n = n << 6;
                        ROMData.put(i, (byte)(n + p));
                    }
                }

                for (int i = 0x94e8; i < 0x9508; i++)
                {
                    if (i != 0x94f8)
                    {
                        int b = ROMData.getByte(i);
                        int p = b & 0x3F;
                        int n = r.Next(4);
                        n = n << 6;
                        ROMData.put(i, (byte)(n + p));
                    }
                }
                for (int i = 0x114e8; i < 0x11508; i++)
                {
                    if (i != 0x114f8)
                    {
                        int b = ROMData.getByte(i);
                        int p = b & 0x3F;
                        int n = r.Next(4);
                        n = n << 6;
                        ROMData.put(i, (byte)(n + p));
                    }
                }
                for (int i = 0x129e8; i < 0x12a09; i++)
                {
                    if (i != 0x129f8)
                    {
                        int b = ROMData.getByte(i);
                        int p = b & 0x3F;
                        int n = r.Next(4);
                        n = n << 6;
                        ROMData.put(i, (byte)(n + p));
                    }
                }
                for (int i = 0x154e8; i < 0x15508; i++)
                {
                    if (i != 0x154f8)
                    {
                        int b = ROMData.getByte(i);
                        int p = b & 0x3F;
                        int n = r.Next(4);
                        n = n << 6;
                        ROMData.put(i, (byte)(n + p));
                    }
                }
            }

            Console.WriteLine("Here");
            ROMData.put(0x4DEA, (Byte)westHyrule.trophyCave.item);
            ROMData.put(0x502A, (Byte)westHyrule.jar.item);
            ROMData.put(0x4DD7, (Byte)westHyrule.heart2.item);
            //Console.WriteLine(westHyrule.heart1.item);
            //Console.WriteLine(westHyrule.heart2.item);
            //Console.WriteLine(westHyrule.medCave.item);
            //Console.WriteLine(westHyrule.trophyCave.item);
            //Console.WriteLine(westHyrule.jar.item);
            //Console.WriteLine(deathMountain.magicCave.item);
            //Console.WriteLine(deathMountain.hammerCave.item);
            //Console.WriteLine(westHyrule.palace1.PalNum + " " + westHyrule.palace1.item);
            //Console.WriteLine(westHyrule.palace2.PalNum + " " + westHyrule.palace2.item);
            //Console.WriteLine(westHyrule.palace3.PalNum + " " + westHyrule.palace3.item);
            //Console.WriteLine(mazeIsland.palace4.PalNum + " " + mazeIsland.palace4.item);
            //Console.WriteLine(eastHyrule.palace5.PalNum + " " + eastHyrule.palace5.item);
            //Console.WriteLine(eastHyrule.palace6.PalNum + " " + eastHyrule.palace6.item);
            //Console.WriteLine(eastHyrule.gp.PalNum + " " + eastHyrule.gp.item);
            //Console.WriteLine(eastHyrule.heart1.item);
            //Console.WriteLine(eastHyrule.heart2.item);
            //Console.WriteLine(mazeIsland.magic.item);
            //Console.WriteLine(mazeIsland.kid.item);
            //Console.WriteLine(eastHyrule.newKasuto.item);
            //Console.WriteLine(eastHyrule.newKasuto2.item);
            //Console.WriteLine(eastHyrule.pbagCave1.item);
            //Console.WriteLine(eastHyrule.pbagCave2.item);
            //Console.WriteLine(eastHyrule.gp.item);
            //Console.WriteLine(spellMap[spells.life]);
            //Console.WriteLine(spellMap[spells.shield]);
            //Console.WriteLine(spellMap[spells.fire]);
            //Console.WriteLine(spellMap[spells.reflect]);
            //Console.WriteLine(spellMap[spells.jump]);
            //Console.WriteLine(spellMap[spells.thunder]);
            //Console.WriteLine(spellMap[spells.fairy]);
            //Console.WriteLine(spellMap[spells.spell]);

            int[] itemLocs2 = { 0x10E91, 0x10E9A, 0x1252D, 0x12538, 0x10EA3, 0x12774 };


            ROMData.put(0x5069, (Byte)westHyrule.medCave.item);
            ROMData.put(0x4ff5, (Byte)westHyrule.heart1.item);
            
            ROMData.put(0x65C3, (Byte)deathMountain.magicCave.item);
            ROMData.put(0x6512, (Byte)deathMountain.hammerCave.item);
            ROMData.put(0x8FAA, (Byte)eastHyrule.heart1.item);
            ROMData.put(0x9011, (Byte)eastHyrule.heart2.item);
            if (!props.createPalaces)
            {
                if (westHyrule.palace1.PalNum != 7)
                {
                    ROMData.put(itemLocs2[westHyrule.palace1.PalNum - 1], (Byte)westHyrule.palace1.item);
                }
                if (westHyrule.palace2.PalNum != 7)
                {
                    ROMData.put(itemLocs2[westHyrule.palace2.PalNum - 1], (Byte)westHyrule.palace2.item);
                }
                if (westHyrule.palace3.PalNum != 7)
                {
                    ROMData.put(itemLocs2[westHyrule.palace3.PalNum - 1], (Byte)westHyrule.palace3.item);
                }
                if (eastHyrule.palace5.PalNum != 7)
                {
                    ROMData.put(itemLocs2[eastHyrule.palace5.PalNum - 1], (Byte)eastHyrule.palace5.item);
                }
                if (eastHyrule.palace6.PalNum != 7)
                {
                    ROMData.put(itemLocs2[eastHyrule.palace6.PalNum - 1], (Byte)eastHyrule.palace6.item);
                }
                if (mazeIsland.palace4.PalNum != 7)
                {
                    ROMData.put(itemLocs2[mazeIsland.palace4.PalNum - 1], (Byte)mazeIsland.palace4.item);
                }


                if (eastHyrule.gp.PalNum != 7)
                {
                    ROMData.put(itemLocs2[eastHyrule.gp.PalNum - 1], (Byte)eastHyrule.gp.item);
                }
            }
            else
            {
                ROMData.elevatorBossFix(props.bossItem);
                if (westHyrule.palace1.PalNum != 7)
                {
                    palaces[westHyrule.palace1.PalNum-1].updateItem(westHyrule.palace1.item);
                }
                if (westHyrule.palace2.PalNum != 7)
                {
                    palaces[westHyrule.palace2.PalNum - 1].updateItem(westHyrule.palace2.item);
                }
                if (westHyrule.palace3.PalNum != 7)
                {
                    palaces[westHyrule.palace3.PalNum - 1].updateItem(westHyrule.palace3.item);
                }
                if (eastHyrule.palace5.PalNum != 7)
                {
                    palaces[eastHyrule.palace5.PalNum - 1].updateItem(eastHyrule.palace5.item);
                }
                if (eastHyrule.palace6.PalNum != 7)
                {
                    palaces[eastHyrule.palace6.PalNum - 1].updateItem(eastHyrule.palace6.item);
                }
                if (mazeIsland.palace4.PalNum != 7)
                {
                    palaces[mazeIsland.palace4.PalNum - 1].updateItem(mazeIsland.palace4.item);
                }


                if (eastHyrule.gp.PalNum != 7)
                {
                    palaces[eastHyrule.gp.PalNum - 1].updateItem(eastHyrule.gp.item);
                }

                ROMData.put(westHyrule.palace1.MemAddress + 0x7e, (byte)palaces[westHyrule.palace1.PalNum - 1].Root.Newmap);
                ROMData.put(westHyrule.palace2.MemAddress + 0x7e, (byte)palaces[westHyrule.palace2.PalNum - 1].Root.Newmap);
                ROMData.put(westHyrule.palace3.MemAddress + 0x7e, (byte)palaces[westHyrule.palace3.PalNum - 1].Root.Newmap);
                ROMData.put(eastHyrule.palace5.MemAddress + 0x7e, (byte)palaces[eastHyrule.palace5.PalNum - 1].Root.Newmap);
                ROMData.put(eastHyrule.palace6.MemAddress + 0x7e, (byte)palaces[eastHyrule.palace6.PalNum - 1].Root.Newmap);
                ROMData.put(eastHyrule.gp.MemAddress + 0x7e, (byte)palaces[eastHyrule.gp.PalNum - 1].Root.Newmap);
                ROMData.put(mazeIsland.palace4.MemAddress + 0x7e, (byte)palaces[mazeIsland.palace4.PalNum - 1].Root.Newmap);

            }
            if (eastHyrule.newKasuto.townNum == 8)
            {
                ROMData.put(0xDB95, (Byte)eastHyrule.newKasuto2.item); //map 47

                ROMData.put(0xDB8C, (Byte)eastHyrule.newKasuto.item); //map 46
            }
            else
            {
                ROMData.put(0xDB95, (Byte)westHyrule.lifeSouth.item); //map 47

                ROMData.put(0xDB8C, (Byte)westHyrule.lifeNorth.item); //map 46
            }

            if (props.townSwap)
            {
                if (westHyrule.shieldTown.townNum != 1)
                {
                    ROMData.put(westHyrule.shieldTown.MemAddress + 0x7E, (byte)(westHyrule.shieldTown.Map + 0xC0));
                    ROMData.put(westHyrule.shieldTown.MemAddress + 0xBD, (byte)8);
                    ROMData.put(eastHyrule.fireTown.MemAddress + 0x7E, (byte)(eastHyrule.fireTown.Map + 0xC0));
                    ROMData.put(eastHyrule.fireTown.MemAddress + 0xBD, (byte)6);
                }

                if (westHyrule.jump.townNum != 2)
                {
                    ROMData.put(westHyrule.jump.MemAddress + 0x7E, (byte)(westHyrule.jump.Map + 0xC0));
                    ROMData.put(westHyrule.jump.MemAddress + 0xBD, (byte)8);
                    ROMData.put(eastHyrule.darunia.MemAddress + 0x7E, (byte)(eastHyrule.darunia.Map + 0xC0));
                    ROMData.put(eastHyrule.darunia.MemAddress + 0xBD, (byte)6);
                }

                if (westHyrule.lifeNorth.townNum != 3)
                {
                    ROMData.put(westHyrule.lifeNorth.MemAddress + 0x7E, (byte)(westHyrule.lifeNorth.Map));
                    ROMData.put(westHyrule.lifeNorth.MemAddress + 0xBD, (byte)8);
                    ROMData.put(westHyrule.lifeSouth.MemAddress, (byte)0);
                    ROMData.put(eastHyrule.newKasuto.MemAddress + 0x7E, (byte)(eastHyrule.newKasuto.Map + 0xC0));
                    ROMData.put(eastHyrule.newKasuto.MemAddress + 0xBD, (byte)6);
                    ROMData.put(eastHyrule.newKasuto2.MemAddress + 0x7F, (byte)(eastHyrule.newKasuto2.Map));
                    ROMData.put(eastHyrule.newKasuto2.MemAddress + 0xBE, (byte)6);
                }

                if (westHyrule.fairy.townNum != 5)
                {
                    ROMData.put(westHyrule.fairy.MemAddress + 0x7E, (byte)(westHyrule.fairy.Map + 0xC0));
                    ROMData.put(westHyrule.fairy.MemAddress + 0xBD, (byte)8);
                    ROMData.put(eastHyrule.oldKasuto.MemAddress + 0x7E, (byte)(eastHyrule.oldKasuto.Map + 0xC0));
                    ROMData.put(eastHyrule.oldKasuto.MemAddress + 0xBD, (byte)6);
                }
            }

            ROMData.put(0xA5A8, (Byte)mazeIsland.magic.item);
            ROMData.put(0xA58B, (Byte)mazeIsland.kid.item);
            
            if (props.pbagItemShuffle)
            {
                ROMData.put(0x4FE2, (Byte)westHyrule.pbagCave.item);
                ROMData.put(0x8ECC, (Byte)eastHyrule.pbagCave1.item);
                ROMData.put(0x8FB3, (Byte)eastHyrule.pbagCave2.item);

            }

            foreach (Location l in pbagHearts)
            {
                if (l == westHyrule.pbagCave)
                {
                    ROMData.put(0x4FE2, (Byte)westHyrule.pbagCave.item);
                }

                if (l == eastHyrule.pbagCave1)
                {
                    ROMData.put(0x8ECC, (Byte)eastHyrule.pbagCave1.item);
                }
                if (l == eastHyrule.pbagCave2)
                {
                    ROMData.put(0x8FB3, (Byte)eastHyrule.pbagCave2.item);
                }
            }

            //Make a hash for select screen
            MD5 hasher = MD5.Create();
            Byte[] fl = hasher.ComputeHash(Encoding.UTF8.GetBytes(props.flags + props.seed + typeof(MainUI).Assembly.GetName().Version.Major + typeof(MainUI).Assembly.GetName().Version.Minor));
            long inthash = BitConverter.ToInt64(fl, 0);

            ROMData.put(0x17C2C, (byte)(((inthash) & 0x1F) + 0xD0));
            ROMData.put(0x17C2E, (byte)(((inthash >> 5) & 0x1F) + 0xD0));
            ROMData.put(0x17C30, (byte)(((inthash >> 10) & 0x1F) + 0xD0));
            ROMData.put(0x17C32, (byte)(((inthash >> 15) & 0x1F) + 0xD0));
            ROMData.put(0x17C34, (byte)(((inthash >> 20) & 0x1F) + 0xD0));
            ROMData.put(0x17C36, (byte)(((inthash >> 25) & 0x1F) + 0xD0));

            //Update raft animation
            bool firstRaft = false;
            foreach(World w in worlds)
            {
                if (w.raft != null)
                {
                    if (!firstRaft)
                    {
                        ROMData.put(0x538, (Byte)w.raft.Xpos);
                        ROMData.put(0x53A, (Byte)w.raft.Ypos);
                        firstRaft = true;
                    } 
                    else
                    {
                        ROMData.put(0x539, (Byte)w.raft.Xpos);
                        ROMData.put(0x53B, (Byte)w.raft.Ypos);
                    }
                } 
            }

            firstRaft = false;
            //Fix Maze Island Bridge music bug
            foreach (World w in worlds)
            {
                if (w.bridge != null)
                {
                    if (!firstRaft)
                    {
                        ROMData.put(0x565, (Byte)w.bridge.Xpos);
                        ROMData.put(0x567, (Byte)w.bridge.Ypos);
                        firstRaft = true;
                    }
                    else
                    {
                        ROMData.put(0x564, (Byte)w.bridge.Xpos);
                        ROMData.put(0x566, (Byte)w.bridge.Ypos);
                    }
                }
            }

            //Update world check for p7
            if (westHyrule.palace1.PalNum == 7 || westHyrule.palace2.PalNum == 7 || westHyrule.palace3.PalNum == 7)
            {
                ROMData.put(0x1dd3b, 0x05);
            }

            if (mazeIsland.palace4.PalNum == 7)
            {
                ROMData.put(0x1dd3b, 0x14);
            }

            Console.WriteLine("Here");

            int spellNameBase = 0x1c3a, effectBase = 0x00e58, spellCostBase = 0xd8b, functionBase = 0xdcb;

            int[,] magLevels = new int[8, 8];
            int[,] magNames = new int[8, 7];
            int[] magEffects = new int[16];
            int[] magFunction = new int[8];
            ROMData.updateSpellText(spellMap);

            for (int i = 0; i < magFunction.Count(); i++)
            {
                magFunction[i] = ROMData.getByte(functionBase + (int)spellMap[(spells)i]);
            }

            for (int i = 0; i < magEffects.Count(); i = i + 2)
            {
                magEffects[i] = ROMData.getByte(effectBase + (int)spellMap[(spells)(i / 2)] * 2);
                magEffects[i + 1] = ROMData.getByte(effectBase + (int)spellMap[(spells)(i / 2)] * 2 + 1);
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    magLevels[i, j] = ROMData.getByte(spellCostBase + ((int)spellMap[(spells)i] * 8 + j));
                }

                for (int j = 0; j < 7; j++)
                {
                    magNames[i, j] = ROMData.getByte(spellNameBase + ((int)spellMap[(spells)i] * 0xe + j));
                }
            }

            for (int i = 0; i < magFunction.Count(); i++)
            {
                ROMData.put(functionBase + i, (Byte)magFunction[i]);
            }

            for (int i = 0; i < magEffects.Count(); i = i + 2)
            {
                ROMData.put(effectBase + i, (Byte)magEffects[i]);
                ROMData.put(effectBase + i + 1, (Byte)magEffects[i + 1]);
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ROMData.put(spellCostBase + (i * 8) + j, (Byte)magLevels[i, j]);
                }

                for (int j = 0; j < 7; j++)
                {
                    ROMData.put(spellNameBase + (i * 0xe) + j, (Byte)magNames[i, j]);
                }
            }

            //fix for rope graphical glitch
            for (int i = 0; i < 16; i++)
            {
                ROMData.put(0x32CD0 + i, ROMData.getByte(0x34CD0 + i));
            }

            //if (hiddenPalace)
            //{
            //    ROMData.put(0x8664, 0);
            //}

            //if (hiddenKasuto)
            //{
            //    ROMData.put(0x8660, 0);
            //}

        }

        public void shuffleE(int enemyPtr, int enemyAddr, List<int> enemies, List<int> generators, List<int> shorties, List<int> tallGuys, List<int> flyingEnemies, bool p7)
        {
            //refactor this to use enemy arrays in rooms
            int maps = 0;
            List<int> mapsNos = new List<int>();
            if(!p7)
            {
                List<int> palacesInt = new List<int> { 1, 2, 5 };
                if (enemyPtr == enemyPtr2)
                {
                    palacesInt = new List<int> { 3, 4, 6 };
                }

                foreach(int palace in palacesInt) 
                { 
                    foreach(Room r in palaces[palace-1].AllRooms)
                    {
                        if(r.Newmap == 0)
                        {
                            mapsNos.Add(r.Map);
                        }
                        else
                        {
                            mapsNos.Add(r.Newmap);
                        }
                    }
                }
            }
            else
            {
                foreach (Room r in palaces[6].AllRooms)
                {
                    if (r.Newmap == 0)
                    {
                        mapsNos.Add(r.Map);
                    }
                    else
                    {
                        mapsNos.Add(r.Newmap);
                    }
                }
            }
            foreach(int map in mapsNos) 
            {
                int low = ROMData.getByte(enemyPtr + map * 2);
                int high = ROMData.getByte(enemyPtr + map * 2 + 1);
                high = high << 8;
                high = high & 0x0FFF;
                int addr = high + low + enemyAddr;
                shuffleEnemies(high + low + enemyAddr, enemies, generators, shorties, tallGuys, flyingEnemies, p7);
            }
        }

        public void shuffleEnemies(int addr, List<int> enemies, List<int> generators, List<int> shorties, List<int> tallGuys, List<int> flyingEnemies, bool p7)
        {
            if (!visitedEnemies.Contains(addr))
            {
                int numBytes = ROMData.getByte(addr);
                for (int j = addr + 2; j < addr + numBytes; j = j + 2)
                {
                    int enemy = ROMData.getByte(j) & 0x3F;
                    int highPart = ROMData.getByte(j) & 0xC0;
                    if (props.mixEnemies)
                    {
                        if (enemies.Contains(enemy))
                        {
                            int swap = enemies[r.Next(0, enemies.Count)];
                            int ypos = ROMData.getByte(j - 1) & 0xF0;
                            int xpos = ROMData.getByte(j - 1) & 0x0F;
                            if (shorties.Contains(enemy) && tallGuys.Contains(swap))
                            {
                                ypos = ypos - 16;
                                while (swap == 0x1D && ypos != 0x70 && !p7)
                                {
                                    swap = tallGuys[R.Next(0, tallGuys.Count)];
                                }
                            }
                            else
                            {
                                while (swap == 0x1D && ypos != 0x70 && !p7)
                                {
                                    swap = enemies[R.Next(0, enemies.Count)];
                                }
                            }


                            ROMData.put(j - 1, (Byte)(ypos + xpos));
                            ROMData.put(j, (Byte)(swap + highPart));
                        }
                    }
                    else
                    {
                        if (tallGuys.Contains(enemy))
                        {
                            int swap = R.Next(0, tallGuys.Count);
                            int ypos = ROMData.getByte(j - 1) & 0xF0;
                            while (tallGuys[swap] == 0x1D && ypos != 0x70 && !p7)
                            {
                                swap = R.Next(0, tallGuys.Count);
                            }
                            ROMData.put(j, (Byte)(tallGuys[swap] + highPart));
                        }

                        if (shorties.Contains(enemy))
                        {
                            int swap = R.Next(0, shorties.Count);
                            ROMData.put(j, (Byte)(shorties[swap] + highPart));
                        }
                    }


                    if (flyingEnemies.Contains(enemy))
                    {
                        int swap = R.Next(0, flyingEnemies.Count);
                        while (enemy == 0x07 && (flyingEnemies[swap] == 0x06 || flyingEnemies[swap] == 0x0E))
                        {
                            swap = R.Next(0, flyingEnemies.Count);
                        }
                        ROMData.put(j, (Byte)(flyingEnemies[swap] + highPart));
                    }

                    if (generators.Contains(enemy))
                    {
                        int swap = R.Next(0, generators.Count);
                        ROMData.put(j, (Byte)(generators[swap] + highPart));
                    }

                    if (enemy == 0x0B)
                    {
                        int swap = R.Next(0, generators.Count + 1);
                        if (swap != generators.Count)
                        {
                            ROMData.put(j, (Byte)(generators[swap] + highPart));
                        }
                    }
                }
                visitedEnemies.Add(addr);
            }

        }
        public void shuffleSmallItems(int world, bool first)
        {
            Console.WriteLine("World: " + world);
            List<int> addresses = new List<int>();
            List<int> items = new List<int>();
            int startAddr;
            if (first)
            {
                startAddr = 0x8523 - 0x8000 + (world * 0x4000) + 0x10;
            }
            else
            {
                startAddr = 0xA000 - 0x8000 + (world * 0x4000) + 0x10;
            }
            int map = 0;
            for (int i = startAddr; i < startAddr + 126; i = i + 2)
            {

                map++;
                int low = ROMData.getByte(i);
                int hi = ROMData.getByte(i + 1) * 256;
                int numBytes = ROMData.getByte(hi + low + 16 - 0x8000 + (world * 0x4000));
                for (int j = 4; j < numBytes; j = j + 2)
                {
                    int yPos = ROMData.getByte(hi + low + j + 16 - 0x8000 + (world * 0x4000)) & 0xF0;
                    yPos = yPos >> 4;
                    if (ROMData.getByte(hi + low + j + 1 + 16 - 0x8000 + (world * 0x4000)) == 0x0F && yPos < 13)
                    {
                        int addr = hi + low + j + 2 + 16 - 0x8000 + (world * 0x4000);
                        int item = ROMData.getByte(addr);
                        if (item == 8 || (item > 9 && item < 14) || (item > 15 && item < 19) && !addresses.Contains(addr))
                        {
                            Console.WriteLine("Map: " + map);
                            Console.WriteLine("Item: " + item);
                            Console.WriteLine("Address: {0:X}", addr);
                            addresses.Add(addr);
                            items.Add(item);
                        }
                        j++;
                    }
                }
            }


            for (int i = 0; i < items.Count; i++)
            {
                int swap = r.Next(i, items.Count);
                int temp = items[swap];
                items[swap] = items[i];
                items[i] = temp;
            }
            for (int i = 0; i < addresses.Count; i++)
            {
                ROMData.put(addresses[i], (Byte)items[i]);
            }
        }

    }
}
