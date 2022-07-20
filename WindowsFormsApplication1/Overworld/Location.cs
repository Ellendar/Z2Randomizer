using System;

namespace Z2Randomizer
{
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
        private Terrain terrainType;
        private Boolean needFairy;
        private Boolean needhammer;
        private Boolean needjump;
        private Boolean needRecorder;
        private Boolean needBagu;
        private Boolean needBoots;
        private int memAddress;
        private Tuple<int, int> coords;
        private Boolean canShuffle;
        public Items item;
        public Boolean itemGet;
        private Boolean reachable;
        private int palNum;
        private Continent continent;

        public Terrain TerrainType { get; set; }
        public int Ypos { get; set; }
        public int Xpos { get; set; }
        public byte[] LocationBytes { get; set; }

        public int MemAddress { get; set; }

        public int PassThrough { get; set; }

        public int Map { get; set; }

        public int World { get; set; }

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

        public bool NeedJump { get; set; }

        public bool NeedHammer { get; set; }
        public Boolean Needboots { get; set; }
        public bool NeedFairy { get; set; }

        public bool NeedRecorder { get; set; }

        public bool NeedBagu { get; set; }

        public bool CanShuffle { get; set; }

        public int HorizontalPos { get; set; }

        public int ExternalWorld { get; set; }

        public bool Reachable { get; set; }

        public int PalNum { get; set; }

        public Town TownNum { get; set; }
        public Continent Continent { get => continent; set => continent = value; }
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
        public Location(Byte[] bytes, Terrain t, int mem, Continent c)
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
            item = Items.donotuse;
            itemGet = false;
            reachable = false;
            palNum = 0;
            TownNum = 0;
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
