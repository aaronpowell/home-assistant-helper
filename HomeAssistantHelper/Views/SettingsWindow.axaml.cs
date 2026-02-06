using Avalonia.Controls;
using HomeAssistantHelper.ViewModels;

namespace HomeAssistantHelper.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    // Design-time/XAML loader constructor
    public SettingsWindow() : this(null!) { }

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        viewModel.CloseRequested += () => Close();

        Loaded += async (_, _) =>
        {
            await viewModel.LoadAsync();
        };
    }
}
