using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class ExchangeRateViewModel : ViewModelBase
{
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly IDialogService? _dialogService;

    [ObservableProperty]
    private ExchangeRate? _selectedExchangeRate;

    [ObservableProperty]
    private string _usdRateInput = string.Empty;

    [ObservableProperty]
    private string _thbRateInput = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => FilterExchangeRates();

    [ObservableProperty]
    private bool _canDelete;

    partial void OnSelectedExchangeRateChanged(ExchangeRate? value) => CanDelete = value != null;

    public ObservableCollection<ExchangeRate> AllExchangeRates { get; } = new();
    public ObservableCollection<ExchangeRate> ExchangeRates { get; } = new();

    public ExchangeRateViewModel(IExchangeRateRepository exchangeRateRepository, IDialogService? dialogService = null)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _dialogService = dialogService;
        _ = LoadDataAsync();
    }

    public ExchangeRateViewModel() : this(null!, null)
    {
    }

    private async Task LoadDataAsync()
    {
        if (_exchangeRateRepository == null) return;

        AllExchangeRates.Clear();
        var history = await _exchangeRateRepository.GetExchangeRateHistoryAsync();
        foreach (var rate in history) AllExchangeRates.Add(rate);
        FilterExchangeRates();
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (!decimal.TryParse(UsdRateInput, out decimal usd) || !decimal.TryParse(ThbRateInput, out decimal thb))
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາປ້ອນອັດຕາແລກປ່ຽນທີ່ຖືກຕ້ອງ");
            return;
        }

        var newRate = new ExchangeRate
        {
            UsdRate = usd,
            ThbRate = thb,
            CreatedDate = DateTime.Now
        };

        bool success = await _exchangeRateRepository.AddExchangeRateAsync(newRate);
        if (success)
        {
            await LoadDataAsync();
            Cancel();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ເພີ່ມອັດຕາແລກປ່ຽນສຳເລັດ");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ເພີ່ມອັດຕາແລກປ່ຽນບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(ExchangeRate? exchangeRate)
    {
        exchangeRate ??= SelectedExchangeRate;
        if (exchangeRate == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາເລືອກອັດຕາແລກປ່ຽນກ່ອນ");
            return;
        }

        bool confirm = true;
        if (_dialogService != null)
            confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", "ຈະລຶບອັດຕາແລກປ່ຽນຈາກລາຍການຊົ່ວຄາວບໍ?");

        if (!confirm) return;

        AllExchangeRates.Remove(exchangeRate);
        FilterExchangeRates();
        Cancel();
    }

    [RelayCommand]
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

        foreach (var rate in query) ExchangeRates.Add(rate);
    }
}
