
using System;
using Game.Core;
using Game.Domain.Messaging;

namespace Game.Application.Messaging
{
    public interface IEventBus
    {
        void Publish<T>(T message, PublishOptions? options = null) where T : IMessage;
        IDisposable Subscribe<T>(Action<T> handler, Topic? topic = null) where T : IMessage;
        void DrainPending(int maxPerFrame = 256);
    }
}