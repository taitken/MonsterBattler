# Effect Processor System Reference

This document explains the EffectProcessor system, its behaviors, and integration with the battle system in MonsterBattler.

## Overview

The **EffectProcessor** is a central component that handles all status effect logic during battles. It implements a behavior pattern where each effect type has its own behavior class that defines how the effect interacts with game events.

## Architecture

### Core Components

#### IEffectProcessor Interface
Located: `Game.Application\Interfaces\Effects\IEffectProcessor.cs`

Defines five main processing methods:
- `ProcessDamageTaken()` - Modifies incoming damage (e.g., Block, Fortify)
- `ProcessCardPlayed()` - Prevents/allows card plays (e.g., Stun)
- `ProcessTurnEnd()` - Handles end-of-turn effects (e.g., Burn, Poison)
- `ProcessEffectApplied()` - Reacts to new effects being applied (e.g., Fortify boosting Block)
- `ProcessOutgoingDamage()` - Modifies damage being dealt (e.g., Strength)

#### EffectProcessor Implementation
Located: `Game.Application\Services\Effects\EffectProcessor.cs`

**Key Features:**
- Uses Dictionary<EffectType, IEffectBehavior> for O(1) behavior lookup
- Processes effects in order they appear on the monster
- Logs all effect processing for debugging
- Publishes events for UI feedback
- Filters out expired effects automatically

**Registration Pattern:**
```csharp
private void RegisterBehaviors()
{
    RegisterBehavior(EffectType.Burn, new BurnEffectBehavior(_bus, _log));
    RegisterBehavior(EffectType.Block, new BlockEffectBehavior(_log));
    // ... more registrations
}
```

### Behavior Interface Hierarchy

#### Base Interface
```csharp
public interface IEffectBehavior
{
    // Marker interface for all effect behaviors
}
```

#### Specialized Behavior Interfaces
Located: `Game.Application\Interfaces\Effects\IEffectBehavior.cs`

1. **IOnDamageTakenBehavior** - Modifies incoming damage
   - Returns `DamageModificationResult` with blocked/reduced amounts
   - Example: Block, Fortify-enhanced Block

2. **IOnTurnEndBehavior** - Triggers at end of monster's turn
   - Example: Burn damage, Poison damage, Regeneration

3. **IOnEffectAppliedBehavior** - Reacts when new effects are applied
   - Example: Fortify boosting newly applied Block effects

4. **IOnCardPlayedBehavior** - Can prevent card plays
   - Returns `CardPlayResult` (allowed/prevented with reason)
   - Example: Stun preventing all actions

5. **IOnDamageDealtBehavior** - Modifies outgoing damage
   - Example: Strength increasing damage dealt

## Current Effect Behaviors

### Defensive Effects

#### BlockEffectBehavior
- **Interface:** `IOnDamageTakenBehavior`
- **Location:** `Game.Application\Services\Effects\Behaviors\BlockEffectBehavior.cs`
- **Logic:** Blocks damage up to effect value, reduces block by damage blocked
- **Key Method:** `ModifyDamage()` - calculates damage blocked and remaining

#### FortifyEffectBehavior
- **Interface:** `IOnEffectAppliedBehavior`
- **Location:** `Game.Application\Services\Effects\Behaviors\FortifyEffectBehavior.cs`
- **Logic:** When Block is applied, increases Block value by Fortify stacks
- **Key Method:** `OnEffectApplied()` - boosts Block effects only

### Damage Over Time Effects

#### BurnEffectBehavior
- **Interface:** `IOnTurnEndBehavior`
- **Location:** `Game.Application\Services\Effects\Behaviors\BurnEffectBehavior.cs`
- **Logic:** Deals damage equal to effect value, reduces by 1 stack per turn
- **Key Method:** `OnTurnEnd()` - applies burn damage and decrements stacks

#### PoisonEffectBehavior
- **Interface:** `IOnTurnEndBehavior`
- **Location:** `Game.Application\Services\Effects\Behaviors\PoisonEffectBehavior.cs`
- **Logic:** Similar to Burn but with different thematic application

#### RegenerateEffectBehavior
- **Interface:** `IOnTurnEndBehavior`
- **Location:** `Game.Application\Services\Effects\Behaviors\RegenerateEffectBehavior.cs`
- **Logic:** Heals monster at end of turn based on effect value

### Control Effects

#### StunEffectBehavior
- **Interface:** `IOnCardPlayedBehavior`, `IOnTurnEndBehavior`
- **Location:** `Game.Application\Services\Effects\Behaviors\StunEffectBehavior.cs`
- **Logic:**
  - Prevents all card plays while active
  - Reduces duration by 1 each turn
- **Key Methods:**
  - `OnCardPlayed()` - returns prevented result
  - `OnTurnEnd()` - decrements duration

### Enhancement Effects

#### StrengthEffectBehavior
- **Interface:** `IOnDamageDealtBehavior`
- **Location:** `Game.Application\Services\Effects\Behaviors\StrengthEffectBehavior.cs`
- **Logic:** Increases outgoing damage by effect value

#### LuckEffectBehavior
- **Interface:** Various (implementation specific)
- **Location:** `Game.Application\Services\Effects\Behaviors\LuckEffectBehavior.cs`
- **Logic:** Affects random outcomes in player's favor

### Negative Effects

#### FrazzledEffectBehavior
- **Interface:** Implementation specific
- **Location:** `Game.Application\Services\Effects\Behaviors\FrazzledEffectBehavior.cs`
- **Logic:** Negative effect that impairs monster performance

#### BacklashEffectBehavior
- **Interface:** `IOnTurnEndBehavior` (likely)
- **Location:** `Game.Application\Services\Effects\Behaviors\BacklashEffectBehavior.cs`
- **Logic:** Deals damage back to the effect owner

## Integration with Battle System

### BattleService Integration
Located: `Game.Application\Services\BattleService.cs:38`

The BattleService injects `IEffectProcessor _effectProcessor` and uses it for:
- **Turn End Processing** (line 230): `_effectProcessor.ProcessTurnEnd(monster)` for all monsters
- **Card Play Validation**: Called before allowing card plays
- **Damage Processing**: Integrated through ResolveDamageCommandHandler

### Damage Resolution Flow
Located: `Game.Application\Handlers\Effects\ResolveDamageCommandHandler.cs`

1. **Outgoing Damage Processing** (line 28):
   ```csharp
   var modifiedDamage = _effectProcessor.ProcessOutgoingDamage(command.Caster, command.Target, command.Value);
   ```

2. **Apply Damage to Target** (line 31):
   ```csharp
   var amountBlocked = command.Target.TakeDamage(modifiedDamage);
   ```

3. **MonsterEntity.TakeDamage()** Integration:
   Located: `Game.Domain\Entities\Creature\MonsterEntity.cs:74`
   - Domain entity handles base damage logic
   - Block effects are processed at the domain level
   - EffectProcessor handles cross-cutting effect logic

### Event Publishing
The EffectProcessor publishes events for UI feedback:
- `DamageModifiedByEffectEvent` - When effects modify damage
- `CardPlayPreventedEvent` - When effects prevent card plays
- `DamageAppliedEvent` - From burn/poison effects

## Key Design Patterns

### 1. Strategy Pattern
Each effect type has its own behavior strategy, making it easy to:
- Add new effects without modifying existing code
- Test effects in isolation
- Maintain single responsibility per effect

### 2. Interface Segregation
Multiple small interfaces instead of one large interface:
- Behaviors only implement interfaces they need
- Clear separation of concerns
- Better compile-time safety

### 3. Event-Driven Communication
- Effects publish events rather than directly calling UI
- Loose coupling between effect logic and presentation
- Easy to add new listeners for effect events

### 4. Dependency Injection
- All behaviors receive necessary services through constructor
- Easy to mock for testing
- Clear dependency declarations

## Adding New Effects

### Step 1: Define Effect Type
Add to `Game.Domain\Enums\EffectType.cs`:
```csharp
public enum EffectType
{
    // existing effects...
    YourNewEffect
}
```

### Step 2: Create Behavior Class
Create `YourNewEffectBehavior.cs` in `Game.Application\Services\Effects\Behaviors\`:
```csharp
public class YourNewEffectBehavior : IAppropriateBehaviorInterface
{
    private readonly ILoggerService _log;
    // other dependencies...

    public YourNewEffectBehavior(ILoggerService log)
    {
        _log = log;
    }

    // Implement required interface methods
}
```

### Step 3: Register Behavior
Add to `EffectProcessor.RegisterBehaviors()`:
```csharp
RegisterBehavior(EffectType.YourNewEffect, new YourNewEffectBehavior(_log));
```

### Step 4: Update DI Registration
If needed, register in `GameInstaller.cs` for any new dependencies.

## Testing Strategy

### Unit Testing Effects
- Mock `ILoggerService` and `IEventBus`
- Create test `StatusEffect` instances
- Test behavior methods directly
- Verify event publishing

### Integration Testing
- Test EffectProcessor with multiple effects
- Verify effect interaction and ordering
- Test with real MonsterEntity instances

## Common Pitfalls

1. **Effect Ordering**: Effects are processed in the order they appear in the StatusEffects list
2. **Expired Effects**: Always check `effect.IsExpired` before processing
3. **Value Modification**: Effects should modify their own values (e.g., reduce stacks) when appropriate
4. **Event Publishing**: Remember to publish events for UI feedback
5. **Null Checks**: Always validate parameters and effect values

## Future Considerations

- **Effect Stacking Rules**: Currently effects stack by value, may need more complex stacking logic
- **Priority System**: Effects currently process in list order, may need priority-based processing
- **Effect Categories**: Group effects by category for batch processing
- **Performance**: Consider effect pooling for high-frequency battles

---

**File Locations Quick Reference:**
- Interface: `Game.Application\Interfaces\Effects\IEffectProcessor.cs`
- Implementation: `Game.Application\Services\Effects\EffectProcessor.cs`
- Behaviors: `Game.Application\Services\Effects\Behaviors\*.cs`
- Battle Integration: `Game.Application\Services\BattleService.cs:38`
- Damage Handler: `Game.Application\Handlers\Effects\ResolveDamageCommandHandler.cs`