using System.Collections.Generic;
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
        public MonsterEntity Create(MonsterType type, BattleTeam team)
        {
            _loggerService?.Log($"Creating MonsterEntity of type: {type}");
            
            var definition = Resources.Load<MonsterDefinition>($"Battle/Monsters/Definitions/{type}");
            
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
            var starterDeck = team == BattleTeam.Player ?
                _abilityCardFactory?.CreateStarterDeckForMonsterEntity(type) :
                _abilityCardFactory?.CreateStarterDeckForEnemy();
            
            var model = new MonsterEntity(
                maxHealth: definition.maxHealth,
                attackDamage: definition.attackDamage,
                type: definition.type,
                monsterName: definition.monsterName,
                runes: new List<RuneType>(definition.runes), // Create defensive copy
                battleTeam: team,
                abilityDeck: starterDeck
            );
            
            _loggerService?.Log($"Successfully created MonsterEntity: {definition.monsterName}");

            return model;
        }
    }
}