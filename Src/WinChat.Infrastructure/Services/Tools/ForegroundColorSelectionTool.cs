using WinChat.Infrastructure.Events;

namespace WinChat.Infrastructure.Services.Tools;

internal sealed class ForegroundColorSelectionTool(EventDispatcher eventDispatcher) : ColorSelectionTool(eventDispatcher, ColorType.ForegroundColor)
{
    public override string Name => "select_application_foreground_text_color";
    public override string Description => "Tool for changing the foreground text color for the whole application" + AdditionalDescription;
}
