using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mini_pos.Views;

public partial class ProductTypeView : UserControl
{
    public ProductTypeView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
