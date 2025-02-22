using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using WinChat.Infrastructure.Services;
using WinChat.ViewModels;

namespace WinChat.Services
{
    public sealed class FunctionHandler(Channel<FunctionCallContent> functionChannel, ColorSettings colorSettings, ILogger<FunctionHandler> logger) : BackgroundService
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
            await foreach (var message in functionChannel.Reader.ReadAllAsync())
            {
                await Handle(message);
            }
        }

        private async Task Handle(FunctionCallContent message)
        {
            var backgroundTool = new BackgroundColorSelectionTool();

            if (message.Name.Equals(backgroundTool.Name, StringComparison.OrdinalIgnoreCase))
            {
                var args = message.Arguments;
                foreach(var arg in args)
                {
                    if (arg.Key.Equals(backgroundTool.AdditionalProperties.First().Key, StringComparison.OrdinalIgnoreCase))
                    {
                        var rgbValue = (arg.Value.ToString()).Trim('"').Trim();
                        colorSettings.BackgroundColorHex = rgbValue;
                    }
                }
            }
        }
    }
}
