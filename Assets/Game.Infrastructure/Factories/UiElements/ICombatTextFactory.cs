using UnityEngine;

public interface ICombatTextFactory
{
    CombatTextUi Spawn(Color color, string text, Vector3 worldPosition);
}