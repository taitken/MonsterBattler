using System;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Core;
using Game.Domain.Enums;


namespace Game.Application.Handlers
{
    public class BattleFlowCompleteCommandHandler : ICommandHandler<BattleFlowCompleteCommand>
    {
        private readonly IEventBus _bus;
        private readonly IBattleHistoryService _battleHistory;
        private readonly IOverworldService _overworldService;

        public BattleFlowCompleteCommandHandler()
        {
            _bus = ServiceLocator.Get<IEventBus>();
            _battleHistory = ServiceLocator.Get<IBattleHistoryService>();
            _overworldService = ServiceLocator.Get<IOverworldService>();
        }

        public void Handle(BattleFlowCompleteCommand command)
        {
            _battleHistory.SaveBattleHistory(command.Result.RoomId, command.Result);
            
            if (command.Result.Outcome == BattleOutcome.PlayerVictory)
            {
                _overworldService.MarkRoomAsCompleted(command.Result.RoomId);
            }
            
            _bus.Publish(new LoadSceneCommand(GameScene.OverworldScene));
        }
    }
}