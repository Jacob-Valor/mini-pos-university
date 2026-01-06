using System;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Reactive;

namespace mini_pos.ViewModels;

public class ReceiptViewModel : ViewModelBase
{
    public string ShopName { get; } = "ສຸກສະຫວັນ ມິນິມາກ";
    public string Address { get; } = "ບ້ານ ໂພນປາເປົ້າ, ເມືອງ ສີສັດຕະນາກ, ນະຄອນຫຼວງວຽງຈັນ";
    public string Phone { get; } = "ໂທ: 202 96887222";

    public string ReceiptNo { get; set; } = "0000000234";
    public string Date { get; set; } = DateTime.Now.ToString("dd/MM/yyyy h.mm tt");
    public string StaffName { get; set; } = "ສຸກສະຫວັນ ຈຸນດາລີ";
    public string CustomerName { get; set; } = "ລູກຄ້າທົ່ວໄປ";

    public ObservableCollection<CartItem> Items { get; }

    public decimal TotalAmount { get; set; }

    // Helper for display
    public decimal UsdAmount => TotalAmount / 23000;
    public decimal ThbAmount => TotalAmount / 626;

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

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    // Numpad Commands
    public ReactiveCommand<object, Unit> EnterNumberCommand { get; }
    public ReactiveCommand<Unit, Unit> BackspaceCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearCommand { get; }
    public ReactiveCommand<Unit, Unit> ConfirmPayCommand { get; }

    public ReceiptViewModel(ObservableCollection<CartItem> items, decimal total, decimal payment, decimal change)
    {
        Items = items;
        TotalAmount = total;
        // Don't set public property directly to avoid triggering change calc prematurely if needed,
        // but here it is fine.
        _moneyReceived = payment;
        _change = change;

        // Initialize commands
        CloseCommand = ReactiveCommand.Create(() => { });

        EnterNumberCommand = ReactiveCommand.Create<object>(arg =>
        {
            string num = arg?.ToString() ?? "";
            // Append number logic
            string current = Math.Floor(MoneyReceived).ToString("0");
            if (current == "0") current = "";

            if (decimal.TryParse(current + num, out decimal newValue))
            {
                MoneyReceived = newValue;
            }
        });

        BackspaceCommand = ReactiveCommand.Create(() =>
        {
            string current = Math.Floor(MoneyReceived).ToString("0");
            if (current.Length > 0)
            {
                current = current.Substring(0, current.Length - 1);
            }

            if (string.IsNullOrEmpty(current)) current = "0";

            if (decimal.TryParse(current, out decimal newValue))
            {
                MoneyReceived = newValue;
            }
        });

        ClearCommand = ReactiveCommand.Create(() =>
        {
            MoneyReceived = 0;
        });

        ConfirmPayCommand = ReactiveCommand.Create(() =>
        {
            // Just close for now, envisioning this window returns result or Prints
            // In a real app this would trigger the actual Print service
            CloseCommand.Execute();
        });
    }

    private void CalculateChange()
    {
        Change = MoneyReceived - TotalAmount;
        if (Change < 0) Change = 0;
    }
}
