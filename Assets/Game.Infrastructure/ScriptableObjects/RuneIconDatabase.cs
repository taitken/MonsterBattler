using System.Linq;
using Game.Domain.Enums;
using Game.Infrastructure.DTOs;
using UnityEngine;

namespace Game.Infrastructure.ScriptableObjects
{
    /// <summary>
    /// Database containing sprite mappings for different rune types.
    /// </summary>
    [CreateAssetMenu(fileName = "RuneIconDatabase", menuName = "App/Databases/RuneIconDatabase")]
    public class RuneIconDatabase : ScriptableObject
    {
        [SerializeField] private RuneIconSpriteData[] runes;

        public Sprite GetRuneSprite(RuneType type)
        {
            return runes.FirstOrDefault(r => r.type == type)?.sprite;
        }

        public Color GetRuneGlowColor(RuneType type)
        {
            var runeData = runes.FirstOrDefault(r => r.type == type);
            return runeData?.glowColor ?? Color.red; // Default to red if not found
        }
    }
}