using Game.Application.DTOs.Effects;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Interfaces.Effects
{
    public interface IEffectBehavior
    {
        // Base interface for all effect behaviors
    }

    public interface IOnDamageTakenBehavior : IEffectBehavior
    {
        DamageModificationResult ModifyDamage(MonsterEntity target, int incomingDamage, MonsterEntity source, StatusEffect effect);
    }

    public interface IOnTurnEndBehavior : IEffectBehavior
    {
        void OnTurnEnd(MonsterEntity owner, StatusEffect effect);
    }

    public interface IOnEffectAppliedBehavior : IEffectBehavior
    {
        void OnEffectApplied(MonsterEntity target, StatusEffect appliedEffect, StatusEffect thisEffect);
    }

    public interface IOnCardPlayedBehavior : IEffectBehavior
    {
        CardPlayResult OnCardPlayed(MonsterEntity player, AbilityCard card, StatusEffect effect);
    }

    public interface IOnDamageDealtBehavior : IEffectBehavior
    {
        int ModifyOutgoingDamage(MonsterEntity caster, MonsterEntity target, int baseDamage, StatusEffect effect);
    }
}