using System;
using System.Collections.Generic;
using Game.Core.Events;

namespace Game.Infrastructure.Services
{
    public class EventQueueService : IEventQueueService
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Publish<TEvent>(TEvent evt)
        {
            var type = typeof(TEvent);
            if (_subscribers.TryGetValue(type, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    ((Action<TEvent>)handler).Invoke(evt);
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);
            if (!_subscribers.TryGetValue(type, out var handlers))
            {
                handlers = new List<Delegate>();
                _subscribers[type] = handlers;
            }

            handlers.Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);
            if (_subscribers.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }
}