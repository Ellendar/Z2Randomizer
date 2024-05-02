using System.Reactive;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; } = new ();

    // The command that navigates a user to first view model.
    public ReactiveCommand<Unit, IRoutableViewModel> LoadRom { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GenerateRom { get; }

    // The command that navigates a user back.
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack!;

    public RomFileViewModel RomFileViewModel { get; }
    public MainViewModel MainViewModel { get; }
    
    public MainWindowViewModel()
    {
        // Manage the routing state. Use the Router.Navigate.Execute
        // command to navigate to different view models. 
        //
        // Note, that the Navigate.Execute method accepts an instance 
        // of a view model, this allows you to pass parameters to 
        // your view models, or to reuse existing view models.
        //

        RomFileViewModel = new(this);
        MainViewModel = new(this);
        
        if (!RomFileViewModel.HasRomData)
        {
            Router.Navigate.Execute(RomFileViewModel);
        }
        else
        {
            Router.Navigate.Execute(MainViewModel);
        }
        
        // GoNext = ReactiveCommand.CreateFromObservable(
        //     () => Router.Navigate.Execute(new MainViewModel(this))
        // );

        LoadRom = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(RomFileViewModel)
        );
        GenerateRom = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new GenerateRomViewModel(this, MainViewModel.Config, RomFileViewModel.RomData))
        );
    }
}