using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class BiomesView : ReactiveUserControl<MainViewModel>
{
    public BiomesView()
    {
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(disposables =>
        {
        });
    }
}
