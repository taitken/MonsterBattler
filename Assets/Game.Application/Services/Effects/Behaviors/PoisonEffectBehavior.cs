using Game.Application.Interfaces;
using Game.Application.Interfaces.Effects;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Services.Effects.Behaviors
{
    public class PoisonEffectBehavior : IOnTurnEndBehavior
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;

        public PoisonEffectBehavior(IEventBus bus, ILoggerService log)
        {
            _bus = bus;
            _log = log;
        }

        public void OnTurnEnd(MonsterEntity owner, StatusEffect effect)
        {
            if (effect.IsExpired || effect.Value <= 0)
                return;

            var damageDealt = owner.TakeDamage(effect.Value);

            _log?.Log($"{owner.MonsterName} takes {effect.Value} poison damage");
            _bus.Publish(new DamageAppliedEvent(owner, owner, effect.Value, 0, null));

            // Reduce burn stacks by 1
            effect.ReduceValue(1);

            if (effect.Value <= 0)
            {
                _log?.Log($"Burn effect on {owner.MonsterName} has been consumed");
            }
        }
    }
}