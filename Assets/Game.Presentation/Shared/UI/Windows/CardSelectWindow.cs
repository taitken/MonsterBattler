using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Application.Interfaces;
using Game.Core;
using System.Threading.Tasks;
using Game.Presentation.Shared.Views;
using Game.Application.Repositories;
using Game.Presentation.Shared.Factories;
using System.Collections.Generic;
using System;
using Game.Application.DTOs.Rewards;

public class CardSelectWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private RectTransform _cardChoiceContainer;
    [SerializeField] private RectTransform _monsterSelectContainer;
    [SerializeField] private RectTransform _cardPosition;
    [SerializeField] private Image _monsterPosition;
    [SerializeField] private Image _monsterPositionLeft;
    [SerializeField] private Image _monsterPositionRight;
    
    private IPlayerDataRepository _playerDataRepository;
    private IMonsterSpriteProvider _spriteProvider;
    private ICardViewFactory _cardViewFactory;
    private CardReward _cardReward;
    private List<CardView> _createdCards = new List<CardView>();
    private List<IDisposable> _subscriptions = new List<IDisposable>();
    
    void Awake()
    {
        _playerDataRepository = ServiceLocator.Get<IPlayerDataRepository>();
        _spriteProvider = ServiceLocator.Get<IMonsterSpriteProvider>();
        _cardViewFactory = ServiceLocator.Get<ICardViewFactory>();
        
        // Set default visibility
        _cardChoiceContainer.gameObject.SetActive(true);
        _monsterSelectContainer.gameObject.SetActive(false);
    }
    
    public void Show(CardReward cardReward)
    {
        _cardReward = cardReward;
        gameObject.SetActive(true);
        
        // Ensure correct initial state
        _cardChoiceContainer.gameObject.SetActive(true);
        _monsterSelectContainer.gameObject.SetActive(false);
        
        // Create cards using factory
        PopulateCards();
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
        
        // Clear cards when hiding
        ClearCreatedCards();
    }
    
    private void PopulateCards()
    {
        // Clear existing cards and subscriptions
        ClearCreatedCards();
        
        // Create card views for each card
        for (int i = 0; i < _cardReward.CardChoices.Count; i++)
        {
            var card = _cardReward.CardChoices[i];
            var cardView = _cardViewFactory.Create(card, new Vector3(1f, 1f, 0), 1.25f);
            
            // Set parent to card choice container  
            cardView.transform.SetParent(_cardChoiceContainer, false);
            cardView.GetComponent<RectTransform>().localPosition = _cardPosition.localPosition + new Vector3(i * 300f - 300f, 0, 0);
            
            // Subscribe to card click events
            var subscription = cardView.SubscribeToClick(OnCardSelected);
            _subscriptions.Add(subscription);
            
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
    
    
    private async void OnCardSelected(CardView selectedCard)
    {
        // Hide card choice container and show monster select container
        _cardChoiceContainer.gameObject.SetActive(false);
        _monsterSelectContainer.gameObject.SetActive(true);
        
        // Display current monsters
        await DisplayCurrentMonsters();
    }
    
    private async Task DisplayCurrentMonsters()
    {
        var playerMonsters = _playerDataRepository.GetPlayerTeam();
        
        // Display up to 3 monsters
        if (playerMonsters.Count > 0 && _monsterPosition != null)
        {
            _monsterPosition.sprite = await _spriteProvider.GetMonsterSpriteAsync<Sprite>(playerMonsters[0].Type);
            _monsterPosition.gameObject.SetActive(true);
        }
        
        if (playerMonsters.Count > 1 && _monsterPositionLeft != null)
        {
            _monsterPositionLeft.sprite = await _spriteProvider.GetMonsterSpriteAsync<Sprite>(playerMonsters[1].Type);
            _monsterPositionLeft.gameObject.SetActive(true);
        }
        
        if (playerMonsters.Count > 2 && _monsterPositionRight != null)
        {
            _monsterPositionRight.sprite = await _spriteProvider.GetMonsterSpriteAsync<Sprite>(playerMonsters[2].Type);
            _monsterPositionRight.gameObject.SetActive(true);
        }
    }
    
    void OnDestroy()
    {
        // Clean up created cards and subscriptions
        ClearCreatedCards();
    }
}
