using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Views;

public partial class WeightedEnumSelection : UserControl
{
    public static readonly StyledProperty<string?> HeadingProperty =
        AvaloniaProperty.Register<WeightedEnumSelection, string?>(nameof(Heading));

    public string? Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    public static readonly StyledProperty<Type?> EnumTypeProperty =
        AvaloniaProperty.Register<WeightedEnumSelection, Type?>(nameof(EnumType));

    public Type? EnumType
    {
        get => GetValue(EnumTypeProperty);
        set => SetValue(EnumTypeProperty, value);
    }

    public static readonly StyledProperty<IDictionary> WeightsProperty =
        AvaloniaProperty.Register<WeightedEnumSelection, IDictionary>(
            nameof(Weights),
            defaultValue: new Dictionary<object, object>(),
            defaultBindingMode: BindingMode.TwoWay);

    public IDictionary Weights
    {
        get => GetValue(WeightsProperty);
        set => SetValue(WeightsProperty, value);
    }

    /// Will contain all (non-Random) Enum options
    public ObservableCollection<EnumItemViewModel> ItemsSource { get; } = new();

    public WeightedEnumSelection()
    {
        InitializeComponent();

        this.GetObservable(EnumTypeProperty).Subscribe(_ => RebuildItems());
        this.GetObservable(WeightsProperty).Subscribe(_ => SetSlidersFromDictionary());
    }

    private void RebuildItems()
    {
        ItemsSource.Clear();

        if (EnumType == null || !EnumType.IsEnum) { return; }

        foreach (var enumValue in Enum.GetValues(EnumType).Cast<Enum>().Where(b => b.CanHaveWeight()))
        {
            string description = enumValue.ToDescription().ToString();
            var vm = new EnumItemViewModel(enumValue, description);

            vm.GetObservable(EnumItemViewModel.SliderValueProperty)
                .Subscribe( w => SetWeight(enumValue, w) );

            ItemsSource.Add(vm);
        }
        SetSlidersFromDictionary();
    }

    private void SetWeight(Enum enumKey, int weightValue)
    {
        if (Weights == null || Weights is not IDictionary dict) { dict = new Dictionary<object, object>(); }

        // fast exit if there's no change
        if (dict.Contains(enumKey))
        {
            int oldWeight = (int)dict[enumKey]!;
            if (oldWeight == weightValue) { return; }
        }
        else
        {
            if (weightValue == 0) { return; }
        }

        Weights = CreateImmutableDictWithUpdatedValue(dict, enumKey, weightValue);
    }

    private void SetSlidersFromDictionary()
    {
        foreach (var item in ItemsSource)
        {
            int w = Weights != null && Weights.Contains(item.Value) ? (int)Weights[item.Value]! : 0;
            item.SliderValue = w;
        }
    }

    /// Magic code to work around not being able to pass generic types via Avalonia XAML.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Trimming", "IL2060:Call to 'System.Reflection.MethodInfo.MakeGenericMethod' can not be statically analyzed. It's not possible to guarantee the availability of requirements of the generic method.", Justification = "Generic type arguments are supplied at runtime due to Avalonia XAML limitations. All required generic instantiations are preserved by explicit application usage and are not subject to trimming.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Trimming", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.", Justification = "The reflected type is constructed from a runtime-provided enum type due to Avalonia XAML limitations. The accessed members are known and required by application logic, and all relevant generic instantiations are preserved by explicit usage.")]
    private IDictionary CreateImmutableDictWithUpdatedValue(IDictionary dict, Enum dictKey, int dictValue)
    {
        if (EnumType is null) { throw new InvalidOperationException("EnumType must be set before calling this method."); }
        if (dictKey.GetType() != EnumType) { throw new ArgumentException("Key enum type does not match EnumType.", nameof(dictKey)); }

        MethodInfo createBuilderMethod =
            typeof(ImmutableDictionary)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "CreateBuilder" &&
                             m.IsGenericMethodDefinition &&
                             m.GetParameters().Length == 0);

        MethodInfo genericCreateBuilder = createBuilderMethod.MakeGenericMethod(EnumType, typeof(int));

        object builder = genericCreateBuilder.Invoke(null, null)!;
        Type builderType = builder.GetType();

        PropertyInfo indexer = builderType.GetProperty("Item") ?? throw new MissingMemberException(builderType.FullName, "Item");
        MethodInfo toImmutableMethod = builderType.GetMethod("ToImmutable") ?? throw new MissingMethodException(builderType.FullName, "ToImmutable");

        foreach (DictionaryEntry entry in dict)
        {
            if (entry.Key.GetType() != EnumType) { throw new ArgumentException("Dictionary contains invalid key type.", nameof(dict)); }

            indexer.SetValue(builder, entry.Value, [entry.Key]);
        }

        indexer.SetValue(builder, dictValue, [dictKey]);

        return (IDictionary)toImmutableMethod.Invoke(builder, null)!;
    }
}

public class EnumItemViewModel : AvaloniaObject
{
    public Enum Value { get; }
    public string Description { get; }

    public static readonly StyledProperty<int> SliderValueProperty =
        AvaloniaProperty.Register<EnumItemViewModel, int>(nameof(SliderValue));

    public int SliderValue
    {
        get => GetValue(SliderValueProperty);
        set => SetValue(SliderValueProperty, value);
    }

    public EnumItemViewModel(Enum value, string description)
    {
        Value = value;
        Description = description;
    }
}
