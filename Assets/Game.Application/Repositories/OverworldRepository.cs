using System;
using Game.Application.Interfaces;
using Game.Domain.Entities.Overworld;
using Game.Domain.Enums;
namespace Game.Application.Repositories
{
    public class OverworldRepository : IOverworldRepository
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