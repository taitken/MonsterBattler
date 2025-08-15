using System;
using Game.Domain.Entities.Overworld;

namespace Game.Application.Interfaces
{
    public interface IOverworldPersistenceService
    {
        OverworldEntity GetOrCreateCurrentOverworld();
        void SaveCurrentOverworld(OverworldEntity overworld);
        void MarkRoomCompleted(Guid roomId);
    }
}