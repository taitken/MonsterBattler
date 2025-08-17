
using System;
using System.Collections.Generic;
using Game.Domain.Structs;

namespace Game.Application.Repositories
{
    public class BattleHistoryRepository : IBattleHistoryRepository
    {
        private Dictionary<Guid, BattleResult> _battleHistory = new();
        public int GetBattleCount()
        {
            return _battleHistory.Count;
        }
        public BattleResult GetBattleHistory(Guid roomId)
        {
            if (_battleHistory.TryGetValue(roomId, out var result))
            {
                return result;
            }
            return default;
        }
        public BattleResult GetLatestBattleHistory()
        {
            if (_battleHistory.Count == 0)
            {
                return default;
            }
            var latestBattle = default(BattleResult);
            foreach (var battle in _battleHistory.Values)
            {
                if (latestBattle.Equals(default(BattleResult)) || battle.BattleCounter > latestBattle.BattleCounter)
                {
                    latestBattle = battle;
                }
            }
            return latestBattle;
        }
        public void SaveBattleHistory(Guid roomId, BattleResult result)
        {
            _battleHistory.Add(roomId, result);
        }
    }
}
