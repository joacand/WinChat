using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using WinChat.Infrastructure;
using WinChat.ViewModels;

namespace WinChat.UnitTests;

public class ColorSettingsTests
{
    private static ColorSettings CreateSut() => new(NullLogger<ColorSettings>.Instance);

    [Fact]
    public void Constructor_WhenCalled_ShouldApplyDefaultDarkTheme()
    {
        var settings = CreateSut();

        settings.BackgroundColorHex.ShouldNotBeNullOrWhiteSpace();
        settings.AssistantChatColorHex.ShouldNotBeNullOrWhiteSpace();
        settings.UserChatColorHex.ShouldNotBeNullOrWhiteSpace();
        settings.ForegroundColorHex.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ProcessColorCommands_WithValidBackgroundColorCommand_UpdatesBackgroundColor()
    {
        var settings = CreateSut();
        string command = $"{{{Constants.Commands.BackgroundColor}:123ABC}}";
        string message = $"Some text {command} remaining text";

        var result = settings.ProcessColorCommands(message);

        settings.BackgroundColorHex.ShouldBe("#123ABC");
        result.ShouldNotContain(command);
    }

    [Fact]
    public void ProcessColorCommands_WithInvalidHexLength_DoesNotUpdateForegroundColor()
    {
        var settings = CreateSut();

        var defaultColor = settings.ForegroundColorHex;

        string command = $"{{{Constants.Commands.ForegroundColor}:FFF}}";
        string message = $"Test message {command} end.";

        var result = settings.ProcessColorCommands(message);

        settings.ForegroundColorHex.ShouldBe(defaultColor);
        result.ShouldContain(command);
    }

    [Fact]
    public void ProcessColorCommands_WithMultipleCommands_UpdatesColorsAndRemovesCommands()
    {
        var settings = CreateSut();
        string bgCommand = $"{{{Constants.Commands.BackgroundColor}:ABCDEF}}";
        string fgCommand = $"{{{Constants.Commands.ForegroundColor}:123456}}";
        string text = $"Before {bgCommand} and {fgCommand} after.";

        var result = settings.ProcessColorCommands(text);

        settings.BackgroundColorHex.ShouldBe("#ABCDEF");
        settings.ForegroundColorHex.ShouldBe("#123456");

        result.ShouldNotContain(bgCommand);
        result.ShouldNotContain(fgCommand);
    }

    [Fact]
    public void ProcessColorCommands_WithNoCommands_ReturnsOriginalText()
    {
        var settings = CreateSut();
        string originalText = "This is a regular message without any commands.";

        var result = settings.ProcessColorCommands(originalText);

        result.ShouldBe(originalText);
    }
}
