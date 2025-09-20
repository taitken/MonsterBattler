using Game.Application.DTOs.Effects;
using Game.Application.Interfaces;
using Game.Application.Interfaces.Effects;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Services.Effects.Behaviors
{
    public class BurnEffectBehavior : IOnTurnEndBehavior
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;

        public BurnEffectBehavior(IEventBus bus, ILoggerService log)
        {
            _bus = bus;
            _log = log;
        }

        public void OnTurnEnd(MonsterEntity owner, StatusEffect effect)
        {
            _log?.Log($"Applying burn to {owner.MonsterName}.");
            if (effect.IsExpired || effect.Stacks <= 0)
                return;

            // Route burn damage through EffectProcessor using ResolveDamageCommand
            _bus.Publish(new ResolveDamageCommand(owner, owner, effect.Stacks));

            _log?.Log($"{owner.MonsterName} takes {effect.Stacks} burn damage");

            // Reduce burn stacks by 1
            effect.ReduceStacks(1);

            if (effect.Stacks <= 0)
            {
                _log?.Log($"Burn effect on {owner.MonsterName} has been consumed");
            }
        }
    }
}