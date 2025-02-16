using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Windows.Media;
using WinChat.Infrastructure;

namespace WinChat.ViewModels;

public partial class ColorSettings : ObservableObject
{
    private readonly ILogger<ColorSettings> logger;

    public ColorSettings(ILogger<ColorSettings> logger)
    {
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

    public string ProcessColorCommands(string text)
    {
        var commands = new Dictionary<string, Action<string>>
        {
            [Constants.Commands.BackgroundColor.Name] = hex => BackgroundColorHex = hex,
            [Constants.Commands.AssistantChatColor.Name] = hex => AssistantChatColorHex = hex,
            [Constants.Commands.UserChatColor.Name] = hex => UserChatColorHex = hex,
            [Constants.Commands.ForegroundColor.Name] = hex => ForegroundColorHex = hex
        };

        var processedText = text;

        foreach (var (command, setColor) in commands)
        {
            var commandWithAffixes = $"{{{command}:";
            if (!processedText.Contains(commandWithAffixes)) continue;

            var parts = processedText.Split([commandWithAffixes], StringSplitOptions.None);
            if (parts.Length < 2) continue;

            var args = new string([.. parts[1].TakeWhile(x => x != '}')]);
            if (args.Length != 6) continue;

            setColor("#" + args);
            processedText = processedText
                .Replace($"{commandWithAffixes}{args}}}", string.Empty)
                .Replace("  ", " ");
        }

        return processedText;
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
