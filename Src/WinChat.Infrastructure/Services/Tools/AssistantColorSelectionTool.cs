using WinChat.Infrastructure.Events;

namespace WinChat.Infrastructure.Services.Tools;

internal class AssistantColorSelectionTool(EventDispatcher eventDispatcher) : ColorSelectionTool(eventDispatcher, ColorType.AssistantChatColor)
{
    public override string Name => "select_assistant_chat_background_color";
    public override string Description => "Tool for changing the background color of the assistant chat boxes";
}
