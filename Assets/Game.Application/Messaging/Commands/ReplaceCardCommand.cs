using System;
using Game.Domain.Entities.Abilities;

namespace Game.Application.Messaging.Commands
{
    public class ReplaceCardCommand : ICommand
    {
        public AbilityCard CardToAdd { get; }
        public AbilityCard CardToRemove { get; }
        public Guid MonsterId { get; }

        public ReplaceCardCommand(AbilityCard cardToAdd, AbilityCard cardToRemove, Guid monsterId)
        {
            CardToAdd = cardToAdd;
            CardToRemove = cardToRemove;
            MonsterId = monsterId;
        }
    }
}