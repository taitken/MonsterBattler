namespace Game.Application.DTOs.Effects
{
    public readonly struct DamageModificationResult
    {
        public readonly int FinalDamage;
        public readonly int DamageBlocked;
        public readonly int DamageReduced;
        public readonly bool WasModified;

        public DamageModificationResult(int finalDamage, int damageBlocked = 0, int damageReduced = 0)
        {
            FinalDamage = finalDamage;
            DamageBlocked = damageBlocked;
            DamageReduced = damageReduced;
            WasModified = damageBlocked > 0 || damageReduced > 0;
        }

        public static DamageModificationResult NoModification(int originalDamage) =>
            new DamageModificationResult(originalDamage);

        public static DamageModificationResult Blocked(int originalDamage, int blocked) =>
            new DamageModificationResult(originalDamage - blocked, blocked);

        public static DamageModificationResult Reduced(int originalDamage, int reduced) =>
            new DamageModificationResult(originalDamage - reduced, 0, reduced);
    }
}