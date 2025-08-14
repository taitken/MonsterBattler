namespace Game.Applcation.Models
{
public readonly struct BarrierKey : System.IEquatable<BarrierKey>
{
    public readonly BarrierToken Token;
    public readonly int Phase;

    public BarrierKey(BarrierToken token, int phase)
    {
        Token = token; Phase = phase;
    }

    public bool Equals(BarrierKey other) => Token.Equals(other.Token) && Phase == other.Phase;
    public override bool Equals(object obj) => obj is BarrierKey k && Equals(k);
    public override int GetHashCode() => System.HashCode.Combine(Token, Phase);
}
}
