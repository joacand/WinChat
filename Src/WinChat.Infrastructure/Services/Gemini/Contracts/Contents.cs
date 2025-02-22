namespace WinChat.Infrastructure.Services.Gemini.Contracts;

public class Contents
{
    public string Role { get; set; } = string.Empty;
    public Parts Parts { get; set; } = new();
}
