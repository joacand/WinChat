namespace WinChat.Infrastructure.Events;

public enum ColorType { BackgroundColor, ForegroundColor, AssistantChatColor, UserChatColor }

public record ColorChangeRequestedEvent(ColorType ColorType, string RgbColor);
