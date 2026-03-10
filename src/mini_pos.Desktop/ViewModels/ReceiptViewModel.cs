using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace mini_pos.ViewModels;

public partial class ReceiptViewModel : ViewModelBase
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

    public decimal UsdAmount => TotalAmount / 23000;
    public decimal ThbAmount => TotalAmount / 626;

    [ObservableProperty]
    private decimal _moneyReceived;

    partial void OnMoneyReceivedChanged(decimal value) => CalculateChange();

    [ObservableProperty]
    private decimal _change;

    public Action? CloseDialogAction { get; set; }
    public Action<decimal>? PaymentConfirmedAction { get; set; }

    public ReceiptViewModel(ObservableCollection<CartItem> items, decimal total, decimal payment, decimal change)
    {
        Items = items;
        TotalAmount = total;
        _moneyReceived = payment;
        _change = change;
    }

    [RelayCommand]
    private void Close()
    {
        CloseDialogAction?.Invoke();
    }

    [RelayCommand]
    private void EnterNumber(object? arg)
    {
        string num = arg?.ToString() ?? "";
        string current = Math.Floor(MoneyReceived).ToString("0");
        if (current == "0") current = "";

        if (decimal.TryParse(current + num, out decimal newValue))
            MoneyReceived = newValue;
    }

    [RelayCommand]
    private void Backspace()
    {
        string current = Math.Floor(MoneyReceived).ToString("0");
        if (current.Length > 0)
            current = current.Substring(0, current.Length - 1);

        if (string.IsNullOrEmpty(current)) current = "0";

        if (decimal.TryParse(current, out decimal newValue))
            MoneyReceived = newValue;
    }

    [RelayCommand]
    private void Clear()
    {
        MoneyReceived = 0;
    }

    [RelayCommand]
    private void ConfirmPay()
    {
        PaymentConfirmedAction?.Invoke(MoneyReceived);
        CloseDialogAction?.Invoke();
    }

    private void CalculateChange()
    {
        Change = MoneyReceived - TotalAmount;
        if (Change < 0) Change = 0;
    }
}
