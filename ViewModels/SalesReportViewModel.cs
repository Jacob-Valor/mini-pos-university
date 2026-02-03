using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class SalesReportViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IReportService? _reportService;
    private readonly IDialogService? _dialogService;

    [ObservableProperty]
    private DateTimeOffset _startDate = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset _endDate = DateTimeOffset.Now;

    [ObservableProperty]
    private ObservableCollection<SalesReportItem> _reportItems = new();

    [ObservableProperty]
    private decimal _totalAmount;

    public SalesReportViewModel(IDatabaseService databaseService, IReportService? reportService = null, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _reportService = reportService;
        _dialogService = dialogService;
    }

    public SalesReportViewModel() : this(null!, null, null)
    {
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (_databaseService == null) return;

        var items = await _databaseService.GetSalesReportAsync(StartDate.DateTime, EndDate.DateTime);
        ReportItems = new ObservableCollection<SalesReportItem>(items);

        decimal total = 0;
        foreach (var item in items) total += item.Total;
        TotalAmount = total;
    }

    [RelayCommand]
    private void Print()
    {
        if (_reportService == null || ReportItems.Count == 0) return;

        try
        {
            var fileName = $"SalesReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), fileName);

            var itemsList = new System.Collections.Generic.List<SalesReportItem>(ReportItems);

            _reportService.GenerateSalesReport(itemsList, StartDate.DateTime, EndDate.DateTime, TotalAmount, path);

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = path,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                Console.WriteLine($"Generated PDF at {path}. Could not open automatically.");
            }

            _dialogService?.ShowSuccessAsync($"ສ້າງໃບລາຍງານສຳເລັດ (Report Generated): {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating report: {ex.Message}");
            _dialogService?.ShowErrorAsync($"ເກີດຂໍ້ຜິດພາດ: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Reset()
    {
        StartDate = DateTimeOffset.Now;
        EndDate = DateTimeOffset.Now;
        ReportItems.Clear();
        TotalAmount = 0;
    }
}
