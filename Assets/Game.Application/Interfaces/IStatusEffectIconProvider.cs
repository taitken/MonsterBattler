using Game.Domain.Enums;
using UnityEngine;

namespace Game.Application.Interfaces
{
    public interface IStatusEffectIconProvider
    {
        /// <summary>
        /// Gets the sprite for a specific status effect type
        /// </summary>
        /// <param name="effectType">The effect type to get the sprite for</param>
        /// <returns>The sprite for the effect, or null if not found</returns>
        Sprite GetEffectSprite(EffectType effectType);

        /// <summary>
        /// Checks if a sprite exists for the given effect type
        /// </summary>
        /// <param name="effectType">The effect type to check</param>
        /// <returns>True if a sprite exists, false otherwise</returns>
        bool HasSprite(EffectType effectType);
    }
}