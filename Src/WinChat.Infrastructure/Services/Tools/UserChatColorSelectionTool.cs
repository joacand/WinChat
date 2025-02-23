using WinChat.Infrastructure.Events;

namespace WinChat.Infrastructure.Services.Tools;

internal class UserChatColorSelectionTool(EventDispatcher eventDispatcher) : ColorSelectionTool(eventDispatcher, ColorType.UserChatColor)
{
    public override string Name => "select_user_chat_background_color";
    public override string Description => "Tool for changing the background color of the user chat boxes";
}
