﻿namespace WinChat.Infrastructure.Services.Gemini.Contracts;

public class Candidate
{
    public Content? Content { get; set; }
    public string? FinishReason { get; set; }
    public double? AvgLogprobs { get; set; }
}
