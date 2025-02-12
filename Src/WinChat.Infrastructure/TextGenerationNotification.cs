namespace WinChat.Infrastructure;

public class TextGenerationNotification
{
    public string? Text { get; set; }
    public string? Error { get; set; }
    public Exception? Exception { get; set; }
}
