using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct CardPlayPreventedEvent : IDomainEvent
    {
        public MonsterEntity Player { get; }
        public AbilityCard Card { get; }
        public StatusEffect PreventingEffect { get; }
        public string Reason { get; }

        public CardPlayPreventedEvent(MonsterEntity player, AbilityCard card, StatusEffect preventingEffect, string reason)
        {
            Player = player;
            Card = card;
            PreventingEffect = preventingEffect;
            Reason = reason;
        }
    }
}