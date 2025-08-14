
using System;

namespace Game.Applcation.DTOs
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
