using UnityEngine;

[CreateAssetMenu(menuName = "Monsters/Monster Definition")]
public class MonsterDefinition : ScriptableObject
{
    public string monsterName;
    public int maxHealth;
    public int attackDamage;
    public MonsterType type;
    
}