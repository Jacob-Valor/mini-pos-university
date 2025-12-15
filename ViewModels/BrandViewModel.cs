using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using mini_pos.Models;
using ReactiveUI;

namespace mini_pos.ViewModels;

public partial class BrandViewModel : ViewModelBase
{
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

    public BrandViewModel()
    {
        // Mock Data
        AllBrands.Add(new Brand { Id = 1, Name = "Apple" });
        AllBrands.Add(new Brand { Id = 2, Name = "Samsung" });
        AllBrands.Add(new Brand { Id = 3, Name = "Sony" });
        AllBrands.Add(new Brand { Id = 4, Name = "Dell" });
        
        FilterBrands();

        AddCommand = ReactiveCommand.Create(Add);
        
        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedBrand)
                                  .Select(x => x != null);

        EditCommand = ReactiveCommand.Create(Edit, canEditOrDelete);
        DeleteCommand = ReactiveCommand.Create(Delete, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void Add()
    {
        if (string.IsNullOrWhiteSpace(BrandName)) return;

        var newId = AllBrands.Any() ? AllBrands.Max(b => b.Id) + 1 : 1;
        var brand = new Brand { Id = newId, Name = BrandName };
        AllBrands.Add(brand);
        FilterBrands();
        BrandName = string.Empty;
    }

    private void Edit()
    {
        if (SelectedBrand != null && !string.IsNullOrWhiteSpace(BrandName))
        {
            SelectedBrand.Name = BrandName;
            // Force refresh if needed, or simple property change notification is enough if Model implements INPC (it doesn't currently, but for this simple mock it might be fine or we might need to replace the item)
            // For now, let's just refresh the list view effectively
            var index = AllBrands.IndexOf(SelectedBrand);
            if (index != -1) 
            {
               AllBrands[index] = new Brand { Id = SelectedBrand.Id, Name = BrandName };
            }
            FilterBrands();
            Cancel();
        }
    }

    private void Delete()
    {
        if (SelectedBrand != null)
        {
            AllBrands.Remove(SelectedBrand);
            FilterBrands();
            Cancel();
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
