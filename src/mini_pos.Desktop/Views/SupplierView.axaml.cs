using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using mini_pos.ViewModels;

namespace mini_pos.Views;

public partial class SupplierView : UserControl
{
    private SupplierViewModel? _vm;

    public SupplierView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_vm != null)
            _vm.ShowSupplierDialogRequested -= ShowSupplierDialog;

        _vm = DataContext as SupplierViewModel;
        if (_vm != null)
            _vm.ShowSupplierDialogRequested += ShowSupplierDialog;
    }

    private void ShowSupplierDialog()
    {
        if (_vm == null) return;

        var window = new SupplierWindow { DataContext = _vm };

        Action closeAction = window.Close;
        _vm.CloseDialogAction = closeAction;
        window.Closed += (_, _) =>
        {
            if (_vm != null && ReferenceEquals(_vm.CloseDialogAction, closeAction))
                _vm.CloseDialogAction = null;
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
            window.ShowDialog(desktop.MainWindow);
        else
            window.Show();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
