using System.Text.Json.Serialization;

namespace WinChat.Infrastructure.Services.Gemini.Contracts;

public class TextGenerationRequest
{
    [JsonPropertyName("system_instruction")]
    public SystemInstruction? SystemInstruction { get; set; }
    public Contents? Contents { get; set; }
}
