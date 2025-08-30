using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Core.Logger;
using Game.Core.Randomness;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Application.IFactories;
using Game.Application.Messaging.Events.BattleFlow;
using Game.Application.Messaging.Events.Spawning;
using Game.Domain.Structs;
using Game.Application.Enums;
using System;
using Game.Application.DTOs;
using Game.Application.Repositories;
using Game.Domain.Entities.Abilities;
using Game.Domain.Entities.Battle;
using System.Linq;

namespace Game.Application.Services
{
    public sealed class BattleService : IBattleService
    {
        private readonly IEventBus _bus;
        private readonly IInteractionBarrier _waitBarrier;
        private readonly IMonsterEntityFactory _monsterFactory;
        private readonly ILoggerService _log;
        private readonly IRandomService _rng;
        private readonly IBattleHistoryRepository _battleHistory;
        private readonly IPlayerDataRepository _playerTeamPersistence;
        private readonly IEnemyEncounterProvider _encounterProvider;
        private readonly IOverworldRepository _overworldPersistenceService;
        private readonly ICardEffectResolver _cardEffectResolver;
        private readonly IRewardGeneratorService _rewardGenerator;
        private readonly List<MonsterEntity> _player = new();
        private readonly List<MonsterEntity> _enemy = new();
        private readonly Dictionary<MonsterEntity, AbilityCard> _drawnCards = new();
        private RuneSlotMachineEntity _runeSlotMachine;

        public BattleService(IEventBus bus,
                             IMonsterEntityFactory monsterFactory,
                             ILoggerService log,
                             IRandomService rng,
                             IInteractionBarrier waitBarrier,
                             IBattleHistoryRepository battleHistory,
                             IPlayerDataRepository playerTeamPersistence,
                             IEnemyEncounterProvider encounterProvider,
                             IOverworldRepository overworldPersistenceService,
                             ICardEffectResolver cardEffectResolver,
                             IRewardGeneratorService rewardGenerator)
        {
            _bus = bus;
            _monsterFactory = monsterFactory;
            _log = log;
            _rng = rng;
            _waitBarrier = waitBarrier;
            _battleHistory = battleHistory;
            _playerTeamPersistence = playerTeamPersistence;
            _encounterProvider = encounterProvider;
            _overworldPersistenceService = overworldPersistenceService;
            _cardEffectResolver = cardEffectResolver;
            _rewardGenerator = rewardGenerator;
            _log.Log("BattleService initialized.");
        }
        public async Task RunBattleAsync(Guid roomId, CancellationToken ct = default)
        {
            _log?.Log($"RunBattleAsync called with roomId: {roomId}");

            if (roomId == Guid.Empty)
                throw new System.ArgumentException("Room ID cannot be empty", nameof(roomId));

            try
            {
                _log?.Log("Setting up battle teams...");
                SetupTeams(roomId);
                _log?.Log($"Teams setup complete. Player count: {_player.Count}, Enemy count: {_enemy.Count}");

                if (_player.Count == 0)
                    throw new System.InvalidOperationException("No player monsters were spawned");

                if (_enemy.Count == 0)
                    throw new System.InvalidOperationException("No enemy monsters were spawned");

                // Create rune slot machine for this battle
                _runeSlotMachine = new RuneSlotMachineEntity(_player);
                _log?.Log("Rune slot machine created with player monster runes");

                _log?.Log("Publishing BattleStartedEvent...");
                _bus.Publish(new BattleStartedEvent(_player, _enemy, _runeSlotMachine));
                _log?.Log("Battle started, waiting for monster views to be created...");

                // Create barrier token for spawn completion
                var spawnCompletionToken = BarrierToken.New();
                var totalMonsterCount = _player.Count + _enemy.Count;

                // Re-publish monster spawned events with barrier info
                foreach (var monster in _player)
                {
                    _bus.Publish(new MonsterSpawnedEvent(monster, BattleTeam.Player, spawnCompletionToken, totalMonsterCount));
                }
                foreach (var monster in _enemy)
                {
                    _bus.Publish(new MonsterSpawnedEvent(monster, BattleTeam.Enemy, spawnCompletionToken, totalMonsterCount));
                }

                await _waitBarrier.WaitAsync(new BarrierKey(spawnCompletionToken), ct);
                _log?.Log("All monster views created, starting battle loop...");

                var result = await RunLoopAsync(roomId, ct);
                _log?.Log("RunLoopAsync completed");

                // Save player team state after battle
                _playerTeamPersistence.UpdatePlayerTeam(_player);
                _log?.Log("Player team state saved to persistence");

                _log?.Log($"Battle ended: {result.Outcome}, turns={result.TurnCount}");
                
                // Generate rewards only on victory
                var rewards = result.Outcome == BattleOutcome.PlayerVictory 
                    ? _rewardGenerator.GenerateBattleRewards() 
                    : null;
                    
                _bus.Publish(new BattleEndedEvent(result, rewards));
                
                // Clean up rune slot machine
                _runeSlotMachine = null;
            }
            catch (System.OperationCanceledException)
            {
                _log?.Log("Battle was cancelled");
                throw;
            }
            catch (System.Exception ex)
            {
                _log?.LogError($"Critical error during battle: {ex.Message}");
                _log?.LogError($"Stack trace: {ex.StackTrace}");
                throw new System.InvalidOperationException($"Battle failed for room {roomId}: {ex.Message}", ex);
            }
        }

        private void SetupTeams(Guid roomId)
        {
            // Clear previous teams
            _player.Clear();
            _enemy.Clear();

            // Load player team from persistence service
            var playerMonsters = _playerTeamPersistence.GetPlayerTeam();
            _player.AddRange(playerMonsters);
            _log?.Log($"Loaded {playerMonsters.Count} player monsters from persistence");

            // Generate enemy team based on room biome
            var newEnemies = GenerateEnemyTeam(roomId);
            newEnemies.ForEach(ne => _enemy.Add(ne));
        }

        private List<MonsterEntity> GenerateEnemyTeam(Guid roomId)
        {
            try
            {
                var room = _overworldPersistenceService.GetRoomById(roomId);
                if (room == null)
                {
                    _log?.LogError($"Room with ID {roomId} not found, using random encounter");
                    var fallbackEnemyTypes = _encounterProvider.GetRandomEnemyTeam();
                    return CreateEnemiesFromTypes(fallbackEnemyTypes);

                }

                _log?.Log($"Generating enemies for biome: {room.Biome}");
                var enemyTypes = _encounterProvider.GetEnemyTeamForBiome(room.Biome);

                if (enemyTypes == null || enemyTypes.Length == 0)
                {
                    _log?.LogWarning($"No enemies found for biome {room.Biome}, using fallback");
                    enemyTypes = new MonsterType[] { MonsterType.Knight }; // Fallback
                }
                return CreateEnemiesFromTypes(enemyTypes);
            }
            catch (System.Exception ex)
            {
                _log?.LogError($"Error generating enemy team: {ex.Message}");
                // Fallback to single enemy
                var fallbackEnemy = new List<MonsterEntity>() { _monsterFactory.Create(MonsterType.Knight, BattleTeam.Enemy) };
                _log?.Log("Created fallback Knight enemy due to error");
                return fallbackEnemy;
            }
        }

        private List<MonsterEntity> CreateEnemiesFromTypes(MonsterType[] enemyTypes)
        {
            var returnList = new List<MonsterEntity>();
            foreach (var enemyType in enemyTypes)
            {
                var enemy = _monsterFactory.Create(enemyType, BattleTeam.Enemy);
                returnList.Add(enemy);
                _log?.Log($"Created enemy: {enemy.MonsterName} ({enemyType})");
            }
            return returnList;
        }


        private async Task<BattleResult> RunLoopAsync(Guid roomId, CancellationToken ct)
        {
            int turns = 0;
            _log?.Log($"Starting battle loop. Player alive: {HasAlive(_player)}, Enemy alive: {HasAlive(_enemy)}");

            while (!ct.IsCancellationRequested && HasAlive(_player) && HasAlive(_enemy))
            {
                turns++;
                _log?.Log($"=== TURN {turns} ===");
                
                // Draw cards for all monsters at the start of the round
                await DrawAllCardsForRound(ct);
                
                await RunTeamTurnAsync(_player, _enemy, BattleTeam.Player, ct);
                if (!HasAlive(_enemy)) break;

                await RunTeamTurnAsync(_enemy, _player, BattleTeam.Enemy, ct);
                
                // Clear drawn cards at the end of the round
                _drawnCards.Clear();
            }

            var outcome = ComputeOutcome(_player, _enemy);
            var survivors = outcome switch
            {
                BattleOutcome.PlayerVictory => GetAlive(_player),
                BattleOutcome.EnemyVictory => GetAlive(_enemy),
                _ => GetAlive(_player, _enemy) // draw => all remaining
            };

            return new BattleResult(roomId, outcome, turns, survivors, _battleHistory.GetBattleCount() + 1);
        }

        private async Task RunTeamTurnAsync(List<MonsterEntity> attackers, List<MonsterEntity> defenders, BattleTeam team, CancellationToken ct)
        {
            _bus.Publish(new TurnStartedEvent(team));

            // You can insert pre‑turn effects here (weather, statuses, etc.)
            var aliveAttackers = GetAlive(attackers);

            for (int i = 0; i < aliveAttackers.Count; i++)
            {
                if (ct.IsCancellationRequested) break;

                var attacker = aliveAttackers[i];
                if (attacker.IsDead) continue;

                var target = ChooseRandomTarget(defenders);
                if (target == null)
                {
                    _log.Log($"{attacker.MonsterName} has no valid targets.");
                    continue;
                }

                await ResolveCardAction(attacker, target, attackers, defenders, team, ct);

                await Task.Yield();
            }

            _bus.Publish(new TurnEndedEvent(team));
        }

        private async Task DrawAllCardsForRound(CancellationToken ct)
        {
            var allMonsters = GetAlive(_player).Concat(GetAlive(_enemy)).ToList();
            var monstersWithCards = allMonsters.Where(m => m.AbilityDeck != null && m.AbilityDeck.CanDrawCard()).ToList();
            
            if (monstersWithCards.Count == 0) return;
            
            // Create barrier tokens for both animations
            var cardDrawCompletionToken = BarrierToken.New();
            var slotMachineCompletionToken = BarrierToken.New();
            
            // Generate random slot machine values based on available faces per tumbler
            var slotMachineValues = new int[3];
            var tumblerFaces = _runeSlotMachine.GetAllTumblerFaces();
            for (int i = 0; i < 3; i++)
            {
                var availableFaceCount = tumblerFaces[i].Count;
                slotMachineValues[i] = _rng.Range(0, Math.Max(1, availableFaceCount)); // 0-based index
            }
            
            // Update slot machine state with the spin
            _runeSlotMachine.Spin(slotMachineValues);
            
            // Draw cards and build list for the event
            var drawnCards = new List<CardsDrawnEvent.DrawnCard>();
            foreach (var monster in monstersWithCards)
            {
                var card = monster.AbilityDeck.DrawRandomCard();
                _drawnCards[monster] = card;
                
                // Determine team for event
                var team = _player.Contains(monster) ? BattleTeam.Player : BattleTeam.Enemy;
                drawnCards.Add(new CardsDrawnEvent.DrawnCard(monster, card, team));
            }
            
            // Publish both events simultaneously
            _bus.Publish(new CardsDrawnEvent(drawnCards, cardDrawCompletionToken));
            _bus.Publish(new SlotMachineSpinEvent(slotMachineValues, slotMachineCompletionToken));
            
            // Wait for both animations to complete
            await _waitBarrier.WaitAsync(new BarrierKey(cardDrawCompletionToken), ct);
            await _waitBarrier.WaitAsync(new BarrierKey(slotMachineCompletionToken), ct);
        }

        private async Task ResolveCardAction(MonsterEntity attacker, MonsterEntity target, 
            List<MonsterEntity> allAllies, List<MonsterEntity> allEnemies, BattleTeam team, CancellationToken ct)
        {
            // Check if attacker has a pre-drawn card
            if (!_drawnCards.TryGetValue(attacker, out var card))
            {
                _log?.LogWarning($"{attacker.MonsterName} has no pre-drawn card - falling back to basic attack");
                await ResolveBasicAttack(attacker, target, team, ct);
                return;
            }

            // Create animation token and publish card played event
            var cardAnimationToken = BarrierToken.New();
            _bus.Publish(new CardPlayedEvent(team, attacker, card, target, cardAnimationToken));

            // Wait for animation hit point
            await _waitBarrier.WaitAsync(new BarrierKey(cardAnimationToken, (int)AttackPhase.Hit), ct);

            // Resolve card effects
            await _cardEffectResolver.ResolveCardEffectsAsync(card, attacker, target, allEnemies, allAllies, _waitBarrier, ct);

            // Move the card to discard pile
            attacker.AbilityDeck.PlayCard(card);

            // Wait for animation completion
            await _waitBarrier.WaitAsync(new BarrierKey(cardAnimationToken, (int)AttackPhase.End), ct);
        }

        private async Task ResolveBasicAttack(MonsterEntity attacker, MonsterEntity target, BattleTeam team, CancellationToken ct)
        {
            var actionAnimationToken = BarrierToken.New();
            _bus.Publish(new ActionSelectedEvent(team, attacker, target, actionAnimationToken));
            await _waitBarrier.WaitAsync(new BarrierKey(actionAnimationToken, (int)AttackPhase.Hit), ct);

            // Domain call (no awaits)
            var beforeHP = target.CurrentHP;
            var amountBlocked = attacker.Attack(target); // applies damage inside domain

            var afterHP = target.CurrentHP;
            var damage = beforeHP - afterHP;
            _bus.Publish(new DamageAppliedEvent(attacker, target, damage, amountBlocked));

            if (target.IsDead)
            {
                _bus.Publish(new MonsterFaintedEvent(target));
            }

            await _waitBarrier.WaitAsync(new BarrierKey(actionAnimationToken, (int)AttackPhase.End), ct);
        }

        private BattleOutcome ComputeOutcome(List<MonsterEntity> player, List<MonsterEntity> enemy)
        {
            bool p = HasAlive(player);
            bool e = HasAlive(enemy);
            if (p && e) return BattleOutcome.Draw;
            if (p && !e) return BattleOutcome.PlayerVictory;
            if (!p && e) return BattleOutcome.EnemyVictory;
            return BattleOutcome.Draw;
        }

        private static bool HasAlive(List<MonsterEntity> team)
        {
            for (int i = 0; i < team.Count; i++) if (!team[i].IsDead) return true;
            return false;
        }

        private static void CollectAlive(List<MonsterEntity> source, List<MonsterEntity> dst)
        {
            for (int i = 0; i < source.Count; i++) if (!source[i].IsDead) dst.Add(source[i]);
        }

        private List<MonsterEntity> GetAlive(List<MonsterEntity> team)
        {
            var list = new List<MonsterEntity>(team.Count);
            CollectAlive(team, list);
            return list;
        }

        private List<MonsterEntity> GetAlive(List<MonsterEntity> a, List<MonsterEntity> b)
        {
            var list = new List<MonsterEntity>(a.Count + b.Count);
            CollectAlive(a, list); CollectAlive(b, list);
            return list;
        }

        private MonsterEntity ChooseRandomTarget(List<MonsterEntity> candidates)
        {
            var aliveCandidates = GetAlive(candidates);
            if (aliveCandidates.Count == 0) return null;
            int idx = _rng.Range(0, aliveCandidates.Count);
            return aliveCandidates[idx];
        }
    }
}