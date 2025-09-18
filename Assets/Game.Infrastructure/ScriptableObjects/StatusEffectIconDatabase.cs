using System.Linq;
using Game.Domain.Enums;
using Game.Infrastructure.DTOs;
using UnityEngine;

namespace Game.Infrastructure.ScriptableObjects
{
    /// <summary>
    /// Database containing sprite mappings for different status effect types.
    /// </summary>
    [CreateAssetMenu(fileName = "StatusEffectIconDatabase", menuName = "App/Databases/StatusEffectIconDatabase")]
    public class StatusEffectIconDatabase : ScriptableObject
    {
        [SerializeField] private StatusEffectIconData[] effects;

        public Sprite GetEffectSprite(EffectType effectType)
        {
            return effects.FirstOrDefault(e => e.effectType == effectType)?.sprite;
        }

        public bool HasSprite(EffectType effectType)
        {
            return effects.Any(e => e.effectType == effectType);
        }
    }
}