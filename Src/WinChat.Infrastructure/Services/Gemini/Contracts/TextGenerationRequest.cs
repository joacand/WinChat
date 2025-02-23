using System.Text.Json.Serialization;

namespace WinChat.Infrastructure.Services.Gemini.Contracts;

public class TextGenerationRequest
{
    [JsonPropertyName("system_instruction")]
    public SystemInstruction? SystemInstruction { get; set; }
    public Contents? Contents { get; set; }
    public List<GeminiTool>? Tools { get; set; }
}

public class GeminiTool
{
    [JsonPropertyName("function_declarations")]
    public List<GeminiFunctionDecleration>? FunctionDecleration { get; set; }
}
public class GeminiFunctionDecleration
{
    public string Name { get; set; }
    public string Description { get; set; }
    public GeminiParameters Parameters { get; set; }
}

public class GeminiParameters
{
    public string Type => "object";
    public Dictionary<string, InternalParameter> Properties { get; set; } = new();
    public List<string> Required { get; set; }
}

public class InternalParameter
{
    public string Type => "string";
    public string Description { get; set; }
}