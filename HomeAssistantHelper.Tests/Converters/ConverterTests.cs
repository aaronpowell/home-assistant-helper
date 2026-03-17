using System.Globalization;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Media;
using HomeAssistantHelper.Converters;
using HomeAssistantHelper.Models;

namespace HomeAssistantHelper.Tests.Converters;

public class RoleToAlignmentConverterTests
{
    private readonly RoleToAlignmentConverter _converter = new();

    [Fact]
    public void User_Returns_Right()
    {
        var result = _converter.Convert(MessageRole.User, typeof(HorizontalAlignment), null, CultureInfo.InvariantCulture);
        Assert.Equal(HorizontalAlignment.Right, result);
    }

    [Fact]
    public void Assistant_Returns_Left()
    {
        var result = _converter.Convert(MessageRole.Assistant, typeof(HorizontalAlignment), null, CultureInfo.InvariantCulture);
        Assert.Equal(HorizontalAlignment.Left, result);
    }

    [Fact]
    public void Tool_Returns_Left()
    {
        var result = _converter.Convert(MessageRole.Tool, typeof(HorizontalAlignment), null, CultureInfo.InvariantCulture);
        Assert.Equal(HorizontalAlignment.Left, result);
    }

    [Fact]
    public void Null_Returns_Left()
    {
        var result = _converter.Convert(null, typeof(HorizontalAlignment), null, CultureInfo.InvariantCulture);
        Assert.Equal(HorizontalAlignment.Left, result);
    }
}

public class RoleToBackgroundConverterTests
{
    private readonly RoleToBackgroundConverter _converter = new();

    [AvaloniaFact]
    public void User_Returns_Blue()
    {
        var result = _converter.Convert(MessageRole.User, typeof(IBrush), null, CultureInfo.InvariantCulture);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(0x00, 0x78, 0xD4), brush.Color);
    }

    [AvaloniaFact]
    public void Tool_Returns_LightGray()
    {
        var result = _converter.Convert(MessageRole.Tool, typeof(IBrush), null, CultureInfo.InvariantCulture);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(0xE8, 0xE8, 0xE8), brush.Color);
    }

    [AvaloniaFact]
    public void Assistant_Returns_OffWhite()
    {
        var result = _converter.Convert(MessageRole.Assistant, typeof(IBrush), null, CultureInfo.InvariantCulture);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Color.FromRgb(0xF0, 0xF0, 0xF0), brush.Color);
    }
}

public class RoleToForegroundConverterTests
{
    private readonly RoleToForegroundConverter _converter = new();

    [AvaloniaFact]
    public void User_Returns_White()
    {
        var result = _converter.Convert(MessageRole.User, typeof(IBrush), null, CultureInfo.InvariantCulture);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Colors.White, brush.Color);
    }

    [AvaloniaFact]
    public void Assistant_Returns_Black()
    {
        var result = _converter.Convert(MessageRole.Assistant, typeof(IBrush), null, CultureInfo.InvariantCulture);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Colors.Black, brush.Color);
    }

    [AvaloniaFact]
    public void Tool_Returns_Black()
    {
        var result = _converter.Convert(MessageRole.Tool, typeof(IBrush), null, CultureInfo.InvariantCulture);
        var brush = Assert.IsType<SolidColorBrush>(result);
        Assert.Equal(Colors.Black, brush.Color);
    }
}
