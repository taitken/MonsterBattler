using Game.Domain.Entities;
using Game.Domain.Enums;

namespace Game.Application.IFactories
{
    public interface IMonsterEntityFactory
    {
        MonsterEntity Create(MonsterType type);
    }
}