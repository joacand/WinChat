namespace WinChat.Infrastructure.Repository;

public class ChatMessage
{
    public int Id { get; set; }
    public string? Role { get; set; }
    public string? Content { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}