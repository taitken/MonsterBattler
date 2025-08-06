using System;

namespace Game.Domain
{
    public abstract class BaseObjectModel : BaseModel
    {
        public event Action OnModelUpdated;
    }
}