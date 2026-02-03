using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;
    private readonly Employee _currentEmployee;
    private ExchangeRate? _currentExchangeRate;

    private const decimal DefaultDollarRate = 23000m;
    private const decimal DefaultBahtRate = 626m;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _customerCode = string.Empty;

    [ObservableProperty]
    private string _barcode = string.Empty;

    partial void OnBarcodeChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) _ = LookupProductByBarcode(value);
    }

    private async Task LookupProductByBarcode(string code)
    {
        if (_databaseService == null) return;
        var product = await _databaseService.GetProductByBarcodeAsync(code);
        if (product != null)
        {
            ProductName = product.Name;
            Unit = product.Unit;
            UnitPrice = product.SellingPrice;
        }
    }

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _unit = string.Empty;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private bool _isWholesale;

    public ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    [ObservableProperty]
    private CartItemViewModel? _selectedCartItem;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private decimal _moneyReceived;

    partial void OnMoneyReceivedChanged(decimal value) => CalculateChange();

    [ObservableProperty]
    private decimal _change;

    [ObservableProperty]
    private decimal _exchangeRateDollar = DefaultDollarRate;

    partial void OnExchangeRateDollarChanged(decimal value) => RecalculateForeignTotals();

    [ObservableProperty]
    private decimal _exchangeRateBaht = DefaultBahtRate;

    partial void OnExchangeRateBahtChanged(decimal value) => RecalculateForeignTotals();

    [ObservableProperty]
    private decimal _totalDollar;

    [ObservableProperty]
    private decimal _totalBaht;

    [ObservableProperty]
    private bool _canAddProduct;

    partial void OnProductNameChanged(string value) => UpdateCanAddProduct();
    partial void OnQuantityChanged(int value) => UpdateCanAddProduct();
    partial void OnUnitPriceChanged(decimal value) => UpdateCanAddProduct();

    private void UpdateCanAddProduct() => CanAddProduct = !string.IsNullOrWhiteSpace(ProductName) && Quantity > 0 && UnitPrice >= 0;

    [ObservableProperty]
    private bool _canRemoveProduct;

    partial void OnSelectedCartItemChanged(CartItemViewModel? value) => CanRemoveProduct = value != null;

    [ObservableProperty]
    private bool _canClearCart;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public event Action<ReceiptViewModel>? ShowReceiptRequested;

    public SalesViewModel(Employee employee, IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _currentEmployee = employee;
        _databaseService = databaseService;
        _dialogService = dialogService;
        CartItems.CollectionChanged += (s, e) =>
        {
            UpdateTotals();
            CanClearCart = CartItems.Count > 0;
        };
        _ = LoadExchangeRateAsync();
    }

    private async Task LoadExchangeRateAsync()
    {
        if (_databaseService != null)
        {
            _currentExchangeRate = await _databaseService.GetLatestExchangeRateAsync();
            if (_currentExchangeRate != null)
            {
                ExchangeRateDollar = _currentExchangeRate.UsdRate;
                ExchangeRateBaht = _currentExchangeRate.ThbRate;
            }
        }
    }

    [RelayCommand]
    private async Task SearchCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerName)) return;

        var results = await _databaseService.SearchCustomersAsync(CustomerName);

        if (results.Any())
        {
            var customer = results.First();
            CustomerCode = customer.Id;
            CustomerName = $"{customer.Name} {customer.Surname}";
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ບໍ່ພົບລູກຄ້າ (Customer not found)");
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddProduct))]
    private async Task AddProductAsync()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        if (!string.IsNullOrWhiteSpace(Barcode) && string.IsNullOrWhiteSpace(ProductName))
        {
            var product = await _databaseService.GetProductByBarcodeAsync(Barcode);
            if (product != null)
            {
                ProductName = product.Name;
                Unit = product.Unit;
                UnitPrice = product.SellingPrice;

                int currentCartQty = CartItems.Where(c => c.Barcode == Barcode).Sum(c => c.Quantity);
                if (product.Quantity < (Quantity + currentCartQty))
                {
                    HasError = true;
                    ErrorMessage = $"ສິນຄ້າບໍ່ພຽງພໍ! ມີເຫຼືອ: {product.Quantity} (Stock low)";
                    if (_dialogService != null)
                        await _dialogService.ShowErrorAsync(ErrorMessage);
                    return;
                }
            }
            else
            {
                HasError = true;
                ErrorMessage = "ບໍ່ພົບສິນຄ້າ (Product not found)";
                if (_dialogService != null)
                    await _dialogService.ShowErrorAsync(ErrorMessage);
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(ProductName)) return;

        var existingItem = CartItems.FirstOrDefault(c => c.Barcode == Barcode);

        if (existingItem != null)
            existingItem.Quantity += Quantity;
        else
        {
            var newItem = new CartItemViewModel
            {
                Barcode = Barcode,
                ProductName = ProductName,
                Unit = Unit,
                Quantity = Quantity,
                UnitPrice = UnitPrice
            };
            CartItems.Add(newItem);
        }

        ClearInputs();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveProduct))]
    private void RemoveProduct()
    {
        if (SelectedCartItem != null)
        {
            CartItems.Remove(SelectedCartItem);
            SelectedCartItem = null;
        }
    }

    [RelayCommand]
    private void ClearInputs()
    {
        Barcode = string.Empty;
        ProductName = string.Empty;
        Unit = string.Empty;
        UnitPrice = 0;
        Quantity = 1;
        IsWholesale = false;
    }

    [RelayCommand(CanExecute = nameof(CanClearCart))]
    private void ClearCart()
    {
        CartItems.Clear();
        SelectedCartItem = null;
        UpdateTotals();
    }

    [RelayCommand]
    private void ClearAll()
    {
        ClearInputs();
        ClearCart();
        CustomerName = string.Empty;
        CustomerCode = string.Empty;
        MoneyReceived = 0;
    }

    [RelayCommand(CanExecute = nameof(CanClearCart))]
    private async Task SaveSaleAsync()
    {
        if (CartItems.Count == 0) return;

        if (_currentExchangeRate == null)
            _currentExchangeRate = await _databaseService.GetLatestExchangeRateAsync();

        var sale = new Sale
        {
            ExchangeRateId = _currentExchangeRate?.Id ?? 0,
            CustomerId = string.IsNullOrWhiteSpace(CustomerCode) ? "CUS0000001" : CustomerCode,
            EmployeeId = _currentEmployee.Id,
            DateSale = DateTime.Now,
            SubTotal = TotalAmount,
            Pay = MoneyReceived,
            Change = Change
        };

        var details = CartItems.Select(item => new SaleDetail
        {
            ProductId = item.Barcode,
            Quantity = item.Quantity,
            Price = item.UnitPrice,
            Total = item.TotalPrice
        }).ToList();

        bool success = await _databaseService.CreateSaleAsync(sale, details);

        if (success)
        {
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ບັນທຶກການຂາຍສຳເລັດ (Sale saved successfully)");
            ClearAll();
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ບັນທຶກການຂາຍບໍ່ສຳເລັດ (Failed to save sale)");
        }
    }

    [RelayCommand(CanExecute = nameof(CanClearCart))]
    private void Payment()
    {
        var receiptVM = new ReceiptViewModel(
            new ObservableCollection<CartItem>(CartItems.Select(x => new CartItem
            {
                Barcode = x.Barcode,
                ProductName = x.ProductName,
                Unit = x.Unit,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice
            })),
            TotalAmount,
            MoneyReceived,
            Change
        );

        ShowReceiptRequested?.Invoke(receiptVM);
    }

    private void CalculateChange()
    {
        Change = Math.Max(0, MoneyReceived - TotalAmount);
    }

    private void UpdateTotals()
    {
        TotalAmount = CartItems.Sum(x => x.TotalPrice);
        CalculateChange();
        RecalculateForeignTotals();
    }

    private void RecalculateForeignTotals()
    {
        TotalDollar = ExchangeRateDollar > 0 ? TotalAmount / ExchangeRateDollar : 0;
        TotalBaht = ExchangeRateBaht > 0 ? TotalAmount / ExchangeRateBaht : 0;
    }
}

public partial class CartItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _barcode = string.Empty;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _unit = string.Empty;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private decimal _unitPrice;

    public decimal TotalPrice => Quantity * UnitPrice;
}

public class CartItem
{
    public string Barcode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
