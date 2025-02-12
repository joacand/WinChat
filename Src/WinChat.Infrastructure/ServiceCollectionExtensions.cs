using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;
using WinChat.Infrastructure.Hosting;

namespace WinChat.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void RegisterInfrastructureServices(IServiceCollection services)
    {
        var newTextGenerationNotificationChannel = Channel.CreateUnbounded<TextGenerationNotification>();
        var requestTextGenerationChannel = Channel.CreateUnbounded<RequestTextGeneration>();
        services.AddSingleton(newTextGenerationNotificationChannel);
        services.AddSingleton(requestTextGenerationChannel);
        services.AddSingleton<AiPromptService>();
        services.AddSingleton<InitiateConversationBackgroundService>();
        services.AddHostedService<AiPromptService>();
        services.AddHostedService<InitiateConversationBackgroundService>();
    }
}
