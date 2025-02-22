namespace WinChat.Infrastructure.Events;

public enum ColorType
{
    BackgroundColor, ForegroundColor, AssistantChatColor, UserChatColor
}
public class ColorChangeRequestEvent
{
    public ColorType ColorType { get; set; }
    public string RgbColor { get; set; }
}
