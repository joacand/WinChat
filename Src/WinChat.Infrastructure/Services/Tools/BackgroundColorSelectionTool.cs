using Microsoft.Extensions.AI;
using WinChat.Infrastructure.Events;

namespace WinChat.Infrastructure.Services.Tools;

public class BackgroundColorSelectionTool(EventDispatcher eventDispatcher) : AIFunction
{
    public override string Name => "select_application_background_color";
    public override string Description => "Tool for changing the background color for the whole application";
    public override IReadOnlyDictionary<string, object?> AdditionalProperties => new Dictionary<string, object?>()
    {
        { "rgbColor", "The RGB color value to set the background to, for example #ABABAB" }
    };
    protected override Task<object?> InvokeCoreAsync(IEnumerable<KeyValuePair<string, object?>> arguments, CancellationToken cancellationToken)
    {
        eventDispatcher.Publish(new ColorChangeRequestEvent()
        {
            ColorType = ColorType.BackgroundColor,
            RgbColor = arguments.First().Value.ToString().Trim('"').Trim()
        });
        return Task.FromResult<object?>(new object());
    }
}
