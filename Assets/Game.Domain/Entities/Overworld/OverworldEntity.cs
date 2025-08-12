
using System.Collections.Generic;

namespace Game.Domain.Entities.Overworld
{
    public class OverworldEntity : BaseEntity
    {
        public List<RoomEntity> Rooms { get; private set; }
        public OverworldEntity()
        {
            Rooms = new List<RoomEntity>();
        }

        public void AddRoom(RoomEntity newRoom)
        {
            if (newRoom == null) return;
            Rooms.Add(newRoom);
        }
    }
}