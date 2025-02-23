namespace WinChat.Infrastructure.Services.Gemini.Contracts;

public class Part
{
    public string? Text { get; set; }
    public GeminiFunctionCall? FunctionCall { get; set; }
}

public class GeminiFunctionCall
{
    public string Name { get; set; }
    public Dictionary<string, object?> Args { get; set; }
}
