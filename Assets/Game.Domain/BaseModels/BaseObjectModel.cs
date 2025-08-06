using UnityEngine;

namespace System
{
    public abstract class BaseObjectModel : BaseModel
    {
        public event Action OnModelUpdated;
    }
}