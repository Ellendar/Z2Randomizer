using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    class Result
    {
        public int Id { get; set; }
        public string Flags { get; set; }
        public int Seed { get; set; }
        //Biomes
        public String WestBiome { get; set; }
        //Spell costs
        //XP thresholds
        //Which items are required (is this cleanly saved?)
        public Result(Hyrule hyrule, RandomizerProperties properties)
        {
            Flags = properties.flags;
            Seed = properties.seed;
            WestBiome = properties.westBiome;
        }
    }
}
