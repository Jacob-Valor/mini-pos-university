using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class BrandViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;

    [ObservableProperty]
    private Brand? _selectedBrand;

    partial void OnSelectedBrandChanged(Brand? value)
    {
        if (value != null)
        {
            BrandName = value.Name;
        }
        CanEditOrDelete = value != null;
    }

    [ObservableProperty]
    private string _brandName = string.Empty;

    partial void OnBrandNameChanged(string value)
    {
        CanAdd = !string.IsNullOrWhiteSpace(value);
    }

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => FilterBrands();

    public ObservableCollection<Brand> AllBrands { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();

    [ObservableProperty]
    private bool _canAdd;

    [ObservableProperty]
    private bool _canEditOrDelete;

    public BrandViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;
        _ = LoadDataAsync();
    }

    public BrandViewModel() : this(null!, null)
    {
    }

    private async Task LoadDataAsync()
    {
        if (_databaseService == null) return;

        AllBrands.Clear();
        var brands = await _databaseService.GetBrandsAsync();
        foreach (var b in brands) AllBrands.Add(b);
        FilterBrands();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(BrandName)) return;

        var maxId = AllBrands.Any()
            ? AllBrands.Max(b => int.TryParse(b.Id.Replace("B", ""), out var num) ? num : 0) + 1
            : 1;
        var newId = $"B{maxId:D3}";

        var brand = new Brand { Id = newId, Name = BrandName };

        bool success = await _databaseService.AddBrandAsync(brand);
        if (success)
        {
            UpsertBrand(brand);
            FilterBrands();
            Cancel();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ເພີ່ມຍີ່ຫໍ້ສຳເລັດ (Brand added)");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ເພີ່ມຍີ່ຫໍ້ບໍ່ສຳເລັດ (Failed to add brand)");
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditAsync()
    {
        if (SelectedBrand != null && !string.IsNullOrWhiteSpace(BrandName))
        {
            var updatedBrand = new Brand { Id = SelectedBrand.Id, Name = BrandName };
            bool success = await _databaseService.UpdateBrandAsync(updatedBrand);
            if (success)
            {
                UpsertBrand(updatedBrand);
                FilterBrands();
                Cancel();
                if (_dialogService != null)
                    await _dialogService.ShowSuccessAsync("ແກ້ໄຂຍີ່ຫໍ້ສຳເລັດ (Brand updated)");
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ແກ້ໄຂຍີ່ຫໍ້ບໍ່ສຳເລັດ (Failed to update brand)");
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedBrand != null)
        {
            bool confirm = true;
            if (_dialogService != null)
                confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ລຶບຍີ່ຫໍ້ {SelectedBrand.Name} ຫຼືບໍ່?");

            if (!confirm) return;

            bool success = await _databaseService.DeleteBrandAsync(SelectedBrand.Id);
            if (success)
            {
                RemoveBrandById(SelectedBrand.Id);
                FilterBrands();
                Cancel();
                if (_dialogService != null)
                    await _dialogService.ShowSuccessAsync("ລຶບຍີ່ຫໍ້ສຳເລັດ (Brand deleted)");
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ລຶບຍີ່ຫໍ້ບໍ່ສຳເລັດ (Failed to delete brand)");
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedBrand = null;
        BrandName = string.Empty;
    }

    private void UpsertBrand(Brand brand)
    {
        for (var i = 0; i < AllBrands.Count; i++)
        {
            if (AllBrands[i].Id == brand.Id)
            {
                AllBrands[i] = brand;
                return;
            }
        }
        AllBrands.Add(brand);
    }

    private void RemoveBrandById(string brandId)
    {
        for (var i = 0; i < AllBrands.Count; i++)
        {
            if (AllBrands[i].Id == brandId)
            {
                AllBrands.RemoveAt(i);
                return;
            }
        }
    }

    private void FilterBrands()
    {
        Brands.Clear();
        var query = AllBrands.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            query = query.Where(b => b.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var brand in query) Brands.Add(brand);
    }
}
