
using System;
using Game.Domain.Enums;

namespace Game.Domain.Entities.Overworld
{
    public class RoomEntity : BaseEntity
    {
        public int X { get; set; }  // Layer index (0 = starting layer, increases toward boss)
        public int Y { get; set; }  // Position within layer (0 = topmost, increases downward)
        public bool IsCompleted { get; private set; }
        public bool IsStartingRoom { get; private set; }
        public Biome Biome { get; set; }
        
        // Convenience properties for layer-based navigation
        public int Layer => X;
        public int PositionInLayer => Y;
        
        // Room connections (null means no connection in that direction)
        public Guid? NorthRoomId { get; set; }
        public Guid? SouthRoomId { get; set; }
        public Guid? EastRoomId { get; set; }
        public Guid? WestRoomId { get; set; }

        public RoomEntity(int layer, int positionInLayer, bool isStartingRoom = false)
        {
            X = layer;  // X represents layer
            Y = positionInLayer;  // Y represents position within layer
            IsStartingRoom = isStartingRoom;
            
            // Randomly select a biome
            var biomes = (Biome[])Enum.GetValues(typeof(Biome));
            var random = new Random();
            Biome = biomes[random.Next(biomes.Length)];
        }

        public void MarkAsCompleted()
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                NotifyModelUpdated();
            }
        }

        public bool HasConnection(Direction direction)
        {
            return direction switch
            {
                Direction.North => NorthRoomId.HasValue,
                Direction.South => SouthRoomId.HasValue,
                Direction.East => EastRoomId.HasValue,
                Direction.West => WestRoomId.HasValue,
                _ => false
            };
        }

        public void SetConnection(Direction direction, Guid roomId)
        {
            switch (direction)
            {
                case Direction.North: NorthRoomId = roomId; break;
                case Direction.South: SouthRoomId = roomId; break;
                case Direction.East: EastRoomId = roomId; break;
                case Direction.West: WestRoomId = roomId; break;
            }
        }

        public int GetConnectionCount()
        {
            int count = 0;
            if (NorthRoomId.HasValue) count++;
            if (SouthRoomId.HasValue) count++;
            if (EastRoomId.HasValue) count++;
            if (WestRoomId.HasValue) count++;
            return count;
        }
    }
}