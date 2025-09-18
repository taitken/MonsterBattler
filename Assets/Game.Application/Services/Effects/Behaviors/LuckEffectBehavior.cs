using Game.Application.Interfaces.Effects;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Services.Effects.Behaviors
{
    public class LuckEffectBehavior : IOnTurnEndBehavior
    {
        private readonly ILoggerService _log;

        public LuckEffectBehavior(ILoggerService log)
        {
            _log = log;
        }

        public void OnTurnEnd(MonsterEntity owner, StatusEffect effect)
        {
            if (effect.IsExpired || effect.Value <= 0)
                return;

            // Reduce Luck stacks by 1
            effect.ReduceValue(1);

            if (effect.Value <= 0)
            {
                _log?.Log($"Luck effect on {owner.MonsterName} has expired");
            }
            else
            {
                _log?.Log($"Luck stacks on {owner.MonsterName} reduced by 1 (remaining: {effect.Value})");
            }
        }
    }
}