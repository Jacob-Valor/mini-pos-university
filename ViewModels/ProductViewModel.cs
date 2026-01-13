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

public partial class ProductViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;

    private Product? _selectedProduct;
    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedProduct, value);
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
        }
    }

    private string _productId = string.Empty;
    public string ProductId
    {
        get => _productId;
        set => this.RaiseAndSetIfChanged(ref _productId, value);
    }

    private string _productName = string.Empty;
    public string ProductName
    {
        get => _productName;
        set => this.RaiseAndSetIfChanged(ref _productName, value);
    }

    private string _productUnit = string.Empty;
    public string ProductUnit
    {
        get => _productUnit;
        set => this.RaiseAndSetIfChanged(ref _productUnit, value);
    }

    private int _productQuantity;
    public int ProductQuantity
    {
        get => _productQuantity;
        set => this.RaiseAndSetIfChanged(ref _productQuantity, value);
    }

    private int _productMinQuantity;
    public int ProductMinQuantity
    {
        get => _productMinQuantity;
        set => this.RaiseAndSetIfChanged(ref _productMinQuantity, value);
    }

    private decimal _productCostPrice;
    public decimal ProductCostPrice
    {
        get => _productCostPrice;
        set => this.RaiseAndSetIfChanged(ref _productCostPrice, value);
    }

    private decimal _productSellingPrice;
    public decimal ProductSellingPrice
    {
        get => _productSellingPrice;
        set => this.RaiseAndSetIfChanged(ref _productSellingPrice, value);
    }

    private Brand? _selectedBrandItem;
    public Brand? SelectedBrandItem
    {
        get => _selectedBrandItem;
        set => this.RaiseAndSetIfChanged(ref _selectedBrandItem, value);
    }

    private ProductType? _selectedTypeItem;
    public ProductType? SelectedTypeItem
    {
        get => _selectedTypeItem;
        set => this.RaiseAndSetIfChanged(ref _selectedTypeItem, value);
    }

    private string? _selectedStatusItem;
    public string? SelectedStatusItem
    {
        get => _selectedStatusItem;
        set => this.RaiseAndSetIfChanged(ref _selectedStatusItem, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterProducts();
        }
    }

    public ObservableCollection<Product> AllProducts { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();

    public ObservableCollection<Brand> Brands { get; } = new();
    public ObservableCollection<ProductType> ProductTypes { get; } = new();
    public ObservableCollection<string> Statuses { get; } = new();

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ProductViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;

        AddCommand = ReactiveCommand.CreateFromTask(AddAsync);

        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedProduct)
            .Select(x => x != null);

        EditCommand = ReactiveCommand.CreateFromTask(EditAsync, canEditOrDelete);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);

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
        foreach (var p in products)
        {
            AllProducts.Add(p);
        }
        FilterProducts();
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set => this.RaiseAndSetIfChanged(ref _hasError, value);
    }

    private async Task<bool> ValidateProductSelectionsAsync()
    {
        if (SelectedBrandItem == null || SelectedTypeItem == null || string.IsNullOrWhiteSpace(SelectedStatusItem))
        {
            HasError = true;
            ErrorMessage = "ກະລຸນາເລືອກຍີ່ຫໍ້, ປະເພດ ແລະ ສະຖານະ";
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync(ErrorMessage);
            }
            return false;
        }

        return true;
    }

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

        if (!await ValidateProductSelectionsAsync())
        {
            return;
        }

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

    private async Task EditAsync()
    {
        if (SelectedProduct != null)
        {
            HasError = false;
            ErrorMessage = string.Empty;

            if (!await ValidateProductSelectionsAsync())
            {
                return;
            }

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

    private async Task DeleteAsync()
    {
        if (SelectedProduct != null)
        {
            bool confirm = true;
            if (_dialogService != null)
            {
                confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ທ່ານຕ້ອງການລຶບສິນຄ້າ {SelectedProduct.Name} ຫຼືບໍ່?");
            }

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

        foreach (var p in query)
        {
            Products.Add(p);
        }
    }
}
