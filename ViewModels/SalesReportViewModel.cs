using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;
using Serilog;

namespace mini_pos.ViewModels;

public partial class SalesReportViewModel : ViewModelBase
{
    private readonly ISalesRepository _salesRepository;
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

    public SalesReportViewModel(ISalesRepository salesRepository, IReportService? reportService = null, IDialogService? dialogService = null)
    {
        _salesRepository = salesRepository;
        _reportService = reportService;
        _dialogService = dialogService;
    }

    public SalesReportViewModel() : this(null!, null, null)
    {
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (_salesRepository == null) return;

        if (StartDate > EndDate)
        {
            await (_dialogService?.ShowErrorAsync("ວັນທີເລີ່ມຕ້ອງນ້ອຍກວ່າ ຫຼື ເທົ່າກັບ ວັນທີສິ້ນສຸດ") ?? Task.CompletedTask);
            return;
        }

        var items = await _salesRepository.GetSalesReportAsync(StartDate.DateTime, EndDate.DateTime);
        ReportItems = new ObservableCollection<SalesReportItem>(items);

        TotalAmount = items.Sum(x => x.Total);
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        if (_reportService == null)
        {
            await (_dialogService?.ShowErrorAsync("ບໍ່ມີບໍລິການສ້າງລາຍງານ") ?? Task.CompletedTask);
            return;
        }

        if (ReportItems.Count == 0)
        {
            await (_dialogService?.ShowErrorAsync("ບໍ່ມີຂໍ້ມູນໃນລາຍງານ") ?? Task.CompletedTask);
            return;
        }

        try
        {
            var fileName = $"SalesReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            var outputDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrWhiteSpace(outputDir) || !System.IO.Directory.Exists(outputDir))
                outputDir = System.IO.Directory.GetCurrentDirectory();

            var path = System.IO.Path.Combine(outputDir, fileName);

            var itemsList = new System.Collections.Generic.List<SalesReportItem>(ReportItems);

            _reportService.GenerateSalesReport(itemsList, StartDate.DateTime, EndDate.DateTime, TotalAmount, path);

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                Log.Warning("Generated PDF at {Path} but could not open automatically", path);
            }

            await (_dialogService?.ShowSuccessAsync($"ສ້າງໃບລາຍງານສຳເລັດ: {fileName}\n{path}") ?? Task.CompletedTask);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating sales report");
            await (_dialogService?.ShowErrorAsync("ເກີດຂໍ້ຜິດພາດ ບໍ່ສາມາດສ້າງລາຍງານໄດ້") ?? Task.CompletedTask);
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
