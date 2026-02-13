using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Serilog;

namespace mini_pos.Services;

public interface IDialogService
{
    Task ShowMessageAsync(string title, string message, Icon icon = Icon.Info);
    Task<bool> ShowConfirmationAsync(string title, string message);
    Task ShowErrorAsync(string message);
    Task ShowSuccessAsync(string message);
}

public class DialogService : IDialogService
{
    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    public async Task ShowMessageAsync(string title, string message, Icon icon = Icon.Info)
    {
        var window = GetMainWindow();
        if (window is null)
        {
            Log.Warning("DialogService: Could not get main window for message {Title}", title);
            return;
        }

        try
        {
            var msg = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
            await msg.ShowWindowDialogAsync(window);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DialogService.ShowMessageAsync error");
        }
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var window = GetMainWindow();
        if (window is null)
        {
            Log.Warning("DialogService: Could not get main window for confirmation {Title}", title);
            return false;
        }

        try
        {
            var msg = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNo, Icon.Question);
            var result = await msg.ShowWindowDialogAsync(window);
            return result == ButtonResult.Yes;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DialogService.ShowConfirmationAsync error");
            return false;
        }
    }

    public async Task ShowErrorAsync(string message)
    {
        await ShowMessageAsync("ຜິດພາດ", message, Icon.Error);
    }

    public async Task ShowSuccessAsync(string message)
    {
        await ShowMessageAsync("ສຳເລັດ", message, Icon.Success);
    }
}
