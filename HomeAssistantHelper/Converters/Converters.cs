using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using HomeAssistantHelper.Models;

namespace HomeAssistantHelper.Converters;

public class RoleToAlignmentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is MessageRole.User ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class RoleToBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            MessageRole.User => new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4)),
            MessageRole.Tool => new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xE8)),
            _ => new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0))
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class RoleToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is MessageRole.User
            ? new SolidColorBrush(Colors.White)
            : new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
