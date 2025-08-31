namespace Game.Domain.Entities.Rewards
{
    public interface IRewardVisitor
    {
        void Visit(GoldReward goldReward);
        void Visit(CardReward cardReward);
        void Visit(ExperienceReward experienceReward);
    }
}