using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct CardDrawnEvent : IDomainEvent
    {
        public MonsterEntity Monster { get; }
        public AbilityCard Card { get; }
        public BattleTeam Team { get; }

        public CardDrawnEvent(MonsterEntity monster, AbilityCard card, BattleTeam team)
        {
            Monster = monster;
            Card = card;
            Team = team;
        }
    }
}