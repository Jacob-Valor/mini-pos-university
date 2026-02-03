using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mini_pos.Views;

public partial class SalesReportView : UserControl
{
    public SalesReportView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
