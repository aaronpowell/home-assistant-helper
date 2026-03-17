using HomeAssistantHelper.Models;
using HomeAssistantHelper.ViewModels;

namespace HomeAssistantHelper.Tests.ViewModels;

public class ChatMessageViewModelTests
{
    [Fact]
    public void Default_HasToolCalls_IsFalse()
    {
        var vm = new ChatMessageViewModel();
        Assert.False(vm.HasToolCalls);
    }

    [Fact]
    public void RefreshToolCallSummary_Sets_HasToolCalls_True()
    {
        var vm = new ChatMessageViewModel();
        vm.ToolCalls.Add(new ToolCallViewModel { Name = "test_tool" });
        vm.RefreshToolCallSummary();

        Assert.True(vm.HasToolCalls);
    }

    [Fact]
    public void ToolCallSummary_Shows_Correct_Count()
    {
        var vm = new ChatMessageViewModel();
        vm.ToolCalls.Add(new ToolCallViewModel { Name = "tool1" });
        Assert.Equal("🔧 1 tool call", vm.ToolCallSummary);

        vm.ToolCalls.Add(new ToolCallViewModel { Name = "tool2" });
        Assert.Equal("🔧 2 tool calls", vm.ToolCallSummary);
    }

    [Fact]
    public void Properties_Can_Be_Set()
    {
        var now = DateTime.UtcNow;
        var vm = new ChatMessageViewModel
        {
            Role = MessageRole.Assistant,
            Content = "Hello",
            Timestamp = now
        };

        Assert.Equal(MessageRole.Assistant, vm.Role);
        Assert.Equal("Hello", vm.Content);
        Assert.Equal(now, vm.Timestamp);
    }

    [Fact]
    public void Content_Change_Raises_PropertyChanged()
    {
        var vm = new ChatMessageViewModel();
        var raised = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ChatMessageViewModel.Content))
                raised = true;
        };

        vm.Content = "updated";
        Assert.True(raised);
    }
}

public class ToolCallViewModelTests
{
    [Fact]
    public void Defaults()
    {
        var vm = new ToolCallViewModel();
        Assert.Equal(string.Empty, vm.Name);
        Assert.Equal("Running...", vm.Status);
        Assert.False(vm.IsComplete);
    }

    [Fact]
    public void Can_Set_Properties()
    {
        var vm = new ToolCallViewModel
        {
            Name = "search_entities",
            Status = "✓ Done",
            IsComplete = true
        };

        Assert.Equal("search_entities", vm.Name);
        Assert.Equal("✓ Done", vm.Status);
        Assert.True(vm.IsComplete);
    }
}

public class SessionListItemTests
{
    [Fact]
    public void Default_Title_Is_NewChat()
    {
        var item = new SessionListItem();
        Assert.Equal("New Chat", item.Title);
    }

    [Fact]
    public void Title_Change_Raises_PropertyChanged()
    {
        var item = new SessionListItem();
        var raised = false;
        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SessionListItem.Title))
                raised = true;
        };

        item.Title = "My Session";
        Assert.True(raised);
    }

    [Fact]
    public void Properties_Can_Be_Set()
    {
        var item = new SessionListItem
        {
            Id = "test-id",
            Title = "Test",
            LastUsed = new DateTime(2025, 1, 1),
            CopilotSessionId = "copilot-123"
        };

        Assert.Equal("test-id", item.Id);
        Assert.Equal("Test", item.Title);
        Assert.Equal(new DateTime(2025, 1, 1), item.LastUsed);
        Assert.Equal("copilot-123", item.CopilotSessionId);
    }
}
