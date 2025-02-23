using WinChat.Infrastructure.Events;

namespace WinChat.Infrastructure.Services.Tools;

internal sealed class BackgroundColorSelectionTool(EventDispatcher eventDispatcher) : ColorSelectionTool(eventDispatcher, ColorType.BackgroundColor)
{
    public override string Name => "select_application_background_color";
    public override string Description => "Tool for changing the background color for the whole application" + AdditionalDescription;
}
