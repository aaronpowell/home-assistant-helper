using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Threading;

namespace HomeAssistantHelper.Views;

public partial class ChatPanel : UserControl
{
    public ChatPanel()
    {
        InitializeComponent();

        // Auto-scroll to bottom when new messages arrive
        Loaded += (_, _) =>
        {
            if (MessagesList.ItemsSource is INotifyCollectionChanged c)
            {
                c.CollectionChanged += (_, _) =>
                {
                    Dispatcher.UIThread.Post(() =>
                        MessagesScrollViewer.ScrollToEnd(),
                        DispatcherPriority.Background);
                };
            }
        };
    }
}
