using CommunityToolkit.Mvvm.DependencyInjection;
using WinChat.ViewModels;

namespace WinChat;

internal static class ViewModelLocator
{
    public static MainWindowViewModel Main => Ioc.Default.GetRequiredService<MainWindowViewModel>();
    public static ConfigurationViewModel Configuration => Ioc.Default.GetRequiredService<ConfigurationViewModel>();
}
