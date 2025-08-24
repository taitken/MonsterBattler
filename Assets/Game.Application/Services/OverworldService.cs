using System;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Core.Logger;
using Game.Domain.Entities.Overworld;
using Game.Application.DTOs;
using Game.Application.Repositories;

namespace Game.Application.Services
{
    public sealed class OverworldService : IOverworldService
    {
        private readonly IEventBus _bus;
        private readonly ILoggerService _log;
        private readonly IOverworldRepository _persistence;
        private readonly IOverworldGenerator _generator;
        private OverworldEntity _currentOverworld;

        public OverworldService(
            IEventBus bus,
            ILoggerService log,
            IOverworldRepository persistence,
            IOverworldGenerator generator)
        {
            _bus = bus;
            _log = log;
            _persistence = persistence;
            _generator = generator;

            _log.Log("OverworldService initialized.");
        }

        public OverworldEntity InitializeOverworld()
        {
            if (_persistence.HasCurrentOverworld())
            {
                _currentOverworld = _persistence.GetCurrentOverworld();
                _log?.Log("Loaded existing overworld");
            }
            else
            {
                _log?.Log("No existing overworld found, generating new one...");
                _currentOverworld = _generator.GenerateOverworld();
                _persistence.SaveCurrentOverworld(_currentOverworld);
                _log?.Log("Created and saved new overworld");
            }
            
            return _currentOverworld;
        }

        public OverworldEntity GetCurrentOverworld()
        {
            return _currentOverworld;
        }

        public RoomEntity GetRoomById(Guid roomId)
        {
            if (_currentOverworld == null)
            {
                _log?.LogWarning("Cannot get room: no current overworld");
                return null;
            }
            return _currentOverworld.GetRoomById(roomId);
        }

        public void MarkRoomAsCompleted(Guid roomId)
        {
            if (_currentOverworld == null)
            {
                _log?.LogWarning("Cannot mark room as completed: no current overworld");
                return;
            }

            _currentOverworld.MarkRoomAsCompleted(roomId);
            _persistence.SaveCurrentOverworld(_currentOverworld);
            _log?.Log($"Marked room {roomId} as completed and updated last completed room");
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

            // Room is accessible only if it's eastward from the last completed room
            var lastCompletedRoom = _currentOverworld.GetLastCompletedRoom();
            if (lastCompletedRoom != null)
            {
                // Check if target room is directly connected AND to the east
                if (IsDirectlyConnectedEastward(lastCompletedRoom, targetRoom))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsDirectlyConnected(RoomEntity roomA, RoomEntity roomB)
        {
            return roomA.GetAllConnectedRoomIds().Contains(roomB.Id);
        }

        private bool IsDirectlyConnectedEastward(RoomEntity fromRoom, RoomEntity toRoom)
        {
            // First check if they are directly connected
            if (!fromRoom.GetAllConnectedRoomIds().Contains(toRoom.Id))
            {
                return false;
            }

            // Then check if the target room is eastward (higher X coordinate)
            // Also allow same layer (X) connections for rooms in the same layer
            return toRoom.X >= fromRoom.X && fromRoom.EastRoomIds.Contains(toRoom.Id);
        }

    }
}