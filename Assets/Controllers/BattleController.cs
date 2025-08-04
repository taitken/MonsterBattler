using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class BattleController : MonoBehaviour
{
    [SerializeField] private MonsterDefinition playerDef;
    [SerializeField] private MonsterDefinition enemyDef;
    [SerializeField] private MonsterFactory monsterFactory;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform enemySpawn;

    private MonsterModel playerModel;
    private MonsterModel enemyModel;
    private EventQueue eventQueue = new();
    private List<Monster> activeMonsters = new();

    private async void Start()
    {
        SetupMonsters();
        await RunBattleLoop();
    }

    private void SetupMonsters()
    {
        playerModel = new MonsterModel(playerDef);
        enemyModel = new MonsterModel(enemyDef);

        activeMonsters.Add(monsterFactory.Spawn(playerModel, playerSpawn));
        activeMonsters.Add(monsterFactory.Spawn(enemyModel, enemySpawn));
    }

    private async Task RunBattleLoop()
    {
        while (!playerModel.IsDead && !enemyModel.IsDead)
        {
            EnqueueAttack(playerModel, enemyModel);
            if (!enemyModel.IsDead) EnqueueAttack(enemyModel, playerModel);

            await eventQueue.ProcessAll();
        }

        Debug.Log("Battle over!");
    }

    private void EnqueueAttack(MonsterModel attacker, MonsterModel target)
    {
        eventQueue.Enqueue(async () =>
        {
            Debug.Log($"{attacker.definition.monsterName} attacks {target.definition.monsterName}!");
            await Task.Delay(1000); // simulate animation
            target.TakeDamage(attacker.definition.attackDamage);
        });
    }
}