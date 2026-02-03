using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using mini_pos.Models;
using mini_pos.Services;
using ReactiveUI;

namespace mini_pos.ViewModels;

public class SalesReportViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IReportService? _reportService;
    private readonly IDialogService? _dialogService;

    private DateTimeOffset _startDate = DateTimeOffset.Now;
    public DateTimeOffset StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    private DateTimeOffset _endDate = DateTimeOffset.Now;
    public DateTimeOffset EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }

    private ObservableCollection<SalesReportItem> _reportItems = new();
    public ObservableCollection<SalesReportItem> ReportItems
    {
        get => _reportItems;
        set => this.RaiseAndSetIfChanged(ref _reportItems, value);
    }

    private decimal _totalAmount;
    public decimal TotalAmount
    {
        get => _totalAmount;
        set => this.RaiseAndSetIfChanged(ref _totalAmount, value);
    }

    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<Unit, Unit> PrintCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }

    public SalesReportViewModel(IDatabaseService databaseService, IReportService? reportService = null, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _reportService = reportService;
        _dialogService = dialogService;

        SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync);
        PrintCommand = ReactiveCommand.Create(Print);
        ResetCommand = ReactiveCommand.Create(Reset);

        // Load data initially if needed, or wait for user to click search
    }

    // Design-time constructor
    // Design-time constructor
    public SalesReportViewModel() : this(null!, null, null)
    {
    }

    private async Task SearchAsync()
    {
        if (_databaseService == null) return;

        var items = await _databaseService.GetSalesReportAsync(StartDate.DateTime, EndDate.DateTime);
        ReportItems = new ObservableCollection<SalesReportItem>(items);
        
        decimal total = 0;
        foreach (var item in items)
        {
            total += item.Total;
        }
        TotalAmount = total;
    }

    private void Print()
    {
        if (_reportService == null || ReportItems.Count == 0) return;

        try 
        {
            var fileName = $"SalesReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), fileName);
            
            // Convert ObservableCollection to List
            var itemsList = new System.Collections.Generic.List<SalesReportItem>(ReportItems);
            
            _reportService.GenerateSalesReport(itemsList, StartDate.DateTime, EndDate.DateTime, TotalAmount, path);
            
            // Try to open the file (Linux specific)
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
                // Ignore if we can't open it automatically
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

    private void Reset()
    {
        StartDate = DateTimeOffset.Now;
        EndDate = DateTimeOffset.Now;
        ReportItems.Clear();
        TotalAmount = 0;
    }
}
