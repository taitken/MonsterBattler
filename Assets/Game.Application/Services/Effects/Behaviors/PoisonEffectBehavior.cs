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
            _log?.Log($"Applying poison to {owner.MonsterName}. Expired: {effect.IsExpired}, Value: {effect.Stacks}");
            if (effect.IsExpired || effect.Stacks <= 0)
                return;

            // Route poison damage through EffectProcessor using ResolveDamageCommand
            _bus.Publish(new ResolveDamageCommand(owner, owner, effect.Stacks));

            _log?.Log($"{owner.MonsterName} takes {effect.Stacks} poison damage");

            // Reduce poison stacks by 1
            effect.ReduceStacks(1);

            if (effect.Stacks <= 0)
            {
                _log?.Log($"Poison effect on {owner.MonsterName} has been consumed");
            }
        }
    }
}