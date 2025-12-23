using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using mini_pos.Models;
using ReactiveUI;

namespace mini_pos.ViewModels;

public class SupplierViewModel : ViewModelBase
{
    private Supplier? _selectedSupplier;
    public Supplier? SelectedSupplier
    {
        get => _selectedSupplier;
        set => this.RaiseAndSetIfChanged(ref _selectedSupplier, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterSuppliers();
        }
    }

    private Supplier _currentSupplier = new();
    public Supplier CurrentSupplier
    {
        get => _currentSupplier;
        set => this.RaiseAndSetIfChanged(ref _currentSupplier, value);
    }

    public ObservableCollection<Supplier> AllSuppliers { get; } = new();
    public ObservableCollection<Supplier> Suppliers { get; } = new();

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    // Interaction to open the window
    public Interaction<Supplier, Unit> ShowDialog { get; } = new();
    public Action? CloseDialogAction { get; set; }

    private bool _isEditMode;

    public SupplierViewModel()
    {
        // Mock Data
        AllSuppliers.Add(new Supplier { Sequence = 1, Id = "SUP001", Name = "ບໍລິສັດ ເອທີ ການຄ້າ ຈໍາກັດ", ContactName = "ນາງ ໄອລີນ", Email = "kt@gmail.com", Phone = "12345678", Address = "ສີໄຄ" });
        AllSuppliers.Add(new Supplier { Sequence = 2, Id = "SUP002", Name = "ບໍລິສັດ ບີບີ ການຄ້າ", ContactName = "ທ້າວ ສົມຊາຍ", Email = "bb@gmail.com", Phone = "87654321", Address = "ໂພນຕ້ອງ" });

        FilterSuppliers();

        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedSupplier)
                                  .Select(x => x != null);

        AddCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            _isEditMode = false;
            CurrentSupplier = new Supplier 
            { 
                Id = $"SUP{AllSuppliers.Count + 1:D3}",
                Sequence = AllSuppliers.Count + 1
            };
            await ShowDialog.Handle(CurrentSupplier);
        });

        EditCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedSupplier != null)
            {
                _isEditMode = true;
                // Clone selected supplier to current
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
                await ShowDialog.Handle(CurrentSupplier);
            }
        }, canEditOrDelete);

        DeleteCommand = ReactiveCommand.Create(Delete, canEditOrDelete);

        SaveCommand = ReactiveCommand.Create(Save);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(CurrentSupplier.Name)) return;

        if (_isEditMode)
        {
            var existing = AllSuppliers.FirstOrDefault(s => s.Id == CurrentSupplier.Id);
            if (existing != null)
            {
                var index = AllSuppliers.IndexOf(existing);
                AllSuppliers[index] = CurrentSupplier;
            }
        }
        else
        {
            AllSuppliers.Add(CurrentSupplier);
        }

        FilterSuppliers();
        CloseDialogAction?.Invoke();
    }

    private void Cancel()
    {
        CloseDialogAction?.Invoke();
    }

    private void Delete()
    {
        if (SelectedSupplier != null)
        {
            AllSuppliers.Remove(SelectedSupplier);
            FilterSuppliers();
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

        foreach (var supplier in query)
        {
            Suppliers.Add(supplier);
        }
    }
}
