using Game.Domain.Enums;
using System.Threading.Tasks;

namespace Game.Application.Interfaces
{
    public interface IBiomeBackgroundProvider
    {
        Task<T> GetBackgroundAsync<T>(Biome biome) where T : class;
        void ReleaseBackground(Biome biome);
        void ReleaseAllBackgrounds();
    }
}