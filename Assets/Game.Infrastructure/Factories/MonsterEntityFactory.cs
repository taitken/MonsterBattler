using Game.Application.IFactories;
using Game.Core.Logger;
using Game.Domain.Entities;
using UnityEngine;

namespace Game.Infrastructure.Spawning
{
    public class MonsterEntityFactory : IMonsterEntityFactory
    {
        private readonly ILoggerService _loggerService;
        public MonsterEntityFactory(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }
        public MonsterEntity Create(MonsterType type)
        {
            _loggerService.Log($"Creating MonsterEntity of type: {type}");
            var definition = Resources.Load<MonsterDefinition>($"Monsters/Definitions/{type}");
            _loggerService.Log($"Definition file: {(definition == null ? "not found" : "found")}");
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