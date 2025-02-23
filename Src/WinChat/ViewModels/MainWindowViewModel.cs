using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Windows;
using WinChat.Infrastructure;
using WinChat.Infrastructure.Events;
using WinChat.Infrastructure.Models;
using WinChat.Infrastructure.Repository;
using WinChat.Services;
using WinChat.Views;

namespace WinChat.ViewModels;

internal partial class MainWindowViewModel : ObservableObject, IEventHandler<CharHistoryClearedEvent>
{
    private readonly ColorSettings _colorSettings;
    private readonly Channel<TextGenerationNotification> _textGenerationNotificationChannel;
    private readonly Channel<RequestTextGeneration> _textGenerationRequestChannel;
    private readonly AppDbContext _appDbContext;
    private readonly SoundService _soundService;
    private readonly ILogger<MainWindowViewModel> _logger;

    public ColorSettings ColorSettings => _colorSettings;

    [ObservableProperty]
    private string _userMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessageEntry> _chatMessages = [];

    public event Action? NewMessageAdded;

    public MainWindowViewModel(
        Channel<TextGenerationNotification> textGenerationNotificationChannel,
        Channel<RequestTextGeneration> textGenerationRequestChannel,
        AppDbContext appDbContext,
        ColorSettings colorSettings,
        SoundService soundService,
        EventDispatcher eventDispatcher,
        ILogger<MainWindowViewModel> logger)
    {
        _textGenerationNotificationChannel = textGenerationNotificationChannel;
        _textGenerationRequestChannel = textGenerationRequestChannel;
        _appDbContext = appDbContext;
        _colorSettings = colorSettings;
        _soundService = soundService;
        _logger = logger;

        StartReadingChannel();
        LoadHistory();

        eventDispatcher.Register(this);
        _logger.LogInformation("MainWindowViewModel initalized");
    }

    private void LoadHistory()
    {
        var chatMessages = _appDbContext.ChatMessages.OrderByDescending(x => x.TimeStamp).Take(10).Reverse();
        foreach (var chatMessage in chatMessages)
        {
            ChatMessages.Add(chatMessage);
        }
    }

    [RelayCommand]
    public static void OpenConfiguration()
    {
        ConfigurationView.Show();
    }

    public async Task Handle(CharHistoryClearedEvent @event)
    {
        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ChatMessages.Clear();
        });
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
            if (!string.IsNullOrWhiteSpace(message?.Error) || message?.Exception != null)
            {
                await HandleError(message);
                return;
            }

            if (string.IsNullOrWhiteSpace(message?.Text)) { continue; }
            message.Text = message.Text.Trim();
            _soundService.PlayNotification();

            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ChatMessages.Add(new ChatMessageEntry
                {
                    Role = "Assistant",
                    Content = message.Text
                });
                NewMessageAdded?.Invoke();
            });
            _appDbContext.ChatMessages.Add(new ChatMessageEntry
            {
                Role = "Assistant",
                Content = message.Text
            });
            await _appDbContext.SaveChangesAsync();
        }
    }

    private async Task HandleError(TextGenerationNotification message)
    {
        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ChatMessages.Add(new ChatMessageEntry
            {
                Role = "Assistant",
                Content = $"{message.Error} {message.Exception}"
            });
            NewMessageAdded?.Invoke();
        });
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

        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ChatMessages.Add(new ChatMessageEntry
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

        _appDbContext.ChatMessages.Add(new ChatMessageEntry
        {
            Role = "User",
            Content = message
        });
        await _appDbContext.SaveChangesAsync();
    }
}
