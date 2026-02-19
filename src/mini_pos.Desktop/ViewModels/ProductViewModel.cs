using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using FluentValidation;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mini_pos.Models;
using mini_pos.Services;
using mini_pos.Validators;

namespace mini_pos.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    private readonly IProductRepository _productRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IProductTypeRepository _productTypeRepository;
    private readonly IDialogService? _dialogService;
    private readonly IValidator<Product> _productValidator;

    [ObservableProperty]
    private Product? _selectedProduct;

    partial void OnSelectedProductChanged(Product? value)
    {
        if (value != null)
        {
            ProductId = value.Barcode;
            ProductName = value.ProductName;
            ProductUnit = value.Unit;
            ProductQuantity = value.Quantity;
            ProductMinQuantity = value.QuantityMin;
            ProductCostPrice = value.CostPrice;
            ProductSellingPrice = value.RetailPrice;
            ProductQuantityInput = value.Quantity.ToString("N0", CultureInfo.CurrentCulture);
            ProductMinQuantityInput = value.QuantityMin.ToString("N0", CultureInfo.CurrentCulture);
            ProductCostPriceInput = value.CostPrice.ToString("N0", CultureInfo.CurrentCulture);
            ProductSellingPriceInput = value.RetailPrice.ToString("N0", CultureInfo.CurrentCulture);
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
    private string _productQuantityInput = "0";

    [ObservableProperty]
    private string _productMinQuantityInput = "0";

    [ObservableProperty]
    private string _productCostPriceInput = "0";

    [ObservableProperty]
    private string _productSellingPriceInput = "0";

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

    public ProductViewModel(
        IProductRepository productRepository,
        IBrandRepository brandRepository,
        IProductTypeRepository productTypeRepository,
        IDialogService? dialogService = null,
        IValidator<Product>? productValidator = null)
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _productTypeRepository = productTypeRepository;
        _dialogService = dialogService;
        _productValidator = productValidator ?? new ProductValidator();
        Statuses.Add("ມີ");
        Statuses.Add("ໝົດ");
        _ = LoadDataAsync();
    }

    public ProductViewModel() : this(null!, null!, null!, null, null)
    {
    }

    private async Task LoadDataAsync()
    {
        if (_productRepository == null || _brandRepository == null || _productTypeRepository == null) return;

        Brands.Clear();
        var brands = await _brandRepository.GetBrandsAsync();
        foreach (var b in brands) Brands.Add(b);

        ProductTypes.Clear();
        var types = await _productTypeRepository.GetProductTypesAsync();
        foreach (var t in types) ProductTypes.Add(t);

        await RefreshProductList();
    }

    private async Task RefreshProductList()
    {
        AllProducts.Clear();
        var products = await _productRepository.GetProductsAsync();
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

    private async Task<bool> ValidateProductModelAsync(Product product)
    {
        var validationResult = _productValidator.Validate(product);
        if (validationResult.IsValid)
        {
            return true;
        }

        HasError = true;
        ErrorMessage = validationResult.Errors[0].ErrorMessage;
        if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync(ErrorMessage);
        }

        return false;
    }

    private async Task<bool> TryParseNumericInputsAsync()
    {
        if (!TryParseNonNegativeInt(ProductQuantityInput, out var quantity)
            || !TryParseNonNegativeInt(ProductMinQuantityInput, out var minQuantity)
            || !TryParseNonNegativeDecimal(ProductCostPriceInput, out var costPrice)
            || !TryParseNonNegativeDecimal(ProductSellingPriceInput, out var sellingPrice))
        {
            HasError = true;
            ErrorMessage = "ກະລຸນາປ້ອນຂໍ້ມູນຕົວເລກໃຫ້ຖືກຕ້ອງ";
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync(ErrorMessage);
            }

            return false;
        }

        ProductQuantity = quantity;
        ProductMinQuantity = minQuantity;
        ProductCostPrice = costPrice;
        ProductSellingPrice = sellingPrice;
        return true;
    }

    private static bool TryParseNonNegativeInt(string? text, out int value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = 0;
            return true;
        }

        var success = int.TryParse(
            text,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out value);

        return success && value >= 0;
    }

    private static bool TryParseNonNegativeDecimal(string? text, out decimal value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = 0;
            return true;
        }

        var success = decimal.TryParse(
            text,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out value);

        return success && value >= 0;
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

        if (!await TryParseNumericInputsAsync()) return;

        if (!await ValidateProductSelectionsAsync()) return;

        bool exists = await _productRepository.ProductExistsAsync(ProductId);
        if (exists)
        {
            HasError = true;
            ErrorMessage = $"ລະຫັດສິນຄ້າ {ProductId} ມີຢູ່ໃນລະບົບແລ້ວ";
            if (_dialogService != null) await _dialogService.ShowErrorAsync(ErrorMessage);
            return;
        }

        var newProduct = new Product
        {
            Barcode = ProductId,
            ProductName = ProductName,
            Unit = ProductUnit,
            Quantity = ProductQuantity,
            QuantityMin = ProductMinQuantity,
            CostPrice = ProductCostPrice,
            RetailPrice = ProductSellingPrice,
            BrandId = SelectedBrandItem?.Id ?? "",
            BrandName = SelectedBrandItem?.Name ?? "",
            CategoryId = SelectedTypeItem?.Id ?? "",
            CategoryName = SelectedTypeItem?.Name ?? "",
            Status = SelectedStatusItem ?? ""
        };

        if (!await ValidateProductModelAsync(newProduct))
        {
            return;
        }

        bool success = await _productRepository.AddProductAsync(newProduct);
        if (success)
        {
            UpsertProduct(newProduct);
            FilterProducts();
            Cancel();
            if (_dialogService != null) await _dialogService.ShowSuccessAsync("ເພີ່ມຂໍ້ມູນສຳເລັດ");
        }
        else
        {
            if (_dialogService != null) await _dialogService.ShowErrorAsync("ບັນທຶກຂໍ້ມູນບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedProduct == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາເລືອກສິນຄ້າກ່ອນ");
            return;
        }

        HasError = false;
        ErrorMessage = string.Empty;

        if (!await TryParseNumericInputsAsync()) return;

        if (!await ValidateProductSelectionsAsync()) return;

        var updatedProduct = new Product
        {
            Barcode = ProductId,
            ProductName = ProductName,
            Unit = ProductUnit,
            Quantity = ProductQuantity,
            QuantityMin = ProductMinQuantity,
            CostPrice = ProductCostPrice,
            RetailPrice = ProductSellingPrice,
            BrandId = SelectedBrandItem?.Id ?? "",
            BrandName = SelectedBrandItem?.Name ?? "",
            CategoryId = SelectedTypeItem?.Id ?? "",
            CategoryName = SelectedTypeItem?.Name ?? "",
            Status = SelectedStatusItem ?? ""
        };

        if (!await ValidateProductModelAsync(updatedProduct))
        {
            return;
        }

        bool success = await _productRepository.UpdateProductAsync(updatedProduct);
        if (success)
        {
            UpsertProduct(updatedProduct);
            FilterProducts();
            Cancel();
            if (_dialogService != null) await _dialogService.ShowSuccessAsync("ແກ້ໄຂຂໍ້ມູນສຳເລັດ");
        }
        else
        {
            if (_dialogService != null) await _dialogService.ShowErrorAsync("ແກ້ໄຂຂໍ້ມູນບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedProduct == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາເລືອກສິນຄ້າກ່ອນ");
            return;
        }

        bool confirm = true;
        if (_dialogService != null)
            confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ທ່ານຕ້ອງການລຶບສິນຄ້າ {SelectedProduct.ProductName} ຫຼືບໍ່?");

        if (!confirm) return;

        bool success = await _productRepository.DeleteProductAsync(SelectedProduct.Barcode);
        if (success)
        {
            RemoveProductById(SelectedProduct.Barcode);
            FilterProducts();
            Cancel();
            if (_dialogService != null) await _dialogService.ShowSuccessAsync("ລຶບຂໍ້ມູນສຳເລັດ");
        }
        else
        {
            if (_dialogService != null) await _dialogService.ShowErrorAsync("ລຶບຂໍ້ມູນບໍ່ສຳເລັດ");
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
        ProductQuantityInput = "0";
        ProductMinQuantityInput = "0";
        ProductCostPriceInput = "0";
        ProductSellingPriceInput = "0";
        SelectedBrandItem = null;
        SelectedTypeItem = null;
        SelectedStatusItem = null;
    }

    private void UpsertProduct(Product product)
    {
        for (var i = 0; i < AllProducts.Count; i++)
        {
            if (AllProducts[i].Barcode == product.Barcode)
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
            if (AllProducts[i].Barcode == productId)
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
                p.ProductName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Barcode.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var p in query) Products.Add(p);
    }
}
