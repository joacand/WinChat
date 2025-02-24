using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Windows.Media;
using WinChat.Infrastructure.Events;
using WinChat.Infrastructure.Repository;

namespace WinChat.ViewModels;

public partial class ColorSettings : ObservableObject, IEventHandler<ColorChangeRequestedEvent>
{
    private readonly ColorSettingsRepository colorSettingsRepository;
    private readonly ILogger<ColorSettings> logger;

    public ColorSettings(EventDispatcher eventDispatcher, ColorSettingsRepository colorSettingsRepository, ILogger<ColorSettings> logger)
    {
        eventDispatcher.Register(this);
        this.colorSettingsRepository = colorSettingsRepository;
        this.logger = logger;
    }

    public async Task Handle(ColorChangeRequestedEvent @event)
    {
        if (@event.ColorType == ColorType.ForegroundColor)
            ForegroundColorHex = @event.RgbColor;
        else if (@event.ColorType == ColorType.BackgroundColor)
            BackgroundColorHex = @event.RgbColor;
        else if (@event.ColorType == ColorType.AssistantChatColor)
            AssistantChatColorHex = @event.RgbColor;
        else if (@event.ColorType == ColorType.UserChatColor)
            UserChatColorHex = @event.RgbColor;

        await SaveColorsAsync();
    }

    public async Task LoadColorsAtStartup()
    {
        var colorSettings = await colorSettingsRepository.LoadColorSettingsAsync();
        if (colorSettings != null)
        {
            ForegroundColorHex = colorSettings.ForegroundColorHex;
            BackgroundColorHex = colorSettings.BackgroundColorHex;
            AssistantChatColorHex = colorSettings.AssistantChatColorHex;
            UserChatColorHex = colorSettings.UserChatColorHex;
        }
        else
        {
            ApplyDefaultDarkTheme();
        }
    }

    private void ApplyDefaultDarkTheme()
    {
        ForegroundColorHex = "#DDDDDD";
        BackgroundColorHex = "#222222";
        AssistantChatColorHex = "#333333";
        UserChatColorHex = "#444444";
    }

    private Task SaveColorsAsync() =>
        colorSettingsRepository.SaveColorSettingsAsync(new()
        {
            ForegroundColorHex = ForegroundColorHex,
            BackgroundColorHex = BackgroundColorHex,
            AssistantChatColorHex = AssistantChatColorHex,
            UserChatColorHex = UserChatColorHex
        });

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
