namespace Game.Application.DTOs.Effects
{
    public readonly struct CardPlayResult
    {
        public readonly bool IsAllowed;
        public readonly string PreventionReason;

        private CardPlayResult(bool isAllowed, string reason = null)
        {
            IsAllowed = isAllowed;
            PreventionReason = reason ?? string.Empty;
        }

        public static CardPlayResult Allowed() => new CardPlayResult(true);

        public static CardPlayResult Prevented(string reason) => new CardPlayResult(false, reason);
    }
}