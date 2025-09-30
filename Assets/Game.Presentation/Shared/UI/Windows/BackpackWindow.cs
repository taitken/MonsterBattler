using UnityEngine;
using Game.Application.Repositories;
using Game.Presentation.Shared.Factories;
using Game.Presentation.Shared.Views;
using Game.Core;
using System.Collections.Generic;
using System;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Application.Interfaces;
using UnityEngine.UI;
using System.Threading.Tasks;
using DG.Tweening;
using Game.Presentation.Shared.UI.Components;
using Game.Application.Messaging;
using Game.Application.Messaging.Commands;
using Game.Application.Messaging.Events.Player;
using System.Linq;
using Game.Domain.Entities.Abilities;
using Game.Domain.Entities.Player;

public class BackpackWindow : MonoBehaviour
{
    public const float CARD_SCALE = 0.7f; // Made public for CardDragBehavior access

    [SerializeField] private RectTransform _newItemPosition;
    [SerializeField] private RectTransform _monsterPosition1;
    [SerializeField] private RectTransform _monsterPosition2;
    [SerializeField] private RectTransform _monsterPosition3;
    [SerializeField] private RectTransform _backpackPosition1;
    [SerializeField] private RectTransform _backpackPosition2;
    [SerializeField] private RectTransform _backpackPosition3;
    [SerializeField] private RectTransform _backpackPosition4;
    [SerializeField] private RectTransform _backpackPosition5;
    
    private IPlayerDataRepository _playerDataRepository;
    private ICardViewFactory _cardViewFactory;
    private IMonsterSpriteProvider _spriteProvider;
    private IEventBus _eventBus;
    private List<CardView> _createdCards = new List<CardView>();
    private List<GameObject> _createdMonsterIcons = new List<GameObject>();
    private List<IDisposable> _subscriptions = new List<IDisposable>();
    private Dictionary<CardView, CardDragBehavior> _cardDragBehaviors = new Dictionary<CardView, CardDragBehavior>();

    // Card position tracking
    private Dictionary<Guid, (CardView view, ContainerType containerType, int monsterIndex, int cardIndex)> _cardPositions = new Dictionary<Guid, (CardView, ContainerType, int, int)>();

    public enum ContainerType
    {
        MonsterDeck,
        Backpack
    }

    void Awake()
    {
        _playerDataRepository = ServiceLocator.Get<IPlayerDataRepository>();
        _cardViewFactory = ServiceLocator.Get<ICardViewFactory>();
        _spriteProvider = ServiceLocator.Get<IMonsterSpriteProvider>();
        _eventBus = ServiceLocator.Get<IEventBus>();

        // Subscribe to cards updated event to refresh UI when data changes
        Debug.Log("BackpackWindow: Subscribing to CardsUpdatedEvent");
        var subscription = _eventBus.Subscribe<CardsUpdatedEvent>(OnCardsUpdated);
        _subscriptions.Add(subscription);
        Debug.Log($"BackpackWindow: CardsUpdatedEvent subscription created: {subscription != null}");
    }

    public void InitializeBackpackDisplay()
    {
        // Clear existing UI elements
        ClearCreatedElements();

        // Display monster party on the left
        DisplayMonsterParty();

        // Display backpack items in bottom right
        DisplayBackpackItems();
    }

    private async void DisplayMonsterParty()
    {
        var playerTeam = _playerDataRepository.GetPlayerTeam();
        var monsterPositions = new RectTransform[] { _monsterPosition1, _monsterPosition2, _monsterPosition3 };

        for (int i = 0; i < playerTeam.Count && i < monsterPositions.Length; i++)
        {
            var monster = playerTeam[i];
            var position = monsterPositions[i];

            // Create lightweight monster icon using just sprite
            await CreateMonsterIcon(monster, position);

            // Display monster's deck cards next to the monster
            DisplayMonsterDeckCards(monster, position, i);
        }
    }

    private async Task CreateMonsterIcon(MonsterEntity monster, RectTransform position)
    {
        // Create a simple GameObject with Image component
        var iconObject = new GameObject($"MonsterIcon_{monster.MonsterName}");
        iconObject.transform.SetParent(transform, false);

        var image = iconObject.AddComponent<Image>();
        var rectTransform = iconObject.GetComponent<RectTransform>();

        // Set position and scale
        rectTransform.localPosition = position.localPosition;
        rectTransform.localScale = Vector3.one * 0.6f;
        rectTransform.sizeDelta = new Vector2(200, 200); // Set a reasonable size

        // Load and set the sprite
        try
        {
            var sprite = await _spriteProvider.GetMonsterSpriteAsync<Sprite>(monster.Type);
            if (sprite != null)
            {
                image.sprite = sprite;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load sprite for monster {monster.MonsterName}: {ex.Message}");
        }

        _createdMonsterIcons.Add(iconObject);
    }

    private void DisplayMonsterDeckCards(MonsterEntity monster, RectTransform monsterPosition, int monsterIndex)
    {
        if (monster.AbilityDeck == null) return;

        var deckCards = monster.AbilityDeck.AllCards;

        // Limit cards displayed to avoid extending past screen center
        int maxCards = Mathf.Min(deckCards.Count, 5);

        for (int i = 0; i < maxCards; i++)
        {
            var card = deckCards[i];
            var cardView = _cardViewFactory.Create(card, Vector3.one, CARD_SCALE);

            cardView.transform.SetParent(transform, false);

            // Calculate position using helper method
            Vector3 cardPosition = CalculateMonsterCardPosition(monsterIndex, i);
            cardView.GetComponent<RectTransform>().localPosition = cardPosition;

            // Track this card's position
            _cardPositions[card.Id] = (cardView, ContainerType.MonsterDeck, monsterIndex, i);

            // Add drag behavior to monster deck cards
            AddDragBehaviorToCard(cardView, CardOriginType.MonsterDeck);

            _createdCards.Add(cardView);
        }
    }

    private void DisplayBackpackItems()
    {
        var backpack = _playerDataRepository.GetBackpack();
        var backpackPositions = new RectTransform[]
        {
            _backpackPosition1, _backpackPosition2, _backpackPosition3,
            _backpackPosition4, _backpackPosition5
        };

        // Iterate through all 5 slots
        for (int i = 0; i < backpackPositions.Length; i++)
        {
            var card = backpack.GetCardAt(i);

            // Only create card view if slot is occupied
            if (card != null)
            {
                var cardView = _cardViewFactory.Create(card, Vector3.one, CARD_SCALE);

                cardView.transform.SetParent(transform, false);

                // Calculate position using helper method
                Vector3 cardPosition = CalculateBackpackCardPosition(i);
                cardView.GetComponent<RectTransform>().localPosition = cardPosition;

                // Track this card's position
                _cardPositions[card.Id] = (cardView, ContainerType.Backpack, 0, i);

                // Add drag behavior to backpack cards
                AddDragBehaviorToCard(cardView, CardOriginType.Backpack);

                _createdCards.Add(cardView);
            }
        }
    }

    private void AddDragBehaviorToCard(CardView cardView, CardOriginType originType)
    {
        var dragBehavior = cardView.gameObject.AddComponent<CardDragBehavior>();
        dragBehavior.Initialize(cardView, this, originType);
        _cardDragBehaviors[cardView] = dragBehavior;
    }

    // Called by CardDragBehavior when drag starts
    public void OnCardDragStarted(CardView cardView, CardOriginType originType)
    {
        Debug.Log($"Drag started for {cardView.name} from {originType}");
        // TODO: Add visual feedback for valid drop zones
    }

    // Called by CardDragBehavior when card is dropped
    public bool OnCardDropped(CardView draggedCard, CardOriginType draggedOriginType, CardView targetCard, int targetSlotIndex = -1)
    {
        // Handle empty slot drops
        if (targetCard == null && targetSlotIndex >= 0)
        {
            Debug.Log($"Dropped on empty slot {targetSlotIndex}");
            return HandleEmptySlotDrop(draggedCard, draggedOriginType, targetSlotIndex);
        }

        if (targetCard == null)
        {
            Debug.Log("Dropped on empty space - invalid drop");
            return false;
        }

        // Get target card's origin type
        if (!_cardDragBehaviors.TryGetValue(targetCard, out var targetDragBehavior))
        {
            Debug.Log("Target card has no drag behavior - invalid drop");
            return false;
        }

        var targetOriginType = targetDragBehavior.GetOriginType();

        // Allow all swaps: Backpack <-> Backpack, MonsterDeck <-> MonsterDeck, and cross-type swaps
        Debug.Log($"Valid swap: {draggedOriginType} <-> {targetOriginType}");

        // Publish card swap command using the origin types directly
        var swapCommand = new CardSwapCommand(draggedCard.model.Id, draggedOriginType, targetCard.model.Id, targetOriginType);
        Debug.Log($"Publishing CardSwapCommand: {swapCommand.Card1Id} ({swapCommand.Card1Location}) <-> {swapCommand.Card2Id} ({swapCommand.Card2Location})");
        _eventBus.Publish(swapCommand);
        Debug.Log("CardSwapCommand published");

        return true;
    }

    private bool HandleEmptySlotDrop(CardView draggedCard, CardOriginType draggedOriginType, int targetSlotIndex)
    {
        // Monster deck cards cannot be moved to empty slots (deck size must remain constant)
        if (draggedOriginType == CardOriginType.MonsterDeck)
        {
            Debug.Log("Monster deck cards cannot be moved to empty slots - they can only swap with other cards");
            return false;
        }

        // Only backpack cards can be moved to empty backpack slots
        if (draggedOriginType != CardOriginType.Backpack)
        {
            Debug.Log("Only backpack cards can be moved to empty backpack slots");
            return false;
        }

        // Verify the target slot is actually empty
        var backpack = _playerDataRepository.GetBackpack();
        if (!backpack.IsSlotEmpty(targetSlotIndex))
        {
            Debug.Log($"Target slot {targetSlotIndex} is not empty");
            return false;
        }

        // Publish card move command - with slot-based system, target index is exactly the slot index
        var moveCommand = new CardMoveCommand(draggedCard.model.Id, draggedOriginType, CardOriginType.Backpack, targetSlotIndex);
        Debug.Log($"Publishing CardMoveCommand: {moveCommand.CardId} from {moveCommand.SourceLocation} to {moveCommand.TargetLocation}[{moveCommand.TargetIndex}]");
        _eventBus.Publish(moveCommand);
        Debug.Log("CardMoveCommand published");

        return true;
    }

    private void OnCardsUpdated(CardsUpdatedEvent cardsUpdatedEvent)
    {
        Debug.Log("*** OnCardsUpdated CALLED! Refreshing card positions");
        RefreshCardPositions();
    }

    private void RefreshCardPositions()
    {
        // Get fresh data from repository
        var playerTeam = _playerDataRepository.GetPlayerTeam();
        var backpack = _playerDataRepository.GetBackpack();

        // Update monster deck cards
        RefreshMonsterDeckCards(playerTeam);

        // Update backpack cards
        RefreshBackpackCards(backpack);
    }

    private void RefreshMonsterDeckCards(List<MonsterEntity> playerTeam)
    {
        // Build a set of cards that should exist
        var expectedCards = new Dictionary<Guid, (MonsterEntity monster, int monsterIndex, int cardIndex)>();

        for (int monsterIndex = 0; monsterIndex < playerTeam.Count && monsterIndex < 3; monsterIndex++)
        {
            var monster = playerTeam[monsterIndex];
            if (monster.AbilityDeck != null)
            {
                var deckCards = monster.AbilityDeck.AllCards;
                int maxCards = Mathf.Min(deckCards.Count, 5);

                for (int cardIndex = 0; cardIndex < maxCards; cardIndex++)
                {
                    var card = deckCards[cardIndex];
                    expectedCards[card.Id] = (monster, monsterIndex, cardIndex);
                }
            }
        }

        // Find existing monster deck cards
        var existingMonsterCards = _cardPositions.Where(kvp => kvp.Value.containerType == ContainerType.MonsterDeck).ToList();

        // Handle cards that are no longer in monster decks
        foreach (var kvp in existingMonsterCards)
        {
            if (!expectedCards.ContainsKey(kvp.Key))
            {
                // Check if this card moved to backpack (don't remove if it did)
                var backpack = _playerDataRepository.GetBackpack();
                var cardMovedToBackpack = backpack.Cards.Any(c => c != null && c.Id == kvp.Key);

                if (!cardMovedToBackpack)
                {
                    // Card was truly removed, not just moved
                    RemoveCardView(kvp.Key);
                }
                // If it moved to backpack, leave it for RefreshBackpackCards to handle
            }
        }

        // Update or create cards
        foreach (var expectedCard in expectedCards)
        {
            var cardId = expectedCard.Key;
            var (monster, monsterIndex, cardIndex) = expectedCard.Value;
            var targetPosition = CalculateMonsterCardPosition(monsterIndex, cardIndex);

            if (_cardPositions.TryGetValue(cardId, out var existingData))
            {
                // Card exists somewhere, animate to monster deck position
                var cardView = existingData.view;
                var currentPos = cardView.GetComponent<RectTransform>().localPosition;

                Debug.Log($"Moving card {cardView.name} from {existingData.containerType} at {currentPos} to MonsterDeck at {targetPosition}");
                AnimateCardToPosition(cardView, targetPosition);

                // Update tracking data
                _cardPositions[cardId] = (cardView, ContainerType.MonsterDeck, monsterIndex, cardIndex);

                // Update drag behavior origin type
                if (_cardDragBehaviors.TryGetValue(cardView, out var dragBehavior))
                {
                    dragBehavior.SetOriginType(CardOriginType.MonsterDeck);
                }
            }
            else
            {
                // Card doesn't exist, create it
                CreateMonsterDeckCard(monster.AbilityDeck.AllCards[cardIndex], targetPosition, monsterIndex, cardIndex);
            }
        }
    }

    private void RefreshBackpackCards(BackpackEntity backpack)
    {
        var playerTeam = _playerDataRepository.GetPlayerTeam();

        // Find existing backpack cards
        var existingBackpackCards = _cardPositions.Where(kvp => kvp.Value.containerType == ContainerType.Backpack).ToList();

        // Handle cards that are no longer in backpack
        foreach (var kvp in existingBackpackCards)
        {
            var cardId = kvp.Key;
            bool cardStillInBackpack = false;

            // Check all 5 slots
            for (int i = 0; i < 5; i++)
            {
                var slotCard = backpack.GetCardAt(i);
                if (slotCard?.Id == cardId)
                {
                    cardStillInBackpack = true;
                    break;
                }
            }

            if (!cardStillInBackpack)
            {
                // Check if this card moved to a monster deck (don't remove if it did)
                var cardMovedToMonsterDeck = false;
                foreach (var monster in playerTeam)
                {
                    if (monster.AbilityDeck?.AllCards.Any(c => c.Id == cardId) == true)
                    {
                        cardMovedToMonsterDeck = true;
                        break;
                    }
                }

                if (!cardMovedToMonsterDeck)
                {
                    // Card was truly removed, not just moved
                    RemoveCardView(cardId);
                }
                // If it moved to monster deck, leave it for RefreshMonsterDeckCards to handle
            }
        }

        // Update or create backpack cards for all 5 slots
        for (int slotIndex = 0; slotIndex < 5; slotIndex++)
        {
            var card = backpack.GetCardAt(slotIndex);
            var targetPosition = CalculateBackpackCardPosition(slotIndex);

            if (card != null)
            {
                if (_cardPositions.TryGetValue(card.Id, out var existingData))
                {
                    // Card exists somewhere, animate to backpack position
                    var cardView = existingData.view;
                    var currentPos = cardView.GetComponent<RectTransform>().localPosition;

                    Debug.Log($"Moving card {cardView.name} from {existingData.containerType} at {currentPos} to Backpack slot {slotIndex} at {targetPosition}");
                    AnimateCardToPosition(cardView, targetPosition);

                    // Update tracking data
                    _cardPositions[card.Id] = (cardView, ContainerType.Backpack, 0, slotIndex);

                    // Update drag behavior origin type
                    if (_cardDragBehaviors.TryGetValue(cardView, out var dragBehavior))
                    {
                        dragBehavior.SetOriginType(CardOriginType.Backpack);
                    }
                }
                else
                {
                    // Card doesn't exist, create it
                    CreateBackpackCard(card, targetPosition, slotIndex);
                }
            }
        }
    }

    private void CreateMonsterDeckCard(AbilityCard card, Vector3 position, int monsterIndex, int cardIndex)
    {
        var cardView = _cardViewFactory.Create(card, Vector3.one, CARD_SCALE);
        cardView.transform.SetParent(transform, false);
        cardView.GetComponent<RectTransform>().localPosition = position;

        // Track this card's position
        _cardPositions[card.Id] = (cardView, ContainerType.MonsterDeck, monsterIndex, cardIndex);

        // Add drag behavior
        AddDragBehaviorToCard(cardView, CardOriginType.MonsterDeck);

        _createdCards.Add(cardView);
        Debug.Log($"Created new monster deck card {cardView.name} at {position}");
    }

    private void CreateBackpackCard(AbilityCard card, Vector3 position, int cardIndex)
    {
        var cardView = _cardViewFactory.Create(card, Vector3.one, CARD_SCALE);
        cardView.transform.SetParent(transform, false);
        cardView.GetComponent<RectTransform>().localPosition = position;

        // Track this card's position
        _cardPositions[card.Id] = (cardView, ContainerType.Backpack, 0, cardIndex);

        // Add drag behavior
        AddDragBehaviorToCard(cardView, CardOriginType.Backpack);

        _createdCards.Add(cardView);
        Debug.Log($"Created new backpack card {cardView.name} at {position}");
    }

    private void AnimateCardToPosition(CardView cardView, Vector3 targetPosition)
    {
        if (_cardDragBehaviors.TryGetValue(cardView, out var dragBehavior))
        {
            dragBehavior.AnimateToPosition(targetPosition);
        }
        else
        {
            // Fallback if no drag behavior
            cardView.GetComponent<RectTransform>().DOLocalMove(targetPosition, 0.2f).SetEase(Ease.OutBack);
        }
    }

    private void RemoveCardView(Guid cardId)
    {
        if (_cardPositions.TryGetValue(cardId, out var cardData))
        {
            var cardView = cardData.view;

            // Remove from tracking
            _cardPositions.Remove(cardId);

            // Remove drag behavior
            if (_cardDragBehaviors.TryGetValue(cardView, out var dragBehavior))
            {
                _cardDragBehaviors.Remove(cardView);
                DestroyImmediate(dragBehavior);
            }

            // Remove from created cards list
            _createdCards.Remove(cardView);

            // Destroy the card
            DestroyImmediate(cardView.gameObject);

            Debug.Log($"Removed card view for card {cardId}");
        }
    }


    // Helper methods for calculating card positions
    private Vector3 CalculateMonsterCardPosition(int monsterIndex, int cardIndex)
    {
        var monsterPositions = new RectTransform[] { _monsterPosition1, _monsterPosition2, _monsterPosition3 };
        if (monsterIndex >= monsterPositions.Length) return Vector3.zero;

        var monsterPosition = monsterPositions[monsterIndex];
        float cardSpacing = 140f;
        float offsetX = 150f;

        return monsterPosition.localPosition + new Vector3(offsetX + (cardIndex * cardSpacing), 0, 0);
    }

    private Vector3 CalculateBackpackCardPosition(int backpackIndex)
    {
        var backpackPositions = new RectTransform[]
        {
            _backpackPosition1, _backpackPosition2, _backpackPosition3,
            _backpackPosition4, _backpackPosition5
        };

        if (backpackIndex >= backpackPositions.Length) return Vector3.zero;
        return backpackPositions[backpackIndex].localPosition;
    }
    
    private void ClearCreatedElements()
    {
        // Clean up drag behaviors
        foreach (var kvp in _cardDragBehaviors)
        {
            if (kvp.Value != null)
            {
                DestroyImmediate(kvp.Value);
            }
        }
        _cardDragBehaviors.Clear();

        // Clear position tracking
        _cardPositions.Clear();

        // Destroy card GameObjects
        foreach (var cardView in _createdCards)
        {
            if (cardView != null)
            {
                DestroyImmediate(cardView.gameObject);
            }
        }
        _createdCards.Clear();

        // Destroy monster icon GameObjects
        foreach (var monsterIcon in _createdMonsterIcons)
        {
            if (monsterIcon != null)
            {
                DestroyImmediate(monsterIcon);
            }
        }
        _createdMonsterIcons.Clear();
    }
    
    void OnDestroy()
    {
        ClearCreatedElements();

        // Dispose of all subscriptions when the window is destroyed
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
        _subscriptions.Clear();
    }
}
