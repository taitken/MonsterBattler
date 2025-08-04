using System;

namespace UnityEngine
{
    public abstract class MonoObject<ModelType> : MonoBehaviour where ModelType : BaseObjectModel
    {
        protected ModelType model;

        public virtual void Bind(ModelType model)
        {
            this.model = model;
            OnModelBound();
        }

        protected abstract void OnModelBound();

        protected virtual void BeforeDeath()
        {

        }

        public void Destroy()
        {
            this.BeforeDeath();
            Destroy(gameObject);
        }

        protected static IServiceType Inject<IServiceType>()
        {
            return ServiceContainer.Instance.Resolve<IServiceType>();
        }
    }
}