using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreSourceGenerator;

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

        classInfo.ReactiveFields.AddRange(reactiveFields);

        // Get all fields without the IgnoreInFlags attribute for serialization
        var serializeFields = classSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => !HasIgnoreInFlagsAttribute(f))
            .Select(f => new SerializedFieldInfo()
            {
                FieldName = f.Name,
                FieldType = f.Type.ToDisplayString(),
                IsConditionallyIncluded = HasConditionallyIncludedInFlagsAttribute(f),
                DefaultValue = GetDefaultValue(f),
                IsEnum = f.Type.TypeKind == TypeKind.Enum,
                EnumSymbol = f.Type.TypeKind == TypeKind.Enum ? f.Type as INamedTypeSymbol : null,
                Limit = GetCustomLimit(f),
                Minimum = GetCustomMinimum(f),
                CustomSerializerName = GetCustomFlagSerializer(f),
            }).ToList();

        classInfo.SerializedFields.AddRange(serializeFields);

        var enumTypes = new HashSet<string>();
        if (enumTypes == null) throw new ArgumentNullException(nameof(enumTypes));
        foreach (var field in serializeFields.Where(f => f.IsEnum && f.EnumSymbol != null))
        {
            enumTypes.Add(field.FieldType);

            // Get enum values for this type
            var enumValues = field.EnumSymbol?.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(f => f.IsStatic && f.HasConstantValue)
                .Select(f => f.Name)
                .ToList();

            if (enumValues != null) classInfo.EnumArrays[field.FieldType] = enumValues;
        }

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

    private static bool HasConditionallyIncludedInFlagsAttribute(IFieldSymbol field)
    {
        return field.GetAttributes()
            .Any(attr => attr.AttributeClass?.Name.StartsWith("ConditionallyIncludeInFlags") ?? false);
    }

    private static string GetDefaultValue(IFieldSymbol f)
    {
        var defaultAttr = f.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DefaultValueAttribute");
        var arg = defaultAttr?.ConstructorArguments[0];
        return arg != null ? FormatArgument(arg.Value) : "default";
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

    private static string? GetCustomFlagSerializer(IFieldSymbol field)
    {
        var customAttr = field.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name.StartsWith("CustomFlagSerializer") ?? false);
        var firstArg = customAttr?.ConstructorArguments.FirstOrDefault();
        if (firstArg is { Kind: TypedConstantKind.Type, Value: ITypeSymbol type })
        {
            return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
        return null;
    }

    private static string GetPropertyName(string fieldName)
    {
        // Remove leading underscore and capitalize first letter
        var name = fieldName.StartsWith("_") ? fieldName[1..] : fieldName;
        return char.ToUpper(name[0]) + name[1..];
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
            // if (attrName.EndsWith("Attribute"))
            //     attrName = attrName[..^9];

            var args = attr.ConstructorArguments
                .Select(arg => FormatArgument(arg))
                .ToList();

            attributes.Add(new AttributeInfo
            {
                Name = attr.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Arguments = args
            });
        }

        return attributes;
    }

    private static void Execute(Compilation _, ImmutableArray<ClassGenerationInfo?> classes, SourceProductionContext context)
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

        // Generate static enum arrays
        GenerateEnumArrays(sb, classInfo.EnumArrays, indent);

        // Generate reactive properties
        foreach (var field in classInfo.ReactiveFields)
        {
            GenerateReactiveProperty(sb, field, indent);
        }

        GenerateSerializeMethod(sb, classInfo.SerializedFields, indent);
        GenerateDeserializeMethod(sb, classInfo.SerializedFields, indent);

        // Generate helper methods for enum serialization
        GenerateSerializerList(sb, classInfo.SerializedFields, indent);
        GenerateEnumHelperMethods(sb, classInfo.EnumArrays, indent);

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

        // Properties can't have a default value if its not using a generated backing field.
        // var defaultValue = field.DefaultValue != null ? $" = {field.DefaultValue};" : string.Empty;
        var defaultValue = string.Empty;

        sb.AppendLine($"{indent}    public {field.FieldType} {field.PropertyName}");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        get => {field.FieldName};");
        sb.AppendLine($"{indent}        set");
        sb.AppendLine($"{indent}        {{");
        sb.AppendLine($"{indent}            if (!Equals({field.FieldName}, value))");
        sb.AppendLine($"{indent}            {{");
        sb.AppendLine($"{indent}                {field.FieldName} = value;");
        sb.AppendLine($"{indent}                OnPropertyChanged(nameof({field.PropertyName}));");
        sb.AppendLine($"{indent}                OnPropertyChanged(\"Flags\");");
        sb.AppendLine($"{indent}            }}");
        sb.AppendLine($"{indent}        }}");
        sb.AppendLine($"{indent}    }}{defaultValue}");
    }

    private static void GenerateSerializeMethod(StringBuilder sb, List<SerializedFieldInfo> fields, string indent)
    {
        sb.AppendLine();
        sb.AppendLine($"{indent}    public string Serialize()");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        global::Z2Randomizer.RandomizerCore.Flags.FlagBuilder flags = new();");
        sb.AppendLine();

        foreach (var field in fields)
        {
            var serializeCall = GetSerializeCall(field);
            if (field.IsConditionallyIncluded)
            {
                sb.AppendLine($"{indent}        if ({field.FieldName}IsIncluded()) {{ ");
                sb.AppendLine($"{indent}            {serializeCall};");
                sb.AppendLine($"{indent}        }}");
            }
            else
            {
                sb.AppendLine($"{indent}        {serializeCall};");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"{indent}        return flags.ToString();");
        sb.AppendLine($"{indent}    }}");
    }

    private static void GenerateDeserializeMethod(StringBuilder sb, List<SerializedFieldInfo> fields, string indent)
    {
        sb.AppendLine();
        sb.AppendLine($"{indent}    public void Deserialize(string flagstring)");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        global::Z2Randomizer.RandomizerCore.Flags.FlagReader flags = new(flagstring);");
        sb.AppendLine();

        foreach (var field in fields)
        {
            var deserializeCall = GetDeserializeCall(field);
            if (field.IsConditionallyIncluded)
            {
                sb.AppendLine($"{indent}        if ({field.FieldName}IsIncluded()) {{ ");
                sb.AppendLine($"{indent}            {deserializeCall};");
                sb.AppendLine($"{indent}        }}");
                sb.AppendLine($"{indent}        else");
                sb.AppendLine($"{indent}        {{");
                sb.AppendLine($"{indent}            {GetPropertyName(field.FieldName)} = {field.DefaultValue};");
                sb.AppendLine($"{indent}        }}");
            }
            else
            {
                sb.AppendLine($"{indent}        {deserializeCall};");
            }
        }
        sb.AppendLine();
        sb.AppendLine($"{indent}    }}");
    }

    private static void GenerateSerializerList(StringBuilder sb, List<SerializedFieldInfo> fields, string indent)
    {
        sb.AppendLine();
        sb.AppendLine($"{indent}    public static global::Z2Randomizer.RandomizerCore.Flags.IFlagSerializer GetSerializer<T>()");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        var type =  typeof(T);");
        sb.AppendLine($"{indent}        switch (type)");
        sb.AppendLine($"{indent}        {{");
        foreach (var field in fields)
        {
            if (field.CustomSerializerName == null) continue;
            sb.AppendLine($"{indent}            case Type _ when type == typeof({field.CustomSerializerName}): return new {field.CustomSerializerName}();");
        }
        sb.AppendLine($"{indent}            default: throw new Exception();");
        sb.AppendLine($"{indent}        }}");
        sb.AppendLine($"{indent}    }}");
    }

    private static string GetSerializeCall(SerializedFieldInfo field)
    {
        var limitExpression = field.Limit != null ? $"{field.Limit}" : "null";
        var minExpression = $"{field.Minimum ?? 0}";
        var output = new StringBuilder();
        if (field.FieldType is "int" or "int?" && field.Limit == null)
            output.AppendLine($"#error Numeric type {field.FieldName} must have a `Limit` attribute!");
        output.Append(field.FieldType switch
        {
            "int" or "System.Int32" => $"SerializeInt(flags, \"{field.FieldName}\", {field.FieldName}, false, {limitExpression}, {minExpression})",
            "int?" or "System.Int32?" => $"SerializeInt(flags, \"{field.FieldName}\", {field.FieldName}, true, {limitExpression}, {minExpression})",
            "bool" or "System.Boolean" => $"SerializeBool(flags, \"{field.FieldName}\", {field.FieldName}, false)",
            "bool?" or "System.Boolean?" => $"SerializeBool(flags, \"{field.FieldName}\", {field.FieldName}, true)",
            var type when field.IsEnum => $"SerializeEnum<{type}>(flags, \"{field.FieldName}\", {field.FieldName})",
            _ => $"SerializeCustom<{field.CustomSerializerName}, {field.FieldType}>(flags, \"{field.FieldName}\", {field.FieldName})"
        });
        return output.ToString();
    }

    private static string GetDeserializeCall(SerializedFieldInfo field)
    {
        var limitExpression = field.Limit != null ? $"{field.Limit}" : "null";
        var minExpression = $"{field.Minimum ?? 0}";
        var output = new StringBuilder();
        var propName = GetPropertyName(field.FieldName);
        if (field.FieldType is "int" or "int?" && field.Limit == null)
            output.AppendLine($"#error Numeric type {field.FieldName} must have a `Limit` attribute!");
        output.Append(field.FieldType switch
        {
            "int" or "System.Int32" => $"{propName} = DeserializeInt(flags, \"{field.FieldName}\", {limitExpression}, {minExpression})",
            "int?" or "System.Int32?" => $"{propName} = DeserializeNullableInt(flags, \"{field.FieldName}\", {limitExpression}, {minExpression})",
            "bool" or "System.Boolean" => $"{propName} = DeserializeBool(flags, \"{field.FieldName}\")",
            "bool?" or "System.Boolean?" => $"{propName} = DeserializeNullableBool(flags, \"{field.FieldName}\")",
            var type when field.IsEnum => $"{propName} = DeserializeEnum<{type}>(flags, \"{field.FieldName}\")",
            _ => $"{propName} = DeserializeCustom<{field.CustomSerializerName}, {field.FieldType}>(flags, \"{field.FieldName}\")"
        });
        return output.ToString();
    }

    private static void GenerateEnumArrays(StringBuilder sb, Dictionary<string, List<string>> enumArrays, string indent)
    {
        if (!enumArrays.Any()) return;

        sb.AppendLine();
        sb.AppendLine($"{indent}    // Static arrays for enum serialization");

        foreach (var enumArray in enumArrays)
        {
            var enumType = enumArray.Key;
            var enumValues = enumArray.Value;
            var arrayName = GetEnumArrayName(enumType);

            sb.AppendLine($"{indent}    private static readonly {enumType}[] {arrayName} = new {enumType}[]");
            sb.AppendLine($"{indent}    {{");

            for (int i = 0; i < enumValues.Count; i++)
            {
                var comma = i < enumValues.Count - 1 ? "," : "";
                sb.AppendLine($"{indent}        {enumType}.{enumValues[i]}{comma}");
            }

            sb.AppendLine($"{indent}    }};");
            sb.AppendLine();
        }
    }

    private static void GenerateEnumHelperMethods(StringBuilder sb, Dictionary<string, List<string>> enumArrays, string indent)
    {
        if (!enumArrays.Any()) return;

        sb.AppendLine();
        sb.AppendLine($"{indent}    // Generic helper method for enum serialization");

        // Generate single generic GetEnumIndex method
        sb.AppendLine($"{indent}    public static int GetEnumIndex<T>(T? enumValue) where T : Enum");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        if (enumValue == null) return -1;");
        sb.AppendLine($"{indent}        var enumType = typeof(T);");
        sb.AppendLine($"{indent}        return enumType.Name switch");
        sb.AppendLine($"{indent}        {{");

        foreach (var enumArray in enumArrays)
        {
            var enumType = enumArray.Key;
            var arrayName = GetEnumArrayName(enumType);
            var shortTypeName = GetShortTypeName(enumType);

            sb.AppendLine($"{indent}            \"{shortTypeName}\" => Array.IndexOf<{enumType}>({arrayName}, ({enumType})(object)enumValue),");
        }

        sb.AppendLine($"{indent}            _ => -1 // Unknown enum type");
        sb.AppendLine($"{indent}        }};");
        sb.AppendLine($"{indent}    }}");
        sb.AppendLine();

        // Generate single generic GetEnumFromIndex method
        sb.AppendLine($"{indent}    public static T? GetEnumFromIndex<T>(int index) where T : Enum");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        var enumType = typeof(T);");
        sb.AppendLine($"{indent}        if (enumType == null) return default;");
        sb.AppendLine($"{indent}        return enumType.Name switch");
        sb.AppendLine($"{indent}        {{");

        foreach (var enumArray in enumArrays)
        {
            var enumType = enumArray.Key;
            var arrayName = GetEnumArrayName(enumType);
            var shortTypeName = GetShortTypeName(enumType);

            sb.AppendLine($"{indent}            \"{shortTypeName}\" => (T)(object)(index >= 0 && index < {arrayName}.Length ? {arrayName}[index] : default({enumType})),");
        }

        sb.AppendLine($"{indent}            _ => default(T) // Unknown enum type");
        sb.AppendLine($"{indent}        }};");
        sb.AppendLine($"{indent}    }}");
        sb.AppendLine();

        // Generate method to get enum count
        sb.AppendLine($"{indent}    public static int GetEnumCount<T>() where T : Enum");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        var enumType = typeof(T);");
        sb.AppendLine($"{indent}        return enumType.Name switch");
        sb.AppendLine($"{indent}        {{");

        foreach (var enumArray in enumArrays)
        {
            var enumType = enumArray.Key;
            var arrayName = GetEnumArrayName(enumType);
            var shortTypeName = GetShortTypeName(enumType);

            sb.AppendLine($"{indent}            \"{shortTypeName}\" => {arrayName}.Length,");
        }

        sb.AppendLine($"{indent}            _ => 0 // Unknown enum type");
        sb.AppendLine($"{indent}        }};");
        sb.AppendLine($"{indent}    }}");
    }

    private static string GetEnumArrayName(string enumType)
    {
        var shortName = GetShortTypeName(enumType);
        return $"__{shortName}Values";
    }

    private static string GetShortTypeName(string fullTypeName)
    {
        // Extract just the type name without namespace
        var lastDot = fullTypeName.LastIndexOf('.');
        return lastDot >= 0 ? fullTypeName[(lastDot + 1)..] : fullTypeName;
    }

    private static string FormatArgument(TypedConstant arg)
    {
        if (arg.Kind == TypedConstantKind.Primitive)
        {
            return arg.Value switch
            {
                bool b => b ? "true" : "false",
                string s => $"\"{s}\"",
                char c => $"'{c}'",
                null => "null",
                _ => Convert.ToString(arg.Value, CultureInfo.InvariantCulture)!
            };
        }

        if (arg.Kind == TypedConstantKind.Enum)
        {
            return $"{arg.Type!.ToDisplayString()}.{arg.Value}";
        }

        return "null";
    }
}

public class ClassGenerationInfo
{
    public string ClassName { get; set; } = string.Empty;
    public string? Namespace { get; set; }
    public List<ReactiveFieldInfo> ReactiveFields { get; set; } = new();
    public List<SerializedFieldInfo> SerializedFields { get; set; } = new();
    public Dictionary<string, List<string>> EnumArrays { get; set; } = new();
}

public class SerializedFieldInfo
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public bool IsConditionallyIncluded { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsEnum { get; set; }
    public INamedTypeSymbol? EnumSymbol { get; set; }
    public int? Limit { get; set; }
    public int? Minimum { get; set; }
    public string? CustomSerializerName { get; set; }
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