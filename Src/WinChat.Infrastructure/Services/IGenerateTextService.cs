using WinChat.Infrastructure.Services.Contracts;

namespace WinChat.Infrastructure.Services;

public interface IGenerateTextService
{
    Task<TextGenerationResponse> GenerateText(TextGenerationRequest textGenerationRequest);
    Task SetApiToken(string apiToken);
}
