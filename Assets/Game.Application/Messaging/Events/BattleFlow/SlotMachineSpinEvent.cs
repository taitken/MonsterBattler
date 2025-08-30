using Game.Application.DTOs;
using Game.Domain.Messaging;

namespace Game.Application.Messaging.Events.BattleFlow
{
    public readonly struct SlotMachineSpinEvent : IDomainEvent
    {
        public int[] WheelValues { get; }
        public BarrierToken CompletionToken { get; }

        public SlotMachineSpinEvent(int[] wheelValues, BarrierToken completionToken)
        {
            WheelValues = wheelValues;
            CompletionToken = completionToken;
        }
    }
}