
using System;
using System.Collections.Generic;
using System.Linq;

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

        public RoomEntity GetStartingRoom()
        {
            return Rooms.FirstOrDefault(r => r.IsStartingRoom);
        }

        public RoomEntity GetRoomAt(int x, int y)
        {
            return Rooms.FirstOrDefault(r => r.X == x && r.Y == y);
        }

        public RoomEntity GetRoomById(Guid roomId)
        {
            return Rooms.FirstOrDefault(r => r.Id == roomId);
        }
    }
}