using Game.Application.Interfaces.Effects;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Services.Effects.Behaviors
{
    public class RegenerateEffectBehavior : IOnTurnEndBehavior
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;

        public RegenerateEffectBehavior(IEventBus bus, ILoggerService log)
        {
            _bus = bus;
            _log = log;
        }

        public void OnTurnEnd(MonsterEntity owner, StatusEffect effect)
        {
            if (effect.IsExpired || effect.Value <= 0)
                return;

            // Heal equal to current stacks
            var healAmount = effect.Value;
            owner.Heal(healAmount);

            _log?.Log($"{owner.MonsterName} regenerates {healAmount} health from {effect.Value} stacks");
            _bus.Publish(new HealingAppliedEvent(owner, owner, healAmount));

            // Half the remaining stacks (1 becomes 0)
            var newValue = effect.Value == 1 ? 0 : effect.Value / 2;
            var reduction = effect.Value - newValue;
            effect.ReduceValue(reduction);

            if (effect.Value <= 0)
            {
                _log?.Log($"Regenerate effect on {owner.MonsterName} has been consumed");
            }
            else
            {
                _log?.Log($"Regenerate stacks reduced from {effect.Value + reduction} to {effect.Value}");
            }
        }
    }
}