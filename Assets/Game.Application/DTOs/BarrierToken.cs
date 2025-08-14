
using System;

namespace Game.Applcation.DTOs
{
    public readonly struct BarrierToken : IEquatable<BarrierToken>
    {
        public readonly Guid Id;
        private BarrierToken(Guid id)
        {
            Id = id;
        }
        public static BarrierToken New()
        {
            return new BarrierToken(Guid.NewGuid());
        }
        public bool Equals(BarrierToken other)
        {
            return Id.Equals(other.Id);
        }
        public override bool Equals(object o)
        {
            return o is BarrierToken t && Equals(t);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public override string ToString()
        {
            return Id.ToString("N");
        } 
    }
}
