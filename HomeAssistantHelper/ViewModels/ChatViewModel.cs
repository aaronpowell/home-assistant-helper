using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHub.Copilot.SDK;
using HomeAssistantHelper.Models;
using HomeAssistantHelper.Services;

namespace HomeAssistantHelper.ViewModels;

public partial class ChatViewModel(CopilotService copilotService, SessionRepository sessionRepository, SettingsService settingsService) : ObservableObject
{
    private CopilotSession? _copilotSession;
    private IDisposable? _eventSubscription;

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];
    public ObservableCollection<SessionListItem> Sessions { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private SessionListItem? _selectedSession;

    [ObservableProperty]
    private string _statusText = "Not connected";

    [ObservableProperty]
    private bool _isConnected;

    public async Task InitializeAsync()
    {
        try
        {
            StatusText = "Starting Copilot...";
            await copilotService.StartAsync();
            IsConnected = true;
            StatusText = "Connected";
            await LoadSessionsAsync();

            // Auto-create a session if none exist
            if (Sessions.Count == 0)
            {
                await NewSessionAsync();
            }
            else
            {
                SelectedSession = Sessions[0];
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            IsConnected = false;
        }
    }

    private async Task LoadSessionsAsync()
    {
        var sessions = await sessionRepository.GetAllSessionsAsync();
        Sessions.Clear();
        foreach (var s in sessions)
        {
            Sessions.Add(new SessionListItem
            {
                Id = s.Id,
                Title = s.Title,
                LastUsed = s.LastUsedAt,
                CopilotSessionId = s.CopilotSessionId
            });
        }
    }

    async partial void OnSelectedSessionChanged(SessionListItem? value)
    {
        if (value is null) return;
        await SwitchToSessionAsync(value);
    }

    private async Task SwitchToSessionAsync(SessionListItem item)
    {
        _eventSubscription?.Dispose();
        Messages.Clear();

        // Load existing messages from DB
        var session = await sessionRepository.GetSessionAsync(item.Id);
        if (session?.Messages is not null)
        {
            foreach (var msg in session.Messages)
            {
                Messages.Add(new ChatMessageViewModel
                {
                    Role = msg.Role,
                    Content = msg.Content,
                    Timestamp = msg.Timestamp
                });
            }
        }

        // Resume or create Copilot session
        try
        {
            var settings = await settingsService.LoadAsync();
            _copilotSession = await copilotService.CreateSessionAsync(settings, item.CopilotSessionId);

            // Store the Copilot session ID if this is a new association
            if (item.CopilotSessionId is null)
            {
                item.CopilotSessionId = _copilotSession.SessionId;
                await sessionRepository.UpdateSessionAsync(new ChatSession
                {
                    Id = item.Id,
                    Title = item.Title,
                    CopilotSessionId = _copilotSession.SessionId,
                    LastUsedAt = DateTime.UtcNow,
                    CreatedAt = item.LastUsed
                });
            }

            SubscribeToEvents();
            StatusText = $"Session: {item.Title}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error connecting session: {ex.Message}";
        }
    }

    private void SubscribeToEvents()
    {
        if (_copilotSession is null) return;

        _eventSubscription = _copilotSession.On(evt =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                switch (evt)
                {
                    case AssistantMessageDeltaEvent delta:
                        AppendDelta(delta.Data.DeltaContent);
                        break;
                    case AssistantMessageEvent msg:
                        FinalizeAssistantMessage(msg.Data.Content);
                        break;
                    case ToolExecutionStartEvent toolStart:
                        AddToolCall(toolStart.Data.ToolName);
                        break;
                    case ToolExecutionCompleteEvent toolComplete:
                        CompleteToolCall(toolComplete.Data.Success);
                        break;
                    case SessionIdleEvent:
                        _currentAssistantMessage = null;
                        IsProcessing = false;
                        break;
                    case SessionErrorEvent error:
                        AddErrorMessage(error.Data.Message);
                        _currentAssistantMessage = null;
                        IsProcessing = false;
                        break;
                }
            });
        });
    }

    private ChatMessageViewModel? _currentAssistantMessage;

    private void AppendDelta(string? deltaContent)
    {
        if (string.IsNullOrEmpty(deltaContent)) return;

        if (_currentAssistantMessage is null)
        {
            _currentAssistantMessage = new ChatMessageViewModel
            {
                Role = MessageRole.Assistant,
                Content = deltaContent,
                Timestamp = DateTime.UtcNow
            };
            Messages.Add(_currentAssistantMessage);
        }
        else
        {
            _currentAssistantMessage.Content += deltaContent;
        }
    }

    private void FinalizeAssistantMessage(string? content)
    {
        // If no delta started the message, create it now from the final content
        if (_currentAssistantMessage is null && !string.IsNullOrEmpty(content))
        {
            _currentAssistantMessage = new ChatMessageViewModel
            {
                Role = MessageRole.Assistant,
                Content = content,
                Timestamp = DateTime.UtcNow
            };
            Messages.Add(_currentAssistantMessage);
        }
        else if (_currentAssistantMessage is not null && !string.IsNullOrEmpty(content))
        {
            _currentAssistantMessage.Content = content;
        }

        if (_currentAssistantMessage is null) return;

        // Persist to DB
        _ = sessionRepository.AddMessageAsync(
            SelectedSession!.Id,
            MessageRole.Assistant,
            _currentAssistantMessage.Content,
            "assistant.message");

        // Auto-title the session from first assistant response
        if (SelectedSession?.Title == "New Chat" && Messages.Count <= 3)
        {
            var title = _currentAssistantMessage.Content.Length > 50
                ? _currentAssistantMessage.Content[..50] + "..."
                : _currentAssistantMessage.Content;
            title = title.Replace("\n", " ").Trim();
            SelectedSession.Title = title;
            _ = sessionRepository.UpdateSessionTitleAsync(SelectedSession.Id, title);
        }

        // Don't null _currentAssistantMessage here — SessionIdle resets it
    }

    private ChatMessageViewModel GetOrCreateAssistantMessage()
    {
        if (_currentAssistantMessage is null)
        {
            _currentAssistantMessage = new ChatMessageViewModel
            {
                Role = MessageRole.Assistant,
                Content = string.Empty,
                Timestamp = DateTime.UtcNow
            };
            Messages.Add(_currentAssistantMessage);
        }
        return _currentAssistantMessage;
    }

    private void AddToolCall(string? toolName)
    {
        var msg = GetOrCreateAssistantMessage();
        msg.ToolCalls.Add(new ToolCallViewModel
        {
            Name = toolName ?? "Unknown tool"
        });
        msg.RefreshToolCallSummary();
    }

    private void CompleteToolCall(bool success)
    {
        if (_currentAssistantMessage is null) return;
        var lastPending = _currentAssistantMessage.ToolCalls.LastOrDefault(t => !t.IsComplete);
        if (lastPending is not null)
        {
            lastPending.Status = success ? "✓ Done" : "✗ Failed";
            lastPending.IsComplete = true;
        }
    }

    private void AddToolMessage(string text)
    {
        Messages.Add(new ChatMessageViewModel
        {
            Role = MessageRole.Tool,
            Content = text,
            Timestamp = DateTime.UtcNow
        });
    }

    private void UpdateLastToolMessage(string text)
    {
        var lastTool = Messages.LastOrDefault(m => m.Role == MessageRole.Tool);
        if (lastTool is not null)
        {
            lastTool.Content = text;
        }
    }

    private void AddErrorMessage(string? message)
    {
        Messages.Add(new ChatMessageViewModel
        {
            Role = MessageRole.Assistant,
            Content = $"⚠️ Error: {message ?? "Unknown error"}",
            Timestamp = DateTime.UtcNow
        });
    }

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessageAsync()
    {
        if (_copilotSession is null || string.IsNullOrWhiteSpace(InputText)) return;

        var text = InputText.Trim();
        InputText = string.Empty;
        IsProcessing = true;

        // Add user message to UI
        Messages.Add(new ChatMessageViewModel
        {
            Role = MessageRole.User,
            Content = text,
            Timestamp = DateTime.UtcNow
        });

        // Persist user message
        await sessionRepository.AddMessageAsync(SelectedSession!.Id, MessageRole.User, text, "user.message");

        try
        {
            await _copilotSession.SendAsync(new MessageOptions { Prompt = text });
        }
        catch (Exception ex)
        {
            AddErrorMessage(ex.Message);
            IsProcessing = false;
        }
    }

    private bool CanSendMessage() => !string.IsNullOrWhiteSpace(InputText) && !IsProcessing;

    [RelayCommand]
    private async Task NewSessionAsync()
    {
        try
        {
            var settings = await settingsService.LoadAsync();
            _copilotSession = await copilotService.CreateSessionAsync(settings);

            var dbSession = await sessionRepository.CreateSessionAsync(_copilotSession.SessionId);
            var item = new SessionListItem
            {
                Id = dbSession.Id,
                Title = dbSession.Title,
                LastUsed = dbSession.LastUsedAt,
                CopilotSessionId = _copilotSession.SessionId
            };
            Sessions.Insert(0, item);

            _eventSubscription?.Dispose();
            Messages.Clear();
            _currentAssistantMessage = null;
            SelectedSession = item;
            SubscribeToEvents();
            StatusText = "New session created";
        }
        catch (Exception ex)
        {
            StatusText = $"Error creating session: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteSessionAsync(SessionListItem? session)
    {
        if (session is null) return;

        await sessionRepository.DeleteSessionAsync(session.Id);
        if (session.CopilotSessionId is not null)
        {
            try { await copilotService.DeleteSessionAsync(session.CopilotSessionId); } catch { }
        }

        Sessions.Remove(session);
        if (SelectedSession == session)
        {
            Messages.Clear();
            _copilotSession = null;
            if (Sessions.Count > 0)
                SelectedSession = Sessions[0];
            else
                await NewSessionAsync();
        }
    }
}

public partial class ChatMessageViewModel : ObservableObject
{
    [ObservableProperty]
    private MessageRole _role;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private DateTime _timestamp;

    public ObservableCollection<ToolCallViewModel> ToolCalls { get; } = [];

    [ObservableProperty]
    private bool _hasToolCalls;

    public string ToolCallSummary => $"🔧 {ToolCalls.Count} tool call{(ToolCalls.Count == 1 ? "" : "s")}";

    public void RefreshToolCallSummary()
    {
        HasToolCalls = ToolCalls.Count > 0;
        OnPropertyChanged(nameof(ToolCallSummary));
    }
}

public partial class ToolCallViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _status = "Running...";

    [ObservableProperty]
    private bool _isComplete;
}

public partial class SessionListItem : ObservableObject
{
    public string Id { get; set; } = string.Empty;

    [ObservableProperty]
    private string _title = "New Chat";

    public DateTime LastUsed { get; set; }
    public string? CopilotSessionId { get; set; }
}
