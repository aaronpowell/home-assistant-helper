using HomeAssistantHelper.Models;
using HomeAssistantHelper.Services;
using HomeAssistantHelper.Tests.Helpers;

namespace HomeAssistantHelper.Tests.Services;

public class SessionRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory;
    private readonly SessionRepository _repo;

    public SessionRepositoryTests()
    {
        _factory = (TestDbContextFactory)DbHelper.CreateInMemoryFactory();
        _repo = new SessionRepository(_factory);
    }

    [Fact]
    public async Task CreateSession_Returns_NewSession()
    {
        var session = await _repo.CreateSessionAsync("copilot-123");

        Assert.NotNull(session);
        Assert.Equal("New Chat", session.Title);
        Assert.Equal("copilot-123", session.CopilotSessionId);
        Assert.NotEmpty(session.Id);
    }

    [Fact]
    public async Task GetAllSessions_Returns_OrderedByLastUsed()
    {
        var s1 = await _repo.CreateSessionAsync();
        await Task.Delay(50);
        var s2 = await _repo.CreateSessionAsync();

        var all = await _repo.GetAllSessionsAsync();

        Assert.Equal(2, all.Count);
        Assert.Equal(s2.Id, all[0].Id);
        Assert.Equal(s1.Id, all[1].Id);
    }

    [Fact]
    public async Task GetSession_ReturnsNull_ForMissing()
    {
        var result = await _repo.GetSessionAsync("nonexistent");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSession_IncludesMessages()
    {
        var session = await _repo.CreateSessionAsync();
        await _repo.AddMessageAsync(session.Id, MessageRole.User, "Hello");
        await _repo.AddMessageAsync(session.Id, MessageRole.Assistant, "Hi there");

        var loaded = await _repo.GetSessionAsync(session.Id);

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded!.Messages.Count);
        Assert.Equal("Hello", loaded.Messages[0].Content);
        Assert.Equal(MessageRole.User, loaded.Messages[0].Role);
        Assert.Equal("Hi there", loaded.Messages[1].Content);
        Assert.Equal(MessageRole.Assistant, loaded.Messages[1].Role);
    }

    [Fact]
    public async Task AddMessage_Updates_SessionLastUsedAt()
    {
        var session = await _repo.CreateSessionAsync();
        var originalTime = session.LastUsedAt;
        await Task.Delay(50);

        await _repo.AddMessageAsync(session.Id, MessageRole.User, "test");

        var loaded = await _repo.GetSessionAsync(session.Id);
        Assert.True(loaded!.LastUsedAt > originalTime);
    }

    [Fact]
    public async Task UpdateSessionTitle_Works()
    {
        var session = await _repo.CreateSessionAsync();
        await _repo.UpdateSessionTitleAsync(session.Id, "My Chat");

        var loaded = await _repo.GetSessionAsync(session.Id);
        Assert.Equal("My Chat", loaded!.Title);
    }

    [Fact]
    public async Task DeleteSession_Removes_Session_And_Messages()
    {
        var session = await _repo.CreateSessionAsync();
        await _repo.AddMessageAsync(session.Id, MessageRole.User, "test");

        await _repo.DeleteSessionAsync(session.Id);

        var loaded = await _repo.GetSessionAsync(session.Id);
        Assert.Null(loaded);

        var all = await _repo.GetAllSessionsAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task UpdateSession_Persists_Changes()
    {
        var session = await _repo.CreateSessionAsync();
        session.Title = "Updated";
        session.CopilotSessionId = "new-copilot-id";

        await _repo.UpdateSessionAsync(session);

        var loaded = await _repo.GetSessionAsync(session.Id);
        Assert.Equal("Updated", loaded!.Title);
        Assert.Equal("new-copilot-id", loaded.CopilotSessionId);
    }

    [Fact]
    public async Task AddMessage_WithEventType_Persists()
    {
        var session = await _repo.CreateSessionAsync();
        await _repo.AddMessageAsync(session.Id, MessageRole.Tool, "tool output", "tool.execution");

        var loaded = await _repo.GetSessionAsync(session.Id);
        var msg = Assert.Single(loaded!.Messages);
        Assert.Equal(MessageRole.Tool, msg.Role);
        Assert.Equal("tool output", msg.Content);
        Assert.Equal("tool.execution", msg.EventType);
    }

    public void Dispose() => _factory.Dispose();
}
