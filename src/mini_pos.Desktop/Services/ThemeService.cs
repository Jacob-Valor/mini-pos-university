using System;
using System.IO;
using Avalonia;
using Avalonia.Styling;

namespace mini_pos.Services;

public sealed class ThemeService : IThemeService
{
    private const string DarkThemeName = "Dark";
    private const string LightThemeName = "Light";

    private readonly string _settingsFilePath;

    public ThemeVariant CurrentTheme { get; private set; } = ThemeVariant.Dark;

    public bool IsDarkTheme => CurrentTheme == ThemeVariant.Dark;

    public ThemeService()
    {
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "mini_pos");

        _settingsFilePath = Path.Combine(settingsDirectory, "theme.txt");
    }

    public void ApplySavedTheme()
    {
        ApplyTheme(ReadThemeVariant());
    }

    public void ToggleTheme()
    {
        ApplyTheme(IsDarkTheme ? ThemeVariant.Light : ThemeVariant.Dark);
    }

    private void ApplyTheme(ThemeVariant themeVariant)
    {
        CurrentTheme = themeVariant;

        if (Application.Current is { } application)
        {
            application.RequestedThemeVariant = themeVariant;
        }

        SaveThemeVariant(themeVariant);
    }

    private ThemeVariant ReadThemeVariant()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return ThemeVariant.Dark;
            }

            var savedTheme = File.ReadAllText(_settingsFilePath).Trim();
            return string.Equals(savedTheme, LightThemeName, StringComparison.OrdinalIgnoreCase)
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }
        catch
        {
            return ThemeVariant.Dark;
        }
    }

    private void SaveThemeVariant(ThemeVariant themeVariant)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(_settingsFilePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(_settingsFilePath, themeVariant == ThemeVariant.Light ? LightThemeName : DarkThemeName);
        }
        catch
        {
        }
    }
}
