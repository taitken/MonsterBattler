using System;
using Game.Application.DTOs.Effects;
using Game.Application.Interfaces.Effects;
using Game.Core.Logger;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Services.Effects.Behaviors
{
    public class BlockEffectBehavior : IOnDamageTakenBehavior
    {
        private readonly ILoggerService _log;

        public BlockEffectBehavior(ILoggerService log)
        {
            _log = log;
        }

        public DamageModificationResult ModifyDamage(MonsterEntity target, int incomingDamage, MonsterEntity source, StatusEffect effect)
        {
            if (effect.IsExpired || effect.Stacks <= 0 || incomingDamage <= 0)
                return DamageModificationResult.NoModification(incomingDamage);

            var damageToBlock = Math.Min(incomingDamage, effect.Stacks);
            var finalDamage = Math.Max(0, incomingDamage - damageToBlock);

            _log?.Log($"{target.MonsterName}'s defense blocks {damageToBlock} damage");

            // Reduce block Stacks by the amount used
            effect.ReduceStacks(damageToBlock);

            return DamageModificationResult.Blocked(incomingDamage, damageToBlock);
        }
    }
}