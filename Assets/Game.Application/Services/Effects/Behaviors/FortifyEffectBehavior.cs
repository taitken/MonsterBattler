using Game.Application.Interfaces.Effects;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Application.Services.Effects.Behaviors
{
    public class FortifyEffectBehavior : IOnEffectAppliedBehavior
    {
        private readonly ILoggerService _log;

        public FortifyEffectBehavior(ILoggerService log)
        {
            _log = log;
        }

        public void OnEffectApplied(MonsterEntity target, StatusEffect appliedEffect, StatusEffect thisEffect)
        {
            // Only modify Block effects
            if (appliedEffect.Type != EffectType.Block || thisEffect.IsExpired || thisEffect.Stacks <= 0)
                return;

            // Add the current stacks of Fortify to the block amount being applied
            var bonusBlock = thisEffect.Stacks;
            appliedEffect.IncreaseStacks(bonusBlock);

            _log?.Log($"{target.MonsterName}'s Fortify ({thisEffect.Stacks} stacks) increased block by {bonusBlock} (new block: {appliedEffect.Stacks})");
        }
    }
}