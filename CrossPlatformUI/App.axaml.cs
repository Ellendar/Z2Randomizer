using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.Services;
using CrossPlatformUI.ViewModels;
using CrossPlatformUI.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace CrossPlatformUI;

public sealed partial class App : Application // , IDisposable
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static ServiceCollection? ServiceContainer;

    public static ISuspendSyncService? SyncSuspensionDriver;

    private object? state;
    

    // private readonly Subject<IDisposable> shouldPersistState = new ();
    // private readonly Subject<Unit> isLaunchingNew = new ();
    
    public override void OnFrameworkInitializationCompleted()
    {
        // Remove built-in validation plugins
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
        
        // We can only use the AutoSuspendHelper on desktop style platforms,
        // so this code is mainly ripped from there with adjustments to allow us to manually save on other platforms.
        // RxApp.SuspensionHost.ShouldPersistState = Design.IsDesignMode ? Observable.Never<IDisposable>() : shouldPersistState;
        // RxApp.SuspensionHost.CreateNewAppState = () => new MainViewModel();
        // RxApp.SuspensionHost.SetupDefaultSuspendResume(SuspensionDriver);
        // RxApp.SuspensionHost.IsResuming = Observable.Never<Unit>();
        // RxApp.SuspensionHost.IsLaunchingNew = isLaunchingNew;
        // var errored = new Subject<Unit>();
        // AppDomain.CurrentDomain.UnhandledException += (o, e) => errored.OnNext(Unit.Default);
        // RxApp.SuspensionHost.ShouldInvalidateState = errored;
        
        // ReactiveUI's suspension stuff doesn't work on browser either. So hack that together too.
        // var state = RxApp.SuspensionHost.GetAppState<MainViewModel>();
        try
        {
            state = SyncSuspensionDriver!.LoadState() ?? new MainViewModel();
        }
        catch (Exception)
        {
            state = new MainViewModel();
        }
        
        ServiceContainer ??= new ();
        
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.Exit += (_,_) =>
                {
                    PersistStateInternal().Wait();
                };
                // isLaunchingNew.OnNext(Unit.Default);

                desktop.MainWindow = new MainWindow
                {
                    DataContext = state
                };

                ServiceContainer.AddSingleton<IFileDialogService>(x => new FileDialogService(desktop.MainWindow));
                break;
            case ISingleViewApplicationLifetime singleViewPlatform:
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = state
                };

                ServiceContainer.AddSingleton<IFileDialogService>(x => new FileDialogService(TopLevel.GetTopLevel(singleViewPlatform.MainView)));
                break;
        }

        Services = ServiceContainer.BuildServiceProvider();
        
        base.OnFrameworkInitializationCompleted();
    }

    public static Task PersistState()
    {
        return Current!.PersistStateInternal();
    }
    
    private Task PersistStateInternal()
    {
        // var tcs = new TaskCompletionSource();
        // shouldPersistState.OnNext(Disposable.Create(() => tcs.SetResult()));
        // return tcs.Task;
        return Task.Run(() =>
        {
            SyncSuspensionDriver!.SaveState(state!);
        });
    }

    public new static App? Current => Application.Current as App;
    
    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }

    // public void Dispose()
    // {
    //     shouldPersistState.Dispose();
    //     isLaunchingNew.Dispose();
    // }
}