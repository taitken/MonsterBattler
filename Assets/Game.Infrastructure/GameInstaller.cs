using System;
using UnityEngine;
public class App : MonoBehaviour
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject combatTextPrefab;
    [SerializeField] private GameObject sharedCanvasPrefab;
    void Awake()
    {
        var diContainer = ServiceContainer.Instance;

        // Services
        diContainer.RegisterAsSingleton<ILoggerService, LoggerService>();

        diContainer.RegisterAsScoped<IEventQueueService, EventQueueService>();
        diContainer.RegisterAsScoped<ITestService, TestService>();

        // Factories
        diContainer.RegisterAsSingleton<IMonsterFactory>(() => new MonsterFactory(monsterPrefab));
        diContainer.RegisterAsSingleton<ICombatTextFactory>(() => new CombatTextFactory(combatTextPrefab, sharedCanvasPrefab.GetComponent<RectTransform>())); 
    }
}