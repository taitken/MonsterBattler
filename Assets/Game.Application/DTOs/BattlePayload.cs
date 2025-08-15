
using System;

namespace Game.Application.DTOs
{
    public readonly struct BattlePayload 
    {
        public BattlePayload(Guid roomId)
        {
            RoomId = roomId;
        }
        public readonly Guid RoomId;
    }
}
