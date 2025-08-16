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

        public OverworldEntity GetCurrentOverworld()
        {
            return _currentOverworld;
        }

        public void MarkRoomAsCompleted(Guid roomId)
        {
            if (_currentOverworld == null)
            {
                _log?.LogWarning("Cannot mark room as completed: no current overworld");
                return;
            }

            var room = _currentOverworld.GetRoomById(roomId);
            if (room != null)
            {
                room.MarkAsCompleted();
                _persistence.SaveCurrentOverworld(_currentOverworld);
                _log?.Log($"Marked room {roomId} as completed");
            }
            else
            {
                _log?.LogWarning($"Room with ID {roomId} not found in current overworld");
            }
        }

        public bool IsRoomAccessible(Guid roomId)
        {
            if (_currentOverworld == null)
            {
                return false;
            }

            var targetRoom = _currentOverworld.GetRoomById(roomId);
            if (targetRoom == null)
            {
                return false;
            }

            // Starting room is always accessible
            if (targetRoom.IsStartingRoom)
            {
                return true;
            }

            // Check if any adjacent room is completed
            foreach (var room in _currentOverworld.Rooms)
            {
                if (room.IsCompleted && IsDirectlyConnected(room, targetRoom))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsDirectlyConnected(RoomEntity roomA, RoomEntity roomB)
        {
            return (roomA.NorthRoomId == roomB.Id) ||
                   (roomA.SouthRoomId == roomB.Id) ||
                   (roomA.EastRoomId == roomB.Id) ||
                   (roomA.WestRoomId == roomB.Id);
        }

    }
}