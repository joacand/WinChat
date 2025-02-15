﻿using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Windows.Media;
using WinChat.Infrastructure;

namespace WinChat.ViewModels;

public partial class ColorSettings(ILogger<ColorSettings> logger) : ObservableObject
{
    public string ProcessColorCommands(string text)
    {
        var commands = new Dictionary<string, Action<string>>
        {
            [Constants.Commands.BackgroundColor] = hex => BackgroundColorHex = hex,
            [Constants.Commands.AssistantChatColor] = hex => AssistantChatColorHex = hex,
            [Constants.Commands.UserChatColor] = hex => UserChatColorHex = hex
        };

        var processedText = text;

        foreach (var (command, setColor) in commands)
        {
            var fullCommand = $"#{command}#";
            if (!processedText.Contains(fullCommand)) continue;

            var parts = processedText.Split([fullCommand], StringSplitOptions.None);
            if (parts.Length < 2) continue;

            var hex = new string([.. parts[1].Take(6)]);
            if (hex.Length != 6) continue;

            setColor("#" + hex);
            processedText = processedText
                .Replace($"{fullCommand}{hex}", "")
                .Replace("  ", " ")
                .Replace("{ }", "")
                .Replace("{}", "");
        }

        return processedText;
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
