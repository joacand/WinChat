﻿using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;
using WinChat.Infrastructure.Events;
using WinChat.Infrastructure.Hosting;
using WinChat.Infrastructure.Models;
using WinChat.Infrastructure.Services;
using WinChat.Infrastructure.Services.Gemini;

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
        services.AddSingleton<GeminiService>();
        services.AddSingleton<IGeminiService>(services => services.GetRequiredService<GeminiService>());
        services.AddSingleton<IApiTokenConfiguration>(services => services.GetRequiredService<GeminiService>());
        services.AddSingleton<GeminiChatClient>();
        services.AddSingleton<IChatClient>(scope => scope.GetRequiredService<GeminiChatClient>());
        services.AddHostedService<AiPromptService>();
        services.AddHostedService<InitiateConversationBackgroundService>();
        services.AddSingleton<EventDispatcher>();
    }
}
