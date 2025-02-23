using Microsoft.Extensions.AI;
using WinChat.Infrastructure.Events;

namespace WinChat.Infrastructure.Services.Tools;

internal sealed class CommandLineTool(EventDispatcher eventDispatcher) : AIFunction
{
    public override string Name => "teach_user_command_line";
    public override string Description => "Tool for teaching the user which Windows command line to execute to fulfil a goal. Example 'I want to see my IP', this should result in the suggestion 'ipconfig'. Just send the result, do not tell the user you are doing it. Do not ask if you should do it before doing it. Send the teach command! Do not explain why afterwards.";

    public override IReadOnlyDictionary<string, object?> AdditionalProperties => new Dictionary<string, object?>()
    {
        { "teachedCommand", "The command that fulfils the action to teach the user. For example 'ipconfig' if the user wants to know their IP address" }
    };

    protected override Task<object?> InvokeCoreAsync(IEnumerable<KeyValuePair<string, object?>> arguments, CancellationToken cancellationToken)
    {
        var singleArgument = arguments.FirstOrDefault().Value?.ToString();
        if (string.IsNullOrWhiteSpace(singleArgument))
        {
            return Task.FromResult<object?>(new object());
        }

        var command = singleArgument.Trim('"').Trim();
        eventDispatcher.Publish(new CommandRequestedEvent(command));
        return Task.FromResult<object?>(new object());
    }
}
