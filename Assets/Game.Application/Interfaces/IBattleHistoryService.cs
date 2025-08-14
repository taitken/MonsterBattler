
using System;
using Game.Domain.Structs;

namespace Game.Application.Interfaces
{
    public interface IBattleHistoryService
    {
        int GetBattleCount();
        BattleResult GetBattleHistory(Guid roomId);
        BattleResult GetLatestBattleHistory();
        void SaveBattleHistory(Guid roomId, BattleResult result);
    }
}
