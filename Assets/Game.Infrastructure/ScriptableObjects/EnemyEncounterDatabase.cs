using System.Linq;
using Game.Domain.Enums;
using Game.Infrastructure.DTOs;
using UnityEngine;

namespace Game.Infrastructure.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnemyEncounterDatabase", menuName = "App/Databases/EnemyEncounterDatabase")]
    public class EnemyEncounterDatabase : ScriptableObject
    {
        [SerializeField] private EnemyTeamDefinition[] encounters;
        
        public EnemyTeamDefinition GetRandomEncounter()
        {
            if (encounters == null || encounters.Length == 0)
            {
                Debug.LogError("No encounters defined in EnemyEncounterDatabase");
                return null;
            }
            
            return encounters[Random.Range(0, encounters.Length)];
        }
        
        public EnemyTeamDefinition GetEncounterForBiome(Biome biome)
        {
            var validEncounters = encounters?.Where(e => e.IsValidForBiome(biome)).ToArray();
            
            if (validEncounters == null || validEncounters.Length == 0)
            {
                Debug.LogWarning($"No encounters found for biome {biome}, falling back to random encounter");
                return GetRandomEncounter();
            }
            
            // Weighted random selection
            return GetWeightedRandomEncounter(validEncounters);
        }
        
        public EnemyTeamDefinition GetEncounterForBiomeAndDifficulty(Biome biome, int maxDifficulty)
        {
            var validEncounters = encounters?
                .Where(e => e.IsValidForBiome(biome) && e.DifficultyLevel <= maxDifficulty)
                .ToArray();
            
            if (validEncounters == null || validEncounters.Length == 0)
            {
                Debug.LogWarning($"No encounters found for biome {biome} with difficulty <= {maxDifficulty}, falling back to biome-only filter");
                return GetEncounterForBiome(biome);
            }
            
            return GetWeightedRandomEncounter(validEncounters);
        }
        
        private EnemyTeamDefinition GetWeightedRandomEncounter(EnemyTeamDefinition[] validEncounters)
        {
            if (validEncounters.Length == 1) return validEncounters[0];
            
            float totalWeight = validEncounters.Sum(e => e.SpawnWeight);
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var encounter in validEncounters)
            {
                currentWeight += encounter.SpawnWeight;
                if (randomValue <= currentWeight)
                {
                    return encounter;
                }
            }
            
            // Fallback to last encounter if something goes wrong
            return validEncounters[validEncounters.Length - 1];
        }
        
        public EnemyTeamDefinition[] GetAllEncounters()
        {
            return encounters ?? new EnemyTeamDefinition[0];
        }
    }
}