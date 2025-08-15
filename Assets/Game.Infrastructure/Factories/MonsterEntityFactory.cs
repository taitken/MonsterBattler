using Game.Application.IFactories;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Infrastructure.ScriptableObjects;
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
            _loggerService?.Log($"Creating MonsterEntity of type: {type}");
            
            var definition = Resources.Load<MonsterDefinition>($"Monsters/Definitions/{type}");
            
            if (definition == null)
            {
                var errorMessage = $"MonsterDefinition not found for type: {type}. " +
                                  $"Expected file at: Resources/Monsters/Definitions/{type}";
                _loggerService?.LogError(errorMessage);
                throw new System.InvalidOperationException(errorMessage);
            }
            
            // Validate definition has required data
            if (definition.maxHealth <= 0)
            {
                throw new System.InvalidOperationException(
                    $"Invalid maxHealth ({definition.maxHealth}) for monster {type}. Health must be greater than 0.");
            }
            
            if (definition.attackDamage < 0)
            {
                throw new System.InvalidOperationException(
                    $"Invalid attackDamage ({definition.attackDamage}) for monster {type}. Damage cannot be negative.");
            }
            
            if (string.IsNullOrWhiteSpace(definition.monsterName))
            {
                throw new System.InvalidOperationException(
                    $"Invalid monsterName for monster {type}. Name cannot be null or empty.");
            }
            
            _loggerService?.Log($"Successfully created MonsterEntity: {definition.monsterName}");
            
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