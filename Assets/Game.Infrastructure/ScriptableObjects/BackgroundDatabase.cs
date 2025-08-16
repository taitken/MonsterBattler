using System.Linq;
using Game.Domain.Enums;
using Game.Infrastructure.DTOs;
using UnityEngine;
namespace Game.Infrastructure.ScriptableObjects
{
    /// <summary>
    /// Represents a monster definition used in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "BackgroundDatabase", menuName = "App/Databases/BackgroundDatabase")]
    public class BackgroundDatabase : ScriptableObject
    {
        [SerializeField] private BackgroundAdrData[] background;

        public Sprite GetBackgroundImage(Biome type)
        {
            return background.FirstOrDefault(m => m.type == type)?.sprite;
        }
    }
}