using System.Threading.Tasks;
using Assets.Game.Presentation.UiObjects;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Presentation;
using Game.Presentation.UI.Combat;
using UnityEngine;

namespace Assets.Game.Presentation.GameObjects
{
    public class MonsterView : MonoObject<MonsterEntity>
    {
        [SerializeField] private HealthBarUi healthBar;
        private Vector3 originalPosition;
        private SpriteRenderer spriteRenderer;
        private ICombatTextFactory combatTextFactory;
        private Color WHITE = Color.white;
        private Color BLUE = Color.Lerp(Color.white, Color.blue, 0.5f);
        private Color RED = Color.Lerp(Color.white, Color.red, 0.5f);
        private string RED_HEX = "#B22222";
        private Color GREEN = Color.Lerp(Color.white, Color.green, 0.5f);

        void Awake()
        {
            Debug.Log("Monster awake");
            originalPosition = transform.position;
            spriteRenderer = GetComponent<SpriteRenderer>();
            combatTextFactory = Inject<ICombatTextFactory>();
        }

        protected override void OnModelBound()
        {
            Debug.Log("Binding Monster");
            spriteRenderer.sprite = Resources.Load<Sprite>($"Monsters/Sprites/{model.Type}");
            model.OnHealthChanged += UpdateHealthVisuals;
            model.OnAttack += PlayAttackAnimation;
            model.OnDied += OnDied;
            healthBar.SetHealth(model.CurrentHealth, model.MaxHealth);
        }

        public async void PlayAttackAnimation()
        {
            // Highlight orange
            FlashColor(BLUE, 0.2f);

            // Move forward slightly
            Vector3 attackPosition = originalPosition + (model.AttackDirection == AttackDirection.Right ? new Vector3(0.3f, 0, 0) : new Vector3(-0.3f, 0, 0));
            await MoveTo(attackPosition, 0.04f);

            // Return to original position
            await MoveTo(originalPosition, 0.1f);
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

        private void ShowDamage(int amount)
        {
            ColorUtility.TryParseHtmlString(RED_HEX, out Color redDamageColor);
            combatTextFactory.Create(redDamageColor, $"{amount}", transform.position + new Vector3(0, 1f, 0));
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
            Debug.Log($"Updating health visuals for {model.MonsterName}: {model.CurrentHealth}/{model.MaxHealth}. Previous: {healthBar.currentHealth}");

            ShowDamage(amount);
            if (amount > 0)
            {
                FlashColor(RED);
            }
            if (amount < 0)
            {
                FlashColor(GREEN);
            }
            healthBar.SetHealth(model.CurrentHealth, model.MaxHealth);
        }

        private void OnDied()
        {
            Debug.Log($"{model.MonsterName} has fainted!");
            gameObject.SetActive(false); // simple visual feedback
        }

        protected override void BeforeDeath()
        {
            model.OnHealthChanged -= UpdateHealthVisuals;
            model.OnAttack -= PlayAttackAnimation;
            model.OnDied -= OnDied;
        }
    }
}