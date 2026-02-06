using GitHub.Copilot.SDK;
using HomeAssistantHelper.Models;

namespace HomeAssistantHelper.Services;

public class CopilotService : IAsyncDisposable
{
    private CopilotClient? _client;

    public bool IsConnected => _client is not null;

    public async Task StartAsync()
    {
        if (_client is not null) return;

        _client = new CopilotClient(new CopilotClientOptions
        {
            AutoStart = true,
            UseStdio = true,
        });
        await _client.StartAsync();
    }

    public async Task<CopilotSession> CreateSessionAsync(AppSettings settings, string? existingCopilotSessionId = null)
    {
        if (_client is null)
            throw new InvalidOperationException("Copilot client not started. Call StartAsync first.");

        if (existingCopilotSessionId is not null)
        {
            return await _client.ResumeSessionAsync(existingCopilotSessionId, new() { OnPermissionRequest = PermissionHandler.ApproveAll });
        }

        var config = new SessionConfig
        {
            Model = settings.CopilotModel,
            Streaming = true,
            SystemMessage = new SystemMessageConfig
            {
                Mode = SystemMessageMode.Append,
                Content = GetSystemMessage()
            },
            OnPermissionRequest = PermissionHandler.ApproveAll
        };

        var mcpConfig = new McpRemoteServerConfig
        {
            Type = "http",
            Url = settings.McpServerUrl,
            Tools = ["*"]
        };

        config.McpServers = new Dictionary<string, object>
        {
            ["home-assistant"] = mcpConfig
        };

        return await _client.CreateSessionAsync(config);
    }

    public async Task<List<SessionMetadata>> ListSessionsAsync()
    {
        if (_client is null)
            throw new InvalidOperationException("Copilot client not started.");
        return await _client.ListSessionsAsync();
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        if (_client is null) return;
        await _client.DeleteSessionAsync(sessionId);
    }

    public async Task<List<string>> ListModelsAsync()
    {
        if (_client is null)
            throw new InvalidOperationException("Copilot client not started.");

        var models = await _client.ListModelsAsync();
        return models.Select(m => m.Id).ToList();
    }

    private static string GetSystemMessage()
    {
        return """
            <home_assistant_helper>
            You are a Home Assistant helper running as a desktop application.
            You have access to a Home Assistant instance via the HA-MCP server tools.

            When the user asks you to create or modify automations, return the YAML configuration
            in a fenced code block with the language tag `yaml`. For example:
            ```yaml
            automation:
              - alias: "Turn on lights at sunset"
                trigger:
                  - platform: sun
                    event: sunset
                action:
                  - service: light.turn_on
                    target:
                      entity_id: light.living_room
            ```

            When asked to create or modify dashboards, return Lovelace YAML in the same format.

            Guidelines:
            - Be concise and helpful
            - Use the HA-MCP tools to query device states, call services, and manage automations
            - When generating YAML, prefer native Home Assistant constructs over Jinja2 templates
            - Always confirm destructive actions before executing them
            - If you need to know entity IDs, use the search tools first
            </home_assistant_helper>
            """;
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            await _client.StopAsync();
            await _client.DisposeAsync();
            _client = null;
        }
    }
}
