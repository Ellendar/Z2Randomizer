using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore;

internal class Towns
{
    public static readonly Dictionary<Town, Requirements> townSpellAndItemRequirements = new()
    {
        { Town.RAURU, new Requirements() },
        { Town.RUTO, new Requirements(new RequirementType[] {RequirementType.TROPHY }) },
        { Town.SARIA_NORTH, new Requirements(new RequirementType[] {RequirementType.MIRROR }) },
        { Town.MIDO_WEST, new Requirements(new RequirementType[] {RequirementType.MEDICINE }) },
        { Town.MIDO_CHURCH, new Requirements(new RequirementType[] {RequirementType.JUMP, RequirementType.FAIRY }) },
        { Town.NABOORU, new Requirements([], [[RequirementType.WATER, RequirementType.FIVE_CONTAINERS]]) },
        { Town.DARUNIA_WEST, new Requirements([], [[RequirementType.CHILD, RequirementType.SIX_CONTAINERS]]) },
        { Town.DARUNIA_ROOF, new Requirements(new RequirementType[] {RequirementType.JUMP, RequirementType.FAIRY }) },
        { Town.NEW_KASUTO, new Requirements(new RequirementType[] {RequirementType.SEVEN_CONTAINERS })  },
        { Town.OLD_KASUTO, new Requirements(new RequirementType[] { RequirementType.EIGHT_CONTAINERS }) },
        { Town.NABOORU_FOUNTAIN, new Requirements() },
        { Town.BAGU, new Requirements() },
        { Town.SARIA_TABLE, new Requirements() }
    };

    public static readonly List<Town> STRICT_SPELL_LOCATIONS = [ Town.RAURU, Town.RUTO, Town.SARIA_NORTH, Town.MIDO_WEST, 
        Town.NABOORU, Town.DARUNIA_WEST, Town.NEW_KASUTO, Town.OLD_KASUTO ];

    public static readonly List<Town> ITEM_LOCATION_TOWNS = [ Town.RAURU, Town.RUTO, Town.SARIA_NORTH, Town.MIDO_WEST, 
        Town.NABOORU, Town.DARUNIA_WEST, Town.NEW_KASUTO, Town.OLD_KASUTO, 
        Town.MIDO_CHURCH, Town.DARUNIA_ROOF, 
        Town.SARIA_TABLE, Town.NABOORU_FOUNTAIN, Town.BAGU];
}
