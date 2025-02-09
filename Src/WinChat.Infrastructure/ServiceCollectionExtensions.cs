using Microsoft.Extensions.DependencyInjection;
using WinChat.Infrastructure.Repository;

namespace WinChat.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void RegisterInfrastructureServices(IServiceCollection services)
    {
        services.AddTransient<AiPromptService>();
    }
}
