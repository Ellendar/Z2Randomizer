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
        IndividualRequirements = new RequirementType[] { };
        CompositeRequirements = new RequirementType[][] { };
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

    public Requirements(string json)
    {
        if(json == null)
        {
            IndividualRequirements = new RequirementType[] { };
            CompositeRequirements = new RequirementType[][] { };
            return;
        }
        dynamic requirementsDynamic = JsonConvert.DeserializeObject(json);
        List<List<RequirementType>> compositeReqirements = new();
        List<RequirementType> individualRequirements = new();
        foreach(dynamic requirement in requirementsDynamic)
        {
            if (requirement is JArray)
            {
                List<RequirementType> compositeRequirementComponents = new();
                foreach (dynamic subRequirement in requirement)
                {
                    compositeRequirementComponents.Add((RequirementType)Enum.Parse(typeof(RequirementType), (string)subRequirement));
                }
                compositeReqirements.Add(compositeRequirementComponents);
            }
            else
            {
                individualRequirements.Add((RequirementType)Enum.Parse(typeof(RequirementType), (string)requirement));
            }
        }

        IndividualRequirements = individualRequirements.ToArray();
        CompositeRequirements = compositeReqirements.Select(i => i.ToArray()).ToArray();
    }

    public string Serialize()
    {
        StringBuilder sb = new();
        sb.Append("[");
        for (int i = 0; i < IndividualRequirements.Length; i++)
        {
            sb.Append('"');
            sb.Append(IndividualRequirements[i].ToString());
            sb.Append('"');
            sb.Append(",");
        }
        for (int i = 0; i < CompositeRequirements.Length; i++)
        {
            sb.Append("[");
            for (int j = 0; j < CompositeRequirements[i].Length; j++)
            {
                sb.Append('"');
                sb.Append(CompositeRequirements[i][j].ToString());
                sb.Append('"');
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");
        }
        if (sb.Length > 1 && sb[sb.Length - 1] != ']')
        {
            sb.Remove(sb.Length - 1, 1);
        }
        sb.Append("]");

        return sb.ToString();
    }

    public bool AreSatisfiedBy(IEnumerable<RequirementType> requireables)
    {

        bool individualRequirementsSatisfied = false;
        foreach (RequirementType requirement in IndividualRequirements)
        {
            if (requireables.Contains(requirement) && !IndividualRequirementsAreAnds)
            {
                individualRequirementsSatisfied = true;
                break;
            }
            else if(IndividualRequirementsAreAnds)
            {
                return false;
            }
        }
        if(IndividualRequirements.Length > 0 && !individualRequirementsSatisfied)
        {
            return false;
        }
        foreach (RequirementType[] compositeRequirement in CompositeRequirements)
        {
            foreach (RequirementType requirement in compositeRequirement)
            {
                if (!requireables.Contains(requirement))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool HasHardRequirement(RequirementType requireable)
    {
        foreach (RequirementType requirement in IndividualRequirements)
        {
            if (requirement != requireable)
            {
                return false;
            }
        }
        foreach (RequirementType[] compositeRequirement in CompositeRequirements)
        {
            bool containsRequireable = false;
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
    public override Requirements ReadJson(JsonReader reader, Type objectType, Requirements existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return new Requirements(reader.ReadAsString());
    }

    public override void WriteJson(JsonWriter writer, Requirements value, JsonSerializer serializer)
    {
        writer.WriteRaw(value.Serialize());
    }
}
