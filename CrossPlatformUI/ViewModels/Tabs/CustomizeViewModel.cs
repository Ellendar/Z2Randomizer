using System.Runtime.Serialization;

namespace CrossPlatformUI.ViewModels.Tabs;

[DataContract]
public class CustomizeViewModel(MainViewModel main) : ViewModelBase
{
    public SpritePreviewViewModel SpritePreviewViewModel { get; } = new (main);
}