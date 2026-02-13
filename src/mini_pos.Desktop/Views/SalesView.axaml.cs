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
    private SalesViewModel? _vm;

    public SalesView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_vm != null)
            _vm.ShowReceiptRequested -= ViewModel_ShowReceiptRequested;

        _vm = DataContext as SalesViewModel;
        if (_vm != null)
            _vm.ShowReceiptRequested += ViewModel_ShowReceiptRequested;
    }

    private void ViewModel_ShowReceiptRequested(ReceiptViewModel receiptVm)
    {
        var sourceVm = _vm ?? DataContext as SalesViewModel;
        var window = new ReceiptWindow { DataContext = receiptVm };

        Action closeAction = window.Close;
        receiptVm.CloseDialogAction = closeAction;

        Action<decimal> confirmedAction = amount =>
        {
            if (sourceVm != null)
                sourceVm.MoneyReceived = amount;
        };
        receiptVm.PaymentConfirmedAction = confirmedAction;

        window.Closed += (_, _) =>
        {
            if (ReferenceEquals(receiptVm.CloseDialogAction, closeAction))
                receiptVm.CloseDialogAction = null;

            if (ReferenceEquals(receiptVm.PaymentConfirmedAction, confirmedAction))
                receiptVm.PaymentConfirmedAction = null;
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
