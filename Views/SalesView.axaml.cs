using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using mini_pos.ViewModels;

namespace mini_pos.Views;

public partial class SalesView : UserControl
{
    public SalesView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SalesViewModel vm)
        {
            // Unsubscribe from previous VM if needed
            vm.ShowReceiptRequested -= ViewModel_ShowReceiptRequested;
            // Subscribe to new VM
            vm.ShowReceiptRequested += ViewModel_ShowReceiptRequested;
        }
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
