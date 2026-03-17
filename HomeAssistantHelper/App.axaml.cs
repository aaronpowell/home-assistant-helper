using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HomeAssistantHelper.Data;
using HomeAssistantHelper.Models;
using HomeAssistantHelper.Services;
using HomeAssistantHelper.ViewModels;
using HomeAssistantHelper.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeAssistantHelper;

public partial class App : Application
{
    private TrayIcon? _trayIcon;
    private ServiceProvider? _serviceProvider;
    private CopilotService? _copilotService;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        // Setup DI
        var services = new ServiceCollection();

        // Configuration: env vars using .NET config scheme (e.g. AppSettings__McpServerUrl)
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbFolder = Path.Combine(appData, "HomeAssistantHelper");
        Directory.CreateDirectory(dbFolder);
        var dbPath = Path.Combine(dbFolder, "app.db");

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddSingleton<SettingsService>();
        services.AddSingleton<SessionRepository>();
        services.AddSingleton<CopilotService>();
        services.AddSingleton<ChatViewModel>();
        services.AddTransient<SettingsViewModel>();
        _serviceProvider = services.BuildServiceProvider();

        _copilotService = _serviceProvider.GetRequiredService<CopilotService>();

        // Ensure DB exists
        var dbFactory = _serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using (var db = await dbFactory.CreateDbContextAsync())
        {
            await db.Database.EnsureCreatedAsync();
        }

        // Create system tray icon
        SetupTrayIcon();

        // Initialize the chat view model
        var chatVm = _serviceProvider.GetRequiredService<ChatViewModel>();
        _ = chatVm.InitializeAsync();
    }

    private void SetupTrayIcon()
    {
        var menu = new NativeMenu();

        var openItem = new NativeMenuItem("Open Chat");
        openItem.Click += (_, _) => ShowMainWindow();
        menu.Add(openItem);

        var settingsItem = new NativeMenuItem("Settings");
        settingsItem.Click += (_, _) => ShowSettings();
        menu.Add(settingsItem);

        menu.Add(new NativeMenuItemSeparator());

        var exitItem = new NativeMenuItem("Exit");
        exitItem.Click += (_, _) => ExitApp();
        menu.Add(exitItem);

        _trayIcon = new TrayIcon
        {
            ToolTipText = "Home Assistant Helper",
            Menu = menu,
            IsVisible = true
        };

        _trayIcon.Clicked += (_, _) => ShowFlyout();
    }

    private FlyoutWindow? _flyoutWindow;
    private MainChatWindow? _mainWindow;

    private void ShowFlyout()
    {
        if (_mainWindow is { IsVisible: true }) return;

        var chatVm = _serviceProvider!.GetRequiredService<ChatViewModel>();

        if (_flyoutWindow is null)
        {
            _flyoutWindow = new FlyoutWindow(chatVm);
            _flyoutWindow.DetachRequested += () => ShowMainWindow();
        }

        _flyoutWindow.PositionNearTray();
        _flyoutWindow.Show();
        _flyoutWindow.Activate();
    }

    private void ShowMainWindow()
    {
        _flyoutWindow?.Hide();

        var chatVm = _serviceProvider!.GetRequiredService<ChatViewModel>();
        var settingsVm = _serviceProvider!.GetRequiredService<SettingsViewModel>();

        if (_mainWindow is null)
        {
            _mainWindow = new MainChatWindow(chatVm, settingsVm);
        }

        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private async void ShowSettings()
    {
        var settingsVm = _serviceProvider!.GetRequiredService<SettingsViewModel>();
        var window = new SettingsWindow(settingsVm);

        var owner = _mainWindow as Window ?? _flyoutWindow as Window;
        if (owner is not null)
            await window.ShowDialog(owner);
        else
            window.Show();
    }

    private async void ExitApp()
    {
        _trayIcon?.Dispose();
        _trayIcon = null;
        _flyoutWindow?.Close();

        if (_mainWindow is not null)
        {
            _mainWindow.AllowClose = true;
            _mainWindow.Close();
        }

        if (_copilotService is not null)
        {
            await _copilotService.DisposeAsync();
        }

        _serviceProvider?.Dispose();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}

