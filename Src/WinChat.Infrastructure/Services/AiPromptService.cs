using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Channels;
using WinChat.Infrastructure.Models;
using WinChat.Infrastructure.Repository;
using WinChat.Infrastructure.Services.Contracts;

namespace WinChat.Infrastructure.Services;

internal sealed class AiPromptService(
    IGenerateTextService generateTextService,
    IServiceScopeFactory scopeFactory,
    Channel<TextGenerationNotification> textGenerationNotificationChannel,
    Channel<RequestTextGeneration> textGenerationRequestChannel,
    ILogger<AiPromptService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                await ListenForMessages();
                await Task.Delay(200, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Channel failed");
            }
        }
    }

    private async Task ListenForMessages()
    {
        await foreach (var message in textGenerationRequestChannel.Reader.ReadAllAsync())
        {
            if (string.IsNullOrWhiteSpace(message.Prompt))
            {
                await textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Prompt cannot be empty" });
            }

            await GenerateTextTranslation(message.Prompt, message.SystemPrompt);
        }
    }

    public async Task GenerateTextTranslation(string prompt, string? systemPrompt = default)
    {
        try
        {

            if (string.IsNullOrWhiteSpace(prompt))
            {
                await textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Prompt cannot be empty" });
                return;
            }

            var chatHistory = GetChatHistorySample();

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

            TextGenerationResponse? textGenerationResponse = null;
            try
            {
                textGenerationResponse = await generateTextService.GenerateText(textGenerationRequest);
            }
            catch (Exception ex)
            {
                await textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Failed to get text generation response", Exception = ex });
                return;
            }

            var parts = textGenerationResponse?.Candidates?.FirstOrDefault()?.Content?.Parts;
            if (parts != null && parts.Count > 0)
            {
                var result = string.Join(Environment.NewLine, parts.Select(x => x.Text));
                await textGenerationNotificationChannel.Writer.WriteAsync(new() { Text = result });
            }
            else
            {
                await textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Empty response" });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get text generation response");
            await textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Failed to get text generation response", Exception = ex });
        }
    }

    /// <summary>
    /// Returns the chat history - the latest messages and a sample of older messages
    /// </summary>
    private List<string> GetChatHistorySample()
    {
        using var scope = scopeFactory.CreateScope();
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
            return [.. recentMessages.Select(FormatChatMessage)];
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

    private static string CreateFullPrompt(string prompt, List<string> chatHistory) =>
        Constants.CommandsPrompt + Environment.NewLine + string.Join(Environment.NewLine, chatHistory) + Environment.NewLine + $"New user message: {prompt}";

    private string FormatChatMessage(ChatMessageContent message, int _) => JsonSerializer.Serialize(message, Constants.DefaultSerializerOptions);
}
