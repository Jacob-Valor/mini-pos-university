using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mini_pos.Views;

public partial class ExchangeRateView : UserControl
{
    public ExchangeRateView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
