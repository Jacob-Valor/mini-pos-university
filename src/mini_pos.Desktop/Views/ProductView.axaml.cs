using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mini_pos.Views;

public partial class ProductView : UserControl
{
    public ProductView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
