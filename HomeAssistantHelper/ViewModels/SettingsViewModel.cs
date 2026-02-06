using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HomeAssistantHelper.Models;
using HomeAssistantHelper.Services;

namespace HomeAssistantHelper.ViewModels;

public partial class SettingsViewModel(SettingsService settingsService, CopilotService copilotService) : ObservableObject
{
    [ObservableProperty]
    private string _mcpServerUrl = string.Empty;

    [ObservableProperty]
    private string _copilotModel = "gpt-4.1";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isSaved;

    public ObservableCollection<string> AvailableModels { get; } = [];

    public event Action? CloseRequested;

    public async Task LoadAsync()
    {
        var settings = await settingsService.LoadAsync();
        McpServerUrl = settings.McpServerUrl;

        await LoadModelsAsync();

        // Set selected model after the list is populated so the ComboBox can match it
        CopilotModel = settings.CopilotModel;
    }

    private async Task LoadModelsAsync()
    {
        try
        {
            var models = await copilotService.ListModelsAsync();
            AvailableModels.Clear();
            foreach (var model in models)
            {
                AvailableModels.Add(model);
            }
        }
        catch
        {
            // Fall back to defaults if Copilot isn't connected yet
            AvailableModels.Clear();
            AvailableModels.Add("gpt-4.1");
            AvailableModels.Add("gpt-5");
            AvailableModels.Add("claude-sonnet-4.5");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var settings = new AppSettings
        {
            McpServerUrl = McpServerUrl,
            CopilotModel = CopilotModel
        };

        await settingsService.SaveAsync(settings);
        StatusMessage = "Settings saved!";
        IsSaved = true;
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}
