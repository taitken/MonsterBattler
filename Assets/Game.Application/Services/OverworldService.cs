using System;
using System.Threading;
using System.Threading.Tasks;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Core.Logger;
using Game.Domain.Entities.Overworld;
using Game.Application.DTOs;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Domain.Enums;

namespace Game.Application.Services
{
    public sealed class OverworldService : IOverworldService
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;
        private readonly IOverworldPersistenceService _persistence;
        private OverworldEntity _currentOverworld;

        public OverworldService(IEventBus bus, ILoggerService log, IOverworldPersistenceService persistence)
        {
            _bus = bus;
            _log = log;
            _persistence = persistence;
            
            
            _log.Log("OverworldService initialized.");
        }

        public OverworldEntity InitializeOverworld(OverworldPayload payload)
        {
            _currentOverworld = _persistence.GetOrCreateCurrentOverworld();
            _log?.Log("Initialized current overworld");
            return _currentOverworld;
        }

        public void LoadOverworld(OverworldPayload payload)
        {
            // if (payload.RoomId == Guid.Empty)
            //     throw new ArgumentException("Room ID cannot be empty", nameof(payload));

            // if (_currentOverworld != null)
            // {
            //     _persistence.SaveCurrentOverworld(_currentOverworld);
            //     _log?.Log("Saved current overworld state");
            // }
        }



    }
}