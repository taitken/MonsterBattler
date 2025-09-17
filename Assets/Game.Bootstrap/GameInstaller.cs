
using Assets.Game.Presentation.UiObjects;
using Game.Application.IFactories;
using Game.Application.Interfaces;
using Game.Application.Interfaces.Effects;
using Game.Application.Messaging;
using Game.Application.Repositories;
using Game.Application.Services;
using Game.Application.Services.Effects;
using Game.Core;
using Game.Core.Logger;
using Game.Core.Randomness;
using Game.Infrastructure.Factories;
using Game.Infrastructure.Providers;
using Game.Infrastructure.Randomness;
using Game.Infrastructure.ScriptableObjects;
using Game.Infrastructure.Services;
using Game.Infrastructure.Spawning;
using Game.Presentation.Core.Interfaces;
using Game.Presentation.Core.Services;
using Game.Presentation.GameObjects.Factories;
using Game.Presentation.Services;
using Game.Presentation.Shared.Factories;
using UnityEngine;

namespace Game.Bootstrap
{
    public class App : MonoBehaviour
    {
        [Header("Image Databases")]
        [SerializeField] private MonsterDatabase monsterDatabase;
        [SerializeField] private EnemyEncounterDatabase enemyEncounterDatabase;
        [SerializeField] private AbilityCardDatabase abilityCardDatabase;
        [SerializeField] private ResourceIconDatabase resourceIconDatabase;
        [SerializeField] private RuneIconDatabase runeIconDatabase;
        
        [Header("Spawnable Game Objects")]
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private GameObject cardPrefab;
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
            services.RegisterAsSingleton<IBattleHistoryRepository, BattleHistoryRepository>();
            services.RegisterAsSingleton<IOverworldRepository, OverworldRepository>();
            services.RegisterAsSingleton<IOverworldGenerator, RandomOverworldGenerator>();
            services.RegisterAsSingleton<IPlayerDataRepository, PlayerDataRepository>();
            services.RegisterAsSingleton<IPauseService, PauseService>();

            // Presentation Services
            services.RegisterAsSingleton<IViewRegistryService, ViewRegistryService>();
            services.RegisterAsSingleton<ISpriteCache, SpriteCache>();
            services.RegisterAsSingleton<ISceneConductorService>(() => GetComponentInChildren<SceneConductorService>());

            // Entity Factories
            services.RegisterAsSingleton<IMonsterEntityFactory, MonsterEntityFactory>();
            services.RegisterAsSingleton<IAbilityCardFactory>(() => new AbilityCardFactory(abilityCardDatabase));
            services.RegisterAsSingleton<IBiomeBackgroundProvider, BiomeBackgroundAdapter>();
        }

        private void RegisterScopedServices(ServiceContainer services)
        {
            services.RegisterAsScoped<IBattleService, BattleService>();
            services.RegisterAsScoped<IOverworldService, OverworldService>();
            services.RegisterAsScoped<ICardEffectResolver, CardEffectResolver>();
            services.RegisterAsScoped<IEffectProcessor, EffectProcessor>();
        }

        private void RegisterTransientServices(ServiceContainer services)
        {
            services.RegisterAsTransient<IRandomService, UnityRandomService>();
            services.RegisterAsTransient<IFadeController, FadeController>();
            services.RegisterAsTransient<IRewardGeneratorService, RewardGeneratorService>();
        }

        private void RegisterFactories(ServiceContainer services)
        {
            services.RegisterAsSingleton<IRoomViewFactory>(() => new RoomViewFactory(roomPrefab));
            services.RegisterAsSingleton<IMonsterViewFactory>(() => new MonsterViewFactory(monsterPrefab));
            services.RegisterAsSingleton<ICardViewFactory>(() => new CardViewFactory(cardPrefab));
            services.RegisterAsSingleton<ICombatTextFactory>(() => new CombatTextFactory(combatTextPrefab));
        }

        private void RegisterDatabases(ServiceContainer services)
        {
            services.RegisterAsSingleton<IMonsterSpriteProvider>(() => new MonsterSpriteAdapter(monsterDatabase));
            services.RegisterAsSingleton<IEnemyEncounterProvider>(() => new EnemyEncounterAdapter(enemyEncounterDatabase));
            services.RegisterAsSingleton<ICardArtProvider>(() => new CardArtAdapter(abilityCardDatabase));
            services.RegisterAsSingleton<IResourceIconProvider>(() => new ResourceIconProvider(resourceIconDatabase));
            services.RegisterAsSingleton<IRuneIconProvider>(() => new RuneIconProvider(runeIconDatabase));
        }

        void Update()
        {
            _eventRunner.Run();
        }
    }
}