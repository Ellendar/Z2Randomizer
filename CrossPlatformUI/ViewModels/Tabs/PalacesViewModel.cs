using System.Diagnostics.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;

namespace CrossPlatformUI.ViewModels.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class PalacesViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }
    public MainViewModel Main { get; }

    public PalacesViewModel(MainViewModel main)
    {
        Main = main;
        Activator = new();

        this.WhenActivated(OnActivate);
    }

    internal void OnActivate(MultipleDisposable disposables)
    {
        // At most one of the "no duplicate rooms" settings may be on:
        // checking either clears the other (was view code-behind before).
        SubscribeExtensions.Subscribe(
            this.WhenAnyValue(x => x.Main.Config.NoDuplicateRoomsByLayout),
            byLayout =>
            {
                if (byLayout)
                    Main.Config.NoDuplicateRoomsByEnemies = false;
            })
            .DisposeWith(disposables);

        SubscribeExtensions.Subscribe(
            this.WhenAnyValue(x => x.Main.Config.NoDuplicateRoomsByEnemies),
            byEnemies =>
            {
                if (byEnemies)
                    Main.Config.NoDuplicateRoomsByLayout = false;
            })
            .DisposeWith(disposables);
    }
}