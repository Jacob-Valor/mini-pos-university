using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using mini_pos.Models;
using ReactiveUI;

namespace mini_pos.ViewModels;

public partial class ProductTypeViewModel : ViewModelBase
{
    private ProductType? _selectedProductType;
    public ProductType? SelectedProductType
    {
        get => _selectedProductType;
        set
        {
             this.RaiseAndSetIfChanged(ref _selectedProductType, value);
             if (value != null)
             {
                 ProductTypeName = value.Name;
             }
        }
    }

    private string _productTypeName = string.Empty;
    public string ProductTypeName
    {
        get => _productTypeName;
        set => this.RaiseAndSetIfChanged(ref _productTypeName, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set 
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterProductTypes();
        }
    }

    public ObservableCollection<ProductType> AllProductTypes { get; } = new();
    public ObservableCollection<ProductType> ProductTypes { get; } = new();

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ProductTypeViewModel()
    {
        // Mock Data
        AllProductTypes.Add(new ProductType { Id = 1, Name = "Electronics" });
        AllProductTypes.Add(new ProductType { Id = 2, Name = "Clothing" });
        AllProductTypes.Add(new ProductType { Id = 3, Name = "Beverages" });
        AllProductTypes.Add(new ProductType { Id = 4, Name = "Food" });
        
        FilterProductTypes();

        AddCommand = ReactiveCommand.Create(Add);
        
        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedProductType)
                                  .Select(x => x != null);

        EditCommand = ReactiveCommand.Create(Edit, canEditOrDelete);
        DeleteCommand = ReactiveCommand.Create(Delete, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void Add()
    {
        if (string.IsNullOrWhiteSpace(ProductTypeName)) return;

        var newId = AllProductTypes.Any() ? AllProductTypes.Max(b => b.Id) + 1 : 1;
        var newType = new ProductType { Id = newId, Name = ProductTypeName };
        AllProductTypes.Add(newType);
        FilterProductTypes();
        ProductTypeName = string.Empty;
    }

    private void Edit()
    {
        if (SelectedProductType != null && !string.IsNullOrWhiteSpace(ProductTypeName))
        {
            SelectedProductType.Name = ProductTypeName;
            var index = AllProductTypes.IndexOf(SelectedProductType);
            if (index != -1) 
            {
               AllProductTypes[index] = new ProductType { Id = SelectedProductType.Id, Name = ProductTypeName };
            }
            FilterProductTypes();
            Cancel();
        }
    }

    private void Delete()
    {
        if (SelectedProductType != null)
        {
            AllProductTypes.Remove(SelectedProductType);
            FilterProductTypes();
            Cancel();
        }
    }

    private void Cancel()
    {
        SelectedProductType = null;
        ProductTypeName = string.Empty;
    }

    private void FilterProductTypes()
    {
        ProductTypes.Clear();
        var query = AllProductTypes.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(b => b.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var type in query)
        {
            ProductTypes.Add(type);
        }
    }
}
