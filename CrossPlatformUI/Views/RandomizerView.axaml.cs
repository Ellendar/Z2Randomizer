using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace CrossPlatformUI.Views;

public partial class RandomizerView : ReactiveUserControl<RandomizerViewModel>
{
    public RandomizerView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
    }

}