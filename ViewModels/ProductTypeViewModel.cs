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

public partial class ProductTypeViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;

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

    public ProductTypeViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;

        var canAdd = this.WhenAnyValue(x => x.ProductTypeName)
                         .Select(name => !string.IsNullOrWhiteSpace(name));

        AddCommand = ReactiveCommand.CreateFromTask(AddAsync, canAdd);

        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedProductType)
                                  .Select(x => x != null);

        EditCommand = ReactiveCommand.CreateFromTask(EditAsync, canEditOrDelete);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);

        _ = LoadDataAsync();
    }

    public ProductTypeViewModel() : this(null!, null)
    {
        // Design-time
    }

    private async Task LoadDataAsync()
    {
        if (_databaseService == null) return;
        
        AllProductTypes.Clear();
        var list = await _databaseService.GetProductTypesAsync();
        foreach (var t in list) AllProductTypes.Add(t);
        FilterProductTypes();
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(ProductTypeName)) return;

        // Generate new ID (C001, C002...)
        var maxId = AllProductTypes.Any() 
            ? AllProductTypes.Max(t => int.TryParse(t.Id.Replace("C", ""), out var num) ? num : 0) + 1 
            : 1;
        var newId = $"C{maxId:D3}";
        
        var newType = new ProductType { Id = newId, Name = ProductTypeName };
        
        bool success = await _databaseService.AddProductTypeAsync(newType);
        if (success)
        {
            await LoadDataAsync();
            Cancel();
            if (_dialogService != null)
            {
                await _dialogService.ShowSuccessAsync("ເພີ່ມປະເພດສິນຄ້າສຳເລັດ (Type added)");
            }
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ເພີ່ມປະເພດສິນຄ້າບໍ່ສຳເລັດ (Failed to add type)");
        }
    }

    private async Task EditAsync()
    {
        if (SelectedProductType != null && !string.IsNullOrWhiteSpace(ProductTypeName))
        {
            var updatedType = new ProductType { Id = SelectedProductType.Id, Name = ProductTypeName };
            bool success = await _databaseService.UpdateProductTypeAsync(updatedType);
            if (success)
            {
                await LoadDataAsync();
                Cancel();
                if (_dialogService != null)
                {
                    await _dialogService.ShowSuccessAsync("ແກ້ໄຂປະເພດສິນຄ້າສຳເລັດ (Type updated)");
                }
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ແກ້ໄຂປະເພດສິນຄ້າບໍ່ສຳເລັດ (Failed to update type)");
            }
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedProductType != null)
        {
            bool confirm = true;
            if (_dialogService != null)
            {
                confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ລຶບປະເພດ {SelectedProductType.Name} ຫຼືບໍ່?");
            }

            if (!confirm) return;

            bool success = await _databaseService.DeleteProductTypeAsync(SelectedProductType.Id);
            if (success)
            {
                await LoadDataAsync();
                Cancel();
                if (_dialogService != null)
                {
                    await _dialogService.ShowSuccessAsync("ລຶບປະເພດສິນຄ້າສຳເລັດ (Type deleted)");
                }
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ລຶບປະເພດສິນຄ້າບໍ່ສຳເລັດ (Failed to delete type)");
            }
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
