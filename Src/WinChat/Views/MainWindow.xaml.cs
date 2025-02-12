using System.Windows;
using WinChat.ViewModels;

namespace WinChat.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        (DataContext as MainWindowViewModel)!.NewMessageAdded += ViewModel_NewMessageAdded;
    }

    private void ViewModel_NewMessageAdded()
    {
        if (ChatScrollViewer.VerticalOffset == ChatScrollViewer.ScrollableHeight)
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}
