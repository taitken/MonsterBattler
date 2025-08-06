using Game.Domain.Entities;

namespace Game.Application.IFactories
{
    public interface IMonsterEntityFactory
    {
        MonsterEntity Create(MonsterType type);
    }
}