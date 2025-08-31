using System.Collections.Generic;
using Game.Application.DTOs.Rewards;
using Game.Domain.Messaging;
using Game.Domain.Structs;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct BattleEndedEvent : IDomainEvent
    {
        public BattleEndedEvent(BattleResult result, IEnumerable<Reward> rewards = null)
        {
            Result = result;
            Rewards = rewards ?? new List<Reward>();
        }
        
        public BattleResult Result { get; }
        public IEnumerable<Reward> Rewards { get; }
    }
}