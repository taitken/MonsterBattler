namespace Game.Presentation.Enums
{
    public enum CardAnimationType
    {
        None,           // No special animation - just float
        Attack,         // Wind up, strike, return animation for damage cards
        Heal,           // Future: gentle glow animation for heal cards
        Defend,         // Future: shield animation for defend cards
        Buff,           // Future: sparkle animation for buff cards
        Debuff          // Future: dark energy animation for debuff cards
    }
}