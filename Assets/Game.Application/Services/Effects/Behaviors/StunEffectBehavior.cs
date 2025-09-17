using Game.Application.DTOs.Effects;
using Game.Application.Interfaces.Effects;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Services.Effects.Behaviors
{
    public class StunEffectBehavior : IOnCardPlayedBehavior, IOnTurnEndBehavior
    {
        private readonly ILoggerService _log;

        public StunEffectBehavior(ILoggerService log)
        {
            _log = log;
        }

        public CardPlayResult OnCardPlayed(MonsterEntity player, AbilityCard card, StatusEffect effect)
        {
            if (effect.IsExpired || effect.Value <= 0)
                return CardPlayResult.Allowed();

            _log?.Log($"{player.MonsterName} is stunned and cannot play cards");
            return CardPlayResult.Prevented($"{player.MonsterName} is stunned and cannot act");
        }

        public void OnTurnEnd(MonsterEntity owner, StatusEffect effect)
        {
            if (effect.IsExpired)
                return;

            // Reduce stun duration by 1 each turn
            effect.ReduceValue(1);

            if (effect.Value <= 0)
            {
                _log?.Log($"{owner.MonsterName} recovers from stun");
            }
        }
    }
}