namespace WinChat.Infrastructure.Contracts;

public class Content
{
    public List<Part> Parts { get; set; } = [];
    public string? Role { get; set; }
}
