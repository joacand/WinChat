﻿namespace WinChat.Infrastructure.Contracts;

public class TextGenerationResponse
{
    public List<Candidate> Candidates { get; set; } = [];
    public UsageMetadata? UsageMetadata { get; set; }
    public string? ModelVersion { get; set; }
}
