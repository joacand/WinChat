using CommunityToolkit.Mvvm.DependencyInjection;
using WinChat.ViewModels;

namespace WinChat;

internal static class ViewModelLocator
{
    public static MainWindowViewModel Main => Ioc.Default.GetRequiredService<MainWindowViewModel>();
}
