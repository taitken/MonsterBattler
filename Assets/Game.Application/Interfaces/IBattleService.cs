
using System.Threading;
using System.Threading.Tasks;

namespace Game.Application.Interfaces
{
    public interface IBattleService
    {
        Task RunBattleAsync(CancellationToken ct);
    }
}
