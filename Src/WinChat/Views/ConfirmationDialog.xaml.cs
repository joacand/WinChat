using System.Windows;
using System.Windows.Input;

namespace WinChat.Views;

public partial class ConfirmationDialog : Window
{
    public ConfirmationDialog(string message, string attentionText = "", string title = "Confirmation")
    {
        InitializeComponent();
        TitleTextBlock.Text = title;
        MessageTextBlock.Text = message;
        AttentionTextBlock.Text = attentionText;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public static bool? Show(string message, string attentionText = "", string title = "Confirmation")
    {
        var msgBox = new ConfirmationDialog(message, attentionText, title)
        {
            Owner = Application.Current.MainWindow
        };
        return msgBox.ShowDialog();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}
