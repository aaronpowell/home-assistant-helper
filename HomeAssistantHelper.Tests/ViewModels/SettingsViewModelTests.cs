using HomeAssistantHelper.Models;
using HomeAssistantHelper.Services;
using HomeAssistantHelper.Tests.Helpers;
using HomeAssistantHelper.ViewModels;
using Microsoft.Extensions.Configuration;

namespace HomeAssistantHelper.Tests.ViewModels;

public class SettingsViewModelTests : IDisposable
{
    private readonly TestDbContextFactory _factory;
    private readonly SettingsService _settingsService;

    public SettingsViewModelTests()
    {
        _factory = (TestDbContextFactory)DbHelper.CreateInMemoryFactory();
        var config = new ConfigurationBuilder().Build();
        _settingsService = new SettingsService(_factory, config);
    }

    [Fact]
    public async Task LoadAsync_Populates_Properties_From_Db()
    {
        await _settingsService.SaveAsync(new AppSettings
        {
            McpServerUrl = "http://test:8123",
            CopilotModel = "gpt-5"
        });

        // CopilotService can't connect in tests, so we test the fallback path
        var copilotService = new CopilotService();
        var vm = new SettingsViewModel(_settingsService, copilotService);

        await vm.LoadAsync();

        Assert.Equal("http://test:8123", vm.McpServerUrl);
        Assert.Equal("gpt-5", vm.CopilotModel);
    }

    [Fact]
    public async Task LoadAsync_Falls_Back_To_Default_Models_When_CopilotNotConnected()
    {
        var copilotService = new CopilotService();
        var vm = new SettingsViewModel(_settingsService, copilotService);

        await vm.LoadAsync();

        Assert.Contains("gpt-4.1", vm.AvailableModels);
        Assert.Contains("gpt-5", vm.AvailableModels);
        Assert.Contains("claude-sonnet-4.5", vm.AvailableModels);
    }

    [Fact]
    public async Task SaveCommand_Persists_Settings()
    {
        var copilotService = new CopilotService();
        var vm = new SettingsViewModel(_settingsService, copilotService)
        {
            McpServerUrl = "http://saved:8123",
            CopilotModel = "saved-model"
        };

        await vm.SaveCommand.ExecuteAsync(null);

        var loaded = await _settingsService.LoadAsync();
        Assert.Equal("http://saved:8123", loaded.McpServerUrl);
        Assert.Equal("saved-model", loaded.CopilotModel);
    }

    [Fact]
    public async Task SaveCommand_Sets_IsSaved_And_StatusMessage()
    {
        var copilotService = new CopilotService();
        var vm = new SettingsViewModel(_settingsService, copilotService);

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.True(vm.IsSaved);
        Assert.Equal("Settings saved!", vm.StatusMessage);
    }

    [Fact]
    public async Task SaveCommand_Fires_CloseRequested()
    {
        var copilotService = new CopilotService();
        var vm = new SettingsViewModel(_settingsService, copilotService);
        var closeRequested = false;
        vm.CloseRequested += () => closeRequested = true;

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.True(closeRequested);
    }

    [Fact]
    public void CancelCommand_Fires_CloseRequested()
    {
        var copilotService = new CopilotService();
        var vm = new SettingsViewModel(_settingsService, copilotService);
        var closeRequested = false;
        vm.CloseRequested += () => closeRequested = true;

        vm.CancelCommand.Execute(null);

        Assert.True(closeRequested);
    }

    public void Dispose() => _factory.Dispose();
}
