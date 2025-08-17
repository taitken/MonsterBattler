
using System;
using Game.Domain.Structs;

namespace Game.Application.Repositories
{
    public interface IBattleHistoryRepository
    {
        int GetBattleCount();
        BattleResult GetBattleHistory(Guid roomId);
        BattleResult GetLatestBattleHistory();
        void SaveBattleHistory(Guid roomId, BattleResult result);
    }
}
