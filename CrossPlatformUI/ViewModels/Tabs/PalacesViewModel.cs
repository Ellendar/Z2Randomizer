using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class PalacesViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }
    public MainViewModel Main { get; }

    public IObservable<bool> PalaceStyleWeightsIncludedObservable { get; }

    public PalacesViewModel(MainViewModel main)
    {
        Main = main;
        Activator = new();

        PalaceStyleWeightsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.palaceStyleWeightsIncluded())
            .DistinctUntilChanged();

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(CompositeDisposable disposables)
    {
    }
}
