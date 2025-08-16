
using Assets.Game.Presentation.UiObjects;
using Game.Application.IFactories;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Application.Services;
using Game.Core;
using Game.Core.Logger;
using Game.Core.Randomness;
using Game.Infrastructure.Randomness;
using Game.Infrastructure.ScriptableObjects;
using Game.Infrastructure.Services;
using Game.Infrastructure.Spawning;
using Game.Presentation.GameObjects.Factories;
using Game.Presentation.Services;
using UnityEngine;

namespace Game.Bootstrap
{
    public class App : MonoBehaviour
    {
        [Header("Image Databases")]
        [SerializeField] private MonsterDatabase monsterDatabase;
        [SerializeField] private EnemyEncounterDatabase enemyEncounterDatabase;
        
        [Header("Spawnable Game Objects")]
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private GameObject combatTextPrefab;
        private EventBusRunner _eventRunner;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            var _services = new ServiceContainer();
            ServiceLocator.Set(_services);

            RegisterSingletonServices(_services);
            RegisterScopedServices(_services);
            RegisterTransientServices(_services);
            RegisterFactories(_services);
            RegisterDatabases(_services);

            _eventRunner = new EventBusRunner();
        }

        private void RegisterSingletonServices(ServiceContainer services)
        {
            // Core Services
            services.RegisterAsSingleton<IServiceContainer, ServiceContainer>();
            services.RegisterAsSingleton<ILoggerService, LoggerService>();
            services.RegisterAsSingleton<IEventBus, EventBus>();
            services.RegisterAsSingleton<IInteractionBarrier, InteractionBarrier>();

            // Application Services
            services.RegisterAsSingleton<INavigationService, NavigationService>();
            services.RegisterAsSingleton<IBattleHistoryService, BattleHistoryService>();
            services.RegisterAsSingleton<IOverworldPersistenceService, OverworldPersistenceService>();
            services.RegisterAsSingleton<IOverworldGenerator, RandomOverworldGenerator>();
            services.RegisterAsSingleton<IPlayerTeamPersistenceService, PlayerTeamPersistenceService>();

            // Presentation Services
            services.RegisterAsSingleton<IViewRegistryService, ViewRegistryService>();
            services.RegisterAsSingleton<ISpriteCache, SpriteCache>();
            services.RegisterAsSingleton<ISceneConductorService>(() => GetComponentInChildren<SceneConductorService>());

            // Entity Factories
            services.RegisterAsSingleton<IMonsterEntityFactory, MonsterEntityFactory>();
            services.RegisterAsSingleton<IBiomeBackgroundProvider, BiomeBackgroundAdapter>();
        }

        private void RegisterScopedServices(ServiceContainer services)
        {
            services.RegisterAsScoped<IBattleService, BattleService>();
            services.RegisterAsScoped<IOverworldService, OverworldService>();
        }

        private void RegisterTransientServices(ServiceContainer services)
        {
            services.RegisterAsTransient<IRandomService, UnityRandomService>();
            services.RegisterAsTransient<IFadeController, FadeController>();
        }

        private void RegisterFactories(ServiceContainer services)
        {
            services.RegisterAsSingleton<IRoomViewFactory>(() => new RoomViewFactory(roomPrefab));
            services.RegisterAsSingleton<IMonsterViewFactory>(() => new MonsterViewFactory(monsterPrefab));
            services.RegisterAsSingleton<ICombatTextFactory>(() => new CombatTextFactory(combatTextPrefab));
        }

        private void RegisterDatabases(ServiceContainer services)
        {
            services.RegisterAsSingleton<IMonsterSpriteProvider>(() => new MonsterSpriteAdapter(monsterDatabase));
            services.RegisterAsSingleton<IEnemyEncounterProvider>(() => new EnemyEncounterAdapter(enemyEncounterDatabase));
        }

        void Update()
        {
            _eventRunner.Run();
        }
    }
}