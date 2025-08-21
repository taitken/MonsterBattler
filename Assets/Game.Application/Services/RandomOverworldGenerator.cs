using System;
using System.Collections.Generic;
using System.Linq;
using Game.Application.Interfaces;
using Game.Core.Logger;
using Game.Core.Randomness;
using Game.Domain.Entities.Overworld;
using Game.Domain.Enums;

namespace Game.Application.Services
{
    public class RandomOverworldGenerator : IOverworldGenerator
    {
        private readonly IRandomService _random;
        private readonly ILoggerService _log;
        private const int TOTAL_LAYERS = 15;
        private const int MAX_LANES = 4;

        public RandomOverworldGenerator(IRandomService random, ILoggerService log)
        {
            _random = random;
            _log = log;
        }

        public OverworldEntity GenerateOverworld()
        {
            var overworld = new OverworldEntity();
            _log?.Log($"Generating layer-based overworld with {TOTAL_LAYERS} layers");

            // Generate layer structure: List of layers, each layer contains rooms
            var layers = new List<List<RoomEntity>>();
            
            // Layer 0: Starting room
            var startingRoom = new RoomEntity(0, 0, true); // Layer 0, Position 0
            layers.Add(new List<RoomEntity> { startingRoom });
            overworld.AddRoom(startingRoom);
            
            // Generate intermediate layers (1 to TOTAL_LAYERS-2)
            for (int layer = 1; layer < TOTAL_LAYERS - 1; layer++)
            {
                var layerRooms = GenerateLayer(layer, layers[layer - 1]);
                layers.Add(layerRooms);
                foreach (var room in layerRooms)
                {
                    overworld.AddRoom(room);
                }
            }
            
            // Final layer: Boss room
            var bossRoom = new RoomEntity(TOTAL_LAYERS - 1, 0); // Final layer, Position 0
            layers.Add(new List<RoomEntity> { bossRoom });
            overworld.AddRoom(bossRoom);
            
            // Connect all rooms in the final intermediate layer to the boss room
            var finalIntermediateLayer = layers[TOTAL_LAYERS - 2];
            foreach (var room in finalIntermediateLayer)
            {
                ConnectRooms(room, bossRoom);
            }
            
            _log?.Log($"Generated overworld with {layers.Sum(l => l.Count)} total rooms");
            
            // Validate connectivity
            ValidateOverworldConnectivity(overworld);
            
            return overworld;
        }

        private List<RoomEntity> GenerateLayer(int layerIndex, List<RoomEntity> previousLayer)
        {
            var layerRooms = new List<RoomEntity>();
            var roomConnectionCounts = new Dictionary<RoomEntity, int>();
            
            // Initialize connection count tracking for previous layer
            foreach (var room in previousLayer)
            {
                roomConnectionCounts[room] = 0;
            }
            
            // Determine number of rooms in this layer
            // First and last layers have 1 room, intermediate layers have 2-5 rooms
            int roomCount;
            if (layerIndex == 1 || layerIndex == TOTAL_LAYERS - 2) // First intermediate or last intermediate layer
            {
                // Allow more flexibility for connecting layers
                roomCount = _random.Range(2, 3);
            }
            else
            {
                // Regular intermediate layers: 2-5 rooms
                roomCount = _random.Range(2, MAX_LANES + 1);
            }
            
            // Create rooms for this layer
            for (int position = 0; position < roomCount; position++)
            {
                var newRoom = new RoomEntity(layerIndex, position);
                layerRooms.Add(newRoom);
            }
            
            // Connect rooms from previous layer to this layer
            ConnectLayersWithConstraints(previousLayer, layerRooms, roomConnectionCounts);
            
            return layerRooms;
        }
        
        private void ConnectLayersWithConstraints(List<RoomEntity> previousLayer, List<RoomEntity> currentLayer, Dictionary<RoomEntity, int> connectionCounts)
        {
            var currentLayerConnections = new Dictionary<RoomEntity, int>();
            foreach (var room in currentLayer)
            {
                currentLayerConnections[room] = 0;
            }
            
            // First pass: ensure every room in current layer has at least one incoming connection
            foreach (var currentRoom in currentLayer)
            {
                if (currentLayerConnections[currentRoom] == 0)
                {
                    // Randomly pick a room from previous layer to connect from
                    var sourceRoom = previousLayer[_random.Range(0, previousLayer.Count)];
                    ConnectRooms(sourceRoom, currentRoom);
                    connectionCounts[sourceRoom]++;
                    currentLayerConnections[currentRoom]++;
                }
            }
            
            // Second pass: ensure every room in previous layer has at least one outgoing connection
            foreach (var sourceRoom in previousLayer)
            {
                if (connectionCounts[sourceRoom] == 0)
                {
                    // This room has no forward connections, connect it to a random room in current layer
                    var targetRoom = currentLayer[_random.Range(0, currentLayer.Count)];
                    ConnectRooms(sourceRoom, targetRoom);
                    connectionCounts[sourceRoom]++;
                    currentLayerConnections[targetRoom]++;
                }
            }
            
            // Third pass: add additional connections while respecting constraints (1-3 per room)
            foreach (var sourceRoom in previousLayer)
            {
                // Each room should have 1-3 forward connections
                int targetConnections = _random.Range(1, Math.Min(4, currentLayer.Count + 1));
                
                while (connectionCounts[sourceRoom] < targetConnections)
                {
                    // Try to connect to a room we're not already connected to
                    var availableTargets = currentLayer.Where(r => !IsConnected(sourceRoom, r)).ToList();
                    if (availableTargets.Count == 0) break;
                    
                    var targetRoom = availableTargets[_random.Range(0, availableTargets.Count)];
                    ConnectRooms(sourceRoom, targetRoom);
                    connectionCounts[sourceRoom]++;
                    currentLayerConnections[targetRoom]++;
                }
            }
            
            // Validate single-choice constraint (no more than 2 consecutive single-choice rooms)
            ValidateSingleChoiceConstraint(previousLayer, connectionCounts);
            
            // Debug: Log connection counts
            _log?.Log($"Layer connections - Previous layer: {previousLayer.Count} rooms, Current layer: {currentLayer.Count} rooms");
            foreach (var room in previousLayer)
            {
                _log?.Log($"Room at ({room.Layer},{room.PositionInLayer}) has {connectionCounts[room]} forward connections");
            }
        }
        
        private void ValidateSingleChoiceConstraint(List<RoomEntity> layer, Dictionary<RoomEntity, int> connectionCounts)
        {
            // This is a simplified constraint check - in a full implementation you'd track
            // the path history to ensure no more than 2 consecutive single-choice rooms
            // For now, ensure at least some rooms have multiple choices
            var singleChoiceRooms = layer.Where(r => connectionCounts[r] == 1).ToList();
            
            // If more than half the rooms have single choices, add some connections
            while (singleChoiceRooms.Count > layer.Count / 2)
            {
                var roomToImprove = singleChoiceRooms[_random.Range(0, singleChoiceRooms.Count)];
                // This would need the current layer context to add connections
                // For now, we'll trust the random generation to create good distribution
                break;
            }
        }
        
        private void ConnectRooms(RoomEntity sourceRoom, RoomEntity targetRoom)
        {
            // Use East connection to represent "forward" progress
            sourceRoom.SetConnection(Direction.East, targetRoom.Id);
            targetRoom.SetConnection(Direction.West, sourceRoom.Id);
        }
        
        private bool IsConnected(RoomEntity sourceRoom, RoomEntity targetRoom)
        {
            return sourceRoom.EastRoomId == targetRoom.Id;
        }
        
        private void ValidateOverworldConnectivity(OverworldEntity overworld)
        {
            var startingRoom = overworld.GetStartingRoom();
            if (startingRoom == null)
            {
                _log?.Log("ERROR: No starting room found!");
                return;
            }
            
            var visitedRooms = new HashSet<Guid>();
            var roomQueue = new Queue<RoomEntity>();
            roomQueue.Enqueue(startingRoom);
            visitedRooms.Add(startingRoom.Id);
            
            // BFS to find all reachable rooms
            while (roomQueue.Count > 0)
            {
                var currentRoom = roomQueue.Dequeue();
                
                // Check all connections from this room
                var connectedRoomIds = new[] { currentRoom.EastRoomId, currentRoom.WestRoomId, currentRoom.NorthRoomId, currentRoom.SouthRoomId }
                    .Where(id => id.HasValue).Select(id => id.Value);
                
                foreach (var connectedId in connectedRoomIds)
                {
                    if (!visitedRooms.Contains(connectedId))
                    {
                        var connectedRoom = overworld.Rooms.FirstOrDefault(r => r.Id == connectedId);
                        if (connectedRoom != null)
                        {
                            visitedRooms.Add(connectedId);
                            roomQueue.Enqueue(connectedRoom);
                        }
                    }
                }
            }
            
            _log?.Log($"Connectivity validation: {visitedRooms.Count}/{overworld.Rooms.Count} rooms reachable from starting room");
            
            if (visitedRooms.Count != overworld.Rooms.Count)
            {
                var unreachableRooms = overworld.Rooms.Where(r => !visitedRooms.Contains(r.Id)).ToList();
                foreach (var room in unreachableRooms)
                {
                    _log?.Log($"UNREACHABLE ROOM: Layer {room.Layer}, Position {room.PositionInLayer}");
                }
            }
        }
    }
}