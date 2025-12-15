using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using mini_pos.Models;
using ReactiveUI;

namespace mini_pos.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
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
                
                SelectedBrandItem = Brands.FirstOrDefault(b => b.Name == value.Brand);
                SelectedTypeItem = ProductTypes.FirstOrDefault(t => t.Name == value.Type);
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

    public ProductViewModel()
    {
        // Mock Reference Data
        Brands.Add(new Brand { Id = 1, Name = "Pepsi" });
        Brands.Add(new Brand { Id = 2, Name = "Coca-Cola" });
        Brands.Add(new Brand { Id = 3, Name = "Nestle" });
        Brands.Add(new Brand { Id = 4, Name = "Lao Brewery" });

        ProductTypes.Add(new ProductType { Id = 1, Name = "Drinks" });
        ProductTypes.Add(new ProductType { Id = 2, Name = "Snacks" });
        ProductTypes.Add(new ProductType { Id = 3, Name = "Household" });

        Statuses.Add("ມີ"); // Available
        Statuses.Add("ໝົດ"); // Out of Stock

        // Mock Product Data
        AllProducts.Add(new Product 
        { 
            Id = "001", 
            Name = "Pepsi 330ml", 
            Unit = "Can", 
            Quantity = 50, 
            MinQuantity = 10, 
            CostPrice = 4000, 
            SellingPrice = 5000, 
            Brand = "Pepsi", 
            Type = "Drinks", 
            Status = "ມີ" 
        });
        
         AllProducts.Add(new Product 
        { 
            Id = "002", 
            Name = "Lays Classic", 
            Unit = "Pack", 
            Quantity = 20, 
            MinQuantity = 5, 
            CostPrice = 8000, 
            SellingPrice = 10000, 
            Brand = "Lays", 
            Type = "Snacks", 
            Status = "ມີ" 
        });

        FilterProducts();

        AddCommand = ReactiveCommand.Create(Add);
        
        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedProduct)
                                  .Select(x => x != null);

        EditCommand = ReactiveCommand.Create(Edit, canEditOrDelete);
        DeleteCommand = ReactiveCommand.Create(Delete, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void Add()
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
            Brand = SelectedBrandItem?.Name ?? "",
            Type = SelectedTypeItem?.Name ?? "",
            Status = SelectedStatusItem ?? ""
        };

        AllProducts.Add(newProduct);
        FilterProducts();
        
        // Reset inputs
        Cancel(); 
    }

    private void Edit()
    {
        if (SelectedProduct != null)
        {
            var index = AllProducts.IndexOf(SelectedProduct);
            if (index != -1)
            {
                var updatedProduct = new Product
                {
                    Id = ProductId,
                    Name = ProductName,
                    Unit = ProductUnit,
                    Quantity = ProductQuantity,
                    MinQuantity = ProductMinQuantity,
                    CostPrice = ProductCostPrice,
                    SellingPrice = ProductSellingPrice,
                    Brand = SelectedBrandItem?.Name ?? "",
                    Type = SelectedTypeItem?.Name ?? "",
                    Status = SelectedStatusItem ?? ""
                };
                AllProducts[index] = updatedProduct;
            }
            FilterProducts();
            Cancel();
        }
    }

    private void Delete()
    {
        if (SelectedProduct != null)
        {
            AllProducts.Remove(SelectedProduct);
            FilterProducts();
            Cancel();
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
