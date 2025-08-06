using Game.Domain.Entities;

namespace Game.Application.IFactories
{
    public interface IMonsterSpawner
    {
        MonsterEntity SpawnMonster(MonsterType type);
    }
}