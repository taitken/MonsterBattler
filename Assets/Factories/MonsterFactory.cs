using UnityEngine;

public class MonsterFactory : MonoBehaviour
{
    [SerializeField] public GameObject monsterPrefab;

    public Monster Spawn(MonsterModel model, Transform spawnPoint)
    {
        var obj = Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
        var monster = obj.GetComponent<Monster>();
        monster.Bind(model);
        return monster;
    }
}