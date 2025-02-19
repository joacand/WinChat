namespace WinChat.Infrastructure.Events;

public interface IEventHandler<T>
{
    Task Handle(T @event);
}
