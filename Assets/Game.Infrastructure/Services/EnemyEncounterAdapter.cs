using Game.Application.Interfaces;
using Game.Domain.Enums;
using Game.Infrastructure.ScriptableObjects;
using UnityEngine;

namespace Game.Infrastructure.Services
{
    public class EnemyEncounterAdapter : IEnemyEncounterProvider
    {
        private readonly EnemyEncounterDatabase _database;

        public EnemyEncounterAdapter(EnemyEncounterDatabase database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public MonsterType[] GetRandomEnemyTeam()
        {
            var encounter = _database.GetRandomEncounter();
            if (encounter == null)
            {
                Debug.LogError("Failed to get random enemy encounter - returning default");
                return new MonsterType[] { MonsterType.Knight }; // Fallback
            }
            
            return encounter.MonsterTypes ?? new MonsterType[0];
        }

        public MonsterType[] GetEnemyTeamForBiome(Biome biome)
        {
            var encounter = _database.GetEncounterForBiome(biome);
            if (encounter == null)
            {
                Debug.LogError($"Failed to get enemy encounter for biome {biome} - returning default");
                return new MonsterType[] { MonsterType.Knight }; // Fallback
            }
            
            return encounter.MonsterTypes ?? new MonsterType[0];
        }

        public MonsterType[] GetEnemyTeamForBiomeAndDifficulty(Biome biome, int maxDifficulty)
        {
            var encounter = _database.GetEncounterForBiomeAndDifficulty(biome, maxDifficulty);
            if (encounter == null)
            {
                Debug.LogError($"Failed to get enemy encounter for biome {biome} with difficulty {maxDifficulty} - returning default");
                return new MonsterType[] { MonsterType.Knight }; // Fallback
            }
            
            return encounter.MonsterTypes ?? new MonsterType[0];
        }
    }
}