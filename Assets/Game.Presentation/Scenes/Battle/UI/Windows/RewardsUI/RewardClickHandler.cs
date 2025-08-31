using Game.Application.DTOs.Rewards;
using Game.Application.Messaging;
using Game.Application.Messaging.Commands;
using Game.Application.Messaging.Events.Rewards;

namespace Game.Presentation.Scenes.Battle.UI.Windows.RewardsUI
{
    public class RewardClickHandler : IRewardVisitor
    {
        private readonly IEventBus _eventBus;
        private readonly RewardOptionUI _rewardOptionUI;

        public RewardClickHandler(IEventBus eventBus, RewardOptionUI rewardOptionUI)
        {
            _eventBus = eventBus;
            _rewardOptionUI = rewardOptionUI;
        }

        public void Visit(GoldReward goldReward)
        {
            // For gold rewards, directly claim the reward
            _eventBus.Publish(new RewardClaimedCommand(goldReward));
            _rewardOptionUI.NotifyRewardClaimed();
        }

        public void Visit(CardReward cardReward)
        {
            // For card rewards, publish event to show card selection window
            _eventBus.Publish(new CardRewardSelectedEvent(cardReward));
            _rewardOptionUI.NotifyRewardClaimed();
        }

        public void Visit(ExperienceReward experienceReward)
        {
            // For experience rewards, directly claim the reward
            _eventBus.Publish(new RewardClaimedCommand(experienceReward));
            _rewardOptionUI.NotifyRewardClaimed();
        }
    }
}