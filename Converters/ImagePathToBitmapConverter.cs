using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace mini_pos.Converters;

public class ImagePathToBitmapConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path && !string.IsNullOrEmpty(path))
        {
            try
            {
                if (path.StartsWith("avares://"))
                {
                    // Handle resource paths if needed
                    return new Bitmap(AssetLoader.Open(new Uri(path)));
                }

                // Handle local file paths
                return new Bitmap(path);
            }
            catch (Exception)
            {
                // Fallback or null if load fails
                return null;
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
