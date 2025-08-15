using System.Collections;
using System.Threading.Tasks;
using Game.Application.DTOs;
using Game.Application.Enums;
using Game.Application.Interfaces;
using Game.Core;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Presentation;
using Game.Presentation.Services;
using Game.Presentation.UI.Combat;
using UnityEngine;

namespace Assets.Game.Presentation.GameObjects
{
    public class MonsterView : MonoObject<MonsterEntity>
    {
        [SerializeField] private HealthBarUi healthBar;
        private Vector3 originalPosition;
        private SpriteRenderer spriteRenderer;
        private IInteractionBarrier _waitBarrier;
        private Color WHITE = Color.white;
        private Color BLUE = Color.Lerp(Color.white, Color.blue, 0.5f);
        private Color RED = Color.Lerp(Color.white, Color.red, 0.5f);
        private Color GREEN = Color.Lerp(Color.white, Color.green, 0.5f);

        void Awake()
        {
            originalPosition = transform.position;
            spriteRenderer = GetComponent<SpriteRenderer>();
            _viewRegistry = ServiceLocator.Get<IViewRegistryService>();
            _waitBarrier = ServiceLocator.Get<IInteractionBarrier>();
        }

        protected override void OnModelBound()
        {
            spriteRenderer.sprite = Resources.Load<Sprite>($"Monsters/Sprites/{model.Type}");
            model.OnHealthChanged += UpdateHealthVisuals;
            healthBar.SetHealth(model.CurrentHP, model.MaxHealth);
            _viewRegistry.Register(model.Id, this);
        }

        public async void PlayAttackAnimation(BarrierToken token)
        {
            Vector3 attackPosition = originalPosition + (model.AttackDirection == AttackDirection.Right ? new Vector3(0.3f, 0, 0) : new Vector3(-0.3f, 0, 0));
            FlashColor(BLUE, 0.2f);
            await MoveTo(attackPosition, 0.04f);
            _waitBarrier.Signal(new BarrierKey(token, (int)AttackPhase.Hit));
            await MoveTo(originalPosition, 0.1f);

            StartCoroutine(SignalEndAfterDelay(.5f, token));
        }

        private IEnumerator SignalEndAfterDelay(float seconds, BarrierToken token)
        {
            yield return new WaitForSeconds(seconds);
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