
using System;

namespace Game.Applcation.DTOs
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
