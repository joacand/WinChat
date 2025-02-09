using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WinChat.Infrastructure;

namespace WinChat.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{
    private readonly AiPromptService _aiPromptService;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    private string _response = string.Empty;

    public MainWindowViewModel(
        AiPromptService aiPromptService,
        ILogger<MainWindowViewModel> logger)
    {
        _aiPromptService = aiPromptService;
        _logger = logger;
        _logger.LogInformation("MainWindowViewModel initalized");
    }

    [RelayCommand]
    public async Task TestApi()
    {
        Response = await _aiPromptService.TestApi();
    }
}
