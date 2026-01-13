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

public class SalesViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;
    private readonly Employee _currentEmployee;
    private ExchangeRate? _currentExchangeRate;

    private const decimal DefaultDollarRate = 23000m;
    private const decimal DefaultBahtRate = 626m;

    // Input Fields
    private string _customerName = string.Empty;
    public string CustomerName
    {
        get => _customerName;
        set => this.RaiseAndSetIfChanged(ref _customerName, value);
    }

    private string _customerCode = string.Empty;
    public string CustomerCode
    {
        get => _customerCode;
        set => this.RaiseAndSetIfChanged(ref _customerCode, value);
    }

    private string _barcode = string.Empty;
    public string Barcode
    {
        get => _barcode;
        set
        {
            this.RaiseAndSetIfChanged(ref _barcode, value);
            if (!string.IsNullOrWhiteSpace(value))
            {
                _ = LookupProductByBarcode(value);
            }
        }
    }

    private async Task LookupProductByBarcode(string code)
    {
        if (_databaseService == null) return;
        var product = await _databaseService.GetProductByBarcodeAsync(code);
        if (product != null)
        {
            ProductName = product.Name;
            Unit = product.Unit;
            // Apply price logic
            UnitPrice = product.SellingPrice;
        }
    }

    private string _productName = string.Empty;
    public string ProductName
    {
        get => _productName;
        set => this.RaiseAndSetIfChanged(ref _productName, value);
    }

    private string _unit = string.Empty;
    public string Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    private decimal _unitPrice;
    public decimal UnitPrice
    {
        get => _unitPrice;
        set => this.RaiseAndSetIfChanged(ref _unitPrice, value);
    }

    private int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set => this.RaiseAndSetIfChanged(ref _quantity, value);
    }

    private bool _isWholesale;
    public bool IsWholesale
    {
        get => _isWholesale;
        set => this.RaiseAndSetIfChanged(ref _isWholesale, value);
    }

    // Cart
    public ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    private CartItemViewModel? _selectedCartItem;
    public CartItemViewModel? SelectedCartItem
    {
        get => _selectedCartItem;
        set => this.RaiseAndSetIfChanged(ref _selectedCartItem, value);
    }

    // Totals
    private decimal _totalAmount;
    public decimal TotalAmount
    {
        get => _totalAmount;
        set => this.RaiseAndSetIfChanged(ref _totalAmount, value);
    }

    private decimal _moneyReceived;
    public decimal MoneyReceived
    {
        get => _moneyReceived;
        set
        {
            this.RaiseAndSetIfChanged(ref _moneyReceived, value);
            CalculateChange();
        }
    }

    private decimal _change;
    public decimal Change
    {
        get => _change;
        set => this.RaiseAndSetIfChanged(ref _change, value);
    }

    private decimal _exchangeRateDollar = DefaultDollarRate;
    public decimal ExchangeRateDollar
    {
        get => _exchangeRateDollar;
        set
        {
            this.RaiseAndSetIfChanged(ref _exchangeRateDollar, value);
            RecalculateForeignTotals();
        }
    }

    private decimal _exchangeRateBaht = DefaultBahtRate;
    public decimal ExchangeRateBaht
    {
        get => _exchangeRateBaht;
        set
        {
            this.RaiseAndSetIfChanged(ref _exchangeRateBaht, value);
            RecalculateForeignTotals();
        }
    }

    private decimal _totalDollar;
    public decimal TotalDollar
    {
        get => _totalDollar;
        set => this.RaiseAndSetIfChanged(ref _totalDollar, value);
    }

    private decimal _totalBaht;
    public decimal TotalBaht
    {
        get => _totalBaht;
        set => this.RaiseAndSetIfChanged(ref _totalBaht, value);
    }

    // Commands
    public ReactiveCommand<Unit, Unit> SearchCustomerCommand { get; }
    public ReactiveCommand<Unit, Unit> AddProductCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveProductCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearInputsCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearCartCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearAllCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveSaleCommand { get; }
    public ReactiveCommand<Unit, Unit> PaymentCommand { get; }

    // Events
    public event Action<ReceiptViewModel>? ShowReceiptRequested;

    public SalesViewModel(Employee employee, IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _currentEmployee = employee;
        _databaseService = databaseService;
        _dialogService = dialogService;

        var canAddProduct = this.WhenAnyValue(
            x => x.ProductName,
            x => x.Quantity,
            x => x.UnitPrice,
            (name, qty, price) => !string.IsNullOrWhiteSpace(name) && qty > 0 && price >= 0);

        var canRemoveProduct = this.WhenAnyValue(x => x.SelectedCartItem)
            .Select(x => x != null);

        var canClearCart = this.WhenAnyValue(x => x.CartItems.Count)
            .Select(count => count > 0);

        SearchCustomerCommand = ReactiveCommand.Create(SearchCustomer);
        AddProductCommand = ReactiveCommand.Create(AddProduct, canAddProduct);
        RemoveProductCommand = ReactiveCommand.Create(RemoveProduct, canRemoveProduct);
        ClearInputsCommand = ReactiveCommand.Create(ClearInputs);
        ClearCartCommand = ReactiveCommand.Create(ClearCart, canClearCart);
        ClearAllCommand = ReactiveCommand.Create(ClearAll);
        SaveSaleCommand = ReactiveCommand.Create(SaveSale, canClearCart);
        PaymentCommand = ReactiveCommand.Create(Payment, canClearCart);

        CartItems.CollectionChanged += (s, e) => UpdateTotals();

        // Load exchange rate on startup
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

    private async Task SearchCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerName)) return;

        var results = await _databaseService.SearchCustomersAsync(CustomerName);

        if (results.Any())
        {
            // For now, auto-select the first match
            var customer = results.First();
            CustomerCode = customer.Id;
            CustomerName = $"{customer.Name} {customer.Surname}"; // Show full name
        }
        else
        {
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ບໍ່ພົບລູກຄ້າ (Customer not found)");
            }
        }
    }

    private void SearchCustomer()
    {
        _ = SearchCustomerAsync();
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

    private async Task AddProductAsync()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        // 1. Validate Barcode or Name logic
        // If we have barcode but no product details, try to find it first
        if (!string.IsNullOrWhiteSpace(Barcode) && string.IsNullOrWhiteSpace(ProductName))
        {
            var product = await _databaseService.GetProductByBarcodeAsync(Barcode);
            if (product != null)
            {
                ProductName = product.Name;
                Unit = product.Unit;
                UnitPrice = product.SellingPrice; // Or wholesale logic

                // Stock Validation
                int currentCartQty = CartItems.Where(c => c.Barcode == Barcode).Sum(c => c.Quantity);
                if (product.Quantity < (Quantity + currentCartQty))
                {
                    HasError = true;
                    ErrorMessage = $"ສິນຄ້າບໍ່ພຽງພໍ! ມີເຫຼືອ: {product.Quantity} (Stock low)";
                    if (_dialogService != null)
                    {
                        await _dialogService.ShowErrorAsync(ErrorMessage);
                    }
                    return;
                }
            }
            else
            {
                HasError = true;
                ErrorMessage = "ບໍ່ພົບສິນຄ້າ (Product not found)";
                if (_dialogService != null)
                {
                    await _dialogService.ShowErrorAsync(ErrorMessage);
                }
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(ProductName)) return;

        // 2. Add to Cart
        var total = Quantity * UnitPrice;
        var existingItem = CartItems.FirstOrDefault(c => c.Barcode == Barcode);

        if (existingItem != null)
        {
            existingItem.Quantity += Quantity;
        }
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

    private void AddProduct()
    {
        _ = AddProductAsync();
    }

    private void RemoveProduct()
    {
        if (SelectedCartItem != null)
        {
            CartItems.Remove(SelectedCartItem);
            SelectedCartItem = null;
        }
    }

    private void ClearInputs()
    {
        Barcode = string.Empty;
        ProductName = string.Empty;
        Unit = string.Empty;
        UnitPrice = 0;
        Quantity = 1;
        IsWholesale = false;
    }

    private void ClearCart()
    {
        CartItems.Clear();
        SelectedCartItem = null;
        UpdateTotals();
    }

    private void ClearAll()
    {
        ClearInputs();
        ClearCart();
        CustomerName = string.Empty;
        CustomerCode = string.Empty;
        MoneyReceived = 0;
    }

    private async Task SaveSaleAsync()
    {
        if (CartItems.Count == 0) return;

        // Ensure we have exchange rate
        if (_currentExchangeRate == null)
        {
            _currentExchangeRate = await _databaseService.GetLatestExchangeRateAsync();
        }

        // Prepare Sale Model
        var sale = new Sale
        {
            ExchangeRateId = _currentExchangeRate?.Id ?? 0, // Should handle if null, but schema might require it.
            CustomerId = string.IsNullOrWhiteSpace(CustomerCode) ? "CUS0000001" : CustomerCode, // Default generic customer if empty
            EmployeeId = _currentEmployee.Id,
            DateSale = DateTime.Now,
            SubTotal = TotalAmount,
            Pay = MoneyReceived,
            Change = Change
        };

        // Prepare Details
        var details = CartItems.Select(item => new SaleDetail
        {
            ProductId = item.Barcode,
            Quantity = item.Quantity,
            Price = item.UnitPrice,
            Total = item.TotalPrice
        }).ToList();

        // Save to DB
        bool success = await _databaseService.CreateSaleAsync(sale, details);

        if (success)
        {
            if (_dialogService != null)
            {
                await _dialogService.ShowSuccessAsync("ບັນທຶກການຂາຍສຳເລັດ (Sale saved successfully)");
            }
            ClearAll();
        }
        else
        {
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ບັນທຶກການຂາຍບໍ່ສຳເລັດ (Failed to save sale)");
            }
        }
    }

    private void SaveSale()
    {
        _ = SaveSaleAsync();
    }

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

public class CartItemViewModel : ViewModelBase
{
    private string _barcode = string.Empty;
    public string Barcode
    {
        get => _barcode;
        set => this.RaiseAndSetIfChanged(ref _barcode, value);
    }

    private string _productName = string.Empty;
    public string ProductName
    {
        get => _productName;
        set => this.RaiseAndSetIfChanged(ref _productName, value);
    }

    private string _unit = string.Empty;
    public string Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    private int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set
        {
            this.RaiseAndSetIfChanged(ref _quantity, value);
            this.RaisePropertyChanged(nameof(TotalPrice));
        }
    }

    private decimal _unitPrice;
    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            this.RaiseAndSetIfChanged(ref _unitPrice, value);
            this.RaisePropertyChanged(nameof(TotalPrice));
        }
    }

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
