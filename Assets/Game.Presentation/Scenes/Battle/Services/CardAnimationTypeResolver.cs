using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Presentation.Shared.Enums;

namespace Game.Presentation.Scenes.Battle.Services
{
    public static class CardAnimationTypeResolver
    {
        public static CardAnimationType GetAnimationType(AbilityCard card)
        {
            if (card?.Effects == null || card.Effects.Count == 0)
                return CardAnimationType.Defend; // Default to Defend animation

            // Check effects in priority order - first match wins
            foreach (var effect in card.Effects)
            {
                switch (effect.Type)
                {
                    case EffectType.Damage:
                        return CardAnimationType.Attack;

                    // Future animation types can be added here
                    case EffectType.Heal:
                        return CardAnimationType.Heal; // Use heal animation when implemented

                    // All other effects use default Defend animation
                    case EffectType.Block:
                    case EffectType.Burn:
                    case EffectType.Poison:
                    case EffectType.Fortify:
                    case EffectType.Regenerate:
                    case EffectType.Frazzled:
                    case EffectType.Luck:
                    case EffectType.Strength:
                    case EffectType.Backlash:
                    case EffectType.Stun:
                    case EffectType.AddRune:
                    case EffectType.Proliferate:
                    case EffectType.Amplify:
                    default:
                        return CardAnimationType.Defend; // Default animation for all effects
                }
            }

            return CardAnimationType.Defend; // Fallback default
        }
    }
}