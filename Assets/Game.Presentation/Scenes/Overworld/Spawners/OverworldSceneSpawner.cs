using System;
using System.Collections.Generic;
using System.Linq;
using Game.Application.Messaging;
using Game.Application.Messaging.Events.Spawning;
using Game.Core;
using Game.Domain.Entities.Overworld;
using Game.Presentation.GameObjects.Factories;
using Game.Presentation.GameObjects.OverworldMap;
using UnityEngine;

namespace Game.Presentation.Spawners
{
    public class OverworldSceneSpawner : MonoBehaviour
    {
        [SerializeField] private Transform roomSpawnParent;
        [SerializeField] private Vector2 gridSpacing;
        [SerializeField] private List<Sprite> roomSprites;
        [SerializeField] private Vector2 positionVariation = new Vector2(0.3f, 0.2f); // X and Y variation ranges
        private const int ROOMS_PER_LAYER = 4;
        
        private IEventBus _eventBus;
        private IDisposable _roomEventSubscription;
        private IRoomViewFactory _roomViewFactory;
        private List<RoomView> spawnedRooms = new();
        private const float GRID_OFFSET = 2f;
        private int roomCount = 0;

        void Awake()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _roomViewFactory = ServiceLocator.Get<IRoomViewFactory>();
            _roomEventSubscription = _eventBus.Subscribe<RoomSpawnedEvent>(OnRoomSpawned);
        }

        private void OnRoomSpawned(RoomSpawnedEvent evt)
        {
            var spawnPosition = DetermineRoomSpawnPoint(evt);
            var roomView = _roomViewFactory.Create(evt.Room, spawnPosition);
            
            // Parent the room to the spawn parent if specified
            if (roomSpawnParent != null)
            {
                roomView.transform.SetParent(roomSpawnParent);
            }
            
            // Assign random sprite
            AssignRandomSprite(roomView, evt.Room);
            
            spawnedRooms.Add(roomView);
            roomCount++;
        }

        private Vector3 DetermineRoomSpawnPoint(RoomSpawnedEvent evt)
        {
            // If event specifies a position, use it
            if (evt.Position != Vector3.zero)
            {
                return evt.Position;
            }

            // Otherwise, arrange in a grid pattern
            return CalculateGridPosition(roomCount);
        }

        private Vector3 CalculateGridPosition(int index)
        {
            int row = index / ROOMS_PER_LAYER;
            int col = index % ROOMS_PER_LAYER;
            
            Vector3 basePosition = roomSpawnParent != null ? roomSpawnParent.position : Vector3.zero;
            Vector3 gridOffset = new Vector3(col * gridSpacing.x - GRID_OFFSET, -row * gridSpacing.y, 0);
            
            return basePosition + gridOffset;
        }

        public void SpawnRoom(RoomEntity roomEntity, Vector3 position = default)
        {
            _eventBus.Publish(new RoomSpawnedEvent(roomEntity, position));
        }

        public void SpawnRoomsFromOverworld(OverworldEntity overworld)
        {
            // Find the starting room to use as origin (0,0)
            var startingRoom = overworld.GetStartingRoom();
            if (startingRoom == null) return;
            
            // Group rooms by layer for proper centering
            var roomsByLayer = new Dictionary<int, List<RoomEntity>>();
            foreach (var room in overworld.Rooms)
            {
                if (!roomsByLayer.ContainsKey(room.Layer))
                    roomsByLayer[room.Layer] = new List<RoomEntity>();
                roomsByLayer[room.Layer].Add(room);
            }
            
            foreach (var room in overworld.Rooms)
            {
                Vector3 worldPosition = CalculateLayerBasedPosition(room, roomsByLayer);
                
                // Add spawn parent offset if it exists
                if (roomSpawnParent != null)
                {
                    worldPosition += roomSpawnParent.position;
                }
                
                SpawnRoom(room, worldPosition);
            }
        }
        
        private Vector3 CalculateLayerBasedPosition(RoomEntity room, Dictionary<int, List<RoomEntity>> roomsByLayer)
        {
            // X position based on layer (horizontal progression) with offset
            float xPos = room.Layer * gridSpacing.x - GRID_OFFSET;
            
            // Y position: center the rooms in each layer vertically
            var layerRooms = roomsByLayer[room.Layer];
            int roomsInLayer = layerRooms.Count;
            
            // Calculate vertical offset to center this layer
            float layerHeight = (roomsInLayer - 1) * gridSpacing.y;
            float layerStartY = layerHeight / 2f; // Start from top of centered layer
            
            // Find this room's position within the layer
            int roomIndexInLayer = room.PositionInLayer;
            float yPos = layerStartY - (roomIndexInLayer * gridSpacing.y);
            
            // Add organic variation (except for starting room and boss room)
            if (!room.IsStartingRoom && room.Layer != 14)
            {
                // Use room ID as seed for consistent randomization
                UnityEngine.Random.State originalState = UnityEngine.Random.state;
                UnityEngine.Random.InitState(room.Id.GetHashCode());
                
                float xVariation = UnityEngine.Random.Range(-positionVariation.x, positionVariation.x);
                float yVariation = UnityEngine.Random.Range(-positionVariation.y, positionVariation.y);
                
                xPos += xVariation;
                yPos += yVariation;
                
                // Restore original random state
                UnityEngine.Random.state = originalState;
            }
            
            return new Vector3(xPos, yPos, 0);
        }
        
        private void AssignRandomSprite(RoomView roomView, RoomEntity room)
        {
            if (roomSprites == null || roomSprites.Count == 0)
                return;
                
            var spriteRenderer = roomView.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;
            
            Sprite selectedSprite;
            
            // Always use first sprite for starting room or final boss room (layer 14)
            if (room.IsStartingRoom || room.Layer == 14)
            {
                selectedSprite = roomSprites[0];
            }
            // 60% chance for first sprite on other rooms
            else if (UnityEngine.Random.Range(0f, 100f) < 60f)
            {
                selectedSprite = roomSprites[0];
            }
            else
            {
                // Randomly select from remaining sprites (if any)
                if (roomSprites.Count > 1)
                {
                    int randomIndex = UnityEngine.Random.Range(1, roomSprites.Count);
                    selectedSprite = roomSprites[randomIndex];
                }
                else
                {
                    // Fallback to first sprite if only one exists
                    selectedSprite = roomSprites[0];
                }
            }
            
            spriteRenderer.sprite = selectedSprite;
        }

        public void ClearAllRooms()
        {
            foreach (var roomView in spawnedRooms)
            {
                if (roomView != null)
                {
                    Destroy(roomView.gameObject);
                }
            }
            spawnedRooms.Clear();
            roomCount = 0;
        }
        
        public void RefreshAllRoomAccessibility()
        {
            foreach (var roomView in spawnedRooms)
            {
                if (roomView != null)
                {
                    roomView.RefreshAccessibility();
                }
            }
        }

        void OnDestroy()
        {
            _roomEventSubscription?.Dispose();
        }
    }
}