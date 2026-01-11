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

    public IObservable<bool> RandomStylesAllowVanillaIncludedObservable { get; }
    public IObservable<bool> RemoveLongDeadEndsIncludedObservable { get; }
    public IObservable<bool> IncludeExpertRoomsIncludedObservable { get; }

    public PalacesViewModel(MainViewModel main)
    {
        Main = main;
        Activator = new();

        RandomStylesAllowVanillaIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.randomStylesAllowVanillaIncluded())
            .DistinctUntilChanged();

        RemoveLongDeadEndsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.removeLongDeadEndsIncluded())
            .DistinctUntilChanged();

        IncludeExpertRoomsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.includeExpertRoomsIncluded())
            .DistinctUntilChanged();

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(CompositeDisposable disposables)
    {
    }
}
