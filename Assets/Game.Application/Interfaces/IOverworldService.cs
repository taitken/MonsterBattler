using System;
using Game.Application.DTOs;
using Game.Domain.Entities.Overworld;

namespace Game.Application.Interfaces
{
    public interface IOverworldService
    {
        OverworldEntity InitializeOverworld();
        OverworldEntity GetCurrentOverworld();
        RoomEntity GetRoomById(Guid roomId);
        void MarkRoomAsCompleted(Guid roomId);
        bool IsRoomAccessible(Guid roomId);
    }
}