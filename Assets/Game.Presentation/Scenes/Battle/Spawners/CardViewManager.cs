using System.Collections;
using System.Collections.Generic;
using Game.Presentation.Shared.Views;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Domain.Entities;
using Game.Presentation.Shared.Factories;
using Game.Presentation.Core.Interfaces;
using UnityEngine;

namespace Game.Presentation.Scenes.Battle.Spawners
{
    public class CardViewManager : MonoBehaviour
    {
        [SerializeField] Canvas _rootCanvas;
        private ICardViewFactory _cardFactory;
        private Dictionary<MonsterEntity, CardView> _drawnCardViews = new();

        public void Initialize(ICardViewFactory cardFactory)
        {
            _cardFactory = cardFactory;
        }

        public (List<CardView> cardViews, Dictionary<CardView, Vector2> deckIconPositions) CreateCardsForDraw(IReadOnlyList<CardsDrawnEvent.DrawnCard> drawnCards, IViewRegistryService viewRegistry)
        {
            var cardViews = new List<CardView>();
            var deckIconPositions = new Dictionary<CardView, Vector2>();

            // Clear any previous drawn card views
            ClearAllDrawnCards();

            if (drawnCards.Count == 0) return (cardViews, deckIconPositions);

            // Calculate uniform height based on first monster + 3f
            float? uniformCardHeight = null;
            foreach (var drawnCard in drawnCards)
            {
                viewRegistry.TryGet(drawnCard.Monster.Id, out MonsterView monsterView);
                if (monsterView != null)
                {
                    uniformCardHeight = monsterView.transform.position.y + 2f;
                    break;
                }
            }

            if (!uniformCardHeight.HasValue)
            {
                Debug.LogWarning("Could not determine uniform card height - no valid monster views found");
                return (cardViews, deckIconPositions);
            }

            // Create card views for all drawn cards
            int cardIndex = 0;
            foreach (var drawnCard in drawnCards)
            {
                // Find the monster's view to get horizontal position
                viewRegistry.TryGet(drawnCard.Monster.Id, out MonsterView monsterView);
                if (monsterView == null)
                {
                    Debug.LogWarning($"Could not find view for monster {drawnCard.Monster.MonsterName}");
                    continue;
                }

                // Get world position using monster's X/Z but uniform Y height
                var worldPosition = new Vector3(monsterView.transform.position.x, uniformCardHeight.Value, monsterView.transform.position.z);
                
                // Convert world position to canvas position
                var canvasPosition = WorldToCanvasPosition(worldPosition);
                
                // Add some offset above the uniform position
                var finalCanvasPosition = canvasPosition + new Vector2(0, 150f); // Move cards up 150 pixels above uniform height
                Debug.Log($"Using final position: {finalCanvasPosition} for card {cardIndex} (converted from world: {worldPosition})");
                
                // Calculate deck icon canvas position for animation
                var deckIconWorldPos = monsterView.DeckIconWorldPosition;
                var deckIconCanvasPos = WorldToCanvasPosition(deckIconWorldPos);
                
                // Spawn card at canvas position 
                var cardView = _cardFactory.Create(drawnCard.Card, finalCanvasPosition, 1.25f);
                cardView.transform.SetParent(_rootCanvas.transform, false);
                
                // Use the properly converted position
                cardView.GetComponent<RectTransform>().anchoredPosition = finalCanvasPosition;
                cardViews.Add(cardView);
                
                // Store deck icon position for animation
                deckIconPositions[cardView] = deckIconCanvasPos;
                
                // Store the card view for later reuse
                _drawnCardViews[drawnCard.Monster] = cardView;
                
                cardIndex++;
            }

            return (cardViews, deckIconPositions);
        }

        public CardView GetDrawnCard(MonsterEntity monster)
        {
            return _drawnCardViews.TryGetValue(monster, out CardView cardView) ? cardView : null;
        }

        public CardView TakeDrawnCard(MonsterEntity monster)
        {
            if (_drawnCardViews.TryGetValue(monster, out CardView cardView))
            {
                _drawnCardViews.Remove(monster);
                return cardView;
            }
            return null;
        }

        public void ClearAllDrawnCards()
        {
            foreach (var cardView in _drawnCardViews.Values)
            {
                if (cardView != null)
                    Destroy(cardView.gameObject);
            }
            _drawnCardViews.Clear();
        }

        public void DestroyCardForMonster(MonsterEntity monster)
        {
            if (_drawnCardViews.TryGetValue(monster, out CardView cardView))
            {
                _drawnCardViews.Remove(monster);
                if (cardView != null)
                {
                    Debug.Log($"Fading out card for dead monster: {monster.MonsterName}");
                    StartCoroutine(FadeOutAndDestroyCard(cardView));
                }
            }
        }

        private IEnumerator FadeOutAndDestroyCard(CardView cardView)
        {
            if (cardView == null) yield break;

            // Get or add CanvasGroup for alpha control
            var canvasGroup = cardView.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();

            float fadeDuration = 0.5f;
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            // Fade out the card
            while (elapsed < fadeDuration && cardView != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            // Destroy the card
            if (cardView != null)
            {
                Debug.Log($"Card fade complete, destroying GameObject");
                Destroy(cardView.gameObject);
            }
        }

        public CardView CreateCardForAction(MonsterEntity caster, Domain.Entities.Abilities.AbilityCard card, IViewRegistryService viewRegistry)
        {
            // Fallback method for creating cards when pre-drawn card is not available
            viewRegistry.TryGet(caster.Id, out MonsterView casterView);
            if (casterView == null)
            {
                Debug.LogWarning($"Could not find view for caster {caster.MonsterName}");
                return null;
            }
            
            // Get world position above caster's head
            var worldPosition = casterView.transform.position + new Vector3(0, 3f, 0);
            var canvasPosition = WorldToCanvasPosition(worldPosition);
            
            // Create card and position it on canvas
            var cardView = _cardFactory.Create(card, Vector3.zero);
            cardView.transform.SetParent(_rootCanvas.transform, false);
            cardView.GetComponent<RectTransform>().anchoredPosition = canvasPosition;
            
            return cardView;
        }
        
        private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
        {
            // Get the camera to use for conversion
            var camera = GetCameraForCanvas();
            
            Vector2 canvasPosition = Vector2.zero;
            
            if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // For Screen Space Overlay, we need to use the main camera for world-to-screen conversion
                var mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("No main camera found for Screen Space Overlay conversion!");
                    return Vector2.zero;
                }
                
                // Convert world position to screen position using main camera
                var screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
                
                // Convert screen position to canvas position (use null camera for overlay)
                var canvasRect = _rootCanvas.GetComponent<RectTransform>();
                bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    null, // Always null for Screen Space Overlay
                    out canvasPosition
                );
                
                if (!success)
                {
                    Debug.LogWarning($"Failed to convert world position {worldPosition} to canvas position!");
                    return Vector2.zero;
                }
            }
            return canvasPosition;
        }
        
        private Camera GetCameraForCanvas()
        {
            // First check if canvas has a specific camera assigned
            if (_rootCanvas.worldCamera != null)
            {
                return _rootCanvas.worldCamera;
            }
            
            // For Screen Space - Overlay, we don't need a camera (use null)
            if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }
            
            // For Screen Space - Camera, try to find the camera
            if (_rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                return _rootCanvas.worldCamera ?? Camera.main;
            }
            
            // Fallback to main camera
            return Camera.main;
        }

        public int GetDrawnCardCount()
        {
            return _drawnCardViews.Count;
        }

        public void OnDestroy()
        {
            ClearAllDrawnCards();
        }
    }
}