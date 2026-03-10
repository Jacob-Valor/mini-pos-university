using System.Collections.ObjectModel;

using mini_pos.ViewModels;

using Xunit;

namespace mini_pos.Tests;

public class ReceiptViewModelTests
{
    [Fact]
    public void MoneyReceived_WhenSetBelowTotal_ClampsChangeToZero()
    {
        var viewModel = CreateViewModel();

        viewModel.MoneyReceived = 1000m;

        Assert.Equal(0m, viewModel.Change);
    }

    [Fact]
    public void EnterNumberCommand_AppendsDigitsToMoneyReceived()
    {
        var viewModel = CreateViewModel();
        viewModel.MoneyReceived = 0m;

        viewModel.EnterNumberCommand.Execute("1");
        viewModel.EnterNumberCommand.Execute("5");

        Assert.Equal(15m, viewModel.MoneyReceived);
    }

    [Fact]
    public void BackspaceCommand_RemovesLastDigit()
    {
        var viewModel = CreateViewModel();
        viewModel.MoneyReceived = 125m;

        viewModel.BackspaceCommand.Execute(null);

        Assert.Equal(12m, viewModel.MoneyReceived);
    }

    [Fact]
    public void ClearCommand_ResetsMoneyReceivedAndChange()
    {
        var viewModel = CreateViewModel();
        viewModel.MoneyReceived = 8000m;

        viewModel.ClearCommand.Execute(null);

        Assert.Equal(0m, viewModel.MoneyReceived);
        Assert.Equal(0m, viewModel.Change);
    }

    [Fact]
    public void ConfirmPayCommand_InvokesPaymentAndCloseActions()
    {
        var viewModel = CreateViewModel();
        var closeCalled = false;
        decimal? confirmedPayment = null;

        viewModel.MoneyReceived = 9000m;
        viewModel.CloseDialogAction = () => closeCalled = true;
        viewModel.PaymentConfirmedAction = amount => confirmedPayment = amount;

        viewModel.ConfirmPayCommand.Execute(null);

        Assert.Equal(9000m, confirmedPayment);
        Assert.True(closeCalled);
    }

    private static ReceiptViewModel CreateViewModel()
        => new(
            new ObservableCollection<CartItem>(),
            total: 5000m,
            payment: 0m,
            change: 0m);
}
