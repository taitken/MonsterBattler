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
using Game.Domain.Structs;

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
        // Clear existing cards
        ClearCreatedCards();
        
        // Create card views for each card
        for (int i = 0; i < _cardReward.CardChoices.Count; i++)
        {
            var card = _cardReward.CardChoices[i];
            var spawnPosition = _cardPosition.position + new Vector3(i * 150f, 0, 0); // Space cards horizontally
            var cardView = _cardViewFactory.Create(card, spawnPosition);
            
            // Set parent to card choice container
            cardView.transform.SetParent(_cardChoiceContainer, false);
            
            // Add Button component for selection
            var button = cardView.GetComponent<Button>();
            if (button == null)
            {
                button = cardView.gameObject.AddComponent<Button>();
            }
            
            // Set up selection handler
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnCardSelected(cardView));
            
            _createdCards.Add(cardView);
        }
    }
    
    private void ClearCreatedCards()
    {
        foreach (var cardView in _createdCards)
        {
            if (cardView != null)
            {
                DestroyImmediate(cardView.gameObject);
            }
        }
        _createdCards.Clear();
    }
    
    private void SubscribeToCardSelectionEvents()
    {
        // This method is now replaced by PopulateCards which creates cards with buttons
        PopulateCards();
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
        // Clean up created cards
        ClearCreatedCards();
    }
}
