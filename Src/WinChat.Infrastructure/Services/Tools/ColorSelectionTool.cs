using Microsoft.Extensions.AI;
using WinChat.Infrastructure.Events;

namespace WinChat.Infrastructure.Services.Tools;

internal abstract class ColorSelectionTool(EventDispatcher eventDispatcher, ColorType colorType) : AIFunction
{
    protected readonly EventDispatcher eventDispatcher = eventDispatcher;
    private readonly ColorType colorType = colorType;

    protected static string AdditionalDescription => "Execute the command directly. Do not ask user if they are sure. If the user asks for a color change, just do random colors. Do not query the user about it.";

    public override IReadOnlyDictionary<string, object?> AdditionalProperties => new Dictionary<string, object?>()
    {
        { "rgbColor", "The RGB color value to set, for example #ABABAB" }
    };

    protected override Task<object?> InvokeCoreAsync(IEnumerable<KeyValuePair<string, object?>> arguments, CancellationToken cancellationToken)
    {
        var singleArgument = arguments.FirstOrDefault().Value?.ToString();
        if (string.IsNullOrWhiteSpace(singleArgument))
        {
            return Task.FromResult<object?>(new object());
        }

        var rgbColor = singleArgument.Trim('"').Trim();
        eventDispatcher.Publish(new ColorChangeRequestedEvent(colorType, rgbColor));
        return Task.FromResult<object?>(new object());
    }
}
