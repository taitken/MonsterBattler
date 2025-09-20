using Game.Application.Interfaces.Effects;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Services.Effects.Behaviors
{
    public class FrazzledEffectBehavior : IOnTurnEndBehavior
    {
        private readonly ILoggerService _log;

        public FrazzledEffectBehavior(ILoggerService log)
        {
            _log = log;
        }

        public void OnTurnEnd(MonsterEntity owner, StatusEffect effect)
        {
            if (effect.IsExpired || effect.Stacks <= 0)
                return;

            // Reduce Frazzled stacks by 1
            effect.ReduceStacks(1);

            if (effect.Stacks <= 0)
            {
                _log?.Log($"Frazzled effect on {owner.MonsterName} has expired");
            }
            else
            {
                _log?.Log($"Frazzled stacks on {owner.MonsterName} reduced by 1 (remaining: {effect.Stacks})");
            }
        }
    }
}