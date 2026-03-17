using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using HomeAssistantHelper.Models;
using HomeAssistantHelper.Services;
using HomeAssistantHelper.Tests.Helpers;
using HomeAssistantHelper.ViewModels;
using HomeAssistantHelper.Views;
using Microsoft.Extensions.Configuration;

namespace HomeAssistantHelper.Tests.Views;

public class SettingsWindowTests : IDisposable
{
    private readonly TestDbContextFactory _factory;
    private readonly SettingsService _settingsService;
    private readonly CopilotService _copilotService;

    public SettingsWindowTests()
    {
        _factory = (TestDbContextFactory)DbHelper.CreateInMemoryFactory();
        var config = new ConfigurationBuilder().Build();
        _settingsService = new SettingsService(_factory, config);
        _copilotService = new CopilotService();
    }

    [AvaloniaFact]
    public void SettingsWindow_Renders_With_ViewModel()
    {
        var vm = new SettingsViewModel(_settingsService, _copilotService)
        {
            McpServerUrl = "http://test:8123",
            CopilotModel = "gpt-5"
        };

        var window = new SettingsWindow(vm);
        window.Show();

        Assert.Equal(vm, window.DataContext);
    }

    [AvaloniaFact]
    public async Task SettingsWindow_Loads_Values_On_Show()
    {
        await _settingsService.SaveAsync(new AppSettings
        {
            McpServerUrl = "http://loaded:8123",
            CopilotModel = "loaded-model"
        });

        var vm = new SettingsViewModel(_settingsService, _copilotService);
        var window = new SettingsWindow(vm);
        window.Show();

        // Simulate the Loaded event triggering LoadAsync
        await vm.LoadAsync();

        Assert.Equal("http://loaded:8123", vm.McpServerUrl);
    }

    public void Dispose() => _factory.Dispose();
}
