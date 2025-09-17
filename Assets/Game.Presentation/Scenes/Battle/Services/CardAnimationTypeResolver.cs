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
                return CardAnimationType.None;

            // Check effects in priority order - first match wins
            foreach (var effect in card.Effects)
            {
                switch (effect.Type)
                {
                    case EffectType.Damage:
                        return CardAnimationType.Attack;
                    
                    // Future animation types can be added here
                    case EffectType.Heal:
                        return CardAnimationType.None; // Not implemented yet
                    
                    case EffectType.Block:
                        return CardAnimationType.Defend;
                    
                    // Add more cases as needed
                    default:
                        continue; // Check next effect
                }
            }

            return CardAnimationType.None;
        }
    }
}