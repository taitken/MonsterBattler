# MonsterBattler - Architecture Documentation

This document provides a comprehensive overview of the MonsterBattler Unity project architecture, designed to help future Claude instances understand the codebase structure and patterns.

## Overview

MonsterBattler is a Unity-based monster battling game implementing **Clean Architecture** principles with **Dependency Injection**, **Event-Driven Architecture**, and **Scene-Based Navigation**. The project follows SOLID principles and maintains clear separation of concerns across multiple architectural layers.

## Architecture Pattern

The project implements **Clean Architecture** with the following layer structure:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                   │
│           (Game.Presentation assembly)                  │
│    UI Controllers, GameObjects, View Factories          │
└─────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                   │
│          (Game.Infrastructure assembly)                 │
│   Services, Factories, ScriptableObjects, Adapters     │
└─────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                      │
│           (Game.Application assembly)                   │
│     Services, DTOs, Commands, Event Handlers            │
└─────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                         │
│            (Game.Domain assembly)                       │
│        Entities, Value Objects, Domain Events           │
└─────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────┐
│                     Core Layer                          │
│             (Game.Core assembly)                        │
│      DI Container, Interfaces, Utilities                │
└─────────────────────────────────────────────────────────┘
```

### Assembly Dependencies

The project enforces clean architecture through Unity Assembly Definitions:

- **Game.Core**: No dependencies - contains core interfaces and utilities
- **Game.Domain**: Depends only on Game.Core - pure domain logic
- **Game.Application**: Depends on Core + Domain - application services and use cases
- **Game.Infrastructure**: Depends on Core + Application + Domain + Unity packages - external concerns
- **Game.Presentation**: Depends on Core + Application + Domain - UI and Unity-specific presentation logic
- **Game.Bootstrap**: Entry point assembly that wires everything together

## Key Directories and Purposes

### Assets/Game.Core/
- **DI/**: Dependency injection interfaces and service locator
- **Interfaces/**: Core abstractions
- **UtilityClasses/**: Shared utilities and extensions

### Assets/Game.Domain/
- **Entities/**: Domain entities (MonsterEntity, RoomEntity, etc.)
- **Enums/**: Domain enumerations (MonsterType, Biome, BattleOutcome)
- **Messaging/**: Domain events and message interfaces
- **Structs/**: Value objects (BattleResult, Xy coordinates)

### Assets/Game.Application/
- **Services/**: Application services (BattleService, NavigationService)
- **DTOs/**: Data transfer objects for cross-layer communication
- **Messaging/**: Commands, events, and messaging infrastructure
- **Interfaces/**: Application layer contracts
- **Handlers/**: Command and event handlers

### Assets/Game.Infrastructure/
- **Services/**: Concrete implementations (ServiceContainer, EventBus)
- **Factories/**: Entity and object factories
- **ScriptableObjects/**: Unity data containers
- **Adapters/**: External service adapters

### Assets/Game.Presentation/
- **Scenes/**: Unity scenes and scene bootstrappers
- **UI/**: User interface components
- **Controllers/**: Presentation controllers
- **Factories/**: View and GameObject factories
- **Services/**: Presentation-specific services

### Assets/Game.Bootstrap/
- **GameInstaller.cs**: Main dependency injection configuration
- **App.prefab**: Application bootstrap prefab

## Dependency Injection System

The project uses a custom **Service Container** implementing standard DI patterns:

### Service Lifetimes
- **Singleton**: Single instance throughout application lifetime
- **Scoped**: Single instance per scope (reset between scenes)
- **Transient**: New instance on each resolution

### Key Components

**ServiceLocator** (`C:\Users\tyler\MonsterBattler\Assets\Game.Core\DI\ServiceLocator.cs`):
```csharp
public static class ServiceLocator
{
    private static IServiceContainer _container;
    public static void Set(IServiceContainer container) => _container = container;
    public static T Get<T>() => _container.Resolve<T>();
}
```

**ServiceContainer** provides:
- Constructor injection
- Circular dependency detection
- Multiple registration strategies (factory functions vs. type mapping)
- Scope management for scene transitions

### Service Registration

Services are registered in `GameInstaller.cs` during application startup:

```csharp
// Core Services - Singletons
services.RegisterAsSingleton<IEventBus, EventBus>();
services.RegisterAsSingleton<INavigationService, NavigationService>();

// Battle/Overworld Services - Scoped (reset between scenes)
services.RegisterAsScoped<IBattleService, BattleService>();
services.RegisterAsScoped<IOverworldService, OverworldService>();

// Utilities - Transient
services.RegisterAsTransient<IRandomService, UnityRandomService>();
```

## Event-Driven Messaging System

The project implements a sophisticated **Event Bus** supporting multiple dispatch strategies:

### Event Bus Features (`C:\Users\tyler\MonsterBattler\Assets\Game.Infrastructure\Services\EventBus.cs`)

1. **Dispatch Modes**:
   - `Immediate`: Synchronous execution
   - `NextFrame`: Queued for next Unity frame
   - `Queued`: Priority-based queue for visual sequencing

2. **Topic-Based Routing**: Messages can be published to specific topics
3. **Priority System**: Lower numbers = higher priority for visual events
4. **Correlation IDs**: For tracking related events

### Usage Patterns

**Publishing Events**:
```csharp
_bus.Publish(new BattleStartedEvent(playerTeam, enemyTeam));
_bus.Publish(new DamageAppliedEvent(attacker, target, damage), 
    new PublishOptions { Dispatch = Dispatch.Queued, Priority = 1 });
```

**Subscribing to Events**:
```csharp
var subscription = _bus.Subscribe<MonsterFaintedEvent>(OnMonsterFainted);
// subscription.Dispose() to unsubscribe
```

### Event Types

- **Battle Events**: BattleStartedEvent, TurnStartedEvent, DamageAppliedEvent, etc.
- **Spawning Events**: MonsterSpawnedEvent, RoomSpawnedEvent
- **Navigation Commands**: LoadSceneCommand, UnloadSceneCommand

## Scene Structure and Bootstrapping

### Scene Architecture

The game uses three main scenes with individual bootstrappers:

1. **StartMenuScene**: Entry point and main menu
2. **OverworldScene**: Exploration and room navigation
3. **BattleScene**: Combat encounters

### Bootstrapping Pattern

Each scene has a dedicated bootstrapper that:
- Resolves dependencies via ServiceLocator
- Handles scene-specific initialization
- Manages payload transfer between scenes
- Sets up scene-specific UI and background

**Example**: `BattleSceneBootstrapper.cs`:
```csharp
async void Start()
{
    // Get navigation payload
    if (!_navigationService.TryTakePayload(GameScene.BattleScene, out BattlePayload payload))
        return;
    
    // Load appropriate background
    await LoadBackgroundForRoom(payload.RoomId);
    
    // Start battle
    await _battleService.RunBattleAsync(payload.RoomId, ct);
}
```

### Scene Navigation

**NavigationService** manages scene transitions and payload passing:
- Type-safe payload storage per scene
- One-time payload consumption (prevents reuse)
- Integrated with SceneConductorService for actual scene loading

### Scene Conductor

**SceneConductorService** handles scene loading operations:
- Queue-based operation sequencing
- Fade transitions
- Scoped service reset between scenes
- Support for additive scene loading

## Entity and Service Patterns

### Domain Entities

**MonsterEntity** (`C:\Users\tyler\MonsterBattler\Assets\Game.Domain\Entities\Creature\MonsterEntity.cs`):
- Pure domain logic with no Unity dependencies
- Event-driven health/state changes
- Encapsulated business rules

```csharp
public class MonsterEntity : BaseEntity
{
    public event Action<int> OnHealthChanged;
    public event Action OnDied;
    
    public void TakeDamage(int amount)
    {
        CurrentHP = Math.Max(0, CurrentHP - amount);
        OnHealthChanged?.Invoke(amount);
        if (CurrentHP <= 0) OnDied?.Invoke();
    }
}
```

### Factory Pattern

**MonsterEntityFactory** creates entities from ScriptableObject definitions:
- Resource-based loading
- Data validation
- Dependency injection via constructor

### Service Patterns

**BattleService** orchestrates combat:
- Async/await for turn management
- Interaction barriers for animation synchronization
- Event publication for UI updates
- Team state management

## Build and Development Commands

### Unity Build
- Build through Unity Editor: File → Build Settings
- Built executable available in `Dist/` directory
- Project uses Universal Render Pipeline (URP)

### Development Setup
- Unity 2022.3+ LTS recommended
- Visual Studio Code integration configured (`.vscode/settings.json`)
- Assembly definitions provide fast compilation
- Addressable Asset System for resource management

### Project Structure Commands
```bash
# View project structure
ls Assets/Game.*/

# Find specific patterns
grep -r "ServiceLocator" Assets/
grep -r "EventBus" Assets/

# Assembly dependency analysis
cat Assets/Game.*/Game.*.asmdef
```

## Interaction Barrier Pattern

The **Interaction Barrier** enables coordination between application logic and presentation animations:

### Use Cases
- Wait for animation completion before proceeding
- Synchronize multiple UI updates
- Coordinate scene transitions
- Battle flow synchronization

### Example Usage
```csharp
// Create barrier token
var animationToken = BarrierToken.New();

// Start animation and publish event
_bus.Publish(new ActionSelectedEvent(team, attacker, target, animationToken));

// Wait for animation hit point
await _waitBarrier.WaitAsync(new BarrierKey(animationToken, (int)AttackPhase.Hit), ct);

// Apply damage
ResolveBasicAttack(attacker, target);

// Wait for animation completion
await _waitBarrier.WaitAsync(new BarrierKey(animationToken, (int)AttackPhase.End), ct);
```

## Key Design Principles

1. **Dependency Inversion**: High-level modules don't depend on low-level modules
2. **Single Responsibility**: Each service has one well-defined purpose
3. **Open/Closed**: Easy to extend without modification
4. **Interface Segregation**: Clients depend only on interfaces they use
5. **Event-Driven**: Loose coupling through message passing
6. **Immutable Events**: Event data is read-only after publication
7. **Fail-Fast**: Validation and error handling at boundaries

## Working with the Codebase

### Adding New Features
1. Define domain entities in Game.Domain
2. Create interfaces in Game.Application
3. Implement services in Game.Infrastructure
4. Add presentation logic in Game.Presentation
5. Register dependencies in GameInstaller
6. Wire up event subscriptions

### Common Patterns
- Use ServiceLocator.Get<T>() to resolve dependencies
- Publish events for cross-cutting concerns
- Use barriers for animation synchronization
- Follow async/await patterns for operations
- Implement proper disposal for subscriptions

### Testing Strategy
- Domain entities are easily unit testable
- Application services can be tested with mocked dependencies
- Infrastructure services require integration testing
- Event flows can be tested by subscribing to published events

This architecture provides a robust foundation for a scalable Unity game with clear separation of concerns and testable components.