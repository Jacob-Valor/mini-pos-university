using System;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.ReactiveUI;

namespace mini_pos;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !HasGuiSession())
        {
            Console.Error.WriteLine("No graphical session detected (missing DISPLAY/WAYLAND_DISPLAY).");
            Console.Error.WriteLine("This Avalonia app needs a GUI session to run.");
            Console.Error.WriteLine("If you're on SSH/CI, try: `xvfb-run -a dotnet run` (requires xvfb).");
            return 1;
        }

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }
        catch (Exception ex) when (IsLikelyDisplayConnectionFailure(ex))
        {
            Console.Error.WriteLine("Failed to connect to a graphical display.");
            Console.Error.WriteLine("If you're on SSH/CI, try: `xvfb-run -a dotnet run` (requires xvfb).");
            Console.Error.WriteLine($"Details: {ex.Message}");
            return 1;
        }
        catch (System.Threading.Tasks.TaskCanceledException ex)
        {
            Console.Error.WriteLine("DBus connection cancelled - this is expected on some Linux configurations.");
            Console.Error.WriteLine($"Details: {ex.Message}");
            return 0;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .AfterPlatformServicesSetup(_ => DbusSafeSynchronizationContext.InstallIfNeeded())
            .UseReactiveUI();

    private static bool HasGuiSession()
    {
        string? x11Display = Environment.GetEnvironmentVariable("DISPLAY");
        string? waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        return !string.IsNullOrWhiteSpace(x11Display) || !string.IsNullOrWhiteSpace(waylandDisplay);
    }

    private static bool IsLikelyDisplayConnectionFailure(Exception ex)
    {
        for (Exception? current = ex; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("XOpenDisplay failed", StringComparison.OrdinalIgnoreCase) ||
                current.Message.Contains("Cannot open display", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
