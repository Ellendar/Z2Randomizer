using System;
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore.Sidescroll.Town;

public class Towns
{

    //There's almost certainly way cleaner way to do this but this works for now.
    public static Town LoadVanillaTown(ROM rom, TownType townType, bool disableContainerRequirements)
    {
        Town town;
        TownMap map;
        switch(townType)
        {
            case TownType.RAURU:
                town = rom.LoadTown(1, 2);
                town.Name = "Rauru";
                map = town.GetTownMap(VanillaTownMap.RAURU_LEFT)!;
                map.Name = "Rauru Left";
                map = town.GetTownMap(VanillaTownMap.RAURU_RIGHT)!;
                map.Name = "Rauru Right";
                map = town.GetTownMap(VanillaTownMap.RAURU_WIZARD)!;
                map.Name = "Rauru Wizard";
                map.Collectable = Collectable.SHIELD_SPELL;
                map.AccessRequirements = disableContainerRequirements ? Requirements.NONE : new Requirements([RequirementType.ONE_CONTAINER]);
                break;
            case TownType.RUTO:
                town = rom.LoadTown(4, 2);
                town.Name = "Ruto";
                map = town.GetTownMap(VanillaTownMap.RUTO_LEFT)!;
                map.Name = "Ruto Left";
                map = town.GetTownMap(VanillaTownMap.RUTO_RIGHT)!;
                map.Name = "Ruto Right";
                map = town.GetTownMap(VanillaTownMap.RUTO_WIZARD)!;
                map.Name = "Ruto Wizard";
                map.Collectable = Collectable.JUMP_SPELL;
                map.AccessRequirements = disableContainerRequirements ? new Requirements([RequirementType.TROPHY])
                    : new Requirements([], [[RequirementType.TROPHY, RequirementType.TWO_CONTAINERS]]);
                break;
            case TownType.SARIA:
                town = rom.LoadTown(6, 3);
                town.Name = "Saria";
                map = town.GetTownMap(VanillaTownMap.SARIA_LEFT)!;
                map.Name = "Saria Moat";
                map.AccessRequirements = new Requirements([RequirementType.FAIRY, RequirementType.BAGU_LETTER], [[RequirementType.JUMP, RequirementType.DASH]]);
                map = town.GetTownMap(VanillaTownMap.SARIA_MID)!;
                map.Name = "Saria Middle";
                map = town.GetTownMap(VanillaTownMap.SARIA_RIGHT)!;
                map.Name = "Saria Right";
                map = town.GetTownMap(VanillaTownMap.SARIA_WIZARD)!;
                map.Name = "Saria Wizard";
                map.Collectable = Collectable.LIFE_SPELL;
                map.AccessRequirements = disableContainerRequirements ?
                    new Requirements([RequirementType.MIRROR]) : new Requirements([], [[RequirementType.MIRROR, RequirementType.THREE_CONTAINERS]]);
                map = town.GetTownMap(VanillaTownMap.SARIA_TABLE)!;
                map.Name = "Saria Table";
                map.Collectable = Collectable.MIRROR;
                break;
            case TownType.MIDO:
                town = rom.LoadTown(9, 3);
                town.Name = "Mido";
                map = town.GetTownMap(VanillaTownMap.MIDO_LEFT)!;
                map.Name = "Mido Left";
                map = town.GetTownMap(VanillaTownMap.MIDO_MIDDLE)!;
                map.Name = "Mido Middle";
                map = town.GetTownMap(VanillaTownMap.MIDO_RIGHT)!;
                map.Name = "Mido Right";
                map = town.GetTownMap(VanillaTownMap.MIDO_WIZARD)!;
                map.Name = "Mido Wizard";
                map.Collectable = Collectable.FAIRY_SPELL;
                map.AccessRequirements = disableContainerRequirements ? new Requirements([RequirementType.MEDICINE])
                    : new Requirements([], [[RequirementType.MEDICINE, RequirementType.FOUR_CONTAINERS]]);
                map = town.GetTownMap(VanillaTownMap.MIDO_TRAINER)!;
                map.Name = "Downstab Guy";
                map.Collectable = Collectable.DOWNSTAB;
                map.AccessRequirements = new Requirements([RequirementType.JUMP, RequirementType.FAIRY]);
                break;
            case TownType.NABOORU:
                town = rom.LoadTown(12, 3);
                town.Name = "Nabooru";
                map = town.GetTownMap(VanillaTownMap.NABOORU_LEFT)!;
                map.Name = "Nabooru Left";
                map = town.GetTownMap(VanillaTownMap.NABOORU_MID)!;
                map.Name = "Nabooru Fountain";
                map.Collectable = Collectable.WATER;
                map = town.GetTownMap(VanillaTownMap.NABOORU_RIGHT)!;
                map.Name = "Nabooru Right";
                map = town.GetTownMap(VanillaTownMap.NABOORU_WIZARD)!;
                map.Name = "Nabooru Wizard";
                map.Collectable = Collectable.FIRE_SPELL;
                map.AccessRequirements = disableContainerRequirements ? new Requirements([RequirementType.WATER])
                    : new Requirements([], [[RequirementType.WATER, RequirementType.FIVE_CONTAINERS]]);
                break;
            case TownType.DARUNIA:
                town = rom.LoadTown(15, 3);
                town.Name = "Darunia";
                map = town.GetTownMap(VanillaTownMap.DARUNIA_LEFT)!;
                map.Name = "Darunia Left";
                map = town.GetTownMap(VanillaTownMap.DARUNIA_MID)!;
                map.Name = "Darunia Middle";
                map = town.GetTownMap(VanillaTownMap.DARUNIA_RIGHT)!;
                map.Name = "Darunia Right";
                map = town.GetTownMap(VanillaTownMap.DARUNIA_WIZARD)!;
                map.Name = "Darunia Wizard";
                map.Collectable = Collectable.REFLECT_SPELL;
                map.AccessRequirements = disableContainerRequirements ? new Requirements([RequirementType.CHILD])
                    : new Requirements([], [[RequirementType.CHILD, RequirementType.SIX_CONTAINERS]]);
                map = town.GetTownMap(VanillaTownMap.DARUNIA_TRAINER)!;
                map.Name = "Upstab Guy";
                map.Collectable = Collectable.UPSTAB;
                map.AccessRequirements = new Requirements([RequirementType.JUMP, RequirementType.FAIRY]);

                break;
            case TownType.NEW_KASUTO:
                town = rom.LoadTown(18, 3);
                town.Name = "New Kasuto";
                map = town.GetTownMap(VanillaTownMap.NEW_KASUTO_LEFT)!;
                map.Name = "New Kasuto Left";
                map = town.GetTownMap(VanillaTownMap.NEW_KASUTO_MID)!;
                map.Name = "New Kasuto Middle";
                map = town.GetTownMap(VanillaTownMap.NEW_KASUTO_RIGHT)!;
                map.Name = "New Kasuto Right";
                map = town.GetTownMap(VanillaTownMap.NEW_KASUTO_WIZARD)!;
                map.Name = "New Kasuto Wizard";
                map.Collectable = Collectable.SPELL_SPELL;
                map.AccessRequirements = disableContainerRequirements ? Requirements.NONE : new Requirements([RequirementType.SEVEN_CONTAINERS]);
                map = town.GetTownMap(VanillaTownMap.GRANNYS_BASEMENT)!;
                map.Name = "Granny's Basement";
                map.AccessRequirements = new Requirements([RequirementType.SEVEN_CONTAINERS]);
                map = town.GetTownMap(VanillaTownMap.SPELL_TOWER_INTERIOR)!;
                map.Name = "Spell Tower" +
                    "";
                map.AccessRequirements = new Requirements([RequirementType.SPELL]);
                break;
            case TownType.OLD_KASUTO:
                town = rom.LoadTown(21, 3);
                town.Name = "Old Kasuto";
                map = town.GetTownMap(VanillaTownMap.OLD_KASUTO_LEFT)!;
                map.Name = "Old Kasuto Left";
                map = town.GetTownMap(VanillaTownMap.OLD_KASUTO_MID)!;
                map.Name = "Old Kasuto Left";
                map = town.GetTownMap(VanillaTownMap.OLD_KASUTO_RIGHT)!;
                map.Name = "Old Kasuto Right";
                map = town.GetTownMap(VanillaTownMap.OLD_KASUTO_WIZARD)!;
                map.Name = "Old Kasuto Wizard";
                map.Collectable = Collectable.THUNDER_SPELL;
                map.AccessRequirements = disableContainerRequirements ? Requirements.NONE
                    : new Requirements([RequirementType.EIGHT_CONTAINERS]);
                break;
            case TownType.BAGU:
                town = rom.LoadTown(24, 1);
                town.Name = "Bagu's House";
                map = town.GetTownMap(VanillaTownMap.BAGU_EXTERNAL)!;
                map.Name = "Bagu Exterior";
                map = town.GetTownMap(VanillaTownMap.BAGU_INDOORS)!;
                map.Name = "Bagu Interior";
                map.Collectable = Collectable.BAGUS_NOTE;
                break;
            case TownType.INVALID:
                throw new Exception($"Unrecognized vanilla town: {townType.HintName()}");
            default:
                throw new Exception($"Unrecognized vanilla town: {townType.HintName()}");
        }
        return town;
    }
}
