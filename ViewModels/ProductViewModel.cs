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

                // When selecting, we try to match the Name to select the item in ComboBox
                // But internally we want to store the ID when saving.
                // The ComboBox binds to SelectedBrandItem (Brand object).
                // So we find the Brand object where Name matches value.Brand (which currently holds Name from DB read?)
                // Actually GetProductsAsync returns Brand ID in the Brand property? 
                // Let's check DatabaseService.GetProductsAsync implementation.
                // It does `reader.GetString("brand_id")`. So value.Brand holds the ID (e.g., "B001").
                
                SelectedBrandItem = Brands.FirstOrDefault(b => b.Id == value.Brand);
                SelectedTypeItem = ProductTypes.FirstOrDefault(t => t.Id == value.Type);
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

    public ProductViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;

        AddCommand = ReactiveCommand.CreateFromTask(AddAsync);
        
        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedProduct)
                                  .Select(x => x != null);

        EditCommand = ReactiveCommand.CreateFromTask(EditAsync, canEditOrDelete);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);
        
        // Initialize Statuses
        Statuses.Add("ມີ"); // Available
        Statuses.Add("ໝົດ"); // Out of Stock

        // Load Data
        _ = LoadDataAsync();
    }

    public ProductViewModel() : this(null!)
    {
        // Design-time
    }

    private async Task LoadDataAsync()
    {
        if (_databaseService == null) return;

        // 1. Load Reference Data
        Brands.Clear();
        var brands = await _databaseService.GetBrandsAsync();
        foreach (var b in brands) Brands.Add(b);

        ProductTypes.Clear();
        var types = await _databaseService.GetProductTypesAsync();
        foreach (var t in types) ProductTypes.Add(t);

        // 2. Load Products
        await RefreshProductList();
    }
    
    private async Task RefreshProductList()
    {
        AllProducts.Clear();
        var products = await _databaseService.GetProductsAsync();
        foreach (var p in products)
        {
            // Map DB ID to Object for display if needed, but current model stores ID in Brand/Type properties
            AllProducts.Add(p);
        }
        FilterProducts();
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(ProductId) || string.IsNullOrWhiteSpace(ProductName)) return;

        var newProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Unit = ProductUnit,
            Quantity = ProductQuantity,
            MinQuantity = ProductMinQuantity,
            CostPrice = ProductCostPrice,
            SellingPrice = ProductSellingPrice,
            Brand = SelectedBrandItem?.Id ?? "",
            Type = SelectedTypeItem?.Id ?? "",
            Status = SelectedStatusItem ?? ""
        };

        bool success = await _databaseService.AddProductAsync(newProduct);
        if (success)
        {
            await RefreshProductList();
            Cancel();
        }
    }

    private async Task EditAsync()
    {
        if (SelectedProduct != null)
        {
            var updatedProduct = new Product
            {
                Id = ProductId, // PK usually shouldn't change, assuming Barcode is immutable for now
                Name = ProductName,
                Unit = ProductUnit,
                Quantity = ProductQuantity,
                MinQuantity = ProductMinQuantity,
                CostPrice = ProductCostPrice,
                SellingPrice = ProductSellingPrice,
                Brand = SelectedBrandItem?.Id ?? "",
                Type = SelectedTypeItem?.Id ?? "",
                Status = SelectedStatusItem ?? ""
            };
            
            bool success = await _databaseService.UpdateProductAsync(updatedProduct);
            if (success)
            {
                await RefreshProductList();
                Cancel();
            }
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedProduct != null)
        {
            bool success = await _databaseService.DeleteProductAsync(SelectedProduct.Id);
            if (success)
            {
                await RefreshProductList();
                Cancel();
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
