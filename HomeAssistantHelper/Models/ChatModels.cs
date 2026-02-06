namespace HomeAssistantHelper.Models;

public enum MessageRole
{
    User,
    Assistant,
    Tool
}

public class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "New Chat";
    public string? CopilotSessionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
    public List<ChatMessage> Messages { get; set; } = [];
}

public class ChatMessage
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? EventType { get; set; }
    public ChatSession? Session { get; set; }
}
