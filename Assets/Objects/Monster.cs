using System;
using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoObject<MonsterModel>
{    
    [SerializeField] private SpriteRenderer monsterImage;
    [SerializeField] private HealthBar healthBar;
    private ITestService testService;

    void Awake()
    {
        Debug.Log("Monster awake");
        testService = Inject<ITestService>();
    }

    protected override void OnModelBound()
    {
        monsterImage.sprite = model.definition.sprite;
        model.OnHealthChanged += UpdateVisuals;
        model.OnDied += OnDied;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        healthBar.SetHealth((float)model.CurrentHealth / model.definition.maxHealth);
    }

    private void OnDied()
    {
        Debug.Log($"{model.definition.monsterName} has fainted!");
        gameObject.SetActive(false); // simple visual feedback
    }
}