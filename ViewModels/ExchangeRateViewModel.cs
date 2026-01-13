using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using mini_pos.Models;
using mini_pos.Services;
using ReactiveUI;

namespace mini_pos.ViewModels;

public class ExchangeRateViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;

    private ExchangeRate? _selectedExchangeRate;
    public ExchangeRate? SelectedExchangeRate
    {
        get => _selectedExchangeRate;
        set => this.RaiseAndSetIfChanged(ref _selectedExchangeRate, value);
    }

    private string _usdRateInput = string.Empty;
    public string UsdRateInput
    {
        get => _usdRateInput;
        set => this.RaiseAndSetIfChanged(ref _usdRateInput, value);
    }

    private string _thbRateInput = string.Empty;
    public string ThbRateInput
    {
        get => _thbRateInput;
        set => this.RaiseAndSetIfChanged(ref _thbRateInput, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterExchangeRates();
        }
    }

    public ObservableCollection<ExchangeRate> AllExchangeRates { get; } = new();
    public ObservableCollection<ExchangeRate> ExchangeRates { get; } = new();

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; } // Probably not needed if we only keep history, but ok
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ExchangeRateViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;

        AddCommand = ReactiveCommand.CreateFromTask(AddAsync);
        
        var canDelete = this.WhenAnyValue(x => x.SelectedExchangeRate)
                            .Select(x => x != null);

        // Delete not implemented in DB service yet, usually we just keep history. 
        // But for completeness let's disable or implement mock behavior locally for list.
        // Actually I won't implement Delete in DB for rates to preserve history integrity for sales.
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, canDelete); 
        CancelCommand = ReactiveCommand.Create(Cancel);

        _ = LoadDataAsync();
    }

    public ExchangeRateViewModel() : this(null!, null)
    {
        // Design-time
    }

    private async Task LoadDataAsync()
    {
        if (_databaseService == null) return;

        AllExchangeRates.Clear();
        var history = await _databaseService.GetExchangeRateHistoryAsync();
        foreach (var rate in history)
        {
            AllExchangeRates.Add(rate);
        }
        FilterExchangeRates();
    }

    private async Task AddAsync()
    {
        if (!decimal.TryParse(UsdRateInput, out decimal usd) || !decimal.TryParse(ThbRateInput, out decimal thb))
        {
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ກະລຸນາປ້ອນອັດຕາແລກປ່ຽນທີ່ຖືກຕ້ອງ (Invalid exchange rate)");
            }
            return;
        }

        var newRate = new ExchangeRate
        {
            UsdRate = usd,
            ThbRate = thb,
            CreatedDate = DateTime.Now
        };

        bool success = await _databaseService.AddExchangeRateAsync(newRate);
        if (success)
        {
            await LoadDataAsync();
            Cancel();
            if (_dialogService != null)
            {
                await _dialogService.ShowSuccessAsync("ເພີ່ມອັດຕາແລກປ່ຽນສຳເລັດ (Rate added)");
            }
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ເພີ່ມອັດຕາແລກປ່ຽນບໍ່ສຳເລັດ (Failed to add rate)");
        }
    }

    private async Task DeleteAsync()
    {
        // Not implementing delete for audit trail reasons
        if (SelectedExchangeRate != null)
        {
            bool confirm = true;
            if (_dialogService != null)
            {
                confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", "ຈະລຶບອັດຕາແລກປ່ຽນຈາກລາຍການຊົ່ວຄາວບໍ?");
            }

            if (!confirm) return;

            // Just remove from UI for now if user persists
            AllExchangeRates.Remove(SelectedExchangeRate);
            FilterExchangeRates();
            Cancel();
        }
    }

    private void Cancel()
    {
        UsdRateInput = string.Empty;
        ThbRateInput = string.Empty;
        SelectedExchangeRate = null;
    }

    private void FilterExchangeRates()
    {
        ExchangeRates.Clear();
        var query = AllExchangeRates.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(x => x.CreatedDate.ToString().Contains(SearchText) || x.Id.ToString().Contains(SearchText));
        }

        query = query.OrderByDescending(x => x.CreatedDate);

        foreach (var rate in query)
        {
            ExchangeRates.Add(rate);
        }
    }
}
