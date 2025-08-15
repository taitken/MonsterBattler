using System;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Core;
using Game.Core.Logger;
using Game.Domain.Entities.Overworld;
using Game.Domain.Enums;

namespace Game.Application.Services
{
    public class OverworldPersistenceService : IOverworldPersistenceService
    {
        private OverworldEntity _currentOverworld;
        private IEventBus _bus;
        private ILoggerService _log;
        private IOverworldGenerator _generator;

        public OverworldPersistenceService()
        {
            _bus = ServiceLocator.Get<IEventBus>();
            _log = ServiceLocator.Get<ILoggerService>();
            _generator = ServiceLocator.Get<IOverworldGenerator>();
        }

        public OverworldEntity GetOrCreateCurrentOverworld()
        {
            if (_currentOverworld == null)
            {
                _log?.Log("Generating new overworld...");
                _currentOverworld = _generator.GenerateOverworld();
                // Subscribe to battle completion events
                _bus.Subscribe<BattleEndedEvent>(OnBattleEnded);
            }
            return _currentOverworld;
        }

        public void SaveCurrentOverworld(OverworldEntity overworld)
        {
            _currentOverworld = overworld;
        }

        public void MarkRoomCompleted(Guid roomId)
        {
            if (_currentOverworld != null)
            {
                var room = _currentOverworld.Rooms.Find(r => r.Id == roomId);
                if (room != null)
                {
                    room.MarkAsCompleted();
                }
            }
        }
        
        private void OnBattleEnded(BattleEndedEvent battleEvent)
        {
            if (battleEvent.Result.Outcome == BattleOutcome.PlayerVictory)
            {
                MarkRoomCompleted(battleEvent.Result.RoomId);
                _log?.Log($"Marked room {battleEvent.Result.RoomId} as completed");
            }
        }
    }
}