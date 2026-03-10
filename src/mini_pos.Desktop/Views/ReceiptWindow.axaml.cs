using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using mini_pos.ViewModels;

namespace mini_pos.Views;

public partial class ReceiptWindow : Window
{
    public ReceiptWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
