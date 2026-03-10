using Avalonia.Styling;

namespace mini_pos.Services;

public interface IThemeService
{
    ThemeVariant CurrentTheme { get; }

    bool IsDarkTheme { get; }

    void ApplySavedTheme();

    void ToggleTheme();
}
