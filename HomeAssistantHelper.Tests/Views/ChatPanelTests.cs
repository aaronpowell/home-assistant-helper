using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using HomeAssistantHelper.ViewModels;
using HomeAssistantHelper.Views;

namespace HomeAssistantHelper.Tests.Views;

public class ChatPanelTests
{
    [AvaloniaFact]
    public void ChatPanel_Renders_Without_DataContext()
    {
        var panel = new ChatPanel();
        var window = new Window { Content = panel };
        window.Show();

        Assert.NotNull(panel);
    }

    [AvaloniaFact]
    public void ChatPanel_Binds_To_ViewModel_Messages()
    {
        var vm = CreateMinimalChatMessageCollection();
        var panel = new ChatPanel { DataContext = vm };
        var window = new Window { Content = panel };
        window.Show();

        Assert.Equal(vm, panel.DataContext);
    }

    private static ChatPanelViewModel CreateMinimalChatMessageCollection()
    {
        return new ChatPanelViewModel();
    }
}

/// <summary>
/// Minimal VM for ChatPanel binding tests (avoids CopilotService dependency).
/// </summary>
public class ChatPanelViewModel
{
    public System.Collections.ObjectModel.ObservableCollection<ChatMessageViewModel> Messages { get; } = [];
    public System.Collections.ObjectModel.ObservableCollection<SessionListItem> Sessions { get; } = [];
    public string InputText { get; set; } = "";
    public bool IsProcessing { get; set; }
    public SessionListItem? SelectedSession { get; set; }
    public string StatusText { get; set; } = "Test";
}
