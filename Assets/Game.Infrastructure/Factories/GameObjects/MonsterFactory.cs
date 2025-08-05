using UnityEngine;

public class MonsterFactory : IMonsterFactory
{
    private readonly GameObject monsterPrefab;

    public MonsterFactory(GameObject _monsterPrefab)
    {
        monsterPrefab = _monsterPrefab;
    }

    public MonsterView Spawn(MonsterType type, Vector3 spawnPoint)
    {
        var definition = Resources.Load<MonsterDefinition>($"Monsters/Definitions/{type}");
        return Spawn(new MonsterEntity(definition), spawnPoint);
    }

    public MonsterView Spawn(MonsterEntity model, Vector3 spawnPoint)
    {
        var obj = Object.Instantiate(monsterPrefab, spawnPoint, Quaternion.identity);
        var monster = obj.GetComponent<MonsterView>();
        monster.Bind(model);
        return monster;
    }
    public MonsterView Spawn(MonsterType type, Transform spawnPoint)
    {
        var definition = Resources.Load<MonsterDefinition>($"Monsters/Definitions/{type}");
        return Spawn(new MonsterEntity(definition), spawnPoint);
    }

    public MonsterView Spawn(MonsterEntity model, Transform spawnPoint)
    {
        var obj = Object.Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
        var monster = obj.GetComponent<MonsterView>();
        monster.Bind(model);
        return monster;
    }
}