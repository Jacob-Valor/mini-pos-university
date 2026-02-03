using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class ProductTypeViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;

    [ObservableProperty]
    private ProductType? _selectedProductType;

    partial void OnSelectedProductTypeChanged(ProductType? value)
    {
        if (value != null) ProductTypeName = value.Name;
        CanEditOrDelete = value != null;
    }

    [ObservableProperty]
    private string _productTypeName = string.Empty;

    partial void OnProductTypeNameChanged(string value)
    {
        CanAdd = !string.IsNullOrWhiteSpace(value);
    }

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => FilterProductTypes();

    public ObservableCollection<ProductType> AllProductTypes { get; } = new();
    public ObservableCollection<ProductType> ProductTypes { get; } = new();

    [ObservableProperty]
    private bool _canAdd;

    [ObservableProperty]
    private bool _canEditOrDelete;

    public ProductTypeViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;
        _ = LoadDataAsync();
    }

    public ProductTypeViewModel() : this(null!, null)
    {
    }

    private async Task LoadDataAsync()
    {
        if (_databaseService == null) return;

        AllProductTypes.Clear();
        var list = await _databaseService.GetProductTypesAsync();
        foreach (var t in list) AllProductTypes.Add(t);
        FilterProductTypes();
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(ProductTypeName)) return;

        var maxId = AllProductTypes.Any()
            ? AllProductTypes.Max(t => int.TryParse(t.Id.Replace("C", ""), out var num) ? num : 0) + 1
            : 1;
        var newId = $"C{maxId:D3}";

        var newType = new ProductType { Id = newId, Name = ProductTypeName };

        bool success = await _databaseService.AddProductTypeAsync(newType);
        if (success)
        {
            UpsertProductType(newType);
            FilterProductTypes();
            Cancel();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ເພີ່ມປະເພດສິນຄ້າສຳເລັດ (Type added)");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ເພີ່ມປະເພດສິນຄ້າບໍ່ສຳເລັດ (Failed to add type)");
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditAsync()
    {
        if (SelectedProductType != null && !string.IsNullOrWhiteSpace(ProductTypeName))
        {
            var updatedType = new ProductType { Id = SelectedProductType.Id, Name = ProductTypeName };
            bool success = await _databaseService.UpdateProductTypeAsync(updatedType);
            if (success)
            {
                UpsertProductType(updatedType);
                FilterProductTypes();
                Cancel();
                if (_dialogService != null)
                    await _dialogService.ShowSuccessAsync("ແກ້ໄຂປະເພດສິນຄ້າສຳເລັດ (Type updated)");
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ແກ້ໄຂປະເພດສິນຄ້າບໍ່ສຳເລັດ (Failed to update type)");
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedProductType != null)
        {
            bool confirm = true;
            if (_dialogService != null)
                confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ລຶບປະເພດ {SelectedProductType.Name} ຫຼືບໍ່?");

            if (!confirm) return;

            bool success = await _databaseService.DeleteProductTypeAsync(SelectedProductType.Id);
            if (success)
            {
                RemoveProductTypeById(SelectedProductType.Id);
                FilterProductTypes();
                Cancel();
                if (_dialogService != null)
                    await _dialogService.ShowSuccessAsync("ລຶບປະເພດສິນຄ້າສຳເລັດ (Type deleted)");
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ລຶບປະເພດສິນຄ້າບໍ່ສຳເລັດ (Failed to delete type)");
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedProductType = null;
        ProductTypeName = string.Empty;
    }

    private void UpsertProductType(ProductType productType)
    {
        for (var i = 0; i < AllProductTypes.Count; i++)
        {
            if (AllProductTypes[i].Id == productType.Id)
            {
                AllProductTypes[i] = productType;
                return;
            }
        }
        AllProductTypes.Add(productType);
    }

    private void RemoveProductTypeById(string productTypeId)
    {
        for (var i = 0; i < AllProductTypes.Count; i++)
        {
            if (AllProductTypes[i].Id == productTypeId)
            {
                AllProductTypes.RemoveAt(i);
                return;
            }
        }
    }

    private void FilterProductTypes()
    {
        ProductTypes.Clear();
        var query = AllProductTypes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            query = query.Where(b => b.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var type in query) ProductTypes.Add(type);
    }
}
