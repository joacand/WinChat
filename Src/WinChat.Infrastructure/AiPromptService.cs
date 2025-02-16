﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using WinChat.Infrastructure.Contracts;
using WinChat.Infrastructure.Repository;

namespace WinChat.Infrastructure;

public sealed class AiPromptService : BackgroundService
{
    private static readonly HttpClient _httpClient = new();
    private string ApiKey = string.Empty;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Channel<TextGenerationNotification> _textGenerationNotificationChannel;
    private readonly Channel<RequestTextGeneration> _textGenerationRequestChannel;
    private readonly ILogger<AiPromptService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AiPromptService(
        IServiceScopeFactory scopeFactory,
        Channel<TextGenerationNotification> textGenerationNotificationChannel,
        Channel<RequestTextGeneration> textGenerationRequestChannel,
        ILogger<AiPromptService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _textGenerationNotificationChannel = textGenerationNotificationChannel;
        _textGenerationRequestChannel = textGenerationRequestChannel;
        _logger = logger ?? NullLogger<AiPromptService>.Instance;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ListenForMessages();
    }

    private async Task ListenForMessages()
    {
        while (true)
        {
            try
            {
                await foreach (var message in _textGenerationRequestChannel.Reader.ReadAllAsync())
                {
                    if (string.IsNullOrWhiteSpace(message.Prompt))
                    {
                        await _textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Prompt cannot be empty" });
                    }

                    await GenerateTextTranslation(message.Prompt, message.SystemPrompt);
                }
                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Channel failed");
            }
        }
    }

    public async Task<bool> GenerateTextTranslation(string prompt, string? systemPrompt = default)
    {
        try
        {
            var apiKey = await GetApiKey();
            if (apiKey == null)
            {
                await _textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "No API key specified" });
                return false;
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                await _textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Prompt cannot be empty" });
                return false;
            }

            var chatHistory = GetChatHistorySample();

            var address = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={ApiKey}";

            var textGenerationRequest = new TextGenerationRequest
            {
                Contents = new Contents
                {
                    Parts = new Parts
                    {
                        Text = CreateFullPrompt(prompt, chatHistory)
                    }
                }
            };
            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                textGenerationRequest.SystemInstruction = new SystemInstruction
                {
                    Parts = new Parts
                    {
                        Text = systemPrompt
                    }
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, address)
            {
                Content = new StringContent(JsonSerializer.Serialize(
                    textGenerationRequest, SerializerOptions), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            TextGenerationResponse? textGenerationResponse = null;
            if (await TryDeserialize<TextGenerationResponse>(response, r => textGenerationResponse = r))
            {
                var parts = textGenerationResponse?.Candidates?.FirstOrDefault()?.Content?.Parts;
                if (parts != null && parts.Count > 0)
                {
                    var result = string.Join(Environment.NewLine, parts.Select(x => x.Text));
                    await _textGenerationNotificationChannel.Writer.WriteAsync(new() { Text = result });
                    return true;
                }
            }
            await _textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Empty response" });
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get text generation response");
            await _textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Failed to get text generation response", Exception = ex });
            return false;
        }
    }

    /// <summary>
    /// Returns the chat history - the latest messages and a sample of older messages
    /// </summary>
    private List<string> GetChatHistorySample()
    {
        using var scope = _scopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const int MaxHistoryLimit = 500;
        const int RecentMessages = 30;

        var recentMessages = appDbContext.ChatMessages
            .OrderByDescending(x => x.TimeStamp)
            .Take(RecentMessages)
            .ToList();

        recentMessages.Reverse();

        var oldestRecent = recentMessages.FirstOrDefault()?.TimeStamp;
        if (oldestRecent == null)
        {
            return recentMessages.Select(FormatChatMessage).ToList();
        }

        var olderCandidates = appDbContext.ChatMessages
            .Where(x => x.TimeStamp < oldestRecent)
            .OrderByDescending(x => x.TimeStamp)
            .Take(MaxHistoryLimit)
            .ToList();

        olderCandidates.Reverse();

        var gap = 1.0;
        var sampledOlder = new List<ChatMessage>();
        for (var i = 0; i < olderCandidates.Count;)
        {
            sampledOlder.Add(olderCandidates[i]);
            i += (int)Math.Ceiling(gap);
            gap *= 1.5;
        }

        var finalChatHistory = sampledOlder
            .Concat(recentMessages)
            .Select(FormatChatMessage)
            .ToList();

        return finalChatHistory;
    }

    private static string CreateFullPrompt(string prompt, List<string> chatHistory)
    {
        return Constants.CommandsPrompt + Environment.NewLine + string.Join(Environment.NewLine, chatHistory) + Environment.NewLine + $"New user message: {prompt}";
    }

    private string FormatChatMessage(ChatMessageContent message, int _)
    {
        return JsonSerializer.Serialize(message, SerializerOptions);
    }

    private async Task<bool> TryDeserialize<T>(HttpResponseMessage response, Action<T> resultCallback)
    {
        try
        {
            var stringContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(stringContent, SerializerOptions);
            if (result != null)
            {
                resultCallback.Invoke(result);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize response");
        }
        return false;
    }

    public async Task AddApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        appDbContext.ApplicationData.Add(new ApplicationData
        {
            SettingKey = Constants.ApplicationDataKeys.ApiKey,
            SettingValue = apiKey
        });
        await appDbContext.SaveChangesAsync();
    }

    private async Task<string?> GetApiKey()
    {
        if (!string.IsNullOrWhiteSpace(ApiKey)) { return ApiKey; }

        using var scope = _scopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dbResponse = await appDbContext.ApplicationData.FindAsync(Constants.ApplicationDataKeys.ApiKey);
        if (string.IsNullOrWhiteSpace(dbResponse?.SettingValue))
        {
            return null;
        }
        ApiKey = dbResponse.SettingValue;

        return ApiKey;
    }
}
