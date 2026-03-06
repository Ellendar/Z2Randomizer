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

    public IObservable<bool> LegacyVanillaShuffledLocationsIncludedObservable { get; }

    public BiomesViewModel(MainViewModel main)
    {
        Main = main;
        Activator = new();

        LegacyVanillaShuffledLocationsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.legacyVanillaShuffledLocationsIncluded())
            .DistinctUntilChanged();

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(CompositeDisposable disposables)
    {
    }
}
