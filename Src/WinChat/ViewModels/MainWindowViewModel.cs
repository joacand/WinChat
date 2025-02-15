using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Windows;
using WinChat.Infrastructure;
using WinChat.Infrastructure.Repository;

namespace WinChat.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ColorSettings _colorSettings;
    private readonly Channel<TextGenerationNotification> _textGenerationNotificationChannel;
    private readonly Channel<RequestTextGeneration> _textGenerationRequestChannel;
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<MainWindowViewModel> _logger;

    public ColorSettings ColorSettings => _colorSettings;

    [ObservableProperty]
    private string _userMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _chatMessages = [];

    public event Action? NewMessageAdded;

    public MainWindowViewModel(
        Channel<TextGenerationNotification> textGenerationNotificationChannel,
        Channel<RequestTextGeneration> textGenerationRequestChannel,
        AppDbContext appDbContext,
        ColorSettings colorSettings,
        ILogger<MainWindowViewModel> logger)
    {
        _textGenerationNotificationChannel = textGenerationNotificationChannel;
        _textGenerationRequestChannel = textGenerationRequestChannel;
        _appDbContext = appDbContext;
        _colorSettings = colorSettings;
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
                await ReadMessages();
                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Channel failed");
            }
        }
    }

    private async Task ReadMessages()
    {
        await foreach (var message in _textGenerationNotificationChannel.Reader.ReadAllAsync())
        {
            if (string.IsNullOrWhiteSpace(message?.Text)) { continue; }
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var messageWithoutCommand = SearchForCommands(message.Text);
                ChatMessages.Add(new ChatMessage
                {
                    Role = "Assistant",
                    Content = messageWithoutCommand
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
    }

    private string SearchForCommands(string text)
    {
        try
        {
            return _colorSettings.ProcessColorCommands(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply a command");
            return text;
        }
    }

    [RelayCommand]
    public async Task SendChat()
    {
        if (string.IsNullOrWhiteSpace(UserMessage))
        {
            return;
        }

        var message = UserMessage;
        UserMessage = string.Empty;

        _appDbContext.ChatMessages.Add(new ChatMessage
        {
            Role = "User",
            Content = message
        });
        await _appDbContext.SaveChangesAsync();

        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ChatMessages.Add(new ChatMessage
            {
                Role = "User",
                Content = message
            });
            NewMessageAdded?.Invoke();
        });

        await _textGenerationRequestChannel.Writer.WriteAsync(new RequestTextGeneration
        {
            Prompt = message,
            SystemPrompt = Constants.UserInputSystemPrompt
        });
    }
}
