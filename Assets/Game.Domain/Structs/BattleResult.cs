using System.Collections.Generic;
using Game.Domain.Entities;
using Game.Domain.Enums;

namespace Game.Domain.Structs
{
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
}