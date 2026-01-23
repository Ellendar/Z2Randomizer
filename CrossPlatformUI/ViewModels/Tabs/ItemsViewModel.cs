using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class ItemsViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }
    public MainViewModel Main { get; }

    public IObservable<bool> TownQuestLocationsAreMinorItemsIsIncludedObservable { get; }

    public ItemsViewModel(MainViewModel main)
    {
        Main = main;
        Activator = new();

        TownQuestLocationsAreMinorItemsIsIncludedObservable = Main.FlagsChanged
            .Select(_ => Main.Config.townQuestLocationsAreMinorItemsIsIncluded())
            .DistinctUntilChanged();

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(CompositeDisposable disposables)
    {
    }
}
