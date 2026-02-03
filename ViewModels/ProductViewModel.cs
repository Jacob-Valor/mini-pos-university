using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;

    [ObservableProperty]
    private Product? _selectedProduct;

    partial void OnSelectedProductChanged(Product? value)
    {
        if (value != null)
        {
            ProductId = value.Id;
            ProductName = value.Name;
            ProductUnit = value.Unit;
            ProductQuantity = value.Quantity;
            ProductMinQuantity = value.MinQuantity;
            ProductCostPrice = value.CostPrice;
            ProductSellingPrice = value.SellingPrice;
            SelectedBrandItem = Brands.FirstOrDefault(b => b.Id == value.BrandId);
            SelectedTypeItem = ProductTypes.FirstOrDefault(t => t.Id == value.CategoryId);
            SelectedStatusItem = value.Status;
        }
        CanEditOrDelete = value != null;
    }

    [ObservableProperty]
    private string _productId = string.Empty;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _productUnit = string.Empty;

    [ObservableProperty]
    private int _productQuantity;

    [ObservableProperty]
    private int _productMinQuantity;

    [ObservableProperty]
    private decimal _productCostPrice;

    [ObservableProperty]
    private decimal _productSellingPrice;

    [ObservableProperty]
    private Brand? _selectedBrandItem;

    [ObservableProperty]
    private ProductType? _selectedTypeItem;

    [ObservableProperty]
    private string? _selectedStatusItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => FilterProducts();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _canEditOrDelete;

    public ObservableCollection<Product> AllProducts { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<ProductType> ProductTypes { get; } = new();
    public ObservableCollection<string> Statuses { get; } = new();

    public ProductViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;
        Statuses.Add("ມີ");
        Statuses.Add("ໝົດ");
        _ = LoadDataAsync();
    }

    public ProductViewModel() : this(null!, null)
    {
    }

    private async Task LoadDataAsync()
    {
        if (_databaseService == null) return;

        Brands.Clear();
        var brands = await _databaseService.GetBrandsAsync();
        foreach (var b in brands) Brands.Add(b);

        ProductTypes.Clear();
        var types = await _databaseService.GetProductTypesAsync();
        foreach (var t in types) ProductTypes.Add(t);

        await RefreshProductList();
    }

    private async Task RefreshProductList()
    {
        AllProducts.Clear();
        var products = await _databaseService.GetProductsAsync();
        foreach (var p in products) AllProducts.Add(p);
        FilterProducts();
    }

    private async Task<bool> ValidateProductSelectionsAsync()
    {
        if (SelectedBrandItem == null || SelectedTypeItem == null || string.IsNullOrWhiteSpace(SelectedStatusItem))
        {
            HasError = true;
            ErrorMessage = "ກະລຸນາເລືອກຍີ່ຫໍ້, ປະເພດ ແລະ ສະຖານະ";
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync(ErrorMessage);
            return false;
        }
        return true;
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ProductId) || string.IsNullOrWhiteSpace(ProductName))
        {
            HasError = true;
            ErrorMessage = "ກະລຸນາປ້ອນລະຫັດ ແລະ ຊື່ສິນຄ້າ";
            if (_dialogService != null) await _dialogService.ShowErrorAsync(ErrorMessage);
            return;
        }

        if (!await ValidateProductSelectionsAsync()) return;

        bool exists = await _databaseService.ProductExistsAsync(ProductId);
        if (exists)
        {
            HasError = true;
            ErrorMessage = $"ລະຫັດສິນຄ້າ {ProductId} ມີຢູ່ໃນລະບົບແລ້ວ (Duplicate Barcode)";
            if (_dialogService != null) await _dialogService.ShowErrorAsync(ErrorMessage);
            return;
        }

        var newProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Unit = ProductUnit,
            Quantity = ProductQuantity,
            MinQuantity = ProductMinQuantity,
            CostPrice = ProductCostPrice,
            SellingPrice = ProductSellingPrice,
            BrandId = SelectedBrandItem?.Id ?? "",
            BrandName = SelectedBrandItem?.Name ?? "",
            CategoryId = SelectedTypeItem?.Id ?? "",
            CategoryName = SelectedTypeItem?.Name ?? "",
            Status = SelectedStatusItem ?? ""
        };

        bool success = await _databaseService.AddProductAsync(newProduct);
        if (success)
        {
            UpsertProduct(newProduct);
            FilterProducts();
            Cancel();
            if (_dialogService != null) await _dialogService.ShowSuccessAsync("ເພີ່ມຂໍ້ມູນສຳເລັດ (Added Successfully)");
        }
        else
        {
            if (_dialogService != null) await _dialogService.ShowErrorAsync("ບັນທຶກຂໍ້ມູນບໍ່ສຳເລັດ (Failed to save)");
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditAsync()
    {
        if (SelectedProduct != null)
        {
            HasError = false;
            ErrorMessage = string.Empty;

            if (!await ValidateProductSelectionsAsync()) return;

            var updatedProduct = new Product
            {
                Id = ProductId,
                Name = ProductName,
                Unit = ProductUnit,
                Quantity = ProductQuantity,
                MinQuantity = ProductMinQuantity,
                CostPrice = ProductCostPrice,
                SellingPrice = ProductSellingPrice,
                BrandId = SelectedBrandItem?.Id ?? "",
                BrandName = SelectedBrandItem?.Name ?? "",
                CategoryId = SelectedTypeItem?.Id ?? "",
                CategoryName = SelectedTypeItem?.Name ?? "",
                Status = SelectedStatusItem ?? ""
            };

            bool success = await _databaseService.UpdateProductAsync(updatedProduct);
            if (success)
            {
                UpsertProduct(updatedProduct);
                FilterProducts();
                Cancel();
                if (_dialogService != null) await _dialogService.ShowSuccessAsync("ແກ້ໄຂຂໍ້ມູນສຳເລັດ (Updated Successfully)");
            }
            else
            {
                if (_dialogService != null) await _dialogService.ShowErrorAsync("ແກ້ໄຂຂໍ້ມູນບໍ່ສຳເລັດ (Failed to update)");
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedProduct != null)
        {
            bool confirm = true;
            if (_dialogService != null)
                confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ທ່ານຕ້ອງການລຶບສິນຄ້າ {SelectedProduct.Name} ຫຼືບໍ່?");

            if (confirm)
            {
                bool success = await _databaseService.DeleteProductAsync(SelectedProduct.Id);
                if (success)
                {
                    RemoveProductById(SelectedProduct.Id);
                    FilterProducts();
                    Cancel();
                    if (_dialogService != null) await _dialogService.ShowSuccessAsync("ລຶບຂໍ້ມູນສຳເລັດ (Deleted Successfully)");
                }
                else
                {
                    if (_dialogService != null) await _dialogService.ShowErrorAsync("ລຶບຂໍ້ມູນບໍ່ສຳເລັດ (Failed to delete)");
                }
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedProduct = null;
        ProductId = string.Empty;
        ProductName = string.Empty;
        ProductUnit = string.Empty;
        ProductQuantity = 0;
        ProductMinQuantity = 0;
        ProductCostPrice = 0;
        ProductSellingPrice = 0;
        SelectedBrandItem = null;
        SelectedTypeItem = null;
        SelectedStatusItem = null;
    }

    private void UpsertProduct(Product product)
    {
        for (var i = 0; i < AllProducts.Count; i++)
        {
            if (AllProducts[i].Id == product.Id)
            {
                AllProducts[i] = product;
                return;
            }
        }
        AllProducts.Add(product);
    }

    private void RemoveProductById(string productId)
    {
        for (var i = 0; i < AllProducts.Count; i++)
        {
            if (AllProducts[i].Id == productId)
            {
                AllProducts.RemoveAt(i);
                return;
            }
        }
    }

    private void FilterProducts()
    {
        Products.Clear();
        var query = AllProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var p in query) Products.Add(p);
    }
}
