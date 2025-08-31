using System.Collections.Generic;
using System.Linq;
using Game.Application.Interfaces;
using Game.Application.IFactories;
using Game.Core.Randomness;
using Game.Domain.Enums;
using Game.Domain.Structs;

namespace Game.Application.Services
{
    public class RewardGeneratorService : IRewardGeneratorService
    {
        private readonly IRandomService _randomService;
        private readonly IAbilityCardFactory _cardFactory;
        private readonly Dictionary<string, CardReward> _generatedCardRewards = new();

        public RewardGeneratorService(IRandomService randomService, IAbilityCardFactory cardFactory)
        {
            _randomService = randomService;
            _cardFactory = cardFactory;
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

        public CardReward GenerateCardReward(string rewardId)
        {
            // Return existing reward if already generated (for persistence)
            if (_generatedCardRewards.TryGetValue(rewardId, out var existingReward))
            {
                return existingReward;
            }

            // Generate new card choices
            var allCards = _cardFactory.CreateAllCards();
            var cardChoices = allCards
                .OrderBy(_ => _randomService.Range(0, int.MaxValue))
                .Take(3)
                .ToList();

            var cardReward = new CardReward(
                amount: 1,
                displayText: "1 Card",
                cardChoices: cardChoices.AsReadOnly(),
                rewardId: rewardId
            );

            // Store for persistence
            _generatedCardRewards[rewardId] = cardReward;

            return cardReward;
        }
    }
}