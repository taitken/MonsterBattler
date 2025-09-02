using System;

namespace Game.Domain
{
    public abstract class BaseEntity : BaseModel
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public event Action OnModelUpdated;

        protected virtual void NotifyModelUpdated()
        {
            OnModelUpdated?.Invoke();
        }
    }
}