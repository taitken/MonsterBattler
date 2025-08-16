using System.Linq;
using Game.Domain.Enums;
using Game.Infrastructure.DTOs;
using UnityEngine;
namespace Game.Infrastructure.ScriptableObjects
{
    /// <summary>
    /// Represents a monster definition used in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterDatabase", menuName = "App/Databases/MonsterDatabase")]
    public class MonsterDatabase : ScriptableObject
    {
        [SerializeField] private MonsterSpriteData[] monsters;

        public Sprite GetMonsterSprite(MonsterType type)
        {
            return monsters.FirstOrDefault(m => m.type == type)?.sprite;
        }
    }
}