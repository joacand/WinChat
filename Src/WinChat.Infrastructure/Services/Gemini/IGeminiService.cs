using WinChat.Infrastructure.Services.Gemini.Contracts;

namespace WinChat.Infrastructure.Services.Gemini;

public interface IGeminiService
{
    Task<TextGenerationResponse> GenerateText(TextGenerationRequest textGenerationRequest, CancellationToken cancellationToken);
}
