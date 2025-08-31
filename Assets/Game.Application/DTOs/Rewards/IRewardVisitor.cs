namespace Game.Application.DTOs.Rewards
{
    public interface IRewardVisitor
    {
        void Visit(GoldReward goldReward);
        void Visit(CardReward cardReward);
        void Visit(ExperienceReward experienceReward);
    }
}