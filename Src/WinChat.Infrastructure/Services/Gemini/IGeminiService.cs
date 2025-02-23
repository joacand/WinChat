using WinChat.Infrastructure.Services.Gemini.Contracts;

namespace WinChat.Infrastructure.Services.Gemini;

internal interface IGeminiService
{
    Task<TextGenerationResponse> GenerateText(TextGenerationRequest textGenerationRequest, CancellationToken cancellationToken);
}
