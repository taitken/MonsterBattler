using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.Application.DTOs;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Entities.Battle;
using Game.Domain.Structs;

namespace Game.Application.Interfaces
{
    public interface ICardEffectResolver
    {
        Task ResolveCardEffectsAsync(AbilityCard card, MonsterEntity caster, MonsterEntity primaryTarget, List<MonsterEntity> allEnemies, List<MonsterEntity> allAllies, IInteractionBarrier waitBarrier, RuneFace[] currentRunes = null, CancellationToken ct = default);
        void ResolveEffect(AbilityEffect effect, MonsterEntity caster, MonsterEntity target, RuneFace[] currentRunes = null, BarrierToken? effectToken = null);
        List<MonsterEntity> GetValidTargetsForEffect(AbilityEffect effect, MonsterEntity caster, MonsterEntity primaryTarget, List<MonsterEntity> allEnemies, List<MonsterEntity> allAllies);
    }
}