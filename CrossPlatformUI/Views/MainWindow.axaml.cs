using System;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CrossPlatformUI.ViewModels;
using ReactiveUI;

namespace CrossPlatformUI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    
    public MainWindow()
    {
        // Prevent the previewer's DataContext from being set when the application is run.
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        // var prevSize = ClientSize;
        
        // newPosition = new(WindowX, WindowY);
        // Resized += (sender, args) =>
        // {
        //     var x = (prevSize.Width - args.ClientSize.Width) / 2;
        //     var y = (prevSize.Height - args.ClientSize.Height) / 2;
        //     prevSize = args.ClientSize;
        //     Position = new PixelPoint(Position.X + (int)x, Position.Y + (int)y);
        // };
        PositionChanged += (sender, args) =>
        {
            var context = DataContext as MainWindowViewModel;
            context.WindowPosition = args.Point;
        };
        Resized += (sender, args) =>
        {
            var context = DataContext as MainWindowViewModel;
            context.WindowSize = args.ClientSize;
        };
    }
}