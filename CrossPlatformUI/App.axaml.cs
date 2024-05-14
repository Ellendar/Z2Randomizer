using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace CrossPlatformUI;

public sealed partial class App : Application // , IDisposable
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static ServiceCollection? ServiceContainer;

    public static IFileSystemService? FileSystemService;

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
        
        ServiceContainer ??= new ();
        
        try
        {
            // var files = Current?.Services?.GetService<IFileSystemService>()!;
            // var files = ServiceContainer.First(service => service.ServiceType == typeof(IFileSystemService)).;
            var files = FileSystemService!;
            var json = files.OpenFile(IFileSystemService.RandomizerPath.Settings, "Settings.json").Result;
            state = JsonConvert.DeserializeObject<object>(json, serializerSettings) ?? new MainViewModel();
        }
        catch (Exception)
        {
            state = new MainViewModel();
        }
        
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
    
    private readonly JsonSerializerSettings serializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
    };
    
    private Task PersistStateInternal()
    {
        // var tcs = new TaskCompletionSource();
        // shouldPersistState.OnNext(Disposable.Create(() => tcs.SetResult()));
        // return tcs.Task;
        return Task.Run(async () =>
        {
            var files = Current?.Services?.GetService<IFileSystemService>()!;
            var json = JsonConvert.SerializeObject(state, serializerSettings);
            var next = JObject.Parse(json);
            try
            {
                var settings = await files.OpenFile(IFileSystemService.RandomizerPath.Settings, "Settings.json");
                var orig = JObject.Parse(settings ?? "{}");
                orig.Merge(next, new JsonMergeSettings
                {
                    // union array values together to avoid duplicates
                    MergeArrayHandling = MergeArrayHandling.Union,
                });
                next = orig;
            }
            catch (Exception e) when (e is JsonException or IOException) { }

            var stringbuild = new StringBuilder();
            await using var sw = new StringWriter(stringbuild);
            await using var writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.None;
            next.WriteTo(writer);
            await files.SaveFile(IFileSystemService.RandomizerPath.Settings, "Settings.json", stringbuild.ToString());
            // SetItem("appstate", stringbuild.ToString());
            // SyncSuspensionDriver!.SaveState(state!);
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