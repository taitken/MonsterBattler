using Game.Domain.Entities.Overworld;
using Game.Domain.Messaging;
using UnityEngine;

namespace Game.Application.Messaging.Events.Spawning
{
    public class RoomSpawnedEvent : IDomainEvent
    {
        public RoomEntity Room { get; }
        public Vector3 Position { get; }

        public RoomSpawnedEvent(RoomEntity room, Vector3 position)
        {
            Room = room;
            Position = position;
        }
    }
}