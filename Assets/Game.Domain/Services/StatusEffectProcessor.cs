using System;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Domain.Structs;

namespace Game.Domain.Services
{
    public static class StatusEffectProcessor
    {
        public static StatusEffectResult ProcessEffect(StatusEffect effect, MonsterEntity target)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return effect.Type switch
            {
                EffectType.Block => ProcessDefend(effect, target),
                EffectType.Damage => StatusEffectResult.Failed("Damage should be handled by CardEffectResolver, not StatusEffectProcessor"),
                EffectType.Heal => StatusEffectResult.Failed("Heal should be handled by CardEffectResolver, not StatusEffectProcessor"),
                _ => StatusEffectResult.NoEffect($"Effect type {effect.Type} not implemented yet")
            };
        }

        private static StatusEffectResult ProcessDefend(StatusEffect effect, MonsterEntity target)
        {
            // Defend is a shield that absorbs damage - no immediate effect, just applied when monster is created
            return StatusEffectResult.Success($"{target.MonsterName} gains {effect.Value} defense (shield)");
        }

    }
}