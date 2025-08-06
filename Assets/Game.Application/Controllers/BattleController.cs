using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Game.Application.IFactories;
using Game.Domain.Entities;
using Game.Core;
using Game.Domain.Structs;
using Game.Core.Events;
using Game.Application.Events.Battle;
using Game.Core.Logger;
using Game.Core.Randomness;

public class BattleController
{
    private IEventQueueService _eventQueueService;
    private IMonsterEntityFactory _monsterFactory;
    private ILoggerService _loggerService;
    private List<MonsterEntity> _playerMonsters = new();
    private List<MonsterEntity> _enemyMonsters = new();

    public BattleController()
    {
        _eventQueueService = ServiceLocator.Get<IEventQueueService>();
        _monsterFactory = ServiceLocator.Get<IMonsterEntityFactory>();
        _loggerService = ServiceLocator.Get<ILoggerService>();
    }

    public async void StartBattle()
    {
        SetupMonsters();
        BattleResult result = await RunBattleLoop();
        HandleBattleResult(result);
    }

    private void SetupMonsters()
    {
        CreateMonster(MonsterType.Goald, BattleTeam.Player);
        CreateMonster(MonsterType.Kraggan, BattleTeam.Player);
        CreateMonster(MonsterType.Flimboon, BattleTeam.Player);

        CreateMonster(MonsterType.Knight, BattleTeam.Enemy);
    }

    private void CreateMonster(MonsterType type, BattleTeam team)
    {
        var monster = _monsterFactory.Create(type);
        GetMonstersByTeam(team).Add(monster);
        _eventQueueService.Publish(new MonsterSpawnedEvent(monster, team));
    }

    private List<MonsterEntity> GetMonstersByTeam(BattleTeam team)
    {
        return team == BattleTeam.Player ? _playerMonsters : _enemyMonsters;
    }

    private async Task<BattleResult> RunBattleLoop()
    {
        _loggerService.Log("Start battle loop");
        int turnCount = 0;
        // Main battle loop
        await Task.Delay(500); // Initial delay before battle starts
        while (HasAlive(_playerMonsters) && HasAlive(_enemyMonsters))
        {
            turnCount++;
            await RunTurn(_playerMonsters, _enemyMonsters, "Player");
            if (!HasAlive(_enemyMonsters))
            {
                _loggerService.Log("All enemy monsters defeated!");
                break;
            }

            await RunTurn(_enemyMonsters, _playerMonsters, "Enemy");
        }

        var surviving = HasAlive(_playerMonsters)
            ? GetAliveMonsters(_playerMonsters)
            : GetAliveMonsters(_enemyMonsters);

        var outcome = !HasAlive(_playerMonsters) && !HasAlive(_enemyMonsters)
            ? BattleOutcome.Draw
            : HasAlive(_playerMonsters)
                ? BattleOutcome.PlayerVictory
                : BattleOutcome.EnemyVictory;

        _loggerService.Log($"Battle over! Outcome: {outcome}");

        return new BattleResult(outcome, turnCount, surviving);
    }

    private bool HasAlive(List<MonsterEntity> team) => team.Any(m => !m.IsDead);

    private List<MonsterEntity> GetAliveMonsters(List<MonsterEntity> monsters)
    {
        return monsters.Where(m => !m.IsDead).ToList();
    }

    private async Task RunTurn(List<MonsterEntity> attackers, List<MonsterEntity> targets, string teamName)
    {
        _loggerService.Log($"{teamName} turn");
        foreach (var attacker in GetAliveMonsters(attackers))
        {
            var target = ChooseRandomTarget(targets);
            if (target == null)
            {
                _loggerService.Log($"{attacker.MonsterName} has no targets left!");
                continue;
            }
            await Attack(attacker, target);
            _loggerService.Log($"Player: {attacker.MonsterName} vs Enemy: {target.MonsterName}");
        }
    }

    private async Task Attack(MonsterEntity attacker, MonsterEntity target)
    {
        attacker.Attack(target);
        await Task.Delay(500);
    }

    private MonsterEntity ChooseRandomTarget(List<MonsterEntity> potentialTargets)
    {
        var _randomService = ServiceLocator.Get<IRandomService>();
        var aliveTargets = GetAliveMonsters(potentialTargets);
        if (aliveTargets.Count == 0) return null;

        int index = _randomService.Range(0, aliveTargets.Count); // Use Unity's Random for determinism in editor
        return aliveTargets[index];
    }

    private void HandleBattleResult(BattleResult result)
    {
        _loggerService.Log($"Winner: {result.Outcome}, Turns: {result.TurnCount}");
    }
}