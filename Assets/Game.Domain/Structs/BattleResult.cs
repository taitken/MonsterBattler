using System;
using System.Collections.Generic;
using Game.Domain.Entities;
using Game.Domain.Enums;

namespace Game.Domain.Structs
{
    public readonly struct BattleResult
    {
        public readonly Guid RoomId;
        public readonly BattleOutcome Outcome;
        public readonly int TurnCount;
        public readonly List<MonsterEntity> SurvivingMonsters;
        public readonly int BattleCounter;

        public BattleResult(Guid roomId, BattleOutcome outcome, int turnCount, List<MonsterEntity> survivingMonsters, int battleCounter)
        {
            RoomId = roomId;
            Outcome = outcome;
            TurnCount = turnCount;
            SurvivingMonsters = survivingMonsters;
            BattleCounter = battleCounter;
        }
    }
}