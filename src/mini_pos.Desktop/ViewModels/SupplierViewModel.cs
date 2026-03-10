using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class SupplierViewModel : ViewModelBase
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IDialogService? _dialogService;

    [ObservableProperty]
    private Supplier? _selectedSupplier;

    partial void OnSelectedSupplierChanged(Supplier? value) => CanEditOrDelete = value != null;

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => FilterSuppliers();

    [ObservableProperty]
    private Supplier _currentSupplier = new();

    [ObservableProperty]
    private bool _canEditOrDelete;

    public ObservableCollection<Supplier> AllSuppliers { get; } = new();
    public ObservableCollection<Supplier> Suppliers { get; } = new();

    public Action? CloseDialogAction { get; set; }

    public event Action? ShowSupplierDialogRequested;

    private bool _isEditMode;

    public SupplierViewModel(ISupplierRepository supplierRepository, IDialogService? dialogService = null)
    {
        _supplierRepository = supplierRepository;
        _dialogService = dialogService;
        _ = LoadDataAsync();
    }

    public SupplierViewModel() : this(null!, null)
    {
    }

    private async Task LoadDataAsync()
    {
        if (_supplierRepository == null) return;

        AllSuppliers.Clear();
        var list = await _supplierRepository.GetSuppliersAsync();
        foreach (var s in list) AllSuppliers.Add(s);
        FilterSuppliers();
    }

    [RelayCommand]
    private Task AddAsync()
    {
        _isEditMode = false;
        var maxId = AllSuppliers.Any()
            ? AllSuppliers.Max(s => int.TryParse(s.Id.Replace("SUP", ""), out var n) ? n : 0)
            : 0;

        CurrentSupplier = new Supplier
        {
            Id = $"SUP{maxId + 1:D3}",
            Sequence = AllSuppliers.Count + 1
        };

        ShowSupplierDialogRequested?.Invoke();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task EditAsync(Supplier? supplier)
    {
        supplier ??= SelectedSupplier;
        if (supplier == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາເລືອກຜູ້ສະໜອງກ່ອນ");
            return;
        }

        _isEditMode = true;
        SelectedSupplier = supplier;
        CurrentSupplier = new Supplier
        {
            Sequence = supplier.Sequence,
            Id = supplier.Id,
            Name = supplier.Name,
            ContactName = supplier.ContactName,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address
        };

        ShowSupplierDialogRequested?.Invoke();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentSupplier.Name))
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາປ້ອນຊື່ຜູ້ສະໜອງ");
            return;
        }

        bool success;
        if (_isEditMode)
            success = await _supplierRepository.UpdateSupplierAsync(CurrentSupplier);
        else
            success = await _supplierRepository.AddSupplierAsync(CurrentSupplier);

        if (success)
        {
            UpsertSupplier(CurrentSupplier);
            FilterSuppliers();
            CloseDialogAction?.Invoke();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ບັນທຶກຜູ້ສະໜອງສຳເລັດ");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ບັນທຶກຜູ້ສະໜອງບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseDialogAction?.Invoke();
    }

    [RelayCommand]
    private async Task DeleteAsync(Supplier? supplier)
    {
        supplier ??= SelectedSupplier;
        if (supplier == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາເລືອກຜູ້ສະໜອງກ່ອນ");
            return;
        }

        bool confirm = true;
        if (_dialogService != null)
            confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ລຶບຜູ້ສະໜອງ {supplier.Name} ຫຼືບໍ່?");

        if (!confirm) return;

        bool success = await _supplierRepository.DeleteSupplierAsync(supplier.Id);
        if (success)
        {
            RemoveSupplierById(supplier.Id);
            FilterSuppliers();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ລຶບຜູ້ສະໜອງສຳເລັດ");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ລຶບຜູ້ສະໜອງບໍ່ສຳເລັດ");
        }
    }

    private Supplier CloneSupplier(Supplier supplier, int sequence)
    {
        return new Supplier
        {
            Sequence = sequence,
            Id = supplier.Id,
            Name = supplier.Name,
            ContactName = supplier.ContactName,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address
        };
    }

    private void UpsertSupplier(Supplier supplier)
    {
        for (var i = 0; i < AllSuppliers.Count; i++)
        {
            if (AllSuppliers[i].Id == supplier.Id)
            {
                AllSuppliers[i] = CloneSupplier(supplier, AllSuppliers[i].Sequence);
                return;
            }
        }
        AllSuppliers.Add(CloneSupplier(supplier, AllSuppliers.Count + 1));
    }

    private void RemoveSupplierById(string supplierId)
    {
        for (var i = 0; i < AllSuppliers.Count; i++)
        {
            if (AllSuppliers[i].Id == supplierId)
            {
                AllSuppliers.RemoveAt(i);
                break;
            }
        }
        ReindexSupplierSequences();
    }

    private void ReindexSupplierSequences()
    {
        for (var i = 0; i < AllSuppliers.Count; i++)
        {
            var supplier = AllSuppliers[i];
            if (supplier.Sequence != i + 1)
                AllSuppliers[i] = CloneSupplier(supplier, i + 1);
        }
    }

    private void FilterSuppliers()
    {
        Suppliers.Clear();
        var query = AllSuppliers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(s => s.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                     s.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var supplier in query) Suppliers.Add(supplier);
    }
}
