using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer
{
    //This isn't actually used for anything. Should it be?
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

        public Boolean Has(Item item)
        {
            //check item map
            throw new NotImplementedException();
        }

        public Boolean Has(Spell spell)
        {
            //check spell map
            throw new NotImplementedException();
        }

    }
}
