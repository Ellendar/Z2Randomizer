using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreSourceGenerator;

/// <summary>
/// Emits the shared <c>ConfigFlag</c> enum into RandomizerCore. It is pure data
/// (names of config properties, no references to UI types), so it lives next to
/// the config it names and is read by CrossPlatformUI's generator, which builds
/// the UI-side <c>FlagRegistry</c>/<c>FlagControlFactory</c> around it.
/// <para>
/// The member set is the union of two things a flag can be named by:
/// </para>
/// <list type="bullet">
/// <item>Every <b>control candidate</b> the CrossPlatformUI generator would render:
/// a public get/set property (i.e. a <c>[Reactive]</c> field) of type <c>bool</c>,
/// <c>bool?</c>, an enum, or an <c>int</c>/<c>int?</c> carrying both
/// <c>[Minimum]</c> and <c>[Maximum]</c>, and not marked
/// <c>[IgnoreInFlagControls]</c>. A <c>[UiNumericPair]</c> end is its own member:
/// the two ends are separate boxes everywhere (the tabs and the diff both name them
/// individually), so no merged member exists.</item>
/// <item>Every <b>serialized field</b> (anything not <c>[IgnoreInFlags]</c>), so the
/// generated <c>Diff</c> can name each changed field by <c>ConfigFlag</c> rather
/// than a string. This is what covers every numeric (a serialized <c>int</c>/<c>int?</c>
/// with bounds), so numerics need no separate handling here.</item>
/// </list>
/// <para>
/// A property that is a control candidate but ended up with no registry entry and
/// no enum member would fail silently, so both sets are unioned here. A serialized
/// field with no member would instead fail the build (the <c>Diff</c> would not
/// compile), which is the point of naming it by the enum.
/// </para>
/// <para>
/// Members are sorted ordinally so the enum is byte-for-byte stable. Values are
/// never persisted, so reordering is harmless, but keeping it sorted keeps the
/// emitted file diff-free.
/// </para>
/// </summary>
[Generator]
public class ConfigFlagGenerator : IIncrementalGenerator
{
    private const string FlagsNamespace = "Z2Randomizer.RandomizerCore.Flags";
    private const string FlagSerialize = "FlagSerializeAttribute";
    private const string ReactiveAttribute = "ReactiveAttribute";
    private const string IgnoreInFlagControls = "IgnoreInFlagControlsAttribute";
    private const string IgnoreInFlags = "IgnoreInFlagsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsPartialClass(s),
                transform: static (ctx, _) => GetFlagSerializeClass(ctx))
            .Where(static c => c is not null);

        context.RegisterSourceOutput(
            classDeclarations.Collect(),
            static (spc, classes) => Execute(classes, spc));
    }

    private static bool IsPartialClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { Modifiers: var modifiers } &&
               modifiers.Any(m => m.ValueText == "partial");
    }

    private static INamedTypeSymbol? GetFlagSerializeClass(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol =
            context.SemanticModel.GetDeclaredSymbol(classDeclaration)
            as INamedTypeSymbol;

        if (classSymbol == null || !HasFlagSerializeAttribute(classSymbol))
            return null;

        return classSymbol;
    }

    private static bool HasFlagSerializeAttribute(INamedTypeSymbol classSymbol)
    {
        return classSymbol.GetAttributes()
            .Any(attr =>
                Matches(attr.AttributeClass?.Name, FlagSerialize));
    }

    private static void Execute(
        ImmutableArray<INamedTypeSymbol?> classes,
        SourceProductionContext context)
    {
        var names = CollectFlagNames(classes);

        if (names.Count == 0)
            return;

        context.AddSource("ConfigFlag.g.cs", GenerateFlagEnum(names));
    }

    private static List<string> CollectFlagNames(
        IEnumerable<INamedTypeSymbol> classes)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);

        foreach (var classSymbol in classes)
        {
            foreach (var field in classSymbol.GetMembers().OfType<IFieldSymbol>())
            {
                // Only a [Reactive] field becomes a public get/set property, which
                // is what a generated control reads.
                if (!HasAttribute(field, ReactiveAttribute))
                    continue;

                var propertyName = GetPropertyName(field.Name);

                // Every serialized field needs a member: the generated Diff names
                // each field by ConfigFlag. Numerics are always serialized, so this
                // is what covers them (and both ends of a Min/Max pair).
                if (!HasAttribute(field, IgnoreInFlags))
                    names.Add(propertyName);

                if (HasAttribute(field, IgnoreInFlagControls))
                    continue;

                // A [Reactive] bool/bool?/enum is also a control candidate. It is
                // usually serialized too, but a control marked [IgnoreInFlags] (e.g.
                // a music toggle) is not, so the union is what keeps the enum
                // complete.
                if (field.Type.SpecialType == SpecialType.System_Boolean ||
                    IsNullableBool(field.Type) ||
                    field.Type.TypeKind == TypeKind.Enum)
                {
                    names.Add(propertyName);
                }
            }
        }

        var ordered = new List<string>(names);
        ordered.Sort(StringComparer.Ordinal);
        return ordered;
    }

    private static bool IsNullableBool(ITypeSymbol type)
    {
        return type is INamedTypeSymbol
        {
            OriginalDefinition.SpecialType: SpecialType.System_Nullable_T
        } nullable &&
        nullable.TypeArguments[0].SpecialType ==
        SpecialType.System_Boolean;
    }

    private static bool HasAttribute(IFieldSymbol field, string name)
    {
        return field.GetAttributes()
            .Any(a => Matches(a.AttributeClass?.Name, name));
    }

    private static bool Matches(string? actual, string name)
    {
        if (actual == name)
            return true;

        // "FooAttribute" also matches "Foo"; the guard keeps a name shorter than
        // the suffix from producing a negative length.
        return name.Length > "Attribute".Length &&
               actual == name.Substring(
                   0,
                   name.Length - "Attribute".Length);
    }

    private static string GetPropertyName(string fieldName)
    {
        // Remove leading underscore and capitalize first letter
        var name = fieldName.StartsWith("_") ? fieldName.Substring(1) : fieldName;
        return char.ToUpper(name[0]) + name.Substring(1);
    }

    private static string GenerateFlagEnum(List<string> names)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {FlagsNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine(
            "/// Every config flag that has a generated control, named exactly after");
        sb.AppendLine("/// its config property.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public enum ConfigFlag");
        sb.AppendLine("{");

        foreach (var name in names)
        {
            sb.AppendLine($"    {name},");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}
