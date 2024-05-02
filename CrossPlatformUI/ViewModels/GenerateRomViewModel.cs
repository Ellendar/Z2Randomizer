using System;
using System.Reactive.Disposables;
using System.Threading;
using CrossPlatformUI.Services;
using RandomizerCore.Asm;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;
using Microsoft.Extensions.DependencyInjection;
namespace CrossPlatformUI.ViewModels;

public class GenerateRomViewModel : ReactiveValidationObject, IRoutableViewModel, IActivatableViewModel
{

    private string progress = "";
    public string Progress { get => progress; set => this.RaiseAndSetIfChanged(ref progress, value); }
    public GenerateRomViewModel()
    {
    }

    public GenerateRomViewModel(IScreen screen, RandomizerConfiguration config, byte[] vanillaRomData)
    {
        HostScreen = screen;
        Activator = new ViewModelActivator();
        this.WhenActivated(async (CompositeDisposable disposables) =>
        {
            var tokenSource = new CancellationTokenSource();
            var engine = App.Current?.Services?.GetService<IAsmEngine>();
            var hyrule = new Hyrule(config, engine!);
            var output = await hyrule.Randomize(vanillaRomData, (str) => Progress = str, tokenSource.Token );
            Disposable
                .Create(() =>
                {
                    tokenSource?.Cancel();
                })
                .DisposeWith(disposables);
            
        });
    }

    // Reference to IScreen that owns the routable view model.
    public IScreen HostScreen { get; }
    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    public ViewModelActivator Activator { get; }
}