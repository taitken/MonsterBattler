
using Assets.Game.Presentation.GameObjects;
using Assets.Game.Presentation.UiObjects;
using Game.Core;
using UnityEngine;
namespace Game.Bootstrap
{
    public class App : MonoBehaviour
    {
        private IServiceContainer _services;
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private GameObject combatTextPrefab;
        [SerializeField] private GameObject sharedCanvasPrefab;

        void Awake()
        {
            var _services = new ServiceContainer();
            ServiceLocator.Set(_services);

            // Services
            _services.RegisterAsSingleton<ILoggerService, LoggerService>();

            _services.RegisterAsScoped<IEventQueueService, EventQueueService>();
            _services.RegisterAsScoped<ITestService, TestService>();

            // Factories
            _services.RegisterAsSingleton<IMonsterViewFactory>(() => new MonsterViewFactory(monsterPrefab));
            _services.RegisterAsSingleton<ICombatTextFactory>(() => new CombatTextFactory(combatTextPrefab, sharedCanvasPrefab.GetComponent<RectTransform>()));
        }
    }
}