using System.Collections.Generic;
using Game.Application.DTOs.Rewards;

namespace Game.Application.Interfaces
{
    public interface IRewardGeneratorService
    {
        IEnumerable<Reward> GenerateBattleRewards();
    }
}