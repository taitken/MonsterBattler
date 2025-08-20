
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Game.Application.DTOs;
using Game.Application.Interfaces;

namespace Game.Presentation.Core.Services
{
    public sealed class InteractionBarrier : IInteractionBarrier
    {
        private readonly ConcurrentDictionary<BarrierKey, TaskCompletionSource<bool>> _pending =
            new ConcurrentDictionary<BarrierKey, TaskCompletionSource<bool>>();

        public Task WaitAsync(BarrierKey token, CancellationToken ct = default)
        {
            var tcs = _pending.GetOrAdd(token, _ => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously));

            if (ct.CanBeCanceled)
            {
                ct.Register(() =>
                {
                    if (_pending.TryRemove(token, out var cts))
                        cts.TrySetCanceled(ct);
                });
            }

            return tcs.Task;
        }

        public void Signal(BarrierKey token)
        {
            if (_pending.TryRemove(token, out var tcs))
                tcs.TrySetResult(true);
        }

        public bool TryCancel(BarrierKey token)
        {
            if (_pending.TryRemove(token, out var tcs))
                return tcs.TrySetCanceled();
            return false;
        }
    }
}