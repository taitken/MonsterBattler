using System.Collections.Generic;
using Game.Application.DTOs;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct CardsDrawnEvent : IDomainEvent
    {
        public readonly struct DrawnCard
        {
            public MonsterEntity Monster { get; }
            public AbilityCard Card { get; }
            public BattleTeam Team { get; }

            public DrawnCard(MonsterEntity monster, AbilityCard card, BattleTeam team)
            {
                Monster = monster;
                Card = card;
                Team = team;
            }
        }

        public IReadOnlyList<DrawnCard> DrawnCards { get; }
        public BarrierToken CompletionToken { get; }

        public CardsDrawnEvent(IReadOnlyList<DrawnCard> drawnCards, BarrierToken completionToken)
        {
            DrawnCards = drawnCards;
            CompletionToken = completionToken;
        }
    }
}