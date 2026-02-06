# 🏠 Home Assistant Helper

A cross-platform desktop assistant that connects [GitHub Copilot](https://github.com/features/copilot) to your [Home Assistant](https://www.home-assistant.io/) instance, letting you manage your smart home through natural language conversation.

Built with [Avalonia UI](https://avaloniaui.net/) for Windows, macOS, and Linux.

## Features

- **AI-powered chat** — Ask Copilot to query devices, trigger automations, or generate YAML configs
- **Home Assistant integration** — Connects via [MCP (Model Context Protocol)](https://modelcontextprotocol.io/) server tools
- **System tray** — Lives in your tray with a quick-access flyout and full chat window
- **Session management** — Multiple chat sessions with persistent history
- **Markdown rendering** — Rich formatting for code blocks, YAML, and more
- **Settings** — Configure your MCP server URL and preferred Copilot model

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [GitHub Copilot](https://github.com/features/copilot) subscription
- A Home Assistant instance with an MCP server (e.g. [HA-MCP](https://github.com/home-assistant/mcp))

## Getting Started

```bash
# Clone the repo
git clone https://github.com/<your-username>/home-assistant-helper.git
cd home-assistant-helper

# Build and run
dotnet run --project HomeAssistantHelper
```

On first launch the app starts in the system tray. Click the tray icon to open the flyout, or right-click for the full menu.

### Configuration

Right-click the tray icon → **Settings** to configure:

| Setting | Description |
|---------|-------------|
| **MCP Server URL** | Your Home Assistant MCP endpoint (e.g. `http://ha-mcp.local:8123`) |
| **Copilot Model** | AI model to use (e.g. `gpt-4.1`, `claude-sonnet-4.5`) |

## Architecture

```
HomeAssistantHelper/
├── Models/          # Data models (ChatSession, ChatMessage, AppSettings)
├── Data/            # EF Core DbContext (SQLite)
├── Services/        # Business logic
│   ├── CopilotService.cs       # GitHub Copilot SDK wrapper
│   ├── SessionRepository.cs    # Chat session persistence
│   └── SettingsService.cs      # Settings persistence
├── ViewModels/      # MVVM ViewModels (CommunityToolkit.Mvvm)
│   ├── ChatViewModel.cs        # Chat + session management
│   └── SettingsViewModel.cs    # Settings dialog
├── Views/           # Avalonia AXAML views
│   ├── ChatPanel.axaml         # Reusable chat UI control
│   ├── MainChatWindow.axaml    # Full chat window
│   ├── FlyoutWindow.axaml      # Tray popup window
│   └── SettingsWindow.axaml    # Settings dialog
├── Converters/      # AXAML value converters
├── App.axaml        # Application entry + tray icon
└── Program.cs       # Avalonia bootstrap
```

## Tech Stack

| Component | Library |
|-----------|---------|
| UI Framework | [Avalonia UI](https://avaloniaui.net/) 11.3 |
| MVVM | [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) |
| AI | [GitHub Copilot SDK](https://www.nuget.org/packages/GitHub.Copilot.SDK) |
| Markdown | [Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia) |
| Database | [EF Core](https://learn.microsoft.com/ef/core/) + SQLite |
| DI | [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection) |

## License

[MIT](LICENSE)
