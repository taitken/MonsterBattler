using System;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Repositories;
using Game.Domain.Enums;


namespace Game.Application.Handlers
{
    public class BattleFlowCompleteCommandHandler : ICommandHandler<BattleFlowCompleteCommand>
    {
        private readonly IEventBus _bus;
        private readonly IBattleHistoryRepository _battleHistory;
        private readonly IOverworldService _overworldService;

        public BattleFlowCompleteCommandHandler(IEventBus bus, IBattleHistoryRepository battleHistory, IOverworldService overworldService)
        {
            _bus = bus;
            _battleHistory = battleHistory;
            _overworldService = overworldService;
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