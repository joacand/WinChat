namespace WinChat.Infrastructure.Events;

public sealed class EventDispatcher
{
    private readonly List<object> _registeredHandlers = [];

    public void Register<T>(IEventHandler<T> handler)
    {
        _registeredHandlers.Add(handler);
    }

    public void Publish<T>(T @event)
    {
        foreach (var handler in _registeredHandlers.OfType<IEventHandler<T>>())
        {
            handler.Handle(@event);
        }
    }
}
