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
    private readonly IDatabaseService _databaseService;
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

    private bool _isEditMode;

    public SupplierViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;
        _ = LoadDataAsync();
    }

    public SupplierViewModel() : this(null!, null)
    {
    }

    private async Task LoadDataAsync()
    {
        if (_databaseService == null) return;

        AllSuppliers.Clear();
        var list = await _databaseService.GetSuppliersAsync();
        foreach (var s in list) AllSuppliers.Add(s);
        FilterSuppliers();
    }

    [RelayCommand]
    private async Task AddAsync()
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
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task EditAsync()
    {
        if (SelectedSupplier != null)
        {
            _isEditMode = true;
            CurrentSupplier = new Supplier
            {
                Sequence = SelectedSupplier.Sequence,
                Id = SelectedSupplier.Id,
                Name = SelectedSupplier.Name,
                ContactName = SelectedSupplier.ContactName,
                Email = SelectedSupplier.Email,
                Phone = SelectedSupplier.Phone,
                Address = SelectedSupplier.Address
            };
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentSupplier.Name))
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາປ້ອນຊື່ຜູ້ສົ່ງ (Supplier name required)");
            return;
        }

        bool success;
        if (_isEditMode)
            success = await _databaseService.UpdateSupplierAsync(CurrentSupplier);
        else
            success = await _databaseService.AddSupplierAsync(CurrentSupplier);

        if (success)
        {
            UpsertSupplier(CurrentSupplier);
            FilterSuppliers();
            CloseDialogAction?.Invoke();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ບັນທຶກຜູ້ສົ່ງສຳເລັດ (Supplier saved)");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ບັນທຶກຜູ້ສົ່ງບໍ່ສຳເລັດ (Failed to save supplier)");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseDialogAction?.Invoke();
    }

    [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedSupplier != null)
        {
            bool confirm = true;
            if (_dialogService != null)
                confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ລຶບຜູ້ສົ່ງ {SelectedSupplier.Name} ຫຼືບໍ່?");

            if (!confirm) return;

            bool success = await _databaseService.DeleteSupplierAsync(SelectedSupplier.Id);
            if (success)
            {
                RemoveSupplierById(SelectedSupplier.Id);
                FilterSuppliers();
                if (_dialogService != null)
                    await _dialogService.ShowSuccessAsync("ລຶບຜູ້ສົ່ງສຳເລັດ (Supplier deleted)");
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ລຶບຜູ້ສົ່ງບໍ່ສຳເລັດ (Failed to delete supplier)");
            }
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
