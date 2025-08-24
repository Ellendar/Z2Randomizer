using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Z2Randomizer.RandomizerCore;

[AttributeUsage(AttributeTargets.Class)]
public class FlagSerializeAttribute : Attribute
{
}

/**
 * We don't need to bring in ReactiveUI to the base RandomizerCore if we just make our own source generator.
 * To keep the usage similar to the original ReactiveUI SourceGenerator, I kept the name `Reactive` for the attribute
 * in case we bail on this idea later.
 */
public class ReactiveAttribute : Attribute
{

}

[Generator]
public class ReactiveObjectSerializeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get all partial classes that have FlagSerializeAttribute and implement INotifyPropertyChanged
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsPartialClass(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation to get the final output
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static bool IsPartialClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { Modifiers: var modifiers } &&
               modifiers.Any(m => m.ValueText == "partial");
    }

    private static ClassGenerationInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (classSymbol == null ||
            !HasFlagSerializeAttribute(classSymbol) ||
            !ImplementsINotifyPropertyChanged(classSymbol))
            return null;

        var classInfo = new ClassGenerationInfo
        {
            ClassName = classSymbol.Name,
            Namespace = classSymbol.ContainingNamespace?.ToDisplayString()
        };


        // Get all fields with ReactiveAttribute for reactive property generation
        var reactiveFields = classSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(HasReactiveAttribute)
            .Select(f =>
            {
                // Look up the source code for the declaration to find if it has a default value
                var equalsSyntax = f.DeclaringSyntaxReferences[0].GetSyntax() switch
                {
                    PropertyDeclarationSyntax property => property.Initializer,
                    VariableDeclaratorSyntax variable => variable.Initializer,
                    _ => throw new Exception("Unknown declaration syntax")
                };

                return new ReactiveFieldInfo
                {
                    FieldName = f.Name,
                    FieldType = f.Type.ToDisplayString(),
                    PropertyName = GetPropertyName(f.Name),
                    DefaultValue = equalsSyntax?.Value.ToString(),
                    PassThroughAttributes = GetPassThroughAttributes(f)
                };
            });

        // Get all fields without the IgnoreInFlags attribute for serialization
        var serializeFields = classSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(p => !HasIgnoreInFlagsAttribute(p))
            .Select(p => new SerializedPropertyInfo()
            {
                PropertyName = p.Name,
                PropertyType = p.Type.ToDisplayString(),
                IsEnum = p.Type.TypeKind == TypeKind.Enum,
                Limit = GetCustomLimit(p),
                Minimum = GetCustomMinimum(p),
                CustomSerializer = GetCustomFlagSerializer(p),
            });

        classInfo.SerializedFields.AddRange(serializeFields);


        classInfo.ReactiveFields.AddRange(reactiveFields);

        return (classInfo.SerializedFields.Any() || classInfo.ReactiveFields.Any()) ? classInfo : null;
    }

    private static bool HasFlagSerializeAttribute(INamedTypeSymbol classSymbol)
    {
        return classSymbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name is "FlagSerializeAttribute" or "FlagSerialize");
    }

    private static bool ImplementsINotifyPropertyChanged(INamedTypeSymbol classSymbol)
    {
        return classSymbol.AllInterfaces
            .Any(i => i.Name == "INotifyPropertyChanged");
    }

    private static bool HasIgnoreInFlagsAttribute(IFieldSymbol field)
    {
        return field.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name.StartsWith("IgnoreInFlags") ?? false);
    }

    private static bool HasReactiveAttribute(IFieldSymbol field)
    {
        return field.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name.StartsWith("Reactive") ?? false);
    }

    private static int? GetCustomLimit(IFieldSymbol property)
    {
        var customAttr = property.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name.StartsWith("Limit") ?? false);

        if (customAttr?.ConstructorArguments.Length > 0 &&
            customAttr.ConstructorArguments[0].Value is int limit)
        {
            return limit;
        }

        return null;
    }

    private static int? GetCustomMinimum(IFieldSymbol property)
    {
        var customAttr = property.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name.StartsWith("Minimum") ?? false);

        if (customAttr?.ConstructorArguments.Length > 0 &&
            customAttr.ConstructorArguments[0].Value is int minimum)
        {
            return minimum;
        }

        return null;
    }

    private static TypedConstant? GetCustomFlagSerializer(IFieldSymbol field)
    {
        var customAttr = field.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name.StartsWith("CustomFlagSerializer") ?? false);

        return customAttr?.ConstructorArguments.FirstOrDefault();
    }

    private static string GetPropertyName(string fieldName)
    {
        // Remove leading underscore and capitalize first letter
        var name = fieldName.StartsWith("_") ? fieldName.Substring(1) : fieldName;
        return char.ToUpper(name[0]) + name.Substring(1);
    }

    private static List<AttributeInfo> GetPassThroughAttributes(IFieldSymbol field)
    {
        var attributes = new List<AttributeInfo>();

        foreach (var attr in field.GetAttributes())
        {
            // Look for attributes with "property:" target
            if (attr.AttributeClass == null) continue;
            var attrName = attr.AttributeClass.Name;
            if (attrName.StartsWith("Reactive") || attrName.StartsWith("CustomFlagSerializer")) continue;
            if (attrName.EndsWith("Attribute"))
                attrName = attrName[..^9];

            var args = attr.ConstructorArguments
                .Select(arg => arg.Value?.ToString() ?? "null")
                .ToList();

            attributes.Add(new AttributeInfo
            {
                Name = attr.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Arguments = args
            });
        }

        return attributes;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassGenerationInfo?> classes, SourceProductionContext context)
    {
        foreach (var classInfo in classes)
        {
            if (classInfo == null) continue;

            var sourceCode = GenerateCode(classInfo);
            context.AddSource($"{classInfo.ClassName}.Generated.g.cs", sourceCode);
        }
    }

    private static string GenerateCode(ClassGenerationInfo classInfo)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(classInfo.Namespace))
        {
            sb.AppendLine($"namespace {classInfo.Namespace}");
            sb.AppendLine("{");
        }

        var indent = string.IsNullOrEmpty(classInfo.Namespace) ? "" : "    ";

        sb.AppendLine($"{indent}partial class {classInfo.ClassName}");
        sb.AppendLine($"{indent}{{");

        // Generate reactive properties
        foreach (var field in classInfo.ReactiveFields)
        {
            GenerateReactiveProperty(sb, field, indent);
        }

        GenerateSerializeMethod(sb, classInfo.SerializedFields, indent);

        sb.AppendLine($"{indent}}}");

        if (!string.IsNullOrEmpty(classInfo.Namespace))
        {
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private static void GenerateReactiveProperty(StringBuilder sb, ReactiveFieldInfo field, string indent)
    {
        sb.AppendLine();

        // Add pass-through attributes
        foreach (var attr in field.PassThroughAttributes)
        {
            var args = attr.Arguments.Any() ? $"({string.Join(", ", attr.Arguments)})" : "";
            sb.AppendLine($"{indent}    [{attr.Name}{args}]");
        }

        var defaultValue = field.DefaultValue != null ? $" = {field.DefaultValue};" : string.Empty;

        sb.AppendLine($"{indent}    public {field.FieldType} {field.PropertyName}");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        get => {field.FieldName};");
        sb.AppendLine($"{indent}        set");
        sb.AppendLine($"{indent}        {{");
        sb.AppendLine($"{indent}            if (!Equals({field.FieldName}, value))");
        sb.AppendLine($"{indent}            {{");
        sb.AppendLine($"{indent}                {field.FieldName} = value;");
        sb.AppendLine($"{indent}                OnPropertyChanged(nameof({field.PropertyName}));");
        sb.AppendLine($"{indent}                OnPropertyChanged(nameof(Flags));");
        sb.AppendLine($"{indent}            }}");
        sb.AppendLine($"{indent}        }}");
        sb.AppendLine($"{indent}    }}{defaultValue}");
    }

    private static void GenerateSerializeMethod(StringBuilder sb, List<SerializedPropertyInfo> fields, string indent)
    {
        sb.AppendLine();
        sb.AppendLine($"{indent}    public string Serialize()");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        FlagBuilder flags = new();");
        sb.AppendLine();

        foreach (var field in fields)
        {
            var serializeCall = GetSerializeCall(field);
            sb.AppendLine($"{indent}        {serializeCall};");
        }

        sb.AppendLine();
        sb.AppendLine($"{indent}        return flags.ToString();");
        sb.AppendLine($"{indent}    }}");
    }

    private static string GetSerializeCall(SerializedPropertyInfo property)
    {
        var limitExpression = "1";
        return property.PropertyType switch
        {
            "int" or "System.Int32" => $"SerializeInt(flags, {property.PropertyName}.Value(), {limitExpression})",
            "bool" or "System.Boolean" => $"SerializeBool(flags, {property.PropertyName}.Value(), {limitExpression})",
            var type when property.IsEnum => $"SerializeEnum<{type}>(flags, {property.PropertyName}.Value(), {limitExpression})",
            _ => $"SerializeCustom(flags, {property.PropertyName}.Value(), {limitExpression})"
        };
    }
}

public class ClassGenerationInfo
{
    public string ClassName { get; set; } = string.Empty;
    public string? Namespace { get; set; }
    public List<ReactiveFieldInfo> ReactiveFields { get; set; } = new();
    public List<SerializedPropertyInfo> SerializedFields { get; set; } = new();
}

public class SerializedPropertyInfo
{
    public string PropertyName { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public bool IsEnum { get; set; }
    public int? Limit { get; set; }
    public int? Minimum { get; set; }
    public TypedConstant? CustomSerializer { get; set; }
}

public class ReactiveFieldInfo
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public List<AttributeInfo> PassThroughAttributes { get; set; } = new();
}

public class AttributeInfo
{
    public string Name { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = new();
}