using Microsoft.Extensions.AI;
using System.Text.Json;

namespace WinChat.Infrastructure.Services.Gemini;

/// <summary>
/// Implements the Microsoft.Extensions.AI interfaces with Gemini - only partly implemented.
/// </summary>
internal sealed class GeminiChatClient(IGeminiService geminiService) : IChatClient
{
    private JsonSerializerOptions _toolCallJsonSerializerOptions = AIJsonUtilities.DefaultOptions;
    public JsonSerializerOptions ToolCallJsonSerializerOptions
    {
        get => _toolCallJsonSerializerOptions;
        set => _toolCallJsonSerializerOptions = value;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatMessages);

        var geminiAIOptions = GeminiMapper.ToGeminiOptions(options);
        var geminiChatMessages = GeminiMapper.ToGeminiMessages(chatMessages, geminiAIOptions, ToolCallJsonSerializerOptions);

        // Make the call to OpenAI.
        var response = await geminiService.GenerateText(geminiChatMessages,  cancellationToken).ConfigureAwait(false);

        return GeminiMapper.FromGeminiResponse(response, options, geminiAIOptions);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceKey);

        return
            serviceKey is not null ? null :
            serviceType == typeof(ChatClientMetadata) ? null :
            serviceType.IsInstanceOfType(this) ? this :
            null;
    }

    public void Dispose() { }
}
