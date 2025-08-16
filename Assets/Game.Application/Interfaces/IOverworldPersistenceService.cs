using System;
using Game.Domain.Entities.Overworld;

namespace Game.Application.Interfaces
{
    public interface IOverworldPersistenceService
    {
        OverworldEntity GetCurrentOverworld();
        RoomEntity GetRoomById(Guid roomId);
        void SaveCurrentOverworld(OverworldEntity overworld);
        bool HasCurrentOverworld();
    }
}