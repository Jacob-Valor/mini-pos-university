using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using mini_pos.ViewModels;
using ReactiveUI;
using System;

namespace mini_pos.Views;

public partial class ReceiptWindow : ReactiveWindow<ReceiptViewModel>
{
    public ReceiptWindow()
    {
        InitializeComponent();
        this.WhenActivated(d => 
        {
            if (ViewModel != null)
            {
                d(ViewModel.CloseCommand.Subscribe(_ => Close()));
            }
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
