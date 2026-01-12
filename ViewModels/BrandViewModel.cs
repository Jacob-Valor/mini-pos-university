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

public partial class BrandViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;

    private Brand? _selectedBrand;
    public Brand? SelectedBrand
    {
        get => _selectedBrand;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedBrand, value);
            if (value != null)
            {
                BrandName = value.Name;
            }
        }
    }

    private string _brandName = string.Empty;
    public string BrandName
    {
        get => _brandName;
        set => this.RaiseAndSetIfChanged(ref _brandName, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterBrands();
        }
    }

    public ObservableCollection<Brand> AllBrands { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public BrandViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;

        var canAdd = this.WhenAnyValue(x => x.BrandName)
                         .Select(name => !string.IsNullOrWhiteSpace(name));

        AddCommand = ReactiveCommand.CreateFromTask(AddAsync, canAdd);

        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedBrand)
                                  .Select(x => x != null);

        EditCommand = ReactiveCommand.CreateFromTask(EditAsync, canEditOrDelete);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);

        _ = LoadDataAsync();
    }

    public BrandViewModel() : this(null!, null)
    {
        // Design-time
    }

    private async Task LoadDataAsync()
    {
        if (_databaseService == null) return;
        
        AllBrands.Clear();
        var brands = await _databaseService.GetBrandsAsync();
        foreach (var b in brands)
        {
            AllBrands.Add(b);
        }
        FilterBrands();
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(BrandName)) return;

        // Generate new ID
        var maxId = AllBrands.Any() 
            ? AllBrands.Max(b => int.TryParse(b.Id.Replace("B", ""), out var num) ? num : 0) + 1 
            : 1;
        var newId = $"B{maxId:D3}";
        
        var brand = new Brand { Id = newId, Name = BrandName };
        
        bool success = await _databaseService.AddBrandAsync(brand);
        if (success)
        {
            await LoadDataAsync();
            Cancel();
            if (_dialogService != null)
            {
                await _dialogService.ShowSuccessAsync("ເພີ່ມຍີ່ຫໍ້ສຳເລັດ (Brand added)");
            }
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ເພີ່ມຍີ່ຫໍ້ບໍ່ສຳເລັດ (Failed to add brand)");
        }
    }

    private async Task EditAsync()
    {
        if (SelectedBrand != null && !string.IsNullOrWhiteSpace(BrandName))
        {
            var updatedBrand = new Brand { Id = SelectedBrand.Id, Name = BrandName };
            bool success = await _databaseService.UpdateBrandAsync(updatedBrand);
            if (success)
            {
                await LoadDataAsync();
                Cancel();
                if (_dialogService != null)
                {
                    await _dialogService.ShowSuccessAsync("ແກ້ໄຂຍີ່ຫໍ້ສຳເລັດ (Brand updated)");
                }
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ແກ້ໄຂຍີ່ຫໍ້ບໍ່ສຳເລັດ (Failed to update brand)");
            }
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedBrand != null)
        {
            bool confirm = true;
            if (_dialogService != null)
            {
                confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ລຶບຍີ່ຫໍ້ {SelectedBrand.Name} ຫຼືບໍ່?");
            }

            if (!confirm) return;

            bool success = await _databaseService.DeleteBrandAsync(SelectedBrand.Id);
            if (success)
            {
                await LoadDataAsync();
                Cancel();
                if (_dialogService != null)
                {
                    await _dialogService.ShowSuccessAsync("ລຶບຍີ່ຫໍ້ສຳເລັດ (Brand deleted)");
                }
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ລຶບຍີ່ຫໍ້ບໍ່ສຳເລັດ (Failed to delete brand)");
            }
        }
    }

    private void Cancel()
    {
        SelectedBrand = null;
        BrandName = string.Empty;
    }

    private void FilterBrands()
    {
        Brands.Clear();
        var query = AllBrands.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(b => b.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var brand in query)
        {
            Brands.Add(brand);
        }
    }
}
