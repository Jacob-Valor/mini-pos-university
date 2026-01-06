using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using mini_pos.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;

namespace mini_pos.Views;

public partial class SalesView : ReactiveUserControl<SalesViewModel>
{
    public SalesView()
    {
        InitializeComponent();
        this.WhenActivated(disposables => 
        {
            if (ViewModel != null)
            {
                ViewModel.ShowReceiptRequested += ViewModel_ShowReceiptRequested;
                Disposable.Create(() => ViewModel.ShowReceiptRequested -= ViewModel_ShowReceiptRequested).DisposeWith(disposables);
            }
        });
    }

    private void ViewModel_ShowReceiptRequested(ReceiptViewModel vm)
    {
        var window = new ReceiptWindow { DataContext = vm };
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
        {
             window.ShowDialog(desktop.MainWindow);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
