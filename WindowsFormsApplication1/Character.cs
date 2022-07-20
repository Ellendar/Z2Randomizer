using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    class Character
    {
        private Boolean[] itemGet;
        private Boolean[] spellGet;
        private Boolean startWithTrophy;
        private Boolean startWithKid;
        private Boolean startWithMedicine;
        

        public Character(RandomizerProperties props)
        {
        }

        public bool StartWithTrophy { get => startWithTrophy;  }
        public bool StartWithKid { get => startWithKid;  }
        public bool StartWithMedicine { get => startWithMedicine;  }

        public Boolean has(items item)
        {
            //check item map
            return false;
        }

        public Boolean has(spells spell)
        {
            return false;
            //check spell map
        }

    }
}
