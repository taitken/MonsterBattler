
using System;

namespace Game.Application.DTOs
{
    public readonly struct OverworldPayload 
    {
        public OverworldPayload(Guid roomId)
        {
            RoomId = roomId;
        }
        public readonly Guid RoomId;
    }
}
