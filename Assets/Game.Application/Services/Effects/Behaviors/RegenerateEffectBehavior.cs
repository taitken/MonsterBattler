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
            if (effect.IsExpired || effect.Stacks <= 0)
                return;

            // Route regeneration through ResolveHealCommand for consistency
            var healAmount = effect.Stacks;
            _bus.Publish(new ResolveHealCommand(owner, owner, healAmount));

            _log?.Log($"{owner.MonsterName} regenerates {healAmount} health from {effect.Stacks} stacks");

            // Half the remaining stacks (1 becomes 0)
            var newStacks = effect.Stacks == 1 ? 0 : effect.Stacks / 2;
            var reduction = effect.Stacks - newStacks;
            effect.ReduceStacks(reduction);

            if (effect.Stacks <= 0)
            {
                _log?.Log($"Regenerate effect on {owner.MonsterName} has been consumed");
            }
            else
            {
                _log?.Log($"Regenerate stacks reduced from {effect.Stacks + reduction} to {effect.Stacks}");
            }
        }
    }
}