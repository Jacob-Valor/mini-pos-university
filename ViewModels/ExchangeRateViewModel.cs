using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using mini_pos.Models;
using ReactiveUI;

namespace mini_pos.ViewModels;

public class ExchangeRateViewModel : ViewModelBase
{
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
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    // public ReactiveCommand<Unit, Unit> RefreshCommand { get; } // Optional, can just be FilterExchangeRates

    public ExchangeRateViewModel()
    {
        // Mock Data
        AllExchangeRates.Add(new ExchangeRate { Id = 1, UsdRate = 2150.25m, ThbRate = 675.70m, CreatedDate = DateTime.Now.AddDays(-2) });
        AllExchangeRates.Add(new ExchangeRate { Id = 2, UsdRate = 2155.00m, ThbRate = 676.00m, CreatedDate = DateTime.Now.AddDays(-1) });
        AllExchangeRates.Add(new ExchangeRate { Id = 3, UsdRate = 2160.50m, ThbRate = 678.50m, CreatedDate = DateTime.Now });

        FilterExchangeRates();

        AddCommand = ReactiveCommand.Create(Add);
        
        var canDelete = this.WhenAnyValue(x => x.SelectedExchangeRate)
                            .Select(x => x != null);
        
        DeleteCommand = ReactiveCommand.Create(Delete, canDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void Add()
    {
        if (decimal.TryParse(UsdRateInput, out decimal usd) && decimal.TryParse(ThbRateInput, out decimal thb))
        {
            var newId = AllExchangeRates.Any() ? AllExchangeRates.Max(x => x.Id) + 1 : 1;
            var newRate = new ExchangeRate
            {
                Id = newId,
                UsdRate = usd,
                ThbRate = thb,
                CreatedDate = DateTime.Now
            };

            AllExchangeRates.Add(newRate);
            FilterExchangeRates();
            Cancel();
        }
    }

    private void Delete()
    {
        if (SelectedExchangeRate != null)
        {
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
            // Simple search by ID or maybe date string? Or verify if user wants to search by Rates logic? 
            // Usually search is for text fields. Let's just search by Date converted to string for now or ID.
            query = query.Where(x => x.CreatedDate.ToString().Contains(SearchText) || x.Id.ToString().Contains(SearchText));
        }
        
        // Order by latest
        query = query.OrderByDescending(x => x.CreatedDate);

        foreach (var rate in query)
        {
            ExchangeRates.Add(rate);
        }
    }
}
