using Game.Domain.Enums;
using Game.Infrastructure.DTOs;
using UnityEngine;
namespace Game.Infrastructure.ScriptableObjects
{
    /// <summary>
    /// Represents a monster definition used in the game.
    /// </summary>
    [System.Serializable]

    [CreateAssetMenu(menuName = "Monsters/Monster Definition")]
    public class MonsterDefinition : ScriptableObject
    {
        public string monsterName;
        public int maxHealth;
        public int attackDamage;
        public MonsterType type;

    }
}