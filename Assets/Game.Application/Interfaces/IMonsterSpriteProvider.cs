using Game.Domain.Enums;
using System.Threading.Tasks;

namespace Game.Application.Interfaces
{
    public interface IMonsterSpriteProvider
    {
        Task<T> GetMonsterSpriteAsync<T>(MonsterType type) where T : class;
    }
}