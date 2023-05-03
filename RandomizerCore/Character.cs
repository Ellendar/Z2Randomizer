using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.Core;

//This isn't actually used for anything. Should it be?
class Character
{
    private bool[] itemGet;
    private bool[] spellGet;
    private bool startWithTrophy;
    private bool startWithKid;
    private bool startWithMedicine;
    

    public Character(RandomizerProperties props)
    {
    }

    public bool StartWithTrophy { get => startWithTrophy;  }
    public bool StartWithKid { get => startWithKid;  }
    public bool StartWithMedicine { get => startWithMedicine;  }

    public bool Has(Item item)
    {
        //check item map
        throw new NotImplementedException();
    }

    public bool Has(Spell spell)
    {
        //check spell map
        throw new NotImplementedException();
    }

}
