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

        public bool HasCurrentOverworld()
        {
            return _currentOverworld != null;
        }

        public OverworldEntity GetCurrentOverworld()
        {
            return _currentOverworld;
        }

        public RoomEntity GetRoomById(Guid roomId)
        {
            return _currentOverworld?.GetRoomById(roomId);
        }

        public void SaveCurrentOverworld(OverworldEntity overworld)
        {
            _currentOverworld = overworld;
        }
    }
}