using System.Collections.Generic;
using Game.Domain.Structs;

namespace Game.Application.Interfaces
{
    public interface IRewardGeneratorService
    {
        IEnumerable<Reward> GenerateBattleRewards();
        CardReward GenerateCardReward(string rewardId);
    }
}