using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels.Tabs;

namespace CrossPlatformUI.Views.Tabs;

public partial class SearchView : ReactiveUserControl<SearchViewModel>
{
    public SearchView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
