using Game.Application.DTOs;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct CardPlayedEvent : IDomainEvent
    {
        public BattleTeam Team { get; }
        public MonsterEntity Caster { get; }
        public AbilityCard Card { get; }
        public MonsterEntity PrimaryTarget { get; }
        public BarrierToken AnimationToken { get; }

        public CardPlayedEvent(BattleTeam team, MonsterEntity caster, AbilityCard card, 
            MonsterEntity primaryTarget, BarrierToken animationToken)
        {
            Team = team;
            Caster = caster;
            Card = card;
            PrimaryTarget = primaryTarget;
            AnimationToken = animationToken;
        }
    }
}