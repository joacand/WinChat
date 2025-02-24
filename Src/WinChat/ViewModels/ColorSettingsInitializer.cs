using Microsoft.Extensions.Hosting;

namespace WinChat.ViewModels;

public class ColorSettingsInitializer(ColorSettings colorSettings) : IHostedService
{
    private readonly ColorSettings colorSettings = colorSettings;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await colorSettings.LoadColorsAtStartup();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
