using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Z2Randomizer.Core;

public class Requirements
{
    internal bool IndividualRequirementsAreAnds { get; }
    public RequirementType[] IndividualRequirements { get; private set; }
    public RequirementType[][] CompositeRequirements { get; private set; }

    public Requirements(bool individualRequirementsAreAnds = false)
    {
        IndividualRequirements = [];
        CompositeRequirements = [];
        IndividualRequirementsAreAnds = individualRequirementsAreAnds;
    }

    public Requirements(RequirementType[] requirements, bool individualRequirementsAreAnds = false) : this(individualRequirementsAreAnds)
    {
        IndividualRequirements = requirements;
    }

    public Requirements(RequirementType[] requirements, RequirementType[][] compositeRequirements,
        bool individualRequirementsAreAnds = false) : this(individualRequirementsAreAnds)
    {
        IndividualRequirements = requirements;
        CompositeRequirements = compositeRequirements;
    }

    public Requirements(string? json)
    {
        var n = Deserialize(json);
        IndividualRequirements = n?.IndividualRequirements ?? [];
        CompositeRequirements = n?.CompositeRequirements ?? [];
    }

    public Requirements? Deserialize(string? json)
    {
        return JsonConvert.DeserializeObject<Requirements>(json ?? "[]");
    }
    
    public string Serialize()
    {
        StringBuilder sb = new();
        sb.Append('[');
        foreach (var t in IndividualRequirements)
        {
            sb.Append('"');
            sb.Append(t.ToString());
            sb.Append('"');
            sb.Append(',');
        }
        foreach (var t in CompositeRequirements)
        {
            sb.Append('[');
            foreach (var t1 in t)
            {
                sb.Append('"');
                sb.Append(t1.ToString());
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

    public bool AreSatisfiedBy(IEnumerable<RequirementType> requireables)
    {

        var individualRequirementsSatisfied = false;
        var requirementTypes = requireables as RequirementType[] ?? requireables.ToArray();
        foreach (var requirement in IndividualRequirements)
        {
            if (requirementTypes.Contains(requirement) && !IndividualRequirementsAreAnds)
            {
                individualRequirementsSatisfied = true;
                break;
            }

            if(IndividualRequirementsAreAnds)
            {
                return false;
            }
        }
        if(IndividualRequirements.Length > 0 && !individualRequirementsSatisfied)
        {
            return false;
        }

        var compositeRequirementSatisfied = 
            CompositeRequirements.Length == 0 || CompositeRequirements.Any(compositeRequirement =>
            compositeRequirement.All(i => requirementTypes.Contains(i)));
        return compositeRequirementSatisfied;
    }

    public bool HasHardRequirement(RequirementType requireable)
    {
        if (IndividualRequirements.Any(requirement => requirement != requireable))
        {
            return false;
        }
        foreach (var compositeRequirement in CompositeRequirements)
        {
            var containsRequireable = false;
            foreach (var requirement in compositeRequirement)
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
    public override Requirements ReadJson(JsonReader reader, Type objectType, Requirements? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jToken = JToken.Load(reader);
        if (jToken.Type != JTokenType.Array)
            return new Requirements();
        List<RequirementType> individualReqs = [];
        List<List<RequirementType>> compositeReqs = []; 
        foreach (var req in jToken.ToObject<List<JToken>>()!)
        {
            switch (req.Type)
            {
                case JTokenType.String:
                    individualReqs.Add((RequirementType)Enum.Parse(typeof(RequirementType), req.ToString()));
                    break;
                case JTokenType.Array:
                    List<RequirementType> newComp = [];
                    compositeReqs.Add(newComp);
                    var comps = req.ToObject<List<JToken>>()!.Select(comp =>
                        (RequirementType)Enum.Parse(typeof(RequirementType), comp.ToString()));
                    newComp.AddRange(comps);
                    break;
            }
        }
        return new Requirements(individualReqs.ToArray(),
            compositeReqs.Select(i => i.ToArray()).ToArray());
    }

    public override void WriteJson(JsonWriter writer, Requirements? value, JsonSerializer serializer)
    {
        writer.WriteRaw(value?.Serialize() ?? "[]");
    }
}
