
using System;

namespace Game.Core.Events
{
    public interface IEventQueueService
    {
        void Publish<TEvent>(TEvent evt);
        void Subscribe<TEvent>(Action<TEvent> handler);
        void Unsubscribe<TEvent>(Action<TEvent> handler);
    }
}