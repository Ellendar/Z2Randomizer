using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD.Tools.BCLExtensions.CollectionsRelated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Z2Randomizer.Core;

public class Requirements
{
    internal bool IndividualRequirementsAreAnds { get; }
    private RequirementType[] individualRequirements;
    private RequirementType[][] compositeRequirements;

    public Requirements(bool individualRequirementsAreAnds = false)
    {
        individualRequirements = [];
        compositeRequirements = [];
        IndividualRequirementsAreAnds = individualRequirementsAreAnds;
    }

    public Requirements(RequirementType[] requirements, bool individualRequirementsAreAnds = false) : this(individualRequirementsAreAnds)
    {
        individualRequirements = requirements;
    }

    public Requirements(RequirementType[] requirements, RequirementType[][] compositeRequirements,
        bool individualRequirementsAreAnds = false) : this(individualRequirementsAreAnds)
    {
        individualRequirements = requirements;
        this.compositeRequirements = compositeRequirements;
    }

    public Requirements(string json)
    {
        if(json == null)
        {
            this.individualRequirements = [];
            compositeRequirements = [];
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

        this.individualRequirements = individualRequirements.ToArray();
        compositeRequirements = compositeReqirements.Select(i => i.ToArray()).ToArray();
    }

    public Requirements AddHardRequirement(RequirementType requirement)
    {
        Requirements newRequirements = new();
        //if no requirements return a single requirement of the type
        if(individualRequirements.Length == 0 && compositeRequirements.Length == 0)
        {
            newRequirements.individualRequirements = [requirement];
            return newRequirements;
        }
        newRequirements.compositeRequirements = new RequirementType[compositeRequirements.Length + individualRequirements.Length][];
        //all individual requirements become composite requirements containing the type
        for(int i = 0; i < individualRequirements.Length; i++)
        {
            newRequirements.compositeRequirements[i] = [individualRequirements[i], requirement];
        }
        //all composite requirements not containing the type now contain the type
        for (int i = 0; i < compositeRequirements.Length; i++)
        {
            newRequirements.compositeRequirements[i + individualRequirements.Length] = [.. compositeRequirements[i], requirement];
        }
        return newRequirements;
    }

    public string Serialize()
    {
        StringBuilder sb = new();
        sb.Append("[");
        for (int i = 0; i < individualRequirements.Length; i++)
        {
            sb.Append('"');
            sb.Append(individualRequirements[i].ToString());
            sb.Append('"');
            sb.Append(",");
        }
        for (int i = 0; i < compositeRequirements.Length; i++)
        {
            sb.Append("[");
            for (int j = 0; j < compositeRequirements[i].Length; j++)
            {
                sb.Append('"');
                sb.Append(compositeRequirements[i][j].ToString());
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

    public override string ToString()
    {
        return Serialize();
    }

    public bool AreSatisfiedBy(IEnumerable<RequirementType> requireables)
    {

        var individualRequirementsSatisfied = false;
        var requirementTypes = requireables as RequirementType[] ?? requireables.ToArray();
        foreach (var requirement in individualRequirements)
        {
            if (requirementTypes.Contains(requirement) && !IndividualRequirementsAreAnds)
            {
                individualRequirementsSatisfied = true;
                break;
            }

            if (IndividualRequirementsAreAnds)
            {
                return false;
            }
        }
        if (individualRequirements.Length > 0 && !individualRequirementsSatisfied)
        {
            return false;
        }

        var compositeRequirementSatisfied =
            compositeRequirements.Length == 0 || compositeRequirements.Any(compositeRequirement =>
            compositeRequirement.All(i => requirementTypes.Contains(i)));
        return (individualRequirements.Length > 0 && individualRequirementsSatisfied) || compositeRequirementSatisfied;
    }

    public bool HasHardRequirement(RequirementType requireable)
    {
        foreach (RequirementType requirement in individualRequirements)
        {
            if (requirement != requireable)
            {
                return false;
            }
        }
        foreach (RequirementType[] compositeRequirement in compositeRequirements)
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
