using Game.Applcation.DTOs;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct ActionSelectedEvent : IDomainEvent
    {
        public ActionSelectedEvent(BattleTeam team, MonsterEntity attacker, MonsterEntity target, BarrierToken barrierToken)
        {
            Team = team;
            Attacker = attacker;
            Target = target;
            Token = barrierToken;
        }
        public BattleTeam Team { get; }
        public MonsterEntity Attacker { get; }
        public MonsterEntity Target { get; }
        public BarrierToken Token { get; }
    }
}