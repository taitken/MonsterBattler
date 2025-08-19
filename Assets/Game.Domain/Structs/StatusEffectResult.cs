namespace Game.Domain.Structs
{
    public readonly struct StatusEffectResult
    {
        public bool WasSuccessful { get; }
        public string Message { get; }
        public int DamageDealt { get; }
        public int HealingDone { get; }
        public bool TargetDied { get; }

        private StatusEffectResult(bool wasSuccessful, string message, int damageDealt = 0, int healingDone = 0, bool targetDied = false)
        {
            WasSuccessful = wasSuccessful;
            Message = message ?? string.Empty;
            DamageDealt = damageDealt;
            HealingDone = healingDone;
            TargetDied = targetDied;
        }

        public static StatusEffectResult Success(string message, int damageDealt = 0, int healingDone = 0, bool targetDied = false)
        {
            return new StatusEffectResult(true, message, damageDealt, healingDone, targetDied);
        }

        public static StatusEffectResult NoEffect(string message = "No effect")
        {
            return new StatusEffectResult(true, message);
        }

        public static StatusEffectResult Failed(string message)
        {
            return new StatusEffectResult(false, message);
        }
    }
}