using System.Reactive;
using Avalonia.ReactiveUI;
using CrossPlatformUI.Views;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainViewModel Main { get; set; }
    public MainWindowViewModel()
    {
        Main = new MainViewModel();
    }
}