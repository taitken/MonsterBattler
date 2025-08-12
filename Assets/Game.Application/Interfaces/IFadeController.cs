
using System.Threading;
using System.Threading.Tasks;

namespace Game.Application.Interfaces
{
    public interface IFadeController
    {
        // Optional fade controller the service can use if present.
        Task FadeOut(float duration = 0.25f, CancellationToken ct = default);
        Task FadeIn(float duration = 0.25f, CancellationToken ct = default);
    }

}
