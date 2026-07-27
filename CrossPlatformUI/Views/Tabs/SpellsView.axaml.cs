using Avalonia.Markup.Xaml;
using CrossPlatformUI.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives.Disposables;
using System.Diagnostics.CodeAnalysis;

namespace CrossPlatformUI.Views.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class SpellsView : ReactiveUserControl<MainViewModel>
{
    public SpellsView()
    {
        this.WhenActivated((MultipleDisposable disposables) => { });
        AvaloniaXamlLoader.Load(this);
    }
}