using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinChat.Infrastructure.Events;
using WinChat.Infrastructure.Repository;
using WinChat.Infrastructure.Services;

namespace WinChat.ViewModels;

internal partial class ConfigurationViewModel(
    IApiTokenConfiguration apiTokenConfiguration,
    AppDbContext appDbContext,
    EventDispatcher eventDispatcher) : ObservableObject
{
    [ObservableProperty]
    private string? _apiToken;

    [RelayCommand]
    public async Task SaveConfiguration()
    {
        if (string.IsNullOrWhiteSpace(ApiToken))
        {
            return;
        }

        await apiTokenConfiguration.SetApiToken(ApiToken);
    }

    [RelayCommand]
    public async Task ClearChatHistory()
    {
        await ClearChat();
        eventDispatcher.Publish(new CharHistoryClearedEvent());
    }

    /// <summary>
    /// Inefficient way of clearing history, can be improved by dropping the table instead
    /// </summary>
    private async Task ClearChat()
    {
        appDbContext.ChatMessages.RemoveRange(appDbContext.ChatMessages);
        await appDbContext.SaveChangesAsync();
    }
}
