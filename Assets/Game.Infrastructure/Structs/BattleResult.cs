using System.Collections.Generic;

public struct BattleResult
{
    public BattleOutcome Outcome;
    public int TurnCount;
    public List<MonsterModel> SurvivingMonsters;

    public BattleResult(BattleOutcome outcome, int turnCount, List<MonsterModel> survivingMonsters)
    {
        Outcome = outcome;
        TurnCount = turnCount;
        SurvivingMonsters = survivingMonsters;
    }
}