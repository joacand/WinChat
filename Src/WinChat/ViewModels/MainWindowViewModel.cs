using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Media;
using WinChat.Infrastructure;
using WinChat.Infrastructure.Repository;

namespace WinChat.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly Channel<TextGenerationNotification> _textGenerationNotificationChannel;
    private readonly Channel<RequestTextGeneration> _textGenerationRequestChannel;
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<MainWindowViewModel> _logger;

    [ObservableProperty]
    private Brush _backgroundColor = Brushes.DarkGray;
    private string? _backgroundColorHex;
    public string? BackgroundColorHex
    {
        get => _backgroundColorHex;
        set
        {
            try
            {
                BackgroundColor = (Brush)new BrushConverter()!.ConvertFrom(value!)!;
                SetProperty(ref _backgroundColorHex, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert color");
            }
        }
    }

    [ObservableProperty]
    private Brush _assistantChatColor = Brushes.LightCyan;
    private string? _assistantChatColorHex;
    public string? AssistantChatColorHex
    {
        get => _assistantChatColorHex;
        set
        {
            try
            {
                AssistantChatColor = (Brush)new BrushConverter()!.ConvertFrom(value!)!;
                SetProperty(ref _assistantChatColorHex, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert color");
            }
        }
    }

    [ObservableProperty]
    private Brush _userChatColor = Brushes.LightBlue;
    private string? _userChatColorHex;
    public string? UserChatColorHex
    {
        get => _userChatColorHex;
        set
        {
            try
            {
                UserChatColor = (Brush)new BrushConverter()!.ConvertFrom(value!)!;
                SetProperty(ref _userChatColorHex, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert color");
            }
        }
    }

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
                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Channel failed");
            }
        }
    }

    private string SearchForCommands(string text)
    {
        var resultingString = text;
        try
        {
            var command = "BACKGROUNDCOLOR";
            if (text.Contains($"#{command}#"))
            {
                resultingString = resultingString.Replace($"#{command}", "");
                var bgc = new string([.. text.Split($"#{command}")[1].Take(7)]);
                resultingString = resultingString.Replace(bgc, "").Replace("  ", " ").Replace("{ }", "");
                BackgroundColorHex = bgc;
            }
            command = "ASSISTANTCHATCOLOR";
            if (text.Contains($"#{command}#"))
            {
                resultingString = resultingString.Replace($"#{command}", "");
                var bgc = new string([.. text.Split($"#{command}")[1].Take(7)]);
                resultingString = resultingString.Replace(bgc, "").Replace("  ", " ").Replace("{ }", "");
                AssistantChatColorHex = bgc;
            }
            command = "USERCHATCOLOR";
            if (text.Contains($"#{command}#"))
            {
                resultingString = resultingString.Replace($"#{command}", "");
                var bgc = new string([.. text.Split($"#{command}")[1].Take(7)]);
                resultingString = resultingString.Replace(bgc, "").Replace("  ", " ").Replace("{ }", "");
                UserChatColorHex = bgc;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply a command");
        }
        return resultingString;
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
