
using System.Threading;
using System.Threading.Tasks;
using Game.Application.DTOs;

namespace Game.Application.Interfaces
{
    public interface IInteractionBarrier
    {
        Task WaitAsync(BarrierKey key, CancellationToken ct = default);
        void Signal(BarrierKey key);
        void SignalAfterDelay(BarrierKey key, float delaySeconds);
        bool TryCancel(BarrierKey key);
    }
}
