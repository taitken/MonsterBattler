using UnityEngine;

public interface IMonsterFactory
{
    MonsterView Spawn(MonsterType type, Vector3 spawnPoint);
    MonsterView Spawn(MonsterEntity model, Vector3 spawnPoint);
    MonsterView Spawn(MonsterType type, Transform spawnPoint);
    MonsterView Spawn(MonsterEntity model, Transform spawnPoint);
}