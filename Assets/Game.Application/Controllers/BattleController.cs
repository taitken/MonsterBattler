using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class BattleController : MonoBehaviour
{
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform enemySpawn;
    private EventQueue eventQueue = new();
    private IMonsterFactory monsterFactory;
    private List<MonsterEntity> playerMonsters = new();
    private List<MonsterEntity> enemyMonsters = new();

    private void Awake()
    {
        monsterFactory = ServiceContainer.Instance.Resolve<IMonsterFactory>();
    }

    private async void Start()
    {
        SetupMonsters();
        BattleResult result = await RunBattleLoop();
        HandleBattleResult(result);
    }

    private void SetupMonsters()
    {
        playerMonsters.Add(monsterFactory.Spawn(MonsterType.Goald, playerSpawn).model);
        playerMonsters.Add(monsterFactory.Spawn(MonsterType.Daybloom, playerSpawn.position + new Vector3(-2.5f, -1f, 0f)).model);
        playerMonsters.Add(monsterFactory.Spawn(MonsterType.Flimboon, playerSpawn.position + new Vector3(2.0f, -1f, 0f)).model);

        enemyMonsters.Add(monsterFactory.Spawn(MonsterType.Knight, enemySpawn).model);
        enemyMonsters.Add(monsterFactory.Spawn(MonsterType.Mage, enemySpawn.position + new Vector3(2.5f, -1f, 0f)).model);
        enemyMonsters.Add(monsterFactory.Spawn(MonsterType.Ranger, enemySpawn.position + new Vector3(-2.0f, -1f, 0f)).model);
    }

    private async Task<BattleResult> RunBattleLoop()
    {
        Debug.Log("Start battle loop");
        int turnCount = 0;
        // Main battle loop
        await Task.Delay(1000); // Initial delay before battle starts
        while (HasAlive(playerMonsters) && HasAlive(enemyMonsters))
        {
            turnCount++;
            await RunTurn(playerMonsters, enemyMonsters, "Player");
            if (!HasAlive(enemyMonsters))
            {
                Debug.Log("All enemy monsters defeated!");
                break;
            }

            await RunTurn(enemyMonsters, playerMonsters, "Enemy");
        }

        var surviving = HasAlive(playerMonsters)
            ? GetAliveMonsters(playerMonsters)
            : GetAliveMonsters(enemyMonsters);

        var outcome = !HasAlive(playerMonsters) && !HasAlive(enemyMonsters)
            ? BattleOutcome.Draw
            : HasAlive(playerMonsters)
                ? BattleOutcome.PlayerVictory
                : BattleOutcome.EnemyVictory;

        Debug.Log($"Battle over! Outcome: {outcome}");

        return new BattleResult(outcome, turnCount, surviving);
    }

    private bool HasAlive(List<MonsterEntity> team) => team.Any(m => !m.IsDead);

    private List<MonsterEntity> GetAliveMonsters(List<MonsterEntity> monsters)
    {
        return monsters.Where(m => !m.IsDead).ToList();
    }

    private async Task RunTurn(List<MonsterEntity> attackers, List<MonsterEntity> targets, string teamName)
    {
        Debug.Log($"{teamName} turn");
        foreach (var attacker in GetAliveMonsters(attackers))
        {
            var target = ChooseRandomTarget(targets);
            if (target == null)
            {
                Debug.Log($"{attacker.definition.monsterName} has no targets left!");
                continue;
            }
            EnqueueAttack(attacker, target);
            Debug.Log($"Player: {attacker.definition.monsterName} vs Enemy: {target.definition.monsterName}");
        }

        await eventQueue.ProcessAll();
    }

    private void EnqueueAttack(MonsterEntity attacker, MonsterEntity target)
    {
        eventQueue.Enqueue(async () =>
        {
            var view = attacker.GetView<MonsterView>();
            if(view != null)
            {
                await view.PlayAttackAnimation();
            }   
            Debug.Log($"{attacker.definition.monsterName} attacks {target.definition.monsterName}!");
            target.TakeDamage(attacker.definition.attackDamage);
            await Task.Delay(1000); 
        });
    }

    private MonsterEntity ChooseRandomTarget(List<MonsterEntity> potentialTargets)
    {
        var aliveTargets = GetAliveMonsters(potentialTargets);
        if (aliveTargets.Count == 0) return null;

        int index = Random.Range(0, aliveTargets.Count); // Use Unity's Random for determinism in editor
        return aliveTargets[index];
    }

    private void HandleBattleResult(BattleResult result)
    {
        Debug.Log($"Winner: {result.Outcome}, Turns: {result.TurnCount}");
    }
}