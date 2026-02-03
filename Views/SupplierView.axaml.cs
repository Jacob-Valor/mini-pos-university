using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using mini_pos.ViewModels;

namespace mini_pos.Views;

public partial class SupplierView : UserControl
{
    public SupplierView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SupplierViewModel vm)
        {
            vm.CloseDialogAction = CloseDialog;
        }
    }

    private void CloseDialog()
    {
        var parentWindow = this.FindAncestorOfType<Window>();
        parentWindow?.Close();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
