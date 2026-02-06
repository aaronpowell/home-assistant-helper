using Avalonia.Controls;
using Avalonia.Interactivity;
using HomeAssistantHelper.ViewModels;

namespace HomeAssistantHelper.Views;

public partial class MainChatWindow : Window
{
    private readonly SettingsViewModel _settingsViewModel;

    public bool AllowClose { get; set; }

    // Design-time/XAML loader constructor
    public MainChatWindow() : this(null!, null!) { }

    public MainChatWindow(ChatViewModel chatViewModel, SettingsViewModel settingsViewModel)
    {
        InitializeComponent();
        _settingsViewModel = settingsViewModel;
        DataContext = chatViewModel;
        ChatContent.DataContext = chatViewModel;
    }

    private async void Settings_Click(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_settingsViewModel);
        await settingsWindow.ShowDialog(this);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!AllowClose)
        {
            // Hide instead of close so tray icon keeps running
            e.Cancel = true;
            Hide();
        }

        base.OnClosing(e);
    }
}
