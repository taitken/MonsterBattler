
using System.Threading;
using System.Threading.Tasks;

namespace Game.Application.Interfaces
{
    public interface ISceneConductorService
    {
        Task LoadSingle(string sceneName, bool withFade = true, CancellationToken ct = default);
        Task LoadAdditive(string sceneName, bool activateOnLoad = true, CancellationToken ct = default);
        Task Unload(string sceneName, CancellationToken ct = default);
        Task Reload(bool withFade = true, CancellationToken ct = default);
    }
}
