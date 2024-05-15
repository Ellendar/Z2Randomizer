using System.Reactive;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.ReactiveUI;
using CrossPlatformUI.Views;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels;


[DataContract]
public class MainWindowViewModel : ViewModelBase
{
    private PixelPoint windowPosition;
    private Size windowSize;
    
    [DataMember]
    public PixelPoint WindowPosition { get => windowPosition; set => this.RaiseAndSetIfChanged(ref windowPosition, value); }
    
    [DataMember]
    public Size WindowSize { get => windowSize; set => this.RaiseAndSetIfChanged(ref windowSize, value); }
    
    [DataMember]
    public MainViewModel Main { get; set; } = new();
}