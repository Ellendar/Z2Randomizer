using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Material.Styles.Themes;
using System;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.Views;

public partial class NesColorDropdown : UserControl
{
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<NesColorDropdown, string?>(nameof(Label));

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// The selected color index (-1 = Default, -2 = Random)
    public static readonly StyledProperty<int> SelectedColorIndexProperty =
        AvaloniaProperty.Register<NesColorDropdown, int>(
            nameof(SelectedColorIndex),
            defaultValue: (int)NesColor.Default,
            defaultBindingMode: BindingMode.TwoWay);

    public int SelectedColorIndex
    {
        get => GetValue(SelectedColorIndexProperty);
        set => SetValue(SelectedColorIndexProperty, value);
    }

    public static readonly StyledProperty<int> DefaultColorIndexProperty =
        AvaloniaProperty.Register<NesColorDropdown, int>(
            nameof(DefaultColorIndex),
            defaultValue: 0x10);

    public int DefaultColorIndex
    {
        get => GetValue(DefaultColorIndexProperty);
        set => SetValue(DefaultColorIndexProperty, value);
    }

    public event EventHandler<int>? ColorSelected;

    public NesColorDropdown()
    {
        InitializeComponent();

        this.GetObservable(LabelProperty).Subscribe(label => ControlLabel.Text = string.IsNullOrEmpty(label) ? "" : label);

        BuildPaletteButtons();

        SetupButton(SelectedColorIndex);

        this.GetObservable(SelectedColorIndexProperty).Subscribe(SetupButton);
        this.GetObservable(DefaultColorIndexProperty).Subscribe(idx =>
        {
            var color = GetBrushFromNesColor(idx);
            DefaultButton.Background = color;
            DefaultButton.Foreground = GetContrastBrush(color.Color);
            if (SelectedColorIndex == (int)NesColor.Default) { SetupButton(SelectedColorIndex); }
        });

        SelectedButton.Click += (_, _) => Popup.IsOpen = true;
        DefaultButton.Click += (_, _) => Select((int)NesColor.Default);
        RandomButton.Click += (_, _) => Select((int)NesColor.Random);
    }

    private void BuildPaletteButtons()
    {
        const int cols = 14, rows = 4;
        PaletteGrid.Children.Clear();
        PaletteGrid.ColumnDefinitions.Clear();
        PaletteGrid.RowDefinitions.Clear();

        for (int i = 0; i < cols; i++)
        {
            PaletteGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        }
        for (int i = 0; i < rows; i++)
        {
            PaletteGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int index = (row * 0x10) + col;
                if (index >= NES.NesColors.Length) { continue; }

                if (index == 0x0D) // Leave color $0D slot empty
                {
                    var blank = new Border
                    {
                        Width = 28,
                        Height = 28,
                        Margin = new Thickness(2),
                        Background = Brushes.Transparent
                    };
                    Grid.SetRow(blank, row);
                    Grid.SetColumn(blank, col);
                    PaletteGrid.Children.Add(blank);
                    continue;
                }

                var brush = GetBrushFromNesColor(index);

                var btn = new Button
                {
                    Background = brush,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(2),
                    Width = 28,
                    Height = 28,
                    Tag = index
                };

                btn.Click += OnColorClick;

                Grid.SetRow(btn, row);
                Grid.SetColumn(btn, col);
                PaletteGrid.Children.Add(btn);
            }
        }
    }

    private static SolidColorBrush GetBrushFromNesColor(int index)
    {
        var color = NES.NesColors[index];
        return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
    }

    private static IBrush GetBackgroundBrush()
    {
        if (Application.Current?.LocateMaterialTheme<MaterialTheme>() is { } theme &&
            theme.TryGetResource("MaterialPaperBrush", null, out var brushObj) &&
            brushObj is IBrush brush)
        {
            return brush;
        }

        return Brushes.LightGray;
    }

    private static IBrush GetContrastBrush(Color background)
    {
        double luminance = (0.299 * background.R + 0.587 * background.G + 0.114 * background.B) / 255;
        return luminance > 0.5 ? Brushes.Black : Brushes.White;
    }

    private void OnColorClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int index) { return; }
        Select(index);
    }

    private void Select(int index)
    {
        SelectedColorIndex = index;
        SetupButton(index);
        ColorSelected?.Invoke(this, index);
        Popup.IsOpen = false;
    }

    private void SetupButton(int index)
    {
        IBrush background = index switch
        {
            (int)NesColor.Default => GetBrushFromNesColor(DefaultColorIndex),
            (int)NesColor.Random => GetBackgroundBrush(),
            _ => GetBrushFromNesColor(index)
        };

        string text = index switch
        {
            (int)NesColor.Default => "Default",
            (int)NesColor.Random => "Random",
            _ => $"{((NesColor)index).ToDescription()} ({index:X2})",
        };

        Color? baseColor = (background as SolidColorBrush)?.Color;
        IBrush foreground = baseColor.HasValue ? GetContrastBrush(baseColor.Value) : Brushes.Black;

        SelectedButton.Background = background;
        SelectedLabel.Foreground = foreground;
        SelectedLabel.Text = text;
    }
}
