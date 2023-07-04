namespace Z2Randomizer.Core;

public enum Spell { SHIELD = 0, JUMP = 1, LIFE = 2, FAIRY = 3, FIRE = 4, REFLECT = 5, SPELL = 6, THUNDER = 7, DOWNSTAB = 8, UPSTAB = 9, DASH = 10 }

public static class SpellExtensions
{
    public static int VanillaSpellOrder(this Spell spell)
    {
        return spell switch
        {
            Spell.SHIELD => 0,
            Spell.JUMP => 1,
            Spell.LIFE => 2,
            Spell.FAIRY => 3,
            Spell.FIRE => 4,
            Spell.DASH => 4,
            Spell.REFLECT => 5,
            Spell.SPELL => 6,
            Spell.THUNDER => 7,
            Spell.DOWNSTAB => 8,
            _ => throw new ImpossibleException("Invalid vanilla spell index")
        };
    }
}
