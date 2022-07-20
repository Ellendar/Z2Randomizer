using System;

namespace Z2Randomizer
{
    public enum terrain
    {
        town = 0,
        cave = 1,
        palace = 2,
        bridge = 3,
        desert = 4,
        grass = 5,
        forest = 6,
        swamp = 7,
        grave = 8,
        road = 9,
        lava = 10,
        mountain = 11,
        water = 12,
        walkablewater = 13,
        rock = 14,
        spider = 15,
        none = 16
    }

    public enum continent
    {
        west = 0,
        dm = 1,
        east = 2,
        maze = 3
    }

    class Location
    {
        private Byte[] locationBytes;
        private int externalWorld;
        private int ypos;
        private int xpos;
        private int secondpartofcave;
        private int appear2loweruponexit;
        private int horizontalPos;
        private int map;
        private int forceEnterRight;
        private int passThrough;
        private int fallInHole;
        private int world;
        private terrain terrainType;
        private Boolean needFairy;
        private Boolean needhammer;
        private Boolean needjump;
        private Boolean needRecorder;
        private Boolean needBagu;
        private Boolean needBoots;
        private int memAddress;
        private Tuple<int, int> coords;
        private Boolean canShuffle;
        public items item;
        public Boolean itemGet;
        private Boolean reachable;
        private int palNum;
        private continent continent;

        public terrain TerrainType
        {
            get
            {
                return terrainType;
            }

            set
            {
                terrainType = value;
            }
        }

        public int Ypos
        {
            get
            {
                return ypos;
            }

            set
            {
                ypos = value;
            }
        }

        public int Xpos
        {
            get
            {
                return xpos;
            }

            set
            {
                xpos = value;
            }
        }

        public byte[] LocationBytes
        {
            get
            {
                return locationBytes;
            }

            set
            {
                locationBytes = value;
            }
        }

        public int MemAddress
        {
            get
            {
                return memAddress;
            }

            set
            {
                memAddress = value;
            }
        }

        public int PassThrough
        {
            get
            {
                return passThrough;
            }

            set
            {
                passThrough = value;
            }
        }

        public int Map
        {
            get
            {
                return map;
            }

            set
            {
                map = value;
            }
        }

        public int World
        {
            get
            {
                return world;
            }

            set
            {
                world = value;
            }
        }

        public Tuple<int, int> Coords
        {
            get
            {
                return Tuple.Create(ypos, xpos);
            }

            set
            {
                coords = value;
            }
        }

        public bool Needjump
        {
            get
            {
                return needjump;
            }

            set
            {
                needjump = value;
            }
        }

        public bool Needhammer
        {
            get
            {
                return needhammer;
            }

            set
            {
                needhammer = value;
            }
        }
        public Boolean Needboots
        {
            get
            {
                return needBoots;
            }
            set
            {
                needBoots = value;
            }
        }
        public bool NeedFairy
        {
            get
            {
                return needFairy;
            }

            set
            {
                needFairy = value;
            }
        }

        public bool NeedRecorder
        {
            get
            {
                return needRecorder;
            }

            set
            {
                needRecorder = value;
            }
        }

        public bool NeedBagu
        {
            get
            {
                return needBagu;
            }

            set
            {
                needBagu = value;
            }
        }

        public bool CanShuffle
        {
            get
            {
                return canShuffle;
            }

            set
            {
                canShuffle = value;
            }
        }

        public int HorizontalPos
        {
            get
            {
                return horizontalPos;
            }

            set
            {
                horizontalPos = value;
            }
        }

        public int ExternalWorld
        {
            get
            {
                return externalWorld;
            }

            set
            {
                externalWorld = value;
            }
        }

        public bool Reachable
        {
            get
            {
                return reachable;
            }

            set
            {
                reachable = value;
            }
        }

        public int PalNum
        {
            get
            {
                return palNum;
            }

            set
            {
                palNum = value;
            }
        }

        public int townNum { get; set; }
        public continent Continent { get => continent; set => continent = value; }
        public int FallInHole { get => fallInHole; set => fallInHole = value; }
        public int ForceEnterRight { get => forceEnterRight; set => forceEnterRight = value; }
        public int Secondpartofcave { get => secondpartofcave; set => secondpartofcave = value; }

        /*
        Byte 0

        .xxx xxxx - Y position
        x... .... - External to this world

        Byte 1 (offset 3F bytes from Byte 0)

        ..xx xxxx - X position
        .x.. .... - Second part of a cave
        x... .... - Appear at the position of the area in ROM offset 2 lower than this one upon exit

        Byte 2 (offset 7E bytes from Byte 0)

        ..xx xxxx - Map number
        xx.. .... - Horizontal position to enter within map
            0 = enter from the left
            1 = enter at x=256 or from the right for 2 screens maps
            2 = enter at x=512 or from the right for 3 screens maps
            3 = enter from the right for 4 screens maps

        Byte 3 (offset BD bytes from Byte 0)

        ...x xxxx - World number
        ..x. .... - Forced enter from the right edge of screen
        .x.. .... - Pass through
        x... .... - Fall in hole
        */
        public Location(Byte[] bytes, terrain t, int mem, continent c)
        {
            locationBytes = bytes;
            externalWorld = bytes[0] & 128;
            ypos = bytes[0] & 127;
            appear2loweruponexit = bytes[1] & 128;
            secondpartofcave = bytes[1] & 64;
            xpos = bytes[1] & 63;
            horizontalPos = bytes[2] & 192;
            map = bytes[2] & 63;
            FallInHole = bytes[3] & 128;
            passThrough = bytes[3] & 64;
            ForceEnterRight = bytes[3] & 32;
            world = bytes[3] & 31;
            terrainType = t;
            memAddress = mem;
            canShuffle = true;
            item = items.donotuse;
            itemGet = false;
            reachable = false;
            palNum = 0;
            townNum = 0;
            Continent = c;
        }

        public Location()
        {

        }
        public void updateBytes()
        {
            if (needhammer || NeedRecorder)
            {
                LocationBytes[0] = 0;
            }
            else
            {
                LocationBytes[0] = (Byte)(externalWorld + ypos);
            }
            LocationBytes[1] = (Byte)(appear2loweruponexit + secondpartofcave + xpos);
            LocationBytes[2] = (Byte)(horizontalPos + map);
            LocationBytes[3] = (Byte)(FallInHole + passThrough + ForceEnterRight + world);
        }
    }
}
