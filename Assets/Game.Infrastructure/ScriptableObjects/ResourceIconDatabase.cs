using System.Linq;
using Game.Domain.Enums;
using Game.Infrastructure.DTOs;
using UnityEngine;
namespace Game.Infrastructure.ScriptableObjects
{
    /// <summary>
    /// Represents a monster definition used in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceIconDatabase", menuName = "App/Databases/ResourceIconDatabase")]
    public class ResourceIconDatabase : ScriptableObject
    {
        [SerializeField] private ResourceIconSpriteData[] resources;

        public Sprite GetResourceSprite(ResourceType type)
        {
            return resources.FirstOrDefault(m => m.type == type)?.sprite;
        }
    }
}