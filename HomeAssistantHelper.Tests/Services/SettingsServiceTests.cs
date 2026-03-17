using HomeAssistantHelper.Data;
using HomeAssistantHelper.Models;
using HomeAssistantHelper.Services;
using HomeAssistantHelper.Tests.Helpers;
using Microsoft.Extensions.Configuration;

namespace HomeAssistantHelper.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly TestDbContextFactory _factory;
    private readonly SettingsService _service;
    private readonly ConfigurationBuilder _configBuilder;

    public SettingsServiceTests()
    {
        _factory = (TestDbContextFactory)DbHelper.CreateInMemoryFactory();
        _configBuilder = new ConfigurationBuilder();
        var config = _configBuilder.Build();
        _service = new SettingsService(_factory, config);
    }

    [Fact]
    public async Task LoadAsync_Returns_Defaults_When_Empty()
    {
        var settings = await _service.LoadAsync();

        Assert.Equal("", settings.McpServerUrl);
        Assert.Equal("gpt-4.1", settings.CopilotModel);
    }

    [Fact]
    public async Task SaveAsync_Then_LoadAsync_Roundtrips()
    {
        var saved = new AppSettings
        {
            McpServerUrl = "http://ha.local:8123",
            CopilotModel = "gpt-5"
        };
        await _service.SaveAsync(saved);

        var loaded = await _service.LoadAsync();

        Assert.Equal("http://ha.local:8123", loaded.McpServerUrl);
        Assert.Equal("gpt-5", loaded.CopilotModel);
    }

    [Fact]
    public async Task SaveAsync_Upserts_Existing_Values()
    {
        await _service.SaveAsync(new AppSettings { McpServerUrl = "http://first", CopilotModel = "model-a" });
        await _service.SaveAsync(new AppSettings { McpServerUrl = "http://second", CopilotModel = "model-b" });

        var loaded = await _service.LoadAsync();

        Assert.Equal("http://second", loaded.McpServerUrl);
        Assert.Equal("model-b", loaded.CopilotModel);
    }

    [Fact]
    public async Task LoadAsync_EnvVar_Overrides_DbValue()
    {
        // Save a DB value
        await _service.SaveAsync(new AppSettings
        {
            McpServerUrl = "http://db-value",
            CopilotModel = "db-model"
        });

        // Create a new service with env var config
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:McpServerUrl"] = "http://env-override",
                ["AppSettings:CopilotModel"] = "env-model"
            })
            .Build();

        var serviceWithEnv = new SettingsService(_factory, config);
        var loaded = await serviceWithEnv.LoadAsync();

        Assert.Equal("http://env-override", loaded.McpServerUrl);
        Assert.Equal("env-model", loaded.CopilotModel);
    }

    [Fact]
    public async Task LoadAsync_EnvVar_Partial_Override()
    {
        await _service.SaveAsync(new AppSettings
        {
            McpServerUrl = "http://db-url",
            CopilotModel = "db-model"
        });

        // Only override McpServerUrl via config
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:McpServerUrl"] = "http://env-url"
            })
            .Build();

        var serviceWithEnv = new SettingsService(_factory, config);
        var loaded = await serviceWithEnv.LoadAsync();

        Assert.Equal("http://env-url", loaded.McpServerUrl);
        Assert.Equal("db-model", loaded.CopilotModel);
    }

    [Fact]
    public async Task LoadAsync_Empty_EnvVar_Does_Not_Override()
    {
        await _service.SaveAsync(new AppSettings
        {
            McpServerUrl = "http://db-url",
            CopilotModel = "db-model"
        });

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:McpServerUrl"] = ""
            })
            .Build();

        var serviceWithEnv = new SettingsService(_factory, config);
        var loaded = await serviceWithEnv.LoadAsync();

        Assert.Equal("http://db-url", loaded.McpServerUrl);
    }

    public void Dispose() => _factory.Dispose();
}
