using System;

namespace Game.Infrastructure.Messaging
{
    public sealed class Subscription : IDisposable
    {
        private readonly Action _onDispose;
        private bool _disposed;
        public Subscription(Action onDispose) => _onDispose = onDispose;
        public void Dispose() { if (_disposed) return; _disposed = true; _onDispose(); }
    }
}