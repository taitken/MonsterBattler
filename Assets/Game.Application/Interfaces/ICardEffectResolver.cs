using System.Collections.Generic;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Structs;

namespace Game.Application.Interfaces
{
    public interface ICardEffectResolver
    {
        void ResolveCardEffects(AbilityCard card, MonsterEntity caster, MonsterEntity primaryTarget, List<MonsterEntity> allEnemies, List<MonsterEntity> allAllies);
        void ResolveEffect(AbilityEffect effect, MonsterEntity caster, MonsterEntity target);
        List<MonsterEntity> GetValidTargetsForEffect(AbilityEffect effect, MonsterEntity caster, MonsterEntity primaryTarget, List<MonsterEntity> allEnemies, List<MonsterEntity> allAllies);
    }
}