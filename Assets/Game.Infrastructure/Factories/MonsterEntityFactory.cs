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
        private readonly IAbilityCardFactory _abilityCardFactory;
        
        public MonsterEntityFactory(ILoggerService loggerService, IAbilityCardFactory abilityCardFactory = null)
        {
            _loggerService = loggerService;
            _abilityCardFactory = abilityCardFactory;
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
            
            if (string.IsNullOrWhiteSpace(definition.monsterName))
            {
                throw new System.InvalidOperationException(
                    $"Invalid monsterName for monster {type}. Name cannot be null or empty.");
            }
            
            
            // Create starter deck if ability card factory is available
            var starterDeck = _abilityCardFactory?.CreateStarterDeckForMonsterEntity(type);
            
            var model = new MonsterEntity(
                maxHealth: definition.maxHealth,
                attackDamage: definition.attackDamage,
                type: definition.type,
                monsterName: definition.monsterName,
                attackDirection: definition.attackDirection,
                abilityDeck: starterDeck
            );
            
            _loggerService?.Log($"Successfully created MonsterEntity: {definition.monsterName}");

            return model;
        }
    }
}