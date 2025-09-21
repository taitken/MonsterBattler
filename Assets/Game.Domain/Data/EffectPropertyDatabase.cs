using System.Collections.Generic;
using Game.Domain.Enums;

namespace Game.Domain.Data
{
    public static class EffectPropertyDatabase
    {
        private static readonly Dictionary<EffectType, EffectProperties> _effectProperties = new()
        {
            // Damage and healing effects
            { EffectType.Damage, EffectProperties.XValue | EffectProperties.Transient },
            { EffectType.Heal, EffectProperties.XValue | EffectProperties.Transient},

            // Defensive buffs
            { EffectType.Block, EffectProperties.XValue | EffectProperties.Buff },
            { EffectType.Fortify, EffectProperties.XValue | EffectProperties.Buff | EffectProperties.Permanent },
            { EffectType.Regenerate, EffectProperties.XValue | EffectProperties.Buff },
            { EffectType.Backlash, EffectProperties.XValue | EffectProperties.Buff | EffectProperties.Permanent },

            // Offensive buffs
            { EffectType.Strength, EffectProperties.XValue | EffectProperties.Buff | EffectProperties.Permanent },
            { EffectType.Luck, EffectProperties.XValue | EffectProperties.Buff },

            // Debuffs
            { EffectType.Burn, EffectProperties.XValue | EffectProperties.Debuff },
            { EffectType.Poison, EffectProperties.XValue | EffectProperties.Debuff },
            { EffectType.Frazzled, EffectProperties.XValue | EffectProperties.Debuff },
            { EffectType.Stun, EffectProperties.XValue | EffectProperties.Debuff },

            // Special effects
            { EffectType.Proliferate, EffectProperties.XValue | EffectProperties.Transient },
            { EffectType.Amplify, EffectProperties.XValue | EffectProperties.Transient },
            { EffectType.AddRune, EffectProperties.Transient | EffectProperties.Permanent }
        };

        public static EffectProperties GetProperties(EffectType effectType)
        {
            return _effectProperties.TryGetValue(effectType, out var properties) ? properties : EffectProperties.None;
        }

        public static bool HasProperty(EffectType effectType, EffectProperties property)
        {
            var properties = GetProperties(effectType);
            return (properties & property) == property;
        }

        public static bool IsBuff(EffectType effectType) => HasProperty(effectType, EffectProperties.Buff);
        public static bool IsDebuff(EffectType effectType) => HasProperty(effectType, EffectProperties.Debuff);
        public static bool IsTransient(EffectType effectType) => HasProperty(effectType, EffectProperties.Transient);
        public static bool IsPermanent(EffectType effectType) => HasProperty(effectType, EffectProperties.Permanent);
        public static bool HasXValue(EffectType effectType) => HasProperty(effectType, EffectProperties.XValue);
    }
}