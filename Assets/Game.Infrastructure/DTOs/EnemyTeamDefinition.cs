using System.Linq;
using Game.Domain.Enums;
using UnityEngine;

namespace Game.Infrastructure.DTOs
{
    [System.Serializable]
    public class EnemyTeamDefinition
    {
        [SerializeField] private string teamName;
        [SerializeField] private MonsterType[] monsterTypes;
        [SerializeField] private Biome[] validBiomes; // Biomes where this encounter can appear
        [SerializeField] private int difficultyLevel = 1;
        [SerializeField] private float spawnWeight = 1f; // For weighted random selection
        
        public string TeamName => teamName;
        public MonsterType[] MonsterTypes => monsterTypes;
        public Biome[] ValidBiomes => validBiomes;
        public int DifficultyLevel => difficultyLevel;
        public float SpawnWeight => spawnWeight;
        
        public bool IsValidForBiome(Biome biome)
        {
            return validBiomes == null || validBiomes.Length == 0 || validBiomes.Contains(biome);
        }
    }
}