using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using mini_pos.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;

namespace mini_pos.Views;

public partial class SupplierView : ReactiveUserControl<SupplierViewModel>
{
    public SupplierView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            if (ViewModel != null)
            {
                ViewModel.ShowDialog.RegisterHandler(DoShowDialogAsync);
            }
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async Task DoShowDialogAsync(IInteractionContext<Models.Supplier, System.Reactive.Unit> interaction)
    {
        var dialog = new SupplierWindow
        {
            DataContext = ViewModel // Share the same ViewModel so we bind to CurrentSupplier
        };

        if (ViewModel != null)
        {
            // Allow ViewModel to close the dialog
            ViewModel.CloseDialogAction = () => dialog.Close();
        }

        var mainWindow = (Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        
        if (mainWindow != null)
        {
            await dialog.ShowDialog(mainWindow);
        }
        
        interaction.SetOutput(System.Reactive.Unit.Default);
    }
}
