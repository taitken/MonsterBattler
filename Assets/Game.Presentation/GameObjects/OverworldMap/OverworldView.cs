using Game.Core;
using Game.Domain.Entities.Overworld;
using Game.Presentation.GameObjects.Factories;
using Game.Presentation.Spawners;
using UnityEngine;

namespace Game.Presentation.GameObjects.OverworldMap
{
    public class OverworldView : MonoObject<OverworldEntity>
    {
        public Vector3 originalPosition;
        private SpriteRenderer _spriteRenderer;
        private IRoomViewFactory _roomFactory;
        private OverworldSceneSpawner _spawner;
        
        void Awake()
        {
            Debug.Log("Overworld Created");
            originalPosition = transform.position;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _roomFactory = ServiceLocator.Get<IRoomViewFactory>();
            _spawner = FindObjectOfType<OverworldSceneSpawner>();
        }
        
        protected override void OnModelBound()
        {
            Debug.Log("Binding Overworld");
            
            if (_spawner != null && model != null)
            {
                Debug.Log($"Spawning {model.Rooms.Count} rooms from overworld");
                _spawner.SpawnRoomsFromOverworld(model);
            }
            else
            {
                Debug.LogError("Spawner or Model is null - cannot spawn rooms");
            }
        }
    }
}