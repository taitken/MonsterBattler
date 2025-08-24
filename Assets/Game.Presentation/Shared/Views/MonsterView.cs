using System.Collections;
using System.Threading.Tasks;
using Game.Application.DTOs;
using Game.Application.Enums;
using Game.Application.Interfaces;
using Game.Core;
using Game.Domain.Entities;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;
using Game.Presentation.Core.Interfaces;
using Game.Presentation.Scenes.Battle.UI.MonsterUI.HealthBarUi;
using UnityEngine;

namespace Game.Presentation.Shared.Views
{
    public class MonsterView : MonoObject<MonsterEntity>
    {
        [SerializeField] private HealthBarUi healthBar;
        [SerializeField] private DeckIconPanelUI deckIcon;
        [SerializeField] private IconPanelUI _iconPanel;
        
        public Vector3 DeckIconWorldPosition
        {
            get
            {
                if (deckIcon != null)
                {
                    // Use the actual deck icon position from the panel
                    return deckIcon.DeckIconPosition;
                }
                else
                {
                    // Fallback: calculate position relative to monster
                    return transform.position + new Vector3(-1.2f, -0.5f, 0);
                }
            }
        }
        private Vector3 originalPosition;
        private SpriteRenderer spriteRenderer;
        private IInteractionBarrier _waitBarrier;
        private IMonsterSpriteProvider _spriteProvider;
        private Color WHITE = Color.white;
        private Color BLUE = Color.Lerp(Color.white, Color.blue, 0.5f);
        private Color RED = Color.Lerp(Color.white, Color.red, 0.5f);
        private Color GREEN = Color.Lerp(Color.white, Color.green, 0.5f);

        void Awake()
        {
            originalPosition = transform.position;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                throw new System.InvalidOperationException(
                    $"SpriteRenderer component not found on {gameObject.name}. MonsterView requires a SpriteRenderer component.");
            }

            try
            {
                _viewRegistry = ServiceLocator.Get<IViewRegistryService>();
                _waitBarrier = ServiceLocator.Get<IInteractionBarrier>();
                _spriteProvider = ServiceLocator.Get<IMonsterSpriteProvider>();
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException(
                    $"Failed to resolve required services in MonsterView on {gameObject.name}. " +
                    $"Ensure services are properly registered in GameInstaller. Error: {ex.Message}", ex);
            }
        }

        protected override async void OnModelBound()
        {
            if (model == null)
            {
                throw new System.InvalidOperationException("Cannot bind null model to MonsterView");
            }

            try
            {
                var sprite = await _spriteProvider.GetMonsterSpriteAsync<Sprite>(model.Type);
                if (sprite != null)
                {
                    spriteRenderer.sprite = sprite;
                }
                else
                {
                    throw new System.InvalidOperationException(
                        $"Failed to load sprite for monster type: {model.Type}");
                }
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException(
                    $"Critical error loading sprite for monster {model.MonsterName} ({model.Type}): {ex.Message}", ex);
            }

            if (healthBar == null)
            {
                throw new System.InvalidOperationException(
                    $"HealthBar component not assigned on MonsterView for {model.MonsterName}");
            }

            model.OnHealthChanged += UpdateHealthVisuals;
            model.AbilityDeck.OnModelUpdated += UpdateDeckVisuals;
            model.OnStatusEffectAdded += OnStatusEffectChanged;
            model.OnStatusEffectRemoved += OnStatusEffectChanged;
            
            UpdateHealthVisuals(0);
            UpdateDeckVisuals();
            UpdateStatusEffectIcons();
            
            // Registration is now handled by MonsterViewSpawner
            Debug.Log($"MonsterView {model.MonsterName} (ID: {model.Id}) bound successfully");
        }

        public async void PlayAttackAnimation(BarrierToken token)
        {
            Vector3 attackPosition = originalPosition + (model.BattleTeam == BattleTeam.Player ? new Vector3(0.3f, 0, 0) : new Vector3(-0.3f, 0, 0));
            FlashColor(BLUE, 0.2f);
            await MoveTo(attackPosition, 0.04f);
            _waitBarrier.Signal(new BarrierKey(token, (int)AttackPhase.Hit));
            await MoveTo(originalPosition, 0.1f);

            StartCoroutine(SignalEndAfterDelay(.5f, token));
        }

        private IEnumerator SignalEndAfterDelay(float seconds, BarrierToken token)
        {
            yield return new WaitForSeconds(seconds);
            Debug.Log($"{model.MonsterName} signaling AttackPhase.End");
            _waitBarrier.Signal(new BarrierKey(token, (int)AttackPhase.End));
        }

        private async void FlashColor(Color color, float duration = 0.1f)
        {
            float elapsed = 0f;
            spriteRenderer.color = color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                await Task.Yield();
            }
            spriteRenderer.color = WHITE;
        }

        private async Task MoveTo(Vector3 target, float duration)
        {
            float elapsed = 0f;
            Vector3 start = transform.position;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(start, target, elapsed / duration);
                elapsed += Time.deltaTime;
                await Task.Yield();
            }

            transform.position = target;
        }

        private void UpdateHealthVisuals(int amount)
        {
            healthBar.SetHealth(model.CurrentHP, model.MaxHealth);
        }

        private void UpdateDeckVisuals()
        {
            Debug.Log($"Setting icon. Cards in deck: {model.AbilityDeck.CardsInDeck}");
            deckIcon.SetDeckIconText(model.AbilityDeck.CardsInDeck, model.AbilityDeck.CardsInDiscard);
        }

        private void OnStatusEffectChanged(StatusEffect statusEffect)
        {
            UpdateStatusEffectIcons();
        }

        private void UpdateStatusEffectIcons()
        {
            if (_iconPanel != null && model != null)
            {
                _iconPanel.UpdateStatusEffects(model.StatusEffects);
            }
        }

        public void PlayHitReaction(int amount)
        {
            if (amount > 0)
            {
                FlashColor(RED);
            }
            if (amount < 0)
            {
                FlashColor(GREEN);
            }
        }

        public void PlayDie()
        {
            Debug.Log($"{model.MonsterName} has fainted!");
            gameObject.SetActive(false); // simple visual feedback
        }

        public void OnDestroy()
        {
            model.OnHealthChanged -= UpdateHealthVisuals;
            _viewRegistry.Unregister(model.Id);
        }
    }
}