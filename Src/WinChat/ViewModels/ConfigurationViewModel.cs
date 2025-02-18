using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinChat.Infrastructure.Services;

namespace WinChat.ViewModels;

internal partial class ConfigurationViewModel(IGenerateTextService generateTextService) : ObservableObject
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
}
