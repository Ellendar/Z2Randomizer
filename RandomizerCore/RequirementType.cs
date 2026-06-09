
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
    /// Mirror of Collectable.AsRequirement()
    public static Collectable? AsCollectable(this RequirementType requirementType)
    {
        return requirementType switch
        {
            RequirementType.JUMP => Collectable.JUMP_SPELL,
            RequirementType.FAIRY => Collectable.FAIRY_SPELL,
            RequirementType.UPSTAB => Collectable.UPSTAB,
            RequirementType.DOWNSTAB => Collectable.DOWNSTAB,
            RequirementType.KEY => Collectable.MAGIC_KEY,
            RequirementType.DASH => Collectable.DASH_SPELL,
            RequirementType.GLOVE => Collectable.GLOVE,
            RequirementType.REFLECT => Collectable.REFLECT_SPELL,
            RequirementType.SPELL => Collectable.SPELL_SPELL,
            RequirementType.TROPHY => Collectable.TROPHY,
            RequirementType.MEDICINE => Collectable.MEDICINE,
            RequirementType.CHILD => Collectable.CHILD,
            RequirementType.MIRROR => Collectable.MIRROR,
            RequirementType.WATER => Collectable.WATER,
            RequirementType.FLUTE => Collectable.FLUTE,
            RequirementType.HAMMER => Collectable.HAMMER,
            RequirementType.BOOTS => Collectable.BOOTS,
            RequirementType.BAGU_LETTER => Collectable.BAGUS_NOTE,
            _ => null
        };
    }

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

    public static RequirementType MagicContainerRequirementFromCost(int magicCost)
    {
        switch (magicCost)
        {
            case <= 16:
                return RequirementType.ONE_CONTAINER;
            case <= 32:
                return RequirementType.TWO_CONTAINERS;
            case <= 48:
                return RequirementType.THREE_CONTAINERS;
            case <= 64:
                return RequirementType.FOUR_CONTAINERS;
            case <= 80:
                return RequirementType.FIVE_CONTAINERS;
            case <= 96:
                return RequirementType.SIX_CONTAINERS;
            case <= 112:
                return RequirementType.SEVEN_CONTAINERS;
            default:
                return RequirementType.EIGHT_CONTAINERS;
        }
    }
}
