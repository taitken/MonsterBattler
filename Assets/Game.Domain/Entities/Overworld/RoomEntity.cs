
using System;
using System.Collections.Generic;
using System.Linq;
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
        
        // Room connections - using lists to support multiple connections per direction
        public List<Guid> EastRoomIds { get; private set; }
        public List<Guid> WestRoomIds { get; private set; }

        public RoomEntity(int layer, int positionInLayer, bool isStartingRoom = false)
        {
            X = layer;  // X represents layer
            Y = positionInLayer;  // Y represents position within layer
            IsStartingRoom = isStartingRoom;
            
            // Initialize connection lists
            EastRoomIds = new List<Guid>();
            WestRoomIds = new List<Guid>();
            
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
                Direction.East => EastRoomIds.Count > 0,
                Direction.West => WestRoomIds.Count > 0,
                _ => false
            };
        }

        public void AddConnection(Direction direction, Guid roomId)
        {
            switch (direction)
            {
                case Direction.East:
                    if (!EastRoomIds.Contains(roomId))
                        EastRoomIds.Add(roomId);
                    break;
                case Direction.West:
                    if (!WestRoomIds.Contains(roomId))
                        WestRoomIds.Add(roomId);
                    break;
            }
        }

        public void RemoveConnection(Direction direction, Guid roomId)
        {
            switch (direction)
            {
                case Direction.East:
                    EastRoomIds.Remove(roomId);
                    break;
                case Direction.West:
                    WestRoomIds.Remove(roomId);
                    break;
            }
        }

        public List<Guid> GetConnections(Direction direction)
        {
            return direction switch
            {
                Direction.East => new List<Guid>(EastRoomIds),
                Direction.West => new List<Guid>(WestRoomIds),
                _ => new List<Guid>()
            };
        }

        public List<Guid> GetAllConnectedRoomIds()
        {
            var allConnections = new List<Guid>();
            allConnections.AddRange(EastRoomIds);
            allConnections.AddRange(WestRoomIds);
            return allConnections.Distinct().ToList();
        }

        public int GetConnectionCount()
        {
            return EastRoomIds.Count + WestRoomIds.Count;
        }
    }
}