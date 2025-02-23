using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using WinChat.Infrastructure.Events;
using WinChat.Infrastructure.Models;
using WinChat.Infrastructure.Services;
using WinChat.Views;

namespace WinChat.Services;

internal sealed class CommandLineService : IEventHandler<CommandRequestedEvent>, IHostedService
{
    private readonly Channel<TextGenerationNotification> textGenerationNotificationChannel;

    public CommandLineService(
        EventDispatcher eventDispatcher,
        Channel<TextGenerationNotification> textGenerationNotificationChannel)
    {
        eventDispatcher.Register(this);
        this.textGenerationNotificationChannel = textGenerationNotificationChannel;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task Handle(CommandRequestedEvent @event)
    {
        try
        {
            var result = ProcessCommands(@event.Command);
            await textGenerationNotificationChannel.Writer.WriteAsync(new() { Text = result });
        }
        catch (Exception ex)
        {
            await textGenerationNotificationChannel.Writer.WriteAsync(new() { Error = ex.Message, Exception = ex });
        }
    }

    public static string ProcessCommands(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return "⚠️ Received empty command ⚠️";
        }

        var userResult = ConfirmationDialog.Show($"The assistant wants to send the following command, do you want to proceed?",
            command, "Confirmation");

        if (userResult != true)
        {
            return $"⚠️ Command '{command}' aborted by user ⚠️";
        }

        var commandResult = CommandSenderService.SendCmd(command);
        return CommandSenderService.FormatProcessResult(commandResult, command);
    }
}
