using System;
using System.Reactive.Disposables;
using System.Runtime.Serialization;
using System.Threading;
using CrossPlatformUI.Services;
using RandomizerCore.Asm;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;
using Microsoft.Extensions.DependencyInjection;
using Z2Randomizer.Core.Sidescroll;

namespace CrossPlatformUI.ViewModels;

public class GenerateRomViewModel : ReactiveValidationObject, IRoutableViewModel, IActivatableViewModel
{

    private string progress = "";
    public string Progress { get => progress; set => this.RaiseAndSetIfChanged(ref progress, value); }
    public GenerateRomViewModel()
    {
    }

    public GenerateRomViewModel(MainViewModel screen)
    {
        HostScreen = screen;
        Activator = new ViewModelActivator();

        this.WhenActivated(Randomize);
        return;

        async void Randomize(CompositeDisposable disposables)
        {
            var tokenSource = new CancellationTokenSource();
            var engine = App.Current?.Services?.GetService<IAsmEngine>();
            // var roomsJson = await fileService!.OpenFileAsync();
            // var customJson = config.UseCustomRooms ? await fileService!.OpenFileAsync() : null;
            var palaceRooms = new PalaceRooms("", null);
            var randomizer = new Hyrule(engine!, palaceRooms);
            var host = (HostScreen as MainViewModel)!;
            var output = await randomizer.Randomize(host.RomFileViewModel.RomData!, host.Config, str => Progress = str, tokenSource.Token);
            Disposable.Create(() => { tokenSource.Cancel(); })
                .DisposeWith(disposables);
        }
    }

    // Reference to IScreen that owns the routable view model.
    public IScreen HostScreen { get; }
    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    public ViewModelActivator Activator { get; }
    
}