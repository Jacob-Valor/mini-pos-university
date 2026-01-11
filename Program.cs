using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.ReactiveUI;

namespace mini_pos;

/// <summary>
/// Main application entry point for the Mini POS Avalonia application.
/// Handles application initialization, GUI session detection, and error handling.
/// </summary>
sealed class Program
{
    /// <summary>
    /// Application entry point. Initializes and starts the Avalonia application.
    /// Includes platform-specific checks for GUI session availability on Linux.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application.</param>
    /// <returns>
    /// Returns 0 on successful execution, 1 on error or missing GUI session.
    /// </returns>
    /// <remarks>
    /// On Linux, this method checks for the presence of DISPLAY or WAYLAND_DISPLAY
    /// environment variables before attempting to start the GUI. If running in a
    /// headless environment (SSH, CI/CD), use xvfb-run to provide a virtual display.
    /// 
    /// Use --test-db argument to run database connection tests without starting the GUI.
    /// </remarks>
    [STAThread]
    public static int Main(string[] args)
    {
        // Handle unobserved task exceptions (often from DBus on Linux)
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            if (e.Exception.InnerException is TaskCanceledException)
            {
                e.SetObserved();
            }
        };

        // Check for database test mode
        if (args.Length > 0 && args[0] == "--test-db")
        {
            DatabaseConnectionTest.RunTestAsync().GetAwaiter().GetResult();
            return 0;
        }

        // Check for GUI session on Linux to prevent startup failures
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
            return 0;        }
        catch (Exception ex) when (IsLikelyDisplayConnectionFailure(ex))
        {
            Console.Error.WriteLine("Failed to connect to a graphical display.");
            Console.Error.WriteLine("If you're on SSH/CI, try: `xvfb-run -a dotnet run` (requires xvfb).");
            Console.Error.WriteLine($"Details: {ex.Message}");
            return 1;
        }
        catch (System.Threading.Tasks.TaskCanceledException ex)
        {
            // DBus connection cancellation is expected on some Linux configurations
            Console.Error.WriteLine("DBus connection cancelled - this is expected on some Linux configurations.");
            Console.Error.WriteLine($"Details: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Configures and builds the Avalonia application instance.
    /// </summary>
    /// <returns>An <see cref="AppBuilder"/> configured for the Mini POS application.</returns>
    /// <remarks>
    /// This method is used by both the application runtime and the Avalonia visual designer.
    /// Do not remove or modify without understanding the impact on designer functionality.
    /// 
    /// Configuration includes:
    /// - Platform detection for cross-platform support
    /// - Inter font family integration
    /// - Logging to trace output
    /// - DBus-safe synchronization context for Linux
    /// - ReactiveUI integration for MVVM support
    /// </remarks>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .AfterPlatformServicesSetup(_ => DbusSafeSynchronizationContext.InstallIfNeeded())
            .UseReactiveUI();

    /// <summary>
    /// Checks if a graphical user session is available by examining environment variables.
    /// </summary>
    /// <returns>
    /// <c>true</c> if DISPLAY (X11) or WAYLAND_DISPLAY environment variables are set;
    /// <c>false</c> otherwise, indicating a headless environment.
    /// </returns>
    /// <remarks>
    /// This method is Linux-specific and helps prevent startup failures when running
    /// in SSH sessions or containerized environments without display servers.
    /// </remarks>
    private static bool HasGuiSession()
    {
        string? x11Display = Environment.GetEnvironmentVariable("DISPLAY");
        string? waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        return !string.IsNullOrWhiteSpace(x11Display) || !string.IsNullOrWhiteSpace(waylandDisplay);
    }

    /// <summary>
    /// Determines if an exception is likely caused by a display connection failure.
    /// </summary>
    /// <param name="ex">The exception to examine.</param>
    /// <returns>
    /// <c>true</c> if the exception or any of its inner exceptions contain messages
    /// indicating X11 display connection failure; <c>false</c> otherwise.
    /// </returns>
    /// <remarks>
    /// This method recursively checks the exception chain for known X11/display-related
    /// error messages to provide more helpful error output to users.
    /// </remarks>
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
