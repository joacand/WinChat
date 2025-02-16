using WinChat.Infrastructure.Services.Contracts;

namespace WinChat.Infrastructure.Services;

internal interface IGenerateTextService
{
    Task<TextGenerationResponse> GenerateText(TextGenerationRequest textGenerationRequest);
}
