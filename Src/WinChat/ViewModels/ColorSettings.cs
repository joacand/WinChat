using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Windows.Media;
using WinChat.Infrastructure.Events;

namespace WinChat.ViewModels;

public partial class ColorSettings : ObservableObject, IEventHandler<ColorChangeRequestedEvent>
{
    private readonly ILogger<ColorSettings> logger;

    public ColorSettings(EventDispatcher eventDispatcher, ILogger<ColorSettings> logger)
    {
        eventDispatcher.Register(this);
        this.logger = logger;
        ApplyDefaultDarkTheme();
    }

    private void ApplyDefaultDarkTheme()
    {
        BackgroundColorHex = "#222222";
        AssistantChatColorHex = "#333333";
        UserChatColorHex = "#444444";
        ForegroundColorHex = "#DDDDDD";
    }

    public Task Handle(ColorChangeRequestedEvent @event)
    {
        if (@event.ColorType == ColorType.ForegroundColor)
            ForegroundColorHex = @event.RgbColor;
        else if (@event.ColorType == ColorType.BackgroundColor)
            BackgroundColorHex = @event.RgbColor;
        else if (@event.ColorType == ColorType.AssistantChatColor)
            AssistantChatColorHex = @event.RgbColor;
        else if (@event.ColorType == ColorType.UserChatColor)
            UserChatColorHex = @event.RgbColor;
        return Task.CompletedTask;
    }

    [ObservableProperty]
    private Brush _foregroundColor = Brushes.Black;
    private string? _forgroundColorHex;
    public string? ForegroundColorHex
    {
        get => _forgroundColorHex;
        set
        {
            try
            {
                ForegroundColor = (Brush)new BrushConverter().ConvertFrom(value!)!;
                SetProperty(ref _forgroundColorHex, value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply foreground color");
            }
        }
    }

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
                BackgroundColor = (Brush)new BrushConverter().ConvertFrom(value!)!;
                SetProperty(ref _backgroundColorHex, value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply background color");
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
                AssistantChatColor = (Brush)new BrushConverter().ConvertFrom(value!)!;
                SetProperty(ref _assistantChatColorHex, value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply assistant color");
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
                UserChatColor = (Brush)new BrushConverter().ConvertFrom(value!)!;
                SetProperty(ref _userChatColorHex, value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to apply user color");
            }
        }
    }
}
