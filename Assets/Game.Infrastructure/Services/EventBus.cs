using System;
using System.Collections.Generic;
using Game.Application.Messaging;
using Game.Domain.Messaging;
using Game.Applcation.HelperClasses;
using Game.Core;
using Game.Infrastructure.Messaging;

namespace Game.Infrastructure.Services
{
    public class EventBus : IEventBus
    {
        // Subscribers keyed by (payload type, topic)
        private readonly Dictionary<(Type, Topic), List<Delegate>> _subs = new();

        // Next-frame FIFO
        private readonly Queue<Action> _nextFrame = new();

        // Simple priority queue for queued/visual sequencing (lower number = higher priority)
        private readonly SortedList<int, Queue<Action>> _queued = new();

        public void Publish<T>(T message, PublishOptions? options = null) where T : IMessage
        {
            var t = typeof(T);
            var topic = options?.Topic ?? BusDefaults.ResolveTopic(t);
            var dispatch = options?.Dispatch ?? BusDefaults.ResolveDefaultDispatch(t);
            var priority = options?.Priority ?? BusDefaults.ResolveDefaultPriority(t);
            var corrId = options?.CorrelationId ?? Guid.NewGuid().ToString("N");

            var env = new Envelope(message!, topic, dispatch, priority, corrId, DateTime.UtcNow);

            PublishInternal<T>(env);
        }

        private void PublishInternal<T>(Envelope e)
        {
            var key = (typeof(T), e.Topic);
            if (!_subs.TryGetValue(key, out var handlers)) return;

            switch (e.Dispatch)
            {
                case Dispatch.Immediate:
                    for (int i = 0; i < handlers.Count; i++)
                        ((Action<T>)handlers[i]).Invoke((T)e.Payload);
                    break;

                case Dispatch.NextFrame:
                    for (int i = 0; i < handlers.Count; i++)
                    {
                        var h = (Action<T>)handlers[i];
                        _nextFrame.Enqueue(() => h((T)e.Payload));
                    }
                    break;

                case Dispatch.Queued:
                    if (!_queued.TryGetValue(e.Priority, out var q))
                        _queued[e.Priority] = q = new Queue<Action>();
                    for (int i = 0; i < handlers.Count; i++)
                    {
                        var h = (Action<T>)handlers[i];
                        q.Enqueue(() => h((T)e.Payload));
                    }
                    break;
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler, Topic? topic = null) where T : IMessage
        {
            var resolved = topic ?? BusDefaults.ResolveTopic(typeof(T));
            return SubscribeInternal<T>(resolved, o => handler((T)o));
        }

        private IDisposable SubscribeInternal<T>(Topic topic, Action<T> handler)
        {
            var key = (typeof(T), topic);
            if (!_subs.TryGetValue(key, out var list))
                _subs[key] = list = new List<Delegate>();
            list.Add(handler);

            return new Subscription(() => list.Remove(handler));
        }

        public void DrainPending(int maxPerFrame = 256)
        {
            int processed = 0;

            // Drain NextFrame FIFO
            while (_nextFrame.Count > 0 && processed < maxPerFrame)
            {
                _nextFrame.Dequeue().Invoke();
                processed++;
            }

            // Drain one queued item at a time by priority (good for sequencing visuals)
            if (processed < maxPerFrame && _queued.Count > 0)
            {
                // lowest key = highest priority
                int prio = _queued.Keys[0];
                var q = _queued.Values[0];
                if (q.Count > 0) { q.Dequeue().Invoke(); processed++; }
                if (q.Count == 0) _queued.RemoveAt(0);
            }
        }
    }
}