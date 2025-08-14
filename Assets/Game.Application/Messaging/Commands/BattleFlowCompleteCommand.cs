
using Game.Domain.Structs;

namespace Game.Application.Messaging
{
    public readonly struct BattleFlowCompleteCommand : ICommand
    {
        public readonly BattleResult Result;
        public BattleFlowCompleteCommand(BattleResult result) => Result = result;
    }
}