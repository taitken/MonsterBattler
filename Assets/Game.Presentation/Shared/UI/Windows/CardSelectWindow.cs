using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Application.Interfaces;
using Game.Core;
using Game.Presentation.Shared.Views;
using Game.Application.Repositories;
using Game.Presentation.Shared.Factories;
using System;
using Game.Application.DTOs.Rewards;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using Game.Application.Messaging.Commands;
using Game.Application.Messaging;

namespace Game.Presentation.Shared.UI.Windows
{
    public class CardSelectWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private RectTransform _cardChoiceContainer;
        [SerializeField] private RectTransform _monsterSelectContainer;
        [SerializeField] private RectTransform _cardPosition;
        [SerializeField] private RectTransform _selectedCardPosition;
        [SerializeField] private Image _monsterPosition;
        [SerializeField] private Image _monsterPositionLeft;
        [SerializeField] private Image _monsterPositionRight;
        [SerializeField] private RectTransform _topRightMonsterPosition;
        [SerializeField] private CardSelectWindowAnimator _animator;

        private IPlayerDataRepository _playerDataRepository;
        private IMonsterSpriteProvider _spriteProvider;
        private ICardViewFactory _cardViewFactory;
        private IEventBus _eventBus;
        private CardReward _cardReward;
        private List<CardView> _createdCards = new List<CardView>();
        private List<IDisposable> _subscriptions = new List<IDisposable>();
        private int _selectedMonsterIndex = -1;
        private CardView _selectedCard;
        private CardView _selectedMonsterCard;

        void Awake()
        {
            _playerDataRepository = ServiceLocator.Get<IPlayerDataRepository>();
            _spriteProvider = ServiceLocator.Get<IMonsterSpriteProvider>();
            _cardViewFactory = ServiceLocator.Get<ICardViewFactory>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            // Set default visibility
            _cardChoiceContainer.gameObject.SetActive(true);
            _monsterSelectContainer.gameObject.SetActive(false);
            
            // Add click handlers to monster images
            SetupMonsterClickHandlers();
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
            // Animate card selection using the animator
            StartCoroutine(AnimateCardSelectionAndShowMonsters(selectedCard));
        }

        private IEnumerator AnimateCardSelectionAndShowMonsters(CardView selectedCard)
        {
            // Store the selected card reference
            _selectedCard = selectedCard;
            
            // Start the card selection animation
            yield return StartCoroutine(_animator.AnimateCardSelection(selectedCard, _selectedCardPosition, _createdCards));

            // Show monster select container after animation completes
            _monsterSelectContainer.gameObject.SetActive(true);

            // Display current monsters
            yield return StartCoroutine(DisplayCurrentMonstersCoroutine());
        }

        private IEnumerator DisplayCurrentMonstersCoroutine()
        {
            var playerMonsters = _playerDataRepository.GetPlayerTeam();

            // Display up to 3 monsters
            if (playerMonsters.Count > 0 && _monsterPosition != null)
            {
                var spriteTask = _spriteProvider.GetMonsterSpriteAsync<Sprite>(playerMonsters[0].Type);
                yield return new WaitUntil(() => spriteTask.IsCompleted);
                _monsterPosition.sprite = spriteTask.Result;
                _monsterPosition.gameObject.SetActive(true);
            }

            if (playerMonsters.Count > 1 && _monsterPositionLeft != null)
            {
                var spriteTask = _spriteProvider.GetMonsterSpriteAsync<Sprite>(playerMonsters[1].Type);
                yield return new WaitUntil(() => spriteTask.IsCompleted);
                _monsterPositionLeft.sprite = spriteTask.Result;
                _monsterPositionLeft.gameObject.SetActive(true);
            }

            if (playerMonsters.Count > 2 && _monsterPositionRight != null)
            {
                var spriteTask = _spriteProvider.GetMonsterSpriteAsync<Sprite>(playerMonsters[2].Type);
                yield return new WaitUntil(() => spriteTask.IsCompleted);
                _monsterPositionRight.sprite = spriteTask.Result;
                _monsterPositionRight.gameObject.SetActive(true);
            }
        }
        
        private void SetupMonsterClickHandlers()
        {
            // Add Event Triggers for monster clicks
            AddMonsterClickHandler(_monsterPosition, 0);
            AddMonsterClickHandler(_monsterPositionLeft, 1);
            AddMonsterClickHandler(_monsterPositionRight, 2);
        }
        
        private void AddMonsterClickHandler(Image monsterImage, int monsterIndex)
        {
            if (monsterImage == null) return;
            
            var eventTrigger = monsterImage.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = monsterImage.gameObject.AddComponent<EventTrigger>();
            
            var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            clickEntry.callback.AddListener((data) => OnMonsterClicked(monsterImage, monsterIndex));
            eventTrigger.triggers.Add(clickEntry);
        }
        
        private void OnMonsterClicked(Image selectedMonster, int monsterIndex)
        {
            _selectedMonsterIndex = monsterIndex;
            StartCoroutine(AnimateMonsterSelectionAndShowCards(selectedMonster));
        }
        
        private System.Collections.IEnumerator AnimateMonsterSelectionAndShowCards(Image selectedMonster)
        {
            var allMonsters = new List<Image> { _monsterPosition, _monsterPositionLeft, _monsterPositionRight };
            
            // Animate monster selection
            yield return StartCoroutine(_animator.AnimateMonsterSelection(selectedMonster, _topRightMonsterPosition, allMonsters));
            
            // Show all cards for the selected monster
            yield return StartCoroutine(DisplayMonsterCards());
        }
        
        private IEnumerator DisplayMonsterCards()
        {
            if (_selectedMonsterIndex < 0) yield break;
            
            var playerMonsters = _playerDataRepository.GetPlayerTeam();
            if (_selectedMonsterIndex >= playerMonsters.Count) yield break;
            
            var selectedMonster = playerMonsters[_selectedMonsterIndex];
            
            // Clear existing cards but preserve the selected card
            ClearCreatedCardsExceptSelected();
            _cardChoiceContainer.gameObject.SetActive(true);
            
            // Create card views for all cards in the selected monster's deck
            var monsterCards = selectedMonster.AbilityDeck.AllCards;
            
            // Calculate dynamic spacing based on number of cards
            float cardSpacing = monsterCards.Count <= 3 ? 350f : 275f;
            float totalWidth = (monsterCards.Count - 1) * cardSpacing;
            float startX = -totalWidth / 2f;
            
            for (int i = 0; i < monsterCards.Count; i++)
            {
                var card = monsterCards[i];
                var cardView = _cardViewFactory.Create(card, new Vector3(1f, 1f, 0), 1.2f);
                
                cardView.transform.SetParent(_cardChoiceContainer, false);
                cardView.GetComponent<RectTransform>().localPosition = _cardPosition.localPosition + new Vector3(startX + i * cardSpacing, -50f, 0);
                
                // Subscribe to monster card click events
                var subscription = cardView.SubscribeToClick(OnMonsterCardSelected);
                _subscriptions.Add(subscription);
                
                _createdCards.Add(cardView);
            }
            
            yield return null;
        }
        
        private void ClearCreatedCardsExceptSelected()
        {
            // Dispose of all subscriptions
            foreach (var subscription in _subscriptions)
            {
                subscription?.Dispose();
            }
            _subscriptions.Clear();

            // Destroy card GameObjects except the selected card
            for (int i = _createdCards.Count - 1; i >= 0; i--)
            {
                var cardView = _createdCards[i];
                if (cardView != null && cardView != _selectedCard)
                {
                    DestroyImmediate(cardView.gameObject);
                    _createdCards.RemoveAt(i);
                }
            }
        }
        
        private void OnMonsterCardSelected(CardView selectedMonsterCard)
        {
            if (_selectedCard == null || _selectedMonsterIndex < 0) return;
            
            _selectedMonsterCard = selectedMonsterCard;
            
            var playerMonsters = _playerDataRepository.GetPlayerTeam();
            if (_selectedMonsterIndex >= playerMonsters.Count) return;
            
            var selectedMonster = playerMonsters[_selectedMonsterIndex];
            
            // Create and publish the ReplaceCardCommand
            var replaceCardCommand = new ReplaceCardCommand(
                cardToAdd: _selectedCard.model,
                cardToRemove: selectedMonsterCard.model,
                monsterId: selectedMonster.Id
            );
            
            _eventBus.Publish(replaceCardCommand);
            
            // Hide the window after command is published
            Hide();
        }

        void OnDestroy()
        {
            // Clean up created cards and subscriptions
            ClearCreatedCards();
        }
    }
}

