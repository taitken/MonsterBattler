using System;

namespace Game.Domain
{
    public abstract class BaseEntity : BaseModel
    {
        public event Action OnModelUpdated;
    }
}