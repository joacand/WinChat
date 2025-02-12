using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Windows;
using WinChat.Infrastructure;
using WinChat.Infrastructure.Repository;

namespace WinChat.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{
    private readonly Channel<TextGenerationNotification> _textGenerationNotificationChannel;
    private readonly Channel<RequestTextGeneration> _textGenerationRequestChannel;
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    private string _userMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _chatMessages = [];

    public event Action? NewMessageAdded;

    public MainWindowViewModel(
        Channel<TextGenerationNotification> textGenerationNotificationChannel,
        Channel<RequestTextGeneration> textGenerationRequestChannel,
        AppDbContext appDbContext,
        ILogger<MainWindowViewModel> logger)
    {
        _textGenerationNotificationChannel = textGenerationNotificationChannel;
        _textGenerationRequestChannel = textGenerationRequestChannel;
        _appDbContext = appDbContext;
        _logger = logger;

        StartReadingChannel();

        _logger.LogInformation("MainWindowViewModel initalized");
    }

    private async void StartReadingChannel()
    {
        while (true)
        {
            try
            {
                await foreach (var message in _textGenerationNotificationChannel.Reader.ReadAllAsync())
                {
                    if (string.IsNullOrWhiteSpace(message?.Text)) { continue; }
                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ChatMessages.Add(new ChatMessage
                        {
                            Role = "Assistant",
                            Content = message.Text
                        });
                        NewMessageAdded?.Invoke();
                    });
                    _appDbContext.ChatMessages.Add(new ChatMessage
                    {
                        Role = "Assistant",
                        Content = message.Text
                    });
                    await _appDbContext.SaveChangesAsync();
                }
                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Channel failed");
            }
        }
    }

    [RelayCommand]
    public async Task TestApi()
    {
        if (string.IsNullOrWhiteSpace(UserMessage))
        {
            return;
        }

        _appDbContext.ChatMessages.Add(new ChatMessage
        {
            Role = "User",
            Content = UserMessage
        });
        await _appDbContext.SaveChangesAsync();

        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ChatMessages.Add(new ChatMessage
            {
                Role = "User",
                Content = UserMessage
            });
            NewMessageAdded?.Invoke();
        });

        await _textGenerationRequestChannel.Writer.WriteAsync(new RequestTextGeneration
        {
            Prompt = UserMessage,
            SystemPrompt = Constants.UserInputSystemPrompt
        });
    }
}
