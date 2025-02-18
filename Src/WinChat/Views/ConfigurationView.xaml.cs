using System.Windows;
using System.Windows.Input;
using WinChat.ViewModels;

namespace WinChat.Views
{
    public partial class ConfigurationView : Window
    {
        public ConfigurationView()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ((ConfigurationViewModel)DataContext).SaveConfigurationCommand.Execute(null);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static new bool? Show()
        {
            var msgBox = new ConfigurationView()
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
}
