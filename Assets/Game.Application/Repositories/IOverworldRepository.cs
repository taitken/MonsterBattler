using System;
using Game.Domain.Entities.Overworld;

namespace Game.Application.Repositories
{
    public interface IOverworldRepository
    {
        OverworldEntity GetCurrentOverworld();
        RoomEntity GetRoomById(Guid roomId);
        void SaveCurrentOverworld(OverworldEntity overworld);
        bool HasCurrentOverworld();
    }
}