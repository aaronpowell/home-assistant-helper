using HomeAssistantHelper.Data;
using HomeAssistantHelper.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeAssistantHelper.Services;

public class SessionRepository(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<ChatSession>> GetAllSessionsAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Sessions
            .OrderByDescending(s => s.LastUsedAt)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetSessionAsync(string id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Sessions
            .Include(s => s.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ChatSession> CreateSessionAsync(string? copilotSessionId = null)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var session = new ChatSession
        {
            CopilotSessionId = copilotSessionId
        };
        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    public async Task UpdateSessionAsync(ChatSession session)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Sessions.Update(session);
        await db.SaveChangesAsync();
    }

    public async Task AddMessageAsync(string sessionId, MessageRole role, string content, string? eventType = null)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var message = new ChatMessage
        {
            SessionId = sessionId,
            Role = role,
            Content = content,
            EventType = eventType
        };
        db.Messages.Add(message);

        var session = await db.Sessions.FindAsync(sessionId);
        if (session is not null)
        {
            session.LastUsedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteSessionAsync(string id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var session = await db.Sessions.FindAsync(id);
        if (session is not null)
        {
            db.Sessions.Remove(session);
            await db.SaveChangesAsync();
        }
    }

    public async Task UpdateSessionTitleAsync(string sessionId, string title)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var session = await db.Sessions.FindAsync(sessionId);
        if (session is not null)
        {
            session.Title = title;
            await db.SaveChangesAsync();
        }
    }
}
