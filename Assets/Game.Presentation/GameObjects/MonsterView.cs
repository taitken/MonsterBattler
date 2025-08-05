using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MonsterView : MonoObject<MonsterEntity>
{
    [SerializeField] private HealthBarUi healthBar;
    private Vector3 originalPosition;
    private SpriteRenderer spriteRenderer;
    private ITestService testService;
    private ICombatTextFactory combatTextFactory;
    private Color WHITE = Color.white;
    private Color BLUE = Color.Lerp(Color.white, Color.blue, 0.5f);
    private Color RED = Color.Lerp(Color.white, Color.red, 0.5f);
    private string RED_HEX = "#B22222";
    private Color GREEN = Color.Lerp(Color.white, Color.green, 0.5f);

    void Awake()
    {
        Debug.Log("Monster awake");
        testService = Inject<ITestService>();
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        combatTextFactory = Inject<ICombatTextFactory>();
    }

    protected override void OnModelBound()
    {
        spriteRenderer.sprite = Resources.Load<Sprite>($"Monsters/Sprites/{model.definition.type}");
        model.OnHealthChanged += UpdateHealthVisuals;
        model.OnDied += OnDied;
        healthBar.SetHealth(model.CurrentHealth, model.MaxHealth);
    }

    public async Task PlayAttackAnimation()
    {
        // Highlight orange
        await FlashColor(BLUE, 0.2f);

        // Move forward slightly
        Vector3 attackPosition = originalPosition + (model.definition.attackDirection == AttackDirection.Right ? new Vector3(0.5f, 0, 0) : new Vector3(-0.5f, 0, 0));
        await MoveTo(attackPosition, 0.1f);

        // Return to original position
        await MoveTo(originalPosition, 0.2f);
    }

    private async Task FlashColor(Color color, float duration = 0.2f)
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
        combatTextFactory.Spawn(redDamageColor, $"{amount}", transform.position + new Vector3(0, 1f, 0));
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

    private async void UpdateHealthVisuals(int amount)
    {
        Debug.Log($"Updating health visuals for {model.definition.monsterName}: {model.CurrentHealth}/{model.definition.maxHealth}. Previous: {healthBar.currentHealth}");

        ShowDamage((int)amount);
        if (amount > 0)
        {
            await FlashColor(RED);
        }
        if (amount < 0)
        {
            await FlashColor(GREEN);
        }
        healthBar.SetHealth(model.CurrentHealth, model.MaxHealth);
    }

    private void OnDied()
    {
        Debug.Log($"{model.definition.monsterName} has fainted!");
        gameObject.SetActive(false); // simple visual feedback
    }

    protected override void BeforeDeath()
    {
        model.OnHealthChanged -= UpdateHealthVisuals;
        model.OnDied -= OnDied;
    }
}