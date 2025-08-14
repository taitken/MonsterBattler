
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
using Game.Presentation.GameObjects.Factories;
using Game.Presentation.Services;
using UnityEngine;

namespace Game.Bootstrap
{
    public class App : MonoBehaviour
    {
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private GameObject combatTextPrefab;
        [SerializeField] private GameObject sharedCanvasPrefab;
        private EventBusRunner _eventRunner;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            var _services = new ServiceContainer();
            ServiceLocator.Set(_services);

            // Services
            _services.RegisterAsSingleton<ILoggerService, LoggerService>();
            _services.RegisterAsSingleton<IEventBus, EventBus>();
            _services.RegisterAsSingleton<IMonsterEntityFactory, MonsterEntityFactory>();
            _services.RegisterAsSingleton<IViewRegistryService, ViewRegistryService>();
            _services.RegisterAsSingleton<IInteractionBarrier, InteractionBarrier>();
            
            _services.RegisterAsSingleton<ISceneConductorService>(() => GetComponentInChildren<SceneConductorService>());

            _services.RegisterAsScoped<IBattleService, BattleService>();

            _services.RegisterAsTransient<IRandomService, UnityRandomService>();
            _services.RegisterAsTransient<IFadeController, FadeController>();

            // Factories
            _services.RegisterAsSingleton<IRoomFactory>(() => new RoomFactory(roomPrefab));
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