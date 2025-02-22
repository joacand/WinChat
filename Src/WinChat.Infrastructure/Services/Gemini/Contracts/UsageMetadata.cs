namespace WinChat.Infrastructure.Services.Gemini.Contracts;

public class UsageMetadata
{
    public int? PromptTokenCount { get; set; }
    public int? CandidatesTokenCount { get; set; }
    public int? TotalTokenCount { get; set; }
    public List<PromptTokensDetail> PromptTokensDetails { get; set; } = [];
    public List<CandidatesTokensDetail> CandidatesTokensDetails { get; set; } = [];
}
