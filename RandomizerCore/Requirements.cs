using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;

namespace Z2Randomizer.RandomizerCore;

public enum RequirementTarget
{
    ANY,
    EXIT,
    ITEM,
    KEY,
}

public record RequirementRoute(
    /// Entry directions. Null or empty matches all entry directions.
    Direction[]? From,
    /// Exit directions. Null or empty matches all exit directions.
    Direction[]? To
);

public record ComplexRequirement(
    RequirementTarget Target,
    /// Individual requirements (OR clause) - any one is sufficient
    RequirementType[] IndividualRequirements,
    /// Composite requirements (OR of AND clauses) - each inner array must all be satisfied, 
    /// and any inner array being satisfied is enough
    RequirementType[][] CompositeRequirements,
    /// Optional traversal routes. If null or empty, applies to all traversals.
    /// If present, only applies when both enterFrom and exitTo match one of the routes.
    /// Null enterFrom or exitTo matches everything.
    RequirementRoute[]? Routes = null
);

/// Requirements in Disjunctive Normal Form (DNF): a disjunction (OR) of conjunctions (AND).
///
/// The three fields are OR'd together - any one being satisfied causes the whole requirement to pass:
/// - IndividualRequirements: a single OR clause (any one requirement suffices)
/// - CompositeRequirements: multiple OR clauses, each being an AND (all requirements in a clause must be satisfied)
/// - ComplexRequirements: additional DNF structures with target/route filters
///
/// Example satisfaction semantics:
/// - IndividualRequirements=[JUMP, FAIRY]: satisfied if JUMP OR FAIRY is available
/// - CompositeRequirements=[[JUMP, FAIRY], [GLOVE]]: satisfied if (JUMP AND FAIRY) OR GLOVE is available
/// - ComplexRequirements with target=KEY: only checked when querying RequirementTarget.KEY
public class Requirements : IDeterministicHash
{
    public static readonly Requirements NONE = new();
    public static readonly Requirements DEFAULT_PALACE_REQUIREMENTS = new([RequirementType.FAIRY, RequirementType.MAGIC_KEY]);

    private static readonly Dictionary<RequirementType, RequirementType[]> ImplicitRequirements = new()
    {
        {RequirementType.JUMP, [RequirementType.TWO_CONTAINERS] },
        {RequirementType.FAIRY, [RequirementType.FOUR_CONTAINERS] },
        {RequirementType.REFLECT, [RequirementType.FOUR_CONTAINERS] },
        {RequirementType.SPELL, [RequirementType.FOUR_CONTAINERS] },
    };

    // the magic level (not number of containers) for a spell's cost considered in-logic
    private static readonly Dictionary<RequirementType, int> ImplicitMagicLevelRequirements = new()
    {
        { RequirementType.JUMP, 1 },
        { RequirementType.FAIRY, 3 },
        { RequirementType.REFLECT, 3 },
        { RequirementType.SPELL, 3 },
    };

    /// List of OR requirements. Any one requirement in this array is sufficient.
    /// Example: [JUMP, FAIRY] means JUMP OR FAIRY
    public RequirementType[] IndividualRequirements { get; private set; }
    /// OR list of AND clauses. Each inner array is an AND clause (all requirements must be satisfied).
    /// The outer array is an OR of these clauses (any one clause being satisfied is enough).
    /// Example: [[JUMP], [FAIRY, GLOVE]] means (JUMP) OR (FAIRY AND GLOVE)
    public RequirementType[][] CompositeRequirements { get; private set; }
    /// More complex requirements in Disjunctive Normal Form, with optional route filtering.
    /// Each `ComplexRequirement` may contain individual requirements and composite requirements,
    /// and only applies when its target matches and (if specified) its routes match the traversal.
    /// For routes, null or empty `From` or `To` arrays match any direction.
    public ComplexRequirement[] ComplexRequirements { get; private set; }

    public Requirements()
    {
        IndividualRequirements = [];
        CompositeRequirements = [];
        ComplexRequirements = [];
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

    public Requirements(RequirementType[] requirements, RequirementType[][] compositeRequirements, ComplexRequirement[] complexRequirements) : this()
    {
        IndividualRequirements = requirements;
        CompositeRequirements = compositeRequirements;
        ComplexRequirements = complexRequirements;
    }

    public Requirements(string? json)
    {
        Requirements? deserialized = Deserialize(json);
        IndividualRequirements = deserialized?.IndividualRequirements ?? [];
        CompositeRequirements = deserialized?.CompositeRequirements ?? [];
        ComplexRequirements = deserialized?.ComplexRequirements ?? [];
    }

    public Requirements? Deserialize(string? json)
    {
        return JsonSerializer.Deserialize(json ?? "[]", RoomSerializationContext.Default.Requirements);
    }

    /// Check if a requirement is included in any way, composite or individually
    public bool Contains(RequirementType requirement)
    {
        return IndividualRequirements.Contains(requirement)
            || CompositeRequirements.Any(reqs => reqs.Contains(requirement))
            || ComplexRequirements.Any(reqs => reqs.IndividualRequirements.Contains(requirement)
                                            || reqs.CompositeRequirements.Any(inner => inner.Contains(requirement)));
    }

    /// Check if a requirement is included in any form in a complex requirement
    public bool ComplexRequirementsContains(RequirementType requirement)
    {
        return ComplexRequirements.Any(complexRequirement =>
            complexRequirement.IndividualRequirements.Contains(requirement)
            || complexRequirement.CompositeRequirements.Any(inner => inner.Contains(requirement)));
    }

    public bool KeyIsAccessible(
        IReadOnlyCollection<RequirementType> requireables,
        Direction? enterFrom = null,
        Direction? exitTo = null)
    {
        return AreSatisfiedBy(requireables, RequirementTarget.KEY, enterFrom, exitTo);
    }

    /// <summary>
    /// Combines two requirement sets using logical AND, after resolving
    /// complex requirements for the specified target and traversal context.
    /// </summary>
    /// <remarks>
    /// Each individual requirement represents a single alternative. Each composite
    /// requirement represents an AND-clause, with the clauses themselves forming
    /// alternatives (OR).
    ///
    /// The combination computes the Cartesian product of the alternatives from
    /// both inputs, merging each pair into a single AND-clause. Redundant
    /// supersets are removed afterward.
    ///
    /// An empty requirement set represents no restriction and is the identity
    /// for this operation.
    /// </remarks>
    public static Requirements Combine(
        Requirements req1,
        Requirements req2,
        RequirementTarget target = RequirementTarget.ANY,
        Direction? enterFrom = null,
        Direction? exitTo = null)
    {
        ArgumentNullException.ThrowIfNull(req1);
        ArgumentNullException.ThrowIfNull(req2);

        var flattened1 = req1.FlattenComplex(target, enterFrom, exitTo);
        var flattened2 = req2.FlattenComplex(target, enterFrom, exitTo);

        // An empty requirement set imposes no restriction.
        if (flattened1.IsEmpty())
        {
            return flattened2;
        }

        if (flattened2.IsEmpty())
        {
            return flattened1;
        }

        var clauses1 = JoinIndividualAndComposite(flattened1);
        var clauses2 = JoinIndividualAndComposite(flattened2);

        List<RequirementType[]> combinedClauses = [];

        // AND distributes over OR:
        // (A OR B) AND (C OR D)
        // = (A AND C) OR (A AND D) OR (B AND C) OR (B AND D).
        foreach (var clause1 in clauses1)
        {
            foreach (var clause2 in clauses2)
            {
                combinedClauses.Add([.. clause1.Concat(clause2).Distinct()]);
            }
        }

        var simplified = RemoveRedundantComposites(combinedClauses);

        /* we could do this if we really wanted, but individual only exists for convenience and it's no longer convenient here.
        var individual = simplified
            .Where(clause => clause.Length == 1)
            .Select(clause => clause[0])
            .Distinct()
            .ToArray();

        var composite = simplified
            .Where(clause => clause.Length > 1)
            .ToArray();

        return new Requirements(individual, composite);
        */

        return new Requirements([], simplified);
    }

    /// Remove redundant composite requirements.
    /// If we have [JUMP] and [JUMP, FAIRY], the larger clause is redundant because satisfying
    // [JUMP] automatically satisfies [JUMP, FAIRY]. We keep only the smaller clause.
    private static RequirementType[][] RemoveRedundantComposites(List<RequirementType[]> composites)
    {
        if (composites.Count <= 1)
        {
            return composites.ToArray();
        }

        // Sort by size (smallest first) for efficient redundancy check
        var sorted = composites.OrderBy(c => c.Length).ToArray();

        var result = new List<RequirementType[]>();

        for (int i = 0; i < sorted.Length; i++)
        {
            var current = sorted[i];
            bool isRedundant = false;

            // Check if this composite is a superset of any already-added composite
            // If so, it's redundant (the smaller one already covers its requirements)
            foreach (var alreadyAdded in result)
            {
                // If 'alreadyAdded' is a subset of 'current', then 'current' is redundant
                // because 'alreadyAdded' being satisfied means 'current' is also satisfied
                if (alreadyAdded.All(r => current.Contains(r)))
                {
                    isRedundant = true;
                    break;
                }
            }

            if (!isRedundant)
            {
                result.Add(current);
            }
        }

        return result.ToArray();
    }

    /// Returns whether the requirements that apply when exiting the room in the given direction
    /// require a small key (RequirementType.KEY) in some capacity.
    ///
    /// It is assumed there is a locked requirement, which should be checked before calling this.
    ///
    /// If the room has a door, but KEY is not part of any complex requirement,
    /// it will always return true. (Assuming the locked door is needed for everything.)
    public bool ExitRequiresKey(Direction? enterFrom, Direction exitTo)
    {
        if (!ComplexRequirementsContains(RequirementType.KEY))
        {
            return true;
        }
        foreach (ComplexRequirement complexRequirement in ComplexRequirements)
        {
            if (!ComplexRequirementMatches(complexRequirement, RequirementTarget.EXIT, enterFrom, exitTo))
            {
                continue;
            }
            if (complexRequirement.IndividualRequirements.Contains(RequirementType.KEY)
                || complexRequirement.CompositeRequirements.Any(inner => inner.Contains(RequirementType.KEY)))
            {
                return true;
            }
        }
        return false;
    }

    public static bool ComplexRequirementTargetMatches(ComplexRequirement complexRequirement, RequirementTarget target)
    {
        return target is RequirementTarget.ANY
            || complexRequirement.Target is RequirementTarget.ANY
            || target == complexRequirement.Target;
    }

    /// Returns whether any route of a complex requirement matches the given (enterFrom, exitTo)
    /// pair. A route matches when its From array is empty or contains the direction entered from,
    /// and its To array is empty or contains the direction exited to. A complex requirement with
    /// no routes applies to every traversal.
    public static bool ComplexRequirementRouteMatches(ComplexRequirement complexRequirement, Direction? enterFrom, Direction? exitTo)
    {
        RequirementRoute[]? routes = complexRequirement.Routes;
        if (routes is null || routes.Length == 0)
        {
            return true;
        }
        if (enterFrom == null && exitTo == null)
        {
            return true;
        }
        foreach (RequirementRoute route in routes)
        {
            if ((route.From == null || route.From.Length == 0 || enterFrom == null || route.From.Contains(enterFrom.Value)) &&
                (route.To == null || route.To.Length == 0 || exitTo == null || route.To.Contains(exitTo.Value)))
            {
                return true;
            }
        }
        return false;
    }

    /// A complex requirement only applies when both its target matches the query
    /// and one of its routes matches the traversal. Otherwise, they are treated
    /// as if they did not exist.
    public static bool ComplexRequirementMatches(ComplexRequirement req, RequirementTarget target, Direction? enterFrom, Direction? exitTo)
    {
        return ComplexRequirementTargetMatches(req, target)
            && ComplexRequirementRouteMatches(req, enterFrom, exitTo);
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

    public bool AreSatisfiedBy(
        IReadOnlyCollection<RequirementType> requireables,
        RequirementTarget target = RequirementTarget.ANY,
        Direction? enterFrom = null,
        Direction? exitTo = null,
        StatRandomizer? statRoll = null)
    {
        if (IsEmpty())
        {
            return true;
        }

        if (IndividualRequirementMatch(IndividualRequirements))
        {
            return true;
        }

        if (CompositeRequirementMatch(CompositeRequirements))
        {
            return true;
        }

        bool hasApplicableComplexRequirement = false;
        foreach (var complexRequirement in ComplexRequirements)
        {
            if (!ComplexRequirementMatches(complexRequirement, target, enterFrom, exitTo))
            {
                continue;
            }
            if (IndividualRequirementMatch(complexRequirement.IndividualRequirements))
            {
                return true;
            }
            if (CompositeRequirementMatch(complexRequirement.CompositeRequirements))
            {
                return true;
            }

            hasApplicableComplexRequirement = true;
        }

        return IndividualRequirements.Length == 0 
            && CompositeRequirements.Length == 0
            && !hasApplicableComplexRequirement;

        bool StatAdjustedContainerRequirementSatisfied(RequirementType requirement)
        {
            if (statRoll != null
                && ImplicitMagicLevelRequirements.TryGetValue(requirement, out var requiredLevel))
            {
                statRoll.AssertHasRandomized();
                Collectable collectable = requirement.AsCollectable()!.Value;
                var magicCost = statRoll.GetSpellCost(collectable, requiredLevel);
                var containerRequirement = RequirementTypeExtensions.MagicContainerRequirementFromCost(magicCost);
                return requireables.Contains(containerRequirement);
            }
            if (ImplicitRequirements.TryGetValue(requirement, out RequirementType[]? implicitRequirements)
                && implicitRequirements.Any(i => !requireables.Contains(i)))
            {
                return false;
            }
            return true;
        }

        bool IndividualRequirementMatch(RequirementType[] individualRequirements)
        {
            foreach (var requirement in individualRequirements)
            {
                if (requireables.Contains(requirement))
                {
                    if (!StatAdjustedContainerRequirementSatisfied(requirement))
                    {
                        continue;
                    }
                    return true;
                }
            }
            return false;
        }

        bool CompositeRequirementMatch(RequirementType[][] compositeRequirements)
        {
            foreach (var compositeRequirement in compositeRequirements)
            {
                if (compositeRequirement.Any(i => !requireables.Contains(i)))
                {
                    continue;
                }
                if (compositeRequirement.Any(i => !StatAdjustedContainerRequirementSatisfied(i)))
                {
                    continue;
                }
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Returns a copy of these requirements with the given requirement added as a hard requirement,
    /// meaning it is required in every way these requirements can be satisfied.
    /// </summary>
    public Requirements WithHardRequirement(RequirementType requirement)
    {
        Requirements newRequirements = new();
        //if no requirements return a single requirement of the type
        if (IsEmpty())
        {
            newRequirements.IndividualRequirements = [requirement];
            return newRequirements;
        }
        newRequirements.CompositeRequirements = WithHardRequirementNewComposite(IndividualRequirements, CompositeRequirements, requirement);
        //complex requirements must also contain the hard requirement
        newRequirements.ComplexRequirements = ComplexRequirements
            .Select(complexRequirement => complexRequirement with
            {
                IndividualRequirements = [],
                CompositeRequirements = WithHardRequirementNewComposite(complexRequirement.IndividualRequirements, complexRequirement.CompositeRequirements, requirement),
            })
            .ToArray();
        return newRequirements;
    }

    private static RequirementType[][] WithHardRequirementNewComposite(RequirementType[] individualRequirements, RequirementType[][] compositeRequirements, RequirementType requirement)
    {
        RequirementType[][] newCompositeRequirements =
        [
            //all individual requirements become composite requirements containing the type
            ..individualRequirements.Select(i => new[] { i, requirement }),
            //all composite requirements not containing the type now contain the type
            ..compositeRequirements.Select(c => c.Append(requirement).ToArray()),
        ];
        if (newCompositeRequirements.Length == 0)
        {
            newCompositeRequirements = [[requirement]];
        }
        return newCompositeRequirements;
    }

    public Requirements Without(RequirementType requirementToRemove)
    {
        return Without([requirementToRemove]);
    }

    /// <summary>
    /// Returns a copy of these requirements with the given requirements removed from all individual,
    /// composite, and complex requirements.
    /// </summary>
    public Requirements Without(IEnumerable<RequirementType> requirementsToRemove)
    {
        RequirementType[] requirementsToRemoveArray = requirementsToRemove.ToArray();
        Requirements newRequirements = new();
        var (newIndividualRequirements, newCompositeRequirements) = FilterRequirements(IndividualRequirements, CompositeRequirements, requirementsToRemoveArray);
        newRequirements.IndividualRequirements = newIndividualRequirements;
        newRequirements.CompositeRequirements = newCompositeRequirements;

        List<ComplexRequirement> newComplexRequirements = [];
        foreach (ComplexRequirement complexRequirement in ComplexRequirements)
        {
            var (complexIndividualRequirements, complexCompositeRequirements) = FilterRequirements(
                complexRequirement.IndividualRequirements,
                complexRequirement.CompositeRequirements,
                requirementsToRemoveArray);
            if (complexIndividualRequirements.Length + complexCompositeRequirements.Length > 0)
            {
                newComplexRequirements.Add(complexRequirement with
                {
                    IndividualRequirements = complexIndividualRequirements,
                    CompositeRequirements = complexCompositeRequirements,
                });
            }
        }
        newRequirements.ComplexRequirements = newComplexRequirements.ToArray();
        return newRequirements;
    }

    private static (RequirementType[] IndividualRequirements, RequirementType[][] CompositeRequirements) FilterRequirements(
        RequirementType[] individualRequirements,
        RequirementType[][] compositeRequirements,
        IReadOnlyCollection<RequirementType> requirementsToRemove)
    {
        List<RequirementType> newIndividualRequirements = [];
        List<RequirementType[]> newCompositeRequirements = [];
        foreach (RequirementType requirement in individualRequirements)
        {
            if (!requirementsToRemove.Contains(requirement))
            {
                newIndividualRequirements.Add(requirement);
            }
        }
        foreach (RequirementType[] compositeRequirement in compositeRequirements)
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
        return (newIndividualRequirements.ToArray(), newCompositeRequirements.ToArray());
    }

    /// <summary>
    /// Returns whether the given requireable must be present in every way these requirements can be
    /// satisfied.
    /// </summary>
    public bool HasHardRequirement(RequirementType requireable)
    {
        if (IsEmpty())
        {
            return false;
        }
        if (IndividualRequirements.Length + CompositeRequirements.Length > 0
            && !HasHardRequirement(IndividualRequirements, CompositeRequirements, requireable))
        {
            return false;
        }
        foreach (ComplexRequirement complexRequirement in ComplexRequirements)
        {
            if (!HasHardRequirement(complexRequirement.IndividualRequirements, complexRequirement.CompositeRequirements, requireable))
            {
                return false;
            }
        }
        return true;
    }

    private static bool HasHardRequirement(RequirementType[] individualRequirements, RequirementType[][] compositeRequirements, RequirementType requireable)
    {
        if (individualRequirements.Length == 0 && compositeRequirements.Length == 0)
        {
            return false;
        }
        //any individual requirement that is not the requireable could satisfy the set without it
        if (individualRequirements.Any(requirement => requirement != requireable))
        {
            return false;
        }
        //any composite requirement that does not include the requireable could satisfy the set without it
        foreach (RequirementType[] compositeRequirement in compositeRequirements)
        {
            if (!compositeRequirement.Contains(requireable))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsEmpty()
    {
        return IndividualRequirements.Length == 0 && CompositeRequirements.Length == 0 && ComplexRequirements.Length == 0;
    }

    // Convert each representation into DNF clauses.
    // Every individual requirement is a one-item alternative.
    public static RequirementType[][] JoinIndividualAndComposite(Requirements requirements)
    {
        if (!requirements.IndividualRequirements.Any())
        {
            return requirements.CompositeRequirements;
        }
        return [
            .. requirements.IndividualRequirements.Select(r => new[] { r }),
            .. requirements.CompositeRequirements,
        ];
    }

    /// Flatten ComplexRequirements into Individual and Composite requirements, given the current context.
    /// ComplexRequirements with routes only apply when traversing in specific directions.
    /// ComplexRequirements with non-ANY targets only apply for specific operations (KEY, EXIT, ITEM).
    ///
    /// Returns a new Requirements object. The current Requirements are untouched.
    public Requirements FlattenComplex(RequirementTarget target = RequirementTarget.ANY, Direction? enterFrom = null, Direction? exitTo = null)
    {
        if (ComplexRequirements.Length == 0)
        {
            return this;
        }

        // Collect all individual and composite requirements from ComplexRequirements that apply
        var combinedIndividual = new HashSet<RequirementType>(IndividualRequirements);
        var combinedComposite = new List<RequirementType[]>(CompositeRequirements);

        foreach (var complex in ComplexRequirements)
        {
            if (!ComplexRequirementMatches(complex, target, enterFrom, exitTo))
            {
                continue;
            }

            foreach (var req in complex.IndividualRequirements)
            {
                combinedIndividual.Add(req);
            }

            foreach (var composite in complex.CompositeRequirements)
            {
                combinedComposite.Add(composite);
            }
        }

        var simplifiedComposite = RemoveRedundantComposites(combinedComposite);
        return new Requirements([.. combinedIndividual], [.. simplifiedComposite]);
    }
}

public class RequirementsJsonConverter : JsonConverter<Requirements>
{
    public override Requirements Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            return new Requirements();
        List<RequirementType> individual = [];
        List<RequirementType[]> composite = [];
        List<ComplexRequirement> complex = [];
        using var doc = JsonDocument.ParseValue(ref reader);
        foreach (var req in doc.RootElement.EnumerateArray())
        {
            switch (req.ValueKind)
            {
                case JsonValueKind.String:
                    var indReq = ParseIndividual(req);
                    individual.Add(indReq);
                    break;
                case JsonValueKind.Array:
                    var compositeReq = ParseComposite(req);
                    composite.Add(compositeReq);
                    break;
                case JsonValueKind.Object:
                    var complexReq = ParseComplex(req);
                    complex.Add(complexReq);
                    break;
            }
        }
        return new Requirements(
            individual.ToArray(),
            composite.ToArray(),
            complex.ToArray());
    }

    private static RequirementType ParseIndividual(JsonElement req)
    {
        return Enum.Parse<RequirementType>(req.GetString()!, ignoreCase: true);
    }

    private static RequirementType[] ParseComposite(JsonElement req)
    {
        List<RequirementType> newComp = [];
        var jsonReqs = req.EnumerateArray();
        var enumReqs = jsonReqs.Select(innerReq => Enum.Parse<RequirementType>(innerReq.GetString()!, ignoreCase: true));
        newComp.AddRange(enumReqs);
        return newComp.ToArray();
    }

    private static ComplexRequirement ParseComplex(JsonElement complexReq)
    {
        var parsed = JsonSerializer.Deserialize(complexReq.GetRawText(), RoomSerializationContext.Default.ComplexRequirement);
        if (parsed == null)
        {
            throw new JsonException("Unable to parse ComplexRequirement");
        }

        var individual = new List<RequirementType>();
        var composite = new List<RequirementType[]>();

        foreach (var innerReq in complexReq.GetProperty("requires").EnumerateArray())
        {
            switch (innerReq.ValueKind)
            {
                case JsonValueKind.String:
                    var indReq = ParseIndividual(innerReq);
                    individual.Add(indReq);
                    break;
                case JsonValueKind.Array:
                    var compositeReq = ParseComposite(innerReq);
                    composite.Add(compositeReq);
                    break;
            }
        }

        return parsed with
        {
            IndividualRequirements = individual.ToArray(),
            CompositeRequirements = composite.ToArray()
        };
    }

    public override void Write(Utf8JsonWriter writer, Requirements value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value?.Serialize() ?? "[]");
    }
}
