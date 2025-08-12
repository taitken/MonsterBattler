
using Game.Core;
using Game.Domain;
using UnityEngine;

namespace Game.Presentation
{
    public abstract class MonoObject<ModelType> : MonoBehaviour, IMonoObject where ModelType : BaseEntity
    {
        public ModelType model;

        public virtual void Bind(ModelType model)
        {
            this.model = model;
            OnModelBound();
        }

        public BaseEntity GetModel() => model;

        public MonoBehaviour AsMonoBehaviour() => this;

        protected abstract void OnModelBound();

        protected virtual void BeforeDeath() { }

        public void Destroy()
        {
            this.BeforeDeath();
            Destroy(gameObject);
        }
    }
}
