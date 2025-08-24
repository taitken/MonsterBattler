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
        #region Constants & Fields
        
        private readonly IRandomService _random;
        private readonly ILoggerService _log;
        private const int TOTAL_LAYERS = 15;
        private const int MAX_LANES = 4;
        private const float CROSS_PATH_CONNECTION_CHANCE = 0.40f;
        
        #endregion

        #region Constructor
        
        public RandomOverworldGenerator(IRandomService random, ILoggerService log)
        {
            _random = random;
            _log = log;
        }
        
        #endregion

        #region Public Methods
        
        public OverworldEntity GenerateOverworld()
        {
            var overworld = new OverworldEntity();
            _log?.Log($"Generating layer-based overworld with {TOTAL_LAYERS} layers");

            var layers = GenerateAllLayers();
            AddRoomsToOverworld(overworld, layers);
            ConnectFinalLayerToBoss(layers);
            
            _log?.Log($"Generated overworld with {layers.Sum(l => l.Count)} total rooms");
            ValidateOverworldConnectivity(overworld);
            
            return overworld;
        }
        
        #endregion

        #region Layer Generation
        
        private List<List<RoomEntity>> GenerateAllLayers()
        {
            var layers = new List<List<RoomEntity>>();
            
            // Layer 0: Starting room
            var startingRoom = new RoomEntity(0, 0, true);
            layers.Add(new List<RoomEntity> { startingRoom });
            
            // Generate intermediate layers (1 to TOTAL_LAYERS-2)
            for (int layer = 1; layer < TOTAL_LAYERS - 1; layer++)
            {
                var layerRooms = GenerateLayer(layer, layers[layer - 1]);
                layers.Add(layerRooms);
            }
            
            // Final layer: Boss room
            var bossRoom = new RoomEntity(TOTAL_LAYERS - 1, 0);
            layers.Add(new List<RoomEntity> { bossRoom });
            
            return layers;
        }

        private List<RoomEntity> GenerateLayer(int layerIndex, List<RoomEntity> previousLayer)
        {
            var layerRooms = new List<RoomEntity>();
            int roomCount = DetermineRoomCount(layerIndex);
            
            // Create all rooms for this layer first
            for (int position = 0; position < roomCount; position++)
            {
                var newRoom = new RoomEntity(layerIndex, position);
                layerRooms.Add(newRoom);
            }
            
            // Connect this layer to the previous layer
            ConnectLayerToPreviousLayer(previousLayer, layerRooms);
            
            return layerRooms;
        }

        private int DetermineRoomCount(int layerIndex)
        {
            // First and last intermediate layers have fewer rooms for better connectivity
            if (layerIndex == 1 || layerIndex == TOTAL_LAYERS - 2)
            {
                return _random.Range(2, 3);
            }
            
            // Regular intermediate layers: 2-5 rooms
            return _random.Range(2, MAX_LANES + 1);
        }

        private void AddRoomsToOverworld(OverworldEntity overworld, List<List<RoomEntity>> layers)
        {
            foreach (var layer in layers)
            {
                foreach (var room in layer)
                {
                    overworld.AddRoom(room);
                }
            }
        }

        private void ConnectFinalLayerToBoss(List<List<RoomEntity>> layers)
        {
            var finalIntermediateLayer = layers[TOTAL_LAYERS - 2];
            var bossRoom = layers[TOTAL_LAYERS - 1][0];
            
            foreach (var room in finalIntermediateLayer)
            {
                ConnectRooms(room, bossRoom);
            }
        }
        
        #endregion

        #region Layer Connection Logic
        
        private void ConnectLayerToPreviousLayer(List<RoomEntity> previousLayer, List<RoomEntity> currentLayer)
        {
            _log?.Log($"Connecting layer {currentLayer[0].Layer} ({currentLayer.Count} rooms) to layer {previousLayer[0].Layer} ({previousLayer.Count} rooms)");

            // Sort both layers by position to ensure consistent ordering
            var sortedPreviousLayer = previousLayer.OrderBy(r => r.PositionInLayer).ToList();
            var sortedCurrentLayer = currentLayer.OrderBy(r => r.PositionInLayer).ToList();

            // Create base non-crossing connections
            ConnectLayersWithoutCrossing(sortedPreviousLayer, sortedCurrentLayer);
            
            // Optionally add cross-path connections for variety
            TryAddCrossPathConnection(sortedPreviousLayer, sortedCurrentLayer, currentLayer[0].Layer);
        }

        private void ConnectLayersWithoutCrossing(List<RoomEntity> sortedPreviousLayer, List<RoomEntity> sortedCurrentLayer)
        {
            var sourceRoomsWithConnections = new HashSet<Guid>();
            var targetRoomsWithConnections = new HashSet<Guid>();
            
            // Create evenly distributed connections with crossing validation
            CreatePrimaryConnections(sortedPreviousLayer, sortedCurrentLayer, sourceRoomsWithConnections, targetRoomsWithConnections);
            
            // Ensure all rooms have at least one connection
            EnsureAllSourcesConnected(sortedPreviousLayer, sortedCurrentLayer, sourceRoomsWithConnections, targetRoomsWithConnections);
            EnsureAllTargetsConnected(sortedPreviousLayer, sortedCurrentLayer, sourceRoomsWithConnections, targetRoomsWithConnections);
        }

        private void CreatePrimaryConnections(List<RoomEntity> sortedPreviousLayer, List<RoomEntity> sortedCurrentLayer, HashSet<Guid> sourceRoomsWithConnections, HashSet<Guid> targetRoomsWithConnections)
        {
            for (int sourceIndex = 0; sourceIndex < sortedPreviousLayer.Count; sourceIndex++)
            {
                var sourceRoom = sortedPreviousLayer[sourceIndex];
                
                // Calculate ideal target using even distribution
                int idealTargetIndex = CalculateEvenDistributionTarget(sourceIndex, sortedPreviousLayer.Count, sortedCurrentLayer.Count);
                
                // Find the best non-crossing target
                int actualTargetIndex = FindNonCrossingTarget(sourceRoom, idealTargetIndex, sortedPreviousLayer, sortedCurrentLayer);
                
                var targetRoom = sortedCurrentLayer[actualTargetIndex];
                ConnectRooms(sourceRoom, targetRoom);
                
                sourceRoomsWithConnections.Add(sourceRoom.Id);
                targetRoomsWithConnections.Add(targetRoom.Id);
                
                _log?.Log($"Primary connection: ({sourceRoom.Layer},{sourceRoom.PositionInLayer}) -> ({targetRoom.Layer},{targetRoom.PositionInLayer}) (ideal: {idealTargetIndex}, actual: {actualTargetIndex})");
            }
        }

        private void EnsureAllSourcesConnected(List<RoomEntity> sortedPreviousLayer, List<RoomEntity> sortedCurrentLayer, HashSet<Guid> sourceRoomsWithConnections, HashSet<Guid> targetRoomsWithConnections)
        {
            foreach (var sourceRoom in sortedPreviousLayer)
            {
                if (!sourceRoomsWithConnections.Contains(sourceRoom.Id))
                {
                    var fallbackTarget = sortedCurrentLayer.Last();
                    ConnectRooms(sourceRoom, fallbackTarget);
                    sourceRoomsWithConnections.Add(sourceRoom.Id);
                    targetRoomsWithConnections.Add(fallbackTarget.Id);
                    
                    _log?.Log($"Fallback source connection: ({sourceRoom.Layer},{sourceRoom.PositionInLayer}) -> ({fallbackTarget.Layer},{fallbackTarget.PositionInLayer})");
                }
            }
        }

        private void EnsureAllTargetsConnected(List<RoomEntity> sortedPreviousLayer, List<RoomEntity> sortedCurrentLayer, HashSet<Guid> sourceRoomsWithConnections, HashSet<Guid> targetRoomsWithConnections)
        {
            foreach (var targetRoom in sortedCurrentLayer)
            {
                if (!targetRoomsWithConnections.Contains(targetRoom.Id))
                {
                    var fallbackSource = sortedPreviousLayer.Last();
                    ConnectRooms(fallbackSource, targetRoom);
                    sourceRoomsWithConnections.Add(fallbackSource.Id);
                    targetRoomsWithConnections.Add(targetRoom.Id);
                    
                    _log?.Log($"Fallback target connection: ({fallbackSource.Layer},{fallbackSource.PositionInLayer}) -> ({targetRoom.Layer},{targetRoom.PositionInLayer})");
                }
            }
        }

        private void TryAddCrossPathConnection(List<RoomEntity> sortedPreviousLayer, List<RoomEntity> sortedCurrentLayer, int layerIndex)
        {
            if (_random.Range(0f, 1f) >= CROSS_PATH_CONNECTION_CHANCE) 
                return;

            var candidateSources = FindCrossPathCandidates(sortedPreviousLayer, sortedCurrentLayer);
            _log?.Log($"Layer {layerIndex}: Found {candidateSources.Count} candidate sources for cross-path connections");
            
            if (candidateSources.Count == 0) 
                return;

            var randomSource = candidateSources[_random.Range(0, candidateSources.Count)];
            var validTargets = FindValidCrossPathTargets(randomSource, sortedPreviousLayer, sortedCurrentLayer);
            
            _log?.Log($"Source ({randomSource.Layer},{randomSource.PositionInLayer}) has {validTargets.Count} non-crossing targets");
            
            if (validTargets.Count > 0)
            {
                var selectedTarget = SelectBalancedTarget(validTargets);
                ConnectRooms(randomSource, selectedTarget);
                _log?.Log($"Added cross-path connection for layer {layerIndex}: ({randomSource.Layer},{randomSource.PositionInLayer}) -> ({selectedTarget.Layer},{selectedTarget.PositionInLayer})");
            }
        }

        private List<RoomEntity> FindCrossPathCandidates(List<RoomEntity> sortedPreviousLayer, List<RoomEntity> sortedCurrentLayer)
        {
            var candidateSources = new List<RoomEntity>();
            
            foreach (var source in sortedPreviousLayer)
            {
                var hasValidTargets = sortedCurrentLayer.Any(target => 
                    !IsConnected(source, target) && 
                    !WouldCreateCrossing(source, target, sortedPreviousLayer, sortedCurrentLayer));
                    
                if (hasValidTargets)
                {
                    candidateSources.Add(source);
                }
            }
            
            return candidateSources;
        }

        private List<RoomEntity> FindValidCrossPathTargets(RoomEntity source, List<RoomEntity> sortedPreviousLayer, List<RoomEntity> sortedCurrentLayer)
        {
            return sortedCurrentLayer.Where(target => 
                !IsConnected(source, target) &&
                !WouldCreateCrossing(source, target, sortedPreviousLayer, sortedCurrentLayer)
            ).ToList();
        }

        private RoomEntity SelectBalancedTarget(List<RoomEntity> validTargets)
        {
            return validTargets
                .OrderBy(r => r.WestRoomIds.Count)
                .ThenBy(r => _random.Range(0, 100))
                .First();
        }
        
        #endregion

        #region Connection Utilities
        
        private void ConnectRooms(RoomEntity sourceRoom, RoomEntity targetRoom)
        {
            sourceRoom.AddConnection(Direction.East, targetRoom.Id);
            targetRoom.AddConnection(Direction.West, sourceRoom.Id);
        }
        
        private bool IsConnected(RoomEntity sourceRoom, RoomEntity targetRoom)
        {
            return sourceRoom.GetConnections(Direction.East).Contains(targetRoom.Id);
        }
        
        #endregion

        #region Distribution & Crossing Logic
        
        private int CalculateEvenDistributionTarget(int sourceIndex, int sourceCount, int targetCount)
        {
            float sourceRatio = (float)sourceIndex / Math.Max(1, sourceCount - 1);
            float targetPosition = sourceRatio * Math.Max(1, targetCount - 1);
            return Math.Min(targetCount - 1, Math.Max(0, (int)Math.Round(targetPosition)));
        }

        private int FindNonCrossingTarget(RoomEntity sourceRoom, int idealTargetIndex, List<RoomEntity> sortedPreviousLayer, List<RoomEntity> sortedCurrentLayer)
        {
            // Expand search from ideal target outward
            for (int distance = 0; distance < sortedCurrentLayer.Count; distance++)
            {
                var candidateIndices = GetCandidateIndices(idealTargetIndex, distance, sortedCurrentLayer.Count);
                
                foreach (int candidateIndex in candidateIndices)
                {
                    var candidateTarget = sortedCurrentLayer[candidateIndex];
                    
                    if (!WouldCreateCrossing(sourceRoom, candidateTarget, sortedPreviousLayer, sortedCurrentLayer))
                    {
                        return candidateIndex;
                    }
                }
            }
            
            // Fallback (should rarely happen)
            _log?.Log($"WARNING: No non-crossing target found for source ({sourceRoom.Layer},{sourceRoom.PositionInLayer}), using fallback");
            return sortedCurrentLayer.Count - 1;
        }

        private List<int> GetCandidateIndices(int idealIndex, int distance, int maxCount)
        {
            var candidates = new List<int>();
            
            if (distance == 0)
            {
                candidates.Add(idealIndex);
            }
            else
            {
                if (idealIndex + distance < maxCount)
                    candidates.Add(idealIndex + distance);
                    
                if (idealIndex - distance >= 0)
                    candidates.Add(idealIndex - distance);
            }
            
            return candidates;
        }

        private bool WouldCreateCrossing(RoomEntity sourceRoom, RoomEntity targetRoom, List<RoomEntity> sortedPreviousLayer, List<RoomEntity> sortedCurrentLayer)
        {
            int sourceIndex = sortedPreviousLayer.FindIndex(r => r.Id == sourceRoom.Id);
            int targetIndex = sortedCurrentLayer.FindIndex(r => r.Id == targetRoom.Id);
            
            if (sourceIndex == -1 || targetIndex == -1)
                return true;
            
            // Check all existing connections for crossing
            for (int i = 0; i < sortedPreviousLayer.Count; i++)
            {
                var otherSource = sortedPreviousLayer[i];
                
                foreach (var eastId in otherSource.EastRoomIds)
                {
                    var otherTarget = sortedCurrentLayer.FirstOrDefault(r => r.Id == eastId);
                    if (otherTarget == null) continue;
                    
                    int otherSourceIndex = i;
                    int otherTargetIndex = sortedCurrentLayer.FindIndex(r => r.Id == otherTarget.Id);
                    
                    // Check for crossing: source order != target order means lines cross
                    bool sourceOrder = sourceIndex < otherSourceIndex;
                    bool targetOrder = targetIndex < otherTargetIndex;
                    
                    if (targetIndex != otherTargetIndex && sourceOrder != targetOrder)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        #endregion

        #region Validation
        
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
                var connectedRoomIds = currentRoom.GetAllConnectedRoomIds();
                
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
        
        #endregion
    }
}