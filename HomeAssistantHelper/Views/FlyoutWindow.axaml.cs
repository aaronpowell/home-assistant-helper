using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using HomeAssistantHelper.ViewModels;

namespace HomeAssistantHelper.Views;

public partial class FlyoutWindow : Window
{
    public event Action? DetachRequested;

    // Design-time/XAML loader constructor
    public FlyoutWindow() : this(null!) { }

    public FlyoutWindow(ChatViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        ChatContent.DataContext = viewModel;

        Deactivated += (_, _) => Hide();
    }

    public void PositionNearTray()
    {
        var screen = Screens.Primary;
        if (screen is not null)
        {
            var workArea = screen.WorkingArea;
            var scaling = screen.Scaling;
            Position = new PixelPoint(
                (int)(workArea.Right / scaling - Width - 12),
                (int)(workArea.Bottom / scaling - Height - 12));
        }
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void Detach_Click(object? sender, RoutedEventArgs e)
    {
        DetachRequested?.Invoke();
        Hide();
    }
}
