using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Domain.Entities.Abilities;
using Game.Application.Interfaces;
using Game.Core;
using System.Threading.Tasks;
using System;
using Game.Presentation.Services;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Domain.Enums;
using System.Collections.Generic;
using Game.Application.Messaging;

namespace Game.Presentation.Shared.Views
{
    public class CardView : MonoObject<AbilityCard>, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI _cardTitle;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [SerializeField] private Image _cardArt;
        private ICardArtProvider _spriteProvider;
        private IEventBus _eventBus;
        private IDisposable _runeFlashSubscription;
        private IReadOnlyList<RuneType> _currentFlashingRunes;

        public event Action<CardView> OnCardClicked;

        void Awake()
        {
            _spriteProvider = ServiceLocator.Get<ICardArtProvider>();
            _eventBus = ServiceLocator.Get<IEventBus>();
            _runeFlashSubscription = _eventBus.Subscribe<RuneFlashEvent>(OnRuneFlash);
        }

        protected override async void OnModelBound()
        {
            if (model == null)
            {
                throw new System.InvalidOperationException("Cannot bind null model to CardView");
            }

            await UpdateCardVisuals();
        }

        private async Task UpdateCardVisuals()
        {
            if (_cardTitle != null)
                _cardTitle.text = model.Name;

            if (_bodyText != null)
            {
                _bodyText.richText = true;
                _bodyText.color = Color.black;
                _bodyText.text = AbilityEffectDescriptionService.GenerateDescription(model.Effects, _currentFlashingRunes);
            }

            if (_cardArt != null)
                _cardArt.sprite = await _spriteProvider.GetCardArtAsync<Sprite>(model.Name);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnCardClicked?.Invoke(this);
        }

        private void OnRuneFlash(RuneFlashEvent runeFlashEvent)
        {
            _currentFlashingRunes = runeFlashEvent.FlashingRunes;

            // Update card description immediately to show amplified values
            if (model != null)
            {
                _ = UpdateCardVisuals();
            }
        }
        
        public IDisposable SubscribeToClick(Action<CardView> onClicked)
        {
            OnCardClicked += onClicked;
            return new UnsubscribeAction(() => OnCardClicked -= onClicked);
        }

        private void OnDestroy()
        {
            if (_viewRegistry != null && model != null)
                _viewRegistry.Unregister(model.Id);

            // Dispose of event subscriptions
            _runeFlashSubscription?.Dispose();
            OnCardClicked = null;
        }
    }
    
    public class UnsubscribeAction : IDisposable
    {
        private readonly Action _unsubscribeAction;
        private bool _disposed = false;
        
        public UnsubscribeAction(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _unsubscribeAction?.Invoke();
                _disposed = true;
            }
        }
    }
}
