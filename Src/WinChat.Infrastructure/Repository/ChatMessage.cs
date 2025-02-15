namespace WinChat.Infrastructure.Repository;

public class ChatMessage : ChatMessageContent
{
    public int Id { get; set; }
}
public class ChatMessageContent
{
    public string? Role { get; set; }
    public string? Content { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}