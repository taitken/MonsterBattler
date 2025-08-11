using Game.Application.Messaging;
using Game.Core;

public class EventBusRunner
{
    private IEventBus _bus;

    public EventBusRunner()
    {
        _bus = ServiceLocator.Get<IEventBus>();
    }
    public void Run(int maxPerFrame = 256) => _bus.DrainPending(maxPerFrame);
}