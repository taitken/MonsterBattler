using System.Collections.Generic;

public struct BattleResult
{
    public BattleOutcome Outcome;
    public int TurnCount;
    public List<MonsterEntity> SurvivingMonsters;

    public BattleResult(BattleOutcome outcome, int turnCount, List<MonsterEntity> survivingMonsters)
    {
        Outcome = outcome;
        TurnCount = turnCount;
        SurvivingMonsters = survivingMonsters;
    }
}