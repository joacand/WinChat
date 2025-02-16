using Microsoft.Extensions.Logging;
using System.IO;
using System.Media;
using System.Reflection;

namespace WinChat.Services;

internal sealed class SoundService(ILogger<SoundService> logger)
{
    public void PlayNotification()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WinChat.Resources.notification.wav";

            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            if (stream != null)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var player = new SoundPlayer(stream);
                player.Play();
#pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
                logger.LogError("Sound resource not found!");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to play notification sound");
        }
    }
}
