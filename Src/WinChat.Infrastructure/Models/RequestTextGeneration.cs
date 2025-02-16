namespace WinChat.Infrastructure.Models;

public sealed  class RequestTextGeneration
{
    public string Prompt { get; set; } = string.Empty;
    public string? SystemPrompt { get; set; }
}
