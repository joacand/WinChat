using System.Windows;
using System.Windows.Input;
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

    private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            (DataContext as MainWindowViewModel)!.SendChatCommand.Execute(null);
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ChatTextBox.Focus();
    }
}
