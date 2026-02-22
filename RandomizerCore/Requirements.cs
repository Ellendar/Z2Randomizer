using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Z2Randomizer.RandomizerCore.Sidescroll;

namespace Z2Randomizer.RandomizerCore;

public class Requirements
{
    public static readonly Requirements NONE = new();

    private static readonly Dictionary<RequirementType, RequirementType[]> ImplicitRequirements = new()
    {
        {RequirementType.JUMP, [RequirementType.TWO_CONTAINERS] },
        {RequirementType.FAIRY, [RequirementType.FOUR_CONTAINERS] },
        {RequirementType.REFLECT, [RequirementType.FOUR_CONTAINERS] },
        {RequirementType.SPELL, [RequirementType.FOUR_CONTAINERS] },
    };

    public RequirementType[] IndividualRequirements { get; private set; }
    public RequirementType[][] CompositeRequirements { get; private set; }

    public Requirements()
    {
        IndividualRequirements = [];
        CompositeRequirements = [];
    }

    public Requirements(RequirementType[] requirements) : this()
    {
        IndividualRequirements = requirements;
    }

    public Requirements(RequirementType[] requirements, RequirementType[][] compositeRequirements) : this()
    {
        IndividualRequirements = requirements;
        CompositeRequirements = compositeRequirements;
    }

    public Requirements(string? json)
    {
        Requirements? deserialized = Deserialize(json);
        IndividualRequirements = deserialized?.IndividualRequirements ?? [];
        CompositeRequirements = deserialized?.CompositeRequirements ?? [];
    }

    public Requirements? Deserialize(string? json)
    {
        return JsonSerializer.Deserialize(json ?? "[]", RoomSerializationContext.Default.Requirements);
    }
    
    public string Serialize()
    {
        StringBuilder sb = new();
        sb.Append('[');
        foreach (RequirementType requirement in IndividualRequirements)
        {
            sb.Append('"');
            sb.Append(requirement.ToString());
            sb.Append('"');
            sb.Append(',');
        }
        foreach (RequirementType[] compositeRequirement in CompositeRequirements)
        {
            sb.Append('[');
            foreach (RequirementType component in compositeRequirement)
            {
                sb.Append('"');
                sb.Append(component.ToString());
                sb.Append('"');
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
        }
        if (sb.Length > 1 && sb[^1] != ']')
        {
            sb.Remove(sb.Length - 1, 1);
        }
        sb.Append(']');

        return sb.ToString();
    }

    public override string ToString()
    {
        return Serialize();
    }

    public bool AreSatisfiedBy(IEnumerable<RequirementType> requireables, bool enforceImplicitRequirements = true)
    {
        if(IndividualRequirements.Length + CompositeRequirements.Length == 0)
        {
            return true;
        }
        var individualRequirementsSatisfied = false;
        var requirementTypes = requireables as RequirementType[] ?? requireables.ToArray();
        foreach (var requirement in IndividualRequirements)
        {
            if (requirementTypes.Contains(requirement))
            {
                individualRequirementsSatisfied = true;
                if (enforceImplicitRequirements && ImplicitRequirements.ContainsKey(requirement))
                { 
                    foreach(RequirementType implicitRequirement in ImplicitRequirements[requirement])
                    {
                        if(!requirementTypes.Contains(implicitRequirement))
                        {
                            individualRequirementsSatisfied = false;
                            continue;
                        }
                    }
                }
                if (individualRequirementsSatisfied)
                {
                    break;
                }
            }
        }

        bool compositeRequirementSatisfied = false;
        foreach (RequirementType[] compositeRequirement in CompositeRequirements)
        {
            if(compositeRequirement.All(i => requireables.Contains(i)))
            {
                compositeRequirementSatisfied = true;
            }
            if(enforceImplicitRequirements)
            {
                foreach(RequirementType component in compositeRequirement)
                {
                    if (ImplicitRequirements.TryGetValue(component, out RequirementType[]? implicitRequirements) 
                        && implicitRequirements.Any(i => !requireables.Contains(i)))
                    {
                        compositeRequirementSatisfied = false;
                        break;
                    }
                }
            }
            if(compositeRequirementSatisfied == true)
            {
                break;
            }
        }
        return individualRequirementsSatisfied || compositeRequirementSatisfied;
    }

    public Requirements WithHardRequirement(RequirementType requirement)
    {
        Requirements newRequirements = new();
        //if no requirements return a single requirement of the type
        if(IndividualRequirements.Length == 0 && CompositeRequirements.Length == 0)
        {
            newRequirements.IndividualRequirements = [requirement];
            return newRequirements;
        }
        newRequirements.CompositeRequirements = new RequirementType[CompositeRequirements.Length + IndividualRequirements.Length][];
        //all individual requirements become composite requirements containing the type
        for(int i = 0; i < IndividualRequirements.Length; i++)
        {
            newRequirements.CompositeRequirements[i] = [IndividualRequirements[i], requirement];
        }
        //all composite requirements not containing the type now contain the type
        for (int i = 0; i < CompositeRequirements.Length; i++)
        {
            newRequirements.CompositeRequirements[i + IndividualRequirements.Length] = [.. CompositeRequirements[i], requirement];
        }
        return newRequirements;
    }

    public Requirements Without(RequirementType requirementToRemove)
    {
        return Without([requirementToRemove]);
    }

    public Requirements Without(IEnumerable<RequirementType> requirementsToRemove)
    {
        Requirements newRequirements = new();
        List<RequirementType> newIndividualRequirements = [];
        List<RequirementType[]> newCompositeRequirements = [];
        foreach (RequirementType requirement in IndividualRequirements)
        {
            if (!requirementsToRemove.Contains(requirement))
            {
                newIndividualRequirements.Add(requirement);
            }
        }
        foreach (RequirementType[] compositeRequirement in CompositeRequirements)
        {
            List<RequirementType> newCompositeRequirement = [];
            foreach (RequirementType requirementComponent in compositeRequirement)
            {
                if (!requirementsToRemove.Contains(requirementComponent))
                {
                    newCompositeRequirement.Add(requirementComponent);
                }
            }
            if (newCompositeRequirement.Count == 1)
            {
                newIndividualRequirements.Add(newCompositeRequirement[0]);
            }
            else if (newCompositeRequirement.Count > 1)
            {
                newCompositeRequirements.Add(newCompositeRequirement.ToArray());
            }
        }
        newRequirements.IndividualRequirements = newIndividualRequirements.ToArray();
        newRequirements.CompositeRequirements = newCompositeRequirements.ToArray();
        return newRequirements;
    }

    public bool HasHardRequirement(RequirementType requireable)
    {
        if(IndividualRequirements.Length + CompositeRequirements.Length == 0)
        {
            return false;
        }
        if (IndividualRequirements.Any(requirement => requirement != requireable))
        {
            return false;
        }
        foreach (RequirementType[] compositeRequirement in CompositeRequirements)
        {
            var containsRequireable = false;
            foreach (RequirementType requirement in compositeRequirement)
            {
                if (requirement != requireable)
                {
                    containsRequireable = true;
                }
            }
            if(!containsRequireable)
            {
                return false;
            }
        }
        return true;
    }
}

public class RequirementsJsonConverter : JsonConverter<Requirements>
{
    public override Requirements Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            return new Requirements();
        List<RequirementType> individualReqs = [];
        List<List<RequirementType>> compositeReqs = [];
        var doc = JsonDocument.ParseValue(ref reader);
        foreach (var req in doc.RootElement.EnumerateArray())
        {
            switch (req.ValueKind)
            {
                case JsonValueKind.String:
                    individualReqs.Add((RequirementType)Enum.Parse(typeof(RequirementType), req.ToString()));
                    break;
                case JsonValueKind.Array:
                    List<RequirementType> newComp = [];
                    compositeReqs.Add(newComp);
                    var subArray = req.EnumerateArray();
                    var comps = subArray.Select(comp =>
                        (RequirementType)Enum.Parse(typeof(RequirementType), comp.ToString()));
                    newComp.AddRange(comps);
                    break;
            }
        }
        return new Requirements(individualReqs.ToArray(),
            compositeReqs.Select(i => i.ToArray()).ToArray());
    }

    public override void Write(Utf8JsonWriter writer, Requirements value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value?.Serialize() ?? "[]");
    }
}
