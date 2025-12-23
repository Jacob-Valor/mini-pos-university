using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mini_pos.Views;

public partial class SupplierWindow : Window
{
    public SupplierWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
