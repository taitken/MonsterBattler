
using Assets.Game.Presentation.GameObjects;
using Assets.Game.Presentation.UiObjects;
using Game.Application.IFactories;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Services;
using Game.Core;
using Game.Core.Logger;
using Game.Core.Randomness;
using Game.Infrastructure.Randomness;
using Game.Infrastructure.Services;
using Game.Infrastructure.Spawning;
using UnityEngine;
namespace Game.Bootstrap
{
    public class App : MonoBehaviour
    {
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private GameObject combatTextPrefab;
        [SerializeField] private GameObject sharedCanvasPrefab;
        private EventBusRunner _eventRunner;

        void Awake()
        {
            var _services = new ServiceContainer();
            ServiceLocator.Set(_services);

            // Services
            _services.RegisterAsSingleton<ILoggerService, LoggerService>();
            _services.RegisterAsSingleton<IEventBus, EventBus>();
            _services.RegisterAsSingleton<IMonsterEntityFactory, MonsterEntityFactory>();

            _services.RegisterAsScoped<IBattleService, BattleService>();

            _services.RegisterAsTransient<IRandomService, UnityRandomService>();

            // Factories
            _services.RegisterAsSingleton<IMonsterViewFactory>(() => new MonsterViewFactory(monsterPrefab));
            _services.RegisterAsSingleton<ICombatTextFactory>(() => new CombatTextFactory(combatTextPrefab, sharedCanvasPrefab.GetComponent<RectTransform>()));
            _eventRunner = new EventBusRunner();
        }

        void Update()
        {
            _eventRunner.Run();
        }
    }
}