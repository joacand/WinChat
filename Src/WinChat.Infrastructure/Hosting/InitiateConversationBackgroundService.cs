using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using WinChat.Infrastructure.Models;
using WinChat.Infrastructure.Repository;

namespace WinChat.Infrastructure.Hosting;

internal sealed class InitiateConversationBackgroundService(
    IServiceScopeFactory scopeFactory,
    Channel<RequestTextGeneration> textGenerationRequestChannel) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly Channel<RequestTextGeneration> _textGenerationRequestChannel = textGenerationRequestChannel;
    private static Random Random { get; } = new();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(Random.Next(5000, 10000), cancellationToken);

            using var scope = _scopeFactory.CreateScope();
            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var chatHistory = appDbContext.ChatMessages
                .OrderByDescending(x => x.TimeStamp)
                .FirstOrDefault();

            if (chatHistory != null)
            {
                var tooShortTimeForLastMessage = (DateTime.UtcNow - chatHistory.TimeStamp) < TimeSpan.FromMinutes(2);

                if (tooShortTimeForLastMessage)
                {
                    // Possible improvement - wait remaining time instead of just skippings
                    continue;
                }
            }

            await _textGenerationRequestChannel.Writer.WriteAsync(new RequestTextGeneration
            {
                Prompt = Constants.InitiateConversationPrompt,
                SystemPrompt = Constants.InitiateConversationSystemPrompt
            }, cancellationToken);
        }
    }
}
