namespace WinChat.Infrastructure.Services.Gemini.Contracts;

public class Content
{
    public List<Part> Parts { get; set; } = [];
    public string? Role { get; set; }
}
