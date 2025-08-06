using Game.Application.IFactories;
using Game.Domain.Entities;
using UnityEngine;

namespace Game.Infrastructure.Spawning
{
    public class MonsterSpawner : IMonsterSpawner
    {
        public MonsterEntity SpawnMonster(MonsterType type)
        {
            var definition = Resources.Load<MonsterDefinition>($"Monsters/{type}");
            var model = new MonsterEntity(
                maxHealth: definition.maxHealth,
                attackDamage: definition.attackDamage,
                type: definition.type,
                monsterName: definition.monsterName,
                attackDirection: definition.attackDirection
            );

            return model;
        }
    }
}