using Game.Core;
using Game.Domain.Entities.Overworld;
using Game.Presentation.GameObjects.Factories;
using UnityEngine;

namespace Game.Presentation.GameObjects.OverworldMap
{
    public class OverworldView : MonoObject<OverworldEntity>
    {
        public Vector3 originalPosition;
        private SpriteRenderer _spriteRenderer;
        private IRoomViewFactory _roomFactory;
        void Awake()
        {
            Debug.Log("Overworld Created");
            originalPosition = transform.position;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _roomFactory = ServiceLocator.Get<IRoomViewFactory>();
        }
        // Overworld specific properties and methods would go here
        // For example, handling rooms, player movement, etc.
        protected override void OnModelBound()
        {
            Debug.Log("Binding Overworld");

        }
    }
}