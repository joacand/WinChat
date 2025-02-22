using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Channels;
using WinChat.Infrastructure.Models;
using WinChat.Infrastructure.Repository;

namespace WinChat.Infrastructure.Services;

internal sealed class AiPromptService(
    IChatClient chatClient,
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

            var messages = new List<ChatMessage>();
            AddChatHistory(messages, chatHistory);

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                messages.Add(new()
                {
                    Role = ChatRole.System,
                    Text = systemPrompt
                });
            }

            messages.Add(
                new()
                {
                    Role = ChatRole.User,
                    Text = CreateFullPrompt(prompt)
                });

            ChatResponse? chatResponse = null;
            try
            {
                chatResponse = await chatClient.GetResponseAsync(messages);
            }
            catch (Exception ex)
            {
                await textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = "Failed to get text generation response", Exception = ex });
                return;
            }

            if (chatResponse.Message.Contents.Count > 0)
            {
                var result = string.Join(Environment.NewLine, chatResponse.Message.Contents.Select(x => x));
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

    private static void AddChatHistory(List<ChatMessage> messages, List<string> chatHistory)
    {
        var deserialized = chatHistory.Select(x => JsonSerializer.Deserialize<ChatMessageContent>(x, Constants.DefaultSerializerOptions));
        foreach (var entry in deserialized)
        {
            messages.Add(new()
            {
                Role = entry.Role == "Assistant" ? ChatRole.Assistant : ChatRole.User,
                Text = $"{entry.TimeStamp:yyyy-MM-dd HH:mm:ss}: {entry.Content}",
                AuthorName = entry.Role
            });
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
        var sampledOlder = new List<ChatMessageEntry>();
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

    private static string CreateFullPrompt(string prompt) =>
        Constants.CommandsPrompt + Environment.NewLine + Environment.NewLine + $"New user message: {prompt}";

    private string FormatChatMessage(ChatMessageContent message, int _) => JsonSerializer.Serialize(message, Constants.DefaultSerializerOptions);
}
