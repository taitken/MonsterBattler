using UnityEngine;

public class MonsterFactory : MonoBehaviour
{
    [SerializeField] public GameObject monsterPrefab;

    public MonsterView Spawn(MonsterType type, Vector3 spawnPoint)
    {
        var definition = Resources.Load<MonsterDefinition>($"Monsters/Definitions/{type}");
        return Spawn(new MonsterModel(definition), spawnPoint);
    }

    public MonsterView Spawn(MonsterModel model, Vector3 spawnPoint)
    {
        var obj = Instantiate(monsterPrefab, spawnPoint, Quaternion.identity);
        var monster = obj.GetComponent<MonsterView>();
        monster.Bind(model);
        return monster;
    }
    public MonsterView Spawn(MonsterType type, Transform spawnPoint)
    {
        var definition = Resources.Load<MonsterDefinition>($"Monsters/Definitions/{type}");
        return Spawn(new MonsterModel(definition), spawnPoint);
    }

    public MonsterView Spawn(MonsterModel model, Transform spawnPoint)
    {
        var obj = Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
        var monster = obj.GetComponent<MonsterView>();
        monster.Bind(model);
        return monster;
    }
}