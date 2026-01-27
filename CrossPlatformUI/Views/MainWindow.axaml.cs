using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using ReactiveUI;
using ReactiveUI.Avalonia;
using CrossPlatformUI.ViewModels;

namespace CrossPlatformUI.Views;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        // Prevent the previewer's DataContext from being set when the application is run.
        this.WhenActivated(disposables => {
            var context = DataContext as MainViewModel;
            ClientSize = new Size(context!.WindowSize.Width,context.WindowSize.Height);
            PositionChanged += (_, args) =>
            {
                context.WindowPosition = new CustomPixelPoint
                {
                    X = args.Point.X,
                    Y = args.Point.Y,
                };
            };
            Resized += (_, args) =>
            {
                context.WindowSize = new CustomSize
                {
                    Width = args.ClientSize.Width,
                    Height = args.ClientSize.Height,
                };
            };
        });
        // AvaloniaXamlLoader.Load(this);
        // var prevSize = ClientSize;
        
        // newPosition = new(WindowX, WindowY);
        // Resized += (sender, args) =>
        // {
        //     var x = (prevSize.Width - args.ClientSize.Width) / 2;
        //     var y = (prevSize.Height - args.ClientSize.Height) / 2;
        //     prevSize = args.ClientSize;
        //     Position = new PixelPoint(Position.X + (int)x, Position.Y + (int)y);
        // };
    }
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        var context = DataContext as MainViewModel;
        ClientSize = new Size(context!.WindowSize.Width,context.WindowSize.Height);
    }
}