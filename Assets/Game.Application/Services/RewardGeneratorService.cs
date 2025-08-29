using System.Collections.Generic;
using Game.Application.Interfaces;
using Game.Core.Randomness;
using Game.Domain.Enums;
using Game.Domain.Structs;

namespace Game.Application.Services
{
    public class RewardGeneratorService : IRewardGeneratorService
    {
        private readonly IRandomService _randomService;

        public RewardGeneratorService(IRandomService randomService)
        {
            _randomService = randomService;
        }

        public IEnumerable<Reward> GenerateBattleRewards()
        {
            var goldAmount = _randomService.Range(10, 31);
            const int cardAmount = 1;

            return new[]
            {
                new Reward(ResourceType.Gold, goldAmount, $"{goldAmount} Gold"),
                new Reward(ResourceType.Card, cardAmount, $"{cardAmount} Card")
            };
        }
    }
}