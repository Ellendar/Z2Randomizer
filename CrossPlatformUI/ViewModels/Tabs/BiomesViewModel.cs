using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class BiomesViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }
    public MainViewModel Main { get; }

    public IObservable<bool> VanillaShuffleUsesActualTerrainIsIncludedObservable { get; }

    public BiomesViewModel(MainViewModel main)
    {
        Main = main;
        Activator = new();

        VanillaShuffleUsesActualTerrainIsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.vanillaShuffleUsesActualTerrainIsIncluded())
            .DistinctUntilChanged();

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(CompositeDisposable disposables)
    {
    }
}
