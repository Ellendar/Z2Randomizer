
using System.Collections.Generic;

namespace Z2Randomizer.RandomizerCore;

public enum RequirementType
{
    JUMP, 
    FAIRY, 
    UPSTAB, 
    DOWNSTAB, 
    KEY, 
    DASH, 
    GLOVE, 
    REFLECT, 
    SPELL,
    ONE_CONTAINER,
    TWO_CONTAINERS,
    THREE_CONTAINERS,
    FOUR_CONTAINERS,
    FIVE_CONTAINERS, 
    SIX_CONTAINERS, 
    SEVEN_CONTAINERS, 
    EIGHT_CONTAINERS, 
    TROPHY, 
    MEDICINE, 
    CHILD,
    MIRROR,
    WATER,
    BOOTS,
    FLUTE,
    HAMMER,
    BAGU_LETTER
}

public static class RequirementTypeExtensions
{
    public static RequirementType[] UpToXContainers(int x)
    {
        List<RequirementType> requirements = [];
        if(x >= 1)
        {
            requirements.Add(RequirementType.ONE_CONTAINER);
        }
        if (x >= 2)
        {
            requirements.Add(RequirementType.TWO_CONTAINERS);
        }
        if (x >= 3)
        {
            requirements.Add(RequirementType.THREE_CONTAINERS);
        }
        if (x >= 4)
        {
            requirements.Add(RequirementType.FOUR_CONTAINERS);
        }
        if (x >= 5)
        {
            requirements.Add(RequirementType.FIVE_CONTAINERS);
        }
        if (x >= 6)
        {
            requirements.Add(RequirementType.SIX_CONTAINERS);
        }
        if (x >= 7)
        {
            requirements.Add(RequirementType.SEVEN_CONTAINERS);
        }
        if (x >= 8)
        {
            requirements.Add(RequirementType.EIGHT_CONTAINERS);
        }
        return requirements.ToArray();
    }
}
