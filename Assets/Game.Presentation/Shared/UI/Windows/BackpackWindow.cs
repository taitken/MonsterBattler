using UnityEngine;
using Game.Application.Repositories;
using Game.Presentation.Shared.Factories;
using Game.Presentation.Shared.Views;
using Game.Core;
using System.Collections.Generic;
using System;

public class BackpackWindow : MonoBehaviour
{
    [SerializeField] private RectTransform _cardPosition;
    
    private IPlayerDataRepository _playerDataRepository;
    private ICardViewFactory _cardViewFactory;
    private List<CardView> _createdCards = new List<CardView>();
    private List<IDisposable> _subscriptions = new List<IDisposable>();

    void Awake()
    {
        _playerDataRepository = ServiceLocator.Get<IPlayerDataRepository>();
        _cardViewFactory = ServiceLocator.Get<ICardViewFactory>();
    }

    public void InitializeBackpackDisplay()
    {
        // Clear existing cards
        ClearCreatedCards();
        
        var backpack = _playerDataRepository.GetBackpack();
        var backpackCards = backpack.Cards;
        
        if (backpackCards.Count == 0)
        {
            // No cards to display
            return;
        }
        
        // Calculate dynamic spacing and centering similar to CardSelectWindow
        float cardSpacing = backpackCards.Count <= 3 ? 350f : 275f;
        float totalWidth = (backpackCards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;
        
        // Create card views for each backpack card
        for (int i = 0; i < backpackCards.Count; i++)
        {
            var card = backpackCards[i];
            var cardView = _cardViewFactory.Create(card, Vector3.one, 1.0f);
            
            // Set parent and position relative to _cardPosition
            cardView.transform.SetParent(transform, false);
            cardView.GetComponent<RectTransform>().localPosition = _cardPosition.localPosition + new Vector3(startX + i * cardSpacing, 0, 0);
            
            // For now, just add to created cards list
            // Future: could add click handlers for card interaction
            _createdCards.Add(cardView);
        }
    }
    
    private void ClearCreatedCards()
    {
        // Dispose of all subscriptions
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
        _subscriptions.Clear();

        // Destroy card GameObjects
        foreach (var cardView in _createdCards)
        {
            if (cardView != null)
            {
                DestroyImmediate(cardView.gameObject);
            }
        }
        _createdCards.Clear();
    }
    
    void OnDestroy()
    {
        ClearCreatedCards();
    }
}
