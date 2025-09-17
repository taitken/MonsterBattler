using Game.Application.DTOs.Effects;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct DamageModifiedByEffectEvent : IDomainEvent
    {
        public MonsterEntity Target { get; }
        public StatusEffect Effect { get; }
        public DamageModificationResult Modification { get; }

        public DamageModifiedByEffectEvent(MonsterEntity target, StatusEffect effect, DamageModificationResult modification)
        {
            Target = target;
            Effect = effect;
            Modification = modification;
        }
    }
}