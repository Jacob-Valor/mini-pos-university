using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(mini_pos.Tests.AvaloniaTestAppBuilder))]

namespace mini_pos.Tests;

public static class AvaloniaTestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<mini_pos.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
