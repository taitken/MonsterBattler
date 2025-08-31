using Game.Domain.Entities.Abilities;
using Game.Presentation.Shared.Factories;
using Game.Presentation.Shared.Views;
using UnityEngine;

namespace Game.Presentation.GameObjects.Factories
{
    public class CardViewFactory : ICardViewFactory
    {
        private readonly GameObject cardPrefab;

        public CardViewFactory(GameObject _cardPrefab)
        {
            cardPrefab = _cardPrefab ?? throw new System.ArgumentNullException(nameof(_cardPrefab),
                "Card prefab cannot be null");
        }

        public CardView Create(AbilityCard model, Vector3 spawnPoint)
        {
            if (model == null)
                throw new System.ArgumentNullException(nameof(model), "Cannot create CardView with null model");

            if (cardPrefab == null)
                throw new System.InvalidOperationException("Card prefab is null. Factory was not properly initialized.");

            var obj = Object.Instantiate(cardPrefab, spawnPoint, Quaternion.identity);
            if (obj == null)
                throw new System.InvalidOperationException("Failed to instantiate card prefab");

            var cardView = obj.GetComponent<CardView>();
            if (cardView == null)
            {
                Object.Destroy(obj); // Clean up the failed instantiation
                throw new System.InvalidOperationException(
                    $"CardView component not found on prefab '{cardPrefab.name}'. " +
                    "Ensure the prefab has a CardView component attached.");
            }

            cardView.Bind(model);
            return cardView;
        }

        public CardView Create(AbilityCard model, Vector3 spawnPoint, float scale)
        {
            var newCardView = Create(model, spawnPoint);
            newCardView.transform.localScale = new Vector2(scale, scale);
            return newCardView;
        }
    }
}