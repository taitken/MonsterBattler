using System.Collections.Generic;
using System.Linq;
using Game.Application.Interfaces;
using Game.Application.IFactories;
using Game.Application.DTOs.Rewards;
using Game.Core.Randomness;
using Game.Domain.Entities.Abilities;
using Game.Domain.Enums;

namespace Game.Application.Services
{
    public class RewardGeneratorService : IRewardGeneratorService
    {
        private readonly IRandomService _randomService;
        private readonly IAbilityCardFactory _cardFactory;

        public RewardGeneratorService(IRandomService randomService, IAbilityCardFactory cardFactory)
        {
            _randomService = randomService;
            _cardFactory = cardFactory;
        }

        public IEnumerable<Reward> GenerateBattleRewards()
        {
            var goldAmount = _randomService.Range(10, 31);
            var cardReward = GenerateCardReward();

            return new List<Reward>()
            {
                new GoldReward(goldAmount),
                cardReward
            };
        }


        private CardReward GenerateCardReward()
        {
            // Generate new card choices
            var allCards = _cardFactory.CreateAllCards();
            var cardChoices = allCards
                .OrderBy(_ => _randomService.Range(0, int.MaxValue))
                .Take(3)
                .ToList();

            var cardReward = new CardReward(
                amount: 1,
                displayText: "1 Card",
                cardChoices: cardChoices.AsReadOnly()
            );

            return cardReward;
        }
    }
}