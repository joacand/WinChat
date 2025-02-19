using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinChat.Infrastructure.Events;
using WinChat.Infrastructure.Repository;
using WinChat.Infrastructure.Services;

namespace WinChat.ViewModels;

internal partial class ConfigurationViewModel(
    IGenerateTextService generateTextService,
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

        await generateTextService.SetApiToken(ApiToken);
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
