using System;
using System.Collections.Generic;
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
        [SerializeField] private Vector2 gridSpacing = new Vector2(3f, 3f);
        [SerializeField] private int roomsPerRow = 5;
        
        private IEventBus _eventBus;
        private IDisposable _roomEventSubscription;
        private IRoomViewFactory _roomViewFactory;
        private List<RoomView> spawnedRooms = new();
        private int roomCount = 0;

        void Awake()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _roomViewFactory = ServiceLocator.Get<IRoomViewFactory>();
            _roomEventSubscription = _eventBus.Subscribe<RoomSpawnedEvent>(OnRoomSpawned);
        }

        private void OnRoomSpawned(RoomSpawnedEvent evt)
        {
            Debug.Log($"Room spawned at position: {evt.Position}");
            var spawnPosition = DetermineRoomSpawnPoint(evt);
            var roomView = _roomViewFactory.Create(evt.Room, spawnPosition);
            
            // Parent the room to the spawn parent if specified
            if (roomSpawnParent != null)
            {
                roomView.transform.SetParent(roomSpawnParent);
            }
            
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
            int row = index / roomsPerRow;
            int col = index % roomsPerRow;
            
            Vector3 basePosition = roomSpawnParent != null ? roomSpawnParent.position : Vector3.zero;
            Vector3 gridOffset = new Vector3(col * gridSpacing.x, -row * gridSpacing.y, 0);
            
            return basePosition + gridOffset;
        }

        public void SpawnRoom(RoomEntity roomEntity, Vector3 position = default)
        {
            _eventBus.Publish(new RoomSpawnedEvent(roomEntity, position));
        }

        public void SpawnRoomsFromOverworld(OverworldEntity overworld)
        {
            foreach (var room in overworld.Rooms)
            {
                SpawnRoom(room);
            }
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

        void OnDestroy()
        {
            _roomEventSubscription?.Dispose();
        }
    }
}