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
using Game.Applcation.Models;
using UnityEngine;
using Game.Applcation.Enums;

namespace Game.Application.Services
{
    public sealed class BattleService : IBattleService
    {
        private readonly IEventBus _bus;
        private readonly IInteractionBarrier _waitBarrier;
        private readonly IMonsterEntityFactory _monsterFactory;
        private readonly ILoggerService _log;
        private readonly IRandomService _rng;
        private readonly List<MonsterEntity> _player = new();
        private readonly List<MonsterEntity> _enemy = new();

        public BattleService(IEventBus bus,
                             IMonsterEntityFactory monsterFactory,
                             ILoggerService log,
                             IRandomService rng,
                             IInteractionBarrier waitBarrier)
        {
            _bus = bus;
            _monsterFactory = monsterFactory;
            _log = log;
            _rng = rng;
            _waitBarrier = waitBarrier;
            _log.Log("BattleService initialized.");
        }
        public async Task RunBattleAsync(CancellationToken ct = default)
        {
            SetupTeams();

            _bus.Publish(new BattleStartedEvent(_player, _enemy));
            _log.Log("Battle started.");

            var result = await RunLoopAsync(ct);

            _log.Log($"Battle ended: {result.Outcome}, turns={result.TurnCount}");
            _bus.Publish(new BattleEndedEvent(result.Outcome, result.SurvivingMonsters, result.TurnCount));
        }

        private void SetupTeams()
        {
            Spawn(MonsterType.Goald, BattleTeam.Player);
            Spawn(MonsterType.Kraggan, BattleTeam.Player);
            Spawn(MonsterType.Flimboon, BattleTeam.Player);
            Spawn(MonsterType.Knight, BattleTeam.Enemy);
        }

        private void Spawn(MonsterType type, BattleTeam team)
        {
            var m = _monsterFactory.Create(type);
            (team == BattleTeam.Player ? _player : _enemy).Add(m);
            _bus.Publish(new MonsterSpawnedEvent(m, team));
        }

        private async Task<BattleResult> RunLoopAsync(CancellationToken ct)
        {
            int turns = 0;

            while (!ct.IsCancellationRequested && HasAlive(_player) && HasAlive(_enemy))
            {
                turns++;

                // PHASE: Player turn
                await RunTeamTurnAsync(_player, _enemy, BattleTeam.Player, ct);
                if (!HasAlive(_enemy)) break;

                // PHASE: Enemy turn
                await RunTeamTurnAsync(_enemy, _player, BattleTeam.Enemy, ct);
            }

            var outcome = ComputeOutcome(_player, _enemy);
            var survivors = outcome switch
            {
                BattleOutcome.PlayerVictory => GetAlive(_player),
                BattleOutcome.EnemyVictory => GetAlive(_enemy),
                _ => GetAlive(_player, _enemy) // draw => all remaining
            };

            return new BattleResult(outcome, turns, survivors);
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
                var actionAnimationToken = BarrierToken.New();
                _bus.Publish(new ActionSelectedEvent(team, attacker, target, actionAnimationToken));
                await _waitBarrier.WaitAsync(new BarrierKey(actionAnimationToken, (int)AttackPhase.Hit), ct);

                ResolveBasicAttack(attacker, target);
                await _waitBarrier.WaitAsync(new BarrierKey(actionAnimationToken, (int)AttackPhase.End), ct);

                await Task.Yield(); 
            }

            _bus.Publish(new TurnEndedEvent(team));
        }

        private void ResolveBasicAttack(MonsterEntity attacker, MonsterEntity target)
        {
            // Domain call (no awaits)
            var beforeHP = target.CurrentHP;
            attacker.Attack(target); // applies damage inside domain

            var afterHP = target.CurrentHP;
            var damage = beforeHP - afterHP;
            _bus.Publish(new DamageAppliedEvent(attacker, target, damage));

            if (target.IsDead)
            {
                _bus.Publish(new MonsterFaintedEvent(target));
            }
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